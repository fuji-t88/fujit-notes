
import Link from "next/link";

export function Footer() {
  return (
    <footer className="border-t border-orange-100 bg-white">
      <div className="mx-auto max-w-6xl px-6 py-10">
        <div className="flex flex-col justify-between gap-6 md:flex-row">
          <div>
            <Link
              href="/"
              className="text-xl font-black italic"
            >
              <span className="text-[#292929]">Fujit</span>{" "}
              <span className="text-orange-500">Notes</span>
            </Link>

            <p className="mt-3 text-sm text-gray-500">
              Build. Learn. Grow.
            </p>
          </div>

          <nav className="flex gap-6 text-sm">
            <Link href="/articles">Articles</Link>
            <Link href="/about">About</Link>
          </nav>
        </div>

        <div className="mt-8 border-t border-gray-100 pt-6">
          <p className="text-sm text-gray-500">
            © 2026 Fujit Notes. All rights reserved.
          </p>
        </div>
      </div>
    </footer>
  );
}
