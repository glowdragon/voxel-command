using Anthropic.SDK;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<AnthropicClient>();
builder.Services.AddControllers();

var app = builder.Build();
app.MapControllers();
app.Run();
