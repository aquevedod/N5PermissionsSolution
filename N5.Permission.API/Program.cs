using N5.Permissions.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using N5.Permissions.Domain.Interfaces;
using N5.Permissions.Infrastructure.Repositories;
using MediatR;
using System.Reflection;
using N5.Permissions.Application.Handlers;
using N5.Permissions.Infrastructure;
using Serilog;
using Nest;
using Elastic.Serilog.Sinks;
using Elastic.Channels;
using Elastic.Ingest.Elasticsearch.DataStreams;
using Elastic.Ingest.Elasticsearch;


namespace N5.Permissions.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File(Path.Combine("C:", "N5PermissionsSolution", "logs", "permissionsapi.log"), rollingInterval: RollingInterval.Day)
                .WriteTo.Elasticsearch(new[] { new Uri("http://localhost:9200") }, opts =>
                {
                    opts.DataStream = new DataStreamName("logs", "console-example", "demo");
                    opts.BootstrapMethod = BootstrapMethod.Failure;
                }, transport =>
                {
                    // transport.Authentication(new BasicAuthentication(username, password));
                    // transport.Authentication(new ApiKey(base64EncodedApiKey));
                })
                .Enrich.FromLogContext()
                .CreateLogger();



            var builder = WebApplication.CreateBuilder(args);
            builder.Host.UseSerilog();
            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddDbContext<PermissionsDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddMediatR(typeof(RequestPermissionHandler).Assembly);
            builder.Services.AddInfrastructure(builder.Configuration);


            var app = builder.Build();

            // Verificar conexión a Elasticsearch al iniciar
            using (var scope = app.Services.CreateScope())
            {
                var elasticClient = scope.ServiceProvider.GetRequiredService<IElasticClient>();
                try
                {
                    var pingResponse = elasticClient.Ping();
                    if (!pingResponse.IsValid)
                    {
                        throw new Exception("No se pudo conectar a Elasticsearch: " + pingResponse.DebugInformation);
                    }
                    Log.Information("Conexión a Elasticsearch verificada correctamente");
                }
                catch (Exception ex)
                {
                    Log.Fatal(ex, "Error al conectar con Elasticsearch");
                    throw;
                }
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
