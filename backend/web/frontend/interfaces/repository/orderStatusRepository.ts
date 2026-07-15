import type { OrderStatus } from "@/models/responses/orderStatus";

/**
 * 注文ステータスに関するAPI呼び出しを抽象化するリポジトリ
 * バックエンドの /api/admin/order-statuses エンドポイントに対応する。
 */
export interface IOrderStatusRepository {
    /**
   　* すべての注文ステータスを取得する
   　* @returns 注文ステータスの一覧
   　*/
    search(): Promise<OrderStatus[]>;
}