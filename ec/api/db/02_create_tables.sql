-- ============================================================
-- Fullness Stationery EC システム  総合開発演習
-- 02_create_tables.sql : テーブル定義 (DDL)
-- 対象DBMS : PostgreSQL 17 (WSL2)
-- ============================================================
--
-- 【実行方法】fullness_ec に接続した状態で実行します。
--   psql -U postgres -d fullness_ec -f 02_create_tables.sql
--   （または psql 接続後  \c fullness_ec  →  \i 02_create_tables.sql ）
--
-- 【設計メモ】仕様書の表記ゆれは以下の方針で統一しています。
--   * customer の登録日カラム : created_at (テーブル定義書に準拠)
--   * employee のカナ         : name_kana (ER図・customer と統一)
--   * product の削除フラグ    : delete_flg / パスワード桁 : VARCHAR(255)
--   * phone_number            : VARCHAR(20) (VRACHAR は誤記と判断)
--   * customer に name_kana(VARCHAR20) を追加 (ER図・FP003 で必須。定義書では欠落)
--   * orders_detail は定義書に準拠し customer_id を持たない
--   * UUID列は PostgreSQL17 標準の gen_random_uuid() を既定値に設定 (拡張不要)
-- ============================================================

-- 冪等に作り直せるよう、依存関係の逆順で既存テーブルを削除
DROP TABLE IF EXISTS orders_detail   CASCADE;
DROP TABLE IF EXISTS orders          CASCADE;
DROP TABLE IF EXISTS payment_method  CASCADE;
DROP TABLE IF EXISTS order_status    CASCADE;
DROP TABLE IF EXISTS product_stock   CASCADE;
DROP TABLE IF EXISTS product         CASCADE;
DROP TABLE IF EXISTS product_category CASCADE;
DROP TABLE IF EXISTS employee_account CASCADE;
DROP TABLE IF EXISTS employee        CASCADE;
DROP TABLE IF EXISTS department      CASCADE;
DROP TABLE IF EXISTS customer        CASCADE;

-- ------------------------------------------------------------
-- 部署 (department)
-- ------------------------------------------------------------
CREATE TABLE department (
    id              SERIAL       PRIMARY KEY,
    department_uuid UUID         NOT NULL DEFAULT gen_random_uuid() UNIQUE,
    name            VARCHAR(100) NOT NULL
);
COMMENT ON TABLE  department      IS '部署テーブル';
COMMENT ON COLUMN department.id              IS '部署ID';
COMMENT ON COLUMN department.department_uuid IS '部署識別ID';
COMMENT ON COLUMN department.name            IS '部署名';

-- ------------------------------------------------------------
-- 社員 (employee)
-- ------------------------------------------------------------
CREATE TABLE employee (
    id            SERIAL       PRIMARY KEY,
    employee_uuid UUID         NOT NULL DEFAULT gen_random_uuid() UNIQUE,
    name          VARCHAR(100) NOT NULL,
    name_kana     VARCHAR(100),
    department_id INT          NOT NULL
        REFERENCES department (id)
);
COMMENT ON TABLE  employee        IS '社員テーブル';
COMMENT ON COLUMN employee.id            IS '社員ID';
COMMENT ON COLUMN employee.employee_uuid IS '社員識別ID';
COMMENT ON COLUMN employee.name          IS '社員名';
COMMENT ON COLUMN employee.name_kana     IS '社員名カナ';
COMMENT ON COLUMN employee.department_id IS '部署ID(外部キー)';

-- ------------------------------------------------------------
-- 社員アカウント (employee_account)  ※管理者ログイン用
-- ------------------------------------------------------------
CREATE TABLE employee_account (
    id           SERIAL       PRIMARY KEY,
    account_uuid UUID         NOT NULL DEFAULT gen_random_uuid() UNIQUE,
    name         VARCHAR(20)  NOT NULL UNIQUE,
    password     VARCHAR(255) NOT NULL,
    employee_id  INT          NOT NULL
        REFERENCES employee (id)
);
COMMENT ON TABLE  employee_account IS '社員アカウントテーブル';
COMMENT ON COLUMN employee_account.id           IS 'アカウントID';
COMMENT ON COLUMN employee_account.account_uuid IS 'アカウント識別ID';
COMMENT ON COLUMN employee_account.name         IS 'アカウント名';
COMMENT ON COLUMN employee_account.password     IS 'パスワード(ハッシュ値)';
COMMENT ON COLUMN employee_account.employee_id  IS '社員ID(外部キー)';

