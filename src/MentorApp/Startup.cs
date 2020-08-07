using MentorApp.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;

namespace MentorApp
{
    public class Startup
    {
        public Startup(IWebHostEnvironment environment)
        {
            WebHostEnvironment = environment;
            Configuration = BuildConfiguration();
        }

        private const string DbConnectionStringKey = "MentorDbContext";
        private IWebHostEnvironment WebHostEnvironment { get; }
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Build configuration from AppSettings, Environment Variables, Azure Key Valut and (User Secrets - DEV only).
        /// </summary>
        /// <returns><see cref="IConfiguration"/></returns>
        private IConfiguration BuildConfiguration()
        {
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .AddUserSecrets(typeof(Startup).Assembly)
                .AddAzureKeyVaultIfAvailable();

            if (WebHostEnvironment.IsDevelopment())
            {
                // Re-add User secrets so it takes precedent for local development
                configurationBuilder.AddUserSecrets(typeof(Startup).Assembly);
            }

            return configurationBuilder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddSingleton<WeatherForecastService>();
            services.AddTransient<MentorService>();
            services.AddSingleton<IKeyVaultHelper, AzureKeyVaultHelper>();
            services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IAppSettingEntity, AppSettingEntity>(config =>
            {
                var connection = Configuration.Get<AppSettingEntity>();
                return connection;
            });
            var dbConnectionString = Configuration.GetConnectionString(DbConnectionStringKey);
            services.AddDbContext<MentorDbContext>(options => options.UseSqlServer(dbConnectionString));
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Runtime")]
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseExceptionHandlerMiddleware();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandlerMiddleware();
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
