import { auth } from "@/auth";
import { NextResponse } from "next/server";

/**
 * 管理画面へのアクセスを保護するミドルウェア
 * 未認証の場合はログイン画面へリダイレクトする。
 */
export default auth((req) => {
    const isLoggedIn = !!req.auth;
    const isLoginPage = req.nextUrl.pathname === "/admin/login";

    // 未認証で管理画面へアクセスした場合はログイン画面へ
    if (!isLoggedIn && !isLoginPage) {
        return NextResponse.redirect(new URL("/admin/login", req.nextUrl));
    }

    // 認証済みでログイン画面へアクセスした場合はメニュー画面へ
    if (isLoggedIn && isLoginPage) {
        return NextResponse.redirect(new URL("/admin", req.nextUrl));
    }

    return NextResponse.next();
});

/**
 * ミドルウェアの適用範囲
 * 管理画面（/admin配下）のみを対象とする。
 */
export const config = {
    matcher: ["/admin/:path*"],
};