
const categories = [
  "Development",
  "Cloud & DevOps",
  "Career & Learning",
  "Build in Public",
  "Life & Fitness",
];

export default function Home() {
  return (
    <section className="mx-auto max-w-6xl px-6 py-20">
      <p className="mb-4 font-semibold text-orange-600">
        ENGINEERING · GROWTH · LIFE
      </p>

      <h1 className="text-5xl font-extrabold tracking-tight">
        Build. Learn.{" "}
        <span className="text-orange-500">Grow.</span>
      </h1>

      <p className="mt-6 max-w-2xl text-lg leading-relaxed text-gray-600">
        技術を学び、挑戦を記録し、人生をアップデートする。
        エンジニアの成長ブログ。
      </p>

      <div className="mt-10 flex flex-wrap gap-3">
        {categories.map((category) => (
          <span
            key={category}
            className="rounded-full border border-orange-200 bg-white px-4 py-2 text-sm"
          >
            {category}
          </span>
        ))}
      </div>
    </section>
  );
}
