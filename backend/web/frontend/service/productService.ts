import { injectable, inject } from "inversify";
import type { IProductService, ProductSearchView } from "@/interfaces/service/productService";
import type { IProductRepository } from "@/interfaces/repository/productRepository";
import type { ICategoryRepository } from "@/interfaces/repository/categoryRepository";
import type { Product } from "@/models/responses/product";
import type { Category } from "@/models/responses/category";
import type { ProductRegisterRequest } from "@/models/requests/productRegisterRequest";
import type { ProductUpdateRequest } from "@/models/requests/productUpdateRequest";
import { TYPES } from "@/di/types";

/**
 * 商品管理に関するユースケースの実装
 * 商品と商品カテゴリの2つのリポジトリを組み合わせ、画面が必要とするデータを提供する。
 */
@injectable()
export class ProductService implements IProductService {
    /**
     * @param productRepository 商品のリポジトリ
     * @param categoryRepository 商品カテゴリのリポジトリ
     */
    constructor(
        @inject(TYPES.ProductRepository) private readonly productRepository: IProductRepository,
        @inject(TYPES.CategoryRepository) private readonly categoryRepository: ICategoryRepository,
    ) {}

    /**
     * 商品検索画面の初期表示に必要なデータを取得する（UC011）
     * @remarks 商品一覧とカテゴリ一覧は互いに依存しないため、並行して取得する。
     */
    async getSearchView(categoryId?: string): Promise<ProductSearchView> {
        const [products, categories] = await Promise.all([
            this.productRepository.search(categoryId),
            this.categoryRepository.search(),
        ]);
        return { products, categories };
    }

    /**
     * 商品を検索する（UC011）
     */
    async search(categoryId?: string): Promise<Product[]> {
        return this.productRepository.search(categoryId);
    }

    /**
     * 商品登録・修正画面で選択肢として使用するカテゴリ一覧を取得する
     */
    async getCategories(): Promise<Category[]> {
        return this.categoryRepository.search();
    }

    /**
     * 新しい商品を登録する（UC010）
     */
    async register(request: ProductRegisterRequest): Promise<Product> {
        return this.productRepository.register(request);
    }

    /**
     * 商品情報を修正する（UC012）
     */
    async update(request: ProductUpdateRequest): Promise<Product> {
        return this.productRepository.update(request);
    }

    /**
     * 商品を削除する（UC013、論理削除）
     */
    async delete(productId: string): Promise<Product> {
        return this.productRepository.delete(productId);
    }
}