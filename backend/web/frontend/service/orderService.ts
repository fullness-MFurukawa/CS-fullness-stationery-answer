import { injectable, inject } from "inversify";
import type { IOrderService, OrderSearchView } from "@/interfaces/service/orderService";
import type { IOrderRepository } from "@/interfaces/repository/orderRepository";
import type { IOrderStatusRepository } from "@/interfaces/repository/orderStatusRepository";
import type { Order } from "@/models/responses/order";
import type { OrderStatus } from "@/models/responses/orderStatus";
import type { OrderStatusUpdateRequest } from "@/models/requests/orderStatusUpdateRequest";
import { TYPES } from "@/di/types";

/**
 * 注文管理に関するユースケースの実装
 * 注文と注文ステータスの2つのリポジトリを組み合わせ、画面が必要とするデータを提供する。
 */
@injectable()
export class OrderService implements IOrderService {
    /**
     * @param orderRepository 注文のリポジトリ
     * @param orderStatusRepository 注文ステータスのリポジトリ
     */
    constructor(
        @inject(TYPES.OrderRepository) private readonly orderRepository: IOrderRepository,
        @inject(TYPES.OrderStatusRepository) private readonly orderStatusRepository: IOrderStatusRepository,
    ) {}

    /**
     * 購入履歴検索画面の初期表示に必要なデータを取得する（UC015）
     * @remarks 注文一覧とステータス一覧は互いに依存しないため、並行して取得する。
     */
    async getSearchView(orderDate?: string, customerAccountName?: string): Promise<OrderSearchView> {
        const [orders, statuses] = await Promise.all([
            this.orderRepository.search(orderDate, customerAccountName),
            this.orderStatusRepository.search(),
        ]);
        return { orders, statuses };
    }

    /**
     * 購入履歴を検索する（UC015）
     */
    async search(orderDate?: string, customerAccountName?: string): Promise<Order[]> {
        return this.orderRepository.search(orderDate, customerAccountName);
    }

    /**
     * ステータス更新の選択肢として使用する注文ステータス一覧を取得する
     */
    async getStatuses(): Promise<OrderStatus[]> {
        return this.orderStatusRepository.search();
    }

    /**
     * 注文ステータスを更新する（UC016）
     */
    async updateStatus(request: OrderStatusUpdateRequest): Promise<Order> {
        return this.orderRepository.updateStatus(request);
    }
}