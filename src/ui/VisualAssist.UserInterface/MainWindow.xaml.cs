using EmbeddedMsalCustomWebUi.Wpf;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensibility;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using VisualAssist.UserInterface.Interfaces;
using VisualAssist.UserInterface.Models;

namespace VisualAssist.UserInterface
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
        : Window
    {
        private HubConnection _hubConnection;
        private DispatcherTimer _dispatcherTimer;
        private string _groupName = "testGroup";

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void SignInButton_Click(object sender, RoutedEventArgs e)
        {
            string[] scopes = new string[] { "user.read", ConfigurationManager.AppSettings["Scope"] };

            var app = App.PublicClientApp;
            try
            {
                ResultText.Text = "";

                var authResult = await (app as PublicClientApplication)
                    .AcquireTokenInteractive(scopes)
                    .WithCustomWebUi(new EmbeddedBrowserWebUi(this))
                    .ExecuteAsync();

                DisplayBasicTokenInfo(authResult);
                UpdateSignInState(true);
            }
            catch (Exception ex)
            {
                ResultText.Text = $"Error Acquiring Token:{Environment.NewLine}{ex}";
            }
        }

        private async void SignOutButton_Click(object sender, RoutedEventArgs e)
        {
            IEnumerable<IAccount> accounts = await App.PublicClientApp.GetAccountsAsync();
            try
            {
                while (accounts.Any())
                {
                    await App.PublicClientApp.RemoveAsync(accounts.FirstOrDefault());
                    accounts = await App.PublicClientApp.GetAccountsAsync();
                }

                _dispatcherTimer.Stop();
                await _hubConnection.StopAsync();

                UpdateSignalRState(false);
                UpdateSignInState(false);
            }
            catch (MsalException ex)
            {
                ResultText.Text = $"Error signing-out user: {ex.Message}";
            }
        }

        private void UpdateSignInState(bool signedIn)
        {
            if (signedIn)
            {
                SignOutButton.Visibility = Visibility.Visible;
                SignInButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                ResultText.Text = "";
                TokenInfoText.Text = "";

                SignOutButton.Visibility = Visibility.Collapsed;
                SignInButton.Visibility = Visibility.Visible;
            }
        }

        private void UpdateSignalRState(bool connected)
        {
            if (connected)
            {
                SignalRDisconnectButton.Visibility = Visibility.Visible;
                SignalRConnectButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                ResultText.Text = "";

                SignalRConnectButton.Visibility = Visibility.Visible;
                SignalRDisconnectButton.Visibility = Visibility.Collapsed;
            }
        }

        private void DisplayBasicTokenInfo(AuthenticationResult authResult)
        {
            TokenInfoText.Text = "";
            if (authResult != null)
            {
                TokenInfoText.Text += $"Username: {authResult.Account.Username}" + Environment.NewLine;
                TokenInfoText.Text += $"Token Expires: {authResult.ExpiresOn.ToLocalTime()}" + Environment.NewLine;
            }
        }

        private async void SignalRConnectButton_Click(object sender, RoutedEventArgs e)
        {
            SignalRConnectButton.IsEnabled = false;

            string[] scopes = new string[] { "" };

            // This shows simplest version assuming there will not be errors 
            // while getting token silently.  If there are, token should be acquired 
            // using interactive API
            var accountList = await App.PublicClientApp.GetAccountsAsync();
            var authResult = await App.PublicClientApp
                .AcquireTokenSilent(scopes, accountList.FirstOrDefault())
                .ExecuteAsync();

            ResultText.Text += $"Connecting to Signal R Service{Environment.NewLine}";
            var username = authResult.ClaimsPrincipal.Claims.Where(c => c.Type == "preferred_username").First().Value;
            var userId = authResult.ClaimsPrincipal.Claims.Where(c => c.Type == "oid").First().Value;
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(ConfigurationManager.AppSettings["SignalRNegotiateEndpoint"], options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(authResult.AccessToken);
                })
                .Build();

            _hubConnection.On<ScreenAssistMessageResponse>("eventListener", (screenAssistMessageResponse) =>
            {
                ResultText.Text += $"Message from {screenAssistMessageResponse.SentBy} on {screenAssistMessageResponse.MessageDate}: {screenAssistMessageResponse.Message}{Environment.NewLine}";
            });

            await _hubConnection.StartAsync();

            ResultText.Text += $"Adding user to group named {_groupName}{Environment.NewLine}";
            var visualAssistService = App.ServiceProvider.GetRequiredService<IVisualAssistService>();
            await visualAssistService.AddUserToGroupAsync(new AddUserToGroupRequest()
            {
                UserId = userId,
                Username = username,
                GroupName = _groupName,
            });

            _dispatcherTimer = new DispatcherTimer();
            _dispatcherTimer.Tick += DispatcherTimer_Tick;
            _dispatcherTimer.Interval = new TimeSpan(0, 0, 5);
            _dispatcherTimer.Start();

            UpdateSignalRState(true);

            //string url = "https://localhost:44389/weatherforecast";
            //HttpClient client = new HttpClient();
            //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            //string json = await client.GetStringAsync(url);
            //ResultText.Text = json;
        }

        private async void SignalRDisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            _dispatcherTimer.Stop();
            await _hubConnection.StopAsync();

            UpdateSignalRState(false);
        }

        private async void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            ResultText.Text += $"Sending message to group{Environment.NewLine}";

            var accounts = await App.PublicClientApp.GetAccountsAsync();
            var account = accounts.First();

            var visualAssistService = App.ServiceProvider.GetRequiredService<IVisualAssistService>();
            await visualAssistService.SendMessageToGroupAsync(new ScreenAssistMessageRequest()
            {
                GroupName = _groupName,
                Message = "Test Message",
            });
        }
    }
}
