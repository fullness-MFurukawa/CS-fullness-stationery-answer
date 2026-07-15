import { describe, it, expect, beforeEach, vi } from "vitest";
import { EmployeeAccountService } from "@/service/employeeAccountService";
import type { IEmployeeRepository } from "@/interfaces/repository/employeeRepository";
import type { IEmployeeAccountRepository } from "@/interfaces/repository/employeeAccountRepository";
import type { Employee } from "@/models/responses/employee";

describe("EmployeeAccountServiceの単体テストドライバ", () => {
    let employeeRepository: IEmployeeRepository;
    let employeeAccountRepository: IEmployeeAccountRepository;
    let service: EmployeeAccountService;

    const sampleEmployees: Employee[] = [
        { employeeId: "emp-1", name: "鈴木花子", nameKana: "スズキハナコ", departmentName: "商品企画部" },
        { employeeId: "emp-2", name: "山本次郎", nameKana: "ヤマモトジロウ", departmentName: "販売管理部" },
    ];

    beforeEach(() => {
        employeeRepository = {
            searchWithoutAccount: vi.fn().mockResolvedValue(sampleEmployees),
        };

        employeeAccountRepository = {
                register: vi.fn().mockResolvedValue({
                accountId: "acc-1",
                accountName: "hanako01",
                employeeName: "鈴木花子",
            }),
        };

        service = new EmployeeAccountService(employeeRepository, employeeAccountRepository);
    });

    describe("getEmployeesWithoutAccount:アカウント未登録の社員一覧を取得する", () => {
        it("社員リポジトリを呼び出し、社員一覧を返す", async () => {
            const result = await service.getEmployeesWithoutAccount();

            expect(employeeRepository.searchWithoutAccount).toHaveBeenCalledOnce();
            expect(result).toHaveLength(2);
            expect(result[0].name).toBe("鈴木花子");
        });
    });

    describe("register:担当者アカウントを登録する", () => {
        it("アカウントリポジトリへリクエストをそのまま渡し、登録されたアカウントを返す", async () => {
            const request = {
                employeeId: "emp-1",
                accountName: "hanako01",
                password: "Password123",
            };

            const result = await service.register(request);

            expect(employeeAccountRepository.register).toHaveBeenCalledWith(request);
            expect(result.accountName).toBe("hanako01");
            expect(result.employeeName).toBe("鈴木花子");
        });
    });
});