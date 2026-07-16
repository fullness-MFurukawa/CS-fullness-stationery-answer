"use client";

import { useState, useMemo } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { useRouter } from "next/navigation";
import { toast } from "sonner";
import { container } from "@/di/container";
import { TYPES } from "@/di/types";
import type { ICategoryService } from "@/interfaces/service/categoryService";
import { ApiError } from "@/infrastructure/http/apiError";
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

/**
 * BP019 商品カテゴリ登録フォーム
 * 入力内容を確認ダイアログで確かめてから登録する。
 */
export function CategoryRegisterForm() {
    const router = useRouter();
    const [isConfirmOpen, setIsConfirmOpen] = useState(false);
    const [isRegistering, setIsRegistering] = useState(false);
    // 検証を通過した入力値（確認ダイアログと登録処理で使用する）
    const [confirmedValues, setConfirmedValues] = useState<CategoryFormValues | null>(null);

    const service = useMemo(
        () => container.get<ICategoryService>(TYPES.CategoryService),
        [],
    );

    const {
        register,
        handleSubmit,
        reset,
        formState: { errors },
        } = useForm<CategoryFormValues>({
            resolver: zodResolver(categorySchema),
            defaultValues: { name: "" },
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
        try {
            const category = await service.register({ name: confirmedValues.name });
            toast.success(`商品カテゴリ「${category.name}」を登録しました`);
            setIsConfirmOpen(false);
            setConfirmedValues(null);
            reset();
        }       catch (e) {
            toast.error(e instanceof ApiError ? e.message : "登録に失敗しました");
        } finally {
            setIsRegistering(false);
        }
    };

  return (
    <>
      <Card className="max-w-xl">
        <CardHeader>
          <CardTitle>商品カテゴリ登録</CardTitle>
        </CardHeader>

        <CardContent>
          <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
            <div className="space-y-2">
              <Label htmlFor="name">
                カテゴリ名 <span className="text-destructive">*</span>
              </Label>
              <Input
                id="name"
                placeholder="例: 画材"
                {...register("name")}
              />
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