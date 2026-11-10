
import type { Article } from "../domain/Article";

// 記事一覧と記事詳細で共通利用する仮データ
export const sampleArticles: Article[] = [
  {
    id: "1",
    title: "Next.js + TypeScriptでブログを開発する",
    slug: "nextjs-typescript-blog",
    excerpt:
      "Fujit Notesのフロントエンドを構築する過程をまとめる記事の候補です。",
    category: "Development",
    tags: ["Next.js", "TypeScript"],
  },
  {
    id: "2",
    title: "なぜFujit Notesを自作するのか",
    slug: "why-build-fujit-notes",
    excerpt:
      "既存のブログサービスではなく、自分で開発する理由をまとめる予定です。",
    category: "Build in Public",
    tags: ["Architecture", "Learning"],
  },
  {
    id: "3",
    title: "ASP.NET CoreとDockerを学ぶ",
    slug: "aspnet-core-docker",
    excerpt:
      "C#バックエンドとDockerを使った開発環境構築の学習記事です。",
    category: "Cloud & DevOps",
    tags: ["C#", "Docker"],
  },
  {
    id: "4",
    title: "マラソン完走への挑戦",
    slug: "marathon-challenge",
    excerpt:
      "運動や健康改善についての取り組みを記録する記事の候補です。",
    category: "Life & Fitness",
    tags: ["Running", "Fitness"],
  },
];

// 記事詳細画面用の仮本文
export interface SampleArticleContent {
  heading: string;
  paragraphs: string[];
  code?: string;
}

export const sampleArticleContents: Record<
  string,
  SampleArticleContent
> = {
  "nextjs-typescript-blog": {
    heading: "なぜNext.jsを採用したのか？",
    paragraphs: [
      "Fujit Notesは、C#とTypeScriptを使って自作するエンジニアの成長ブログです。",
      "Next.jsを選んだ理由は、SEOへの対応、サーバーレンダリング、TypeScriptとの相性です。",
      "開発の過程では、採用した技術だけでなく、設計判断の理由も記録していきます。",
    ],
    code: `export default function Home() {
  return <h1>Fujit Notes</h1>;
}`,
  },

  "why-build-fujit-notes": {
    heading: "ブログを自作する理由",
    paragraphs: [
      "ブログを作る目的は、技術学習の成果を形として残すことです。",
      "C#のDDDとTypeScriptのOOPを比較しながら、フルスタック開発を学びます。",
      "完成したコードだけでなく、設計の理由や失敗した経験も記事にまとめます。",
    ],
  },

  "aspnet-core-docker": {
    heading: "C#バックエンドの学習",
    paragraphs: [
      "ASP.NET Coreで記事管理APIを開発します。",
      "Dockerを使って開発環境を再現できるようにし、PostgreSQLと接続します。",
      "DDDを取り入れて、記事の公開ルールや状態管理を設計します。",
    ],
  },

  "marathon-challenge": {
    heading: "健康と成長の記録",
    paragraphs: [
      "Fujit Notesでは、技術以外の挑戦も記録します。",
      "運動の継続やマラソン完走に向けた取り組みを発信していく予定です。",
      "今後は写真やグラフも使って、日々の変化を分かりやすく紹介したいと考えています。",
    ],
  },
};
