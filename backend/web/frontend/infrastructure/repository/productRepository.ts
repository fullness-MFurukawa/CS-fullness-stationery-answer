import { injectable, inject } from "inversify";
import type { IProductRepository } from "@/interfaces/repository/productRepository";
import type { Product } from "@/models/responses/product";
import type { ProductRegisterRequest } from "@/models/requests/productRegisterRequest";
import type { ProductUpdateRequest } from "@/models/requests/productUpdateRequest";
import { HttpClient } from "@/infrastructure/http/httpClient";
import { TYPES } from "@/di/types";

/**
 * 商品に関するAPI呼び出しの実装
 * バックエンドの /api/admin/products エンドポイント群を呼び出す。
 */
@injectable()
export class ProductRepository implements IProductRepository {
    /**
     * @param httpClient API通信の共通クライアント
     */
    constructor(
        @inject(TYPES.HttpClient) private readonly httpClient: HttpClient,
    ) {}

    /**
     * 商品を検索する（UC011）
     */
    async search(categoryId?: string): Promise<Product[]> {
        const query = categoryId ? `?categoryId=${encodeURIComponent(categoryId)}` : "";
        return this.httpClient.get<Product[]>(`/api/admin/products${query}`);
    }

    /**
     * 新しい商品を登録する（UC010）
     */
    async register(request: ProductRegisterRequest): Promise<Product> {
        const formData = this.toFormData(request);
        return this.httpClient.sendForm<Product>("POST", "/api/admin/products", formData);
    }

    /**
     * 商品情報を修正する（UC012）
     */
    async update(request: ProductUpdateRequest): Promise<Product> {
        const formData = this.toFormData(request);
        return this.httpClient.sendForm<Product>(
            "PUT",
            `/api/admin/products/${request.productId}`,
            formData,
        );
    }

    /**
     * 商品を削除する（UC013、論理削除）
     */
    async delete(productId: string): Promise<Product> {
        return this.httpClient.delete<Product>(`/api/admin/products/${productId}`);
    }

    /**
     * 登録・修正のリクエストを multipart/form-data の本文へ変換する
     * @remarks フィールド名はバックエンドの ViewModel のプロパティ名に一致させる。
     */
    private toFormData(request: ProductRegisterRequest | ProductUpdateRequest): FormData {
        const formData = new FormData();
        formData.append("Name", request.name);
        formData.append("Price", String(request.price));
        formData.append("CategoryId", request.categoryId);
        formData.append("Quantity", String(request.quantity));
        if (request.image) {
            formData.append("Image", request.image);
        }
        return formData;
    }
}