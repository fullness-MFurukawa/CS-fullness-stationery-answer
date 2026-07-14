/**
 * 注文明細（バックエンドの OrderDetailResponse に対応）
 */
export interface OrderDetail {
    productName: string;  //  商品名
    price: number;        //  単価
    count: number;        //  数量
    subtotal: number;     //  小計
}