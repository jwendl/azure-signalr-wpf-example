using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace VisualAssist.UserInterface.Handlers
{
    public class AuthenticationMessageHandler
        : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var httpRequestHeaders = request.Headers;

            // If you have the following attribute in your interface, the authorization header will be "Bearer", not null.
            // [Headers("Authorization: Bearer")]
            // If we have a token, then we want to use that token - otherwise generate a service to service one.
            var authenticationHeaderValue = httpRequestHeaders.Authorization;
            if (authenticationHeaderValue != null && authenticationHeaderValue.Scheme == "Bearer")
            {
                string[] scopes = new string[] { "" };

                // This shows simplest version assuming there will not be errors 
                // while getting token silently.  If there are, token should be acquired 
                // using interactive API
                var accountList = await App.PublicClientApp.GetAccountsAsync();
                var authResult = await App.PublicClientApp
                    .AcquireTokenSilent(scopes, accountList.FirstOrDefault())
                    .ExecuteAsync();

                httpRequestHeaders.Authorization = new AuthenticationHeaderValue(authenticationHeaderValue.Scheme, authResult.AccessToken);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
