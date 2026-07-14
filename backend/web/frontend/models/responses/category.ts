/**
 * 商品カテゴリ（バックエンドの CategoryResponse に対応）
 */
export interface Category {
  categoryId: string;   // カテゴリId(uuid)
  name: string;         // カテゴリ名
}