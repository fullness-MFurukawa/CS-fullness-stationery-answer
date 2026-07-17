"use client";

import { useEffect, useState, useMemo } from "react";
import { toast } from "sonner";
import { container } from "@/di/container";
import { TYPES } from "@/di/types";
import type { IDashboardService } from "@/interfaces/service/dashboardService";
import type { DashboardSummary } from "@/models/responses/dashboardSummary";

/**
 * ダッシュボード（BP001）のデータ取得を担うフック
 * 集計はバックエンド側で行うため、取得した結果をそのまま保持する。
 */
export function useDashboard() {
    const [summary, setSummary] = useState<DashboardSummary | null>(null);
    const [isLoading, setIsLoading] = useState(true);

    const service = useMemo(
        () => container.get<IDashboardService>(TYPES.DashboardService),[],);

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

    return { summary, isLoading };
}