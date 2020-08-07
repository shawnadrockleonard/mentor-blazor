using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MentorApp.Data
{
    public class MentorDbContext : DbContext
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ILogger logger;

        public MentorDbContext(DbContextOptions<MentorDbContext> options, IHttpContextAccessor httpContextAccessor, ILogger<MentorDbContext> logger)
            : base(options)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.logger = logger;

            var conn = (Microsoft.Data.SqlClient.SqlConnection)Database.GetDbConnection();
            if (conn.ConnectionString.Contains("database.usgovcloudapi.net", StringComparison.OrdinalIgnoreCase))
            {
                var provider = new Microsoft.Azure.Services.AppAuthentication.AzureServiceTokenProvider();
                conn.AccessToken = provider.GetAccessTokenAsync("https://database.usgovcloudapi.net/").ConfigureAwait(true).GetAwaiter().GetResult();
            }
            if (conn.ConnectionString.Contains("database.windows.net", StringComparison.OrdinalIgnoreCase))
            {
                var provider = new Microsoft.Azure.Services.AppAuthentication.AzureServiceTokenProvider();
                conn.AccessToken = provider.GetAccessTokenAsync("https://database.windows.net/").ConfigureAwait(true).GetAwaiter().GetResult();
            }
        }

        public DbSet<HelloWorld> Helloworld { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (modelBuilder == null)
            {
                throw new ArgumentNullException(nameof(modelBuilder));
            }

            modelBuilder.Entity<HelloWorld>().HasNoKey();
        }

        public override int SaveChanges()
        {
            logger.LogInformation($"Change by {httpContextAccessor.HttpContext.User?.Identity.Name ?? "unknown"}");
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
