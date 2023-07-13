using HtmlAgilityPack;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();



string GetId(string document)
{
    string cadena = document;

    // Definir la expresión regular para extraer el número
    string patron = @"/(\d+)/";

    // Buscar la coincidencia en la cadena
    Match coincidencia = Regex.Match(cadena, patron);

    if (coincidencia.Success)
    {
        // Obtener el valor numérico como cadena
        string numeroStr = coincidencia.Groups[1].Value;

        // Convertir la cadena a un valor numérico
        if (int.TryParse(numeroStr, out int numero))
        {
            // Imprimir el valor numérico obtenido
            Console.WriteLine("Valor numérico: " + numero);
            return numero.ToString();
        }
        else
        {
            Console.WriteLine("No se pudo convertir el valor numérico");
        }
    }
    else
    {
        Console.WriteLine("No se encontró un valor numérico en la cadena");
    }

    return document;
}


app.MapGet("/test", async () =>
{

    using HttpClient httpClient = new();

    httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/114.0.0.0 Safari/537.36 Edg/114.0.1823.79");

    var response = await httpClient.GetAsync("https://{base}/letra/A/");

    if (response.IsSuccessStatusCode)
    {
        HtmlDocument htmlDocument = new();
        htmlDocument.LoadHtml(await response.Content.ReadAsStringAsync());

        var container = htmlDocument.DocumentNode.Descendants("div")
        .FirstOrDefault(x => x.GetAttributeValue("class", "").Contains("anime__page__content"));

        if (container != null)
        {
            var grid = container.ChildNodes.FirstOrDefault((a => a.Name == "div" && a.GetAttributeValue("class", "") != "anime__page__title"));

            if (grid != null)
            {
                var items = grid.ChildNodes.Where(x => x.Descendants("div")
                .Any(x => x.GetAttributeValue("class", "")
                .Contains("anime__item"))).ToList();

                foreach (var item in items)
                {
                    var div = item.ChildNodes.FirstOrDefault(x => x.Descendants("div").Any());
                    var link = div?.ChildNodes.FirstOrDefault(x => x.Name == "a")?.Attributes["href"]?.Value;

                    //var link = item
                     using HttpClient itemClient = new();
                    itemClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/114.0.0.0 Safari/537.36 Edg/114.0.1823.79");

                    var responseItem = await httpClient.GetAsync(link);

                    if (responseItem.IsSuccessStatusCode)
                    {
                        HtmlDocument htmlDocument1 = new HtmlDocument();

                        htmlDocument1.LoadHtml(await  responseItem.Content.ReadAsStringAsync());

                        var document = htmlDocument1.DocumentNode.Descendants("body").FirstOrDefault()?
                        .Descendants("script");

                        var script = document?.ToArray()[1];

                        var text = script.InnerHtml;

                        var id = GetId(text);


                        //var id = GetId();
                                               
                    }



                }

            }
        }

        //var itemsResponse = items

    }




    return "";
})
.WithName("Test")
.WithOpenApi();

app.Run();