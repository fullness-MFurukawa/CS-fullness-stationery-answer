import { test, expect } from "@playwright/test";

/**
 * BP019 商品カテゴリ登録画面のE2Eテスト
 *
 * @remarks
 * カテゴリには削除機能がない（UC013は登録のみ）ため、
 * 商品のように「登録して最後に消す」後始末ができない。
 * このテストを実行するたびにカテゴリが1件増える点に注意すること。
 * テスト用とわかる名前を付け、不要になったら手作業で削除する運用とする。
 */
test.describe("商品カテゴリ登録", () => {
    test.beforeEach(async ({ page }) => {
        await page.goto("/admin/categories/new");
    });

    test("未入力の場合はエラーが表示され、確認ダイアログが開かない", async ({page,}) => {
        await expect(
            page.getByRole("heading", { name: "商品カテゴリ登録" }),
        ).toBeVisible();

        await page.getByRole("button", { name: "登録する" }).click();

        await expect(page.getByText("カテゴリ名を入力してください")).toBeVisible();

        // 入力チェックで止まるため、確認ダイアログは開かない。
        // 「エラーが出ること」だけでなく「先へ進まないこと」まで確認する。
        await expect(page.getByRole("heading", { name: "この内容で登録しますか？" }),).not.toBeVisible();
    });

    test("スペースだけの入力はエラーになる", async ({ page }) => {
        // zod の .trim() により、前後の空白を除いてから文字数を検証する。
        // スペースだけの入力は空文字となり、min(1) で弾かれる。
        await page.getByLabel("カテゴリ名").fill("   ");
        await page.getByRole("button", { name: "登録する" }).click();

        await expect(page.getByText("カテゴリ名を入力してください")).toBeVisible();
    });

    test("31文字以上の入力はエラーになる", async ({ page }) => {
        // データベースの定義は VARCHAR(30)。境界値である31文字で検証する。
        // "あ".repeat(31) で31文字の文字列を作る。
        await page.getByLabel("カテゴリ名").fill("あ".repeat(31));
        await page.getByRole("button", { name: "登録する" }).click();

        await expect(page.getByText("カテゴリ名は30文字以内で入力してください"),).toBeVisible();
    });

    test("確認ダイアログでキャンセルすると登録されない", async ({ page }) => {
        await page.getByLabel("カテゴリ名").fill("キャンセルされるカテゴリ");
        await page.getByRole("button", { name: "登録する" }).click();

        // 確認ダイアログに入力内容が表示される
        await expect(page.getByRole("heading", { name: "この内容で登録しますか？" }),).toBeVisible();
        await expect(page.getByText("キャンセルされるカテゴリ")).toBeVisible();

        await page.getByRole("button", { name: "キャンセル" }).click();

        // ダイアログが閉じ、入力値は残ったままになる（やり直せる）
        await expect(page.getByRole("heading", { name: "この内容で登録しますか？" }),).not.toBeVisible();
        await expect(page.getByLabel("カテゴリ名")).toHaveValue("キャンセルされるカテゴリ",);
    });

    test("カテゴリを登録すると商品検索のプルダウンに現れる", async ({ page }) => {
        // 実行のたびに一意な名前にする
        const categoryName = `E2Eテスト_${Date.now()}`;

        await page.getByLabel("カテゴリ名").fill(categoryName);
        await page.getByRole("button", { name: "登録する" }).click();

        // 確認ダイアログの「登録する」を押す。
        // フォームの送信ボタンと同じ文言のため、ダイアログの中に絞り込む。
        await expect(page.getByRole("heading", { name: "この内容で登録しますか？" }),).toBeVisible();
        await page.getByRole("alertdialog").getByRole("button", { name: "登録する" }).click();

        // 完了のトーストが表示される
        await expect(page.getByText(new RegExp(`${categoryName}.*登録しました`))).toBeVisible();

        // 登録に成功するとフォームがクリアされる（続けて登録できる）
        await expect(page.getByLabel("カテゴリ名")).toHaveValue("");

        // 商品検索画面のカテゴリのプルダウンに現れることを確認する。
        // トーストの表示だけでなく、実際にデータベースへ保存され、
        // 別の画面から取得できることまで確かめる。
        // これは画面をまたぐ検証であり、E2Eテストの利点が出る部分。
        await page.goto("/admin/products");
        await page.getByRole("combobox").click();
        await expect(page.getByRole("option", { name: categoryName }),).toBeVisible();
    });
});