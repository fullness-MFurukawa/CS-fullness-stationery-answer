import { injectable, inject } from "inversify";
import type { IEmployeeAccountRepository } from "@/interfaces/repository/employeeAccountRepository";
import type { EmployeeAccount } from "@/models/responses/employeeAccount";
import type { EmployeeAccountRegisterRequest } from "@/models/requests/employeeAccountRegisterRequest";
import { HttpClient } from "@/infrastructure/http/httpClient";
import { TYPES } from "@/di/types";

/**
 * 担当者アカウントに関するAPI呼び出しの実装
 * バックエンドの /api/admin/employee-accounts エンドポイントを呼び出す。
 */
@injectable()
export class EmployeeAccountRepository implements IEmployeeAccountRepository {
    /**
     * @param httpClient API通信の共通クライアント
     */
    constructor(
        @inject(TYPES.HttpClient) private readonly httpClient: HttpClient,
    ) {}

    /**
     * 担当者アカウントを登録する（UC009）
     */
    async register(request: EmployeeAccountRegisterRequest): Promise<EmployeeAccount> {
        return this.httpClient.sendJson<EmployeeAccount>(
            "POST",
            "/api/admin/employee-accounts",
            request,
        );
    }
}