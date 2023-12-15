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

                    // Get and set AccessPlant and Username in ViewBag and session
                    string accessPlant = GetAccessPlantFromDatabase(userId);
                    ViewBag.AccessPlant = accessPlant; // Store AccessPlant in ViewBag
                    HttpContext.Session.SetString("AccessPlant", accessPlant); // Store in session

                    string username = GetUsernameFromDatabase(userId);
                    ViewBag.CurrentUsername = username; // Store Username in ViewBag
                    HttpContext.Session.SetString("CurrentUsername", username); // Store in session

                    // Get and set LevelId in ViewBag and session
                    ViewBag.LevelId = GetLevelIdAndSetInSession(userId);
                    HttpContext.Session.SetInt32("LevelId", levelId);

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
            ViewBag.ErrorMessage = "*Invalid username or password.";
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

        private string GetUsernameFromDatabase(string userId)
        {
            var user = _dbContext.UserTbs.FirstOrDefault(u => u.UserId == userId);
            if (user != null)
            {
                return user.Username;
            }
            return ""; // Return an empty string if the username is not found
        }

        private int GetLevelIdFromDatabase(string userId)
        {
            // Retrieve the LevelId value from the database based on the userId
            var user = _dbContext.UserTbs.FirstOrDefault(u => u.UserId == userId);

            if (user != null)
            {
                return user.LevelId;
            }

            return 0; // Return a default value if the LevelId is not found
        }


        private int GetLevelIdAndSetInSession(string userId)
        {
            // Retrieve the LevelId value from the database based on the userId
            var levelId = GetLevelIdFromDatabase(userId);

            // Store LevelId in ViewBag
            ViewBag.LevelId = levelId;

            // Store LevelId in session
            HttpContext.Session.SetInt32("LevelId", levelId);

            return levelId;
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
