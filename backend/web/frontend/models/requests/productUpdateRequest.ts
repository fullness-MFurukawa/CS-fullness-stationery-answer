/**
 * 商品修正のリクエスト（バックエンドの ProductUpdateRequest に対応）
 * 画像を含むため multipart/form-data で送信する。
 * image が null の場合は既存の画像を維持する。
 */
export interface ProductUpdateRequest {
    productId: string;      // 商品Id(uuid（URLパスに使用)
    name: string;           // 商品名
    price: number;          // 単価
    categoryId: string;     // カテゴリId(uuid)
    quantity: number;       // 在庫数
    image: File | null;     // 画像ファイル
}