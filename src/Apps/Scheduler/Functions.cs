using Core.Objects;
using Core.Objects.Entities.CMS;
using Core.Objects.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Scheduler
{
    public class Functions
    {
        private readonly IEnumerable<IScheduledOperationRunner> services;
        private readonly ICoreDataContext core;

        public Functions(IEnumerable<IScheduledOperationRunner> services, ICoreDataContext core)
        {
            this.services = services;
            this.core = core;
        }

        [FunctionName("Migrate")]
        public string Migrate([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req, ILogger log, ExecutionContext context)
        {
            try
            {
                log.LogInformation("Database Migration Starting ...");

                try
                {
                    core.Migrate();
                }
                catch (Exception ex)
                {
                    log.LogError($"{ex.Message}\n{ex.StackTrace}");
                }

                log.LogInformation("Database Migration Complete!");
                return new { Status = "Migration Complete!" }.ToJsonForOdata();
            }
            catch (Exception ex)
            {
                log.LogError($"Migration Failure:\n{ex.Message}\n{ex.StackTrace}");
                return new { Status = "Migration Failed!" }.ToJsonForOdata();
            }
        }

        [FunctionName("Run")]
        public void Run([TimerTrigger("0 */1 * * * *")] TimerInfo timer, ILogger log, ExecutionContext context)
        {
            try
            {
                log.LogInformation($"Scheduled run, last called on {timer.ScheduleStatus.Last:yyyy-MM-ddTHH:mm:ss}");
                log.LogInformation("Running services");

                Task.WaitAll(
                    services.Select(async s =>
                    {
                        try
                        {
                            await s.Run();
                            s.Dispose();
                        }
                        catch (Exception ex)
                        {
                            log.LogError($"Exception caught in {s.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
                        }
                    }).ToArray()
                );

                log.LogInformation("Done!");
            }
            catch (Exception ex)
            {
                log.LogError($"Run Failure:\n{ex.Message}\n{ex.StackTrace}");
            }
        }

        [FunctionName("SetupEnvironment")]
        public async Task<string> SetupEnvironment([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req, ILogger log, ExecutionContext context)
        {
            Migrate(req, log, context);

            try
            {
                InitParams initParams = Data.ParseJson<InitParams>(new StreamReader(req.Body).ReadToEnd());
                ((SystemAuthInfo)core.AuthInfo).SSOUserId = initParams.User;

                if (core.GetAll<App>().Any())
                    throw new InvalidOperationException("Environment already setup!");
                else
                {
                    //TODO: rebuild this later
                    /*
                    string destinationDomain = config.Services["Api"].TrimStart("https://".ToCharArray()).TrimEnd("/Api/".ToCharArray());
                    using NewEnvironment setup = new(config, ContextHelper.GetSecurity(configRoot), core);
                    await setup.Go(destinationDomain, initParams.SourceDomain, initParams.User, initParams.Pass);
                    return new { Success = true, Message = config.Services["Api"].Replace("Api/", "") }.ToJsonForOdata();
                    */

                    return await Task.FromResult("Not Implemented.");
                }
            }
            catch (Exception ex) { return new { Success = false, Message = $"{ex.Message}\n{ex.StackTrace}" }.ToJson(); }
        }
    }

    class InitParams
    {
        public string SourceDomain { get; set; }

        [Required]
        public string User { get; set; }

        [Required]
        public string Pass { get; set; }
    }
}