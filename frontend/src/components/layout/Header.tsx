
import Link from "next/link";

export function Header() {
  return (
    <header className="border-b border-orange-100 bg-white">
      <div className="mx-auto flex max-w-6xl items-center justify-between px-6 py-5">
        {/* ブログロゴ */}
        <Link href="/" className="text-2xl font-black italic">
          <span className="text-[#292929]">Fujit</span>{" "}
          <span className="text-orange-500">Notes</span>
        </Link>

        {/* ナビゲーション */}
        <nav className="flex gap-6 text-sm font-medium text-[#292929]">
          <Link
            href="/articles"
            className="transition-colors hover:text-orange-500"
          >
            Articles
          </Link>
          <Link
            href="/about"
            className="transition-colors hover:text-orange-500"
          >
            About
          </Link>
        </nav>
      </div>
    </header>
  );
}
