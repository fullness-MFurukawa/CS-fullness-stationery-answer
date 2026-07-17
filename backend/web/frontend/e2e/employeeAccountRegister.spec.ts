import { test, expect } from "@playwright/test";

/**
 * BP003 担当者アカウント登録画面のE2Eテスト
 *
 * @remarks
 * この画面は「アカウント未登録の社員」だけを選択肢に出す。
 * そのため登録を1件行うたびに選択肢が1人減り、
 * 社員は有限なので、いつかは登録できる社員がいなくなる。
 * 商品のように後始末で消すこともできない（アカウントの削除機能はない）。
 *
 * そこで、このファイルでは
 * 「データを変えないテスト」（入力チェック・重複エラー・キャンセル）を常時実行し、
 * 「実際に登録するテスト」は test.skip() で普段は実行しない構成とする。
 * 確認したいときは skip を外して実行する。
 */
test.describe("担当者アカウント登録", () => {
    test.beforeEach(async ({ page }) => {
        await page.goto("/admin/employee-accounts/new");
    });

    test("未入力の場合はすべての項目にエラーが表示される", async ({ page }) => {
        await page.getByRole("button", { name: "登録する" }).click();

        await expect(page.getByRole("paragraph").filter({ hasText: "社員を選択してください" }),).toBeVisible();
        await expect(page.getByText("アカウント名を入力してください")).toBeVisible();
        await expect(page.getByText("パスワードを入力してください")).toBeVisible();
        // 入力チェックで止まるため、確認ダイアログは開かない
        await expect(page.getByRole("heading", { name: "この内容で登録しますか？" }),).not.toBeVisible();
    });

    test("アカウント名が4文字以下の場合はエラーになる", async ({ page }) => {
        // 仕様は5〜20文字。境界値の1つ手前である4文字で検証する。
        await page.getByLabel("アカウント名").fill("abcd");
        await page.getByLabel("パスワード").fill("password1");
        await page.getByRole("button", { name: "登録する" }).click();

        await expect(page.getByText("アカウント名は5〜20文字で入力してください"),).toBeVisible();
    });

    test("アカウント名に記号が含まれる場合はエラーになる", async ({ page }) => {
        // 半角英数字のみを許可する。ハイフンを含めて検証する。
        await page.getByLabel("アカウント名").fill("test-user");
        await page.getByLabel("パスワード").fill("password1");
        await page.getByRole("button", { name: "登録する" }).click();

        await expect(page.getByText("アカウント名は半角英数字で入力してください"),).toBeVisible();
    });

    test("パスワードが4文字以下の場合はエラーになる", async ({ page }) => {
        await page.getByLabel("アカウント名").fill("testuser");
        await page.getByLabel("パスワード").fill("abcd");
        await page.getByRole("button", { name: "登録する" }).click();

        await expect(page.getByText("パスワードは5〜20文字で入力してください"),).toBeVisible();
    });

    test("パスワードは伏せ字で入力される", async ({ page }) => {
        // 入力欄の type が password であることを確認する。
        // 画面の見た目に関わる仕様であり、セキュリティ上の配慮でもある。
        await expect(page.getByLabel("パスワード")).toHaveAttribute("type","password",);
    });

    test("既に使われているアカウント名では登録できない", async ({ page }) => {
        // 既存のアカウント名を指定すると、バックエンドが 409 を返す。
        // フロントエンドはそれを ApiError.isConflict で判定し、
        // アカウント名の項目のエラーとして表示する。
        //
        // このテストは登録が失敗するため、データベースの状態を変えない。
        // 「失敗することを確かめるテスト」は安全に繰り返せるという利点がある。

        // 社員を選ぶ（先頭の候補を使う）
        await page.getByRole("combobox").click();
        await page.getByRole("option").first().click();

        // 既にログインに使っているアカウント名を指定する
        await page.getByLabel("アカウント名").fill("fullness");
        await page.getByLabel("パスワード").fill("password1");
        await page.getByRole("button", { name: "登録する" }).click();

        // 確認ダイアログで登録を実行する
        await page.getByRole("alertdialog").getByRole("button", { name: "登録する" }).click();

        // 重複のエラーが表示され、ダイアログは閉じる
        await expect(page.getByText("このアカウント名は既に使用されています"),).toBeVisible();
        await expect(page.getByRole("heading", { name: "この内容で登録しますか？" }),).not.toBeVisible();
    });

    test("確認ダイアログでパスワードが伏せ字で表示される", async ({ page }) => {
        await page.getByRole("combobox").click();
        await page.getByRole("option").first().click();
        await page.getByLabel("アカウント名").fill("testuser01");
        await page.getByLabel("パスワード").fill("secret123");
        await page.getByRole("button", { name: "登録する" }).click();

        const dialog = page.getByRole("alertdialog");
        await expect(dialog.getByText("testuser01")).toBeVisible();

        // パスワードは「•」の並びで表示され、平文は現れない
        await expect(dialog.getByText("•".repeat(9))).toBeVisible();
        await expect(dialog.getByText("secret123")).not.toBeVisible();

        // キャンセルして登録しない
        await page.getByRole("button", { name: "キャンセル" }).click();
        await expect(page.getByRole("heading", { name: "この内容で登録しますか？" }),).not.toBeVisible();
    });

    /**
     * 実際にアカウントを登録するテスト。
     *
     * @remarks
     * 実行するとアカウント未登録の社員が1人減り、元に戻せない。
     * そのため普段は test.skip() で実行しない。
     * 動作を確認したいときは、次の行の test.skip を test に変えて実行する。
     */
    test.skip("社員を選んでアカウントを登録できる", async ({ page }) => {
        // 実行のたびに一意なアカウント名にする。
        // 半角英数字のみで20文字以内という制約があるため、
        // 現在時刻の下8桁を使う。
        const accountName = `e2e${String(Date.now()).slice(-8)}`;

        // 先頭の社員を選び、名前を控える
        await page.getByRole("combobox").click();
        const firstOption = page.getByRole("option").first();
        const employeeLabel = await firstOption.textContent();
        await firstOption.click();

        // 「氏名（部署名）」の形式で表示されるため、氏名の部分だけ取り出す
        const employeeName = employeeLabel?.split("（")[0];

        await page.getByLabel("アカウント名").fill(accountName);
        await page.getByLabel("パスワード").fill("password1");
        await page.getByRole("button", { name: "登録する" }).click();

        await page.getByRole("alertdialog").getByRole("button", { name: "登録する" }).click();

        // 完了のトーストが表示される
        await expect(page.getByText(`「${employeeName}」のアカウントを登録しました`),).toBeVisible();

        // 登録に成功するとフォームがクリアされる
        await expect(page.getByLabel("アカウント名")).toHaveValue("");

        // 登録した社員は選択肢から消える（二重登録を防ぐ）。
        // フックの setEmployees によるリストの更新が効いていることの検証。
        await page.getByRole("combobox").click();
        await expect(page.getByRole("option", { name: new RegExp(`^${employeeName}`) }),).not.toBeVisible();
    });
});