import { injectable, inject } from "inversify";
import type { IDashboardService } from "@/interfaces/service/dashboardService";
import type { IDashboardRepository } from "@/interfaces/repository/dashboardRepository";
import type { DashboardSummary } from "@/models/responses/dashboardSummary";
import { TYPES } from "@/di/types";

/**
 * ダッシュボードに関するユースケースの実装
 * 集計はバックエンド側で行うため、取得した結果をそのまま返す。
 */
@injectable()
export class DashboardService implements IDashboardService {
   /**
    * @param dashboardRepository ダッシュボードのリポジトリ
    */
    constructor(
        @inject(TYPES.DashboardRepository) private readonly dashboardRepository: IDashboardRepository,
    ) {}

    /**
     * ダッシュボードに表示する集計値を取得する
     */
    async getSummary(): Promise<DashboardSummary> {
        return this.dashboardRepository.getSummary();
    }
}