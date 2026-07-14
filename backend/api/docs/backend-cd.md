# バックエンド CD（backend_cd.yml）

## 目的

`Backend CI` が成功した後に自動実行し、ビルドした成果物を Azure の仮想マシン（VM）へデプロイする。
テストを通過したコードだけが本番へ反映される仕組みにすることで、壊れたコードのデプロイを防ぐ。

## 全体像

```
GitHub Actions（CD）
  │  dotnet publish で成果物を作成
  │  SSH / rsync で VM へ転送
  ▼
Azure VM
  ├─ Nginx（リバースプロキシ, :80）
  │     └─ 127.0.0.1:5000 へ転送
  └─ Kestrel（ASP.NET Core, systemd で常駐）
        └─ Azure Database for PostgreSQL / Azure Blob Storage へ接続
```

CD は「成果物をビルドして VM へ送り、systemd サービスを再起動する」ことに責任を持つ。
VM 側の常駐（systemd）とリバースプロキシ（Nginx）は、事前に一度だけ構築しておく。

## トリガー

- `Backend CI`（`backend_ci.yml`）が `main` で完了し、成功したとき（`workflow_run`）
- 手動実行（`workflow_dispatch`）

CI が失敗した場合はデプロイしない。手動実行は無条件で通す。

## 事前条件

CD を動かす前に、VM 側とリポジトリ側で以下を一度だけ準備しておく必要がある。

### 1. VM の準備

**.NET ランタイムの導入**

VM 上でアプリを実行するため、ASP.NET Core ランタイムを入れる。SDK は不要（ビルドは GitHub Actions 側で行う）。

```bash
sudo apt install -y aspnetcore-runtime-10.0
dotnet --list-runtimes   # Microsoft.AspNetCore.App 10.0.x が出れば OK
```

**アプリ用ユーザーとディレクトリ**

権限を最小化するため、ログイン不可の専用ユーザーで実行する。

```bash
sudo useradd --system --no-create-home --shell /usr/sbin/nologin fullness
sudo mkdir -p /var/www/fullness-api/wwwroot/images/products
sudo mkdir -p /etc/fullness
sudo chown -R fullness:fullness /var/www/fullness-api
```

**systemd ユニット**

Kestrel を常駐サービスとして登録する。`/etc/systemd/system/fullness-api.service`。

```ini
[Unit]
Description=Fullness Stationery データ管理サービス API
After=network.target

[Service]
Type=simple
WorkingDirectory=/var/www/fullness-api
ExecStart=/var/www/fullness-api/Backend.Api
Restart=always
RestartSec=5
User=fullness
Group=fullness
EnvironmentFile=/etc/fullness/api.env
SyslogIdentifier=fullness-api
NoNewPrivileges=true
PrivateTmp=true
ProtectSystem=strict
ProtectHome=true
ReadWritePaths=/var/www/fullness-api/wwwroot/images/products

[Install]
WantedBy=multi-user.target
```

- `EnvironmentFile` で、接続文字列や署名鍵などの設定を外部ファイルから読む（この `api.env` は CD が生成する）。
- `ProtectSystem=strict` でファイルシステムを読み取り専用にし、書き込みを許すのは `ReadWritePaths` に挙げた画像保存先だけに限定する。パストラバーサルが起きても他の場所へは書けない。

登録する。

```bash
sudo systemctl daemon-reload
sudo systemctl enable fullness-api
```

**パスワードなし sudo**

CD は SSH 経由で `sudo systemctl` や `sudo tee` を実行する。パスワードを求められると処理が止まるため、デプロイユーザーにパスワードなし sudo を許可する（演習用の簡易設定）。

```bash
echo "azureuser ALL=(ALL) NOPASSWD:ALL" | sudo tee /etc/sudoers.d/azureuser
sudo chmod 440 /etc/sudoers.d/azureuser
```

### 2. Nginx（リバースプロキシ）

外部からの :80 のリクエストを、内部の Kestrel（127.0.0.1:5000）へ転送する。`/etc/nginx/sites-available/fullness-api`。

