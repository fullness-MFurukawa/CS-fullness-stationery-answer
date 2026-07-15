import { describe, it, expect, beforeEach, afterEach, vi } from "vitest";
import { EmployeeAccountRepository } from "@/infrastructure/repository/employeeAccountRepository";
import { HttpClient } from "@/infrastructure/http/httpClient";
import { ApiError } from "@/infrastructure/http/apiError";
import type { EmployeeAccountRegisterRequest } from "@/models/requests/employeeAccountRegisterRequest";

/**
 * fetch のモックを組み立てるヘルパー
 */
function mockFetch(status: number, body: unknown) {
    const text = body === undefined ? "" : JSON.stringify(body);
    return vi.fn().mockResolvedValue({
        ok: status >= 200 && status < 300,
        status,
        text: () => Promise.resolve(text),
        json: () => Promise.resolve(body),
    }     as Response);
}

describe("EmployeeAccountRepositoryの単体テストドライバ", () => {
    let repository: EmployeeAccountRepository;

    const request: EmployeeAccountRegisterRequest = {
        employeeId: "emp-1",
        accountName: "hanako01",
        password: "Password123",
    };

    beforeEach(() => {
        repository = new EmployeeAccountRepository(new HttpClient(""));
    });

    afterEach(() => {
        vi.restoreAllMocks();
    });

    describe("register:担当者アカウントを登録する", () => {
        it("JSONボディでPOSTし、登録されたアカウントを返す", async () => {
            const fetchMock = mockFetch(201, {
                accountId: "acc-1",
                accountName: "hanako01",
                employeeName: "鈴木花子",
            });
            vi.stubGlobal("fetch", fetchMock);

            const result = await repository.register(request);

            const [url, options] = fetchMock.mock.calls[0];
            expect(url).toBe("/api/admin/employee-accounts");
            expect(options.method).toBe("POST");
            expect(options.headers["Content-Type"]).toBe("application/json");
            expect(JSON.parse(options.body)).toEqual({
                employeeId: "emp-1",
                accountName: "hanako01",
                password: "Password123",
            });

            expect(result.accountId).toBe("acc-1");
            expect(result.accountName).toBe("hanako01");
            expect(result.employeeName).toBe("鈴木花子");
        });

        it("レスポンスにパスワードを含まない", async () => {
            const fetchMock = mockFetch(201, {
                accountId: "acc-1",
                accountName: "hanako01",
                employeeName: "鈴木花子",
            });
            vi.stubGlobal("fetch", fetchMock);

            const result = await repository.register(request);

            expect(result).not.toHaveProperty("password");
        });
    });

    describe("エラー処理", () => {
        it("アカウント名が重複する409のときApiErrorをスローする", async () => {
            const fetchMock = mockFetch(409, {
                title: "既に登録されています",
                status: 409,
                detail: "このアカウント名は既に使用されています。",
            });
            vi.stubGlobal("fetch", fetchMock);

            try {
                await repository.register(request);
                expect.fail("ApiError がスローされるべき");
            } catch (e) {
                expect(e).toBeInstanceOf(ApiError);
                const error = e as ApiError;
                expect(error.status).toBe(409);
                expect(error.isConflict).toBe(true);
                expect(error.detail).toBe("このアカウント名は既に使用されています。");
            }
        });

        it("社員が存在しない404のときApiErrorをスローする", async () => {
            const fetchMock = mockFetch(404, {
                title: "対象が見つかりません",
                status: 404,
                detail: "指定された社員は存在しません。",
            });
            vi.stubGlobal("fetch", fetchMock);

            try {
                await repository.register(request);
                expect.fail("ApiError がスローされるべき");
            } catch (e) {
                expect(e).toBeInstanceOf(ApiError);
                expect((e as ApiError).isNotFound).toBe(true);
            }
        });
    });
});