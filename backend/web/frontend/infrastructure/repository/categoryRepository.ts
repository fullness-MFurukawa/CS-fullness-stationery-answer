import { injectable, inject } from "inversify";
import type { ICategoryRepository } from "@/interfaces/repositpry/categoryRepository";
import type { Category } from "@/models/responses/category";
import type { CategoryRegisterRequest } from "@/models/requests/categoryRegisterRequest";
import { HttpClient } from "@/infrastructure/http/httpClient";
import { TYPES } from "@/di/types";

/**
 * 商品カテゴリに関するAPI呼び出しの実装
 * バックエンドの /api/admin/categories エンドポイント群を呼び出す。
 */
@injectable()
export class CategoryRepository implements ICategoryRepository {
    /**
     * @param httpClient API通信の共通クライアント
     */
    constructor(
        @inject(TYPES.HttpClient) private readonly httpClient: HttpClient,
    ) {}

    /**
     * 商品カテゴリの一覧を取得する
     */
    async search(): Promise<Category[]> {
        return this.httpClient.get<Category[]>("/api/admin/categories");
    }

    /**
     * 商品カテゴリを登録する（UC014）
     */
    async register(request: CategoryRegisterRequest): Promise<Category> {
        return this.httpClient.sendJson<Category>("POST", "/api/admin/categories", request);
    }
}