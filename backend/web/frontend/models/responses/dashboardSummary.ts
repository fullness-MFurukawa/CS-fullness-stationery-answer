import type { OrderStatusCount } from "./orderStatusCount";

/**
 * ダッシュボードの集計結果（バックエンドの DashboardSummaryResponse に対応）
 */
export interface DashboardSummary {
    productCount: number;
    categoryCount: number;
    orderCount: number;
    totalSales: number;
    statusCounts: OrderStatusCount[];
}