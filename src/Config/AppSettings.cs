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
        private readonly IConfigurationSection _connectionStrings;
        private readonly IConfigurationRoot _appSecrets;
        
        private static string _basePath = AppDomain.CurrentDomain.BaseDirectory;

        private AppSettings(IConfigurationRoot? appSettingsRoot, IConfigurationRoot? appSecretsRoot)
        {
            _appSettings = appSettingsRoot!.GetRequiredSection("AppSettings");
            if (_appSettings == null) 
            {
                throw new InvalidOperationException("AppSettings file not loaded");
            }

            if (appSecretsRoot == null)
            {
                throw new InvalidOperationException("AppSecrets file not loaded");
            }

            this._appSecrets = appSecretsRoot;
            this._connectionStrings = appSecretsRoot!.GetRequiredSection("ConnectionStrings");
            if (_connectionStrings == null)
            {
                throw new InvalidOperationException("ConnectionStrings section not declared in appSecrets");
            }
        }



        public string DefaultConnectionString  => this._connectionStrings["Default"]!;

        public string MistralApiKey => this._appSecrets["MistralApiKey"]!;

        public string CricketLawChangeProposalDocumentPath => Path.Combine(_basePath, "Resources", "cricket_law_change_proposals.txt");

        public string CricketBookPath => Path.Combine(_basePath, "Resources", "cricket.txt");

        public static AppSettings Load()
        {
            var appConfig = new ConfigurationBuilder()
                .SetBasePath(_basePath)
                .AddJsonFile("AppSettings.json")
                .Build();

            var appSecrets = new ConfigurationBuilder()
                .SetBasePath(_basePath)
                .AddJsonFile("AppSecrets.json")
                .Build();

            return  new AppSettings(appConfig, appSecrets);
        }
    }
}
