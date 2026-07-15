"use client";

import { useEffect, useState, useMemo } from "react";
import { toast } from "sonner";
import { Package, FolderTree, Receipt, TrendingUp } from "lucide-react";
import { container } from "@/di/container";
import { TYPES } from "@/di/types";
import type { IProductService } from "@/interfaces/service/productService";
import type { IOrderService } from "@/interfaces/service/orderService";
import type { Order } from "@/models/responses/order";
import type { OrderStatus } from "@/models/responses/orderStatus";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";

/**
 * ダッシュボードの集計結果
 */
type DashboardData = {
  productCount: number;
  categoryCount: number;
  orders: Order[];
  statuses: OrderStatus[];
};

/**
 * BP001 メニュー画面（ダッシュボード）の本体
 * 商品・カテゴリ・注文の状況を集計して表示する。
 * @param employeeName ログイン中の担当者名
 */
export function Dashboard({ employeeName }: { employeeName: string }) {
  const [data, setData] = useState<DashboardData | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  const productService = useMemo(
    () => container.get<IProductService>(TYPES.ProductService),
    [],
  );
  const orderService = useMemo(
    () => container.get<IOrderService>(TYPES.OrderService),
    [],
  );

  useEffect(() => {
    const load = async () => {
      try {
        // 商品・カテゴリと、注文・ステータスを並行して取得する
        const [productView, orderView] = await Promise.all([
          productService.getSearchView(),
          orderService.getSearchView(),
        ]);
        setData({
          productCount: productView.products.length,
          categoryCount: productView.categories.length,
          orders: orderView.orders,
          statuses: orderView.statuses,
        });
      } catch {
        toast.error("データの取得に失敗しました");
      } finally {
        setIsLoading(false);
      }
    };
    load();
  }, [productService, orderService]);

  if (isLoading) {
    return (
      <div className="space-y-6">
        <Skeleton className="h-9 w-64" />
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
          {Array.from({ length: 4 }).map((_, i) => (
            <Skeleton key={i} className="h-28 rounded-xl" />
          ))}
        </div>
      </div>
    );
  }

  const totalSales = data?.orders.reduce((sum, o) => sum + o.amountTotal, 0) ?? 0;

  const stats = [
    {
      title: "登録商品数",
      value: `${data?.productCount ?? 0} 件`,
      icon: Package,
    },
    {
      title: "カテゴリ数",
      value: `${data?.categoryCount ?? 0} 件`,
      icon: FolderTree,
    },
    {
      title: "注文件数",
      value: `${data?.orders.length ?? 0} 件`,
      icon: Receipt,
    },
    {
      title: "売上合計",
      value: `${totalSales.toLocaleString()} 円`,
      icon: TrendingUp,
    },
  ];

  // ステータスごとの注文件数を集計する
  const statusCounts = (data?.statuses ?? []).map((status) => ({
    ...status,
    count:
      data?.orders.filter((o) => o.statusId === status.orderStatusId).length ?? 0,
  }));

  // 最近の注文（一覧は新しい順で返るため先頭から5件）
  const recentOrders = (data?.orders ?? []).slice(0, 5);

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold">ダッシュボード</h1>
        <p className="mt-1 text-muted-foreground">
          ようこそ、{employeeName}さん
        </p>
      </div>

      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        {stats.map((stat) => (
          <Card key={stat.title}>
            <CardHeader className="flex flex-row items-center justify-between pb-2">
              <CardTitle className="text-sm font-medium text-muted-foreground">
                {stat.title}
              </CardTitle>
              <stat.icon className="size-4 text-muted-foreground" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{stat.value}</div>
            </CardContent>
          </Card>
        ))}
      </div>

      <div className="grid gap-4 lg:grid-cols-3">
        <Card>
          <CardHeader>
            <CardTitle className="text-base">ステータス別の注文</CardTitle>
          </CardHeader>
          <CardContent className="space-y-3">
            {statusCounts.map((status) => (
              <div
                key={status.orderStatusId}
                className="flex items-center justify-between"
              >
                <Badge variant="secondary">{status.name}</Badge>
                <span className="text-sm font-medium">{status.count} 件</span>
              </div>
            ))}
          </CardContent>
        </Card>

        <Card className="lg:col-span-2">
          <CardHeader>
            <CardTitle className="text-base">最近の注文</CardTitle>
          </CardHeader>
          <CardContent>
            {recentOrders.length === 0 ? (
              <p className="text-sm text-muted-foreground">注文がありません</p>
            ) : (
              <div className="space-y-3">
                {recentOrders.map((order) => (
                  <div
                    key={order.orderId}
                    className="flex items-center justify-between border-b pb-3 last:border-0 last:pb-0"
                  >
                    <div className="min-w-0">
                      <div className="truncate text-sm font-medium">
                        {order.customerName}
                      </div>
                      <div className="text-xs text-muted-foreground">
                        {new Date(order.orderDate).toLocaleDateString("ja-JP")}
                      </div>
                    </div>
                    <div className="flex items-center gap-3">
                      <Badge variant="secondary">{order.statusName}</Badge>
                      <span className="text-sm font-medium">
                        {order.amountTotal.toLocaleString()} 円
                      </span>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </CardContent>
        </Card>
      </div>
    </div>
  );
}