using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.Configuration;


namespace SFAuthApp2
{
    // Main program class
    public class Program
    {
        // HttpClient for making HTTP requests
        public static HttpClient HttpClient { get; set; } = new HttpClient();

        // Access token for Salesforce authentication
        public static string AccessToken { get; private set; } // Declare AccessToken at the class level

        // Main method
        public static async Task Main(string[] args)
        {
            // Salesforce authentication endpoint
            string authEndpoint = "https://login.salesforce.com/services/oauth2/token";
            string clientId = ConfigurationManager.AppSettings["SalesforceClientId"];
            string clientSecret = ConfigurationManager.AppSettings["SalesforceClientSecret"];
            string username = ConfigurationManager.AppSettings["SalesforceUsername"];
            string password = ConfigurationManager.AppSettings["SalesforcePassword"] + ConfigurationManager.AppSettings["SalesforceSecurityToken"];

            // Set the base address for HttpClient
            HttpClient.BaseAddress = new Uri("https://your-salesforce-instance-url/services/data/v57.0/query/?q=SELECT+Id,Name+FROM+Account"); // Replace with your actual Salesforce base URL

            // Authenticate Salesforce
            bool authenticationResult = await AuthenticateSalesforce(authEndpoint, clientId, clientSecret, username, password);

            if (authenticationResult)
            {
                // Retrieve Salesforce accounts with the authenticated access token
                await RetrieveAccounts(AccessToken); // AccessToken is accessible here
            }
            else
            {
                Console.WriteLine("Salesforce authentication failed.");
            }

            Console.WriteLine("Press Enter to exit.");
            Console.ReadLine();
        }

        // Method to authenticate Salesforce
        public static async Task<bool> AuthenticateSalesforce(string authEndpoint, string clientId, string clientSecret, string username, string password)
        {
            // Prepare the request content for Salesforce authentication
            var requestContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret),
                new KeyValuePair<string, string>("username", username),
                new KeyValuePair<string, string>("password", password)
            });

            // Send the authentication request
            var response = await HttpClient.PostAsync(authEndpoint, requestContent);

            if (response.IsSuccessStatusCode)
            {
                // Read and parse the authentication response
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Authentication response: " + responseContent);

                var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(responseContent);

                if (!string.IsNullOrEmpty(tokenResponse.access_token))
                {
                    // Set the access token upon successful authentication
                    AccessToken = tokenResponse.access_token;
                    Console.WriteLine("Authentication successful. Access token: " + AccessToken);
                    return true;
                }
                else
                {
                    Console.WriteLine("Access token not found in the response.");
                }
            }
            else
            {
                Console.WriteLine("Authentication failed. HTTP Status Code: " + response.StatusCode);
            }

            return false;
        }

        // Method to retrieve Salesforce accounts
        static async Task RetrieveAccounts(string accessToken)
        {
            // Salesforce API endpoint for querying account
            string apiUrl = "https://your-salesforce-instance-url/services/data/v57.0/query/?q=SELECT+Id,Name+FROM+Account";

            using (var apiClient = new HttpClient())
            {
                apiClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);

                // Send the request to retrieve accounts
                var response = await apiClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    // Read and parse the response data
                    var responseData = await response.Content.ReadAsStringAsync();
                    var accountsResponse = JsonConvert.DeserializeObject<AccountsResponse>(responseData);

                    if (accountsResponse.totalSize > 0)
                    {
                        // Display retrieved accounts
                        Console.WriteLine("Accounts retrieved:");
                        Console.WriteLine("Account ID\tAccount Name");

                        foreach (var account in accountsResponse.records)
                        {
                            Console.WriteLine($"{account.Id}\t{account.Name}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("No account records found in the response.");
                    }
                }
                else
                {
                    Console.WriteLine("Failed to retrieve accounts. HTTP Status Code: " + response.StatusCode);

                    // Read and display the error response
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Error response: " + errorResponse);
                }
            }
        }


    }

    // Class to represent the token response from Salesforce authentication
    public class TokenResponse
    {
        public string access_token { get; set; }
        public string instance_url { get; set; }
        public string id { get; set; }
        public string token_type { get; set; }
        public string issued_at { get; set; }
        public string signature { get; set; }
    }

    // Class to represent the response containing Salesforce accounts
    public class AccountsResponse
    {
        [JsonProperty("totalSize")]
        public int totalSize { get; set; }
        [JsonProperty("records")]
        public List<Account> records { get; set; }
    }

    // Class to represent a Salesforce account
    public class Account
    {
        [JsonProperty("Id")]
        public string Id { get; set; }
        [JsonProperty("Name")]
        public string Name { get; set; }
    }
}
