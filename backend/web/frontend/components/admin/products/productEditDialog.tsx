"use client";

import { useState, useEffect, useMemo } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { toast } from "sonner";
import Image from "next/image";
import { ImagePlus, X } from "lucide-react";
import { container } from "@/di/container";
import { TYPES } from "@/di/types";
import type { IProductService } from "@/interfaces/service/productService";
import type { Product } from "@/models/responses/product";
import type { Category } from "@/models/responses/category";
import { ApiError } from "@/infrastructure/http/apiError";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";

/** 画像の最大サイズ（2MB） */
const MAX_IMAGE_SIZE = 2 * 1024 * 1024;

/** 許可する画像のMIMEタイプ */
const ACCEPTED_IMAGE_TYPES = ["image/png", "image/jpeg"];

/**
 * 商品修正の入力値スキーマ
 * 制約は新商品登録（UC010）と同じとする。
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

type ProductFormInput = z.input<typeof productSchema>;
type ProductFormValues = z.output<typeof productSchema>;

/**
 * BP009 商品修正ダイアログ
 * 商品検索画面の一覧から選択した商品を修正する。
 * @param product 修正対象の商品。nullの場合はダイアログを表示しない
 * @param categories カテゴリの選択肢
 * @param onClose ダイアログを閉じるときの処理
 * @param onUpdated 修正が完了したときの処理
 */
export function ProductEditDialog({
  product,
  categories,
  onClose,
  onUpdated,
}: {
  product: Product | null;
  categories: Category[];
  onClose: () => void;
  onUpdated: (updated: Product) => void;
}) {
  const [isUpdating, setIsUpdating] = useState(false);
  // 既存の画像を削除するかどうか
  const [removeImage, setRemoveImage] = useState(false);
  const [previewUrl, setPreviewUrl] = useState<string | null>(null);

  const service = useMemo(
    () => container.get<IProductService>(TYPES.ProductService),
    [],
  );

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
    defaultValues: { name: "", price: 0, categoryId: "", quantity: 0, image: null },
  });

  const categoryId = watch("categoryId");
  const image = watch("image");

  /**
   * 対象の商品が変わったらフォームへ既存の値を反映する
   */
  useEffect(() => {
    if (!product) return;

    reset({
      name: product.name,
      price: product.price,
      categoryId: product.categoryId,
      quantity: product.quantity,
      image: null,
    });
    setRemoveImage(false);
  }, [product, reset]);

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
   * 商品を修正する
   * @param values 検証済みの入力値
   */
  const onSubmit = async (values: ProductFormValues) => {
    if (!product) return;

    setIsUpdating(true);
    try {
      const updated = await service.update({
        productId: product.productId,
        name: values.name,
        price: values.price,
        categoryId: values.categoryId,
        quantity: values.quantity,
        image: values.image,
        removeImage,
      });
      toast.success(`商品「${updated.name}」を修正しました`);
      onUpdated(updated);
      onClose();
    } catch (e) {
      toast.error(e instanceof ApiError ? e.message : "修正に失敗しました");
    } finally {
      setIsUpdating(false);
    }
  };

  const selectedCategory = categories.find((c) => c.categoryId === categoryId);

  // 表示する画像を決める（新しく選択した画像 > 既存の画像 > なし）
  const displayImageUrl = previewUrl ?? (removeImage ? null : product?.imageUrl ?? null);

  return (
    <Dialog open={product !== null} onOpenChange={(open) => !open && onClose()}>
      <DialogContent className="sm:max-w-lg">
        <DialogHeader>
          <DialogTitle>商品修正</DialogTitle>
          <DialogDescription>商品の情報を変更します。</DialogDescription>
        </DialogHeader>

        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="edit-name">
              商品名 <span className="text-destructive">*</span>
            </Label>
            <Input id="edit-name" {...register("name")} />
            {errors.name && (
              <p className="text-sm text-destructive">{errors.name.message}</p>
            )}
          </div>

          <div className="space-y-2">
            <Label htmlFor="edit-categoryId">
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
              <SelectTrigger id="edit-categoryId">
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
              <Label htmlFor="edit-price">
                価格 <span className="text-destructive">*</span>
              </Label>
              <div className="relative">
                <Input id="edit-price" type="number" min={0} className="pr-8" {...register("price")} />
                <span className="absolute right-3 top-1/2 -translate-y-1/2 text-sm text-muted-foreground">
                  円
                </span>
              </div>
              {errors.price && (
                <p className="text-sm text-destructive">{errors.price.message}</p>
              )}
            </div>

            <div className="space-y-2">
              <Label htmlFor="edit-quantity">
                在庫数 <span className="text-destructive">*</span>
              </Label>
              <div className="relative">
                <Input id="edit-quantity" type="number" min={0} className="pr-8" {...register("quantity")} />
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
            <Label htmlFor="edit-image">商品画像</Label>
            {displayImageUrl ? (
              <div className="relative w-32">
                <div className="relative aspect-square overflow-hidden rounded-lg border bg-muted">
                  <Image
                    src={displayImageUrl}
                    alt="商品画像"
                    fill
                    className="object-contain p-2"
                  />
                </div>
                <Button
                  type="button"
                  variant="secondary"
                  size="icon"
                  className="absolute -right-2 -top-2 size-7 rounded-full"
                  onClick={() => {
                    setValue("image", null);
                    setRemoveImage(true);
                  }}
                >
                  <X className="size-4" />
                </Button>
              </div>
            ) : (
              <label
                htmlFor="edit-image"
                className="flex aspect-square w-32 cursor-pointer flex-col items-center justify-center gap-2 rounded-lg border border-dashed text-muted-foreground hover:bg-accent"
              >
                <ImagePlus className="size-6" />
                <span className="text-xs">画像を選択</span>
              </label>
            )}
            <Input
              id="edit-image"
              type="file"
              accept="image/png,image/jpeg"
              className="hidden"
              onChange={(e) => {
                const file = e.target.files?.[0] ?? null;
                setValue("image", file);
                if (file) {
                  setRemoveImage(false);
                }
                clearErrors("image");
              }}
            />
            <p className="text-xs text-muted-foreground">
              PNG形式またはJPEG形式、2MB以下
            </p>
            {errors.image && (
              <p className="text-sm text-destructive">{errors.image.message}</p>
            )}
          </div>

          <DialogFooter>
            <Button type="button" variant="outline" onClick={onClose} disabled={isUpdating}>
              キャンセル
            </Button>
            <Button type="submit" disabled={isUpdating}>
              {isUpdating ? "更新中..." : "更新する"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}