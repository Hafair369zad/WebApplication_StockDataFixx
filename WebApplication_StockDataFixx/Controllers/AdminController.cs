using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using WebApplication_StockDataFixx.Database;
using WebApplication_StockDataFixx.Models.Domain;
using System.Linq;
using WebApplication_StockDataFixx.Models;

namespace WebApplication_StockDataFixx.Controllers
{
    public class AdminController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly PMIDbContext _dbContext;

        public AdminController(IConfiguration configuration, IWebHostEnvironment webHostEnvironment, PMIDbContext dbContext)
        {
            _configuration = configuration;
            _webHostEnvironment = webHostEnvironment;
            _dbContext = dbContext;
        }

        public IActionResult DashboardAdmin()
        {
            var users = _dbContext.UserTbs.ToList();
            return View(users);
        }

        [HttpGet]
        public IActionResult AddUser()
        {
            var levels = _dbContext.SecurityTbs.ToList();
            ViewBag.Levels = levels;

            return View();
        }


        public IActionResult GenerateToken()
        {
            return View();
        }


        [HttpPost]
        public IActionResult AddUser(AddUserRequest addUserRequest)
        {
            if (ModelState.IsValid)
            {
                // Konversi AddUserRequest ke UserTb
                var newUser = new UserTb
                {
                    LevelId = addUserRequest.LevelId,
                    UserId = addUserRequest.UserId,
                    Username = addUserRequest.Username,
                    Password = addUserRequest.Password,
                    JobId = addUserRequest.JobId,
                    PlantId = addUserRequest.PlantId
                };

                _dbContext.UserTbs.Add(newUser);
                _dbContext.SaveChanges();
                return RedirectToAction("DashboardAdmin");
            }

            var levels = _dbContext.SecurityTbs.ToList();
            ViewBag.Levels = levels;

            return View(addUserRequest);
        }


        [HttpPost]
        public IActionResult DeleteUser(string userId)
        {
            var user = _dbContext.UserTbs.FirstOrDefault(u => u.UserId == userId);
            if (user != null)
            {
                _dbContext.UserTbs.Remove(user);
                _dbContext.SaveChanges();
            }
            return RedirectToAction("DashboardAdmin");
        }
    }
}
