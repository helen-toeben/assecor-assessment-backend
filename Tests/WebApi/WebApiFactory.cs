using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Tests.WebApi;

public class WebApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        string tempCsv = Path.GetTempFileName();
        
        File.WriteAllLines(tempCsv, new[]
        {
            "Müller, Hans, 67742 Lauterecken, 1",
            "Petersen, Peter, 18439 Stralsund, 2"
        });

        builder.ConfigureAppConfiguration(config =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Csv:FilePath"] = tempCsv,
            });
        });
    }
}