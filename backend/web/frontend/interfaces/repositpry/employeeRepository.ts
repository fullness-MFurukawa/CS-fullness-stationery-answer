import type { Employee } from "@/models/responses/employee";

/**
 * 社員に関するAPI呼び出しを抽象化するリポジトリ
 * バックエンドの /api/admin/employees エンドポイント群に対応する。
 */
export interface IEmployeeRepository {
    /**
   　* アカウントが未登録の社員をすべて取得する
   　* @returns アカウント未登録の社員一覧
   　*/
    searchWithoutAccount(): Promise<Employee[]>;
}