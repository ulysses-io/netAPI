using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using JsonApiDotNetCore.Configuration;

using Breatheasy_API.Context;
using Breatheasy_API.Models;
using Breatheasy_API.Support;
using Breatheasy_API.Configuration;

namespace Breatheasy_API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {

            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<BreatheasyContext>(options => options
                .UseSqlServer(Configuration.GetConnectionString("DevTest")));

            services.AddControllers();
            services.AddJsonApi<BreatheasyContext>();

            services.Configure<StripeOptions>(options =>
              {
                IConfigurationSection consec = Configuration.GetSection("Stripe");
                options.PublishableKey = consec.GetValue(typeof(string), "PublishableKey").ToString();
                options.SecretKey = consec.GetValue(typeof(string), "SecretKey").ToString();
                options.WebhookSecret = "";
                options.BasicPrice = "1.0";
                options.ProPrice = "5.0";
                options.Domain = "http://localhost:5000";
              });

            services.ConfigureSameSiteNoneCookies();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                IConfigurationSection consec = Configuration.GetSection("Auth0");
                options.Authority = Configuration.GetValue<string>("Auth0:Authority");
                options.Audience = Configuration.GetValue<string>("Auth0:Audience");
            });


            //services.AddSwaggerGen(c =>
            //{
            //    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Breatheasy_API", Version = "v1" });
            //});
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            //if (env.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
            //    app.UseSwagger();
            //    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Breatheasy_API v1"));
            //}

            //context.Database.EnsureCreated();

            //if (!context.People.Any())
            //{
            //    context.People.Add(new Person
            //    {
            //        Name = "John Doe"
            //    });
            //    context.SaveChanges();
            //}

            ////////////// ALLOWS FRONT END TO CONNECT TO BACK FOR STRIPE ///////
            app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseJsonApi();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
