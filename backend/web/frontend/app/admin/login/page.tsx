import { LoginForm } from "@/components/admin/login/loginForm";

/**
 * BP002 担当者ログイン画面
 * URL: /admin/login
 */
export default function LoginPage() {
  return (
    <div className="flex min-h-screen items-center justify-center bg-muted/30 p-4">
      <LoginForm />
    </div>
  );
}