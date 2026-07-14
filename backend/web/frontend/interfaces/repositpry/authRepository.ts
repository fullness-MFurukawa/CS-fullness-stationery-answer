import type { LoginResult } from "@/models/responses/loginResult";
import type { LoginRequest } from "@/models/requests/loginRequest";

/**
 * 担当者の認証に関するAPI呼び出しを抽象化するリポジトリ
 * バックエンドの /api/admin/auth エンドポイント群に対応する。
 */
export interface IAuthRepository {
    /**
   　* 担当者を認証する（UC017）
   　* 成功時、認証Cookieはブラウザに自動保存される。
   　* @param request ログインのリクエスト
   　* @returns ログインした担当者の情報
   　*/
    login(request: LoginRequest): Promise<LoginResult>;

    /**
   　* ログアウトする（UC018）
   　* 認証Cookieを失効させる。
   　*/
    logout(): Promise<void>;
}