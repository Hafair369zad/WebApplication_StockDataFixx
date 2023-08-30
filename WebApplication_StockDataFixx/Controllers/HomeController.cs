using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebApplication_StockDataFixx.Database;
using WebApplication_StockDataFixx.Models.Domain;
using Microsoft.AspNetCore.Http;


namespace WebApplication_StockDataFixx.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly PMIDbContext _dbContext; // Add the PMIDbContext instance

        public HomeController(ILogger<HomeController> logger, PMIDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext; // Inject the PMIDbContext instance
        }

        [HttpGet]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public ActionResult Index()
        {
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";

            return View();
        }


        [HttpPost]
        public ActionResult Index(string userId, string password)
        {
            // Periksa validitas login dan jenis user
            if (IsValidLogin(userId, password, out int levelId))
            {
                // Mengambil nilai STAT dari database
                int stat = GetStatFromDatabase(userId);

                // Memeriksa nilai STAT
                if (stat == 0)
                {
                    // Menyimpan informasi user di session
                    HttpContext.Session.SetString("CurrentUser", userId);

                    // Get the user's AccessPlant value
                    string accessPlant = GetAccessPlantFromDatabase(userId);
                    ViewBag.AccessPlant = accessPlant; // Store AccessPlant in ViewBag
                    HttpContext.Session.SetString("AccessPlant", accessPlant); // Store in session


                    // Redirect ke halaman utama sesuai dengan jenis user
                    if (levelId == 0)
                    {
                        return RedirectToAction("DashboardAdmin", "Admin");
                    }
                    else if (levelId == 3)
                    {
                        return RedirectToAction("DashboardWarehouse", "Warehouse");
                    }
                    else if (levelId == 4)
                    {
                        return RedirectToAction("DashboardProduction", "Production");
                    }
                    else if (levelId == 5)
                    {
                        return RedirectToAction("DashboardManagement", "Management");
                    }
                }
                else if (stat == 1)
                {
                    // Jika STAT bernilai 1, kembalikan pesan "Invalid"
                    ViewBag.ErrorMessage = "Invalid.";
                    return View();
                }
            }

            // Jika login tidak valid, kembalikan ke halaman login dengan pesan error
            ViewBag.ErrorMessage = "Invalid username or password.";
            return View();
        }


        private int GetStatFromDatabase(string userId)
        {
            var user = _dbContext.UserTbs.FirstOrDefault(u => u.UserId == userId);
            if (user != null)
            {
                return user.Stat;
            }
            return -1; // Jika user tidak ditemukan, berikan nilai default (-1)
        }

        private string GetAccessPlantFromDatabase(string userId)
        {
            // Retrieve the AccessPlant value from the database based on the userId
            var user = _dbContext.UserTbs.FirstOrDefault(u => u.UserId == userId);
            if (user != null && !string.IsNullOrEmpty(user.AccessPlant))
            {
                return user.AccessPlant;
            }
            return ""; // Return null if the AccessPlant is not found
        }


        [HttpGet]
        public ActionResult Logout()
        {
            // Menghapus informasi user dari session
            HttpContext.Session.Clear();

            // Mengarahkan pengguna ke halaman login setelah logout
            return RedirectToAction("Index", "Home");
        }


        private bool IsValidLogin(string userId, string password, out int levelId)
        {
            // Use the _dbContext to access the database and perform the login validation
            var dbUser = _dbContext.UserTbs.FirstOrDefault(u => u.UserId == userId && u.Password == password);

            if (dbUser != null)
            {
                // User found, get the level_id
                levelId = dbUser.LevelId;
                return true; // Valid login
            }

            levelId = 0; // Default value if login is invalid
            return false; // Invalid login
        }
    }
}




