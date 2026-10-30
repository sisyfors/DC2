using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Formatters;
using RestSharp;

namespace WebApplication2.Services
{
    public class BankApiClient
    {
        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        private readonly RestClient _client;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BankApiClient(RestClient client, IHttpContextAccessor httpContextAccessor)
        {
            _client = client;
            _httpContextAccessor = httpContextAccessor;
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

        private static ApiResult<T>? CheckResponse<T>(RestResponse response)
        {
            if (response.StatusCode == 0)
            {
                return new ApiResult<T>
                {
                    Success = false,
                    ErrorMessage = "An error occured while processing the request"
                };
            }

            if (response.IsSuccessful)
            {
                return null;
            }

            var result = new ApiResult<T> 
            { 
                Success = false, 
                ErrorMessage = "An error occurred while processing the request." 
            };

            try
            {
                var error = JsonSerializer.Deserialize<ApiError>(response.Content ?? "", JsonOpts);
                if (!string.IsNullOrEmpty(error?.Message))
                {
                    result.ErrorMessage = error.Message;
                }

                result.Details = error?.Details;
            }
            catch { }
            return result;
        }

        public async Task<ApiResult<T>> SendAsync<T>(
            Method method,
            string resource,
            object? body = null)
        {
            RestRequest request = new RestRequest(resource, method);

            if (body != null)
            {
                request.AddJsonBody(body);
            }

            var cookie = _httpContextAccessor.HttpContext?.Request.Headers.Cookie.ToString();
            if (!string.IsNullOrWhiteSpace(cookie))
            {
                request.AddHeader("Cookie", cookie);
            }

            RestResponse<T> response;
            try
            {
                response = await _client.ExecuteAsync<T>(request);
            }
            catch (Exception ex) when (ex is HttpRequestException ||
                                       ex is TaskCanceledException ||
                                       ex is InvalidOperationException)
            {
                Console.WriteLine($"[API] {method} {resource} failed: {ex.Message}");
                return new ApiResult<T>
                {
                    Success = false,
                    ErrorMessage = "service is unavailable. please try again later."
                };
            }

            Console.WriteLine($"[API] {method} {resource} -> {(int)response.StatusCode} " +
                      $"{response.ResponseStatus} {response.ErrorMessage}");

            var failure = CheckResponse<T>(response);
            return failure ?? new ApiResult<T> { Success = true, Data = response.Data };
        }

        private async Task<ApiResult<T>> ExecuteAsync<T>(RestRequest request)
        {
            var cookie = _httpContextAccessor.HttpContext?.Request.Headers.Cookie.ToString();
            if (!string.IsNullOrWhiteSpace(cookie))
            {
                request.AddHeader("Cookie", cookie);
            }

            RestResponse<T> response;
            try
            {
                response = await _client.ExecuteAsync<T>(request);
            }
            catch (Exception ex) when (ex is HttpRequestException ||
                                       ex is TaskCanceledException ||
                                       ex is InvalidOperationException)
            {
                Console.WriteLine($"[API] {request.Method} {request.Resource} failed: {ex.Message}");
                return new ApiResult<T>
                {
                    Success = false,
                    ErrorMessage = "service is unavailable. please try again later."
                };
            }

            Console.WriteLine($"[API] {request.Method} {request.Resource} -> {(int)response.StatusCode} " +
                      $"{response.ResponseStatus} {response.ErrorMessage}");

            var failure = CheckResponse<T>(response);
            return failure ?? new ApiResult<T> { Success = true, Data = response.Data };
        }
    }
}
