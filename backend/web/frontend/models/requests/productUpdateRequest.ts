/**
 * 商品修正のリクエスト（バックエンドの ProductUpdateRequest に対応）
 * 画像を含むため multipart/form-data で送信する。
 * image を指定した場合は差し替え、removeImage が true の場合は削除、
 * どちらも指定しない場合は既存の画像を維持する。
 */
export interface ProductUpdateRequest {
  productId: string;       // uuid（URLパスに使用）
  name: string;
  price: number;
  categoryId: string;      // uuid
  quantity: number;
  image: File | null;
  removeImage: boolean;
}