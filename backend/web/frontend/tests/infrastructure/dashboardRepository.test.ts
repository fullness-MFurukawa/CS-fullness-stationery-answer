import { describe, it, expect, beforeEach, afterEach, vi } from "vitest";
import { DashboardRepository } from "@/infrastructure/repository/dashboardRepository";
import { HttpClient } from "@/infrastructure/http/httpClient";
import { ApiError } from "@/infrastructure/http/apiError";
import type { DashboardSummary } from "@/models/responses/dashboardSummary";

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

describe("DashboardRepositoryの単体テストドライバ", () => {
    let repository: DashboardRepository;

    const sampleSummary: DashboardSummary = {
        productCount: 13,
        categoryCount: 3,
        orderCount: 4,
        totalSales: 4360,
        statusCounts: [
            { orderStatusId: 1, name: "注文済", count: 1 },
            { orderStatusId: 2, name: "入金済", count: 0 },
            { orderStatusId: 3, name: "配送中", count: 1 },
            { orderStatusId: 4, name: "完了", count: 2 },
        ],
    };

    beforeEach(() => {
        repository = new DashboardRepository(new HttpClient(""));
    });

    afterEach(() => {
        vi.restoreAllMocks();
    });

    describe("getSummary:ダッシュボードの集計値を取得する", () => {
        it("集計APIのURLでGETし、集計結果を返す", async () => {
            const fetchMock = mockFetch(200, sampleSummary);
            vi.stubGlobal("fetch", fetchMock);

            const result = await repository.getSummary();

            const [url, options] = fetchMock.mock.calls[0];
            expect(url).toBe("/api/admin/dashboard/summary");
            expect(options.method).toBe("GET");
            expect(options.credentials).toBe("include");

            expect(result.productCount).toBe(13);
            expect(result.categoryCount).toBe(3);
            expect(result.orderCount).toBe(4);
            expect(result.totalSales).toBe(4360);
        });

        it("ステータス別の件数を返す", async () => {
            const fetchMock = mockFetch(200, sampleSummary);
            vi.stubGlobal("fetch", fetchMock);

            const result = await repository.getSummary();

            expect(result.statusCounts).toHaveLength(4);
            expect(result.statusCounts[0].name).toBe("注文済");
            expect(result.statusCounts[0].count).toBe(1);
            // 該当する注文が無いステータスも0件として含まれる
            expect(result.statusCounts[1].count).toBe(0);
        });
    });

    describe("エラー処理", () => {
        it("未認証など401のときApiErrorをスローする", async () => {
            const fetchMock = mockFetch(401, undefined);
            vi.stubGlobal("fetch", fetchMock);

            try {
                await repository.getSummary();
                expect.fail("ApiError がスローされるべき");
            } catch (e) {
                expect(e).toBeInstanceOf(ApiError);
                expect((e as ApiError).isUnauthorized).toBe(true);
            }
        });
    });
});