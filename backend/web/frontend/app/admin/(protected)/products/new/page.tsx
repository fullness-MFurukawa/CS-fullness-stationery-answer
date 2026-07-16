import { ProductRegisterForm } from "@/components/admin/products/productRegisterForm";

/**
 * BP012 新商品登録画面
 * URL: /admin/products/new
 */
export default function ProductNewPage() {
  return (
    <div className="mx-auto max-w-2xl space-y-6">
      <div>
        <h1 className="text-2xl font-bold">新商品登録</h1>
        <p className="mt-1 text-muted-foreground">新しい商品を登録します</p>
      </div>
      <ProductRegisterForm />
    </div>
  );
}