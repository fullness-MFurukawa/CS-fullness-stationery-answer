import { describe, it, expect, beforeEach, vi } from "vitest";
import { CategoryService } from "@/service/categoryService";
import type { ICategoryRepository } from "@/interfaces/repository/categoryRepository";
import type { Category } from "@/models/responses/category";

describe("CategoryServiceの単体テストドライバ", () => {
    let categoryRepository: ICategoryRepository;
    let service: CategoryService;

    const sampleCategories: Category[] = [
        { categoryId: "cat-1", name: "文房具" },
        { categoryId: "cat-2", name: "雑貨" },
    ];

    beforeEach(() => {
        categoryRepository = {
            search: vi.fn().mockResolvedValue(sampleCategories),
            register: vi.fn().mockResolvedValue({ categoryId: "cat-9", name: "画材" }),
        };

        service = new CategoryService(categoryRepository);
    });

    describe("search:商品カテゴリ一覧を取得する", () => {
        it("リポジトリを呼び出し、カテゴリ一覧を返す", async () => {
            const result = await service.search();

            expect(categoryRepository.search).toHaveBeenCalledOnce();
            expect(result).toHaveLength(2);
            expect(result[0].name).toBe("文房具");
        });
    });

    describe("register:商品カテゴリを登録する", () => {
        it("リポジトリへリクエストをそのまま渡し、登録されたカテゴリを返す", async () => {
            const request = { name: "画材" };

            const result = await service.register(request);

            expect(categoryRepository.register).toHaveBeenCalledWith(request);
            expect(result.name).toBe("画材");
        });
    });
});