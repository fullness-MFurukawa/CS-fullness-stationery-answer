"use client";

import type { ReactNode } from "react";
import { SidebarInset, SidebarProvider, SidebarTrigger } from "@/components/ui/sidebar";
import { Separator } from "@/components/ui/separator";
import { AdminSidebar } from "./adminSidebar";

/**
 * 管理画面の外枠
 * サイドバーとヘッダーを配置し、各画面の内容を表示する。
 * @param employeeName ログイン中の担当者名
 * @param children 各画面の内容
 */
export function AdminShell({
  employeeName,
  children,
}: {
  employeeName: string;
  children: ReactNode;
}) {
  return (
    <SidebarProvider>
      <AdminSidebar employeeName={employeeName} />
      <SidebarInset>
        <header className="flex h-14 shrink-0 items-center gap-2 border-b px-4">
          <SidebarTrigger className="-ml-1" />
          <Separator orientation="vertical" className="mr-2 h-4" />
          <span className="text-sm font-medium">データ管理サービス</span>
        </header>
        <main className="flex-1 p-6">{children}</main>
      </SidebarInset>
    </SidebarProvider>
  );
}