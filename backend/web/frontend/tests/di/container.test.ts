import { describe, it, expect } from "vitest";
import { container } from "@/di/container";
import { TYPES } from "@/di/types";
import { ProductRepository } from "@/infrastructure/repository/productRepository";
import { CategoryRepository } from "@/infrastructure/repository/categoryRepository";
import { OrderRepository } from "@/infrastructure/repository/orderRepository";
import { OrderStatusRepository } from "@/infrastructure/repository/orderStatusRepository";
import { EmployeeRepository } from "@/infrastructure/repository/employeeRepository";
import { EmployeeAccountRepository } from "@/infrastructure/repository/employeeAccountRepository";
import { AuthRepository } from "@/infrastructure/repository/authRepository";
import { HttpClient } from "@/infrastructure/http/httpClient";
import { CategoryService } from "@/service/categoryService";
import { ProductService } from "@/service/productService";
import { OrderService } from "@/service/orderService";
import { EmployeeAccountService } from "@/service/employeeAccountService";

describe("DIコンテナの検証", () => {
    it("HttpClientを取得できる", () => {
        const client = container.get<HttpClient>(TYPES.HttpClient);
        expect(client).toBeInstanceOf(HttpClient);
    });

    it("すべてのリポジトリを取得できる", () => {
        expect(container.get(TYPES.ProductRepository)).toBeInstanceOf(ProductRepository);
        expect(container.get(TYPES.CategoryRepository)).toBeInstanceOf(CategoryRepository);
        expect(container.get(TYPES.OrderRepository)).toBeInstanceOf(OrderRepository);
        expect(container.get(TYPES.OrderStatusRepository)).toBeInstanceOf(OrderStatusRepository);
        expect(container.get(TYPES.EmployeeRepository)).toBeInstanceOf(EmployeeRepository);
        expect(container.get(TYPES.EmployeeAccountRepository)).toBeInstanceOf(EmployeeAccountRepository);
        expect(container.get(TYPES.AuthRepository)).toBeInstanceOf(AuthRepository);
    });


    it("すべてのサービスを取得できる", () => {
        expect(container.get(TYPES.ProductService)).toBeInstanceOf(ProductService);
        expect(container.get(TYPES.CategoryService)).toBeInstanceOf(CategoryService);
        expect(container.get(TYPES.OrderService)).toBeInstanceOf(OrderService);
        expect(container.get(TYPES.EmployeeAccountService)).toBeInstanceOf(EmployeeAccountService);
    });
});