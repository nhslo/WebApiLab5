using System.ComponentModel.DataAnnotations;

namespace ProductApi.Models;

public class Product
{
    public int Id { get; set; }

    [Required, StringLength(160, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(1000, MinimumLength = 1)]
    public string Description { get; set; } = string.Empty;

    [Range(typeof(decimal), "1", "1000000000")]
    public decimal Price { get; set; }

    [Range(0, 1000000)]
    public int Quantity { get; set; }
}
