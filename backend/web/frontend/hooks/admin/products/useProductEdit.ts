"use client";

import { useState, useMemo } from "react";
import { toast } from "sonner";
import { container } from "@/di/container";
import { TYPES } from "@/di/types";
import type { IProductService } from "@/interfaces/service/productService";
import type { Product } from "@/models/responses/product";
import type { ProductUpdateRequest } from "@/models/requests/productUpdateRequest";
import { ApiError } from "@/infrastructure/http/apiError";

/**
 * 商品修正（BP009）のデータ操作を担うフック
 * 商品の更新と、更新中かどうかの状態を提供する。
 * @remarks カテゴリの選択肢は呼び出し元（商品検索画面）が保持しているものを使うため、ここでは取得しない。
 */
export function useProductEdit() {
    const [isUpdating, setIsUpdating] = useState(false);

    const service = useMemo(() => container.get<IProductService>(TYPES.ProductService),[],);

    /**
     * 商品を修正する
     * @param request 商品修正のリクエスト（画像および画像削除の指示を含む）
     * @returns 修正に成功した場合は修正後の商品、失敗した場合はnull
     */
    const update = async (request: ProductUpdateRequest,): Promise<Product | null> => {
        setIsUpdating(true);
        try {
            const updated = await service.update(request);
            toast.success(`商品「${updated.name}」を修正しました`);
            return updated;
        } catch (e) {
            toast.error(e instanceof ApiError ? e.message : "修正に失敗しました");
            return null;
        } finally {
            setIsUpdating(false);
        }
    };

    return { isUpdating, update };
}