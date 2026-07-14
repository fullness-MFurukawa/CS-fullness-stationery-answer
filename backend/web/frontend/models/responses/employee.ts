/**
 * 社員（バックエンドの EmployeeResponse に対応）
 */
export interface Employee {
    employeeId: string;         // 社員Id(uuid)
    name: string;               // 社員名
    nameKana: string | null;    // 社員名カナ
    departmentName: string;     // 部署名
}