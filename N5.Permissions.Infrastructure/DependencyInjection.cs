using Microsoft.Extensions.DependencyInjection;
using N5.Permissions.Domain.Interfaces;
using N5.Permissions.Infrastructure.Services;
using N5.Permissions.Infrastructure.Repositories;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using N5.Permissions.Application.Common.Interfaces;
using N5.Permissions.Infrastructure.Messaging;
using Microsoft.Extensions.Configuration;

namespace N5.Permissions.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var settings = new ConnectionSettings(new Uri(configuration["Elasticsearch:Uri"]))
                .DefaultIndex("permissions");

            var client = new ElasticClient(settings);
            services.AddSingleton<IElasticClient>(client);
            services.AddSingleton<IKafkaProducer>(sp =>
                new KafkaProducer("localhost:29092"));

            services.AddScoped<IPermissionIndexer, PermissionIndexer>();

            services.AddScoped<IPermissionRepository, PermissionRepository>();
            services.AddScoped<IPermissionTypeRepository, PermissionTypeRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}
