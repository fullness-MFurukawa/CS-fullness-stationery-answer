"use client";

import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { useRouter } from "next/navigation";
import { useCategoryRegister } from "@/hooks/admin/categories/useCategoryRegister";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
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

/**
 * 商品カテゴリ登録の入力値スキーマ
 * 文字数はデータベースの定義（VARCHAR(30)）に合わせる。
 */
const categorySchema = z.object({
  name: z
    .string()
    .trim()
    .min(1, "カテゴリ名を入力してください")
    .max(30, "カテゴリ名は30文字以内で入力してください"),
});

type CategoryFormValues = z.infer<typeof categorySchema>;

/** フォームの初期値 */
const defaultValues: CategoryFormValues = { name: "" };

/**
 * BP019 商品カテゴリ登録フォーム
 * 入力内容を確認ダイアログで確かめてから登録する。
 */
export function CategoryRegisterForm() {
  const router = useRouter();
  const { register: registerCategory } = useCategoryRegister();

  const [isConfirmOpen, setIsConfirmOpen] = useState(false);
  const [isRegistering, setIsRegistering] = useState(false);
  // 検証を通過した入力値（確認ダイアログと登録処理で使用する）
  const [confirmedValues, setConfirmedValues] = useState<CategoryFormValues | null>(null);

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<CategoryFormValues>({
    resolver: zodResolver(categorySchema),
    defaultValues,
  });

  /**
   * 入力値の検証を通過したら確認ダイアログを表示する
   * @param values 検証済みの入力値（前後の空白は除去済み）
   */
  const onSubmit = (values: CategoryFormValues) => {
    setConfirmedValues(values);
    setIsConfirmOpen(true);
  };

  /**
   * 商品カテゴリを登録する
   */
  const handleRegister = async () => {
    if (!confirmedValues) return;

    setIsRegistering(true);
    const succeeded = await registerCategory({ name: confirmedValues.name });
    setIsRegistering(false);

    if (succeeded) {
      setIsConfirmOpen(false);
      setConfirmedValues(null);
      reset(defaultValues);
    }
  };

  return (
    <>
      <Card>
        <CardHeader>
          <CardTitle>商品カテゴリ登録</CardTitle>
        </CardHeader>

        <CardContent>
          <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
            <div className="space-y-2">
              <Label htmlFor="name">
                カテゴリ名 <span className="text-destructive">*</span>
              </Label>
              <Input id="name" placeholder="例: 画材" {...register("name")} />
              {errors.name && (
                <p className="text-sm text-destructive">{errors.name.message}</p>
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

          <div className="rounded-lg border p-3 text-sm">
            <span className="text-muted-foreground">カテゴリ名: </span>
            <span className="font-medium">{confirmedValues?.name}</span>
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