import type { DashboardSummary } from "@/models/responses/dashboardSummary";

/**
 * ダッシュボードに関するAPI呼び出しを抽象化するリポジトリ
 * バックエンドの /api/admin/dashboard エンドポイントに対応する。
 */
export interface IDashboardRepository {
    /**
     * ダッシュボードに表示する集計値を取得する
     * @returns 商品数・カテゴリ数・注文件数・売上合計・ステータス別の注文件数
     */
    getSummary(): Promise<DashboardSummary>;
}