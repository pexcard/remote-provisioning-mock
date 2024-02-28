using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using RemoteProvisioningServiceMock.Extensions;
using RemoteProvisioningServiceMock.Storage;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace RemoteProvisioningServiceMock
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            RegisterStorage(services);

            services
                .AddControllers()
                .AddJsonOptions(opt => opt.JsonSerializerOptions.PropertyNamingPolicy = null);

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Token Provisioning",
                    Description = "This is a Mock service that allows you to set-up how your endpoints will respond: " +
                                  "(shared secret, status code, delay, contacts for step-up flow)"
                });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

                c.IncludeXmlComments(xmlPath);
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Token Provisioning Mock API");
                c.RoutePrefix = string.Empty;
                c.DocumentTitle = "PEX Remote Provisioning Mock";
                c.DocExpansion(DocExpansion.None);
                c.DisplayRequestDuration();
                c.EnableDeepLinking();
                c.EnableTryItOutByDefault();
            });

            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private void RegisterStorage(IServiceCollection services)
        {
            var storageConnection = Configuration.GetConnectionString("StorageConnectionString");

            services
                .AddSingleton(x => new ResponseSettingsStorage(storageConnection).InitTable());

            services
                .AddSingleton(x => new TokenProvisioningStorage(storageConnection).InitTable());

            services
                .AddSingleton(x => new OtpCodeStorage(storageConnection).InitTable());

            services
                .AddSingleton(x => new CallbackStorage(storageConnection).InitTable());
        }
    }
}