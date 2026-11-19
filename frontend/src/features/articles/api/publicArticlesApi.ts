
import "server-only";

export type PublicArticleDto = {
  id: string;
  title: string;
  content: string;
  publishedAt: string | null;
};

function getApiBaseUrl(): string {
  const url = process.env.FUJIT_NOTES_API_BASE_URL;

  if (!url) {
    throw new Error(
      "FUJIT_NOTES_API_BASE_URL が設定されていません。",
    );
  }

  return url.replace(/\/+$/, "");
}

export async function getPublicArticles(): Promise<
  PublicArticleDto[]
> {
  const response = await fetch(
    `${getApiBaseUrl()}/api/public/articles`,
    {
      cache: "no-store",
    },
  );

  if (!response.ok) {
    throw new Error(
      `公開記事一覧の取得に失敗しました。HTTP ${response.status}`,
    );
  }

  return (await response.json()) as PublicArticleDto[];
}

export async function getPublicArticleById(
  id: string,
): Promise<PublicArticleDto | null> {
  const response = await fetch(
    `${getApiBaseUrl()}/api/public/articles/${encodeURIComponent(id)}`,
    {
      cache: "no-store",
    },
  );

  if (response.status === 404) {
    return null;
  }

  if (!response.ok) {
    throw new Error(
      `公開記事詳細の取得に失敗しました。HTTP ${response.status}`,
    );
  }

  return (await response.json()) as PublicArticleDto;
}
