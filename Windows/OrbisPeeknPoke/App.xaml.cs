﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace OrbisPeeknPoke
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ServiceProvider _serviceProvider;

        public App()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddSingleton<MainWindow>();

            services.AddLogging(builder =>
            {
                builder.AddSimpleConsole(options =>
                {
                    options.IncludeScopes = false;
                    options.SingleLine = true;
                    options.TimestampFormat = "HH:mm:ss ";
                });
            });

            _serviceProvider = services.BuildServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            var logger = _serviceProvider.GetService<ILoggerFactory>()
            .AddFile(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\Orbis Suite\Logging\OrbisPeeknPokeLog.txt")
            .CreateLogger<MainWindow>();

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show(logger);

            base.OnStartup(e);
        }
    }
}
