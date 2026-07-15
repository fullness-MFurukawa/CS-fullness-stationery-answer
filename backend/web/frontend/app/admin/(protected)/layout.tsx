import { redirect } from "next/navigation";
import { auth } from "@/auth";
import { AdminShell } from "@/components/admin/adminShell";

/**
 * 管理画面の共通レイアウト
 * セッションを確認し、未認証の場合はログイン画面へリダイレクトする。
 * middlewareでも保護しているが、多層防御としてここでも確認する。
 */
export default async function AdminLayout({
  children,
}: {
    children: React.ReactNode;
}) {
    const session = await auth();

    if (!session) {
        redirect("/admin/login");
    }

    return (
        <AdminShell employeeName={session.user?.name ?? ""}>{children}</AdminShell>
    );
}