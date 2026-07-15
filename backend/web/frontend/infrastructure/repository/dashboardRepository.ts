import { injectable, inject } from "inversify";
import type { IDashboardRepository } from "@/interfaces/repository/dashboardRepository";
import type { DashboardSummary } from "@/models/responses/dashboardSummary";
import { HttpClient } from "@/infrastructure/http/httpClient";
import { TYPES } from "@/di/types";

/**
 * ダッシュボードに関するAPI呼び出しの実装
 * バックエンドの /api/admin/dashboard エンドポイントを呼び出す。
 */
@injectable()
export class DashboardRepository implements IDashboardRepository {
    /**
     * @param httpClient API通信の共通クライアント
     */
    constructor(
        @inject(TYPES.HttpClient) private readonly httpClient: HttpClient,
    ) {}

    /**
     * ダッシュボードに表示する集計値を取得する
     */
    async getSummary(): Promise<DashboardSummary> {
        return this.httpClient.get<DashboardSummary>("/api/admin/dashboard/summary");
    }
}