```nginx
server {
    listen 80;
    server_name 20.78.11.94;
    client_max_body_size 2m;

    location / {
        proxy_pass         http://127.0.0.1:5000;
        proxy_http_version 1.1;
        proxy_set_header   Host              $host;
        proxy_set_header   X-Forwarded-For   $proxy_add_x_forwarded_for;
        proxy_set_header   X-Forwarded-Proto $scheme;
    }
}
```

- `client_max_body_size 2m` は、画像アップロードの上限（2MB）に合わせる。これを省くと 2MB 近い画像が Nginx の段階で 413 になり、アプリのログに何も残らない。
- Kestrel は `127.0.0.1:5000` にのみバインドし、外部から直接叩けないようにする。公開口は Nginx の :80 だけにする。

有効化し、既定サイトを無効化する。

```bash
sudo ln -s /etc/nginx/sites-available/fullness-api /etc/nginx/sites-enabled/
sudo rm -f /etc/nginx/sites-enabled/default
sudo nginx -t
sudo systemctl reload nginx
```

### 3. GitHub Secrets

CD で使う秘密情報を、リポジトリの Settings → Secrets and variables → Actions に登録しておく。

| Secret 名 | 用途 |
| --- | --- |
| `VM_HOST` | VM のパブリック IP またはホスト名 |
| `VM_USER` | SSH 接続ユーザー（例: `azureuser`） |
| `VM_SSH_KEY` | SSH 秘密鍵の全文 |
| `DB_CONNECTION_STRING` | Azure Database for PostgreSQL の接続文字列 |
| `JWT_SIGNING_KEY` | JWT の署名鍵 |
| `AZURE_BLOB_CONNECTION_STRING` | Azure Blob Storage の接続文字列 |

これらは `api.env` の生成時に埋め込まれる。リポジトリやコードには一切書かない。

### 4. Azure（ネットワーク）

- ネットワークセキュリティグループ（NSG）で、受信ポート **80（HTTP）** と **22（SSH）** を許可する。5000 番や 5432 番は開けない。
- 22 番は SSH デプロイに必要。可能なら接続元を絞る。

## ワークフロー全体

```yaml
name: Backend CD

on:
  workflow_run:
    workflows: ["Backend CI"]   # backend_ci.yml の name と一致させる
    types: [completed]
    branches: [main]
  workflow_dispatch:            # 手動実行も残す

jobs:
  deploy:
    name: Azure VMへデプロイ
    runs-on: ubuntu-latest
    # CIが成功したときのみ実行（手動実行時は無条件で通す）
    if: >-
      github.event_name == 'workflow_dispatch' ||
      github.event.workflow_run.conclusion == 'success'
    env:
      DEPLOY_DIR: /var/www/fullness-api
      SERVICE_NAME: fullness-api

    steps:
      - name: リポジトリをチェックアウト
        uses: actions/checkout@v4

      - name: .NET SDKをセットアップ
        uses: actions/setup-dotnet@v4
        with:
          global-json-file: backend/api/global.json

      - name: 発行
        working-directory: backend/api
        run: dotnet publish Apps/Backend.Api/Backend.Api.csproj --configuration Release --output ${{ github.workspace }}/publish

      - name: SSH鍵を準備
        run: |
          mkdir -p ~/.ssh
          echo "${{ secrets.VM_SSH_KEY }}" > ~/.ssh/deploy_key
          chmod 600 ~/.ssh/deploy_key
          ssh-keyscan -H ${{ secrets.VM_HOST }} >> ~/.ssh/known_hosts

      - name: 環境変数ファイルを生成
        run: |
          ssh -i ~/.ssh/deploy_key ${{ secrets.VM_USER }}@${{ secrets.VM_HOST }} "sudo tee /etc/fullness/api.env > /dev/null <<'EOF'
          ASPNETCORE_ENVIRONMENT=Production
          ASPNETCORE_URLS=http://127.0.0.1:5000
          ConnectionStrings__FullnessEc=${{ secrets.DB_CONNECTION_STRING }}
          Jwt__SigningKey=${{ secrets.JWT_SIGNING_KEY }}
          Jwt__Issuer=http://${{ secrets.VM_HOST }}
          Jwt__Audience=http://${{ secrets.VM_HOST }}
          ImageStorage__PublicBaseUrl=http://${{ secrets.VM_HOST }}
          AuthCookie__Secure=false
          Swagger__Enabled=true
          AzureBlobStorage__ConnectionString=${{ secrets.AZURE_BLOB_CONNECTION_STRING }}
          Cors__AllowedOrigins__0=http://${{ secrets.VM_HOST }}
          EOF
          sudo chown root:fullness /etc/fullness/api.env
          sudo chmod 640 /etc/fullness/api.env"

      - name: 成果物を転送
        run: rsync -az --delete -e "ssh -i ~/.ssh/deploy_key" ${{ github.workspace }}/publish/ ${{ secrets.VM_USER }}@${{ secrets.VM_HOST }}:/tmp/fullness-publish/

      - name: 成果物を配置
        run: |
          ssh -i ~/.ssh/deploy_key ${{ secrets.VM_USER }}@${{ secrets.VM_HOST }} "\
            sudo rsync -a --delete --exclude 'wwwroot/images/products/' /tmp/fullness-publish/ ${{ env.DEPLOY_DIR }}/ && \
            sudo mkdir -p ${{ env.DEPLOY_DIR }}/wwwroot/images/products && \
            sudo chown -R fullness:fullness ${{ env.DEPLOY_DIR }} && \
            sudo chmod +x ${{ env.DEPLOY_DIR }}/Backend.Api"

      - name: サービスを再起動
        run: |
          ssh -i ~/.ssh/deploy_key ${{ secrets.VM_USER }}@${{ secrets.VM_HOST }} "\
            sudo systemctl restart ${{ env.SERVICE_NAME }} && \
            sleep 8 && \
            sudo systemctl is-active ${{ env.SERVICE_NAME }}"
```

