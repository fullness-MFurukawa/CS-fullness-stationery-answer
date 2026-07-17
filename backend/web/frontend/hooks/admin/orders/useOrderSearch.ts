"use client";

import { useState, useEffect, useMemo, useCallback } from "react";
import { toast } from "sonner";
import { container } from "@/di/container";
import { TYPES } from "@/di/types";
import type { IOrderService } from "@/interfaces/service/orderService";
import type { Order } from "@/models/responses/order";
import type { OrderStatus } from "@/models/responses/orderStatus";
import { ApiError } from "@/infrastructure/http/apiError";

/** 購入履歴の検索条件 */
export type OrderSearchCondition = {
    /** 購入日（yyyy-MM-dd）。未指定の場合はundefined */
    orderDate?: string;
    /** 顧客アカウント名。未指定の場合はundefined */
    customerAccountName?: string;
    /** 注文ステータスID。未指定の場合はundefined */
    orderStatusId?: number;
};

/**
 * 購入履歴検索（BP015）のデータ操作を担うフック
 * 注文と注文ステータスの取得、条件による絞り込み、注文ステータスの更新を提供する。
 */
export function useOrderSearch() {
    const [orders, setOrders] = useState<Order[]>([]);
    const [statuses, setStatuses] = useState<OrderStatus[]>([]);
    const [isLoading, setIsLoading] = useState(true);

    const service = useMemo(() => container.get<IOrderService>(TYPES.OrderService),[],);

    /**
     * 画面の初期表示に必要なデータ（注文の全件と注文ステータスの選択肢）を取得する
     */
    useEffect(() => {
        const load = async () => {
            setIsLoading(true);
            try {
                const view = await service.getSearchView();
                setOrders(view.orders);
                setStatuses(view.statuses);
            } catch (e) {
                toast.error(e instanceof ApiError ? e.message : "データの取得に失敗しました",);
            } finally {
                setIsLoading(false);
            }
        };
        load();
    }, [service]);

    /**
     * 条件を指定して購入履歴を絞り込む
     * @param condition 検索条件。省略した場合は全件を取得する
     */
    const search = useCallback(async (condition: OrderSearchCondition = {}) => {
        setIsLoading(true);
        try {
            const result = await service.search(
            condition.orderDate,
            condition.customerAccountName,
            condition.orderStatusId,
            );
            setOrders(result);
        } catch (e) {
            toast.error(e instanceof ApiError ? e.message : "検索に失敗しました");
        } finally {
            setIsLoading(false);
        }
    },[service],);

    /**
     * 注文ステータスを更新する
     * @param order 対象の注文
     * @param statusId 変更後の注文ステータスID
     */
    const updateStatus = useCallback(
        async (order: Order, statusId: number) => {
            try {
                const updated = await service.updateStatus({
                orderId: order.orderId,
                orderStatusId: statusId,});
                setOrders((prev) =>
                    prev.map((o) => (o.orderId === updated.orderId ? updated : o)),
                );
                toast.success(`ステータスを「${updated.statusName}」に更新しました`);
            } catch (e) {
                toast.error(e instanceof ApiError ? e.message : "ステータスの更新に失敗しました",);
            }
        },[service],);

    return { orders, statuses, isLoading, search, updateStatus };
}