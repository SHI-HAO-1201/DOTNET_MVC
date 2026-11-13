using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyMvcApp.Data;
using MyMvcApp.Models;

namespace MyMvcApp.Controllers;

public class HomeController(AppDbContext database) : Controller
{
    public async Task<IActionResult> Index() => View(await database.Motorcycles.AsNoTracking()
        .Where(product => product.IsActive)
        .OrderBy(product => product.Id)
        .Take(3)
        .ToListAsync());

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult About() => View();

    public IActionResult Contact() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Contact(string name, string email, string message)
    {
        ViewData["Submitted"] = true;
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
