

using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using SharedLibrary.Middleware;

namespace SharedLibrary.DependencyInjection
{
    public static class SharedServiceContainer
    {
        public static IServiceCollection AddSharedServices<TContext>(this IServiceCollection services, IConfiguration config , string fileName) where TContext: DbContext
        {
            services.AddDbContext<TContext>(option => option.UseSqlServer(
                config
                .GetConnectionString("TestConnection"), sqlserverOption => sqlserverOption.EnableRetryOnFailure()));

            Log.Logger = new LoggerConfiguration()
               .MinimumLevel.Information()
               .WriteTo.Debug()
               .WriteTo.Console()
               .WriteTo.File(
                path: $"{fileName}-.text",
                restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information,
                outputTemplate: "{Timestamp:yyyy-MM-dd:HH:ùù:ss.fff zzz} [{level:u3}]{message:lj}{NewLine}{EXception} ",
                rollingInterval: RollingInterval.Day)
               .CreateLogger();


            //Add JWT Scheme 

            JWTAuthenticationScheme.AddJWTAuthenticationScheme(services, config);
            return services;
        }
            public static IApplicationBuilder UseSharedPolicies (this IApplicationBuilder app)
        {
            // use global exception 
            app.UseMiddleware<GlobalException>(); 
            app.UseMiddleware<ListenToOnlyApiGateway>();

            return app;
        }
            
    }
}
