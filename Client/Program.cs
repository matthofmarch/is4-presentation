using System;
using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel.OidcClient;
using Newtonsoft.Json.Linq;

namespace ConsoleClientWithBrowser
{
    public static class Program
    {
        private const string Authority = "https://localhost:5001";
        private const string Api = "https://localhost:5002/WeatherForecast";

        private static OidcClient _oidcClient;

        public static void Main()
        {
            MainAsync().GetAwaiter().GetResult();
        }

        public static async Task MainAsync()
        {
            Console.WriteLine("+-----------------------+");
            Console.WriteLine("|  Sign in with OIDC    |");
            Console.WriteLine("+-----------------------+");
            Console.WriteLine("");
            Console.WriteLine("Next step: Login");
            Console.ReadLine();

            await Login();
        }

        private static async Task Login()
        {
            var port = 6655;
            var browser = new SystemBrowser(port);
            var redirectUri = string.Format($"http://localhost:{port}/");

            var options = new OidcClientOptions
            {
                Authority = Authority,
                ClientId = "interactive.public",
                RedirectUri = redirectUri,
                Scope = "openid profile",
                FilterClaims = false,
                Browser = browser,
                Flow = OidcClientOptions.AuthenticationFlow.AuthorizationCode,
                ResponseMode = OidcClientOptions.AuthorizeResponseMode.Redirect
            };

            _oidcClient = new OidcClient(options);
            var result = await _oidcClient.LoginAsync(new LoginRequest());

            Console.WriteLine("\n\nClaims:");
            foreach (var claim in result.User.Claims) Console.WriteLine("{0}: {1}", claim.Type, claim.Value);

            Console.WriteLine();
            Console.WriteLine($"id token: {result.IdentityToken}");
            Console.WriteLine($"access token:   {result.AccessToken}");
            Console.WriteLine($"refresh token:  {result.RefreshToken}");

            Console.WriteLine("Next step: Call API");
            var key = Console.ReadLine();
            await CallApi(result.AccessToken);
        }

        private static async Task CallApi(string currentAccessToken)
        {
            var apiClient = new HttpClient {BaseAddress = new Uri(Api)};
            apiClient.SetBearerToken(currentAccessToken);

            var response = await apiClient.GetAsync("WeatherForecast");

            if (response.IsSuccessStatusCode)
            {
                var json = JArray.Parse(await response.Content.ReadAsStringAsync());
                Console.WriteLine("\n\n");
                Console.WriteLine(json);
            }
            else
            {
                Console.WriteLine($"Error: {response.ReasonPhrase}");
            }
        }
    }
}