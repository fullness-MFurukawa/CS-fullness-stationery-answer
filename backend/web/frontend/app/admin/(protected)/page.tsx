import { auth } from "@/auth";

/**
 * BP001 メニュー画面
 * URL: /admin
 */
export default async function AdminPage() {
  const session = await auth();

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold">メニュー</h1>
        <p className="text-muted-foreground mt-1">
          ようこそ、{session?.user?.name}さん
        </p>
      </div>
    </div>
  );
}