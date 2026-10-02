using ProductClient.Models;

namespace ProductClient.Services;

public interface IProductApiService
{
    Task<IReadOnlyList<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(int id);
    Task<Product> CreateAsync(Product product);
    Task<Product> UpdateAsync(Product product);
    Task DeleteAsync(int id);
    Task TriggerBadRequestAsync();
    Task TriggerServerErrorAsync();
}
