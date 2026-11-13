using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyMvcApp.Data;
using MyMvcApp.Models;

namespace MyMvcApp.Controllers;

public class ShoppingController(AppDbContext database) : Controller
{
    public IActionResult Cart() => View();

    [HttpGet]
    public async Task<IActionResult> Checkout()
    {
        ViewData["OrderPlaced"] = TempData["OrderPlaced"];
        if (User.IsInRole("Customer") && int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var customerId))
        {
            var customer = await database.Customers.AsNoTracking().FirstOrDefaultAsync(item => item.Id == customerId, HttpContext.RequestAborted);
            if (customer is not null)
            {
                ViewData["CheckoutFullName"] = customer.FullName;
                ViewData["CheckoutPhone"] = customer.Phone;
                ViewData["CheckoutEmail"] = customer.Email;
                ViewData["CheckoutAddress"] = customer.Address;
            }
        }
        await SetPaymentAvailabilityAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(string fullName, string phone, string email, string address,
        string paymentMethod, string cartJson, string? promotionCode)
    {
        if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(phone)
            || string.IsNullOrWhiteSpace(address) || !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(email))
        {
            ModelState.AddModelError(string.Empty, "Vui lòng điền đầy đủ và chính xác thông tin nhận xe.");
        }

        var settingValues = await database.StoreSettings.AsNoTracking().ToDictionaryAsync(setting => setting.Key, setting => setting.Value);
        var paymentSettingKey = paymentMethod == "bank" ? "BankTransfer" : "PayOnDelivery";
        var paymentEnabled = !settingValues.TryGetValue(paymentSettingKey, out var paymentSetting)
            || (bool.TryParse(paymentSetting, out var parsedPaymentSetting) && parsedPaymentSetting);
        ViewData["BankTransferEnabled"] = ReadSetting(settingValues, "BankTransfer", true);
        ViewData["PayOnDeliveryEnabled"] = ReadSetting(settingValues, "PayOnDelivery", true);
        if (paymentMethod is not ("bank" or "consult") || !paymentEnabled)
            ModelState.AddModelError(nameof(paymentMethod), "Phương thức thanh toán này hiện không khả dụng.");

        List<CheckoutCartItem>? cart;
        try
        {
            cart = JsonSerializer.Deserialize<List<CheckoutCartItem>>(cartJson ?? "[]", new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (JsonException)
        {
            cart = null;
        }

        if (cart is null || cart.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "Giỏ hàng đang trống. Hãy thêm xe trước khi đặt hàng.");
        }

        if (!ModelState.IsValid)
        {
            ViewData["CheckoutError"] = true;
            return View();
        }

        var cartItems = cart!;
        var requestedQuantities = cartItems
            .Where(item => int.TryParse(item.Id, out _))
            .GroupBy(item => int.Parse(item.Id))
            .ToDictionary(group => group.Key, group => group.Sum(item => item.Quantity));
        if (requestedQuantities.Count != cartItems.Count || requestedQuantities.Values.Any(quantity => quantity < 1))
        {
            ModelState.AddModelError(string.Empty, "Giỏ hàng có sản phẩm hoặc số lượng không hợp lệ.");
            ViewData["CheckoutError"] = true;
            return View();
        }

        await using var transaction = await database.Database.BeginTransactionAsync();
        var products = await database.Motorcycles
            .Where(product => requestedQuantities.Keys.Contains(product.Id) && product.IsActive
                && product.StockQuantity > 0)
            .ToDictionaryAsync(product => product.Id);
        if (products.Count != requestedQuantities.Count)
        {
            ModelState.AddModelError(string.Empty, "Một số mẫu xe không còn được bán. Hãy cập nhật giỏ hàng.");
            ViewData["CheckoutError"] = true;
            return View();
        }

        foreach (var (productId, quantity) in requestedQuantities)
        {
            var product = products[productId];
            if (product.StockQuantity < quantity)
            {
                ModelState.AddModelError(string.Empty, $"{product.Name} hiện không đủ hàng để đặt mua. Vui lòng chọn mẫu xe khác.");
            }
        }
        if (!ModelState.IsValid)
        {
            ViewData["CheckoutError"] = true;
            return View();
        }

        Promotion? promotion = null;
        if (!string.IsNullOrWhiteSpace(promotionCode))
        {
            var normalizedCode = promotionCode.Trim().ToUpperInvariant();
            promotion = await database.Promotions.FirstOrDefaultAsync(item => item.Code == normalizedCode);
            var today = DateTime.UtcNow.Date;
            if (promotion is null || !promotion.IsActive || promotion.StartsAtUtc.Date > today
                || promotion.EndsAtUtc.Date < today || promotion.UsageLimit > 0 && promotion.UsedCount >= promotion.UsageLimit)
            {
                ModelState.AddModelError(string.Empty, "Mã khuyến mãi không hợp lệ, hết hạn hoặc đã hết lượt sử dụng.");
                ViewData["CheckoutError"] = true;
                return View();
            }
        }

        var normalizedEmail = email.Trim().ToLowerInvariant();
        Customer? customer = null;
        if (User.IsInRole("Customer")
            && int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var signedInCustomerId))
            customer = await database.Customers.FirstOrDefaultAsync(record => record.Id == signedInCustomerId);

