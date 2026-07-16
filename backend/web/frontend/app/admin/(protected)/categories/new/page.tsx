import { CategoryRegisterForm } from "@/components/admin/categories/categoryRegisterForm";

/**
 * BP019 商品カテゴリ登録画面
 * URL: /admin/categories/new
 */
export default function CategoryNewPage() {
  return (
    <div className="mx-auto max-w-xl space-y-6">
      <div>
        <h1 className="text-2xl font-bold">商品カテゴリ登録</h1>
        <p className="mt-1 text-muted-foreground">
          新しい商品カテゴリを登録します
        </p>
      </div>
      <CategoryRegisterForm />
    </div>
  );
}