-- ------------------------------------------------------------
-- 商品カテゴリ (product_category)
-- ------------------------------------------------------------
CREATE TABLE product_category (
    id            SERIAL      PRIMARY KEY,
    category_uuid UUID        NOT NULL DEFAULT gen_random_uuid() UNIQUE,
    name          VARCHAR(30) NOT NULL
);
COMMENT ON TABLE  product_category IS '商品カテゴリテーブル';
COMMENT ON COLUMN product_category.id            IS '商品カテゴリID';
COMMENT ON COLUMN product_category.category_uuid IS '商品カテゴリ識別ID';
COMMENT ON COLUMN product_category.name          IS '商品カテゴリ名';

-- ------------------------------------------------------------
-- 商品 (product)
-- ------------------------------------------------------------
CREATE TABLE product (
    id                  SERIAL       PRIMARY KEY,
    product_uuid        UUID         NOT NULL DEFAULT gen_random_uuid() UNIQUE,
    name                VARCHAR(100) NOT NULL,
    price               INT          NOT NULL,
    image_url           VARCHAR(200),
    product_category_id INT          NOT NULL
        REFERENCES product_category (id),
    delete_flg          INT          NOT NULL DEFAULT 0
);
COMMENT ON TABLE  product          IS '商品テーブル';
COMMENT ON COLUMN product.id                  IS '商品ID';
COMMENT ON COLUMN product.product_uuid        IS '商品識別ID';
COMMENT ON COLUMN product.name                IS '商品名';
COMMENT ON COLUMN product.price               IS '価格';
COMMENT ON COLUMN product.image_url           IS '画像URL';
COMMENT ON COLUMN product.product_category_id IS '商品カテゴリID(外部キー)';
COMMENT ON COLUMN product.delete_flg          IS '削除フラグ(0:通常, 1:削除)';

-- ------------------------------------------------------------
-- 商品在庫 (product_stock)
-- ------------------------------------------------------------
CREATE TABLE product_stock (
    id         SERIAL PRIMARY KEY,
    stock_uuid UUID   NOT NULL DEFAULT gen_random_uuid() UNIQUE,
    quantity   INT    NOT NULL DEFAULT 0,
    product_id INT    NOT NULL UNIQUE
        REFERENCES product (id)
);
COMMENT ON TABLE  product_stock    IS '商品在庫テーブル';
COMMENT ON COLUMN product_stock.id         IS '商品在庫ID';
COMMENT ON COLUMN product_stock.stock_uuid IS '商品在庫識別ID';
COMMENT ON COLUMN product_stock.quantity   IS '商品在庫数';
COMMENT ON COLUMN product_stock.product_id IS '商品ID(外部キー)';

-- ------------------------------------------------------------
-- 注文ステータス (order_status)  ※マスタ
-- ------------------------------------------------------------
CREATE TABLE order_status (
    id   SERIAL       PRIMARY KEY,
    name VARCHAR(100) NOT NULL
);
COMMENT ON TABLE  order_status     IS '注文ステータステーブル';
COMMENT ON COLUMN order_status.id   IS '注文ステータスID';
COMMENT ON COLUMN order_status.name IS '注文ステータス名';

-- ------------------------------------------------------------
-- 支払い方法 (payment_method)  ※マスタ
-- ------------------------------------------------------------
CREATE TABLE payment_method (
    id   SERIAL       PRIMARY KEY,
    name VARCHAR(100) NOT NULL
);
COMMENT ON TABLE  payment_method   IS '支払い方法テーブル';
COMMENT ON COLUMN payment_method.id   IS '支払い方法ID';
COMMENT ON COLUMN payment_method.name IS '支払い方法名';

