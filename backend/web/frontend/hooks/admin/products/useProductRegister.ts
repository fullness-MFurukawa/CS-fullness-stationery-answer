"use client";

import { useEffect, useState, useMemo } from "react";
import { toast } from "sonner";
import { container } from "@/di/container";
import { TYPES } from "@/di/types";
import type { IProductService } from "@/interfaces/service/productService";
import type { Category } from "@/models/responses/category";
import type { ProductRegisterRequest } from "@/models/requests/productRegisterRequest";
import { ApiError } from "@/infrastructure/http/apiError";

/**
 * 新商品登録（BP012）のデータ操作を担うフック
 * 選択肢となるカテゴリ一覧の取得と、商品の登録を提供する。
 */
export function useProductRegister() {
    const [categories, setCategories] = useState<Category[]>([]);
    const [isLoading, setIsLoading] = useState(true);

    const service = useMemo(() => container.get<IProductService>(TYPES.ProductService),[],);

    /**
     * カテゴリ一覧を取得する
     */
    useEffect(() => {
        const load = async () => {
            try {
                const result = await service.getCategories();
                setCategories(result);
            } catch (e) {
                toast.error(e instanceof ApiError ? e.message : "カテゴリの取得に失敗しました",);
            } finally {
                setIsLoading(false);
            }
        };
        load();
    }, [service]);

    /**
     * 商品を登録する
     * @param request 新商品登録のリクエスト（画像を含む）
     * @returns 登録に成功した場合はtrue
     */
    const register = async (request: ProductRegisterRequest): Promise<boolean> => {
        try {
            const product = await service.register(request);
            toast.success(`商品「${product.name}」を登録しました`);
            return true;
        } catch (e) {
            toast.error(e instanceof ApiError ? e.message : "登録に失敗しました");
            return false;
        }
    };

    return { categories, isLoading, register };
}