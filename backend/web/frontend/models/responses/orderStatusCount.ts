/**
 * 注文ステータスごとの注文件数（バックエンドの OrderStatusCountResponse に対応）
 */
export interface OrderStatusCount {
    orderStatusId: number;
    name: string;
    count: number;
}