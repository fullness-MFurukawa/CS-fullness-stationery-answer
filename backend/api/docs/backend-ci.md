# バックエンド CI（backend_ci.yml）

## 目的

`main` へのpush または Pull Request をトリガーに、バックエンドの品質を自動検証する。
ビルド・静的解析・書式チェック・4層すべてのテストを実行し、問題があればマージ前に検知する。

# 事前準備
- インフラストラクチャ層のテストプロジェクトに、appsettings.Test.jsonを作成して接続文字列を記述しておく。
- dbディレクトリを作成し、テーブルの作成用SQLファイルとデータ登録用SQLファイルを用意する。

## トリガー

- `main` ブランチへの push
- `main` ブランチへの Pull Request

## 処理の流れ

1. PostgreSQL（サービスコンテナ）を起動する
2. スキーマとサンプルデータを投入する
3. 依存パッケージを復元する
4. ビルドする（警告をエラーとして扱う）
5. 書式をチェックする（`dotnet format`）
6. ドメイン層・アプリケーション層・インフラ層・API層のテストを実行する

## ワークフロー全体
```yml
name: Backend CI

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  build-and-test:
    name: ビルドとテスト
    runs-on: ubuntu-latest

    # リポジトリ統合テストで使用するPostgreSQL
    # ジョブの実行中だけ起動する使い捨てのコンテナで、テスト終了後に破棄される
    services:
      postgres:
        # PostgreSQL 17 の公式イメージを使用する
        image: postgres:17
        env:
          # コンテナ起動時に作成する管理ユーザーとパスワードは、使い捨てのため固定値を利用
          POSTGRES_USER: postgres
          POSTGRES_PASSWORD: postgres
          # コンテナ起動時に自動作成するデータベース名
          POSTGRES_DB: fullness_ec
        ports:
          # コンテナの5432番を、ジョブ実行環境の5432番へ公開する
          - 5432:5432
          # DBが接続を受け付けられる状態になるまでヘルスチェックで待機する
          # これが完了する前にテストが走ると接続エラーになるため、起動完了を保証する
        options: >-
          --health-cmd "pg_isready -U postgres"
          --health-interval 10s
          --health-timeout 5s
          --health-retries 5

    env:
      # インフラストラクチャ層のテストプロジェクトに作成した、appsettings.Test.json の接続文字列を上書きする
      ConnectionStrings__FullnessEc: "Host=localhost;Port=5432;Database=fullness_ec;Username=postgres;Password=postgres"
     
    steps:
      - name: リポジトリをチェックアウト
        uses: actions/checkout@v4

      - name: .NET SDKをセットアップ
        uses: actions/setup-dotnet@v4
        with:
          global-json-file: backend/api/global.json

      - name: スキーマとサンプルデータを投入
        # パスの基準はリポジトリのルートになるので環境に合わせて修正する
        working-directory: backend/api
        # データベースは POSTGRES_DBにより作成済みのためデータベース作成は実行しない
        env:
          PGPASSWORD: postgres
        run: |
          psql -h localhost -U postgres -d fullness_ec -v ON_ERROR_STOP=1 -f db/02_create_tables.sql
          psql -h localhost -U postgres -d fullness_ec -v ON_ERROR_STOP=1 -f db/03_insert_sample_data.sql

      - name: 依存パッケージを復元
        working-directory: backend/api
        run: dotnet restore

      - name: ビルド（警告をエラーとして扱う）
        working-directory: backend/api
        run: dotnet build --configuration Release --no-restore -warnaserror

      - name: 書式チェック
        working-directory: backend/api
        run: dotnet format --verify-no-changes --severity warn --no-restore    

      - name: テスト（ドメイン層）
        working-directory: backend/api
        run: dotnet test Tests/Backend.Domain.Tests/Backend.Domain.Tests.csproj --configuration Release --no-build

      - name: テスト（アプリケーション層）
        working-directory: backend/api
        run: dotnet test Tests/Backend.Application.Tests/Backend.Application.Tests.csproj --configuration Release --no-build

      - name: テスト（インフラストラクチャ層）
        working-directory: backend/api
        run: dotnet test Tests/Backend.Infrastructure.Tests/Backend.Infrastructure.Tests.csproj --configuration Release --no-build

      - name: テスト（API層）
        working-directory: backend/api
        run: dotnet test Tests/Backend.Api.Tests/Backend.Api.Tests.csproj --configuration Release --no-build
```

## 補足

- リポジトリ統合テストは実データベースを使うため、サービスコンテナの PostgreSQL 17 を起動し、接続文字列を環境変数で上書きする
- `-warnaserror` により、警告を1件でも残さない品質基準を強制している