## 各ステップの説明

**発行** — `dotnet publish` で、実行可能な成果物を生成する。VM に .NET ランタイムを入れてあるため、framework-dependent（ランタイム同梱なし）で発行する。

**SSH鍵を準備** — Secrets の秘密鍵を一時ファイルに書き出し、`ssh-keyscan` で接続先を既知ホストに登録する。これで対話的な確認なしに SSH 接続できる。

**環境変数ファイルを生成** — VM 上の `/etc/fullness/api.env` を、Secrets の値から組み立てる。接続文字列・署名鍵はここでのみ埋め込む。画像URLのオリジンやCORS許可元も VM のアドレスに合わせる。`AuthCookie__Secure=false` は HTTP 運用のための設定。

**成果物を転送** — `rsync` で発行結果を VM の一時ディレクトリへ送る。`--delete` で古いファイルを掃除する。

**配置してサービスを起動** — 一時ディレクトリから本番ディレクトリへ配置する。このとき、アップロード済み画像フォルダは `--exclude` で保護する。所有者と実行権限を整えてから `systemctl restart` で再起動し、数秒待って `is-active` で起動を確認する。

## デプロイ後の確認

```bash
# 未認証は 401
curl -i http://<VM_HOST>/api/admin/categories

# ログイン → 200 と Set-Cookie
curl -i -c /tmp/c.txt -X POST http://<VM_HOST>/api/admin/auth/login \
  -H 'Content-Type: application/json' \
  -d '{"accountName":"fullness","password":"Password123"}'

# Cookie 付きで取得 → 200
curl -s -b /tmp/c.txt http://<VM_HOST>/api/admin/categories
```

VM 側でサービスの状態やログを確認する場合。

```bash
sudo systemctl status fullness-api --no-pager
journalctl -u fullness-api -n 50 --no-pager
```

## 注意点

- **HTTP 運用のため、認証Cookie は `Secure=false`**。JWT が平文で流れるため、本番相当の運用では HTTPS 化と `AuthCookie__Secure=true` への切り替えが望ましい。
- **`workflow_run` は「次に CI が完了したとき」から有効**になる。CD を追加した直後の初回は、手動実行（Run workflow）で動作を確認するとよい。
- **起動確認の待機時間**（`sleep 8`）は、Azure DB への初回接続に時間がかかる場合を見込んだもの。短すぎると、起動途中を失敗と誤判定することがある。
- **秘密情報は Secrets に一元管理**し、`api.env` は CD が毎回生成する。VM 上に秘密情報を手で置かない運用にすると、鍵のローテーション時も Secrets の更新だけで済む。
