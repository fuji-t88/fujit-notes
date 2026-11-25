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
    builder.Configuration.GetConnectionString("DefaultConnection");

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

// 記事管理用UseCase
builder.Services.AddScoped<CreateArticleUseCase>();
builder.Services.AddScoped<GetArticleUseCase>();
builder.Services.AddScoped<GetArticlesUseCase>();
builder.Services.AddScoped<PublishArticleUseCase>();
builder.Services.AddScoped<UpdateArticleUseCase>();
builder.Services.AddScoped<ArchiveArticleUseCase>();

// 読者向けUseCase
builder.Services.AddScoped<GetPublishedArticlesUseCase>();
builder.Services.AddScoped<GetPublishedArticleUseCase>();

var app = builder.Build();

// Step 25：本番環境のAPIアクセス制限
// Development以外では、公開記事のGETのみを許可する。
if (!app.Environment.IsDevelopment())
{
    app.Use(async (context, next) =>
    {
        var isPublicArticleRead =
            HttpMethods.IsGet(context.Request.Method)
            && context.Request.Path.StartsWithSegments(
                "/api/public/articles",
                StringComparison.OrdinalIgnoreCase);

        if (!isPublicArticleRead)
        {
            context.Response.StatusCode =
                StatusCodes.Status404NotFound;

            return;
        }

        await next();
    });
}

// 本番環境でも読者向けControllerを利用可能にする
app.MapControllers();

app.Run();