import type { Order } from "@/models/responses/order";
import type { OrderStatus } from "@/models/responses/orderStatus";
import type { OrderStatusUpdateRequest } from "@/models/requests/orderStatusUpdateRequest";

/**
 * 購入履歴検索画面が必要とするデータ
 * 注文一覧と、ステータス更新用のステータス一覧をまとめて返す。
 */
export interface OrderSearchView {
    orders: Order[];
    statuses: OrderStatus[];
}

/**
 * 注文管理に関するユースケースを提供するサービス
 * UC015（購入履歴検索）・UC016（注文ステータス更新）に対応する。
 */
export interface IOrderService {
    /**
     * 購入履歴検索画面の初期表示に必要なデータを取得する（UC015）
     * 注文一覧と、ステータス更新の選択肢となるステータス一覧を同時に取得する。
     * @param orderDate 購入日（yyyy-MM-dd）。未指定の場合は条件に含めない
     * @param customerAccountName 顧客アカウント名。未指定の場合は条件に含めない
     * @returns 注文一覧とステータス一覧
     */
    getSearchView(orderDate?: string, customerAccountName?: string): Promise<OrderSearchView>;

    /**
     * 購入履歴を検索する（UC015）
     * 検索条件の変更時など、注文一覧のみを再取得する場合に使用する。
     * @param orderDate 購入日（yyyy-MM-dd）。未指定の場合は条件に含めない
     * @param customerAccountName 顧客アカウント名。未指定の場合は条件に含めない
     * @returns 条件に一致する注文の一覧（新しい順）
     */
    search(orderDate?: string, customerAccountName?: string): Promise<Order[]>;

    /**
     * ステータス更新の選択肢として使用する注文ステータス一覧を取得する
     * @returns 注文ステータスの一覧
     */
    getStatuses(): Promise<OrderStatus[]>;

    /**
     * 注文ステータスを更新する（UC016）
     * @param request 注文ステータス更新のリクエスト
     * @returns ステータスを更新した注文
     */
    updateStatus(request: OrderStatusUpdateRequest): Promise<Order>;
}