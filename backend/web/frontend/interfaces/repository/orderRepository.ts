import type { Order } from "@/models/responses/order";
import type { OrderStatusUpdateRequest } from "@/models/requests/orderStatusUpdateRequest";

/**
 * 注文に関するAPI呼び出しを抽象化するリポジトリ
 * バックエンドの /api/admin/orders エンドポイント群に対応する。
 */
export interface IOrderRepository {
    /**
     * 購入履歴を検索する（UC015）
     * @param orderDate 購入日（yyyy-MM-dd）。未指定の場合は条件に含めない
     * @param customerAccountName 顧客アカウント名。未指定の場合は条件に含めない
     * @param orderStatusId 注文ステータスID。未指定の場合は条件に含めない
     * @returns 条件に一致する注文の一覧（新しい順）
     */
    search(orderDate?: string,customerAccountName?: string,orderStatusId?: number): Promise<Order[]>;

    /**
     * 注文ステータスを更新する（UC016）
     * @param request 注文ステータス更新のリクエスト
     * @returns ステータスを更新した注文
     */
    updateStatus(request: OrderStatusUpdateRequest): Promise<Order>;
}