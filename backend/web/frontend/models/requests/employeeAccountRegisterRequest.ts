/**
 * 担当者アカウント登録のリクエスト（バックエンドの EmployeeAccountRegisterRequest に対応）
 */
export interface EmployeeAccountRegisterRequest {
    employeeId: string;     // 従業員Id(uuid)
    accountName: string;    // アカウント名
    password: string;       // パスワード
}