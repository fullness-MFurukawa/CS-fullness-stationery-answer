"use client";

import { useEffect, useState, useMemo } from "react";
import { toast } from "sonner";
import { Package, FolderTree, Receipt, TrendingUp } from "lucide-react";
import { container } from "@/di/container";
import { TYPES } from "@/di/types";
import type { IDashboardService } from "@/interfaces/service/dashboardService";
import type { DashboardSummary } from "@/models/responses/dashboardSummary";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";

/**
 * BP001 メニュー画面（ダッシュボード）の本体
 * 商品・カテゴリ・注文の集計値を表示する。
 * 集計はバックエンド側で行うため、全件を取得しない。
 * @param employeeName ログイン中の担当者名
 */
export function Dashboard({ employeeName }: { employeeName: string }) {
  const [summary, setSummary] = useState<DashboardSummary | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  const service = useMemo(
    () => container.get<IDashboardService>(TYPES.DashboardService),
    [],
  );

  useEffect(() => {
    const load = async () => {
      try {
        const result = await service.getSummary();
        setSummary(result);
      } catch {
        toast.error("集計データの取得に失敗しました");
      } finally {
        setIsLoading(false);
      }
    };
    load();
  }, [service]);

  const stats = [
    {
      title: "登録商品数",
      value: `${summary?.productCount ?? 0} 件`,
      icon: Package,
    },
    {
      title: "カテゴリ数",
      value: `${summary?.categoryCount ?? 0} 件`,
      icon: FolderTree,
    },
    {
      title: "注文件数",
      value: `${summary?.orderCount ?? 0} 件`,
      icon: Receipt,
    },
    {
      title: "売上合計",
      value: `${(summary?.totalSales ?? 0).toLocaleString()} 円`,
      icon: TrendingUp,
    },
  ];

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold">ダッシュボード</h1>
        <p className="mt-1 text-muted-foreground">ようこそ、{employeeName}さん</p>
      </div>

      {isLoading ? (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
          {Array.from({ length: 4 }).map((_, i) => (
            <Skeleton key={i} className="h-28 rounded-xl" />
          ))}
        </div>
      ) : (
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
      )}

      <Card>
        <CardHeader>
          <CardTitle className="text-base">ステータス別の注文</CardTitle>
        </CardHeader>
        <CardContent>
          {isLoading ? (
            <div className="space-y-3">
              {Array.from({ length: 4 }).map((_, i) => (
                <Skeleton key={i} className="h-6" />
              ))}
            </div>
          ) : (
            <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
              {summary?.statusCounts.map((status) => (
                <div
                  key={status.orderStatusId}
                  className="flex items-center justify-between rounded-lg border p-3"
                >
                  <Badge variant="secondary">{status.name}</Badge>
                  <span className="text-sm font-medium">{status.count} 件</span>
                </div>
              ))}
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}