using MyLab.ApiClient;
using MyLab.Log;
using MyLab.OidcFinisher;
using MyLab.OidcFinisher.ApiSpecs.BizLogicApi;
using MyLab.OidcFinisher.ApiSpecs.OidcProvider;
using MyLab.WebErrors;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(opt => opt.AddExceptionProcessing());

builder.Services
    .AddApiClients
    (
        r =>
        {
            r.RegisterContract<IOidcProvider>("oidc");
        }
    )
    .AddOptionalApiClients
    (
        r =>
        {
            r.RegisterContract<IBizLogicApi>("app");
        }
    )
    .ConfigureApiClients(builder.Configuration)
    .AddMediatR(c => c.RegisterServicesFromAssemblyContaining<Program>())
    .Configure<ExceptionProcessingOptions>(opt => opt.HideError = !builder.Environment.IsDevelopment())
    .AddLogging(c => c.AddMyLabConsole());

builder.Services.AddOptions<OidcFinisherOptions>()
    .Bind(builder.Configuration.GetSection("OidcFinisher"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

app.Run();


public partial class Program{}