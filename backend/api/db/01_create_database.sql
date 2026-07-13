
01 create database · SQL
-- ============================================================
-- Fullness Stationery EC システム  総合開発演習
-- 01_create_database.sql : データベース作成
-- 対象DBMS : PostgreSQL 17 (WSL2)
-- ============================================================
--
-- 【実行方法】WSL2 のターミナルから postgres ユーザーで実行します。
--   sudo -u postgres psql -f 01_create_database.sql
--   （または psql に接続後  \i 01_create_database.sql ）
--
-- ※ データベースを作り直したい場合は、先に下記コメントを外して削除します。
-- DROP DATABASE IF EXISTS fullness_ec;
 
-- ※ WSL2 に ja_JP.UTF-8 ロケールが無い環境向けに、ロケール指定は付けていません。
--   日本語データは ENCODING=UTF8 で問題なく保存できます。
--   もし ja_JP.UTF-8 で照合順序を厳密にしたい場合は、先に WSL2 側で
--     sudo locale-gen ja_JP.UTF-8 && sudo service postgresql restart
--   を実行してから、下記に  LC_COLLATE='ja_JP.UTF-8' LC_CTYPE='ja_JP.UTF-8'  を追加してください。
CREATE DATABASE fullness_ec
    WITH
    ENCODING = 'UTF8'
    TEMPLATE = template0;
 
COMMENT ON DATABASE fullness_ec IS 'Fullness Stationery 文具/雑貨販売ECシステム 演習用DB';
 
-- 作成後、以下のコマンドで fullness_ec に接続し 02, 03 を実行してください。
--   \c fullness_ec
--   \i 02_create_tables.sql
--   \i 03_insert_sample_data.sql