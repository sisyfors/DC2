using System.Text.Json;
using RestSharp;

namespace WebApplication2.Services
{
    public class BankApiClient
    {
        private readonly RestClient _client;
        public BankApiClient(RestClient client)
        {
            _client = client;
        }
        public Task<ApiResult<T>> GetAsync<T>(
            string resource,
            Dictionary<string, string?>? query = null)
        {
            RestRequest request = new RestRequest(resource, Method.Get);

            if (query != null)
            {
                foreach (var (key, value) in query)
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        request.AddQueryParameter(key, value);
                    }
                }
            }

            return ExecuteAsync<T>(request);
        }

        public Task<ApiResult<T>> SendAsync<T>(
            Method method,
            string resource,
            object? body = null)
        {
            RestRequest request = new RestRequest(resource, method);

            if (body != null)
            {
                request.AddJsonBody(body);
            }

            return ExecuteAsync<T>(request);
        }

        private async Task<ApiResult<T>> ExecuteAsync<T>(RestRequest request)
        {
            var response = await _client.ExecuteAsync<T>(request);

            // time out if unreachable
            if (response.ResponseStatus != ResponseStatus.Completed)
            {
                return new ApiResult<T>
                {
                    Success = false,
                    ErrorMessage = "The request timed out or the server is unreachable."
                };
            }

            if (!response.IsSuccessful)
            {
                ApiResult<T> result = new ApiResult<T>
                {
                    Success = false,
                    ErrorMessage = "An error occurred while processing the request."
                };

                try
                {
                    var error = JsonSerializer.Deserialize<ApiError>(
                        response.Content ?? string.Empty);

                    if (!string.IsNullOrEmpty(error?.Message))
                    {
                        result.ErrorMessage = error.Message;
                        result.Details = error?.Details;
                    }
                }
                catch
                {
                    // Ignore JSON parsing errors and return the default error message
                }

                return result;
            }

            return new ApiResult<T>
            {
                Success = true,
                Data = response.Data
            };
        }
    }
}
