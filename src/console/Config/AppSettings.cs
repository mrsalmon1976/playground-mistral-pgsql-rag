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
        
        private static string _basePath = AppDomain.CurrentDomain.BaseDirectory;

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

        public string CricketLawChangeProposalDocumentPath => Path.Combine(_basePath, "cricket_law_change_proposals.txt");

        public static AppSettings Load()
        {
            var appConfig = new ConfigurationBuilder()
                .SetBasePath(_basePath)
                .AddJsonFile("AppSettings.json")
                .Build()
                .GetRequiredSection("AppSettings");

            var appSecrets = new ConfigurationBuilder()
                .SetBasePath(_basePath)
                .AddJsonFile("AppSecrets.json")
                .Build();

            return  new AppSettings(appConfig, appSecrets);
        }
    }
}
