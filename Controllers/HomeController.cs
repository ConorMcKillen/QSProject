using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QSProject.Data.Services;
using QSProject.Models;
using QSProject.Data.Repositories;



namespace QSProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private IMedicineService _svc;

        public HomeController(ILogger<HomeController> logger, IMedicineService svc)
        {
            _logger = logger;
            _svc = svc;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            var about = new AboutViewModel
            {
                Title = "About",
                Message = "Quick Scripts is a prescription ordering service that works with your local clinic and pharmacy.",
                Formed = new DateTime(2020, 11, 12)
            };

            return View(about); 
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }



    }
}
