import { defineConfig, devices } from "@playwright/test";

/**
 * Playwright の設定
 * ローカルで起動した Next.js（http://localhost:3000）に対して E2E テストを実行する。
 * バックエンドAPIへのリクエストは next.config.ts のプロキシ経由で Azure の API へ転送される。
 * @see https://playwright.dev/docs/test-configuration
 */
export default defineConfig({
  // テストファイルを置くディレクトリ
  testDir: "./e2e",

  // テストの実行を直列にする
  // 本演習のテストは Azure 上の同一データベースを更新するため、
  // 並列に実行すると、あるテストの登録・削除が別のテストの検索結果に影響する。
  fullyParallel: false,
  workers: 1,

  // CI に test.only が残っていたら失敗させる（一部のテストしか実行されない事故を防ぐ）
  forbidOnly: !!process.env.CI,

  // CI でのみ再試行する（ネットワークの一時的な失敗を吸収する）
  retries: process.env.CI ? 2 : 0,

  // テスト結果をHTMLレポートで出力する。
  // open の既定値は "on-failure"（失敗時のみ自動で開く）。
  // "always" にすると、成功したときもレポートがブラウザで開く。
  // CI では開いても意味がなく、レポートの表示待ちで処理が止まるため "never" にする。
  reporter: [["html", { open: process.env.CI ? "never" : "always" }]],

  // すべてのプロジェクトで共有する設定
  use: {
    // page.goto("/admin") のように、パスだけで指定できるようにする
    baseURL: "http://localhost:3000",

    // 操作の記録（トレース）を採取する。
    // "on-first-retry" は再試行時のみで、retries: 0 のローカルでは採取されない。
    // "retain-on-failure" にすると、失敗したテストのトレースがローカルでも残り、
    // レポートから「どの操作でどう失敗したか」を再生して追える。
    trace: "retain-on-failure",

    // 失敗したテストのスクリーンショットを残す
    screenshot: "only-on-failure",
  },

  projects: [
    // ログインを行い、認証状態をファイルへ保存するためのプロジェクト
    {
      name: "setup",
      testMatch: /auth\.setup\.ts/,
    },

    // 実際のテスト。setup が保存した認証状態を読み込んでから実行する
    {
      name: "chromium",
      use: {
        ...devices["Desktop Chrome"],
        storageState: "e2e/.auth/admin.json",
        // ブラウザを表示せずに実行する（既定）。
        // コマンドラインの --headed を付けると、この設定より優先されて表示される。
        // 環境変数でも切り替えられるようにしておくと、CI とローカルで使い分けやすい。
        headless: process.env.HEADED !== "1",
        // 各操作の間に待ち時間（ミリ秒）を入れて、動きを目で追えるようにする。
        // 環境変数 SLOWMO が指定されたときだけ効かせる。
        // 常時有効にすると通常の実行まで遅くなるため、デモのときだけ使う。
        launchOptions: {
          slowMo: Number(process.env.SLOWMO ?? 0),
        },
      },
      dependencies: ["setup"],
    },
  ],

  // テストの前に Next.js の開発サーバーを起動する
  webServer: {
    command: "npm run dev",
    url: "http://localhost:3000",
    // すでにサーバーが起動していれば、それを使い回す（ローカルでの開発時に便利）
    reuseExistingServer: !process.env.CI,
    timeout: 120 * 1000,
  },

  
});