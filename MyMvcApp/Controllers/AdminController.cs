using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using MyMvcApp.Data;
using MyMvcApp.Models;

namespace MyMvcApp.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController(AppDbContext database) : Controller
{
    private static readonly string[] OrderStatuses = ["Pending", "Processing", "Shipping", "Completed", "Cancelled"];

    public IActionResult Index() => RedirectToAction(nameof(Dashboard));

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        ViewData["PendingOrders"] = await database.Orders.CountAsync(order => order.Status == "Pending");
        await next();
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null) =>
        RedirectToAction("Login", "Account", new { returnUrl });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login", "Account");
    }

    public async Task<IActionResult> Dashboard() => View(await CreateStatisticsAsync());

    public async Task<IActionResult> Products() => View(await database.Motorcycles
        .AsNoTracking()
        .OrderBy(product => product.Id)
        .ToListAsync());

    public async Task<IActionResult> ExportProducts()
    {
        var products = await database.Motorcycles.AsNoTracking().OrderBy(product => product.Name).ToListAsync();
        var csv = new System.Text.StringBuilder("Id,Code,Name,Brand,Type,Price,StockQuantity,IsActive\r\n");
        foreach (var product in products)
        {
            csv.AppendLine(string.Join(",", new object?[] { product.Id, Csv(product.Code), Csv(product.Name), Csv(product.Brand),
                Csv(product.Type), product.Price, product.StockQuantity, product.IsActive }));
        }
        return File(System.Text.Encoding.UTF8.GetPreamble().Concat(System.Text.Encoding.UTF8.GetBytes(csv.ToString())).ToArray(),
            "text/csv; charset=utf-8", "motospeed-products.csv");
    }

    [HttpGet]
    public IActionResult CreateProduct() => View("ProductForm", new AdminProductInput());

    [HttpGet]
    public async Task<IActionResult> EditProduct(int id)
    {
        var product = await database.Motorcycles.FindAsync(id);
        if (product is null) return NotFound();

        return View("ProductForm", new AdminProductInput
        {
            Id = product.Id,
            Name = product.Name,
            Brand = product.Brand,
            Type = product.Type,
            Price = product.Price,
            Engine = product.Engine,
            Image = product.Image,
            Code = product.Code,
            StockQuantity = product.StockQuantity,
            IsActive = product.IsActive,
            AvailabilityStatus = product.AvailabilityStatus
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveProduct(AdminProductInput input)
    {
        var allowedStatuses = new[]
        {
            MotorcycleAvailabilityStatus.ForSale,
            MotorcycleAvailabilityStatus.Incoming
        };
        if (!allowedStatuses.Contains(input.AvailabilityStatus, StringComparer.Ordinal))
            ModelState.AddModelError(nameof(input.AvailabilityStatus), "Trạng thái bán xe không hợp lệ.");
        if (input.AvailabilityStatus == MotorcycleAvailabilityStatus.ForSale && input.StockQuantity < 1)
            ModelState.AddModelError(nameof(input.StockQuantity), "Xe ở trạng thái còn hàng phải có ít nhất 1 xe trong kho.");
        if (!ModelState.IsValid) return View("ProductForm", input);

        input.Code = input.Code.Trim().ToUpperInvariant();

        var duplicateCode = await database.Motorcycles.AnyAsync(product =>
            product.Code == input.Code && product.Id != input.Id);
        if (duplicateCode)
        {
            ModelState.AddModelError(nameof(input.Code), "Mã sản phẩm đã được sử dụng.");
            return View("ProductForm", input);
        }

        Motorcycle product;
        if (input.Id == 0)
        {
            product = new Motorcycle(0, input.Name.Trim(), input.Brand.Trim(), input.Type.Trim(), input.Price,
                input.Engine.Trim(), string.Empty, string.Empty, input.Image.Trim(), input.Code.Trim())
            {
                StockQuantity = input.StockQuantity,
                IsActive = input.IsActive,
                AvailabilityStatus = input.AvailabilityStatus
            };
            database.Entry(product).Property<decimal>("GiaNiemYet").CurrentValue = input.Price;
            database.Motorcycles.Add(product);
        }
        else
        {
            product = await database.Motorcycles.FindAsync(input.Id) ?? throw new InvalidOperationException("Không tìm thấy sản phẩm.");
            product.Name = input.Name.Trim();
            product.Brand = input.Brand.Trim();
            product.Type = input.Type.Trim();
            product.Price = input.Price;
            product.Engine = input.Engine.Trim();
            product.Image = input.Image.Trim();
            product.Code = input.Code.Trim();
            product.StockQuantity = input.StockQuantity;
            product.IsActive = input.IsActive;
            product.AvailabilityStatus = input.AvailabilityStatus;
        }

        if (input.AvailabilityStatus != MotorcycleAvailabilityStatus.ForSale)
            product.StockQuantity = 0;

        await database.SaveChangesAsync();
        TempData["AdminSuccess"] = input.Id == 0 ? "Đã thêm sản phẩm." : "Đã cập nhật sản phẩm.";
        return RedirectToAction(nameof(Products));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await database.Motorcycles.FindAsync(id);
        if (product is null) return NotFound();

        product.IsActive = false;
        await database.SaveChangesAsync();
        TempData["AdminSuccess"] = "Đã ngừng kinh doanh sản phẩm.";
        return RedirectToAction(nameof(Products));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleProductVisibility(int id)
    {
        var product = await database.Motorcycles.FindAsync(id);
        if (product is null) return NotFound();

        product.IsActive = !product.IsActive;
        await database.SaveChangesAsync();
        TempData["AdminSuccess"] = product.IsActive
            ? $"Đã hiện {product.Name} trên cửa hàng."
            : $"Đã ẩn {product.Name} khỏi cửa hàng.";
        return RedirectToAction(nameof(Products));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStock(int id, int stockQuantity)
    {
        if (stockQuantity < 0)
        {
            TempData["AdminError"] = "Tồn kho không thể là số âm.";
            return RedirectToAction(nameof(Inventory));
        }

        var product = await database.Motorcycles.FindAsync(id);
        if (product is null) return NotFound();

        SetInventory(product, stockQuantity);
        await database.SaveChangesAsync();
        TempData["AdminSuccess"] = $"Đã cập nhật tồn kho {product.Name}: {stockQuantity} xe.";
        return RedirectToAction(nameof(Inventory));
    }

    public async Task<IActionResult> Orders()
    {
        var orders = await database.Orders.AsNoTracking()
            .Include(order => order.Customer)
            .Include(order => order.Items)
            .OrderByDescending(order => order.CreatedAtUtc)
            .ToListAsync();

        return View(new AdminOrdersViewModel
        {
            Statistics = await CreateStatisticsAsync(),
            Orders = orders
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateOrderStatus(int id, string status)
    {
        if (!OrderStatuses.Contains(status, StringComparer.Ordinal)) return BadRequest();

        var order = await database.Orders.Include(order => order.Items).FirstOrDefaultAsync(order => order.Id == id);
        if (order is null) return NotFound();

        if (status == "Cancelled" && order.Status != "Cancelled")
        {
            foreach (var item in order.Items)
            {
                var product = await database.Motorcycles.FindAsync(item.MotorcycleId);
                if (product is not null)
                {
                    product.StockQuantity += item.Quantity;
                }
            }
        }
        else if (order.Status == "Cancelled" && status != "Cancelled")
        {
            foreach (var item in order.Items)
            {
                var product = await database.Motorcycles.FindAsync(item.MotorcycleId);
                if (product is null || product.StockQuantity < item.Quantity)
                {
                    TempData["AdminError"] = "Không đủ tồn kho để mở lại đơn hàng này.";
                    return RedirectToAction(nameof(Orders));
                }
                SetInventory(product, product.StockQuantity - item.Quantity);
            }
        }

        order.Status = status;
        await database.SaveChangesAsync();
        TempData["AdminSuccess"] = "Đã cập nhật trạng thái đơn hàng.";
        return RedirectToAction(nameof(Orders));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteOrder(int id)
    {
        var order = await database.Orders.Include(order => order.Items).FirstOrDefaultAsync(order => order.Id == id);
        if (order is null) return NotFound();

        if (order.Status != "Cancelled")
        {
            foreach (var item in order.Items)
            {
                var product = await database.Motorcycles.FindAsync(item.MotorcycleId);
                if (product is not null)
                {
                    product.StockQuantity += item.Quantity;
                    if (product.AvailabilityStatus == MotorcycleAvailabilityStatus.Incoming)
                        product.AvailabilityStatus = MotorcycleAvailabilityStatus.ForSale;
                    if (product.IsHiddenByStock)
                    {
                        product.IsActive = true;
                        product.IsHiddenByStock = false;
                    }
                }
            }
        }

        database.Orders.Remove(order);
        await database.SaveChangesAsync();
        TempData["AdminSuccess"] = "Đã xóa đơn hàng và hoàn lại số lượng xe vào kho.";
        return RedirectToAction(nameof(Orders));
    }

    public async Task<IActionResult> Customers()
    {
        return View(new AdminCustomersViewModel
        {
            Statistics = await CreateStatisticsAsync(),
            Customers = await database.Customers.AsNoTracking()
                .Include(customer => customer.Orders)
                .OrderByDescending(customer => customer.CreatedAtUtc)
                .ToListAsync()
        });
    }

    [HttpGet]
    public IActionResult CreateCustomer() => View("CustomerForm", new AdminCustomerInput());

    [HttpGet]
    public async Task<IActionResult> EditCustomer(int id)
    {
        var customer = await database.Customers.FindAsync(id);
        if (customer is null) return NotFound();

        return View("CustomerForm", new AdminCustomerInput
        {
            Id = customer.Id,
            FullName = customer.FullName,
            Email = customer.Email,
            Phone = customer.Phone,
            Address = customer.Address
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveCustomer(AdminCustomerInput input)
    {
        var normalizedEmail = (input.Email ?? string.Empty).Trim().ToLowerInvariant();
        if (await database.Customers.AnyAsync(customer => customer.Email == normalizedEmail && customer.Id != input.Id))
        {
            ModelState.AddModelError(nameof(input.Email), "Email này đã được sử dụng.");
        }
        if (!ModelState.IsValid) return View("CustomerForm", input);

        if (input.Id == 0)
        {
            database.Customers.Add(new Customer
            {
                FullName = input.FullName.Trim(),
                Email = normalizedEmail,
                Phone = input.Phone.Trim(),
                Address = input.Address.Trim()
            });
        }
        else
        {
            var customer = await database.Customers.FindAsync(input.Id);
            if (customer is null) return NotFound();
            customer.FullName = input.FullName.Trim();
            customer.Email = normalizedEmail;
            customer.Phone = input.Phone.Trim();
            customer.Address = input.Address.Trim();
        }

        await database.SaveChangesAsync();
        TempData["AdminSuccess"] = input.Id == 0 ? "Đã thêm khách hàng." : "Đã cập nhật khách hàng.";
        return RedirectToAction(nameof(Customers));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCustomer(int id)
    {
        var customer = await database.Customers.FindAsync(id);
        if (customer is null) return NotFound();
        if (await database.Orders.AnyAsync(order => order.CustomerId == id))
        {
            TempData["AdminError"] = "Không thể xóa khách hàng có lịch sử đơn hàng.";
            return RedirectToAction(nameof(Customers));
        }

        database.Customers.Remove(customer);
        await database.SaveChangesAsync();
        TempData["AdminSuccess"] = "Đã xóa khách hàng.";
        return RedirectToAction(nameof(Customers));
    }

    public async Task<IActionResult> Inventory() => View(await database.Motorcycles.AsNoTracking()
        .OrderBy(product => product.Name)
        .ToListAsync());

    public async Task<IActionResult> ProductContactRequests() => View(await database.ProductContactRequests
        .AsNoTracking()
        .Include(request => request.Motorcycle)
        .OrderBy(request => request.IsContacted)
        .ThenByDescending(request => request.CreatedAtUtc)
        .ToListAsync());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkProductContactRequest(int id, bool isContacted)
    {
        var request = await database.ProductContactRequests.FindAsync(id);
        if (request is null) return NotFound();
        request.IsContacted = isContacted;
        await database.SaveChangesAsync();
        TempData["AdminSuccess"] = isContacted ? "Đã đánh dấu yêu cầu đã liên hệ." : "Đã đưa yêu cầu về trạng thái cần liên hệ.";
        return RedirectToAction(nameof(ProductContactRequests));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteProductContactRequest(int id)
    {
        var request = await database.ProductContactRequests.FindAsync(id);
        if (request is null) return NotFound();
        database.ProductContactRequests.Remove(request);
        await database.SaveChangesAsync();
        TempData["AdminSuccess"] = "Đã xóa yêu cầu liên hệ.";
        return RedirectToAction(nameof(ProductContactRequests));
    }

    public async Task<IActionResult> Promotions() => View(new AdminPromotionsViewModel
    {
        Statistics = await CreateStatisticsAsync(),
        Promotions = await database.Promotions.AsNoTracking().OrderByDescending(promotion => promotion.Id).ToListAsync()
    });

    [HttpGet]
    public IActionResult CreatePromotion() => View("PromotionForm", new AdminPromotionInput());

    [HttpGet]
    public async Task<IActionResult> EditPromotion(int id)
    {
        var promotion = await database.Promotions.FindAsync(id);
        if (promotion is null) return NotFound();
        return View("PromotionForm", new AdminPromotionInput
        {
            Id = promotion.Id,
            Name = promotion.Name,
            Code = promotion.Code,
            Description = promotion.Description,
            DiscountAmount = promotion.DiscountAmount,
            UsageLimit = promotion.UsageLimit,
            StartsAt = promotion.StartsAtUtc,
            EndsAt = promotion.EndsAtUtc,
            IsActive = promotion.IsActive
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SavePromotion(AdminPromotionInput input)
    {
        var code = (input.Code ?? string.Empty).Trim().ToUpperInvariant();
        if (input.EndsAt.Date < input.StartsAt.Date)
            ModelState.AddModelError(nameof(input.EndsAt), "Ngày kết thúc phải sau ngày bắt đầu.");
        if (await database.Promotions.AnyAsync(promotion => promotion.Code == code && promotion.Id != input.Id))
            ModelState.AddModelError(nameof(input.Code), "Mã khuyến mãi đã tồn tại.");
        if (!ModelState.IsValid) return View("PromotionForm", input);

        Promotion promotion;
        if (input.Id == 0)
        {
            promotion = new Promotion { Name = input.Name.Trim(), Code = code };
            database.Promotions.Add(promotion);
        }
        else
        {
            promotion = await database.Promotions.FindAsync(input.Id) ?? throw new InvalidOperationException("Không tìm thấy khuyến mãi.");
        }
        promotion.Name = input.Name.Trim();
        promotion.Code = code;
        promotion.Description = input.Description.Trim();
        promotion.DiscountAmount = input.DiscountAmount;
        promotion.UsageLimit = input.UsageLimit;
        promotion.StartsAtUtc = DateTime.SpecifyKind(input.StartsAt.Date, DateTimeKind.Utc);
        promotion.EndsAtUtc = DateTime.SpecifyKind(input.EndsAt.Date, DateTimeKind.Utc);
        promotion.IsActive = input.IsActive;
        await database.SaveChangesAsync();
        TempData["AdminSuccess"] = "Đã lưu chương trình khuyến mãi.";
        return RedirectToAction(nameof(Promotions));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePromotion(int id)
    {
        var promotion = await database.Promotions.FindAsync(id);
        if (promotion is null) return NotFound();
        database.Promotions.Remove(promotion);
        await database.SaveChangesAsync();
        TempData["AdminSuccess"] = "Đã xóa chương trình khuyến mãi.";
        return RedirectToAction(nameof(Promotions));
    }

    public async Task<IActionResult> Reviews()
    {
        var reviews = await database.Reviews.AsNoTracking().Include(review => review.Motorcycle)
            .OrderByDescending(review => review.CreatedAtUtc).ToListAsync();
        return View(new AdminReviewsViewModel
        {
            Statistics = await CreateStatisticsAsync(),
            Reviews = reviews,
            AverageRating = reviews.Count == 0 ? 0 : (decimal)reviews.Average(review => review.Rating),
            FiveStarCount = reviews.Count(review => review.Rating == 5)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveReviewReply(int id, string adminReply)
    {
        var review = await database.Reviews.FindAsync(id);
        if (review is null) return NotFound();
        if (string.IsNullOrWhiteSpace(adminReply) || adminReply.Length > 2000)
        {
            TempData["AdminError"] = "Phản hồi phải có nội dung và tối đa 2.000 ký tự.";
            return RedirectToAction(nameof(Reviews));
        }
        review.AdminReply = adminReply.Trim();
        await database.SaveChangesAsync();
        TempData["AdminSuccess"] = "Đã lưu phản hồi đánh giá.";
        return RedirectToAction(nameof(Reviews));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleReviewVisibility(int id)
    {
        var review = await database.Reviews.FindAsync(id);
        if (review is null) return NotFound();
        review.IsVisible = !review.IsVisible;
        await database.SaveChangesAsync();
        TempData["AdminSuccess"] = review.IsVisible ? "Đã hiển thị đánh giá." : "Đã ẩn đánh giá trên cửa hàng.";
        return RedirectToAction(nameof(Reviews));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteReview(int id)
    {
        var review = await database.Reviews.FindAsync(id);
        if (review is null) return NotFound();
        database.Reviews.Remove(review);
        await database.SaveChangesAsync();
        TempData["AdminSuccess"] = "Đã xóa đánh giá.";
        return RedirectToAction(nameof(Reviews));
    }

    public async Task<IActionResult> Reports(DateTime? start, DateTime? end) => View(await CreateStatisticsAsync(start, end));

    public async Task<IActionResult> ExportReport(DateTime? start, DateTime? end)
    {
        var ordersQuery = database.Orders.AsNoTracking().Include(order => order.Customer).AsQueryable();
        if (start.HasValue) ordersQuery = ordersQuery.Where(order => order.CreatedAtUtc >= DateTime.SpecifyKind(start.Value.Date, DateTimeKind.Utc));
        if (end.HasValue) ordersQuery = ordersQuery.Where(order => order.CreatedAtUtc < DateTime.SpecifyKind(end.Value.Date.AddDays(1), DateTimeKind.Utc));
        var orders = await ordersQuery.OrderByDescending(order => order.CreatedAtUtc).ToListAsync();
        var csv = new System.Text.StringBuilder("OrderNumber,Customer,Email,Date,Total,Status\r\n");
        foreach (var order in orders)
            csv.AppendLine(string.Join(",", new object?[] { Csv(order.OrderNumber), Csv(order.Customer.FullName), Csv(order.Customer.Email),
                order.CreatedAtUtc.ToString("O"), order.TotalAmount, Csv(order.Status) }));
        return File(System.Text.Encoding.UTF8.GetPreamble().Concat(System.Text.Encoding.UTF8.GetBytes(csv.ToString())).ToArray(),
            "text/csv; charset=utf-8", "motospeed-report.csv");
    }

    public async Task<IActionResult> Staff() => View(new AdminStaffViewModel
    {
        Statistics = await CreateStatisticsAsync(),
        Members = await database.StaffMembers.AsNoTracking().OrderByDescending(member => member.Id).ToListAsync()
    });

    [HttpGet]
    public IActionResult CreateStaff() => View("StaffForm", new AdminStaffInput());

    [HttpGet]
    public async Task<IActionResult> EditStaff(int id)
    {
        var member = await database.StaffMembers.FindAsync(id);
        if (member is null) return NotFound();
        return View("StaffForm", new AdminStaffInput
        {
            Id = member.Id,
            FullName = member.FullName,
            Email = member.Email,
            Role = member.Role,
            StoreLocation = member.StoreLocation,
            IsActive = member.IsActive
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveStaff(AdminStaffInput input)
    {
        var email = (input.Email ?? string.Empty).Trim().ToLowerInvariant();
        if (await database.StaffMembers.AnyAsync(member => member.Email == email && member.Id != input.Id))
            ModelState.AddModelError(nameof(input.Email), "Email nhân viên đã tồn tại.");
        if (!ModelState.IsValid) return View("StaffForm", input);

        StaffMember member;
        if (input.Id == 0)
        {
            member = new StaffMember { FullName = input.FullName.Trim(), Email = email, Role = input.Role.Trim() };
            database.StaffMembers.Add(member);
        }
        else
        {
            member = await database.StaffMembers.FindAsync(input.Id) ?? throw new InvalidOperationException("Không tìm thấy nhân viên.");
        }
        member.FullName = input.FullName.Trim();
        member.Email = email;
        member.Role = input.Role.Trim();
        member.StoreLocation = input.StoreLocation.Trim();
        member.IsActive = input.IsActive;
        await database.SaveChangesAsync();
        TempData["AdminSuccess"] = "Đã lưu hồ sơ nhân viên.";
        return RedirectToAction(nameof(Staff));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteStaff(int id)
    {
        var member = await database.StaffMembers.FindAsync(id);
        if (member is null) return NotFound();
        database.StaffMembers.Remove(member);
        await database.SaveChangesAsync();
        TempData["AdminSuccess"] = "Đã xóa hồ sơ nhân viên.";
        return RedirectToAction(nameof(Staff));
    }

    [HttpGet]
    public async Task<IActionResult> Settings()
    {
        var settings = await database.StoreSettings.AsNoTracking().ToDictionaryAsync(setting => setting.Key, setting => setting.Value);
        string Get(string key, string fallback = "") => settings.TryGetValue(key, out var value) ? value : fallback;
        bool GetFlag(string key, bool fallback = false) => bool.TryParse(Get(key), out var value) ? value : fallback;
        return View(new StoreSettingsInput
        {
            StoreName = Get("StoreName", "MOTOSPEED"),
            SupportEmail = Get("SupportEmail", "hello@motospeed.vn"),
            Hotline = Get("Hotline", "1900 6868"),
            TaxCode = Get("TaxCode"),
            Address = Get("Address", "123 Nguyễn Văn Linh, Quận 7, TP. Hồ Chí Minh"),
            Description = Get("Description", "Đồng hành cùng đam mê trên mọi cung đường."),
            PayOnDelivery = GetFlag("PayOnDelivery", true),
            BankTransfer = GetFlag("BankTransfer", true),
            FreeDelivery = GetFlag("FreeDelivery", true),
            NotifyNewOrder = GetFlag("NotifyNewOrder"),
            NotifyLowStock = GetFlag("NotifyLowStock")
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveSettings(StoreSettingsInput input)
    {
        if (!ModelState.IsValid) return View("Settings", input);
        var values = new Dictionary<string, string>
        {
            ["StoreName"] = input.StoreName.Trim(), ["SupportEmail"] = input.SupportEmail.Trim(),
            ["Hotline"] = input.Hotline.Trim(), ["TaxCode"] = input.TaxCode.Trim(),
            ["Address"] = input.Address.Trim(), ["Description"] = input.Description.Trim(),
            ["PayOnDelivery"] = input.PayOnDelivery.ToString(), ["BankTransfer"] = input.BankTransfer.ToString(),
            ["FreeDelivery"] = input.FreeDelivery.ToString(), ["NotifyNewOrder"] = input.NotifyNewOrder.ToString(),
            ["NotifyLowStock"] = input.NotifyLowStock.ToString()
        };
        var existing = await database.StoreSettings.ToDictionaryAsync(setting => setting.Key);
        foreach (var (key, value) in values)
        {
            if (existing.TryGetValue(key, out var setting)) setting.Value = value;
            else database.StoreSettings.Add(new StoreSetting { Key = key, Value = value });
        }
        await database.SaveChangesAsync();
        TempData["AdminSuccess"] = "Đã lưu cài đặt cửa hàng.";
        return RedirectToAction(nameof(Settings));
    }

    private async Task<AdminStatisticsViewModel> CreateStatisticsAsync(DateTime? start = null, DateTime? end = null)
    {
        var year = start?.Year ?? end?.Year ?? DateTime.UtcNow.Year;
        var ordersQuery = database.Orders.AsNoTracking().AsQueryable();
        if (start.HasValue) ordersQuery = ordersQuery.Where(order => order.CreatedAtUtc >= DateTime.SpecifyKind(start.Value.Date, DateTimeKind.Utc));
        if (end.HasValue) ordersQuery = ordersQuery.Where(order => order.CreatedAtUtc < DateTime.SpecifyKind(end.Value.Date.AddDays(1), DateTimeKind.Utc));
        var orders = await ordersQuery.ToListAsync();
        var recentOrdersQuery = database.Orders.AsNoTracking().Include(order => order.Customer).Include(order => order.Items).AsQueryable();
        if (start.HasValue) recentOrdersQuery = recentOrdersQuery.Where(order => order.CreatedAtUtc >= DateTime.SpecifyKind(start.Value.Date, DateTimeKind.Utc));
        if (end.HasValue) recentOrdersQuery = recentOrdersQuery.Where(order => order.CreatedAtUtc < DateTime.SpecifyKind(end.Value.Date.AddDays(1), DateTimeKind.Utc));
        var recentOrders = await recentOrdersQuery.OrderByDescending(order => order.CreatedAtUtc).Take(5).ToListAsync();
        var soldItemsQuery = database.OrderItems.AsNoTracking().Where(item => item.Order.Status == "Completed");
        if (start.HasValue) soldItemsQuery = soldItemsQuery.Where(item => item.Order.CreatedAtUtc >= DateTime.SpecifyKind(start.Value.Date, DateTimeKind.Utc));
        if (end.HasValue) soldItemsQuery = soldItemsQuery.Where(item => item.Order.CreatedAtUtc < DateTime.SpecifyKind(end.Value.Date.AddDays(1), DateTimeKind.Utc));
        var soldItems = await soldItemsQuery.Select(item => new { item.MotorcycleName, item.Quantity, item.UnitPrice }).ToListAsync();
        var topSellingProducts = soldItems.GroupBy(item => item.MotorcycleName)
            .Select(group => new AdminTopProductStatistics(group.Key, group.Sum(item => item.Quantity),
                group.Sum(item => (decimal)item.UnitPrice * item.Quantity)))
            .OrderByDescending(item => item.Quantity).Take(5).ToArray();
        var completedOrders = orders.Where(order => order.Status == "Completed").ToArray();
        var reviewRatings = await database.Reviews.AsNoTracking().Select(review => review.Rating).ToListAsync();
        var monthlyRevenue = Enumerable.Range(1, 12)
            .Select(month => completedOrders
                .Where(order => order.CreatedAtUtc.Year == year && order.CreatedAtUtc.Month == month)
                .Sum(order => order.TotalAmount))
            .Select(amount => (decimal)amount)
            .ToArray();

        return new AdminStatisticsViewModel
        {
            Revenue = completedOrders.Sum(order => (decimal)order.TotalAmount),
            RecentOrders = recentOrders,
            TopSellingProducts = topSellingProducts,
            TotalOrders = orders.Count,
            PendingOrders = orders.Count(order => order.Status == "Pending"),
            ShippingOrders = orders.Count(order => order.Status == "Shipping"),
            CompletedOrders = completedOrders.Length,
            CancelledOrders = orders.Count(order => order.Status == "Cancelled"),
            CustomerCount = await database.Customers.CountAsync(),
            ProductCount = await database.Motorcycles.CountAsync(product => product.IsActive),
            InventoryQuantity = await database.Motorcycles.SumAsync(product => product.StockQuantity),
            ActivePromotions = await database.Promotions.CountAsync(promotion => promotion.IsActive
                && promotion.StartsAtUtc <= DateTime.UtcNow && promotion.EndsAtUtc >= DateTime.UtcNow),
            PromotionUses = await database.Promotions.SumAsync(promotion => promotion.UsedCount),
            PromotionValue = await database.Promotions.SumAsync(promotion => promotion.DiscountAmount * promotion.UsedCount),
            ReviewCount = reviewRatings.Count,
            PendingReviews = await database.Reviews.CountAsync(review => review.AdminReply == null || review.AdminReply == string.Empty),
            FiveStarReviews = reviewRatings.Count(rating => rating == 5),
            AverageRating = reviewRatings.Count == 0 ? 0 : (decimal)reviewRatings.Average(),
            StaffCount = await database.StaffMembers.CountAsync(),
            ActiveStaffCount = await database.StaffMembers.CountAsync(member => member.IsActive),
            MonthlyRevenue = monthlyRevenue
        };
    }

    private static void SetInventory(Motorcycle product, int stockQuantity)
    {
        product.StockQuantity = stockQuantity;
    }

    private static string Csv(string value) => $"\"{value.Replace("\"", "\"\"")}\"";
}
