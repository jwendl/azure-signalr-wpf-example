using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;
using Polly;
using Polly.Extensions.Http;
using System;
using System.Configuration;
using System.IO;
using System.Text;
using System.Windows;

namespace VisualAssist.UserInterface
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
        : Application
    {
        private static readonly string ClientId = ConfigurationManager.AppSettings["ClientId"];
        private static readonly string Tenant = ConfigurationManager.AppSettings["Tenant"];
        private static readonly string Instance = ConfigurationManager.AppSettings["Instance"];

        public static IPublicClientApplication PublicClientApp { get; private set; }
        public static IServiceProvider ServiceProvider { get; private set; }

        static App()
        {
            PublicClientApp = PublicClientApplicationBuilder.Create(ClientId)
                .WithAuthority($"{Instance}{Tenant}")
                .WithRedirectUri("http://localhost")
                .Build();

            TokenCacheHelper.Bind(PublicClientApp.UserTokenCache);

            var asyncRetryPolicy = HttpPolicyExtensions.HandleTransientHttpError()
                .WaitAndRetryAsync(3, retryAttempts => TimeSpan.FromMilliseconds(Math.Pow(2, retryAttempts)));

            var serviceCollection = new ServiceCollection();
            //serviceCollection.AddSingleton<AuthenticationMessageHandler>();
            //serviceCollection.AddRefitClient<IVisualAssistService>((sp) =>
            //{
            //    var jsonSerializerOptions = new JsonSerializerOptions()
            //    {
            //        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            //        WriteIndented = true,
            //    };

            //    return new RefitSettings()
            //    {
            //        ContentSerializer = new SystemTextJsonContentSerializer(jsonSerializerOptions),
            //    };
            //})
            //    .AddPolicyHandler(asyncRetryPolicy)
            //    .AddHttpMessageHandler<AuthenticationMessageHandler>()
            //    .ConfigureHttpClient(http => http.BaseAddress = new Uri(ConfigurationManager.AppSettings["FunctionAppUrl"]));
            ServiceProvider = serviceCollection.BuildServiceProvider();
        }
        private static void Log(LogLevel level, string message, bool containsPii)
        {
            string logs = ($"{level} {message}");
            StringBuilder sb = new StringBuilder();
            sb.Append(logs);
            File.AppendAllText(System.Reflection.Assembly.GetExecutingAssembly().Location + ".msalLogs.txt", sb.ToString());
            sb.Clear();
        }
    }
}
