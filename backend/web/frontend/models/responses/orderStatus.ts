/**
 * 注文ステータス（バックエンドの OrderStatusResponse に対応）
 */
export interface OrderStatus {
    orderStatusId: number;  // 注文ステータスId
    name: string;           // 注文ステータス名
}