using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Consul;
using Ocelot.Provider.Consul;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
builder.Services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(config =>
{
    config.Address = new Uri("http://localhost:8500");
}));

builder.Services.AddOcelot(builder.Configuration).AddConsul();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSwagger();
app.UseSwaggerUI();

await app.UseOcelot();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();