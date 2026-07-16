import type { NextConfig } from "next";

/**
 * API Proxyの設定
 * ブラウザからは同一オリジン(/api/admin/...)へアクセスさせ、
 * Next.jsサーバーが裏でAzureのバックエンドへ転送する。
 * これによりCORSとCookieのSameSite制約を回避する。
 */
const nextConfig: NextConfig = {
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