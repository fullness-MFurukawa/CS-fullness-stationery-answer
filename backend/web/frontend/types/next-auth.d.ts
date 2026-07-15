import type { DefaultSession } from "next-auth";

declare module "next-auth" {
    /**
     * セッションの型を拡張し、バックエンドのアクセストークンとアカウント名を保持する
     */
    interface Session {
        accessToken: string;
        user: {
            accountName: string;
        } & DefaultSession["user"];
    }
}

declare module "next-auth/jwt" {
    /**
     * JWTの型を拡張し、バックエンドのアクセストークンとアカウント名を保持する
     */
    interface JWT {
        accessToken: string;
        accountName: string;
    }
}