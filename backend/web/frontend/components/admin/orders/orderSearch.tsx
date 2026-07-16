"use client";

import { useState, useEffect, useMemo, useCallback } from "react";
import { toast } from "sonner";
import { Search, X } from "lucide-react";
import { container } from "@/di/container";
import { TYPES } from "@/di/types";
import type { IOrderService } from "@/interfaces/service/orderService";
import type { Order } from "@/models/responses/order";
import type { OrderStatus } from "@/models/responses/orderStatus";
import { ApiError } from "@/infrastructure/http/apiError";
import { OrderCard } from "./orderCard";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";

/** ステータス絞り込みの「すべて」を表す値 */
const ALL_STATUSES = "all";

/**
 * BP015 購入履歴検索画面の本体
 * 注文を一覧表示し、購入日・顧客アカウント名・注文ステータスによる絞り込みと、
 * 注文ステータスの更新を行う。
 */
export function OrderSearch() {
  const [orders, setOrders] = useState<Order[]>([]);
  const [statuses, setStatuses] = useState<OrderStatus[]>([]);
  const [orderDate, setOrderDate] = useState("");
  const [customerAccountName, setCustomerAccountName] = useState("");
  const [statusFilter, setStatusFilter] = useState<string>(ALL_STATUSES);
  const [isLoading, setIsLoading] = useState(true);

  const service = useMemo(
    () => container.get<IOrderService>(TYPES.OrderService),
    [],
  );

  /**
   * 画面の初期表示に必要なデータを取得する
   */
  const loadInitial = useCallback(async () => {
    setIsLoading(true);
    try {
      const view = await service.getSearchView();
      setOrders(view.orders);
      setStatuses(view.statuses);
    } catch (e) {
      toast.error(
        e instanceof ApiError ? e.message : "データの取得に失敗しました",
      );
    } finally {
      setIsLoading(false);
    }
  }, [service]);

  useEffect(() => {
    loadInitial();
  }, [loadInitial]);

  /**
   * 検索条件で購入履歴を絞り込む
   */
  const handleSearch = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsLoading(true);
    try {
      const result = await service.search(
        orderDate || undefined,
        customerAccountName.trim() || undefined,
        statusFilter === ALL_STATUSES ? undefined : Number(statusFilter),
      );
      setOrders(result);
    } catch (e) {
      toast.error(e instanceof ApiError ? e.message : "検索に失敗しました");
    } finally {
      setIsLoading(false);
    }
  };

  /**
   * 検索条件をクリアして全件を再取得する
   */
  const handleClear = async () => {
    setOrderDate("");
    setCustomerAccountName("");
    setStatusFilter(ALL_STATUSES);
    setIsLoading(true);
    try {
      const result = await service.search();
      setOrders(result);
    } catch (e) {
      toast.error(e instanceof ApiError ? e.message : "検索に失敗しました");
    } finally {
      setIsLoading(false);
    }
  };

  /**
   * 注文ステータスを更新する
   * @param order 対象の注文
   * @param statusId 変更後の注文ステータスID
   */
  const handleStatusChange = async (order: Order, statusId: number) => {
    try {
      const updated = await service.updateStatus({
        orderId: order.orderId,
        orderStatusId: statusId,
      });
      setOrders((prev) =>
        prev.map((o) => (o.orderId === updated.orderId ? updated : o)),
      );
      toast.success(`ステータスを「${updated.statusName}」に更新しました`);
    } catch (e) {
      toast.error(
        e instanceof ApiError ? e.message : "ステータスの更新に失敗しました",
      );
    }
  };

  const hasCondition =
    orderDate !== "" || customerAccountName !== "" || statusFilter !== ALL_STATUSES;

  const selectedStatus = statuses.find(
    (s) => String(s.orderStatusId) === statusFilter,
  );

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold">購入履歴検索</h1>
        <p className="mt-1 text-muted-foreground">
          {isLoading ? "読み込み中..." : `${orders.length}件の注文`}
        </p>
      </div>

      <Card>
        <CardContent className="pt-6">
          <form onSubmit={handleSearch} className="flex flex-wrap items-end gap-4">
            <div className="space-y-2">
              <Label htmlFor="orderDate">購入日</Label>
              <Input
                id="orderDate"
                type="date"
                value={orderDate}
                onChange={(e) => setOrderDate(e.target.value)}
                className="w-44"
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="customerAccountName">顧客アカウント名</Label>
              <Input
                id="customerAccountName"
                placeholder="例: testuser"
                value={customerAccountName}
                onChange={(e) => setCustomerAccountName(e.target.value)}
                className="w-56"
              />
            </div>

            <div className="flex flex-col gap-2">
              <Label htmlFor="statusFilter">注文ステータス</Label>
              <Select
                value={statusFilter}
                onValueChange={(value) => setStatusFilter(value ?? ALL_STATUSES)}
              >
                <SelectTrigger id="statusFilter" className="w-40">
                  <SelectValue>
                    {statusFilter === ALL_STATUSES
                      ? "すべて"
                      : selectedStatus?.name}
                  </SelectValue>
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value={ALL_STATUSES}>すべて</SelectItem>
                  {statuses.map((status) => (
                    <SelectItem
                      key={status.orderStatusId}
                      value={String(status.orderStatusId)}
                    >
                      {status.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div className="flex gap-2">
              <Button type="submit" disabled={isLoading}>
                <Search />
                検索
              </Button>
              {hasCondition && (
                <Button
                  type="button"
                  variant="outline"
                  onClick={handleClear}
                  disabled={isLoading}
                >
                  <X />
                  クリア
                </Button>
              )}
            </div>
          </form>
        </CardContent>
      </Card>

      {isLoading ? (
        <div className="space-y-4">
          {Array.from({ length: 3 }).map((_, i) => (
            <Skeleton key={i} className="h-48 rounded-xl" />
          ))}
        </div>
      ) : orders.length === 0 ? (
        <div className="rounded-lg border border-dashed py-16 text-center text-muted-foreground">
          該当する注文がありません
        </div>
      ) : (
        <div className="space-y-4">
          {orders.map((order) => (
            <OrderCard
              key={order.orderId}
              order={order}
              statuses={statuses}
              onStatusChange={handleStatusChange}
            />
          ))}
        </div>
      )}
    </div>
  );
}