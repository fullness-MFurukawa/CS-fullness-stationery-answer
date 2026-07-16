import { EmployeeAccountRegisterForm } from "@/components/admin/employee-accounts/employeeAccountRegisterForm";

/**
 * BP003 担当者アカウント登録画面
 * URL: /admin/employee-accounts/new
 */
export default function EmployeeAccountNewPage() {
  return (
    <div className="mx-auto max-w-xl space-y-6">
      <div>
        <h1 className="text-2xl font-bold">担当者アカウント登録</h1>
        <p className="mt-1 text-muted-foreground">
          担当者のログイン用アカウントを登録します
        </p>
      </div>
      <EmployeeAccountRegisterForm />
    </div>
  );
}