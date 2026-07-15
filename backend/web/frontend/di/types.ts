/**
 * InversifyJSで使う識別子
 * インターフェイスは型情報しか持たず実行時に消えるため、Symbolをキーとして用いる
 */
export const TYPES = {
    HttpClient: Symbol.for("HttpClient"),
    // リポジトリ
    ProductRepository: Symbol.for("ProductRepository"),
    CategoryRepository: Symbol.for("CategoryRepository"),
    OrderRepository: Symbol.for("OrderRepository"),
    OrderStatusRepository: Symbol.for("OrderStatusRepository"),
    EmployeeRepository: Symbol.for("EmployeeRepository"),
    EmployeeAccountRepository: Symbol.for("EmployeeAccountRepository"),
    AuthRepository: Symbol.for("AuthRepository"),
    // サービス
    ProductService: Symbol.for("ProductService"),
    CategoryService: Symbol.for("CategoryService"),
    OrderService: Symbol.for("OrderService"),
    EmployeeAccountService: Symbol.for("EmployeeAccountService"),
} as const;