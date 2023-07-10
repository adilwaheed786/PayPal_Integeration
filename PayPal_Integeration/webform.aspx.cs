using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace PayPal_Integeration
{
    public partial class webform : System.Web.UI.Page
    {
        public  string clientId = ConfigurationManager.AppSettings["PayPalClientId"];
        public string clientSecret = ConfigurationManager.AppSettings["PayPalClientSecret"];
        public string emailAddress = ConfigurationManager.AppSettings["EmailAddress"];
        public static string baseurl = ConfigurationManager.AppSettings["base_url"];
        protected void Page_Load(object sender, EventArgs e)
        {
            //bool isSubscriptionSuccessful = true; // Set to true or false based on your logic

            //if (isSubscriptionSuccessful)
            //{
            //    // Clear the session value
            //    Session["SubscriptionPlanId"] = null;
            //}
        }
        private static async Task<string> GetAccessToken(string clientId, string clientSecret)
        {
            string tokenEndpoint = $"{baseurl}/v1/oauth2/token";

            // Set up the request parameters
            var requestData = new Dictionary<string, string>
              {
                  { "grant_type", "client_credentials" }
              };
            var requestContent = new FormUrlEncodedContent(requestData);

            // Create a Base64-encoded string of the client credentials
            string credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));

            // Set up the HTTP client
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

            // Send the POST request to the token endpoint
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            HttpResponseMessage response = await client.PostAsync(tokenEndpoint, requestContent);

            // Process the response
            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                dynamic responseData = JsonConvert.DeserializeObject(responseContent);

                // Extract and return the access token
                string accessToken = responseData.access_token;
                return accessToken;
            }
            else
            {
                // Handle the case when the token request fails
                string errorContent = await response.Content.ReadAsStringAsync();
                // Log or display an error message
                return null;
            }
        }
        private static async Task<string> CreateProduct(string accessToken)
        {
            string apiUrl = $"{baseurl}/v1/catalogs/products";

            // Set up the request payload
            var product = new
            {
                name="Donate A Money",
                type= "SERVICE",
                description="Donation For Needy People",
                category= "SERVICES"
            };

            // Convert the product object to JSON
            string jsonPayload = JsonConvert.SerializeObject(product);

            // Set up the HTTP client
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            // Send the POST request to create the product
            StringContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            HttpResponseMessage response = await client.PostAsync(apiUrl, content);

            // Process the response
            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                dynamic responseData = JsonConvert.DeserializeObject(responseContent);

                // Extract and return the product ID
                string productId = responseData.id;
                return productId;
            }
            else
            {
                // Handle the case when the product creation fails
                string errorContent = await response.Content.ReadAsStringAsync();
                // Log or display an error message
                return null;
            }
        }

        protected async void btnDonate_Click1(object sender, EventArgs e)
        {
            decimal amount = decimal.Parse(userAmount.Value);
            bool isRecurringPayment = ddlRecurringPeriod.SelectedValue != "0";
            var frequency = ddlRecurringPeriod.SelectedItem.Text;
            var recurrenceCount = ddlRecurringTimes.SelectedItem.Text;
            // Store the donation amount in a session variable or pass it as a query parameter to the checkout page
            Session["DonationAmount"] = amount;

            // Redirect to the PayPal checkout page
            string paypalUrl = "https://www.paypal.com/cgi-bin/webscr";
            string businessEmail = emailAddress; // Replace with your PayPal business email
            string itemName = "Testing Donation";
            string returnURL = "https://example.com/success"; // Replace with your success return URL
            string cancelURL = "https://example.com/cancel"; // Replace with your cancel return URL



            if (isRecurringPayment)
            {
                // Create PayPal subscription

                // Set up the API request payload
                string apiEndpoint = $"{baseurl}/v1/billing/plans";
                string accessToken = await GetAccessToken(clientId, clientSecret); // Replace with your access token
                string productID = await CreateProduct(accessToken); // Replace with your product ID
                string planName = "Subscription Plan";
                string planDescription = "Subscription plan for recurring donations";
                string currencyCode = "USD";

                // Create the billing cycle object based on the selected frequency and recurrence count
                var billingCycles = new List<object>();
                switch (frequency)
                {
                    case "weekly":
                        billingCycles.Add(new
                        {
                            frequency = new
                            {
                                interval_unit = "WEEK",
                                interval_count = 1
                            },
                            tenure_type = "REGULAR",
                            sequence = 1,
                            total_cycles = int.Parse(recurrenceCount),
                            pricing_scheme = new
                            {
                                fixed_price = new
                                {
                                    value = amount.ToString(),
                                    currency_code = currencyCode
                                }
                            }
                        });
                        break;
                    case "monthly":
                        billingCycles.Add(new
                        {
                            frequency = new
                            {
                                interval_unit = "MONTH",
                                interval_count = 1
                            },
                            tenure_type = "REGULAR",
                            sequence = 1,
                            total_cycles = int.Parse(recurrenceCount),
                            pricing_scheme = new
                            {
                                fixed_price = new
                                {
                                    value = amount.ToString(),
                                    currency_code = currencyCode
                                }
                            }
                        });
                        break;
                    case "yearly":
                        billingCycles.Add(new
                        {
                            frequency = new
                            {
                                interval_unit = "YEAR",
                                interval_count = 1
                            },
                            tenure_type = "REGULAR",
                            sequence = 1,
                            total_cycles = int.Parse(recurrenceCount),
                            pricing_scheme = new
                            {
                                fixed_price = new
                                {
                                    value = amount.ToString(),
                                    currency_code = currencyCode
                                }
                            }
                        });
                        break;
                }
                // Construct the request payload
                var requestPayload = new
                {
                    product_id = productID,
                    name = planName,
                    description = planDescription,
                    billing_cycles = billingCycles,
                    payment_preferences = new
                    {
                        auto_bill_outstanding = true,
                        setup_fee = new
                        {
                            value = "0",
                            currency_code = currencyCode
                        },
                        setup_fee_failure_action = "CONTINUE",
                        payment_failure_threshold = 3
                    }
                };
                // Convert the request payload to JSON
                string jsonPayload = JsonConvert.SerializeObject(requestPayload);

                // Make the API call to create the subscription plan
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                StringContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                HttpResponseMessage response = await client.PostAsync(apiEndpoint, content);
                // Check if the API call was successful
                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    dynamic responseData = JsonConvert.DeserializeObject(responseContent);

                    // Retrieve the subscription plan ID from the response data
                    string planId = responseData.id;

                    // Store the plan ID in a session variable or pass it as a query parameter to the checkout page
                    Session["SubscriptionPlanId"] = planId;


                }
                else
                {
                    // Handle the case when the API call fails
                    string responseContent = await response.Content.ReadAsStringAsync();
                    // Log or display an error message to the user
                }

            }
            else
            {
                // Build the query string parameters
                string redirectUrl = $"{paypalUrl}?cmd=_xclick&business={businessEmail}&item_name={itemName}&amount={amount}&return={returnURL}&cancel_return={cancelURL}";
                // Redirect the user to the PayPal checkout page
                Response.Redirect(redirectUrl, false);
            }


        }
    }
}