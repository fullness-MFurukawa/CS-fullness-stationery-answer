import { describe, it, expect, beforeEach, vi } from "vitest";
import { DashboardService } from "@/service/dashboardService";
import type { IDashboardRepository } from "@/interfaces/repository/dashboardRepository";
import type { DashboardSummary } from "@/models/responses/dashboardSummary";

describe("DashboardServiceの単体テストドライバ", () => {
    let dashboardRepository: IDashboardRepository;
    let service: DashboardService;

    const sampleSummary: DashboardSummary = {
        productCount: 13,
        categoryCount: 3,
        orderCount: 4,
        totalSales: 4360,
        statusCounts: [
            { orderStatusId: 1, name: "注文済", count: 1 },
            { orderStatusId: 4, name: "完了", count: 2 },
        ],
    };

    beforeEach(() => {
        dashboardRepository = {
            getSummary: vi.fn().mockResolvedValue(sampleSummary),
        };

        service = new DashboardService(dashboardRepository);
    });

    describe("getSummary:ダッシュボードの集計値を取得する", () => {
        it("リポジトリを呼び出し、集計結果を返す", async () => {
            const result = await service.getSummary();

            expect(dashboardRepository.getSummary).toHaveBeenCalledOnce();
            expect(result.productCount).toBe(13);
            expect(result.totalSales).toBe(4360);
            expect(result.statusCounts).toHaveLength(2);
        });
    });
});