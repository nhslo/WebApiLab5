using Microsoft.AspNetCore.Mvc;
using ProductClient.Models;
using ProductClient.Services;

namespace ProductClient.Controllers;

public class ProductController(IProductApiService productApiService, IWebHostEnvironment environment) : Controller
{
    public async Task<IActionResult> Index()
    {
        try
        {
            var products = await productApiService.GetAllAsync();
            return View(products);
        }
        catch (ProductApiException exception)
        {
            ViewBag.ErrorMessage = exception.Message;
            return View(Array.Empty<Product>());
        }
    }

    [HttpGet]
    public async Task<IActionResult> Details(int? id)
    {
        if (id is null or <= 0)
        {
            ViewBag.ErrorMessage = "Укажите положительный ID товара.";
            return View(model: null);
        }

        try
        {
            var product = await productApiService.GetByIdAsync(id.Value);
            if (product is null) ViewBag.ErrorMessage = "Товар не найден.";
            return View(product);
        }
        catch (ProductApiException exception)
        {
            ViewBag.ErrorMessage = exception.Message;
            return View(model: null);
        }
    }

    [HttpGet]
    public IActionResult Create() => View(new Product());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product product)
    {
        if (!ModelState.IsValid) return View(product);

        try
        {
            var created = await productApiService.CreateAsync(product);
            TempData["SuccessMessage"] = $"Товар «{created.Name}» добавлен (ID {created.Id}).";
            return RedirectToAction(nameof(Index));
        }
        catch (ProductApiException exception)
        {
            ViewBag.ErrorMessage = exception.Message;
            return View(product);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var product = await productApiService.GetByIdAsync(id);
            if (product is null)
            {
                TempData["ErrorMessage"] = "Товар не найден (HTTP 404).";
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }
        catch (ProductApiException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Product product)
    {
        if (id != product.Id) return BadRequest();
        if (!ModelState.IsValid) return View(product);

        try
        {
            var updated = await productApiService.UpdateAsync(product);
            TempData["SuccessMessage"] = $"Изменения товара «{updated.Name}» сохранены.";
            return RedirectToAction(nameof(Index));
        }
        catch (ProductApiException exception)
        {
            ViewBag.ErrorMessage = exception.Message;
            return View(product);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var product = await productApiService.GetByIdAsync(id);
            if (product is null)
            {
                TempData["ErrorMessage"] = "Товар не найден (HTTP 404).";
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }
        catch (ProductApiException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            await productApiService.DeleteAsync(id);
            TempData["SuccessMessage"] = $"Товар с ID {id} удалён.";
        }
        catch (ProductApiException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> TestBadRequest()
    {
        try
        {
            await productApiService.TriggerBadRequestAsync();
            TempData["SuccessMessage"] = "Тестовый API запрос неожиданно прошёл проверку.";
        }
        catch (ProductApiException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> TestServerError()
    {
        if (!environment.IsDevelopment()) return NotFound();
        try
        {
            await productApiService.TriggerServerErrorAsync();
        }
        catch (ProductApiException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
        }
        return RedirectToAction(nameof(Index));
    }
}
