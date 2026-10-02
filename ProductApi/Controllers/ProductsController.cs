using Microsoft.AspNetCore.Mvc;
using ProductApi.Models;
using ProductApi.Services;

namespace ProductApi.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController(ProductStore store, IWebHostEnvironment environment, ILogger<ProductsController> logger) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Product>), StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<Product>> GetAll()
    {
        logger.LogInformation("GET all products");
        return Ok(store.GetAll());
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<Product> GetById(int id)
    {
        var product = store.Get(id);
        if (product is null)
        {
            logger.LogWarning("Product {ProductId} not found", id);
            return NotFound(new ProblemDetails { Title = "Product not found", Detail = $"Product {id} was not found." });
        }

        logger.LogInformation("GET product {ProductId}", id);
        return Ok(product);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Product), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<Product> Create(Product product)
    {
        var created = store.Create(product);
        logger.LogInformation("POST created product {ProductId}", created.Id);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<Product> Update(int id, Product product)
    {
        var updated = store.Update(id, product);
        if (updated is null)
        {
            logger.LogWarning("Cannot update missing product {ProductId}", id);
            return NotFound(new ProblemDetails { Title = "Product not found", Detail = $"Product {id} was not found." });
        }

        logger.LogInformation("PUT updated product {ProductId}", id);
        return Ok(updated);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(int id)
    {
        if (!store.Delete(id))
        {
            logger.LogWarning("Cannot delete missing product {ProductId}", id);
            return NotFound(new ProblemDetails { Title = "Product not found", Detail = $"Product {id} was not found." });
        }

        logger.LogInformation("DELETE product {ProductId}", id);
        return Ok(new { message = $"Product {id} deleted." });
    }

    [HttpGet("diagnostics/server-error")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult SimulateServerError()
    {
        if (!environment.IsDevelopment()) return NotFound();
        logger.LogWarning("Returning the intentional development-only HTTP 500 test response");
        return StatusCode(StatusCodes.Status500InternalServerError,
            new ProblemDetails { Title = "Simulated server error", Detail = "This development endpoint verifies client error handling." });
    }
}
