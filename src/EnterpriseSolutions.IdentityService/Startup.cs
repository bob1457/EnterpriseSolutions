using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnterpriseSolutions.IdentityService.Data;
using EnterpriseSolutions.IdentityService.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EnterpriseSolutions.IdentityService
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public IConfiguration Configuration { get; }

        public IWebHostEnvironment Environment { get; }

        protected virtual void AddDbContext(IServiceCollection services)
        {
            services.AddDbContext<AppIdentityDbContext>(ConfigureDatabase);
        }

        protected virtual void ConfigureDatabase(DbContextOptionsBuilder ctxBuilder)
        {
            ctxBuilder.UseSqlServer(Configuration.GetConnectionString("Default"));
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            AddDbContext(services);

            services.AddIdentity<AppUser, IdentityRole>()
            .AddEntityFrameworkStores<AppIdentityDbContext>();

            var builder = services.AddIdentityServer()
              // this adds the operational data from DB (codes, tokens, consents)
              .AddOperationalStore(options =>
              {
                  options.ConfigureDbContext = ConfigureDatabase;
                  // this enables automatic token cleanup. this is optional.
                  options.EnableTokenCleanup = true;
                  options.TokenCleanupInterval = 30; // interval in seconds
              })
              //.AddInMemoryPersistedGrants()
              .AddInMemoryIdentityResources(Config.GetIdentityResources())
              .AddInMemoryApiResources(Config.GetApiResources())
              //.AddInMemoryClients(Config.GetClients(Configuration.GetValue("AppSettings:Address", "")))
              .AddInMemoryClients(Config.GetClients())
              .AddAspNetIdentity<AppUser>();

            if (Environment.IsDevelopment())
            {
                builder.AddDeveloperSigningCredential();
            }
            else
            {
                throw new Exception("need to configure key material");
            }

            services.AddControllers();

            //services.AddTransient<IProfileService, IdentityClaimsProfileService>();
            //services.AddScoped<IUserRepository, UserRepository>();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Latest);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseIdentityServer();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });

            //app.UseEndpoints(endpoints =>
            //{
            //    endpoints.MapControllers();
            //});
        }
    }
}
