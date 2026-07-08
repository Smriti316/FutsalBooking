using System.Text;
using System.Text.Json;

namespace FutsalBooking.Services
{
    // this service handles communication with Khalti payment API
    // khalti docs: https://docs.khalti.com/khalti-epayment/
    public class KhaltiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public KhaltiService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;

            // set secret key in header for every request
            string secretKey = _config["Khalti:SecretKey"] ?? "";
            _httpClient.DefaultRequestHeaders.Add("Authorization", "Key " + secretKey);
        }

        // step 1: initiate payment - tells khalti we want to charge this amount
        // returns a payment url where user is redirected to pay
        public async Task<KhaltiInitiateResponse?> InitiatePayment(KhaltiInitiateRequest request)
        {
            try
            {
                string baseUrl = _config["Khalti:BaseUrl"] ?? "https://a.khalti.com/api/v2/";
                string url = baseUrl + "epayment/initiate/";

                string jsonBody = JsonSerializer.Serialize(request);
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);
                string responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<KhaltiInitiateResponse>(responseBody,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return result;
                }
                else
                {
                    // log error - khalti returned error
                    Console.WriteLine("Khalti initiate failed: " + responseBody);
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Khalti error: " + ex.Message);
                return null;
            }
        }

        // step 2: verify payment - after user pays, khalti sends back a token
        // we verify it with khalti to confirm the payment actually happened
        public async Task<KhaltiVerifyResponse?> VerifyPayment(string pidx)
        {
            try
            {
                string baseUrl = _config["Khalti:BaseUrl"] ?? "https://a.khalti.com/api/v2/";
                string url = baseUrl + "epayment/lookup/";

                var body = new { pidx = pidx };
                string jsonBody = JsonSerializer.Serialize(body);
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);
                string responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<KhaltiVerifyResponse>(responseBody,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return result;
                }
                else
                {
                    Console.WriteLine("Khalti verify failed: " + responseBody);
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Khalti verify error: " + ex.Message);
                return null;
            }
        }
    }

    // request object to send to khalti
    public class KhaltiInitiateRequest
    {
        public string return_url { get; set; } = string.Empty;    // where to redirect after payment
        public string website_url { get; set; } = string.Empty;   // your website url
        public long amount { get; set; }                           // amount in PAISA (Rs.1 = 100 paisa)
        public string purchase_order_id { get; set; } = string.Empty;  // your booking id
        public string purchase_order_name { get; set; } = string.Empty; // description
        public KhaltiCustomer? customer_info { get; set; }
    }

    public class KhaltiCustomer
    {
        public string name { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string phone { get; set; } = string.Empty;
    }

    // response from khalti after initiating
    public class KhaltiInitiateResponse
    {
        public string pidx { get; set; } = string.Empty;       // payment id - save this
        public string payment_url { get; set; } = string.Empty; // redirect user here
        public string expires_at { get; set; } = string.Empty;
    }

    // response from khalti after verifying
    public class KhaltiVerifyResponse
    {
        public string pidx { get; set; } = string.Empty;
        public string status { get; set; } = string.Empty;      // "Completed" means success
        public long total_amount { get; set; }
        public string transaction_id { get; set; } = string.Empty;
    }
}
