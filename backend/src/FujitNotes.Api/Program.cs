
using FujitNotes.Application.Articles.Repositories;
using FujitNotes.Application.Articles.UseCases;
using FujitNotes.Infrastructure.Articles.Repositories;
using FujitNotes.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Controllerの登録
builder.Services.AddControllers();

// PostgreSQL接続
var connectionString =
    builder.Configuration.GetConnectionString(
        "DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "PostgreSQLの接続文字列が設定されていません。");
}

builder.Services.AddDbContext<FujitNotesDbContext>(
    options => options.UseNpgsql(connectionString));

// Repositoryの登録
builder.Services.AddScoped<
    IArticleRepository,
    EfArticleRepository>();

// UseCaseの登録
builder.Services.AddScoped<CreateArticleUseCase>();
builder.Services.AddScoped<GetArticleUseCase>();
builder.Services.AddScoped<GetArticlesUseCase>();
builder.Services.AddScoped<PublishArticleUseCase>();
builder.Services.AddScoped<UpdateArticleUseCase>();

// Step 19：記事アーカイブ
builder.Services.AddScoped<ArchiveArticleUseCase>();

var app = builder.Build();

// 認証未実装のためDevelopment環境のみ有効化
if (app.Environment.IsDevelopment())
{
    app.MapControllers();
}

app.Run();
