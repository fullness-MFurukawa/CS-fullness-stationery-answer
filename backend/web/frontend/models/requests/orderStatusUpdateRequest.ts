/**
 * 注文ステータス更新のリクエスト（バックエンドの OrderStatusUpdateRequest に対応）
 */
export interface OrderStatusUpdateRequest {
    orderId: string;        // 注文Id(uuid（URLパスに使用）)
    orderStatusId: number;  // 注文ステータス
}