using System;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Shared;
using Shared.Web;
using WebApp.Filters;

namespace WebApp
{

    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddSingleton<IServiceProxy, SingletonServiceProxy>();
            services.AddScoped<IRequestData, RequestData>();
            //services.AddTransient(typeof(IProvider<>), typeof(Provider<>));

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = "http://dev.sbmsec.com/Authentication/";
                    options.RequireHttpsMetadata = false;
                    options.ApiName = "ApiResource";
                });

            services.AddAuthorization(options =>
            {
                options.InvokeHandlersAfterFailure = false;
                options.AddPolicy("accessControl", policy =>
                {
                    policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);                    
                    policy.RequireAssertion(cxt =>
                    {
                        var isAuthenticated = cxt.User?.Identity?.IsAuthenticated ?? false;
                        if (isAuthenticated != true)
                            cxt.Fail();
                        return isAuthenticated;
                    });
                    policy.AddRequirements(new AccessControlRequirement(false));
                });
            });

            services.AddTransient<IAuthorizationHandler, AccessControlRequirementFirstHandler>();
            services.AddTransient<IAuthorizationHandler, AccessControlRequirementSecondHandler>();

            var mvc = services.AddMvc(o =>
            {
                o.EnableEndpointRouting = true;
                o.Filters.Add(typeof(SetRequestDataFilter));
                o.Filters.Add(new AuthorizeFilter("accessControl"));
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }



        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env,IServiceProvider sp)
        {
            ServiceLocator.Initialize(sp.GetService<IServiceProxy>());

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseMvc();            
        }
    }

}
