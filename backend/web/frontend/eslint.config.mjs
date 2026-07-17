import { defineConfig, globalIgnores } from "eslint/config";
import nextVitals from "eslint-config-next/core-web-vitals";
import nextTs from "eslint-config-next/typescript";

const eslintConfig = defineConfig([
  ...nextVitals,
  ...nextTs,
  // Override default ignores of eslint-config-next.
  globalIgnores([
    // Default ignores of eslint-config-next:
    ".next/**",
    "out/**",
    "build/**",
    "next-env.d.ts",
    // shadcn/ui が生成したコード。
    // npx shadcn add で導入したものであり、手を入れても再導入で上書きされる。
    // 自分たちで保守する対象ではないため、静的解析の対象から外す。
    "components/ui/**",
    "hooks/use-mobile.ts",
  ]),
]);

export default eslintConfig;
