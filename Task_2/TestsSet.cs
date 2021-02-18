using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Task_2
{
    public class TestsSet
    {
        private static readonly HttpClient HttpClient = new HttpClient();
        private readonly IJsonReader _jsonReader;
        
        public TestsSet(IJsonReader jsonReader)
        {
            _jsonReader = jsonReader;
        }
        
        private bool ConfigureHttpClient()
        {
            string configBaseUrl;
            try
            {
                configBaseUrl = _jsonReader.ReadBaseUrl();   
            }
            catch (DataAccessException exception)
            {
                Console.WriteLine($"Exception occured: {exception.Message}");
                return false;
            }

            try
            {
                HttpClient.BaseAddress = new Uri(configBaseUrl);
            }
            catch (Exception exception)
            {
                if (exception is ArgumentNullException ||
                    exception is UriFormatException)
                {
                    Console.WriteLine($"Exception occured: {exception.Message}");
                    return false; 
                }
                
                throw;
            }
            
            return true;
        }
        public async Task RunAll()
        {
            bool isConfigured = ConfigureHttpClient();
            if (!isConfigured)
            {
                return;
            }

            Task[] tasks = 
            {
                Test_BaseUrl(),
                Test_IsPrimeNumberEndpoint_RequestPrimeNumber(),
                Test_IsPrimeNumberEndpoint_RequestNonPrimeNumber(),
                Test_FindPrimeNumbersInRange_NotEmptyResponse(),
                Test_FindPrimeNumbersInRange_EmptyResponse(),
                Test_FindPrimeNumbersInRange_InvalidQueryParameter()
            };

            await Task.WhenAll(tasks);
        }
        public async Task Test_BaseUrl()
        {
            await TestBase(1, "/", 200, 
                            "Made by Mulish Vadym\nTask 1 Prime Numbers Web Service");
        }

        public async Task Test_IsPrimeNumberEndpoint_RequestPrimeNumber()
        {
            await TestBase(2, "/primes/5", 200, string.Empty);
        }

        public async Task Test_IsPrimeNumberEndpoint_RequestNonPrimeNumber()
        {
            await TestBase(3, "/primes/4", 404, string.Empty);
        }
        
        public async Task Test_FindPrimeNumbersInRange_NotEmptyResponse()
        {
            await TestBase_FindPrimeNumbersInRange(4, "/primes?from=0&to=5", 200, new int[] {2, 3, 5});
        }
        
        public async Task Test_FindPrimeNumbersInRange_EmptyResponse()
        {
            await TestBase_FindPrimeNumbersInRange(5, "/primes?from=-20&to=1", 200, new int[] {});
        }
        
        public async Task Test_FindPrimeNumbersInRange_InvalidQueryParameter()
        {
            await TestBase(6, "/primes?to=1", 400, "response status code: 400 | Bad Request");
        }
        
        private async Task TestBase(
            int testNumber,
            string endpoint,
            int expectedResponseStatusCode,
            string expectedResponseBody)
        {
            //test info | arrange
            StringBuilder builder = new StringBuilder();

            builder.AppendLine($"----------------------Test #{testNumber}----------------------");
            builder.AppendLine($"Testing '{endpoint}' endpoint");
            builder.AppendLine($"Expected response body: '{expectedResponseBody}'");
            builder.AppendLine($"Expected response status code: '{expectedResponseStatusCode}'");
            builder.AppendLine();

            //sending request | act
            HttpResponseMessage response;
            try
            {
                response = await HttpClient.GetAsync(endpoint);
            }
            catch (HttpRequestException exception)
            {
                builder.AppendLine($"Exception occured: {exception.Message}");
                ShowTestSummary(builder);
                
                return;
            }
            
            string actualResponseBody = await response.Content.ReadAsStringAsync();
            int actualResponseStatusCode = (int)response.StatusCode;

            // checking expected adn actual results | assert
            if (expectedResponseBody.Equals(actualResponseBody))
            {
                builder.AppendLine("Response body matches expected value");
            }
            else
            {
                builder.AppendLine("Response body doesn't match expected value");
                builder.AppendLine($"Actual response body: '{actualResponseBody}'");
            }

            if (expectedResponseStatusCode == actualResponseStatusCode)
            {
                builder.AppendLine("Response status code matches expected value");
            }
            else
            {
                builder.AppendLine("Response status code doesn't match expected value");
                builder.AppendLine($"Actual response status code: '{actualResponseStatusCode}'");
            }
            
            ShowTestSummary(builder);
        }
        
        private async Task TestBase_FindPrimeNumbersInRange(
            int testNumber, 
            string endpoint, 
            int expectedResponseStatusCode,
            int[] expectedResponseArray)
        {
            //test info | arrange
            StringBuilder builder = new StringBuilder();
            
            string expectedResponseString = "null";
            if (expectedResponseArray != null)
            {
                expectedResponseString = $"[{string.Join(", ", expectedResponseArray)}]";
            }

            builder.AppendLine($"----------------------Test #{testNumber}----------------------");
            builder.AppendLine($"Testing '{endpoint}' endpoint");
            builder.AppendLine($"Expected response value: '{expectedResponseString}'");
            builder.AppendLine($"Expected response status code: '{expectedResponseStatusCode}'");
            builder.AppendLine();
            
            //sending request | act
            HttpResponseMessage response;
            try
            {
                response = await HttpClient.GetAsync(endpoint);
            }
            catch (HttpRequestException exception)
            {
                builder.AppendLine($"Exception occured: {exception.Message}");
                ShowTestSummary(builder);
                
                return;
            }
            
            string actualResponseBody = await response.Content.ReadAsStringAsync();
            int actualResponseStatusCode = (int)response.StatusCode;
            
            // checking expected adn actual results | assert
            if (expectedResponseArray == null)
            {
                if (string.IsNullOrEmpty(actualResponseBody) || actualResponseBody.Trim().ToLower().Equals("null"))
                {
                    builder.AppendLine("Response body value matches expected value");
                }
                else
                {
                    builder.AppendLine("Response body value doesn't match expected value");
                    builder.AppendLine($"Actual response body: '{actualResponseBody}'");
                }
            }
            else if (actualResponseBody != null)
            {
                int[] actualResponseArray;
                try
                {
                    actualResponseArray = JsonSerializer.Deserialize<int[]>(actualResponseBody);
                }
                catch (JsonException exception)
                {
                    builder.AppendLine($"Exception occured: {exception.Message}");
                    ShowTestSummary(builder);
                
                    return;
                }
                
                if (actualResponseArray != null && actualResponseArray.SequenceEqual(expectedResponseArray))
                {
                    builder.AppendLine("Response body value matches expected value");
                }
                else
                {
                    builder.AppendLine("Response body value doesn't match expected value");
                    builder.AppendLine($"Actual response body: '{actualResponseBody}'");
                }
            }
            else
            {
                builder.AppendLine("Response body value doesn't match expected value");
                builder.AppendLine("Actual response body: 'null'");
            }
            

            if (expectedResponseStatusCode == actualResponseStatusCode)
            {
                builder.AppendLine("Response status code matches expected value");
            }
            else
            {
                builder.AppendLine("Response status code doesn't match expected value");
                builder.AppendLine($"Actual response status code: '{actualResponseStatusCode}'");
            }
            
            ShowTestSummary(builder);
        }
        
        private void ShowTestSummary(StringBuilder stringBuilder)
        {
            stringBuilder.AppendLine();
            string testSummary = stringBuilder.ToString();
            Console.WriteLine(testSummary);
        }
    }
}