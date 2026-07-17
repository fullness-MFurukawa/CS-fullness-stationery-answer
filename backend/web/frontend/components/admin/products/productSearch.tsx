"use client";

import { useState } from "react";
import type { Product } from "@/models/responses/product";
import { useProductSearch, ALL_CATEGORIES } from "@/hooks/admin/products/useProductSearch";
import { ProductCard } from "./productCard";
import { ProductEditDialog } from "./productEditDialog";
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
 * BP006 商品検索画面の本体
 * 商品一覧をカード形式で表示し、カテゴリによる絞り込みと削除を行う。
 */
export function ProductSearch() {
    const {
        products,
        categories,
        categoryId,
        isLoading,
        changeCategory,
        remove,
        applyUpdated,
    } = useProductSearch();

    // ダイアログの開閉に関する状態は画面側で保持する
    const [deleteTarget, setDeleteTarget] = useState<Product | null>(null);
    const [isDeleting, setIsDeleting] = useState(false);
    const [editTarget, setEditTarget] = useState<Product | null>(null);

    /**
     * 削除の確認ダイアログで削除を実行する
     */
    const handleDelete = async () => {
        if (!deleteTarget) return;

        setIsDeleting(true);
        const succeeded = await remove(deleteTarget);
        setIsDeleting(false);

        if (succeeded) {
          setDeleteTarget(null);
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

          <Select value={categoryId} onValueChange={changeCategory}>
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
                onEdit={setEditTarget}
                onDelete={setDeleteTarget}
              />
            ))}
          </div>
        )}

        <ProductEditDialog
          product={editTarget}
          categories={categories}
          onClose={() => setEditTarget(null)}
          onUpdated={applyUpdated}
        />

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