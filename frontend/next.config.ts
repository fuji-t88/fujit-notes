import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  // Step 26: Dockerによる本番公開に対応
  output: "standalone",

  cacheComponents: true,
  partialPrefetching: true,

  turbopack: {
    rules: {
      "*.css": {
        loaders: ["@tailwindcss/turbopack"],
        as: "*.css",
      },
    },
  },
};

export default nextConfig;