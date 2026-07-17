import type { NextConfig } from "next";

/**
 * API Proxyの設定
 * ブラウザからは同一オリジン(/api/admin/...)へアクセスさせ、
 * Next.jsサーバーが裏でAzureのバックエンドへ転送する。
 * これによりCORSとCookieのSameSite制約を回避する。
 */
const nextConfig: NextConfig = {
  /**
   * 実行に必要な依存だけを抽出した成果物を出力する。
   * node_modules 全体を配布せずに済むため、デプロイが軽くなる。
   * 出力は .next/standalone に生成され、node server.js で起動する。
   */
  output: "standalone",
  async rewrites() {
    return [
      {
        source: "/api/admin/:path*",
        destination: `${process.env.API_BASE_URL}/api/admin/:path*`,
      },
    ];
  },
  async redirects() {
    return [
      { source: "/", destination: "/admin", permanent: false },
    ];
  },
  images: {
    remotePatterns: [
      {
        protocol: "https",
        hostname: "blob20260713.blob.core.windows.net",
        pathname: "/images/**",
      },
    ],
  },
};

export default nextConfig;