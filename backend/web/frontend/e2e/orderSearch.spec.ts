import { test, expect } from "@playwright/test";
import type { Page } from "@playwright/test";

/**
 * 注文カードだけを取得する
 *
 * @remarks
 * この画面では検索条件のフォームも Card で囲まれているため、
 * [data-slot='card'] だけで取ると検索条件の枠も含まれてしまう。
 * 注文カードにのみ存在する「注文明細」の文字を手がかりに絞り込む。
 * filter({ has: ... }) は「特定の要素を含むもの」で絞り込むための仕組み。
 * @param page テスト対象のページ
 */
function orderCards(page: Page) {
    return page.locator("[data-slot='card']").filter({ has: page.getByText("注文明細") });
}

/**
 * BP015 購入履歴検索画面のE2Eテスト
 *
 * @remarks
 * この画面には検索条件のプルダウンと、注文カードごとのステータス変更用の
 * プルダウンが同居する。getByRole("combobox") では複数に一致するため、
 * 検索条件側は id で、カード側はカードの内側へ絞り込んで特定する。
 *
 * ステータス変更のテストは既存の注文を更新するが、
 * テストの最後に元の値へ戻すことで、繰り返し実行できるようにしている。
 */
test.describe("購入履歴検索", () => {
    test.beforeEach(async ({ page }) => {
        await page.goto("/admin/orders");
        // 一覧の読み込みが終わるまで待つ
        await expect(page.getByText(/^\d+件の注文$/)).toBeVisible();
    });

    test("注文の一覧が明細つきで表示される", async ({ page }) => {
        await expect(page.getByRole("heading", { name: "購入履歴検索" }),).toBeVisible();

        // 注文カードが1件以上あることを確認する
        const cards = orderCards(page);
        expect(await cards.count()).toBeGreaterThan(0);

        // 先頭のカードに、注文の概要と明細が表示されていることを確認する。
        // 注文と注文明細を結合して返すバックエンドの実装が
        // 画面まで届いていることの検証になる。
        const firstCard = cards.first();
        await expect(firstCard.getByText("注文明細")).toBeVisible();
        await expect(firstCard.getByText(/支払い方法: /)).toBeVisible();
        await expect(firstCard.getByText("合計")).toBeVisible();
    });

    test("顧客アカウント名で絞り込める", async ({ page }) => {
        // 先頭の注文から、実在する顧客アカウント名を取り出す。
        // テストに固定値を書くと、データが変わったときに壊れる。
        // 画面に表示されている実際の値を使うことで、データに依存しないテストになる。
        // 顧客名は「氏名（アカウント名）」の形式で表示される。
        const customerText = await orderCards(page)
            .first()
            .getByText(/^（.+）$/)
            .textContent();
        const accountName = customerText?.match(/（(.+)）/)?.[1];
        expect(accountName).toBeTruthy();

        await page.getByLabel("顧客アカウント名").fill(accountName!);
        await page.getByRole("button", { name: "検索" }).click();

        // 検索結果の描画が終わるまで待つ。
        // 検索中は一覧がスケルトンに差し替わるため、待たずに count() を呼ぶと0件になる。
        // count() は expect() と違って自動待機しない。現在の数をその場で返すだけである。
        // そのため、まず自動待機の効く expect() で描画の完了を待ってから数える。
        await expect(page.getByText(/^\d+件の注文$/)).toBeVisible();

        // 絞り込み後のカードが、すべてその顧客のものであることを確認する
        const cards = orderCards(page);
        const count = await cards.count();
        expect(count).toBeGreaterThan(0);
        for (let i = 0; i < count; i++) {
            await expect(cards.nth(i).getByText(`（${accountName}）`)).toBeVisible();
        }
    });

    test("注文ステータスで絞り込める", async ({ page }) => {
        // 先頭の注文の実際のステータスを取り出し、それで絞り込む。
        // 「注文済」のような固定値を書くと、そのステータスの注文が
        // 1件もないデータベースではテストが失敗する。
        // 画面にある実在の値を使うことで、データの状態に依存しないテストになる。
        const targetStatus = await orderCards(page)
            .first()
            .locator("[data-slot='select-value']")
            .textContent();

        // 検索条件のプルダウンは id="statusFilter" を持つ。
        // カード側のプルダウンと区別するために id で特定する。
        await page.locator("#statusFilter").click();
        await page.getByRole("option", { name: targetStatus!, exact: true }).click();
        await page.getByRole("button", { name: "検索" }).click();

        // 検索結果の描画が終わるまで待つ（count() は自動待機しないため）
        await expect(page.getByText(/^\d+件の注文$/)).toBeVisible();

        // 絞り込み後のカードのステータスが、すべて選んだステータスであることを確認する。
        // カードの内側の select-value に絞り込むことで、
        // 検索条件側のプルダウンと混同しないようにする。
        const cards = orderCards(page);
        const count = await cards.count();
        expect(count).toBeGreaterThan(0);
        for (let i = 0; i < count; i++) {
            await expect(cards.nth(i).locator("[data-slot='select-value']"),).toHaveText(targetStatus!);
        }
    });

    test("該当する注文がない場合はその旨が表示される", async ({ page }) => {
        // 存在しない顧客アカウント名で検索する
        await page.getByLabel("顧客アカウント名").fill("notexistuser999");
        await page.getByRole("button", { name: "検索" }).click();

        await expect(page.getByText("該当する注文がありません")).toBeVisible();
        await expect(page.getByText("0件の注文")).toBeVisible();
    });

    test("クリアボタンで条件が消え全件に戻る", async ({ page }) => {
        // 絞り込み前の件数を控える
        const countText = await page.getByText(/^\d+件の注文$/).textContent();
        const totalCount = Number(countText?.match(/\d+/)?.[0]);

        // 条件を入れて検索する。
        // 条件が1つでも入るとクリアボタンが現れる仕様。
        await page.getByLabel("顧客アカウント名").fill("notexistuser999");
        await page.getByRole("button", { name: "検索" }).click();
        await expect(page.getByText("該当する注文がありません")).toBeVisible();

        // クリアする
        await page.getByRole("button", { name: "クリア" }).click();

        // 入力欄が空になり、全件に戻る
        await expect(page.getByLabel("顧客アカウント名")).toHaveValue("");
        await expect(page.getByText(`${totalCount}件の注文`)).toBeVisible();

        // 条件がなくなるとクリアボタンは消える
        await expect(page.getByRole("button", { name: "クリア" })).not.toBeVisible();
    });

    test("購入日で絞り込める", async ({ page }) => {
        // 注文が存在しないはずの過去の日付で検索する。
        // type="date" の入力欄には yyyy-MM-dd の形式で値を渡す。
        await page.getByLabel("購入日").fill("2000-01-01");
        await page.getByRole("button", { name: "検索" }).click();

        await expect(page.getByText("該当する注文がありません")).toBeVisible();
    });

    test("注文のステータスを変更できる", async ({ page }) => {
        const firstCard = orderCards(page).first();

        // 変更前のステータスを控える。テストの最後に元へ戻すために使う。
        const originalStatus = await firstCard
            .locator("[data-slot='select-value']")
            .textContent();

        // 現在と異なるステータスを選ぶ。
        // 同じ値を選んでも OrderCard の handleChange が早期に return するため、
        // 更新が起きず検証にならない。
        const newStatus = originalStatus === "注文済" ? "入金済" : "注文済";

        await firstCard.getByRole("combobox").click();
        await page.getByRole("option", { name: newStatus, exact: true }).click();

        // 完了のトーストが表示される
        await expect(page.getByText(`ステータスを「${newStatus}」に更新しました`),).toBeVisible();

        // 画面の表示も変更後の値になる。
        // フックの setOrders による一覧の更新が効いていることの検証。
        await expect(firstCard.locator("[data-slot='select-value']")).toHaveText(newStatus,);

        // 再読み込みしても変更が残っていることを確認する。
        // 画面上の見た目だけでなく、データベースへ保存されたことの検証になる。
        await page.reload();
        await expect(page.getByText(/^\d+件の注文$/)).toBeVisible();
        const cardAfterReload = orderCards(page).first();
        await expect(cardAfterReload.locator("[data-slot='select-value']"),).toHaveText(newStatus);

        // 後始末として元のステータスへ戻す。
        // これによりテストを何度でも実行できる状態を保つ。
        await cardAfterReload.getByRole("combobox").click();
        await page.getByRole("option", { name: originalStatus!, exact: true }).click();
        await expect(page.getByText(`ステータスを「${originalStatus}」に更新しました`),).toBeVisible();
    });
});