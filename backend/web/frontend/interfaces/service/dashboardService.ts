import type { DashboardSummary } from "@/models/responses/dashboardSummary";

/**
 * ダッシュボードに関するユースケースを提供するサービス
 * メニュー画面で管理業務の状況を表示する。
 */
export interface IDashboardService {
    /**
     * ダッシュボードに表示する集計値を取得する
     * @returns 集計結果
     */
    getSummary(): Promise<DashboardSummary>;
}