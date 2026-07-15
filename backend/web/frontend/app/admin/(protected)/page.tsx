import { auth } from "@/auth";
import { Dashboard } from "@/components/admin/dashboard/dashboard";

/**
 * BP001 メニュー画面（ダッシュボード）
 * URL: /admin
 */
export default async function AdminPage() {
  const session = await auth();

  return <Dashboard employeeName={session?.user?.name ?? ""} />;
}