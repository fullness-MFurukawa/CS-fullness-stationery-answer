"use client";

import { useEffect, useState, useMemo } from "react";
import { toast } from "sonner";
import { container } from "@/di/container";
import { TYPES } from "@/di/types";
import type { IEmployeeAccountService } from "@/interfaces/service/employeeAccountService";
import type { Employee } from "@/models/responses/employee";
import type { EmployeeAccountRegisterRequest } from "@/models/requests/employeeAccountRegisterRequest";
import { ApiError } from "@/infrastructure/http/apiError";

/**
 * 担当者アカウント登録の結果
 * 成功・失敗に加え、アカウント名の重複（409）を区別して伝える。
 */
export type RegisterResult =
    |   { ok: true }
    |   { ok: false; conflict: boolean };

/**
 * 担当者アカウント登録（BP003）のデータ操作を担うフック
 * アカウント未登録の社員一覧の取得と、アカウントの登録を提供する。
 */
export function useEmployeeAccountRegister() {
    const [employees, setEmployees] = useState<Employee[]>([]);
    const [isLoading, setIsLoading] = useState(true);

    const service = useMemo(() => container.get<IEmployeeAccountService>(TYPES.EmployeeAccountService),[],);

    /**
     * アカウント未登録の社員一覧を取得する
     */
    useEffect(() => {
        const load = async () => {
            try {
                const result = await service.getEmployeesWithoutAccount();
                setEmployees(result);
            } catch (e) {
                toast.error(e instanceof ApiError ? e.message : "社員一覧の取得に失敗しました",);
            } finally {
                setIsLoading(false);
            }
        };
        load();
    }, [service]);

    /**
     * 担当者アカウントを登録する
     * 登録に成功した場合、その社員を選択肢から除外する。
     * @param request 担当者アカウント登録のリクエスト
     * @returns 登録の結果。アカウント名が重複した場合は conflict を true とする
     */
    const register = async (request: EmployeeAccountRegisterRequest,): Promise<RegisterResult> => {
            try {
                const account = await service.register(request);
                toast.success(`「${account.employeeName}」のアカウントを登録しました`);

                // 登録した社員は選択肢から除外する
                setEmployees((prev) =>
                    prev.filter((e) => e.employeeId !== request.employeeId),
                );
                return { ok: true };
            } catch (e) {
                // アカウント名の重複は、呼び出し側が入力項目のエラーとして扱う
                if (e instanceof ApiError && e.isConflict) {
                    return { ok: false, conflict: true };
            }
                toast.error(e instanceof ApiError ? e.message : "登録に失敗しました");
                return { ok: false, conflict: false };
            }
        };

    return { employees, isLoading, register };
}