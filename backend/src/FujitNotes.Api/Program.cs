
using FujitNotes.Application.Articles.Repositories;
using FujitNotes.Application.Articles.UseCases;
using FujitNotes.Infrastructure.Articles.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Controllerを有効にする
builder.Services.AddControllers();

// 開発用Repositoryを登録
// Singletonにすることで、リクエストをまたいで
// 同じメモリ上の記事データを利用できる
builder.Services.AddSingleton<
    IArticleRepository,
    InMemoryArticleRepository>();

// Application層のユースケースを登録
builder.Services.AddScoped<CreateArticleUseCase>();
builder.Services.AddScoped<GetArticleUseCase>();
builder.Services.AddScoped<PublishArticleUseCase>();

var app = builder.Build();

// 現段階では、認証・認可が未実装のため
// 開発環境でのみControllerを公開する
if (app.Environment.IsDevelopment())
{
    app.MapControllers();
}

app.Run();
