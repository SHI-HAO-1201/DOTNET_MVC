using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyMvcApp.Data;
using MyMvcApp.Models;

namespace MyMvcApp.Controllers;

public class AccountController(MyMvcApp.Services.SharedAdminStore admins, AppDbContext database) : Controller
{
    private static readonly PasswordHasher<Customer> PasswordHasher = new();
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.IsInRole("Admin")) return RedirectToAction("Dashboard", "Admin");
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string email, string password, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        var admin = await admins.AuthenticateAsync(email ?? string.Empty, password ?? string.Empty, HttpContext.RequestAborted);
        if (admin is not null)
        {
            var adminClaims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, admin.Value.Id.ToString()),
                new Claim(ClaimTypes.Name, admin.Value.Name),
                new Claim(ClaimTypes.Email, admin.Value.Email),
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim(ClaimTypes.Role, "QuanTriVien")
            };
            var adminPrincipal = new ClaimsPrincipal(new ClaimsIdentity(adminClaims, CookieAuthenticationDefaults.AuthenticationScheme));
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, adminPrincipal,
                new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8) });
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
            return RedirectToAction("Dashboard", "Admin");
        }

        var normalizedEmail = (email ?? string.Empty).Trim().ToLowerInvariant();
        var customer = await database.Customers.SingleOrDefaultAsync(item => item.Email == normalizedEmail, HttpContext.RequestAborted);
        if (customer?.PasswordHash is null || PasswordHasher.VerifyHashedPassword(customer, customer.PasswordHash, password ?? string.Empty) == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không chính xác.");
            return View();
        }

        var customerClaims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, customer.Id.ToString()),
            new Claim(ClaimTypes.Name, customer.FullName),
            new Claim(ClaimTypes.Email, customer.Email),
            new Claim(ClaimTypes.Role, "Customer")
        };
        var customerPrincipal = new ClaimsPrincipal(new ClaimsIdentity(customerClaims, CookieAuthenticationDefaults.AuthenticationScheme));
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, customerPrincipal,
            new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8) });
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
        return RedirectToAction("Index", "Home");
    }

    public IActionResult Register() => View();
    public IActionResult ForgotPassword() => View();
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var customer = await GetSignedInCustomerAsync();
        if (customer is null) return RequireCustomerLogin();
        return View(new AccountProfileViewModel
        {
            FullName = customer.FullName,
            Email = customer.Email,
            Phone = customer.Phone,
            Address = customer.Address
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(AccountProfileViewModel model)
    {
        var customer = await GetSignedInCustomerAsync();
        if (customer is null) return RequireCustomerLogin();
        model.Email = customer.Email;
        if (!ModelState.IsValid) return View(model);

        customer.FullName = model.FullName.Trim();
        customer.Phone = (model.Phone ?? string.Empty).Trim();
        customer.Address = (model.Address ?? string.Empty).Trim();
        await database.SaveChangesAsync(HttpContext.RequestAborted);
        await SignInCustomerAsync(customer);
        TempData["Notice"] = "Thông tin cá nhân đã được cập nhật.";
        return RedirectToAction(nameof(Profile));
    }

    [HttpGet]
    public async Task<IActionResult> Orders()
    {
        var customer = await GetSignedInCustomerAsync();
        if (customer is null) return RequireCustomerLogin();
        var orders = await database.Orders.AsNoTracking()
            .Where(order => order.CustomerId == customer.Id)
            .Include(order => order.Items)
            .OrderByDescending(order => order.CreatedAtUtc)
            .ToListAsync(HttpContext.RequestAborted);
        return View(orders);
    }

    public IActionResult Saved() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(string fullName, string email, string password)
    {
        fullName = (fullName ?? string.Empty).Trim();
        email = (email ?? string.Empty).Trim().ToLowerInvariant();
        password ??= string.Empty;
        ViewData["fullName"] = fullName;
        ViewData["email"] = email;

        if (fullName.Length is < 2 or > 100)
            ModelState.AddModelError(nameof(fullName), "Họ tên cần từ 2 đến 100 ký tự.");
        if (!System.Net.Mail.MailAddress.TryCreate(email, out var address) || address.Address != email)
            ModelState.AddModelError(nameof(email), "Email không hợp lệ.");
        if (password.Length is < 8 or > 256)
            ModelState.AddModelError(nameof(password), "Mật khẩu cần từ 8 đến 256 ký tự.");
        var existingCustomer = ModelState.IsValid
            ? await database.Customers.SingleOrDefaultAsync(customer => customer.Email == email, HttpContext.RequestAborted)
            : null;
        if (existingCustomer?.PasswordHash is not null)
            ModelState.AddModelError(nameof(email), "Email này đã được đăng ký.");

        if (!ModelState.IsValid) return View();

        var customer = existingCustomer ?? new Customer
        {
            FullName = fullName,
            Email = email,
            Phone = string.Empty,
            PasswordHash = string.Empty,
            CreatedAtUtc = DateTime.UtcNow
        };
        customer.FullName = fullName;
        customer.PasswordHash = PasswordHasher.HashPassword(customer, password);
        if (existingCustomer is null) database.Customers.Add(customer);
        try
        {
            await database.SaveChangesAsync(HttpContext.RequestAborted);
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(nameof(email), "Email này đã được đăng ký.");
            return View();
        }

        await SignInCustomerAsync(customer);
        TempData["Notice"] = "Đăng ký thành công. Bạn đã được đăng nhập.";
        return RedirectToAction(nameof(Profile));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    private async Task<Customer?> GetSignedInCustomerAsync()
    {
        if (!User.IsInRole("Customer")
            || !int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var customerId))
            return null;
        return await database.Customers.FirstOrDefaultAsync(customer => customer.Id == customerId, HttpContext.RequestAborted);
    }

    private IActionResult RequireCustomerLogin() => RedirectToAction(nameof(Login), new
    {
        returnUrl = Request.Path.ToString() + Request.QueryString
    });

    private async Task SignInCustomerAsync(Customer customer)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, customer.Id.ToString()),
            new Claim(ClaimTypes.Name, customer.FullName),
            new Claim(ClaimTypes.Email, customer.Email),
            new Claim(ClaimTypes.Role, "Customer")
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
            new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ForgotPassword(string email)
    {
        TempData["Notice"] = "Vui lòng liên hệ cửa hàng để khôi phục mật khẩu.";
        return View();
    }
}
