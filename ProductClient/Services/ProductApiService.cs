using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ProductClient.Models;

namespace ProductClient.Services;

public class ProductApiService : IProductApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProductApiService> _logger;

    public ProductApiService(IHttpClientFactory httpClientFactory, ILogger<ProductApiService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("ProductApi");
        _logger = logger;
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync()
    {
        _logger.LogInformation("Calling Web API: GET api/products");
        using var response = await SendAsync(() => _httpClient.GetAsync("api/products"));
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<List<Product>>() ?? [];
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        _logger.LogInformation("Calling Web API: GET api/products/{ProductId}", id);
        using var response = await SendAsync(() => _httpClient.GetAsync($"api/products/{id}"));
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Product {ProductId} was not found by the Web API (HTTP 404)", id);
            return null;
        }
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<Product>();
    }

    public async Task<Product> CreateAsync(Product product)
    {
        _logger.LogInformation("Calling Web API: POST api/products");
        using var response = await SendAsync(() => _httpClient.PostAsJsonAsync("api/products", product));
        await EnsureSuccessAsync(response);
        if (response.StatusCode != HttpStatusCode.Created)
            throw new ProductApiException(response.StatusCode, "API не вернул ожидаемый статус 201 Created.");
        return await response.Content.ReadFromJsonAsync<Product>()
            ?? throw new ProductApiException(response.StatusCode, "API вернул пустой ответ при создании товара.");
    }

    public async Task<Product> UpdateAsync(Product product)
    {
        _logger.LogInformation("Calling Web API: PUT api/products/{ProductId}", product.Id);
        using var response = await SendAsync(() => _httpClient.PutAsJsonAsync($"api/products/{product.Id}", product));
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<Product>()
            ?? throw new ProductApiException(response.StatusCode, "API вернул пустой ответ при изменении товара.");
    }

    public async Task DeleteAsync(int id)
    {
        _logger.LogInformation("Calling Web API: DELETE api/products/{ProductId}", id);
        using var response = await SendAsync(() => _httpClient.DeleteAsync($"api/products/{id}"));
        await EnsureSuccessAsync(response);
    }

    public async Task TriggerBadRequestAsync()
    {
        var invalid = new Product { Name = string.Empty, Description = string.Empty, Price = 0, Quantity = -1 };
        using var response = await SendAsync(() => _httpClient.PostAsJsonAsync("api/products", invalid));
        await EnsureSuccessAsync(response);
    }

    public async Task TriggerServerErrorAsync()
    {
        using var response = await SendAsync(() => _httpClient.GetAsync("api/products/diagnostics/server-error"));
        await EnsureSuccessAsync(response);
    }

    private async Task<HttpResponseMessage> SendAsync(Func<Task<HttpResponseMessage>> send)
    {
        try
        {
            return await send();
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(exception, "Could not connect to the configured Product API");
            throw new ProductApiException(null, "Не удалось связаться с Web API. Проверьте, запущен ли сервер.", exception);
        }
        catch (TaskCanceledException exception)
        {
            _logger.LogError(exception, "The Product API request timed out");
            throw new ProductApiException(null, "Web API не ответил вовремя. Повторите запрос позже.", exception);
        }
    }

    private async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation("Web API responded with HTTP {StatusCode}", (int)response.StatusCode);
            return;
        }

        var detail = await GetApiErrorAsync(response);
        var message = response.StatusCode switch
        {
            HttpStatusCode.BadRequest => $"Web API отклонил данные (HTTP 400). {detail}",
            HttpStatusCode.NotFound => "Товар не найден (HTTP 404).",
            HttpStatusCode.InternalServerError => "Web API сообщил о внутренней ошибке (HTTP 500). Повторите запрос позже.",
            _ => $"Web API вернул HTTP {(int)response.StatusCode}. {detail}"
        };

        _logger.LogWarning("Web API returned HTTP {StatusCode}: {Message}", (int)response.StatusCode, message);
        throw new ProductApiException(response.StatusCode, message);
    }

    private static async Task<string> GetApiErrorAsync(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(json)) return string.Empty;
        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            if (root.TryGetProperty("detail", out var detail)) return detail.GetString() ?? string.Empty;
            if (root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
            {
                var messages = errors.EnumerateObject()
                    .SelectMany(property => property.Value.EnumerateArray())
                    .Select(item => item.GetString())
                    .Where(message => !string.IsNullOrWhiteSpace(message));
                return string.Join(" ", messages);
            }
            if (root.TryGetProperty("title", out var title)) return title.GetString() ?? string.Empty;
        }
        catch (JsonException)
        {
            // Keep the UI message safe and useful even if the API error body is not JSON.
        }

        return string.Empty;
    }
}
