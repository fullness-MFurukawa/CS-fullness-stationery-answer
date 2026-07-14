import type { OrderDetail } from "./orderDetail";

/**
 * 注文（バックエンドの OrderResponse に対応）
 */
export interface Order {
    orderId: string;                // 注文Id(uuid)
    orderDate: string;              // 注文日 ISO 8601（例: "2024-05-12T15:30:00"）
    amountTotal: number;            // 合計金額
    customerAccountName: string;    // 顧客アカウント名 
    customerName: string;           // 顧客名
    statusId: number;               // 注文ステータスId
    statusName: string;             // 注文ステータス名
    paymentMethodName: string;      // 購入方法
    details: OrderDetail[];         // 注文明細
}