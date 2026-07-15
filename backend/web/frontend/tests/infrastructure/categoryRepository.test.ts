import { describe, it, expect, beforeEach, afterEach, vi } from "vitest";
import { CategoryRepository } from "@/infrastructure/repository/categoryRepository";
import { HttpClient } from "@/infrastructure/http/httpClient";
import { ApiError } from "@/infrastructure/http/apiError";
import type { Category } from "@/models/responses/category";
import type { CategoryRegisterRequest } from "@/models/requests/categoryRegisterRequest";

/**
 * fetch のモックを組み立てるヘルパー
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

describe("CategoryRepositoryの単体テストドライバ", () => {
    let repository: CategoryRepository;

    const sampleCategory: Category = {
        categoryId: "cat-1",
        name: "文房具",
    };

    beforeEach(() => {
        repository = new CategoryRepository(new HttpClient(""));
    });

    afterEach(() => {
        vi.restoreAllMocks();
    });

    describe("search:商品カテゴリ一覧を取得する", () => {
        it("一覧取得のURLでGETし、カテゴリ一覧を返す", async () => {
            const fetchMock = mockFetch(200, [
                sampleCategory,
                { categoryId: "cat-2", name: "雑貨" },
            ]);
            vi.stubGlobal("fetch", fetchMock);

            const result = await repository.search();

            const [url, options] = fetchMock.mock.calls[0];
            expect(url).toBe("/api/admin/categories");
            expect(options.method).toBe("GET");
            expect(options.credentials).toBe("include");
            expect(result).toHaveLength(2);
            expect(result[0].name).toBe("文房具");
        });

        it("該当0件のとき空配列を返す", async () => {
            const fetchMock = mockFetch(200, []);
            vi.stubGlobal("fetch", fetchMock);

            const result = await repository.search();

            expect(result).toHaveLength(0);
        });
    });

    describe("register:商品カテゴリを登録する", () => {
        it("JSONボディでPOSTし、登録されたカテゴリを返す", async () => {
            const fetchMock = mockFetch(201, { categoryId: "cat-9", name: "画材" });
            vi.stubGlobal("fetch", fetchMock);

            const request: CategoryRegisterRequest = { name: "画材" };
            const result = await repository.register(request);

            const [url, options] = fetchMock.mock.calls[0];
            expect(url).toBe("/api/admin/categories");
            expect(options.method).toBe("POST");
            expect(options.headers["Content-Type"]).toBe("application/json");
            expect(JSON.parse(options.body)).toEqual({ name: "画材" });

            expect(result.categoryId).toBe("cat-9");
            expect(result.name).toBe("画材");
        });
    });

    describe("エラー処理", () => {
        it("400のときApiErrorをスローする", async () => {
            const fetchMock = mockFetch(400, {
                title: "入力内容に誤りがあります",
                status: 400,
            });
            vi.stubGlobal("fetch", fetchMock);

            try {
                await repository.register({ name: "" });
                expect.fail("ApiError がスローされるべき");
            } catch (e) {
                expect(e).toBeInstanceOf(ApiError);
                const error = e as ApiError;
                expect(error.status).toBe(400);
                expect(error.isValidationError).toBe(true);
            }
        });
    });
});