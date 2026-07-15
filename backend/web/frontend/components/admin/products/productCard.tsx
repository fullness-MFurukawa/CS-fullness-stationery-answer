"use client";

import Image from "next/image";
import Link from "next/link";
import { ImageOff, Pencil, Trash2 } from "lucide-react";
import type { Product } from "@/models/responses/product";
import { Card, CardContent, CardFooter } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";

/**
 * 商品カード
 * 商品検索画面（BP006）で、商品1件を表示する。
 * @param product 表示する商品
 * @param onDelete 削除ボタンが押されたときの処理
 */
export function ProductCard({
  product,
  onDelete,
}: {
  product: Product;
  onDelete: (product: Product) => void;
}) {
  return (
    <Card className="flex flex-col overflow-hidden pt-0">
      <div className="relative aspect-square bg-muted">
        {product.imageUrl ? (
          <Image
            src={product.imageUrl}
            alt={product.name}
            fill
            sizes="(max-width: 768px) 50vw, (max-width: 1200px) 33vw, 25vw"
            className="object-contain p-4"
          />
        ) : (
          <div className="flex size-full items-center justify-center text-muted-foreground">
            <ImageOff className="size-10" />
          </div>
        )}
      </div>

      <CardContent className="flex-1 space-y-2">
        <Badge variant="secondary">{product.categoryName}</Badge>
        <h3 className="font-medium leading-tight">{product.name}</h3>
        <div className="flex items-baseline justify-between">
          <p className="text-lg font-bold">
            {product.price.toLocaleString()}
            <span className="ml-0.5 text-sm font-normal">円</span>
          </p>
          <p className="text-sm text-muted-foreground">
            在庫 {product.quantity.toLocaleString()} 個
          </p>
        </div>
      </CardContent>

      <CardFooter className="gap-2">
        <Button
          variant="outline"
          size="sm"
          className="flex-1"
          nativeButton={false}
          render={
            <Link href={`/admin/products/${product.productId}/edit`}>
              <Pencil />
              修正
            </Link>
          }
        />
        <Button
          variant="outline"
          size="sm"
          className="flex-1 text-destructive hover:text-destructive"
          onClick={() => onDelete(product)}
        >
          <Trash2 />
          削除
        </Button>
      </CardFooter>
    </Card>
  );
}