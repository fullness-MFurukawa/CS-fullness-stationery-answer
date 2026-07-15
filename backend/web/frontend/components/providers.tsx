"use client";

import { SessionProvider } from "next-auth/react";
import type { ReactNode } from "react";

/**
 * アプリ全体で使用するProvider群
 * SessionProviderにより、クライアントコンポーネントからセッション情報を参照できるようにする。
 */
export function Providers({ children }: { children: ReactNode }) {
    return <SessionProvider>{children}</SessionProvider>;
}