        if (customer is not null)
        {
            normalizedEmail = customer.Email;
        }
        else
        {
            customer = await database.Customers.FirstOrDefaultAsync(record => record.Email == normalizedEmail);
        }
        if (customer is null)
        {
            customer = new Customer { FullName = fullName.Trim(), Email = normalizedEmail, Phone = phone.Trim(), Address = address.Trim() };
            database.Customers.Add(customer);
        }
        else
        {
            customer.FullName = fullName.Trim();
            customer.Phone = phone.Trim();
            customer.Address = address.Trim();
        }

        var order = new MotorcycleOrder
        {
            OrderNumber = $"MS-{DateTime.UtcNow:yyMMddHHmmss}-{Guid.NewGuid():N}"[..21].ToUpperInvariant(),
            Customer = customer,
            TotalAmount = 0,
            PromotionCode = promotion?.Code,
            ShippingAddress = address.Trim(),
            PaymentMethod = paymentMethod == "bank" ? "Chuyển khoản" : "Nhân viên tư vấn",
            Status = "Pending"
        };
        foreach (var (productId, quantity) in requestedQuantities)
        {
            var product = products[productId];
            product.StockQuantity -= quantity;
            order.Items.Add(new MotorcycleOrderItem
            {
                MotorcycleId = product.Id,
                MotorcycleName = product.Name,
                UnitPrice = product.Price,
                Quantity = quantity
            });
            order.TotalAmount = checked(order.TotalAmount + product.Price * quantity);
        }

        if (promotion is not null)
        {
            order.DiscountAmount = Math.Min(order.TotalAmount, promotion.DiscountAmount);
            order.TotalAmount -= order.DiscountAmount;
            promotion.UsedCount++;
        }

        database.Orders.Add(order);
        await database.SaveChangesAsync();
        await transaction.CommitAsync();
        TempData["OrderPlaced"] = order.OrderNumber;
        return RedirectToAction(nameof(Checkout));
    }

    private sealed record CheckoutCartItem(string Id, int Quantity);

    private async Task SetPaymentAvailabilityAsync()
    {
        var settings = await database.StoreSettings.AsNoTracking().ToDictionaryAsync(setting => setting.Key, setting => setting.Value);
        ViewData["BankTransferEnabled"] = ReadSetting(settings, "BankTransfer", true);
        ViewData["PayOnDeliveryEnabled"] = ReadSetting(settings, "PayOnDelivery", true);
    }

    private static bool ReadSetting(IReadOnlyDictionary<string, string> settings, string key, bool defaultValue) =>
        !settings.TryGetValue(key, out var value) || !bool.TryParse(value, out var enabled) ? defaultValue : enabled;
}
