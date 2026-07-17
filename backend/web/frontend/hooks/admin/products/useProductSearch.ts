"use client";

import { useEffect, useState, useCallback, useMemo } from "react";
import { toast } from "sonner";
import { container } from "@/di/container";
import { TYPES } from "@/di/types";
import type { IProductService } from "@/interfaces/service/productService";
import type { Product } from "@/models/responses/product";
import type { Category } from "@/models/responses/category";
import { ApiError } from "@/infrastructure/http/apiError";

/** カテゴリ絞り込みの「すべて」を表す値 */
export const ALL_CATEGORIES = "all";

/**
 * 商品検索画面（BP006）のデータ操作を担うフック
 * 商品・カテゴリの取得、カテゴリによる絞り込み、削除、修正結果の反映を提供する。
 * 画面の状態（ダイアログの開閉など）は保持しない。
 */
export function useProductSearch() {
    const [products, setProducts] = useState<Product[]>([]);
    const [categories, setCategories] = useState<Category[]>([]);
    const [categoryId, setCategoryId] = useState<string>(ALL_CATEGORIES);
    const [isLoading, setIsLoading] = useState(true);

    // Serviceの取得はレンダリングのたびに行わない
    const service = useMemo(
        () => container.get<IProductService>(TYPES.ProductService),[],);

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
            toast.error(e instanceof ApiError ? e.message : "データの取得に失敗しました",);
        } finally {
            setIsLoading(false);
        }
    }, [service]);

    useEffect(() => {
        loadInitial();
    }, [loadInitial]);

    /**
     * カテゴリを変更して商品一覧のみを再取得する
     * @param value 選択されたカテゴリ識別ID。未選択の場合はnull
     */
    const changeCategory = async (value: string | null) => {
        const selected = value ?? ALL_CATEGORIES;
        setCategoryId(selected);
        setIsLoading(true);
        try {
            const result = await service.search(
            selected === ALL_CATEGORIES ? undefined : selected,);
            setProducts(result);
        } catch (e) {
            toast.error(e instanceof ApiError ? e.message : "商品の検索に失敗しました",);
        } finally {
            setIsLoading(false);
        }
    };

    /**
     * 商品を削除する（論理削除）
     * @param product 削除対象の商品
     * @returns 削除に成功した場合はtrue
     */
    const remove = async (product: Product): Promise<boolean> => {
        try {
            await service.delete(product.productId);
            setProducts((prev) =>
            prev.filter((p) => p.productId !== product.productId),);
            toast.success(`「${product.name}」を削除しました`);
            return true;
        } catch (e) {
            toast.error(e instanceof ApiError ? e.message : "削除に失敗しました");
            return false;
        }
    };

    /**
     * 修正された商品を一覧へ反映する
     * @param updated 修正後の商品
     */
    const applyUpdated = (updated: Product) => {
        setProducts((prev) =>
            prev.map((p) => (p.productId === updated.productId ? updated : p)),);
    };

    return {
        products,
        categories,
        categoryId,
        isLoading,
        changeCategory,
        remove,
        applyUpdated,
    };
}