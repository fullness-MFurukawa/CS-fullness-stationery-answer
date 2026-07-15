import "reflect-metadata";
import { Container } from "inversify";
import { TYPES } from "./types";
import { HttpClient } from "@/infrastructure/http/httpClient";
import type { IProductRepository } from "@/interfaces/repository/productRepository";
import type { ICategoryRepository } from "@/interfaces/repository/categoryRepository";
import type { IOrderRepository } from "@/interfaces/repository/orderRepository";
import type { IOrderStatusRepository } from "@/interfaces/repository/orderStatusRepository";
import type { IEmployeeRepository } from "@/interfaces/repository/employeeRepository";
import type { IEmployeeAccountRepository } from "@/interfaces/repository/employeeAccountRepository";
import type { IAuthRepository } from "@/interfaces/repository/authRepository";
import { ProductRepository } from "@/infrastructure/repository/productRepository";
import { CategoryRepository } from "@/infrastructure/repository/categoryRepository";
import { OrderRepository } from "@/infrastructure/repository/orderRepository";
import { OrderStatusRepository } from "@/infrastructure/repository/orderStatusRepository";
import { EmployeeRepository } from "@/infrastructure/repository/employeeRepository";
import { EmployeeAccountRepository } from "@/infrastructure/repository/employeeAccountRepository";
import { AuthRepository } from "@/infrastructure/repository/authRepository";
import { getSession } from "next-auth/react";
import type { IProductService } from "@/interfaces/service/productService";
import type { ICategoryService } from "@/interfaces/service/categoryService";
import type { IOrderService } from "@/interfaces/service/orderService";
import type { IEmployeeAccountService } from "@/interfaces/service/employeeAccountService";
import { ProductService } from "@/service/productService";
import { CategoryService } from "@/service/categoryService";
import { OrderService } from "@/service/orderService";
import { EmployeeAccountService } from "@/service/employeeAccountService"

/**
 * DIコンテナ
 * 各インターフェイスに実装を束縛する。
 * APIのベースURLは空文字とし、Next.jsのrewritesによるProxy経由で
 * 同一オリジンの相対パスへリクエストする。
 */
const container = new Container();


// リポジトリ
container.bind<IProductRepository>(TYPES.ProductRepository).to(ProductRepository);
container.bind<ICategoryRepository>(TYPES.CategoryRepository).to(CategoryRepository);
container.bind<IOrderRepository>(TYPES.OrderRepository).to(OrderRepository);
container.bind<IOrderStatusRepository>(TYPES.OrderStatusRepository).to(OrderStatusRepository);
container.bind<IEmployeeRepository>(TYPES.EmployeeRepository).to(EmployeeRepository);
container.bind<IEmployeeAccountRepository>(TYPES.EmployeeAccountRepository).to(EmployeeAccountRepository);
container.bind<IAuthRepository>(TYPES.AuthRepository).to(AuthRepository);

// API通信の共通クライアント
// ベースURLは空（Proxy経由の相対パス）、トークンはNextAuthのセッションから取得する
container.bind<HttpClient>(TYPES.HttpClient).toConstantValue(
    new HttpClient("", async () => {
        const session = await getSession();
        console.log("tokenProvider session:", session);
        console.log("tokenProvider token:", session?.accessToken);
        return session?.accessToken;
    }),
);

// サービス
container.bind<IProductService>(TYPES.ProductService).to(ProductService);
container.bind<ICategoryService>(TYPES.CategoryService).to(CategoryService);
container.bind<IOrderService>(TYPES.OrderService).to(OrderService);
container.bind<IEmployeeAccountService>(TYPES.EmployeeAccountService).to(EmployeeAccountService);

export { container };