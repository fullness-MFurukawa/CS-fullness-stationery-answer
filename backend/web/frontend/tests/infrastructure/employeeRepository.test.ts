import { describe, it, expect, beforeEach, afterEach, vi } from "vitest";
import { EmployeeRepository } from "@/infrastructure/repository/employeeRepository";
import { HttpClient } from "@/infrastructure/http/httpClient";
import { ApiError } from "@/infrastructure/http/apiError";
import type { Employee } from "@/models/responses/employee";

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
    } as Response);
}

describe("EmployeeRepositoryの単体テストドライバ", () => {
    let repository: EmployeeRepository;

    beforeEach(() => {
        repository = new EmployeeRepository(new HttpClient(""));
    });

    afterEach(() => {
        vi.restoreAllMocks();
    });

    describe("searchWithoutAccount:アカウント未登録の社員一覧を取得する", () => {
        it("専用のURLでGETし、社員一覧を返す", async () => {
            const employees: Employee[] = [
                { employeeId: "emp-1", name: "鈴木花子", nameKana: "スズキハナコ", departmentName: "商品企画部" },
                { employeeId: "emp-2", name: "山本次郎", nameKana: "ヤマモトジロウ", departmentName: "販売管理部" },
            ];
            const fetchMock = mockFetch(200, employees);
            vi.stubGlobal("fetch", fetchMock);

            const result = await repository.searchWithoutAccount();

            const [url, options] = fetchMock.mock.calls[0];
            expect(url).toBe("/api/admin/employees/without-account");
            expect(options.method).toBe("GET");
            expect(options.credentials).toBe("include");
            expect(result).toHaveLength(2);
            expect(result[0].name).toBe("鈴木花子");
            expect(result[0].departmentName).toBe("商品企画部");
        });

        it("該当0件のとき空配列を返す", async () => {
            const fetchMock = mockFetch(200, []);
            vi.stubGlobal("fetch", fetchMock);

            const result = await repository.searchWithoutAccount();

            expect(result).toHaveLength(0);
        });
    });

    describe("エラー処理", () => {
        it("未認証など401のときApiErrorをスローする", async () => {
            const fetchMock = mockFetch(401, undefined);
            vi.stubGlobal("fetch", fetchMock);

            try {
                await repository.searchWithoutAccount();
                expect.fail("ApiError がスローされるべき");
            } catch (e) {
                expect(e).toBeInstanceOf(ApiError);
                expect((e as ApiError).isUnauthorized).toBe(true);
            }
        });
    });
});