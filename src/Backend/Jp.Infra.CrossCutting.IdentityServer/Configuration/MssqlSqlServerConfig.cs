﻿using Jp.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Jp.Infra.CrossCutting.IdentityServer.Configuration
{
    public static class MssqlSqlServerConfig
    {
        public static IIdentityServerBuilder UseIdentityServerSqlDatabase(this IIdentityServerBuilder builder,
            IServiceCollection services, IConfiguration configuration, ILogger logger)
        {
            var connectionString = configuration.GetConnectionString("SSOConnection");
            var migrationsAssembly = "Jp.Infra.Migrations.Sql.Identity";//typeof(IdentityServerSqlConfig).GetTypeInfo().Assembly.GetName().Name;

            services.AddDbContext<JpContext>(options => options.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(migrationsAssembly)));
            // this adds the config data from DB (clients, resources)
            builder.AddConfigurationStore(options =>
                {
                    options.ConfigureDbContext = b =>
                        b.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(migrationsAssembly));
                })
                // this adds the operational data from DB (codes, tokens, consents)
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = b =>
                        b.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(migrationsAssembly));

                    // this enables automatic token cleanup. this is optional.
                    options.EnableTokenCleanup = true;
                    options.TokenCleanupInterval = 15; // frequency in seconds to cleanup stale grants. 15 is useful during debugging
                });



            return builder;
        }

    }
}