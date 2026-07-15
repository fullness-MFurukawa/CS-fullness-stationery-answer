import { injectable, inject } from "inversify";
import type { IOrderStatusRepository } from "@/interfaces/repository/orderStatusRepository";
import type { OrderStatus } from "@/models/responses/orderStatus";
import { HttpClient } from "@/infrastructure/http/httpClient";
import { TYPES } from "@/di/types";

/**
 * 注文ステータスに関するAPI呼び出しの実装
 * バックエンドの /api/admin/order-statuses エンドポイントを呼び出す。
 */
@injectable()
export class OrderStatusRepository implements IOrderStatusRepository {
    /**
     * @param httpClient API通信の共通クライアント
     */
    constructor(
        @inject(TYPES.HttpClient) private readonly httpClient: HttpClient,
    ) {}

    /**
     * すべての注文ステータスを取得する
     */
    async search(): Promise<OrderStatus[]> {
        return this.httpClient.get<OrderStatus[]>("/api/admin/order-statuses");
    }
}