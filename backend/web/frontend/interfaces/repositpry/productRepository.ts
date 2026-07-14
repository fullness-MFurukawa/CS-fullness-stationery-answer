import type { Product } from "@/models/responses/product";
import type { ProductRegisterRequest } from "@/models/requests/productRegisterRequest";
import type { ProductUpdateRequest } from "@/models/requests/productUpdateRequest";

/**
 * 商品に関するAPI呼び出しを抽象化するリポジトリ
 * バックエンドの /api/admin/products エンドポイント群に対応する。
 */
export interface IProductRepository {
    /**
     * 商品を検索する（UC011）
     * @param categoryId 商品カテゴリ識別ID(uuid)。未指定の場合は全件取得する
     * @returns 論理削除を除いた商品の一覧
     */
    search(categoryId?: string): Promise<Product[]>;

    /**
     * 新しい商品を登録する（UC010）
     * @param request 新商品登録のリクエスト（画像を含む）
     * @returns 登録された商品
     */
    register(request: ProductRegisterRequest): Promise<Product>;

    /**
     * 商品情報を修正する（UC012）
     * @param request 商品修正のリクエスト（画像を含む）
     * @returns 修正された商品
     */
    update(request: ProductUpdateRequest): Promise<Product>;

    /**
     * 商品を削除する（UC013、論理削除）
     * @param productId 削除対象の商品識別ID(uuid)
     * @returns 削除された商品
     */
    delete(productId: string): Promise<Product>;
}