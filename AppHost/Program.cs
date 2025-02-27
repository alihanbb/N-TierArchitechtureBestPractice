using Projects;
var builder = DistributedApplication.CreateBuilder(args);
builder.AddProject<AppApi>("product-api");
builder.Build().Run();
