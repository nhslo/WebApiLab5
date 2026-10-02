using System.ComponentModel.DataAnnotations;

namespace ProductClient.Models;

public class Product
{
    public int Id { get; set; }

    [Required, StringLength(160, MinimumLength = 1)]
    [Display(Name = "Название")]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(1000, MinimumLength = 1)]
    [Display(Name = "Описание")]
    public string Description { get; set; } = string.Empty;

    [Range(typeof(decimal), "1", "1000000000")]
    [Display(Name = "Цена")]
    public decimal Price { get; set; }

    [Range(0, 1000000)]
    [Display(Name = "Количество")]
    public int Quantity { get; set; }
}
