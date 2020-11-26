using System;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MediatR;
using Microsoft.Net.Http.Headers;

namespace api
{
    public class Startup
    {
        private readonly string _allowOrigins = "_allowOrigins";
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddSingleton<Repository>()
                .AddMediatR(Assembly.GetExecutingAssembly())
                .AddCors(options =>
                {
                    options.AddPolicy(_allowOrigins, builder =>
                    {
                        builder
                            .WithMethods("GET", "POST", "OPTIONS")
                            .WithHeaders(HeaderNames.ContentType, "user-id")
                            .SetIsOriginAllowed(origin =>
                                origin.Equals("http://localhost:3000", StringComparison.CurrentCultureIgnoreCase)
                                || origin.Equals("http://localhost:4000", StringComparison.CurrentCultureIgnoreCase)
                                );
                    });
                })
                .AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app
                    .UseDeveloperExceptionPage()
                    .UseCors(_allowOrigins);
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
