import  NextAuth from "next-auth";
import Credentials from "next-auth/providers/credentials";
import { AuthRepository } from "@/infrastructure/repository/authRepository";
import { HttpClient } from "@/infrastructure/http/httpClient";
import { ApiError } from "@/infrastructure/http/apiError";

/**
 * NextAuthの設定
 * Credentials Providerで、バックエンドの認証API（UC017）を呼び出す。
 * 取得したアクセストークンはセッション（JWT）に保持し、
 * API呼び出し時に Authorization ヘッダで送信する。
 */
export const { handlers, signIn, signOut, auth } = NextAuth({
    providers: [
        Credentials({
                // ログインフォームの入力項目
                credentials: {
                    accountName: { label: "アカウント名", type: "text" },
                    password: { label: "パスワード", type: "password" },
                },

                /**
                 * 認証処理
                 * サーバー側で実行されるため、Proxyを経由せずバックエンドを直接呼び出す。
                 * @returns 認証に成功した場合はユーザー情報、失敗した場合はnull
                 */
                async authorize(credentials) {
                    const accountName = credentials?.accountName as string;
                    const password = credentials?.password as string;
                    if (!accountName || !password) {
                        return null;
                }

                // サーバー側からはバックエンドを絶対URLで直接呼び出す
                const repository = new AuthRepository(
                    new HttpClient(process.env.API_BASE_URL ?? ""),
                );

                try {
                    const result = await repository.login({ accountName, password });
                    // 認証成功時、NextAuthのユーザー情報として返す値
                    return {
                        id: result.accountName,
                        name: result.employeeName,
                        accountName: result.accountName,
                        accessToken: result.accessToken,
                    };
                } catch (e) {
                    // 認証失敗（401）はログイン失敗として扱う
                    if (e instanceof ApiError && e.isUnauthorized) {
                        return null;
                    }
                    throw e;
                }
            },
        }),
    ],

    session: {
        strategy: "jwt",
        // バックエンドのアクセストークンの有効期限（30分）に合わせる
        maxAge: 30 * 60,
    },

    callbacks: {
        /**
         * JWTコールバック
         * authorizeが返した値を、セッションのJWTへ保持する。
         */
        async jwt({ token, user }) {
            if (user) {
                token.accessToken = (user as { accessToken: string }).accessToken;
                token.accountName = (user as { accountName: string }).accountName;
            }
            return token;
        },

        /**
         * セッションコールバック
         * JWTの内容を、画面から参照できるセッションへ反映する。
         */
        async session({ session, token }) {
            session.accessToken = token.accessToken as string;
            session.user.accountName = token.accountName as string;
            return session;
        },
    },

    pages: {
        // 独自のログイン画面を使う
        signIn: "/login",
    },
});