-- ------------------------------------------------------------
-- 顧客 (customer)  ※顧客ログイン用
-- ------------------------------------------------------------
CREATE TABLE customer (
    id            SERIAL       PRIMARY KEY,
    customer_uuid UUID         NOT NULL DEFAULT gen_random_uuid() UNIQUE,
    name          VARCHAR(20)  NOT NULL,
    name_kana     VARCHAR(20),
    address1      VARCHAR(100) NOT NULL,
    address2      VARCHAR(100),
    phone_number  VARCHAR(20)  NOT NULL,
    mail_address  VARCHAR(200) NOT NULL UNIQUE,
    username      VARCHAR(30)  NOT NULL UNIQUE,
    password      VARCHAR(255) NOT NULL,
    created_at    TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP
);
COMMENT ON TABLE  customer         IS '顧客(アカウント)テーブル';
COMMENT ON COLUMN customer.id            IS '顧客ID';
COMMENT ON COLUMN customer.customer_uuid IS '顧客識別ID';
COMMENT ON COLUMN customer.name          IS '顧客名';
COMMENT ON COLUMN customer.name_kana     IS '顧客名カナ';
COMMENT ON COLUMN customer.address1      IS '住所1';
COMMENT ON COLUMN customer.address2      IS '住所2';
COMMENT ON COLUMN customer.phone_number  IS '電話番号';
COMMENT ON COLUMN customer.mail_address  IS 'メールアドレス';
COMMENT ON COLUMN customer.username      IS 'アカウント名';
COMMENT ON COLUMN customer.password      IS 'パスワード(ハッシュ値)';
COMMENT ON COLUMN customer.created_at    IS '登録日';

-- ------------------------------------------------------------
-- 注文 (orders)  ※ orders は予約語のため複数形
-- ------------------------------------------------------------
CREATE TABLE orders (
    id                SERIAL    PRIMARY KEY,
    order_uuid        UUID      NOT NULL DEFAULT gen_random_uuid() UNIQUE,
    order_date        TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    amount_total      INT       NOT NULL,
    customer_id       INT       NOT NULL
        REFERENCES customer (id),
    order_status_id   INT       NOT NULL
        REFERENCES order_status (id),
    payment_method_id INT       NOT NULL
        REFERENCES payment_method (id)
);
COMMENT ON TABLE  orders           IS '注文テーブル';
COMMENT ON COLUMN orders.id                IS '注文ID';
COMMENT ON COLUMN orders.order_uuid        IS '注文識別ID';
COMMENT ON COLUMN orders.order_date        IS '注文日';
COMMENT ON COLUMN orders.amount_total      IS '合計金額';
COMMENT ON COLUMN orders.customer_id       IS '顧客ID(外部キー)';
COMMENT ON COLUMN orders.order_status_id   IS '注文ステータスID(外部キー)';
COMMENT ON COLUMN orders.payment_method_id IS '支払い方法ID(外部キー)';

-- ------------------------------------------------------------
-- 注文明細 (orders_detail)
-- ------------------------------------------------------------
CREATE TABLE orders_detail (
    id         SERIAL PRIMARY KEY,
    order_id   INT    NOT NULL
        REFERENCES orders (id),
    product_id INT    NOT NULL
        REFERENCES product (id),
    count      INT    NOT NULL
);
COMMENT ON TABLE  orders_detail    IS '注文明細テーブル';
COMMENT ON COLUMN orders_detail.id         IS '注文明細ID';
COMMENT ON COLUMN orders_detail.order_id   IS '注文ID(外部キー)';
COMMENT ON COLUMN orders_detail.product_id IS '商品ID(外部キー)';
COMMENT ON COLUMN orders_detail.count      IS '注文数';

-- 参照用インデックス
CREATE INDEX idx_employee_department       ON employee (department_id);
CREATE INDEX idx_employee_account_employee ON employee_account (employee_id);
CREATE INDEX idx_product_category          ON product (product_category_id);
CREATE INDEX idx_orders_customer           ON orders (customer_id);
CREATE INDEX idx_orders_status             ON orders (order_status_id);
CREATE INDEX idx_orders_detail_order       ON orders_detail (order_id);
CREATE INDEX idx_orders_detail_product     ON orders_detail (product_id);
