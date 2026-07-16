import { describe, it, expect, beforeEach, afterEach, vi } from "vitest";
import { ProductRepository } from "@/infrastructure/repository/productRepository";
import { HttpClient } from "@/infrastructure/http/httpClient";
import { ApiError } from "@/infrastructure/http/apiError";
import type { Product } from "@/models/responses/product";
import type { ProductRegisterRequest } from "@/models/requests/productRegisterRequest";
import type { ProductUpdateRequest } from "@/models/requests/productUpdateRequest";

/**
 * fetch のモックを組み立てるヘルパー
 * @param status HTTPステータス
 * @param body レスポンスボディ（JSONへ変換される）
 */
function mockFetch(status: number, body: unknown) {
    const text = body === undefined ? "" : JSON.stringify(body);
    return vi.fn().mockResolvedValue({
        ok: status >= 200 && status < 300,
        status,
        text: () => Promise.resolve(text),
        json: () => Promise.resolve(body),
    } as Response);
}

describe("ProductRepositoryの単体テストドライバ", () => {
    let repository: ProductRepository;

    /**
     * テスト用Product
     */
    const sampleProduct: Product = {
        productId: "aaaa-1111",
        name: "水性ボールペン(黒)",
        price: 120,
        imageUrl: null,
        quantity: 10,
        categoryId: "cat-1",
        categoryName: "文房具",
        isDeleted: false,
    };

    /**
     * テストの前処理
     */
    beforeEach(() => {
        // ベースURLは空（Proxy経由の相対パス）
        repository = new ProductRepository(new HttpClient(""));
    });
    /**
     * テストの後処理
     */
    afterEach(() => {
        vi.restoreAllMocks();
    });


    describe("search:指定されたカテゴリIDの商品を検索する", () => {
        it("カテゴリID未指定のとき全件取得のURLでGETする", async () => {
            const fetchMock = mockFetch(200, [sampleProduct]);
            vi.stubGlobal("fetch", fetchMock);

            const result = await repository.search();

            expect(fetchMock).toHaveBeenCalledOnce();
            const [url, options] = fetchMock.mock.calls[0];
            expect(url).toBe("/api/admin/products");
            expect(options.method).toBe("GET");
            expect(options.credentials).toBe("include");
            expect(result).toHaveLength(1);
            expect(result[0].name).toBe("水性ボールペン(黒)");
        });

        it("カテゴリID指定のときクエリ付きURLでGETする", async () => {
            const fetchMock = mockFetch(200, []);
            vi.stubGlobal("fetch", fetchMock);

            await repository.search("cat-1");

            const [url] = fetchMock.mock.calls[0];
            expect(url).toBe("/api/admin/products?categoryId=cat-1");
        });
    });

    describe("register:商品登録", () => {
        it("multipart/form-dataでPOSTし登録された商品を返す", async () => {
            const fetchMock = mockFetch(201, sampleProduct);
            vi.stubGlobal("fetch", fetchMock);

            const request: ProductRegisterRequest = {
                name: "水性ボールペン(黒)",
                price: 120,
                categoryId: "cat-1",
                quantity: 10,
                image: null,
            };
            const result = await repository.register(request);

            const [url, options] = fetchMock.mock.calls[0];
            expect(url).toBe("/api/admin/products");
            expect(options.method).toBe("POST");
            expect(options.body).toBeInstanceOf(FormData);

            const form = options.body as FormData;
            expect(form.get("Name")).toBe("水性ボールペン(黒)");
            expect(form.get("Price")).toBe("120");
            expect(form.get("CategoryId")).toBe("cat-1");
            expect(form.get("Quantity")).toBe("10");
            expect(form.get("Image")).toBeNull();

            expect(result.productId).toBe("aaaa-1111");
        });

        it("画像を指定するとImageフィールドを含める", async () => {
            const fetchMock = mockFetch(201, sampleProduct);
            vi.stubGlobal("fetch", fetchMock);

            const image = new File(["dummy"], "pen.png", { type: "image/png" });
            const request: ProductRegisterRequest = {
                name: "水性ボールペン(黒)",
                price: 120,
                categoryId: "cat-1",
                quantity: 10,
                image,
            };
            await repository.register(request);

            const form = fetchMock.mock.calls[0][1].body as FormData;
            expect(form.get("Image")).toBeInstanceOf(File);
        });
  });

  describe("update:商品の変更", () => {
        it("商品IDをパスに含めてPUTし、本文には商品IDを含めない", async () => {
            const fetchMock = mockFetch(200, sampleProduct);
            vi.stubGlobal("fetch", fetchMock);

            const request: ProductUpdateRequest = {
                productId: "aaaa-1111",
                name: "水性ボールペン(黒) 改",
                price: 150,
                categoryId: "cat-1",
                quantity: 20,
                image: null,
                removeImage: false,
            };
            await repository.update(request);

            const [url, options] = fetchMock.mock.calls[0];
            expect(url).toBe("/api/admin/products/aaaa-1111");
            expect(options.method).toBe("PUT");

            const form = options.body as FormData;
            expect(form.get("productId")).toBeNull();
            expect(form.get("ProductId")).toBeNull();
        });
        it("画像の削除を指定するとRemoveImageをtrueで送る", async () => {
            const fetchMock = mockFetch(200, sampleProduct);
            vi.stubGlobal("fetch", fetchMock);

            const request: ProductUpdateRequest = {
                productId: "aaaa-1111",
                name: "テスト商品",
                price: 550,
                categoryId: "cat-1",
                quantity: 20,
                image: null,
                removeImage: true,
            };
            await repository.update(request);

            const form = fetchMock.mock.calls[0][1].body as FormData;
            expect(form.get("RemoveImage")).toBe("true");
            expect(form.get("Image")).toBeNull();
        });
    });

    describe("delete:商品の削除", () => {
        it("商品IDをパスに含めてDELETEし削除された商品を返す", async () => {
            const deleted = { ...sampleProduct, isDeleted: true };
            const fetchMock = mockFetch(200, deleted);
            vi.stubGlobal("fetch", fetchMock);

            const result = await repository.delete("aaaa-1111");

            const [url, options] = fetchMock.mock.calls[0];
            expect(url).toBe("/api/admin/products/aaaa-1111");
            expect(options.method).toBe("DELETE");
            expect(result.isDeleted).toBe(true);
        });
    });

    describe("エラー処理", () => {
        it("404のときApiErrorをスローし詳細を保持する", async () => {
            const fetchMock = mockFetch(404, {
                title: "対象が見つかりません",
                status: 404,
                detail: "指定された商品は存在しません。",
            });
            vi.stubGlobal("fetch", fetchMock);

            try {
                await repository.delete("no-such-id");
                expect.fail("ApiError がスローされるべき");
            } catch (e) {
                expect(e).toBeInstanceOf(ApiError);
                const error = e as ApiError;
                expect(error.status).toBe(404);
                expect(error.isNotFound).toBe(true);
                expect(error.detail).toBe("指定された商品は存在しません。");
            }
        });
    });
});