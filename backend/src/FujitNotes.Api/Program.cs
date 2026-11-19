
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

// 管理用UseCase
builder.Services.AddScoped<CreateArticleUseCase>();
builder.Services.AddScoped<GetArticleUseCase>();
builder.Services.AddScoped<GetArticlesUseCase>();
builder.Services.AddScoped<PublishArticleUseCase>();
builder.Services.AddScoped<UpdateArticleUseCase>();
builder.Services.AddScoped<ArchiveArticleUseCase>();

// Step 20：読者向け公開記事取得UseCase
builder.Services.AddScoped<GetPublishedArticlesUseCase>();
builder.Services.AddScoped<GetPublishedArticleUseCase>();

var app = builder.Build();

// 認証・本番公開設定が未実装のため
// 現時点ではDevelopment環境のみControllerを登録
if (app.Environment.IsDevelopment())
{
    app.MapControllers();
}

app.Run();
