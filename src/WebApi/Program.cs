using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

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

app.MapGet("/", (HttpContext context) =>
{
        return System.IO.File.ReadAllTextAsync("log.txt");
})
.WithName("Get");


app.MapPost("/", (HttpContext context) =>
{
    try
    {
        //Código existente para registrar a requisição no arquivo de log
        var request = context.Request;

        // Converte os cabeçalhos em uma única string separada por "|"
        var cabecalhosString = ConvertHeadersToString(context.Request.Headers);

        IFormCollection corpoRequisicao = null;

        if (EhContextType(context.Request.Headers))
        {
            // Desserializa o corpo da requisição
            corpoRequisicao = context.Request.ReadFormAsync()?.Result;
        }

        var requestInfo = new
        {
            Date = DateTime.Now,
            Method = request.Method,
            Path = request.Path,
            QueryString = request.QueryString,
            Headers = cabecalhosString,
            Body = corpoRequisicao is null ? new System.IO.StreamReader(request.Body).ReadToEndAsync().Result : corpoRequisicao?.FirstOrDefault().Value.ToString(),
        };
        System.IO.File.AppendAllTextAsync("log.txt", requestInfo.ToString() + Environment.NewLine);
        context.Response.WriteAsync("Requisição registrada com sucesso!");
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = 500;
        context.Response.WriteAsync($"Erro ao registrar o arquivo de log: {ex.Message}");
        System.IO.File.AppendAllTextAsync("log.txt", ex.Message + Environment.NewLine);
    }
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


bool EhContextType(IHeaderDictionary headers)
{
    return headers.FirstOrDefault(f => f.Key == "Content-Type").Value.ToString() == "application/x-www-form-urlencoded";
}

string ConvertHeadersToString(IHeaderDictionary headers)
{
    var stringBuilder = new StringBuilder();

    foreach (var (key, values) in headers)
    {
        stringBuilder.Append($"{key}: {string.Join(", ", values)}|");
    }

    return stringBuilder.ToString().TrimEnd('|');
}