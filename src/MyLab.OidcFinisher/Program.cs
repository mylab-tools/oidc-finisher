using MyLab.ApiClient;
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
            r.RegisterContract<IBizLogicApi>("biz-api");
            r.RegisterContract<IOidcProvider>("oidc");
        }
    )
    .AddMediatR(c => c.RegisterServicesFromAssemblyContaining<Program>())
    .Configure<ExceptionProcessingOptions>(opt => opt.HideError = !builder.Environment.IsDevelopment());

builder.Services.AddOptions<OidcFinisherOptions>()
    .Bind(builder.Configuration.GetSection("Finisher"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

app.Run();


public partial class Program{}