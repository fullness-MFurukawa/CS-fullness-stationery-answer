import { describe, it, expect, beforeEach, afterEach, vi } from "vitest";
import { AuthRepository } from "@/infrastructure/repository/authRepository";
import { HttpClient } from "@/infrastructure/http/httpClient";
import { ApiError } from "@/infrastructure/http/apiError";
import type { LoginRequest } from "@/models/requests/loginRequest";

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

describe("AuthRepositoryの単体テストドライバ", () => {
    let repository: AuthRepository;

    const request: LoginRequest = {
        accountName: "fullness",
        password: "Password123",
    };

    beforeEach(() => {
        repository = new AuthRepository(new HttpClient(""));
    });

    afterEach(() => {
        vi.restoreAllMocks();
    });

    describe("login:担当者を認証する", () => {
        it("JSONボディでPOSTし、担当者情報とアクセストークンを返す", async () => {
            const fetchMock = mockFetch(200, {
                accountName: "fullness",
                employeeName: "フルネス太郎",
                accessToken: "dummy.jwt.token",
            });
            vi.stubGlobal("fetch", fetchMock);

            const result = await repository.login(request);

            const [url, options] = fetchMock.mock.calls[0];
            expect(url).toBe("/api/admin/auth/login");
            expect(options.method).toBe("POST");
            expect(options.headers["Content-Type"]).toBe("application/json");
            expect(JSON.parse(options.body)).toEqual({
                accountName: "fullness",
                password: "Password123",
            });

            expect(result.accountName).toBe("fullness");
            expect(result.employeeName).toBe("フルネス太郎");
            expect(result.accessToken).toBe("dummy.jwt.token");
        });
    });

    describe("logout:ログアウトする", () => {
        it("ボディなしでPOSTする", async () => {
            const fetchMock = mockFetch(204, undefined);
            vi.stubGlobal("fetch", fetchMock);

            await repository.logout();

            const [url, options] = fetchMock.mock.calls[0];
            expect(url).toBe("/api/admin/auth/logout");
            expect(options.method).toBe("POST");
            expect(options.credentials).toBe("include");
            expect(options.body).toBeUndefined();
        });
    });

    describe("エラー処理", () => {
        it("認証失敗の401のときApiErrorをスローする", async () => {
            // バックエンドの認証失敗は空ボディの401を返す
            const fetchMock = mockFetch(401, undefined);
            vi.stubGlobal("fetch", fetchMock);

            try {
                await repository.login({ accountName: "fullness", password: "wrong" });
                expect.fail("ApiError がスローされるべき");
            } catch (e) {
                expect(e).toBeInstanceOf(ApiError);
                const error = e as ApiError;
                expect(error.status).toBe(401);
                expect(error.isUnauthorized).toBe(true);
            }
        });

        it("入力不備の400のときApiErrorをスローする", async () => {
            const fetchMock = mockFetch(400, {
                title: "入力内容に誤りがあります",
                status: 400,
            });
            vi.stubGlobal("fetch", fetchMock);

            try {
                await repository.login({ accountName: "", password: "" });
                expect.fail("ApiError がスローされるべき");
            } catch (e) {
                expect(e).toBeInstanceOf(ApiError);
                expect((e as ApiError).isValidationError).toBe(true);
            }
        });
    });
});