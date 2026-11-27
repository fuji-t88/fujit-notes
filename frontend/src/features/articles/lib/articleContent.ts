
import { decode } from "html-entities";

// 新規記事の保存形式を識別するためのマーカー。
// 既存記事の本文には付けず、互換性を維持する。
const RICH_TEXT_PREFIX =
  "<!-- fujit-notes:richtext:v1 -->\n";

export function markRichText(html: string): string {
  return `${RICH_TEXT_PREFIX}${html}`;
}

export function isRichText(
  content: string,
): boolean {
  return content.startsWith(RICH_TEXT_PREFIX);
}

export function getRichTextHtml(
  content: string,
): string {
  if (!isRichText(content)) {
    return "";
  }

  return content.slice(RICH_TEXT_PREFIX.length);
}

export function getArticleExcerpt(
  content: string,
  maxLength = 140,
): string {
  let plainText = content;

  if (isRichText(content)) {
    const html = getRichTextHtml(content);

    // 段落間などの区切りを残してからタグを除去する。
    // これは概要表示専用。HTML表示には使用しない。
    plainText = html
      .replace(/<br\s*\/?>/gi, " ")
      .replace(
        /<\/(?:p|div|h[1-6]|li|blockquote|pre)>/gi,
        " ",
      )
      .replace(/<[^>]*>/g, "");

    plainText = decode(plainText);
  }

  plainText = plainText
    .replace(/\s+/g, " ")
    .trim();

  return plainText.length > maxLength
    ? `${plainText.slice(0, maxLength)}…`
    : plainText;
}
