/**
 * 商品（バックエンドの ProductResponse に対応）
 */
export interface Product {
    productId: string;        // 商品Id(uuid)
    name: string;             // 商品名
    price: number;            // 単価
    imageUrl: string | null;  // 画像URL
    quantity: number;         // 在庫数
    categoryId: string;       // カテゴリId(uuid)
    categoryName: string;     // カテゴリ名
    isDeleted: boolean;       // 削除フラグS
}