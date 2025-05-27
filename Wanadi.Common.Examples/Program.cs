using System.Globalization;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Wanadi.Common.Extensions;

CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("pt-BR");
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("pt-BR");
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile(path: "appsettings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .AddCommandLine(args)
                    .Build();

builder.Services.ResolveDependenciesByAttributes();
builder.Services.AddHttpClientConfigurations(builder.Configuration);

using var app = builder.Build();
using var serviceScope = app.Services.CreateScope();
var serviceProvider = serviceScope.ServiceProvider;