import type { Employee } from "@/models/responses/employee";
import type { EmployeeAccount } from "@/models/responses/employeeAccount";
import type { EmployeeAccountRegisterRequest } from "@/models/requests/employeeAccountRegisterRequest";

/**
 * 担当者アカウント管理に関するユースケースを提供するサービス
 * UC009（担当者アカウント登録）に対応する。
 */
export interface IEmployeeAccountService {
    /**
     * 担当者アカウント登録画面で選択肢として使用する、アカウント未登録の社員一覧を取得する
     * @returns アカウント未登録の社員一覧
     */
    getEmployeesWithoutAccount(): Promise<Employee[]>;

    /**
     * 担当者アカウントを登録する（UC009）
     * @param request 担当者アカウント登録のリクエスト
     * @returns 登録された担当者アカウント
     */
    register(request: EmployeeAccountRegisterRequest): Promise<EmployeeAccount>;
}