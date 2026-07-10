-- ============================================================
-- Fullness Stationery EC システム  総合開発演習
-- 03_insert_sample_data.sql : 演習用サンプルデータ
-- 対象DBMS : PostgreSQL 17 (WSL2)
-- ============================================================
--
-- 【実行方法】fullness_ec に接続した状態で実行します。
--   psql -U postgres -d fullness_ec -f 03_insert_sample_data.sql
--
-- 【パスワードについて】
--   employee_account / customer のパスワードは
--   ASP.NET Core Identity PasswordHasher<T> (PBKDF2 / v3形式) の実ハッシュです。
--   すべて平文は  "Password123"  です。
--   ハッシュ内にアルゴリズム・反復回数・ソルトが埋め込まれているため、
--   PasswordHasher.VerifyHashedPassword() はプロジェクトの反復回数設定に
--   依存せず検証に成功します。
-- ============================================================

-- 再投入できるよう既存データを全削除 (子→親の順)
TRUNCATE TABLE
    orders_detail, orders, product_stock, product,
    product_category, employee_account, employee, department,
    customer, order_status, payment_method
    RESTART IDENTITY CASCADE;

-- ------------------------------------------------------------
-- 部署 (department)
-- ------------------------------------------------------------
INSERT INTO department (id, name) VALUES
    (1, '営業部'),
    (2, '販売管理部'),
    (3, '商品企画部');

-- ------------------------------------------------------------
-- 社員 (employee)
-- ------------------------------------------------------------
INSERT INTO employee (id, name, name_kana, department_id) VALUES
    (1, 'フルネス太郎', 'フルネスタロウ', 2),
    (2, '鈴木花子',     'スズキハナコ',   3),
    (3, '山本次郎',     'ヤマモトジロウ', 2),
    (4, '佐藤三郎',     'サトウサブロウ', 2);  -- アカウント未登録(BP003の選択肢確認用)

-- ------------------------------------------------------------
-- 社員アカウント (employee_account)  ※平文 "Password123"
-- ------------------------------------------------------------
INSERT INTO employee_account (id, name, password, employee_id) VALUES
    (1, 'fullness', 'AQAAAAEAAYagAAAAEP7skW1gcyhyAYGBae+WVI0u5as7DgmHJQc+ef75AOg8H7whgZ8XxbU4/yk3yRKpAQ==', 1);

-- ------------------------------------------------------------
-- 商品カテゴリ (product_category)
-- ------------------------------------------------------------
INSERT INTO product_category (id, name) VALUES
    (1, '文房具'),
    (2, 'パソコン周辺機器'),
    (3, '雑貨');

-- ------------------------------------------------------------
-- 商品 (product)
-- ------------------------------------------------------------
-- image_url は Azure Blob Storage 登録後にURLを設定するため、現在は NULL(空) とする
INSERT INTO product (id, name, price, image_url, product_category_id, delete_flg) VALUES
    (1,  '水性ボールペン(黒)',       120, NULL, 1, 0),
    (2,  '水性ボールペン(赤)',       120, NULL, 1, 0),
    (3,  '水性ボールペン(青)',       120, NULL, 1, 0),
    (4,  '油性ボールペン(黒)',       100, NULL, 1, 0),
    (5,  '鉛筆(黒)',                 100, NULL, 1, 0),
    (6,  'ワイヤレスマウス',         900, NULL, 2, 0),
    (7,  'ワイヤレストラックボール', 1300,NULL, 2, 0),
    (8,  '有線光学式マウス',         500, NULL, 2, 0),
    (9,  '有線ゲーミングマウス',     3800,NULL, 2, 0),
    (10, 'USB有線式キーボード',      1400,NULL, 2, 0),
    (11, '無線式キーボード',         1900,NULL, 2, 0),
    (12, '付箋(イエロー)',          200, NULL, 3, 0),
    (13, 'クリアファイル',           150, NULL, 3, 0),
    (14, '廃番ボールペン',           110, NULL, 1, 1); -- 論理削除済(delete_flg=1)

-- ------------------------------------------------------------
-- 商品在庫 (product_stock)
-- ------------------------------------------------------------
INSERT INTO product_stock (id, quantity, product_id) VALUES
    (1,  10,  1),
    (2,  50,  2),
    (3,  50,  3),
    (4,  100, 4),
    (5,  200, 5),
    (6,  30,  6),
    (7,  15,  7),
    (8,  40,  8),
    (9,  8,   9),
    (10, 25,  10),
    (11, 20,  11),
    (12, 300, 12),
    (13, 150, 13),
    (14, 0,   14);

