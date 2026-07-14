/**
 * 担当者ログインのリクエスト（バックエンドの LoginRequest に対応）
 */
export interface LoginRequest {
    accountName: string;    // アカウント名
    password: string;       // パスワード
}