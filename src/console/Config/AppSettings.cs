using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mistral.Config
{
    internal class AppSettings
    {
        private readonly IConfigurationSection _appSettings;
        private readonly IConfigurationRoot _appSecrets;

        private AppSettings(IConfigurationSection? appSettings, IConfigurationRoot? appSecrets)
        {
            if (appSettings == null) 
            {
                throw new InvalidOperationException("AppSettings file not loaded");
            }
            if (appSecrets == null)
            {
                throw new InvalidOperationException("AppSecrets file not loaded");
            }
            this._appSettings = appSettings;
            this._appSecrets = appSecrets;
        }

        public string TestKey  => this._appSettings["TestKey"]!;

        public string MistralApiKey => this._appSecrets["MistralApiKey"]!;

        public static AppSettings Load()
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;

            var appConfig = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("AppSettings.json")
                .Build()
                .GetRequiredSection("AppSettings");

            var appSecrets = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("AppSecrets.json")
                .Build();

            return  new AppSettings(appConfig, appSecrets);
        }
    }
}
