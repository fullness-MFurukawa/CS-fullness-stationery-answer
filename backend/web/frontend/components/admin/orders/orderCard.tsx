"use client";

import { useState } from "react";
import type { Order } from "@/models/responses/order";
import type { OrderStatus } from "@/models/responses/orderStatus";
import { Card, CardContent, CardHeader } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Separator } from "@/components/ui/separator";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";

/**
 * 注文カード
 * 購入履歴検索画面（BP015）で、注文1件を明細とともに表示する。
 * @param order 表示する注文
 * @param statuses 注文ステータスの選択肢
 * @param onStatusChange ステータスが変更されたときの処理
 */
export function OrderCard({
  order,
  statuses,
  onStatusChange,
}: {
  order: Order;
  statuses: OrderStatus[];
  onStatusChange: (order: Order, statusId: number) => Promise<void>;
}) {
  const [isUpdating, setIsUpdating] = useState(false);

  /**
   * 注文ステータスを変更する
   * @param value 選択された注文ステータスID
   */
  const handleChange = async (value: string | null) => {
    if (!value) return;

    const statusId = Number(value);
    if (statusId === order.statusId) return;

    setIsUpdating(true);
    try {
      await onStatusChange(order, statusId);
    } finally {
      setIsUpdating(false);
    }
  };

  const currentStatus = statuses.find((s) => s.orderStatusId === order.statusId);

  return (
    <Card>
      <CardHeader className="pb-3">
        <div className="flex flex-wrap items-start justify-between gap-3">
          <div className="space-y-1">
            <div className="text-sm text-muted-foreground">
              {new Date(order.orderDate).toLocaleString("ja-JP", {
                year: "numeric",
                month: "2-digit",
                day: "2-digit",
                hour: "2-digit",
                minute: "2-digit",
              })}
            </div>
            <div className="font-medium">
              {order.customerName}
              <span className="ml-2 text-sm font-normal text-muted-foreground">
                （{order.customerAccountName}）
              </span>
            </div>
          </div>

          <div className="flex items-center gap-3">
            <div className="text-right">
              <div className="text-xs text-muted-foreground">合計</div>
              <div className="text-lg font-bold">
                {order.amountTotal.toLocaleString()} 円
              </div>
            </div>
            <Select
              value={String(order.statusId)}
              onValueChange={handleChange}
              disabled={isUpdating}
            >
              <SelectTrigger className="w-32">
                <SelectValue>{currentStatus?.name ?? order.statusName}</SelectValue>
              </SelectTrigger>
              <SelectContent>
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
        </div>
      </CardHeader>

      <Separator />

      <CardContent className="pt-4">
        <div className="mb-2 flex items-center justify-between text-xs text-muted-foreground">
          <span>注文明細</span>
          <span>支払い方法: {order.paymentMethodName}</span>
        </div>
        <div className="space-y-2">
          {order.details.map((detail, index) => (
            <div
              key={index}
              className="flex items-center justify-between text-sm"
            >
              <div className="flex items-center gap-2">
                <span>{detail.productName}</span>
                <Badge variant="secondary" className="text-xs">
                  × {detail.count}
                </Badge>
              </div>
              <div className="flex items-center gap-4 text-muted-foreground">
                <span className="text-xs">
                  {detail.price.toLocaleString()} 円
                </span>
                <span className="w-20 text-right font-medium text-foreground">
                  {detail.subtotal.toLocaleString()} 円
                </span>
              </div>
            </div>
          ))}
        </div>
      </CardContent>
    </Card>
  );
}