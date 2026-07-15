import { describe, it, expect, beforeEach, afterEach, vi } from "vitest";
import { OrderStatusRepository } from "@/infrastructure/repository/orderStatusRepository";
import { HttpClient } from "@/infrastructure/http/httpClient";
import { ApiError } from "@/infrastructure/http/apiError";
import type { OrderStatus } from "@/models/responses/orderStatus";

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

describe("OrderStatusRepositoryの単体テストドライバ", () => {
    let repository: OrderStatusRepository;

    beforeEach(() => {
        repository = new OrderStatusRepository(new HttpClient(""));
    });

    afterEach(() => {
        vi.restoreAllMocks();
    });

    describe("search:注文ステータス一覧を取得する", () => {
        it("一覧取得のURLでGETし、全ステータスを返す", async () => {
            const statuses: OrderStatus[] = [
                { orderStatusId: 1, name: "注文済" },
                { orderStatusId: 2, name: "入金済" },
                { orderStatusId: 3, name: "配送中" },
                { orderStatusId: 4, name: "完了" },
            ];
            const fetchMock = mockFetch(200, statuses);
            vi.stubGlobal("fetch", fetchMock);

            const result = await repository.search();

            const [url, options] = fetchMock.mock.calls[0];
            expect(url).toBe("/api/admin/order-statuses");
            expect(options.method).toBe("GET");
            expect(options.credentials).toBe("include");
            expect(result).toHaveLength(4);
            expect(result[0].orderStatusId).toBe(1);
            expect(result[0].name).toBe("注文済");
        });
    });

    describe("エラー処理", () => {
        it("未認証など401のときApiErrorをスローする", async () => {
            const fetchMock = mockFetch(401, undefined);
            vi.stubGlobal("fetch", fetchMock);

            try {
                await repository.search();
                expect.fail("ApiError がスローされるべき");
            } catch (e) {
                expect(e).toBeInstanceOf(ApiError);
                expect((e as ApiError).isUnauthorized).toBe(true);
            }
        });
    });
});