-- ------------------------------------------------------------
-- 注文ステータス (order_status)  ※マスタ
-- ------------------------------------------------------------
INSERT INTO order_status (id, name) VALUES
    (1, '注文済'),
    (2, '入金済'),
    (3, '配送中'),
    (4, '完了');

-- ------------------------------------------------------------
-- 支払い方法 (payment_method)  ※マスタ (現時点では現金のみ使用)
-- ------------------------------------------------------------
INSERT INTO payment_method (id, name) VALUES
    (1, '現金'),
    (2, 'クレジットカード'),
    (3, '銀行振込');

-- ------------------------------------------------------------
-- 顧客 (customer)  ※平文 "Password123"
-- ------------------------------------------------------------
INSERT INTO customer
    (id, name, name_kana, address1, address2, phone_number, mail_address, username, password) VALUES
    (1, 'テスト顧客', 'テストコキャク', '東京都新宿区', 'テストビル101',
        '090-1234-5678', 'test@example.com', 'testuser',
        'AQAAAAEAAYagAAAAEDWHnUvoQFQDSOJrgQyxH7Tt3sXRqOt2qtfqhj9r5aiYvaRqq/QHmyjV8U5HDUlMVg=='),
    (2, '山田太郎', 'ヤマダタロウ', '東京都渋谷区1-11-11', 'マンション渋谷101号室',
        '03-1111-2222', 'taro@example.com', 'taro123',
        'AQAAAAEAAYagAAAAEGCcwoiMdpE7wAVL05VbweO4n2tq/igkUbmmH5SL3O0JJ7JjB0YciwirJRH4CV1+DQ==');

-- ------------------------------------------------------------
-- 注文 (orders)  ※ BP015 購入履歴一覧のモックに対応
-- ------------------------------------------------------------
INSERT INTO orders
    (id, order_date, amount_total, customer_id, order_status_id, payment_method_id) VALUES
    (1, '2024-05-10 10:00:00', 340,  1, 4, 1),  -- 完了
    (2, '2024-05-12 15:30:00', 100,  1, 3, 1),  -- 配送中
    (3, '2024-05-13 09:00:00', 120,  1, 4, 1),  -- 完了
    (4, '2025-05-12 23:58:16', 3800, 2, 1, 1);  -- 注文済 (FP012モック対応)

-- ------------------------------------------------------------
-- 注文明細 (orders_detail)
-- ------------------------------------------------------------
INSERT INTO orders_detail (id, order_id, product_id, count) VALUES
    (1, 1, 1, 2),   -- 注文1: 水性ボールペン(黒) x2 = 240
    (2, 1, 4, 1),   -- 注文1: 油性ボールペン(黒) x1 = 100  (合計340)
    (3, 2, 5, 1),   -- 注文2: 鉛筆(黒) x1 = 100
    (4, 3, 2, 1),   -- 注文3: 水性ボールペン(赤) x1 = 120
    (5, 4, 9, 1);   -- 注文4: 有線ゲーミングマウス x1 = 3800

-- ------------------------------------------------------------
-- SERIAL シーケンスを最大ID+1へ補正 (明示IDで投入したため)
-- ------------------------------------------------------------
SELECT setval('department_id_seq',       (SELECT MAX(id) FROM department));
SELECT setval('employee_id_seq',         (SELECT MAX(id) FROM employee));
SELECT setval('employee_account_id_seq', (SELECT MAX(id) FROM employee_account));
SELECT setval('product_category_id_seq', (SELECT MAX(id) FROM product_category));
SELECT setval('product_id_seq',          (SELECT MAX(id) FROM product));
SELECT setval('product_stock_id_seq',    (SELECT MAX(id) FROM product_stock));
SELECT setval('order_status_id_seq',     (SELECT MAX(id) FROM order_status));
SELECT setval('payment_method_id_seq',   (SELECT MAX(id) FROM payment_method));
SELECT setval('customer_id_seq',         (SELECT MAX(id) FROM customer));
SELECT setval('orders_id_seq',           (SELECT MAX(id) FROM orders));
SELECT setval('orders_detail_id_seq',    (SELECT MAX(id) FROM orders_detail));
