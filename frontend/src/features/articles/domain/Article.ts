
export interface Article {
  id: string;
  title: string;
  slug: string;
  excerpt: string;
  category: string | null;
  tags: string[];
}
