import { ApiError } from "./apiError";

/**
 * アクセストークンを取得する関数の型
 * 未ログインなどでトークンが無い場合は undefined を返す。
 */
export type TokenProvider = () => Promise<string | undefined>;

/**
 * 認証エラー（401）が発生したときに実行する処理の型
 * セッションの破棄とログイン画面への遷移を想定する。
 */
export type UnauthorizedHandler = () => Promise<void>;

/**
 * バックエンドAPIとの通信を担う共通クライアント
 * ベースURLの付与、認証トークンの付与、エラーのApiErrorへの変換を一元的に行う。
 */
export class HttpClient {
    /**
     * @param baseUrl APIのベースURL。Proxy経由で相対パスを使う場合は空文字
     * @param tokenProvider アクセストークンを取得する関数。省略時は認証ヘッダを付与しない
     * @param onUnauthorized 認証エラー時に実行する処理。省略時は何もしない
     */
    constructor(
        private readonly baseUrl: string = "",
        private readonly tokenProvider?: TokenProvider,
        private readonly onUnauthorized?: UnauthorizedHandler,
    ) {}

    // ... authHeader, get, sendJson, sendForm, sendEmpty, delete は変更なし

    /**
     * レスポンスを検査し、成功ならJSONを返し、失敗ならApiErrorをスローする
     * @param response fetchのレスポンス
     * @returns 成功時のJSON。204などボディが無い場合は undefined を T として返す
     */
    private async handleResponse<T>(response: Response): Promise<T> {
        if (response.ok) {
            if (response.status === 204) {
                return undefined as T;
            }
            const text = await response.text();
            return (text ? JSON.parse(text) : undefined) as T;
        }

        let title = "エラーが発生しました";
        let detail: string | undefined;
        try {
            const problem = await response.json();
            title = problem.title ?? title;
            detail = problem.detail;
        } catch {
            // ボディが無い、またはJSONでない場合（例: 認証失敗の空ボディ401）はステータスのみで扱う
        }

        // 認証エラーの場合、セッションが無効になっているため再ログインを促す
        if (response.status === 401 && this.onUnauthorized) {
            await this.onUnauthorized();
        }

        throw new ApiError(response.status, title, detail);
    }
}