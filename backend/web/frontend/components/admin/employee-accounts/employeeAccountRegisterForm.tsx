"use client";

import { useState, useMemo, useEffect } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { useRouter } from "next/navigation";
import { toast } from "sonner";
import { container } from "@/di/container";
import { TYPES } from "@/di/types";
import type { IEmployeeAccountService } from "@/interfaces/service/employeeAccountService";
import type { Employee } from "@/models/responses/employee";
import { ApiError } from "@/infrastructure/http/apiError";
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
  const [employees, setEmployees] = useState<Employee[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isConfirmOpen, setIsConfirmOpen] = useState(false);
  const [isRegistering, setIsRegistering] = useState(false);
  const [confirmedValues, setConfirmedValues] = useState<AccountFormValues | null>(null);

  const service = useMemo(
    () => container.get<IEmployeeAccountService>(TYPES.EmployeeAccountService),
    [],
  );

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
   * アカウント未登録の社員一覧を取得する
   */
  useEffect(() => {
    const load = async () => {
      try {
        const result = await service.getEmployeesWithoutAccount();
        setEmployees(result);
      } catch (e) {
        toast.error(
          e instanceof ApiError ? e.message : "社員一覧の取得に失敗しました",
        );
      } finally {
        setIsLoading(false);
      }
    };
    load();
  }, [service]);

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
    try {
      const account = await service.register(confirmedValues);
      toast.success(`「${account.employeeName}」のアカウントを登録しました`);

      setIsConfirmOpen(false);
      setConfirmedValues(null);

      // 登録した社員は選択肢から除外する
      setEmployees((prev) =>
        prev.filter((e) => e.employeeId !== confirmedValues.employeeId),
      );

      // フォームを初期状態へ戻す（入力値と検証エラーの両方をクリアする）
      reset(defaultValues);
      clearErrors();
    } catch (e) {
      setIsConfirmOpen(false);
      if (e instanceof ApiError && e.isConflict) {
        // アカウント名の重複は入力項目のエラーとして表示する
        setError("accountName", {
          message: "このアカウント名は既に使用されています",
        });
        return;
      }
      toast.error(e instanceof ApiError ? e.message : "登録に失敗しました");
    } finally {
      setIsRegistering(false);
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
            <div className="space-y-2">
              <Label htmlFor="employeeId">
                社員 <span className="text-destructive">*</span>
              </Label>
               <Select
                value={employeeId}
                onValueChange={(value) => {
                  setValue("employeeId", value ?? "");
                  // 選択したらエラー表示を消す
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