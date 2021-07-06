//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Builder;
//using Microsoft.AspNetCore.Hosting;
//using Microsoft.AspNetCore.HttpOverrides;
//using Microsoft.AspNetCore.HttpsPolicy;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Logging;
//using Microsoft.OpenApi.Models;
//using YarinGeorge.Utilities.Databases.MongoDButilities;

//namespace GogyBot_Alpha
//{
//    public class Startup
//    {
//        public static MongoCRUD LDB { get; set; }
//        public static MongoCRUD RemindersDB { get; set; }

//        // This method gets called by the runtime. Use this method to add services to the container.
//        public void ConfigureServices(IServiceCollection services)
//        {
            
//            //services.AddSingleton()

//            MongoCRUDConnectionOptions RemindersDBConOptions = new MongoCRUDConnectionOptions
//            {
//                Database = "Jobs",
//                ConnectionString = "mongodb://localhost:27017/?readPreference=primary&appname=MongoDB%20Compass&ssl=false"
//            };

//            MongoCRUDConnectionOptions StreamersDB = new MongoCRUDConnectionOptions
//            {
//                Database = "TestingStreamersAgain",
//                ConnectionString = "mongodb://localhost:27017/?readPreference=primary&appname=MongoDB%20Compass&ssl=false"
//            };

//            LDB = new MongoCRUD(StreamersDB);
//            RemindersDB = new MongoCRUD(RemindersDBConOptions);

//            services.AddSingleton(bot);
//            services.AddSingleton(LDB);
//            services.AddSingleton(RemindersDB);
//        }

//        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
//        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
//        {

//        }
//    }
//}