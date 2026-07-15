import { injectable, inject } from "inversify";
import type { IOrderRepository } from "@/interfaces/repository/orderRepository";
import type { Order } from "@/models/responses/order";
import type { OrderStatusUpdateRequest } from "@/models/requests/orderStatusUpdateRequest";
import { HttpClient } from "@/infrastructure/http/httpClient";
import { TYPES } from "@/di/types";

/**
 * 注文に関するAPI呼び出しの実装
 * バックエンドの /api/admin/orders エンドポイント群を呼び出す。
 */
@injectable()
export class OrderRepository implements IOrderRepository {
    /**
     * @param httpClient API通信の共通クライアント
     */
    constructor(
        @inject(TYPES.HttpClient) private readonly httpClient: HttpClient,
    ) {}

    /**
     * 購入履歴を検索する（UC015）
     */
    async search(orderDate?: string, customerAccountName?: string): Promise<Order[]> {
        const params = new URLSearchParams();
        if (orderDate) {
            params.append("orderDate", orderDate);
        }
        if (customerAccountName) {
            params.append("customerAccountName", customerAccountName);
        }
        const query = params.toString();
        const path = query ? `/api/admin/orders?${query}` : "/api/admin/orders";
        return this.httpClient.get<Order[]>(path);
    }

    /**
     * 注文ステータスを更新する（UC016）
     */
    async updateStatus(request: OrderStatusUpdateRequest): Promise<Order> {
        return this.httpClient.sendJson<Order>(
            "PUT",
            `/api/admin/orders/${request.orderId}/status`,
            { orderStatusId: request.orderStatusId },
        );
    }
}