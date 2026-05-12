var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// hello world endpoint
app.MapGet("/", () => "Hello World!");

app.Run();
