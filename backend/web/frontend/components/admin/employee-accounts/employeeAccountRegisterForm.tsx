"use client";

import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { useRouter } from "next/navigation";
import { useEmployeeAccountRegister } from "@/hooks/admin/employee-accounts/useEmployeeAccountRegister";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/components/ui/alert-dialog";
import { Skeleton } from "@/components/ui/skeleton";

/**
 * 担当者アカウント登録の入力値スキーマ
 * 画面設計(BP003)のバリデーション仕様に対応する。
 */
const accountSchema = z.object({
  employeeId: z.string().min(1, "社員を選択してください"),
  accountName: z
    .string()
    .trim()
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

type AccountFormValues = z.infer<typeof accountSchema>;

/** フォームの初期値 */
const defaultValues: AccountFormValues = {
  employeeId: "",
  accountName: "",
  password: "",
};

/**
 * BP003 担当者アカウント登録フォーム
 * アカウント未登録の社員に対して、ログイン用のアカウントを登録する。
 */
export function EmployeeAccountRegisterForm() {
  const router = useRouter();
  const { employees, isLoading, register: registerAccount } =
    useEmployeeAccountRegister();

  const [isConfirmOpen, setIsConfirmOpen] = useState(false);
  const [isRegistering, setIsRegistering] = useState(false);
  const [confirmedValues, setConfirmedValues] = useState<AccountFormValues | null>(null);

  const {
    register,
    handleSubmit,
    setValue,
    watch,
    setError,
    clearErrors,
    reset,
    formState: { errors },
  } = useForm<AccountFormValues>({
    resolver: zodResolver(accountSchema),
    defaultValues,
  });

  const employeeId = watch("employeeId");

  /**
   * 入力値の検証を通過したら確認ダイアログを表示する
   * @param values 検証済みの入力値
   */
  const onSubmit = (values: AccountFormValues) => {
    setConfirmedValues(values);
    setIsConfirmOpen(true);
  };

  /**
   * 担当者アカウントを登録する
   */
  const handleRegister = async () => {
    if (!confirmedValues) return;

    setIsRegistering(true);
    const result = await registerAccount(confirmedValues);
    setIsRegistering(false);

    if (result.ok) {
      setIsConfirmOpen(false);
      setConfirmedValues(null);
      reset(defaultValues);
      clearErrors();
      return;
    }

    setIsConfirmOpen(false);
    if (result.conflict) {
      // アカウント名の重複は入力項目のエラーとして表示する
      setError("accountName", {
        message: "このアカウント名は既に使用されています",
      });
    }
  };

  const selectedEmployee = employees.find((e) => e.employeeId === employeeId);
  const confirmedEmployee = employees.find(
    (e) => e.employeeId === confirmedValues?.employeeId,
  );

  if (isLoading) {
    return (
      <Card>
        <CardContent className="space-y-6 pt-6">
          <Skeleton className="h-10" />
          <Skeleton className="h-10" />
          <Skeleton className="h-10" />
        </CardContent>
      </Card>
    );
  }

  if (employees.length === 0) {
    return (
      <Card>
        <CardContent className="py-12 text-center text-muted-foreground">
          アカウント未登録の社員がいません
        </CardContent>
      </Card>
    );
  }

  return (
    <>
      <Card>
        <CardContent className="pt-6">
          <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
            <div className="flex flex-col gap-2">
              <Label htmlFor="employeeId">
                社員 <span className="text-destructive">*</span>
              </Label>
              <Select
                value={employeeId}
                onValueChange={(value) => {
                  setValue("employeeId", value ?? "");
                  if (value) {
                    clearErrors("employeeId");
                  }
                }}
              >
                <SelectTrigger id="employeeId">
                  <SelectValue>
                    {selectedEmployee
                      ? `${selectedEmployee.name}（${selectedEmployee.departmentName}）`
                      : "社員を選択してください"}
                  </SelectValue>
                </SelectTrigger>
                <SelectContent>
                  {employees.map((employee) => (
                    <SelectItem key={employee.employeeId} value={employee.employeeId}>
                      {employee.name}（{employee.departmentName}）
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
              {errors.employeeId && (
                <p className="text-sm text-destructive">
                  {errors.employeeId.message}
                </p>
              )}
            </div>

            <div className="space-y-2">
              <Label htmlFor="accountName">
                アカウント名 <span className="text-destructive">*</span>
              </Label>
              <Input
                id="accountName"
                placeholder="例: hanako01"
                autoComplete="off"
                {...register("accountName")}
              />
              <p className="text-xs text-muted-foreground">半角英数字で5〜20文字</p>
              {errors.accountName && (
                <p className="text-sm text-destructive">
                  {errors.accountName.message}
                </p>
              )}
            </div>

            <div className="space-y-2">
              <Label htmlFor="password">
                パスワード <span className="text-destructive">*</span>
              </Label>
              <Input
                id="password"
                type="password"
                autoComplete="new-password"
                {...register("password")}
              />
              <p className="text-xs text-muted-foreground">半角英数字で5〜20文字</p>
              {errors.password && (
                <p className="text-sm text-destructive">
                  {errors.password.message}
                </p>
              )}
            </div>

            <div className="flex gap-2">
              <Button type="submit">登録する</Button>
              <Button
                type="button"
                variant="outline"
                onClick={() => router.push("/admin")}
              >
                キャンセル
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>

      <AlertDialog open={isConfirmOpen} onOpenChange={setIsConfirmOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>この内容で登録しますか？</AlertDialogTitle>
            <AlertDialogDescription>
              入力内容を確認してください。
            </AlertDialogDescription>
          </AlertDialogHeader>

          <div className="space-y-2 rounded-lg border p-3 text-sm">
            <div>
              <span className="text-muted-foreground">社員: </span>
              <span className="font-medium">{confirmedEmployee?.name}</span>
            </div>
            <div>
              <span className="text-muted-foreground">アカウント名: </span>
              <span className="font-medium">{confirmedValues?.accountName}</span>
            </div>
            <div>
              <span className="text-muted-foreground">パスワード: </span>
              <span className="font-medium">
                {"•".repeat(confirmedValues?.password.length ?? 0)}
              </span>
            </div>
          </div>

          <AlertDialogFooter>
            <AlertDialogCancel disabled={isRegistering}>
              キャンセル
            </AlertDialogCancel>
            <AlertDialogAction onClick={handleRegister} disabled={isRegistering}>
              {isRegistering ? "登録中..." : "登録する"}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </>
  );
}