import { ApiError } from "./apiError";

/**
 * バックエンドAPIとの通信を担う共通クライアント
 * ベースURLの付与、認証Cookieの送信、エラーのApiErrorへの変換を一元的に行う。
 */
export class HttpClient {
    /**
     * @param baseUrl APIのベースURL（例: http://localhost:5266）
     */
    constructor(private readonly baseUrl: string) {}

    /**
     * GET リクエストを送信し、JSONを取得する
     * @param path ベースURLからのパス（例: /api/admin/categories）
     * @returns レスポンスのJSONを型 T として返す
     */
    async get<T>(path: string): Promise<T> {
        const response = await fetch(this.baseUrl + path, {
            method: "GET",
            credentials: "include",
            headers: { Accept: "application/json" },
        });
        return this.handleResponse<T>(response);
    }

    /**
     * JSON ボディを伴う POST / PUT リクエストを送信する
     * @param method HTTPメソッド
     * @param path ベースURLからのパス
     * @param body 送信するオブジェクト（JSONへ変換する）
     * @returns レスポンスのJSONを型 T として返す
     */
    async sendJson<T>(method: "POST" | "PUT", path: string, body: unknown): Promise<T> {
        const response = await fetch(this.baseUrl + path, {
            method,
            credentials: "include",
            headers: {
            Accept: "application/json",
            "Content-Type": "application/json",
            },
            body: JSON.stringify(body),
        });
        return this.handleResponse<T>(response);
    }

    /**
     * FormData（multipart/form-data）を伴う POST / PUT リクエストを送信する
     * @param method HTTPメソッド
     * @param path ベースURLからのパス
     * @param formData 送信するFormData（画像ファイルを含む）
     * @returns レスポンスのJSONを型 T として返す
     * @remarks Content-Type はブラウザが境界文字付きで自動設定するため、明示しない。
     */
    async sendForm<T>(method: "POST" | "PUT", path: string, formData: FormData): Promise<T> {
        const response = await fetch(this.baseUrl + path, {
            method,
            credentials: "include",
            headers: { Accept: "application/json" },
            body: formData,
        });
        return this.handleResponse<T>(response);
  }

    /**
     * ボディを返さない POST / DELETE リクエストを送信する（例: ログアウト）
     * @param method HTTPメソッド
     * @param path ベースURLからのパス
     */
    async sendEmpty(method: "POST" | "DELETE", path: string): Promise<void> {
        const response = await fetch(this.baseUrl + path, {
            method,
            credentials: "include",
            headers: { Accept: "application/json" },
        });
        await this.handleResponse<void>(response);
    }

    /**
     * DELETE リクエストを送信し、JSONを取得する（例: 商品削除は削除された商品を返す）
     * @param path ベースURLからのパス
     * @returns レスポンスのJSONを型 T として返す
     */
    async delete<T>(path: string): Promise<T> {
        const response = await fetch(this.baseUrl + path, {
            method: "DELETE",
            credentials: "include",
            headers: { Accept: "application/json" },
        });
        return this.handleResponse<T>(response);
    }

    /**
     * レスポンスを検査し、成功ならJSONを返し、失敗ならApiErrorをスローする
     * @param response fetchのレスポンス
     * @returns 成功時のJSON。204などボディが無い場合は undefined を T として返す
     */
    private async handleResponse<T>(response: Response): Promise<T> {
        if (response.ok) {
            // 204 No Content や空ボディに対応する
            if (response.status === 204) {
                return undefined as T;
            }
            const text = await response.text();
            return (text ? JSON.parse(text) : undefined) as T;
        }

        // エラー時は ProblemDetails を読み取って ApiError に変換する
        let title = "エラーが発生しました";
        let detail: string | undefined;
        try {
            const problem = await response.json();
            title = problem.title ?? title;
            detail = problem.detail;
        } catch {
            // ボディが無い、またはJSONでない場合（例: 認証失敗の空ボディ401）はステータスのみで扱う
        }

        throw new ApiError(response.status, title, detail);
    }
}