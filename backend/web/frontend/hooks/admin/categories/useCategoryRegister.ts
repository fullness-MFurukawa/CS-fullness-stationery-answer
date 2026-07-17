"use client";

import { useMemo } from "react";
import { toast } from "sonner";
import { container } from "@/di/container";
import { TYPES } from "@/di/types";
import type { ICategoryService } from "@/interfaces/service/categoryService";
import type { CategoryRegisterRequest } from "@/models/requests/categoryRegisterRequest";
import { ApiError } from "@/infrastructure/http/apiError";

/**
 * 商品カテゴリ登録（BP019）のデータ操作を担うフック
 * 登録処理とその結果通知を提供する。
 */
export function useCategoryRegister() {
    const service = useMemo(
        () => container.get<ICategoryService>(TYPES.CategoryService),[],);

    /**
     * 商品カテゴリを登録する
     * @param request 商品カテゴリ登録のリクエスト
     * @returns 登録に成功した場合はtrue
     */
    const register = async (request: CategoryRegisterRequest): Promise<boolean> => {
        try {
            const category = await service.register(request);
            toast.success(`商品カテゴリ「${category.name}」を登録しました`);
            return true;
        } catch (e) {
            toast.error(e instanceof ApiError ? e.message : "登録に失敗しました");
            return false;
        }
    };

    return { register };
}