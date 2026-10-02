using ProductApi.Models;

namespace ProductApi.Services;

public class ProductStore
{
    private readonly object _lock = new();
    private readonly Dictionary<int, Product> _products = new()
    {
        [1] = new Product { Id = 1, Name = "Notebook", Description = "14-inch student laptop", Price = 350000m, Quantity = 5 },
        [2] = new Product { Id = 2, Name = "Mouse", Description = "Wireless optical mouse", Price = 8000m, Quantity = 15 },
        [3] = new Product { Id = 3, Name = "Keyboard", Description = "Compact mechanical keyboard", Price = 15000m, Quantity = 10 }
    };
    private int _nextId = 4;

    public IReadOnlyList<Product> GetAll()
    {
        lock (_lock) return _products.Values.OrderBy(product => product.Id).Select(Clone).ToArray();
    }

    public Product? Get(int id)
    {
        lock (_lock) return _products.TryGetValue(id, out var product) ? Clone(product) : null;
    }

    public Product Create(Product product)
    {
        lock (_lock)
        {
            var saved = Clone(product);
            saved.Id = _nextId++;
            _products.Add(saved.Id, saved);
            return Clone(saved);
        }
    }

    public Product? Update(int id, Product product)
    {
        lock (_lock)
        {
            if (!_products.ContainsKey(id)) return null;
            var saved = Clone(product);
            saved.Id = id;
            _products[id] = saved;
            return Clone(saved);
        }
    }

    public bool Delete(int id)
    {
        lock (_lock) return _products.Remove(id);
    }

    private static Product Clone(Product product) => new()
    {
        Id = product.Id,
        Name = product.Name,
        Description = product.Description,
        Price = product.Price,
        Quantity = product.Quantity
    };
}
