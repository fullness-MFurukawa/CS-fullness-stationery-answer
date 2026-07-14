/**
 * 担当者アカウント（バックエンドの EmployeeAccountResponse に対応）
 */
export interface EmployeeAccount {
    accountId: string;      // アカウントId(uuid)
    accountName: string;    // アカウント名
    employeeName: string;   // 従業員名
}