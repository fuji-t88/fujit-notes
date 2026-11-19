
using FujitNotes.Application.Articles.Repositories;
using FujitNotes.Application.Articles.UseCases;
using FujitNotes.Infrastructure.Articles.Repositories;
using FujitNotes.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Controllerを登録
builder.Services.AddControllers();

// PostgreSQLの接続文字列
var connectionString =
    builder.Configuration.GetConnectionString(
        "DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "PostgreSQLの接続文字列が設定されていません。");
}

// EF CoreとPostgreSQL
builder.Services.AddDbContext<FujitNotesDbContext>(
    options => options.UseNpgsql(connectionString));

// Repository
builder.Services.AddScoped<
    IArticleRepository,
    EfArticleRepository>();

// Application UseCases
builder.Services.AddScoped<CreateArticleUseCase>();
builder.Services.AddScoped<GetArticleUseCase>();
builder.Services.AddScoped<GetArticlesUseCase>();
builder.Services.AddScoped<PublishArticleUseCase>();

var app = builder.Build();

// 認証機能がまだないため、
// APIはDevelopment環境のみ有効
if (app.Environment.IsDevelopment())
{
    app.MapControllers();
}

app.Run();
