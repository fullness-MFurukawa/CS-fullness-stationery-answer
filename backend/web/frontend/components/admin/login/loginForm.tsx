"use client";

import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { signIn } from "next-auth/react";
import { useRouter } from "next/navigation";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { PenTool } from "lucide-react";

/**
 * ログインフォームの入力値スキーマ
 * 画面設計(BP002)のバリデーション仕様に対応する。
 */
const loginSchema = z.object({
  accountName: z
    .string()
    .min(1, "アカウント名を入力してください")
    .min(5, "アカウント名は5〜20文字で入力してください")
    .max(20, "アカウント名は5〜20文字で入力してください")
    .regex(/^[a-zA-Z0-9]+$/, "アカウント名は半角英数字で入力してください"),
  password: z
    .string()
    .min(1, "パスワードを入力してください")
    .min(5, "パスワードは5〜20文字で入力してください")
    .max(20, "パスワードは5〜20文字で入力してください")
    .regex(/^[a-zA-Z0-9]+$/, "パスワードは半角英数字で入力してください"),
});

type LoginFormValues = z.infer<typeof loginSchema>;

/**
 * BP002 担当者ログインフォーム
 * アカウント名とパスワードで認証し、成功時はメニュー画面へ遷移する。
 */
export function LoginForm() {
  const router = useRouter();

  const {
    register,
    handleSubmit,
    setError,
    formState: { errors, isSubmitting },
  } = useForm<LoginFormValues>({
    resolver: zodResolver(loginSchema),
    defaultValues: { accountName: "", password: "" },
  });

  /**
   * ログイン処理
   * 認証に成功した場合はメニュー画面へ遷移し、失敗した場合はエラーを表示する。
   */
  const onSubmit = async (values: LoginFormValues) => {
    const result = await signIn("credentials", {
      accountName: values.accountName,
      password: values.password,
      redirect: false,
    });

    if (result?.error) {
      // 認証失敗。アカウント名の誤りとパスワードの誤りは区別しない
      setError("root", {
        message: "アカウント名またはパスワードが正しくありません",
      });
      return;
    }

    router.push("/admin");
    router.refresh();
  };

  return (
    <Card className="w-full max-w-md">
      <CardHeader className="space-y-2 text-center">
        <div className="mx-auto flex size-12 items-center justify-center rounded-xl bg-primary text-primary-foreground">
          <PenTool className="size-6" />
        </div>
        <CardTitle className="text-xl">Fullness Stationery</CardTitle>
        <CardDescription>データ管理サービス</CardDescription>
      </CardHeader>

      <CardContent>
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          {errors.root && (
            <Alert variant="destructive">
              <AlertDescription>{errors.root.message}</AlertDescription>
            </Alert>
          )}

          <div className="space-y-2">
            <Label htmlFor="accountName">アカウント名</Label>
            <Input
              id="accountName"
              placeholder="アカウント名"
              autoComplete="username"
              {...register("accountName")}
            />
            {errors.accountName && (
              <p className="text-sm text-destructive">
                {errors.accountName.message}
              </p>
            )}
          </div>

          <div className="space-y-2">
            <Label htmlFor="password">パスワード</Label>
            <Input
              id="password"
              type="password"
              placeholder="パスワード"
              autoComplete="current-password"
              {...register("password")}
            />
            {errors.password && (
              <p className="text-sm text-destructive">
                {errors.password.message}
              </p>
            )}
          </div>

          <Button type="submit" className="w-full" disabled={isSubmitting}>
            {isSubmitting ? "ログイン中..." : "ログイン"}
          </Button>
        </form>
      </CardContent>
    </Card>
  );
}