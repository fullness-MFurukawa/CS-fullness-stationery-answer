"use client";

import { useState, useEffect } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { useRouter } from "next/navigation";
import Image from "next/image";
import { ImagePlus, X } from "lucide-react";
import { useProductRegister } from "@/hooks/admin/products/useProductRegister";
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

/** 画像の最大サイズ（2MB） */
const MAX_IMAGE_SIZE = 2 * 1024 * 1024;

/** 許可する画像のMIMEタイプ */
const ACCEPTED_IMAGE_TYPES = ["image/png", "image/jpeg"];

/**
 * 新商品登録の入力値スキーマ
 * 文字数・範囲はデータベースの定義に合わせる。
 */
const productSchema = z.object({
  name: z
    .string()
    .trim()
    .min(1, "商品名を入力してください")
    .max(100, "商品名は100文字以内で入力してください"),
  price: z.coerce
    .number({ message: "価格を数値で入力してください" })
    .int("価格は整数で入力してください")
    .min(0, "価格は0以上で入力してください"),
  categoryId: z.string().min(1, "カテゴリを選択してください"),
  quantity: z.coerce
    .number({ message: "在庫数を数値で入力してください" })
    .int("在庫数は整数で入力してください")
    .min(0, "在庫数は0以上で入力してください"),
  image: z
    .instanceof(File)
    .refine((file) => file.size <= MAX_IMAGE_SIZE, "画像は2MB以下にしてください")
    .refine(
      (file) => ACCEPTED_IMAGE_TYPES.includes(file.type),
      "PNG形式またはJPEG形式の画像を選択してください",
    )
    .nullable(),
});

/** フォームの入力値の型（変換前。price と quantity は文字列として入力される） */
type ProductFormInput = z.input<typeof productSchema>;

/** フォームの検証済みの値の型（変換後） */
type ProductFormValues = z.output<typeof productSchema>;

/** フォームの初期値 */
const defaultValues: ProductFormInput = {
  name: "",
  price: 0,
  categoryId: "",
  quantity: 0,
  image: null,
};

/**
 * BP012 新商品登録フォーム
 * 商品情報と画像を同時に登録する（画像はストレージへ保存し、URLを商品に記録する）。
 */
