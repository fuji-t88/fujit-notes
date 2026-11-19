
using FujitNotes.Application.Articles.Repositories;
using FujitNotes.Application.Articles.UseCases;
using FujitNotes.Infrastructure.Articles.Repositories;
using FujitNotes.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Controller
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

// Repository
builder.Services.AddScoped<
    IArticleRepository,
    EfArticleRepository>();

// UseCases
builder.Services.AddScoped<CreateArticleUseCase>();
builder.Services.AddScoped<GetArticleUseCase>();
builder.Services.AddScoped<GetArticlesUseCase>();
builder.Services.AddScoped<PublishArticleUseCase>();

// Step 18：記事編集
builder.Services.AddScoped<UpdateArticleUseCase>();

var app = builder.Build();

// 認証未実装のためDevelopmentのみAPIを有効化
if (app.Environment.IsDevelopment())
{
    app.MapControllers();
}

app.Run();
