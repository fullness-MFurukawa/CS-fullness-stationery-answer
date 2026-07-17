import { test, expect } from "@playwright/test";

/**
 * BP006 商品検索画面のE2Eテスト
 *
 * @remarks
 * 商品の一覧・絞り込み・削除ダイアログの表示を検証する。
 * 実際に商品を削除する流れは、登録から削除までを一続きで扱う
 * productLifecycle.spec.ts で検証する。
 * 他のテストが使う既存データを消してしまうと、
 * テストの実行順序によって結果が変わる不安定なテストになるためである。
 */
test.describe("商品検索", () => {
    test.beforeEach(async ({ page }) => {
        await page.goto("/admin/products");
    });

    test("商品の一覧が表示される", async ({ page }) => {
        await expect(page.getByRole("heading", { name: "商品検索" })).toBeVisible();

        // 商品件数は登録状況によって変わるため、具体的な数ではなく形式を検証する。
        // 「読み込み中...」が「n件の商品」へ変わるまで自動待機が働く。
        await expect(page.getByText(/^\d+件の商品$/)).toBeVisible();

        // 商品カードが1件以上表示されることを確認する。
        // カードの修正ボタンの個数で数える。
        const editButtons = page.getByRole("button", { name: "修正" });
        expect(await editButtons.count()).toBeGreaterThan(0);
    });

    test("カテゴリで商品を絞り込める", async ({ page }) => {
        // 絞り込み前の件数を控える。
        // textContent() で要素の文字列を取り出し、数値へ変換する。
        const countText = await page.getByText(/^\d+件の商品$/).textContent();
        const totalCount = Number(countText?.match(/\d+/)?.[0]);

        // カテゴリのプルダウンを開く。
        // shadcn/ui の Select は内部で Base UI を使っており、
        // トリガーは <button> だが ARIA のロールは combobox になる。
        // そのため getByRole("button") では取得できない。
        const categoryFilter = page.getByRole("combobox");
        await categoryFilter.click();

        // 選択肢はロール option として公開されるため、getByRole で選べる
        await page.getByRole("option", { name: "文房具" }).click();

        // 絞り込み後は、選んだカテゴリ名がトリガーに表示される。
        // トリガーには選択値のほかに開閉アイコンも含まれるため、
        // 値の部分（data-slot="select-value"）に絞って検証する。
        await expect(page.locator("[data-slot='select-value']")).toHaveText("文房具");

        // 絞り込み後の件数は、全件以下になる
        const filteredText = await page.getByText(/^\d+件の商品$/).textContent();
        const filteredCount = Number(filteredText?.match(/\d+/)?.[0]);
        expect(filteredCount).toBeLessThanOrEqual(totalCount);

        // 表示されているカードのカテゴリが、すべて選んだカテゴリであることを確認する。
        // 絞り込みが「件数が減った」だけでなく「正しく効いている」ことの検証。
        const badges = page.locator("[data-slot='badge']");
        const badgeCount = await badges.count();
        expect(badgeCount).toBeGreaterThan(0);
        for (let i = 0; i < badgeCount; i++) {
            await expect(badges.nth(i)).toHaveText("文房具");
        }
    });

    test("「すべてのカテゴリ」へ戻すと全件が表示される", async ({ page }) => {
        const countText = await page.getByText(/^\d+件の商品$/).textContent();
        const totalCount = Number(countText?.match(/\d+/)?.[0]);

        const categoryFilter = page.getByRole("combobox");
        const categoryValue = page.locator("[data-slot='select-value']");

        // いったん絞り込む
        await categoryFilter.click();
        await page.getByRole("option", { name: "文房具" }).click();
        await expect(categoryValue).toHaveText("文房具");

        // 「すべてのカテゴリ」へ戻す
        await categoryFilter.click();
        await page.getByRole("option", { name: "すべてのカテゴリ" }).click();

        // 元の件数に戻ることを確認する
        await expect(page.getByText(`${totalCount}件の商品`)).toBeVisible();
    });

    test("削除ボタンで確認ダイアログが表示され、キャンセルすると削除されない", async ({page,}) => {
        // 先頭の商品カードの商品名を控える。
        // .first() は複数一致するロケーターから最初の1件へ絞る。
        const firstCardName = await page
            .getByRole("heading", { level: 3 })
            .first()
            .textContent();

        await page.getByRole("button", { name: "削除" }).first().click();

        // 確認ダイアログが表示され、対象の商品名が示されることを確認する
        await expect(page.getByRole("heading", { name: "商品を削除しますか？" }),).toBeVisible();
        await expect(page.getByText(`「${firstCardName}」を削除します。`),).toBeVisible();

        // キャンセルする
        await page.getByRole("button", { name: "キャンセル" }).click();

        // ダイアログが閉じ、商品が残っていることを確認する。
        // 「削除できること」と同じくらい「誤って削除されないこと」の検証は重要。
        await expect(page.getByRole("heading", { name: "商品を削除しますか？" }),).not.toBeVisible();
        await expect(page.getByRole("heading", { level: 3, name: firstCardName! }),).toBeVisible();
    });
});