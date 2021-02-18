using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Task_1.Domain;

namespace Task_1
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IPrimesWorker, PrimesWorker>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    ILogger logger = context.RequestServices.GetRequiredService<ILogger<Startup>>();
                    logger.LogInformation("[CustomLog] Endpoint '/': request received");
                    
                    await context.Response.WriteAsync("Made by Mulish Vadym\n" +
                                                      "Task 1 Prime Numbers Web Service");
                    
                    logger.LogInformation($"[CustomLog] Endpoint '/': request finished with StatusCode {context.Response.StatusCode}");

                });
                
                endpoints.MapGet("/primes/{number:int}", async context =>
                {
                    int number = int.Parse((string) context.Request.RouteValues["number"]);
                    
                    ILogger logger = context.RequestServices.GetRequiredService<ILogger<Startup>>();
                    logger.LogInformation(@"[CustomLog] Endpoint '/primes/{number:int}' with route value " + 
                                          $"'number' = {number}: request received");

                    IPrimesWorker primesWorker = context.RequestServices.GetRequiredService<IPrimesWorker>();


                    bool isPrime = primesWorker.IsPrime(number);
                    context.Response.StatusCode = isPrime ? 200 : 404;
                    
                    logger.LogInformation(@"[CustomLog] Endpoint '/primes/{number:int}' with route value " + 
                                          $"'number' = {number}: request finished with StatusCode {context.Response.StatusCode}");
                });

                endpoints.MapGet("/primes", async context =>
                {
                    ILogger logger = context.RequestServices.GetRequiredService<ILogger<Startup>>();

                    var rangeStart = context.Request.Query["from"].FirstOrDefault();
                    var rangeEnd = context.Request.Query["to"].FirstOrDefault();
                    
                    logger.LogInformation(@"[CustomLog] Endpoint '/primes/' with query parameters " + 
                                          $"'from' = {rangeStart}, 'to' = {rangeEnd}: request received");

                    
                    if (int.TryParse(rangeStart, out int from) && int.TryParse(rangeEnd, out int to))
                    {
                        IPrimesWorker primesWorker = context.RequestServices.GetRequiredService<IPrimesWorker>();
                        int[] primes = primesWorker.FindPrimesInRange(from, to);
                        await context.Response.WriteAsync(JsonSerializer.Serialize(primes));
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                        await context.Response.WriteAsync("response status code: 400 | Bad Request");
                    }
                    
                    logger.LogInformation(@"[CustomLog] Endpoint '/primes/' with query parameters " + 
                                          $"'from' = {rangeStart}, 'to' = {rangeEnd}: request finished with StatusCode {context.Response.StatusCode}");
                });
                
            });
            
            /*
            // 404
            app.Use(next => async context =>
            {
                ILogger logger = context.RequestServices.GetRequiredService<ILogger<Startup>>();
                logger.LogInformation($"[CustomLog] Endpoint '{context.Request.Path}' not found, custom 404 page was displayed: " + 
                                      $"request finished with StatusCode {context.Response.StatusCode}");
                context.Response.StatusCode = 404;
                await context.Response.WriteAsync("404 | Page not found. (custom 404 page)");
            });*/
        }
    }
}
