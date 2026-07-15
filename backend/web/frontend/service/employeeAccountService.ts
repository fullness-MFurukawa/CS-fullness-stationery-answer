import { injectable, inject } from "inversify";
import type { IEmployeeAccountService } from "@/interfaces/service/employeeAccountService";
import type { IEmployeeRepository } from "@/interfaces/repository/employeeRepository";
import type { IEmployeeAccountRepository } from "@/interfaces/repository/employeeAccountRepository";
import type { Employee } from "@/models/responses/employee";
import type { EmployeeAccount } from "@/models/responses/employeeAccount";
import type { EmployeeAccountRegisterRequest } from "@/models/requests/employeeAccountRegisterRequest";
import { TYPES } from "@/di/types";

/**
 * 担当者アカウント管理に関するユースケースの実装
 * 社員と担当者アカウントの2つのリポジトリを組み合わせる。
 * UC009（担当者アカウント登録）に対応する。
 */
@injectable()
export class EmployeeAccountService implements IEmployeeAccountService {
    /**
     * @param employeeRepository 社員のリポジトリ
     * @param employeeAccountRepository 担当者アカウントのリポジトリ
     */
    constructor(
        @inject(TYPES.EmployeeRepository) private readonly employeeRepository: IEmployeeRepository,
        @inject(TYPES.EmployeeAccountRepository) private readonly employeeAccountRepository: IEmployeeAccountRepository,
    ) {}

    /**
     * 担当者アカウント登録画面で選択肢として使用する、アカウント未登録の社員一覧を取得する
     */
    async getEmployeesWithoutAccount(): Promise<Employee[]> {
        return this.employeeRepository.searchWithoutAccount();
    }

    /**
     * 担当者アカウントを登録する（UC009）
     */
    async register(request: EmployeeAccountRegisterRequest): Promise<EmployeeAccount> {
        return this.employeeAccountRepository.register(request);
    }
}