"use client";

import { useEffect, useState, useCallback, useMemo } from "react";
import { toast } from "sonner";
import { container } from "@/di/container";
import { TYPES } from "@/di/types";
import type { IProductService } from "@/interfaces/service/productService";
import type { Product } from "@/models/responses/product";
import type { Category } from "@/models/responses/category";
import { ApiError } from "@/infrastructure/http/apiError";
import { ProductCard } from "./productCard";
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

/** カテゴリ絞り込みの「すべて」を表す値 */
const ALL_CATEGORIES = "all";

/**
 * BP006 商品検索画面の本体
 * 商品一覧をカード形式で表示し、カテゴリによる絞り込みと削除を行う。
 */
export function ProductSearch() {
  const [products, setProducts] = useState<Product[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);
  const [categoryId, setCategoryId] = useState<string>(ALL_CATEGORIES);
  const [isLoading, setIsLoading] = useState(true);
  const [deleteTarget, setDeleteTarget] = useState<Product | null>(null);
  const [isDeleting, setIsDeleting] = useState(false);

  // Serviceの取得はレンダリングのたびに行わない
  const service = useMemo(
    () => container.get<IProductService>(TYPES.ProductService),
    [],
  );

  /**
   * 画面の初期表示に必要なデータを取得する
   */
  const loadInitial = useCallback(async () => {
    setIsLoading(true);
    try {
      const view = await service.getSearchView();
      setProducts(view.products);
      setCategories(view.categories);
    } catch (e) {
      toast.error(
        e instanceof ApiError ? e.message : "データの取得に失敗しました",
      );
    } finally {
      setIsLoading(false);
    }
  }, [service]);

  useEffect(() => {
    loadInitial();
  }, [loadInitial]);

  /**
   * カテゴリを変更したときに商品一覧のみを再取得する
   * @param value 選択されたカテゴリ識別ID。未選択の場合はnull
   */
  const handleCategoryChange = async (value: string | null) => {
    const selected = value ?? ALL_CATEGORIES;
    setCategoryId(selected);
    setIsLoading(true);
    try {
      const result = await service.search(
        selected === ALL_CATEGORIES ? undefined : selected,
      );
      setProducts(result);
    } catch (e) {
      toast.error(
        e instanceof ApiError ? e.message : "商品の検索に失敗しました",
      );
    } finally {
      setIsLoading(false);
    }
  };

  /**
   * 商品を削除する（論理削除）
   */
  const handleDelete = async () => {
    if (!deleteTarget) return;

    setIsDeleting(true);
    try {
      await service.delete(deleteTarget.productId);
      setProducts((prev) =>
        prev.filter((p) => p.productId !== deleteTarget.productId),
      );
      toast.success(`「${deleteTarget.name}」を削除しました`);
      setDeleteTarget(null);
    } catch (e) {
      toast.error(e instanceof ApiError ? e.message : "削除に失敗しました");
    } finally {
      setIsDeleting(false);
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold">商品検索</h1>
          <p className="mt-1 text-muted-foreground">
            {isLoading ? "読み込み中..." : `${products.length}件の商品`}
          </p>
        </div>

        <Select value={categoryId} onValueChange={handleCategoryChange}>
          <SelectTrigger className="w-56">
            <SelectValue>
              {categoryId === ALL_CATEGORIES
                ? "すべてのカテゴリ"
                : categories.find((c) => c.categoryId === categoryId)?.name}
            </SelectValue>
          </SelectTrigger>
          <SelectContent>
            <SelectItem value={ALL_CATEGORIES}>すべてのカテゴリ</SelectItem>
            {categories.map((category) => (
              <SelectItem key={category.categoryId} value={category.categoryId}>
                {category.name}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>

      {isLoading ? (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
          {Array.from({ length: 8 }).map((_, i) => (
            <Skeleton key={i} className="h-80 rounded-xl" />
          ))}
        </div>
      ) : products.length === 0 ? (
        <div className="rounded-lg border border-dashed py-16 text-center text-muted-foreground">
          該当する商品がありません
        </div>
      ) : (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
          {products.map((product) => (
            <ProductCard
              key={product.productId}
              product={product}
              onDelete={setDeleteTarget}
            />
          ))}
        </div>
      )}

      <AlertDialog
        open={deleteTarget !== null}
        onOpenChange={(open) => !open && setDeleteTarget(null)}
      >
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>商品を削除しますか？</AlertDialogTitle>
            <AlertDialogDescription>
              「{deleteTarget?.name}」を削除します。この操作は取り消せません。
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel disabled={isDeleting}>
              キャンセル
            </AlertDialogCancel>
            <AlertDialogAction onClick={handleDelete} disabled={isDeleting}>
              {isDeleting ? "削除中..." : "削除する"}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}