import { describe, it, expect, beforeEach, afterEach, vi } from "vitest";
import { OrderRepository } from "@/infrastructure/repository/orderRepository";
import { HttpClient } from "@/infrastructure/http/httpClient";
import { ApiError } from "@/infrastructure/http/apiError";
import type { Order } from "@/models/responses/order";
import type { OrderStatusUpdateRequest } from "@/models/requests/orderStatusUpdateRequest";

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

describe("OrderRepositoryの単体テストドライバ", () => {
    let repository: OrderRepository;

    const sampleOrder: Order = {
        orderId: "order-1",
        orderDate: "2024-05-12T15:30:00",
        amountTotal: 100,
        customerAccountName: "testuser",
        customerName: "テスト顧客",
        statusId: 3,
        statusName: "配送中",
        paymentMethodName: "現金",
        details: [
            { productName: "鉛筆(黒)", price: 100, count: 1, subtotal: 100 },
        ],
    };

    beforeEach(() => {
        repository = new OrderRepository(new HttpClient(""));
    });

    afterEach(() => {
        vi.restoreAllMocks();
    });

    describe("search:購入履歴を検索する", () => {
        it("条件未指定のとき全件取得のURLでGETする", async () => {
            const fetchMock = mockFetch(200, [sampleOrder]);
            vi.stubGlobal("fetch", fetchMock);

            const result = await repository.search();

            const [url, options] = fetchMock.mock.calls[0];
            expect(url).toBe("/api/admin/orders");
            expect(options.method).toBe("GET");
            expect(result).toHaveLength(1);
            expect(result[0].details).toHaveLength(1);
        });

        it("購入日を指定するとクエリに含める", async () => {
            const fetchMock = mockFetch(200, []);
            vi.stubGlobal("fetch", fetchMock);

            await repository.search("2024-05-12");

            const [url] = fetchMock.mock.calls[0];
            expect(url).toBe("/api/admin/orders?orderDate=2024-05-12");
        });

        it("購入日と顧客アカウント名の両方を指定するとクエリに含める", async () => {
            const fetchMock = mockFetch(200, []);
            vi.stubGlobal("fetch", fetchMock);

            await repository.search("2024-05-12", "testuser");

            const [url] = fetchMock.mock.calls[0];
            expect(url).toBe("/api/admin/orders?orderDate=2024-05-12&customerAccountName=testuser");
        });
    });

    describe("updateStatus:注文ステータスを更新する", () => {
        it("注文IDをパスに含め、ステータスIDを本文でPUTする", async () => {
            const updated = { ...sampleOrder, statusId: 4, statusName: "完了" };
            const fetchMock = mockFetch(200, updated);
            vi.stubGlobal("fetch", fetchMock);

            const request: OrderStatusUpdateRequest = { orderId: "order-1", orderStatusId: 4 };
            const result = await repository.updateStatus(request);

            const [url, options] = fetchMock.mock.calls[0];
            expect(url).toBe("/api/admin/orders/order-1/status");
            expect(options.method).toBe("PUT");

            // 本文には orderStatusId のみを含め、orderId は含めない
            const body = JSON.parse(options.body);
            expect(body).toEqual({ orderStatusId: 4 });
            expect(body.orderId).toBeUndefined();

            expect(result.statusName).toBe("完了");
        });
    });

    describe("エラー処理", () => {
        it("存在しないステータスなど404のときApiErrorをスローする", async () => {
            const fetchMock = mockFetch(404, {
                title: "対象が見つかりません",
                status: 404,
                detail: "指定された注文ステータスは存在しません。",
            });
            vi.stubGlobal("fetch", fetchMock);

            try {
                await repository.updateStatus({ orderId: "order-1", orderStatusId: 99 });
                expect.fail("ApiError がスローされるべき");
            } catch (e) {
                expect(e).toBeInstanceOf(ApiError);
                expect((e as ApiError).isNotFound).toBe(true);
            }
        });
    });
});