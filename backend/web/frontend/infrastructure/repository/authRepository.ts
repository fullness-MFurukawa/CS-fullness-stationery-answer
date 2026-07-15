import { injectable, inject } from "inversify";
import type { IAuthRepository } from "@/interfaces/repository/authRepository";
import type { LoginResult } from "@/models/responses/loginResult";
import type { LoginRequest } from "@/models/requests/loginRequest";
import { HttpClient } from "@/infrastructure/http/httpClient";
import { TYPES } from "@/di/types";

/**
 * 担当者の認証に関するAPI呼び出しの実装
 * バックエンドの /api/admin/auth エンドポイント群を呼び出す。
 */
@injectable()
export class AuthRepository implements IAuthRepository {
    /**
     * @param httpClient API通信の共通クライアント
     */
    constructor(
        @inject(TYPES.HttpClient) private readonly httpClient: HttpClient,
    ) {}

    /**
     * 担当者を認証する（UC017）
     * 成功時、アクセストークンをレスポンスボディで受け取る。
     */
    async login(request: LoginRequest): Promise<LoginResult> {
        return this.httpClient.sendJson<LoginResult>("POST", "/api/admin/auth/login", request);
    }

    /**
     * ログアウトする（UC018）
     * 認証Cookieを失効させる。
     */
    async logout(): Promise<void> {
        return this.httpClient.sendEmpty("POST", "/api/admin/auth/logout");
     }
}