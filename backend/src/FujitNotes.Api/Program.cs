
using FujitNotes.Application.Articles.Repositories;
using FujitNotes.Application.Articles.UseCases;
using FujitNotes.Infrastructure.Articles.Repositories;
using FujitNotes.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Controllerを登録
builder.Services.AddControllers();

// PostgreSQLの接続文字列を取得
var connectionString =
    builder.Configuration.GetConnectionString(
        "DefaultConnection");

// 接続文字列の設定確認
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "PostgreSQLの接続文字列が設定されていません。");
}

// EF CoreとPostgreSQLを登録
builder.Services.AddDbContext<FujitNotesDbContext>(
    options =>
        options.UseNpgsql(connectionString));

// RepositoryをPostgreSQL用に変更
builder.Services.AddScoped<
    IArticleRepository,
    EfArticleRepository>();

// Application層のユースケース
builder.Services.AddScoped<CreateArticleUseCase>();
builder.Services.AddScoped<GetArticleUseCase>();
builder.Services.AddScoped<PublishArticleUseCase>();

var app = builder.Build();

// 認証機能が未実装のため、
// 開発環境だけでControllerを有効にする
if (app.Environment.IsDevelopment())
{
    app.MapControllers();
}

app.Run();
