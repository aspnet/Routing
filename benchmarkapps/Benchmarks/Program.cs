// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Benchmarks
{
    public class Program
    {
        public static void Main(string[] args)
        {
            GetWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder GetWebHostBuilder(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddCommandLine(args)
                .Build();

            // Consoler logger has a major impact on perf results, so do not use
            // default builder.

            var webHostBuilder = new WebHostBuilder()
                    .UseConfiguration(config)
                    .UseKestrel();

            var scenario = config["scenario"]?.ToLower();
            if (scenario == "plaintextdispatcher")
            {
                webHostBuilder.UseStartup<StartupUsingDispatcher>();
                // for testing
                webHostBuilder.UseSetting("Startup", nameof(StartupUsingDispatcher));
            }
            else
            {
                webHostBuilder.UseStartup<StartupUsingRouting>();
                // for testing
                webHostBuilder.UseSetting("Startup", nameof(StartupUsingRouting));
            }

            return webHostBuilder;
        }
    }
}