export function ProductRegisterForm() {
  const router = useRouter();
  const { categories, isLoading, register: registerProduct } = useProductRegister();

  const [isConfirmOpen, setIsConfirmOpen] = useState(false);
  const [isRegistering, setIsRegistering] = useState(false);
  const [confirmedValues, setConfirmedValues] = useState<ProductFormValues | null>(null);
  const [previewUrl, setPreviewUrl] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    setValue,
    watch,
    clearErrors,
    reset,
    formState: { errors },
  } = useForm<ProductFormInput, unknown, ProductFormValues>({
    resolver: zodResolver(productSchema),
    defaultValues,
  });

  const categoryId = watch("categoryId");
  const image = watch("image");

  /**
   * 選択された画像のプレビューURLを生成する
   * @remarks 生成したURLはメモリを占有するため、不要になった時点で解放する。
   */
  useEffect(() => {
    if (!image) {
      setPreviewUrl(null);
      return;
    }
    const url = URL.createObjectURL(image);
    setPreviewUrl(url);
    return () => URL.revokeObjectURL(url);
  }, [image]);

  /**
   * 入力値の検証を通過したら確認ダイアログを表示する
   * @param values 検証済みの入力値
   */
  const onSubmit = (values: ProductFormValues) => {
    setConfirmedValues(values);
    setIsConfirmOpen(true);
  };

  /**
   * 商品を登録する
   */
  const handleRegister = async () => {
    if (!confirmedValues) return;

    setIsRegistering(true);
    const succeeded = await registerProduct({
      name: confirmedValues.name,
      price: confirmedValues.price,
      categoryId: confirmedValues.categoryId,
      quantity: confirmedValues.quantity,
      image: confirmedValues.image,
    });
    setIsRegistering(false);

    setIsConfirmOpen(false);
    if (succeeded) {
      setConfirmedValues(null);
      reset(defaultValues);
      clearErrors();
    }
  };

  const selectedCategory = categories.find((c) => c.categoryId === categoryId);
  const confirmedCategory = categories.find(
    (c) => c.categoryId === confirmedValues?.categoryId,
  );

  if (isLoading) {
    return (
      <Card>
        <CardContent className="space-y-6 pt-6">
          {Array.from({ length: 5 }).map((_, i) => (
            <Skeleton key={i} className="h-10" />
          ))}
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
              <Label htmlFor="name">
                商品名 <span className="text-destructive">*</span>
              </Label>
              <Input id="name" placeholder="例: 水性ボールペン(黒)" {...register("name")} />
              {errors.name && (
                <p className="text-sm text-destructive">{errors.name.message}</p>
              )}
            </div>

            <div className="flex flex-col gap-2">
              <Label htmlFor="categoryId">
                カテゴリ <span className="text-destructive">*</span>
              </Label>
              <Select
                value={categoryId}
                onValueChange={(value) => {
                  setValue("categoryId", value ?? "");
                  if (value) {
                    clearErrors("categoryId");
                  }
                }}
              >
                <SelectTrigger id="categoryId">
                  <SelectValue>
                    {selectedCategory ? selectedCategory.name : "カテゴリを選択してください"}
                  </SelectValue>
                </SelectTrigger>
                <SelectContent>
                  {categories.map((category) => (
                    <SelectItem key={category.categoryId} value={category.categoryId}>
                      {category.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
              {errors.categoryId && (
                <p className="text-sm text-destructive">{errors.categoryId.message}</p>
              )}
            </div>

            <div className="grid gap-4 sm:grid-cols-2">
              <div className="space-y-2">
                <Label htmlFor="price">
                  価格 <span className="text-destructive">*</span>
                </Label>
                <div className="relative">
                  <Input
                    id="price"
                    type="number"
                    min={0}
                    className="pr-8"
                    {...register("price")}
                  />
                  <span className="absolute right-3 top-1/2 -translate-y-1/2 text-sm text-muted-foreground">
                    円
                  </span>
                </div>
                {errors.price && (
                  <p className="text-sm text-destructive">{errors.price.message}</p>
                )}
              </div>

              <div className="space-y-2">
                <Label htmlFor="quantity">
                  在庫数 <span className="text-destructive">*</span>
                </Label>
                <div className="relative">
                  <Input
                    id="quantity"
                    type="number"
                    min={0}
                    className="pr-8"
                    {...register("quantity")}
                  />
                  <span className="absolute right-3 top-1/2 -translate-y-1/2 text-sm text-muted-foreground">
                    個
                  </span>
                </div>
                {errors.quantity && (
                  <p className="text-sm text-destructive">{errors.quantity.message}</p>
                )}
              </div>
            </div>

            <div className="space-y-2">
              <Label htmlFor="image">商品画像</Label>
              {previewUrl ? (
                <div className="relative w-40">
                  <div className="relative aspect-square overflow-hidden rounded-lg border bg-muted">
                    <Image
                      src={previewUrl}
                      alt="プレビュー"
                      fill
                      className="object-contain p-2"
                    />
                  </div>
                  <Button
                    type="button"
                    variant="secondary"
                    size="icon"
                    className="absolute -right-2 -top-2 size-7 rounded-full"
                    onClick={() => setValue("image", null)}
                  >
                    <X className="size-4" />
                  </Button>
                </div>
              ) : (
                <label
                  htmlFor="image"
                  className="flex aspect-square w-40 cursor-pointer flex-col items-center justify-center gap-2 rounded-lg border border-dashed text-muted-foreground hover:bg-accent"
                >
                  <ImagePlus className="size-8" />
                  <span className="text-xs">画像を選択</span>
                </label>
              )}
              <Input
                id="image"
                type="file"
                accept="image/png,image/jpeg"
                className="hidden"
                onChange={(e) => {
                  setValue("image", e.target.files?.[0] ?? null);
                  clearErrors("image");
                }}
              />
              <p className="text-xs text-muted-foreground">
                PNG形式またはJPEG形式、2MB以下（任意）
              </p>
              {errors.image && (
                <p className="text-sm text-destructive">{errors.image.message}</p>
              )}
            </div>

            <div className="flex gap-2">
              <Button type="submit">登録する</Button>
              <Button
                type="button"
                variant="outline"
                onClick={() => router.push("/admin/products")}
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
              <span className="text-muted-foreground">商品名: </span>
              <span className="font-medium">{confirmedValues?.name}</span>
            </div>
            <div>
              <span className="text-muted-foreground">カテゴリ: </span>
              <span className="font-medium">{confirmedCategory?.name}</span>
            </div>
            <div>
              <span className="text-muted-foreground">価格: </span>
              <span className="font-medium">
                {confirmedValues?.price.toLocaleString()} 円
              </span>
            </div>
            <div>
              <span className="text-muted-foreground">在庫数: </span>
              <span className="font-medium">
                {confirmedValues?.quantity.toLocaleString()} 個
              </span>
            </div>
            <div>
              <span className="text-muted-foreground">画像: </span>
              <span className="font-medium">
                {confirmedValues?.image ? confirmedValues.image.name : "なし"}
              </span>
            </div>
          </div>

          <AlertDialogFooter>
            <AlertDialogCancel disabled={isRegistering}>キャンセル</AlertDialogCancel>
            <AlertDialogAction onClick={handleRegister} disabled={isRegistering}>
              {isRegistering ? "登録中..." : "登録する"}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </>
  );
}