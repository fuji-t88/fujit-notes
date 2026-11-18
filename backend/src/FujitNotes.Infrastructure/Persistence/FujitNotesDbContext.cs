
using FujitNotes.Domain.Articles;
using Microsoft.EntityFrameworkCore;

namespace FujitNotes.Infrastructure.Persistence;

public sealed class FujitNotesDbContext : DbContext
{
    public FujitNotesDbContext(
        DbContextOptions<FujitNotesDbContext> options)
        : base(options)
    {
    }

    // 記事を管理するDbSet
    public DbSet<Article> Articles => Set<Article>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Infrastructure内のマッピング設定を読み込む
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(FujitNotesDbContext).Assembly);
    }
}
