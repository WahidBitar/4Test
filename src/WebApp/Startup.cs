using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace WebApp
{

    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = "http://dev.sb.com/Authentication/";
                    options.RequireHttpsMetadata = false;
                    options.ApiName = "ApiResource";
                });

            services.AddAuthorization(options =>
            {
                options.InvokeHandlersAfterFailure = false;
                options.AddPolicy("accessControlPolicy", policy =>
                {
                    policy.AddAuthenticationSchemes("Bearer");
                    policy.RequireAuthenticatedUser();
                    //policy.RequireAssertion(cxt => cxt.User?.Identity != null);
                    policy.AddRequirements(new AccessControlRequirement(false));
                });
            });

            services.AddTransient<IAuthorizationHandler, AccessControlRequirementSecondHandler>();
            services.AddTransient<IAuthorizationHandler, AccessControlRequirementFirstHandler>();

            var mvc = services.AddMvc(o =>
            {
                o.EnableEndpointRouting = true;
                o.Filters.Add(new AuthorizeFilter("accessControlPolicy"));
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }



        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseMvc();            
        }
    }
}
