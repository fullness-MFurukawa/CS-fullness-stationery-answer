import type { Product } from "@/models/responses/product";
import type { Category } from "@/models/responses/category";
import type { ProductRegisterRequest } from "@/models/requests/productRegisterRequest";
import type { ProductUpdateRequest } from "@/models/requests/productUpdateRequest";

/**
 * 商品検索画面が必要とするデータ
 * 商品一覧と、絞り込み用のカテゴリ一覧をまとめて返す。
 */
export interface ProductSearchView {
  products: Product[];
  categories: Category[];
}

/**
 * 商品管理に関するユースケースを提供するサービス
 * UC010（新商品登録）・UC011（商品検索）・UC012（商品修正）・UC013（商品削除）に対応する。
 */
export interface IProductService {
    /**
     * 商品検索画面の初期表示に必要なデータを取得する（UC011）
     * 商品一覧とカテゴリ一覧を同時に取得する。
     * @param categoryId 絞り込むカテゴリ識別ID(uuid)。未指定の場合は全件
     * @returns 商品一覧とカテゴリ一覧
     */
    getSearchView(categoryId?: string): Promise<ProductSearchView>;

    /**
     * 商品を検索する（UC011）
     * 絞り込み条件の変更時など、商品一覧のみを再取得する場合に使用する。
     * @param categoryId 絞り込むカテゴリ識別ID(uuid)。未指定の場合は全件
     * @returns 論理削除を除いた商品の一覧
     */
    search(categoryId?: string): Promise<Product[]>;

    /**
     * 商品登録・修正画面で選択肢として使用するカテゴリ一覧を取得する
     * @returns 商品カテゴリの一覧
     */
    getCategories(): Promise<Category[]>;

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