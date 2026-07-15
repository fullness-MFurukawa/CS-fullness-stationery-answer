import { describe, it, expect, beforeEach, vi } from "vitest";
import { ProductService } from "@/service/productService";
import type { IProductRepository } from "@/interfaces/repository/productRepository";
import type { ICategoryRepository } from "@/interfaces/repository/categoryRepository";
import type { Product } from "@/models/responses/product";
import type { Category } from "@/models/responses/category";

describe("ProductServiceの単体テストドライバ", () => {
    let productRepository: IProductRepository;
    let categoryRepository: ICategoryRepository;
    let service: ProductService;

    const sampleProduct: Product = {
        productId: "prod-1",
        name: "水性ボールペン(黒)",
        price: 120,
        imageUrl: null,
        quantity: 10,
        categoryId: "cat-1",
        categoryName: "文房具",
        isDeleted: false,
    };

    const sampleCategories: Category[] = [
        { categoryId: "cat-1", name: "文房具" },
        { categoryId: "cat-2", name: "雑貨" },
    ];

    beforeEach(() => {
        // リポジトリはインターフェイス型のため、モックを直接注入する
        productRepository = {
            search: vi.fn().mockResolvedValue([sampleProduct]),
            register: vi.fn().mockResolvedValue(sampleProduct),
            update: vi.fn().mockResolvedValue(sampleProduct),
            delete: vi.fn().mockResolvedValue({ ...sampleProduct, isDeleted: true }),
        };

        categoryRepository = {
            search: vi.fn().mockResolvedValue(sampleCategories),
            register: vi.fn().mockResolvedValue(sampleCategories[0]),
        };

        service = new ProductService(productRepository, categoryRepository);
    });

    describe("getSearchView:商品検索画面のデータを取得する", () => {
        it("商品一覧とカテゴリ一覧をまとめて返す", async () => {
            const result = await service.getSearchView();

            expect(result.products).toHaveLength(1);
            expect(result.products[0].name).toBe("水性ボールペン(黒)");
            expect(result.categories).toHaveLength(2);
            expect(result.categories[0].name).toBe("文房具");
        });

        it("両方のリポジトリを1回ずつ呼び出す", async () => {
            await service.getSearchView();

            expect(productRepository.search).toHaveBeenCalledOnce();
            expect(categoryRepository.search).toHaveBeenCalledOnce();
        });

        it("カテゴリIDを商品リポジトリへ渡す", async () => {
            await service.getSearchView("cat-1");

            expect(productRepository.search).toHaveBeenCalledWith("cat-1");
        });

        it("カテゴリID未指定のときundefinedを渡す", async () => {
            await service.getSearchView();

            expect(productRepository.search).toHaveBeenCalledWith(undefined);
        });
    });

    describe("search:商品を検索する", () => {
        it("商品リポジトリのみを呼び出す", async () => {
            const result = await service.search("cat-1");

            expect(productRepository.search).toHaveBeenCalledWith("cat-1");
            expect(categoryRepository.search).not.toHaveBeenCalled();
            expect(result).toHaveLength(1);
        });
    });

    describe("getCategories:カテゴリ一覧を取得する", () => {
        it("カテゴリリポジトリのみを呼び出す", async () => {
            const result = await service.getCategories();

            expect(categoryRepository.search).toHaveBeenCalledOnce();
            expect(productRepository.search).not.toHaveBeenCalled();
            expect(result).toHaveLength(2);
        });
    });

    describe("register:商品を登録する", () => {
        it("商品リポジトリへリクエストをそのまま渡す", async () => {
            const request = {
                name: "新商品",
                price: 500,
                categoryId: "cat-1",
                quantity: 20,
                image: null,
            };

            const result = await service.register(request);

            expect(productRepository.register).toHaveBeenCalledWith(request);
            expect(result.productId).toBe("prod-1");
        });
    });

    describe("update:商品を修正する", () => {
        it("商品リポジトリへリクエストをそのまま渡す", async () => {
            const request = {
                productId: "prod-1",
                name: "修正後",
                price: 550,
                categoryId: "cat-1",
                quantity: 30,
                image: null,
            };

            await service.update(request);

            expect(productRepository.update).toHaveBeenCalledWith(request);
        });
    });

    describe("delete:商品を削除する", () => {
        it("商品リポジトリへ商品IDを渡し、削除された商品を返す", async () => {
            const result = await service.delete("prod-1");

            expect(productRepository.delete).toHaveBeenCalledWith("prod-1");
            expect(result.isDeleted).toBe(true);
        });
    });
});