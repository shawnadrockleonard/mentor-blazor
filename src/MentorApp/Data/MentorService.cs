using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace MentorApp.Data
{
    public class MentorService
    {
        private MentorDbContext Context { get; }
        private ILogger<MentorService> Logger { get; }

        public MentorService(MentorDbContext context, ILogger<MentorService> logger)
        {
            Context = context;
            Logger = logger;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "debugging...")]
        public async Task<IList<HelloWorld>> GetHelloWorldsAsync()
        {
            Logger.LogDebug($"Querying for HelloWorld at {DateTime.UtcNow}");
            List<HelloWorld> list;
            try
            {
                list = await Context.Helloworld.ToListAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Failed to retreive Helloworld with message {ex.Message}");
                throw new HttpStatusCodeException((int)HttpStatusCode.BadRequest, $"Failed in GetHelloWorldsAsync", ex);
            }

            return list;
        }
    }
}
