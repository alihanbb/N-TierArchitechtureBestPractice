using AppRepository.Extentions;
using AppService.Extentions;
using AppService.Products;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers(options =>options.Filters.Add<FluentValidationFiltre>());
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Test ortamýnda AddRepository'yi çaðýrma
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddRepository(builder.Configuration);
}

builder.Services.AddServices(builder.Configuration);

builder.Services.Configure<ApiBehaviorOptions>(opt =>
{
    opt.SuppressModelStateInvalidFilter = true;
    //opt.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseExceptionHandler(x => { });
app.UseHttpsRedirection();

app.UseAuthorization();
app.MapControllers();

app.Run();

// Integration testler için Program class'ýný public yap
public partial class Program { }
