import type { Category } from "@/models/responses/category";
import type { CategoryRegisterRequest } from "@/models/requests/categoryRegisterRequest";

/**
 * 商品カテゴリに関するAPI呼び出しを抽象化するリポジトリ
 * バックエンドの /api/admin/categories エンドポイント群に対応する。
 */
export interface ICategoryRepository {
    /**
   　* 商品カテゴリの一覧を取得する
   　* @returns 商品カテゴリの一覧
   　*/
    search(): Promise<Category[]>;

    /**
   　* 商品カテゴリを登録する（UC014）
   　* @param request 商品カテゴリ登録のリクエスト
   　* @returns 登録された商品カテゴリ
   　*/
    register(request: CategoryRegisterRequest): Promise<Category>;
}