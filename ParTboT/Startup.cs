using Hangfire;
using Hangfire.Mongo;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Owin;
using System;

namespace ParTboT
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            Console.WriteLine("hello");
        }

        public void Configuration(IAppBuilder app)
        {
            app.Run(context =>
            {
                string t = DateTime.Now.Millisecond.ToString();
                return context.Response.WriteAsync(t + " Test OWIN App");
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IRecurringJobManager recurringJobManager, MongoClient client)
        {
            Console.WriteLine("hello");

            GlobalConfiguration.Configuration.UseMongoStorage(client, "Hangfire", new MongoStorageOptions() { CheckConnection = true }); ;
            app.UseHangfireDashboard();
            app.UseHangfireServer();
        }
    }
}