import { injectable, inject } from "inversify";
import type { IEmployeeRepository } from "@/interfaces/repository/employeeRepository";
import type { Employee } from "@/models/responses/employee";
import { HttpClient } from "@/infrastructure/http/httpClient";
import { TYPES } from "@/di/types";

/**
 * 社員に関するAPI呼び出しの実装
 * バックエンドの /api/admin/employees エンドポイント群を呼び出す。
 */
@injectable()
export class EmployeeRepository implements IEmployeeRepository {
    /**
     * @param httpClient API通信の共通クライアント
     */
    constructor(
        @inject(TYPES.HttpClient) private readonly httpClient: HttpClient,
    ) {}

    /**
     * アカウントが未登録の社員をすべて取得する
     */
    async searchWithoutAccount(): Promise<Employee[]> {
        return this.httpClient.get<Employee[]>("/api/admin/employees/without-account");
    }
}