"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { signOut } from "next-auth/react";
import {
  LayoutDashboard,
  Search,
  PackagePlus,
  FolderPlus,
  Receipt,
  UserPlus,
  LogOut,
  PenTool,
} from "lucide-react";
import {
  Sidebar,
  SidebarContent,
  SidebarFooter,
  SidebarGroup,
  SidebarGroupContent,
  SidebarGroupLabel,
  SidebarHeader,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
} from "@/components/ui/sidebar";

/**
 * サイドバーのメニュー項目
 * 画面設計(BP001)の処理仕様に対応する。
 */
const menuItems = [
  { title: "メニュー", url: "/admin", icon: LayoutDashboard },
  { title: "商品検索", url: "/admin/products", icon: Search },
  { title: "新商品登録", url: "/admin/products/new", icon: PackagePlus },
  { title: "商品カテゴリ登録", url: "/admin/categories/new", icon: FolderPlus },
  { title: "購入履歴検索", url: "/admin/orders", icon: Receipt },
  { title: "担当者アカウント登録", url: "/admin/employee-accounts/new", icon: UserPlus },
];

/**
 * 管理画面のサイドバー
 * @param employeeName ログイン中の担当者名
 */
export function AdminSidebar({ employeeName }: { employeeName: string }) {
  const pathname = usePathname();

  return (
    <Sidebar>
      <SidebarHeader className="border-b">
        <div className="flex items-center gap-2 px-2 py-3">
          <div className="flex size-9 shrink-0 items-center justify-center rounded-lg bg-primary text-primary-foreground">
            <PenTool className="size-5" />
          </div>
          <div className="flex flex-col">
            <span className="text-sm font-semibold">Fullness Stationery</span>
            <span className="text-xs text-muted-foreground">データ管理サービス</span>
          </div>
        </div>
      </SidebarHeader>

      <SidebarContent>
        <SidebarGroup>
          <SidebarGroupLabel>管理機能</SidebarGroupLabel>
          <SidebarGroupContent>
            <SidebarMenu>
              {menuItems.map((item) => (
                <SidebarMenuItem key={item.url}>
                  <SidebarMenuButton
                    render={
                      <Link href={item.url}>
                        <item.icon />
                        <span>{item.title}</span>
                      </Link>
                    }
                    isActive={pathname === item.url}
                  />
                </SidebarMenuItem>
              ))}
            </SidebarMenu>
          </SidebarGroupContent>
        </SidebarGroup>
      </SidebarContent>

      <SidebarFooter className="border-t">
        <SidebarMenu>
          <SidebarMenuItem>
            <div className="px-2 py-1.5 text-sm">
              <div className="text-muted-foreground text-xs">ログイン中</div>
              <div className="font-medium">{employeeName}</div>
            </div>
          </SidebarMenuItem>
          <SidebarMenuItem>
            <SidebarMenuButton
              onClick={() => signOut({ callbackUrl: "/admin/login" })}
            >
              <LogOut />
              <span>ログアウト</span>
            </SidebarMenuButton>
          </SidebarMenuItem>
        </SidebarMenu>
      </SidebarFooter>
    </Sidebar>
  );
}