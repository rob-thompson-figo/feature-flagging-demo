
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.FeatureManagement;
#pragma warning disable ASP0013

namespace FeatureFlagging
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var environmentSetting = builder.Configuration["Environment"];
            if (!Enum.TryParse(environmentSetting, true, out Environment environment))
            {
                throw new InvalidOperationException($"Invalid environment setting: {environmentSetting}");
            }

            builder.Host.ConfigureAppConfiguration(config =>
            {
                var settings = config.Build();
                var connectionString = settings["AppConfigConnectionString"];
                config.AddAzureAppConfiguration(options =>
                {
                    options.Connect(connectionString)
                        .Select(KeyFilter.Any, LabelFilter.Null)     // Fetch flags with no label
                        .Select(KeyFilter.Any, environment.ToString().ToLowerInvariant())  // Fetch flags for the specific environment
                        .UseFeatureFlags();
                });
            });


            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddFeatureManagement(); //Register the feature management services

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();




            app.MapControllers();

            app.Run();
        }
    }
}
