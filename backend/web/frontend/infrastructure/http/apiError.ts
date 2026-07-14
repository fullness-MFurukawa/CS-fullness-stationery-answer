/**
 * API 呼び出しで発生したエラーを表す例外
 * バックエンドが返す ProblemDetails（status・title・detail）を保持する。
 */
export class ApiError extends Error {
    /**
     * @param status HTTPステータスコード
     * @param title エラーの概要（ProblemDetails の title）
     * @param detail エラーの詳細（ProblemDetails の detail）。無い場合は undefined
     */
    constructor(
        public readonly status: number,
        public readonly title: string,
        public readonly detail?: string,
    ) {
        super(detail ?? title);
        this.name = "ApiError";
    }

    /** 認証エラー（401）かどうか */
    get isUnauthorized(): boolean {
        return this.status === 401;
    }

    /** 対象が見つからない（404）かどうか */
    get isNotFound(): boolean {
        return this.status === 404;
    }

    /** 重複エラー（409）かどうか */
    get isConflict(): boolean {
        return this.status === 409;
    }

    /** 入力検証エラー（400）かどうか */
    get isValidationError(): boolean {
        return this.status === 400;
    }
}