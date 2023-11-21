var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.MapGet("/", () =>
{
    return System.IO.File.ReadAllTextAsync("log.txt");

})
.WithName("Get");


app.MapPost("/", (HttpContext context) =>//async context =>
{
    //Código existente para registrar a requisição no arquivo de log
    var request = context.Request;
    var requestInfo = new
    {
        Date = DateTime.Now,
        Method = request.Method,
        Path = request.Path,
        QueryString = request.QueryString,
        Headers = request.Headers,
        Body = new System.IO.StreamReader(request.Body).ReadToEndAsync().Result,
    };
    context.Response.WriteAsync("Requisição registrada com sucesso!");
})
.WithName("Post");


app.MapDelete("/", (HttpContext context) =>
{
    try
    {
        System.IO.File.WriteAllText("log.txt", string.Empty);
        context.Response.WriteAsync("Arquivo de log limpo com sucesso!");
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = 500;
        context.Response.WriteAsync($"Erro ao limpar o arquivo de log: {ex.Message}");
    }
})
.WithName("Delete");


app.Run();
