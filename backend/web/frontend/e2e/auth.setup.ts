import { test as setup, expect } from "@playwright/test";
import path from "path";

/** 認証状態を保存するファイルのパス */
const authFile = path.join(__dirname, ".auth/admin.json");

/**
 * ログインを行い、認証状態をファイルへ保存する
 * @remarks
 * このファイルは playwright.config.ts の setup プロジェクトとして、
 * 他のすべてのテストより先に一度だけ実行される。
 * 保存した認証状態を各テストが storageState で読み込むことで、
 * テストごとにログイン操作を繰り返す必要がなくなる。
 */
setup("ログインして認証状態を保存する", async ({ page }) => {
    await page.goto("/admin/login");

    await page.getByLabel("アカウント名").fill("fullness");
    await page.getByLabel("パスワード").fill("Password123");
    await page.getByRole("button", { name: "ログイン" }).click();

    // ログインに成功するとダッシュボードへ遷移する
    await page.waitForURL("/admin");
    await expect(
        page.getByRole("heading", { name: "ダッシュボード" }),
    ).toBeVisible();

    // Cookie と localStorage の状態をファイルへ書き出す
    await page.context().storageState({ path: authFile });
});