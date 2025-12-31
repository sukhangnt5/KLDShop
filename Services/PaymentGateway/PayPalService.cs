namespace KLDShop.Services.PaymentGateway
{
    using System.Text;
    using System.Text.Json;

    /// <summary>
    /// PayPal Payment Gateway Service
    /// </summary>
    public class PayPalService : IPaymentGateway
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<PayPalService> _logger;

        // PayPal Configuration
        private string _paypalClientId;
        private string _paypalClientSecret;
        private string _paypalApiUrl;
        private string _paypalMode;

        public PayPalService(IConfiguration configuration, IHttpClientFactory httpClientFactory, ILogger<PayPalService> logger)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _logger = logger;

            // Load configuration từ appsettings.json
            _paypalClientId = _configuration["PayPal:ClientId"] ?? "";
            _paypalClientSecret = _configuration["PayPal:ClientSecret"] ?? "";
            _paypalMode = _configuration["PayPal:Mode"] ?? "sandbox"; // sandbox or live
            _paypalApiUrl = _paypalMode == "sandbox" 
                ? "https://api-m.sandbox.paypal.com" 
                : "https://api-m.paypal.com";
            
            _logger.LogInformation($"PayPal Service initialized - Mode: {_paypalMode}, ClientId: {_paypalClientId?.Substring(0, 10)}...");
        }

        /// <summary>
        /// Tạo payment link cho PayPal
        /// </summary>
        public async Task<PaymentResponse> CreatePaymentAsync(PaymentRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(_paypalClientId) || string.IsNullOrEmpty(_paypalClientSecret))
                {
                    return new PaymentResponse
                    {
                        Success = false,
                        Message = "PayPal configuration không đầy đủ. Vui lòng kiểm tra appsettings.json"
                    };
                }

                // Get access token
                var token = await GetAccessTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    return new PaymentResponse
                    {
                        Success = false,
                        Message = "Không thể lấy access token từ PayPal"
                    };
                }

                // Create PayPal order
                var orderData = new
                {
                    intent = "CAPTURE",
                    purchase_units = new[]
                    {
                        new
                        {
                            amount = new
                            {
                                currency_code = "USD",
                                value = Math.Round((double)request.Amount / 23500, 2).ToString("F2") // Convert VND to USD (approximate)
                            },
                            description = request.Description,
                            reference_id = request.OrderId.ToString()
                        }
                    },
                    application_context = new
                    {
                        return_url = $"{request.ReturnUrl}?orderId={request.OrderId}",
                        cancel_url = $"{request.CancelUrl}?orderId={request.OrderId}",
                        brand_name = "KLDShop",
                        locale = "en-US",
                        user_action = "PAY_NOW"
                    }
                };

                var client = _httpClientFactory.CreateClient();
                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(orderData),
                    Encoding.UTF8,
                    "application/json"
                );

                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                var orderDataJson = JsonSerializer.Serialize(orderData);
                _logger.LogInformation($"Creating PayPal order with data: {orderDataJson}");
                
                var response = await client.PostAsync($"{_paypalApiUrl}/v2/checkout/orders", jsonContent);
                var responseBody = await response.Content.ReadAsStringAsync();
                
                _logger.LogInformation($"PayPal Response Status: {response.StatusCode}");
                _logger.LogInformation($"PayPal Response Body: {responseBody}");

                if (response.IsSuccessStatusCode)
                {
                    var jsonDoc = JsonDocument.Parse(responseBody);
                    var root = jsonDoc.RootElement;

                    if (root.TryGetProperty("id", out var orderId) && root.TryGetProperty("links", out var links))
                    {
                        var approveLink = links.EnumerateArray()
                            .FirstOrDefault(link => link.GetProperty("rel").GetString() == "approve");

                        if (approveLink.ValueKind != JsonValueKind.Undefined)
                        {
                            var href = approveLink.GetProperty("href").GetString() ?? "";
                            var orderIdStr = orderId.GetString() ?? "";
                            _logger.LogInformation($"PayPal Order Created: {orderIdStr}");
                            return new PaymentResponse
                            {
                                Success = true,
                                Message = "Tạo payment thành công",
                                PaymentUrl = href,
                                TransactionId = orderIdStr
                            };
                        }
                        else
                        {
                            _logger.LogWarning($"No approve link found in response");
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"No id or links in response. Keys: {string.Join(", ", root.EnumerateObject().Select(p => p.Name))}");
                    }
                }
                else
                {
                    _logger.LogError($"PayPal API Error: {response.StatusCode} - {responseBody}");
                }

                return new PaymentResponse
                {
                    Success = false,
                    Message = "Lỗi tạo order từ PayPal"
                };
            }
            catch (Exception ex)
            {
                return new PaymentResponse
                {
                    Success = false,
                    Message = $"Lỗi khi tạo PayPal payment: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Xác minh callback từ PayPal và trả về transaction ID (capture ID)
        /// </summary>
        public async Task<(bool Success, string? TransactionId)> VerifyPaymentAndGetTransactionAsync(IQueryCollection queryParams)
        {
            try
            {
                _logger.LogInformation($"Verifying PayPal payment - queryParams count: {queryParams.Count}");

                // Kiểm tra token và PayPal order ID
                if (queryParams.TryGetValue("token", out var token) && 
                    queryParams.TryGetValue("PayerID", out var payerId))
                {
                    _logger.LogInformation($"PayPal verification - token: {token}, PayerID: {payerId}");

                    // Lấy access token
                    var accessToken = await GetAccessTokenAsync();
                    if (string.IsNullOrEmpty(accessToken))
                    {
                        _logger.LogError("Failed to get PayPal access token for verification");
                        return (false, null);
                    }

                    // Capture payment
                    var client = _httpClientFactory.CreateClient();
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
                    
                    var captureUrl = $"{_paypalApiUrl}/v2/checkout/orders/{token}/capture";
                    _logger.LogInformation($"Capturing PayPal payment at: {captureUrl}");
                    
                    var response = await client.PostAsync(captureUrl, new StringContent("", Encoding.UTF8, "application/json"));
                    var responseBody = await response.Content.ReadAsStringAsync();

                    _logger.LogInformation($"PayPal Capture Response Status: {response.StatusCode}");
                    _logger.LogInformation($"PayPal Capture Response Body: {responseBody}");

                    if (response.IsSuccessStatusCode)
                    {
                        // Extract transaction ID (capture ID) from response
                        var jsonDoc = JsonDocument.Parse(responseBody);
                        var root = jsonDoc.RootElement;
                        
                        string? transactionId = null;
                        
                        // PayPal returns capture ID in: purchase_units[0].payments.captures[0].id
                        if (root.TryGetProperty("purchase_units", out var purchaseUnits) &&
                            purchaseUnits.GetArrayLength() > 0)
                        {
                            var firstUnit = purchaseUnits[0];
                            if (firstUnit.TryGetProperty("payments", out var payments) &&
                                payments.TryGetProperty("captures", out var captures) &&
                                captures.GetArrayLength() > 0)
                            {
                                var firstCapture = captures[0];
                                if (firstCapture.TryGetProperty("id", out var captureId))
                                {
                                    transactionId = captureId.GetString();
                                    _logger.LogInformation($"PayPal Capture ID (Transaction ID): {transactionId}");
                                }
                            }
                        }
                        
                        if (string.IsNullOrEmpty(transactionId))
                        {
                            _logger.LogWarning($"Could not extract capture ID from response, using order ID instead");
                            transactionId = token.ToString();
                        }
                        
                        _logger.LogInformation($"PayPal payment captured successfully - Order ID: {token}, Transaction ID: {transactionId}");
                        return (true, transactionId);
                    }
                    else
                    {
                        _logger.LogError($"PayPal capture failed: {response.StatusCode} - {responseBody}");
                        return (false, null);
                    }
                }
                else
                {
                    _logger.LogWarning($"PayPal verification - missing token or PayerID");
                }

                return (false, null);
            }
            catch (Exception ex)
            {
                _logger.LogError($"PayPal verification error: {ex.Message} - {ex.StackTrace}");
                return (false, null);
            }
        }

        /// <summary>
        /// Xác minh callback từ PayPal (backward compatibility)
        /// </summary>
        public async Task<bool> VerifyPaymentAsync(IQueryCollection queryParams)
        {
            var (success, _) = await VerifyPaymentAndGetTransactionAsync(queryParams);
            return success;
        }

        /// <summary>
        /// Lấy chi tiết giao dịch từ PayPal
        /// </summary>
        public async Task<PaymentDetail> GetPaymentDetailAsync(string transactionId)
        {
            try
            {
                _logger.LogInformation($"Getting PayPal payment detail for transaction: {transactionId}");

                // Get access token
                var accessToken = await GetAccessTokenAsync();
                if (string.IsNullOrEmpty(accessToken))
                {
                    _logger.LogError("Failed to get PayPal access token");
                    throw new Exception("Không thể lấy access token từ PayPal");
                }

                // Call PayPal API to get order details
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

                var detailsUrl = $"{_paypalApiUrl}/v2/checkout/orders/{transactionId}";
                var response = await client.GetAsync(detailsUrl);
                var responseBody = await response.Content.ReadAsStringAsync();

                _logger.LogInformation($"PayPal GetDetail Response Status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var jsonDoc = JsonDocument.Parse(responseBody);
                    var root = jsonDoc.RootElement;

                    // Extract payment details
                    var status = root.TryGetProperty("status", out var statusProp)
                        ? statusProp.GetString() ?? "UNKNOWN"
                        : "UNKNOWN";

                    decimal amount = 0;
                    if (root.TryGetProperty("purchase_units", out var purchaseUnits) &&
                        purchaseUnits.GetArrayLength() > 0)
                    {
                        var firstUnit = purchaseUnits[0];
                        if (firstUnit.TryGetProperty("amount", out var amountProp) &&
                            amountProp.TryGetProperty("value", out var valueProp))
                        {
                            var valueStr = valueProp.GetString() ?? "0";
                            decimal.TryParse(valueStr, out amount);
                        }
                    }

                    // Extract payment date from create_time
                    var paymentDate = DateTime.UtcNow;
                    if (root.TryGetProperty("create_time", out var createTimeProp))
                    {
                        var createTimeStr = createTimeProp.GetString();
                        if (!string.IsNullOrEmpty(createTimeStr) && DateTime.TryParse(createTimeStr, out var parsedDate))
                        {
                            paymentDate = parsedDate;
                        }
                    }

                    _logger.LogInformation($"PayPal Order Details - Status: {status}, Amount: {amount}");

                    return new PaymentDetail
                    {
                        TransactionId = transactionId,
                        Status = status,
                        Amount = amount,
                        PaymentDate = paymentDate
                    };
                }
                else
                {
                    _logger.LogError($"PayPal GetDetail Error: {response.StatusCode} - {responseBody}");
                    throw new Exception($"PayPal API returned error: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting PayPal payment detail: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Lấy Access Token từ PayPal
        /// </summary>
        private async Task<string> GetAccessTokenAsync()
        {
            try
            {
                _logger.LogInformation("Getting PayPal access token...");
                var client = _httpClientFactory.CreateClient();
                var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_paypalClientId}:{_paypalClientSecret}"));
                client.DefaultRequestHeaders.Add("Authorization", $"Basic {authHeader}");

                var content = new StringContent("grant_type=client_credentials", Encoding.UTF8, "application/x-www-form-urlencoded");
                var response = await client.PostAsync($"{_paypalApiUrl}/v1/oauth2/token", content);
                var responseBody = await response.Content.ReadAsStringAsync();

                _logger.LogInformation($"PayPal Token Response: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var jsonDoc = JsonDocument.Parse(responseBody);
                    var root = jsonDoc.RootElement;

                    if (root.TryGetProperty("access_token", out var token))
                    {
                        var tokenStr = token.GetString() ?? "";
                        _logger.LogInformation("PayPal Access Token obtained successfully");
                        return tokenStr;
                    }
                }
                else
                {
                    _logger.LogError($"PayPal Token Error: {response.StatusCode} - {responseBody}");
                }

                return "";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting PayPal access token: {ex.Message} - {ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Call PayPal API - Generic method for making API calls to PayPal
        /// </summary>
        /// <param name="token">PayPal access token</param>
        /// <param name="endpoint">API endpoint (relative path from base URL)</param>
        /// <param name="data">Request data (can be null for GET requests)</param>
        /// <returns>Parsed JSON response as JsonElement</returns>
        private async Task<JsonElement?> CallPayPalApiAsync(string token, string endpoint, object? data = null)
        {
            try
            {
                _logger.LogInformation($"Calling PayPal API: {endpoint}");

                using (var client = _httpClientFactory.CreateClient())
                {
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

                    HttpResponseMessage response;
                    var fullUrl = $"{_paypalApiUrl}/{endpoint}";

                    if (data != null)
                    {
                        // POST request with data
                        var jsonContent = new StringContent(
                            JsonSerializer.Serialize(data),
                            Encoding.UTF8,
                            "application/json"
                        );
                        response = await client.PostAsync(fullUrl, jsonContent);
                    }
                    else
                    {
                        // GET request
                        response = await client.GetAsync(fullUrl);
                    }

                    var responseBody = await response.Content.ReadAsStringAsync();

                    _logger.LogInformation($"PayPal API Response Status: {response.StatusCode}");

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonDoc = JsonDocument.Parse(responseBody);
                        return jsonDoc.RootElement;
                    }
                    else
                    {
                        _logger.LogError($"PayPal API Error: {response.StatusCode} - {responseBody}");
                        throw new Exception($"PayPal API Error: {response.StatusCode} - {responseBody}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error calling PayPal API: {ex.Message}");
                throw;
            }
        }
    }
}
