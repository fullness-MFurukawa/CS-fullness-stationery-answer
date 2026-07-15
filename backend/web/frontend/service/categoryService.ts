import { injectable, inject } from "inversify";
import type { ICategoryService } from "@/interfaces/service/categoryService";
import type { ICategoryRepository } from "@/interfaces/repository/categoryRepository";
import type { Category } from "@/models/responses/category";
import type { CategoryRegisterRequest } from "@/models/requests/categoryRegisterRequest";
import { TYPES } from "@/di/types";

/**
 * 商品カテゴリ管理に関するユースケースの実装
 * UC014（商品カテゴリ登録）に対応する。
 */
@injectable()
export class CategoryService implements ICategoryService {
    /**
     * @param categoryRepository 商品カテゴリのリポジトリ
     */
    constructor(
        @inject(TYPES.CategoryRepository) private readonly categoryRepository: ICategoryRepository,
    ) {}

    /**
     * 商品カテゴリの一覧を取得する
     */
    async search(): Promise<Category[]> {
        return this.categoryRepository.search();
    }

    /**
     * 商品カテゴリを登録する（UC014）
     */
    async register(request: CategoryRegisterRequest): Promise<Category> {
        return this.categoryRepository.register(request);
    }
}