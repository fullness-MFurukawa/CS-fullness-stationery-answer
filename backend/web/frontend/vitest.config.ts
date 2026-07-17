import { defineConfig, configDefaults } from "vitest/config";
import { resolve } from "node:path";

export default defineConfig({
    test: {
        environment: "node",
        globals: true,
        // Vitest の対象を tests ディレクトリに限定する。
        // 既定では **/*.spec.ts も拾うため、e2e ディレクトリの
        // Playwright のテストまで実行しようとして失敗する。
        // Playwright と Vitest はどちらも test/expect という名前を使うが、
        // 互換性はないため、対象を明確に分ける必要がある。
        include: ["tests/**/*.test.ts"],
        exclude: [...configDefaults.exclude, "e2e/**"],
    },
    resolve: {
        alias: {
            "@": resolve(__dirname, "."),
        },
    },
});