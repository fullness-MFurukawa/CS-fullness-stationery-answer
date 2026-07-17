import { test, expect } from "@playwright/test";

/**
 * BP001 メニュー画面（ダッシュボード）のE2Eテスト
 *
 * @remarks
 * このファイルのテストは、playwright.config.ts で設定した認証状態を
 * そのまま使うため、ログイン済みの状態から始まる。
 * login.spec.ts のような test.use() による打ち消しは不要。
 *
 * ダッシュボードの集計値はバックエンドの
 * GET /api/admin/dashboard/summary が返す実データであり、
 * 商品の登録や注文の追加によって変わる。
 * そのため「4件ちょうど」のような固定値ではなく、
 * 「数値が表示されていること」を検証する。
 * 変化するデータに対しては、値そのものではなく形式を検証するのが定石。
 */
test.describe("ダッシュボード", () => {
    /**
     * 各テストの前に、共通してダッシュボードを開く。
     * beforeEach に括り出すことで、各テストは本題の検証だけに集中できる。
     */
    test.beforeEach(async ({ page }) => {
        await page.goto("/admin");
    });

    test("集計カードが4種類表示される", async ({ page }) => {
        await expect(
            page.getByRole("heading", { name: "ダッシュボード" }),
        ).toBeVisible();

        // 4種類の集計項目がそろっていることを確認する
        await expect(page.getByText("登録商品数")).toBeVisible();
        await expect(page.getByText("カテゴリ数")).toBeVisible();
        await expect(page.getByText("注文件数")).toBeVisible();
        await expect(page.getByText("売上合計")).toBeVisible();
        await expect(page.getByText("完了", { exact: true })).toBeVisible();
    });

    test("集計値がバックエンドから取得され数値で表示される", async ({ page }) => {
        // 読み込み中はスケルトンが表示され、取得後に値へ差し替わる。
        // getByText() に正規表現を渡すと、部分一致ではなくパターンで検索できる。
        // 「1桁以上の数字＋スペース＋件」という形式の文字列が現れるまで待つ。
        await expect(page.getByText(/^\d+ 件$/).first()).toBeVisible();

        // 売上合計は3桁区切りのカンマを含むため、別のパターンで確認する
        await expect(page.getByText(/^[\d,]+ 円$/)).toBeVisible();
    });

    test("ステータス別の注文件数が表示される", async ({ page }) => {
        await expect(page.getByText("ステータス別の注文")).toBeVisible();

        // 注文ステータスは4種類（注文済・入金済・配送中・完了）ある。
        // 件数が0のステータスも表示される仕様のため、必ず4件そろう。
        // これはバックエンドの DashboardSummaryInteractor が
        // 注文のないステータスも欠落させずに返していることの検証になる。
        await expect(page.getByText("注文済")).toBeVisible();
        await expect(page.getByText("入金済")).toBeVisible();
        await expect(page.getByText("配送中")).toBeVisible();
        await expect(page.getByText("完了")).toBeVisible();
    });

    test("ログイン中の担当者名が表示される", async ({ page }) => {
        // NextAuth のセッションに保持した担当者名が画面へ渡っていることを確認する
        await expect(page.getByText("ようこそ、フルネス太郎さん")).toBeVisible();
    });
});