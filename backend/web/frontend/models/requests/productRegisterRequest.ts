/**
 * 新商品登録のリクエスト（バックエンドの ProductRegisterRequest に対応）
 * 画像を含むため multipart/form-data で送信する。
 */
export interface ProductRegisterRequest {
    name: string;           // 商品名
    price: number;          // 単価
    categoryId: string;     // カテゴリId(uuid)
    quantity: number;       // 在庫数
    image: File | null;     // 画像ファイル、未選択の場合は null
}