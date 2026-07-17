import { test, expect } from "@playwright/test";

/**
 * 商品の登録から修正・削除までの一連の流れのE2Eテスト
 * BP012（新商品登録）、BP009（商品修正）、BP010（商品削除）を通しで検証する。
 *
 * @remarks
 * このテストは Azure 上の実際のデータベースを更新する。
 * 既存のデータを壊さないよう、テスト専用の商品を自分で登録し、
 * 最後に自分で削除して、実行前の状態へ戻す。
 * これを「テストの後始末」と呼び、繰り返し実行できるテストの条件になる。
 *
 * test.describe.serial() は、中のテストを記述順に実行し、
 * 途中で失敗したら残りを飛ばす。
 * 通常テストは互いに独立しているべきだが、
 * ここでは「登録した商品を修正し、削除する」という前後関係があるため、
 * 意図的に順序を持たせている。
 */
test.describe.serial("商品の登録・修正・削除", () => {
    /**
     * テストで使う商品名。
     * 実行のたびに一意にするため、現在時刻のミリ秒を付ける。
     * 固定の名前にすると、前回の実行で削除に失敗した商品が残っていた場合に、
     * 同名の商品が複数見つかってテストが不安定になる。
     */
    const productName = `E2Eテスト商品_${Date.now()}`;
    const updatedName = `${productName}_修正後`;

    /**
     * 1×1ピクセルのPNG画像のバイト列。
     * バックエンドは拡張子やContent-Typeだけでなく、
     * ファイル先頭のマジックナンバーまで検証するため、
     * 正しいPNGの構造を持つデータを渡す必要がある。
     */
    const pngBuffer = Buffer.from(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==",
        "base64",
    );

    test("新しい商品を画像つきで登録できる", async ({ page }) => {
        await page.goto("/admin/products/new");

        await page.getByLabel("商品名").fill(productName);

        // カテゴリを選ぶ
        await page.getByRole("combobox").click();
        await page.getByRole("option", { name: "文房具" }).click();

        // 価格と在庫数を入力する。
        // fill() は既存の値を消してから入力するため、初期値の 0 を消す操作は不要。
        await page.getByLabel("価格").fill("1200");
        await page.getByLabel("在庫数").fill("30");

        // 画像を選ぶ。
        // ファイル選択の input は CSS で非表示にしているが、
        // setInputFiles() は実際のクリックを伴わないため、非表示のままで動作する。
        // ファイルのパスではなく、名前・MIMEタイプ・バイト列を直接渡せる。
        // これにより、リポジトリへテスト用の画像ファイルを置かずに済む。
        await page.locator("input[type='file']").setInputFiles({
            name: "test.png",
            mimeType: "image/png",
            buffer: pngBuffer,
        });

        // 登録する
        await page.getByRole("button", { name: "登録する" }).click();

        // 確認ダイアログで入力内容を確かめる
        await expect(
            page.getByRole("heading", { name: "この内容で登録しますか？" }),).toBeVisible();
        await expect(page.getByText("1,200 円")).toBeVisible();
        await expect(page.getByText("30 個")).toBeVisible();
        await expect(page.getByText("test.png")).toBeVisible();

        // ダイアログ内の「登録する」を押す。
        // フォームの送信ボタンと同じ文言のため、
        // ダイアログ（role="alertdialog"）の中に絞り込んで特定する。
        await page.getByRole("alertdialog").getByRole("button", { name: "登録する" }).click();

        // 完了のトーストが表示されることを確認する
        await expect(page.getByText(`商品「${productName}」を登録しました`),).toBeVisible();
    });

    test("登録した商品が商品検索に表示される", async ({ page }) => {
        await page.goto("/admin/products");

        // 登録した商品が一覧に現れることを確認する。
        // バックエンドへ保存され、取得できることの検証になる。
        await expect(page.getByRole("heading", { level: 3, name: productName }),).toBeVisible();
    });

    test("登録した商品を修正できる", async ({ page }) => {
        await page.goto("/admin/products");

        // 対象の商品カードを特定し、その中の修正ボタンを押す。
        // filter({ has: ... }) は「特定の要素を含むもの」で絞り込む。
        // 画面には複数の商品カードがあるため、
        // 「この商品名を含むカード」の修正ボタン、という指定が必要になる。
        const card = page
            .locator("[data-slot='card']")
            .filter({ has: page.getByRole("heading", { name: productName }) });
        await card.getByRole("button", { name: "修正" }).click();

        // 修正ダイアログが開き、既存の値が入っていることを確認する
        const dialog = page.getByRole("dialog");
        await expect(dialog.getByRole("heading", { name: "商品修正" })).toBeVisible();
        await expect(dialog.getByLabel("商品名")).toHaveValue(productName);
        await expect(dialog.getByLabel("価格")).toHaveValue("1200");

        // 商品名と価格を変更する
        await dialog.getByLabel("商品名").fill(updatedName);
        await dialog.getByLabel("価格").fill("1500");
        await dialog.getByRole("button", { name: "更新する" }).click();

        await expect(page.getByText(`商品「${updatedName}」を修正しました`),).toBeVisible();

        // 一覧の表示が変更後の内容へ更新されることを確認する。
        // 画面を再読み込みせずに反映される（applyUpdated による更新）ことの検証。
        await expect(page.getByRole("heading", { level: 3, name: updatedName }),).toBeVisible();
        await expect(page.getByRole("heading", { level: 3, name: productName, exact: true }),).not.toBeVisible();
    });

    test("登録した商品を削除できる", async ({ page }) => {
        await page.goto("/admin/products");

        const card = page
            .locator("[data-slot='card']")
            .filter({ has: page.getByRole("heading", { name: updatedName }) });
        await card.getByRole("button", { name: "削除" }).click();

        // 確認ダイアログで削除を実行する
        await expect(page.getByRole("heading", { name: "商品を削除しますか？" }),).toBeVisible();
        await page.getByRole("button", { name: "削除する" }).click();

        await expect(page.getByText(`「${updatedName}」を削除しました`),).toBeVisible();

        // 一覧から消えることを確認する。
        // これでテスト開始前の状態へ戻り、繰り返し実行できる。
        await expect(page.getByRole("heading", { level: 3, name: updatedName }),).not.toBeVisible();
    });
});