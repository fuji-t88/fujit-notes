
using System.Threading.RateLimiting;
using FujitNotes.Application.Articles.Repositories;
using FujitNotes.Application.Articles.UseCases;
using FujitNotes.Infrastructure.Articles.Repositories;
using FujitNotes.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// API controllers
builder.Services.AddControllers();

// PostgreSQL
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection");

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

// Cookie認証
builder.Services
    .AddAuthentication(
        CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "FujitNotes.Admin";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.Path = "/api";

        options.Cookie.SecurePolicy =
            builder.Environment.IsDevelopment()
                ? CookieSecurePolicy.SameAsRequest
                : CookieSecurePolicy.Always;

        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = false;

        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode =
                StatusCodes.Status401Unauthorized;

            return Task.CompletedTask;
        };

        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode =
                StatusCodes.Status403Forbidden;

            return Task.CompletedTask;
        };
    });

// 管理APIの認可
builder.Services.AddAuthorization();

// ログイン試行回数の制限
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode =
        StatusCodes.Status429TooManyRequests;

    options.AddPolicy("admin-login", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey:
                context.Connection.RemoteIpAddress?.ToString()
                ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(5),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
});

var app = builder.Build();

// 本番環境では公開記事閲覧と管理者向けAPIのみ受け付ける。
// 管理者向けAPIのアクセス制御は各Controllerの認可属性で行う。
if (!app.Environment.IsDevelopment())
{
    app.Use(async (context, next) =>
    {
        var isPublicArticleRead =
            HttpMethods.IsGet(context.Request.Method)
            && context.Request.Path.StartsWithSegments(
                "/api/public/articles",
                StringComparison.OrdinalIgnoreCase);

        var isAdminAuthentication =
            context.Request.Path.StartsWithSegments(
                "/api/admin/auth",
                StringComparison.OrdinalIgnoreCase);

        var isAuthenticatedArticleApi =
            context.Request.Path.StartsWithSegments(
                "/api/articles",
                StringComparison.OrdinalIgnoreCase);

        if (!isPublicArticleRead
            && !isAdminAuthentication
            && !isAuthenticatedArticleApi)
        {
            context.Response.StatusCode =
                StatusCodes.Status404NotFound;

            return;
        }

        await next();
    });
}

app.UseRouting();

// エンドポイント別の試行回数制限
app.UseRateLimiter();

// 認証 → 認可
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
