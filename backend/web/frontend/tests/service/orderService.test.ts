import { describe, it, expect, beforeEach, vi } from "vitest";
import { OrderService } from "@/service/orderService";
import type { IOrderRepository } from "@/interfaces/repository/orderRepository";
import type { IOrderStatusRepository } from "@/interfaces/repository/orderStatusRepository";
import type { Order } from "@/models/responses/order";
import type { OrderStatus } from "@/models/responses/orderStatus";

describe("OrderServiceの単体テストドライバ", () => {
    let orderRepository: IOrderRepository;
    let orderStatusRepository: IOrderStatusRepository;
    let service: OrderService;

    const sampleOrder: Order = {
        orderId: "order-1",
        orderDate: "2024-05-12T15:30:00",
        amountTotal: 100,
        customerAccountName: "testuser",
        customerName: "テスト顧客",
        statusId: 3,
        statusName: "配送中",
        paymentMethodName: "現金",
        details: [{ productName: "鉛筆(黒)", price: 100, count: 1, subtotal: 100 }],
    };

    const sampleStatuses: OrderStatus[] = [
        { orderStatusId: 1, name: "注文済" },
        { orderStatusId: 2, name: "入金済" },
        { orderStatusId: 3, name: "配送中" },
        { orderStatusId: 4, name: "完了" },
    ];

    beforeEach(() => {
        orderRepository = {
            search: vi.fn().mockResolvedValue([sampleOrder]),
            updateStatus: vi.fn().mockResolvedValue({ ...sampleOrder, statusId: 4, statusName: "完了" }),
        };

        orderStatusRepository = {
            search: vi.fn().mockResolvedValue(sampleStatuses),
        };

        service = new OrderService(orderRepository, orderStatusRepository);
    });

    describe("getSearchView:購入履歴検索画面のデータを取得する", () => {
        it("注文一覧とステータス一覧をまとめて返す", async () => {
            const result = await service.getSearchView();

            expect(result.orders).toHaveLength(1);
            expect(result.orders[0].customerName).toBe("テスト顧客");
            expect(result.statuses).toHaveLength(4);
        });

        it("両方のリポジトリを1回ずつ呼び出す", async () => {
            await service.getSearchView();

            expect(orderRepository.search).toHaveBeenCalledOnce();
            expect(orderStatusRepository.search).toHaveBeenCalledOnce();
        });

        it("検索条件を注文リポジトリへ渡す", async () => {
            await service.getSearchView("2024-05-12", "testuser");

            expect(orderRepository.search).toHaveBeenCalledWith("2024-05-12", "testuser", undefined);
        });
    });

    describe("search:購入履歴を検索する", () => {
        it("注文リポジトリのみを呼び出す", async () => {
            const result = await service.search("2024-05-12");

            expect(orderRepository.search).toHaveBeenCalledWith("2024-05-12", undefined, undefined);
            expect(orderStatusRepository.search).not.toHaveBeenCalled();
            expect(result).toHaveLength(1);
        });
    });

    describe("getStatuses:注文ステータス一覧を取得する", () => {
        it("ステータスリポジトリのみを呼び出す", async () => {
            const result = await service.getStatuses();

            expect(orderStatusRepository.search).toHaveBeenCalledOnce();
            expect(orderRepository.search).not.toHaveBeenCalled();
            expect(result).toHaveLength(4);
        });
    });

    describe("updateStatus:注文ステータスを更新する", () => {
        it("注文リポジトリへリクエストをそのまま渡し、更新後の注文を返す", async () => {
            const request = { orderId: "order-1", orderStatusId: 4 };

            const result = await service.updateStatus(request);

            expect(orderRepository.updateStatus).toHaveBeenCalledWith(request);
            expect(result.statusName).toBe("完了");
        });
    });
});