import type { EmployeeAccount } from "@/models/responses/employeeAccount";
import type { EmployeeAccountRegisterRequest } from "@/models/requests/employeeAccountRegisterRequest";

/**
 * 担当者アカウントに関するAPI呼び出しを抽象化するリポジトリ
 * バックエンドの /api/admin/employee-accounts エンドポイントに対応する。
 */
export interface IEmployeeAccountRepository {
    /**
   　* 担当者アカウントを登録する（UC009）
   　* @param request 担当者アカウント登録のリクエスト
   　* @returns 登録された担当者アカウント
   　*/
    register(request: EmployeeAccountRegisterRequest): Promise<EmployeeAccount>;
}