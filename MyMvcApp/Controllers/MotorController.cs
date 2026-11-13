using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyMvcApp.Data;
using MyMvcApp.Models;

namespace MyMvcApp.Controllers;

public class MotorController(AppDbContext database) : Controller
{
    public async Task<IActionResult> Index() => View(await database.Motorcycles
        .AsNoTracking()
        .Where(product => product.IsActive)
        .OrderBy(product => product.Id)
        .ToListAsync());

    public async Task<IActionResult> Details(int id)
    {
        var motorcycle = await database.Motorcycles.AsNoTracking()
            .FirstOrDefaultAsync(product => product.Id == id && product.IsActive);
        if (motorcycle is null) return NotFound();
        ViewData["Reviews"] = await database.Reviews.AsNoTracking()
            .Where(review => review.MotorcycleId == id && review.IsVisible)
            .OrderByDescending(review => review.CreatedAtUtc)
            .ToListAsync();
        return View(motorcycle);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitReview(ReviewInput input)
    {
        var product = await database.Motorcycles.FirstOrDefaultAsync(item =>
            item.Id == input.MotorcycleId && item.IsActive);
        if (product is null) return NotFound();

        if (!ModelState.IsValid)
        {
            TempData["ReviewError"] = "Vui lòng nhập tên, email, số sao và nội dung đánh giá hợp lệ (ít nhất 10 ký tự).";
            return RedirectToAction(nameof(Details), new { id = input.MotorcycleId });
        }

        database.Reviews.Add(new MotorcycleReview
        {
            MotorcycleId = product.Id,
            ReviewerName = input.ReviewerName.Trim(),
            ReviewerEmail = input.ReviewerEmail.Trim().ToLowerInvariant(),
            Rating = input.Rating,
            Comment = input.Comment.Trim()
        });
        await database.SaveChangesAsync();
        TempData["ReviewSuccess"] = "Đã gửi đánh giá của bạn.";
        return RedirectToAction(nameof(Details), new { id = input.MotorcycleId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RequestIncomingProduct(ProductContactRequestInput input)
    {
        var product = await database.Motorcycles.FirstOrDefaultAsync(item =>
            item.Id == input.MotorcycleId && item.IsActive && item.StockQuantity == 0);
        if (product is null) return NotFound();

        if (!ModelState.IsValid)
        {
            TempData["ContactRequestError"] = "Vui lòng nhập họ tên, số điện thoại hợp lệ và kiểm tra email.";
            return RedirectToAction(nameof(Details), new { id = product.Id });
        }

        database.ProductContactRequests.Add(new ProductContactRequest
        {
            MotorcycleId = product.Id,
            FullName = input.FullName.Trim(),
            Phone = input.Phone.Trim(),
            Email = string.IsNullOrWhiteSpace(input.Email) ? null : input.Email.Trim().ToLowerInvariant(),
            Note = string.IsNullOrWhiteSpace(input.Note) ? null : input.Note.Trim()
        });
        await database.SaveChangesAsync();
        TempData["ContactRequestSuccess"] = "Đã nhận thông tin. Nhân viên sẽ liên hệ bạn khi xe về hàng.";
        return RedirectToAction(nameof(Details), new { id = product.Id });
    }

    public async Task<IActionResult> ByCategory(string? brand)
    {
        if (string.IsNullOrWhiteSpace(brand))
        {
            return RedirectToAction(nameof(Index));
        }

        var isType = await database.Motorcycles.AnyAsync(product =>
            product.IsActive && product.Type.ToLower() == brand.ToLower());
        var productsQuery = database.Motorcycles.AsNoTracking().Where(product => product.IsActive);
        var products = await (isType
                ? productsQuery.Where(product => product.Type.ToLower() == brand.ToLower())
                : productsQuery.Where(product => product.Brand.ToLower() == brand.ToLower()))
            .OrderBy(product => product.Id)
            .ToListAsync();

        ViewData["Brand"] = brand;
        ViewData["IsType"] = isType;
        return View(products);
    }
}
