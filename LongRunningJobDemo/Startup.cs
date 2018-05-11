using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using Hangfire;
using Hangfire.MemoryStorage;

[assembly: OwinStartup(typeof(LongRunningJobDemo.Startup))]

namespace LongRunningJobDemo
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
            //Lo stato dei job può essere persistito su database (Sql server) 
            //ma in questo esempio uso lo storage in-memory
            GlobalConfiguration.Configuration.UseMemoryStorage(new MemoryStorageOptions { CountersAggregateInterval = TimeSpan.FromSeconds(2), JobExpirationCheckInterval = TimeSpan.FromSeconds(2) });
            app.UseHangfireDashboard("/Jobs");
            app.UseHangfireServer();
        }
    }
}
