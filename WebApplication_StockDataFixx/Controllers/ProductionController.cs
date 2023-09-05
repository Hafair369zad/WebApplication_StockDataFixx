using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Text;
using ExcelDataReader;
using WebApplication_StockDataFixx.Models.Domain;
using NPOI.SS.UserModel;
using ClosedXML.Excel;
using WebApplication_StockDataFixx.Database;
using System.Globalization;

namespace WebApplication_StockDataFixx.Controllers
{
    public class ProductionController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly PMIDbContext _dbContext;

        public ProductionController(IConfiguration configuration, IWebHostEnvironment webHostEnvironment, PMIDbContext dbContext)
        {
            _configuration = configuration;
            _webHostEnvironment = webHostEnvironment;
            _dbContext = dbContext;
        }

        [HttpGet]
        public ActionResult DashboardProduction()
        {
            // Tampilkan halaman dashboard Production
            return View();
        }

        [HttpGet]
        public ActionResult UploadDataProduction()
        {
            // Tampilkan halaman upload file Production
            return View();
        }


        // Awal Method UploadFile //

        [HttpPost]
        public IActionResult UploadFile(IFormFile file)
        {
            if (file == null || file.Length <= 0)
            {
                TempData["Message"] = "Error: File tidak ada atau kosong.";
                return RedirectToAction("UploadDataProduction");
            }

            List<ProductionItem> uploadedData = ProcessExcelFile(file);

            // Save the data to the database
            foreach (var item in uploadedData)
            {
                // Cek apakah data dengan bulan dan plant yang sama sudah ada di database
                var existingData = _dbContext.ProductionItems
                    .Where(p => p.Month == item.Month &&
                           p.Plant == item.Plant)
                    .ToList();

                if (existingData.Count > 0)
                {
                    // Jika data dengan bulan yang sama sudah ada, ganti data lama dengan data baru
                    foreach (var existingItem in existingData)
                    {
                        _dbContext.ProductionItems.Remove(existingItem); // Hapus data lama
                    }

                    // Simpan perubahan penghapusan
                    _dbContext.SaveChanges();
                }

                // Tambahkan data baru ke dalam database
                _dbContext.ProductionItems.Add(item);

                // Simpan perubahan penambahan
                _dbContext.SaveChanges();
            }

            // Serialize the uploadedData list to JSON before storing it in TempData
            string jsonUploadedData = JsonConvert.SerializeObject(uploadedData);

            // Store the JSON data in TempData
            TempData["UploadedData"] = jsonUploadedData;

            // Redirect to the ReportProduction action
            return RedirectToAction("ReportProduction");
        }


        // Akhir Method UploadFile //


        //[HttpGet]
        //public ActionResult ReportProduction(string serialNo, string selectedMonth)
        //{
        //    List<ProductionItem> uploadedDataList = null!;

        //    // Retrieve the AccessPlant value from the session
        //    string accessPlant = HttpContext.Session.GetString("AccessPlant") ?? "";

        //    // Fetch all data from the database
        //    uploadedDataList = GetDataFromDatabase(serialNo, accessPlant);

        //    // Get unique months from the uploaded data
        //    IEnumerable<string> uniqueMonths = GetUniqueMonths(uploadedDataList);


        //    // Pass the unique months and selected month to the view
        //    ViewBag.UniqueMonths = uniqueMonths;
        //    ViewBag.SelectedMonth = selectedMonth;

        //    if (string.IsNullOrEmpty(selectedMonth) || selectedMonth == "Latest Update")
        //    {
        //        // Determine the latest month based on the "Month" column
        //        var latestMonth = uploadedDataList.Max(d => d.Month);
        //        uploadedDataList = uploadedDataList.Where(d => d.Month == latestMonth).ToList();
        //    }
        //    else
        //    {
        //        // Fetch data from the database based on the selected month
        //        uploadedDataList = uploadedDataList.Where(w => w.Month == selectedMonth).ToList();
        //    }

        //    return View(uploadedDataList);
        //}

        [HttpGet]
        public ActionResult ReportProduction(string serialNo, string selectedMonth)
        {
            List<ProductionItem> data;

            string accessPlant = HttpContext.Session.GetString("AccessPlant") ?? "";

            data = GetDataFromDatabase(serialNo, accessPlant); // Tidak ada Isvmi

            if (!string.IsNullOrEmpty(accessPlant))
            {
                // Filter data based on the AccessPlant value
                data = data.Where(w => w.AccessPlant == accessPlant).ToList();
            }

            // Jika menggunakan kolom Month
            if (selectedMonth == "Latest Update")
            {
                // Filter data based on the highest month value
                var highestMonth = data.Max(d => d.Month);
                data = data.Where(d => d.Month == highestMonth).ToList();
            }
            else if (!string.IsNullOrEmpty(selectedMonth))
            {
                // Filter data based on the selected month
                data = data.Where(w => w.Month == selectedMonth).ToList();
            }

            ViewBag.SerialNo = serialNo;
            ViewBag.LastUpload = GetUniqueMonths();

            return View(data);
        }

       

        private IEnumerable<string> GetUniqueMonths()
        {
            var uniqueMonths = _dbContext.ProductionItems
                .Select(w => w.Month)
                .Distinct()
                .OrderByDescending(m => m)
                .ToList();

            return uniqueMonths;
        }

        private List<ProductionItem> GetDataFromDatabase(string serialNo, string accessPlant)
        {
            var data = _dbContext.ProductionItems.AsQueryable();

            if (!string.IsNullOrEmpty(serialNo))
            {
                data = data.Where(w => w.SerialNo == serialNo);
            }

            if (!string.IsNullOrEmpty(accessPlant))
            {
                data = data.Where(w => w.AccessPlant == accessPlant);
            }

            return data.OrderBy(w => w.SerialNo).ToList();
        }


        // Action method to handle the request for checking the status of saved data
        [HttpGet]
        public IActionResult CheckDataSaved()
        {
            // Example: Check if there is any WarehouseItem data in the database
            bool dataSaved = _dbContext.ProductionItems.Any();

            // Return the result in JSON format
            return Json(new { saved = dataSaved });
        }




        private List<ProductionItem> ProcessExcelFile(IFormFile file)
        {
            List<ProductionItem> uploadedData = new List<ProductionItem>();

            // Mendapatkan path file temporary untuk menyimpan file Excel yang diupload
            string tempPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
            if (!Directory.Exists(tempPath))
            {
                Directory.CreateDirectory(tempPath);
            }

            string filePath = Path.Combine(tempPath, Guid.NewGuid().ToString() + Path.GetExtension(file.FileName));

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            // Baca data dari file Excel dengan menggunakan encoding UTF-8
            using (var workbook = new XLWorkbook(filePath))
            {
                var worksheet = workbook.Worksheet(1); // Assuming data is in the first worksheet
                var rows = worksheet.RowsUsed();

                foreach (var row in rows.Skip(1)) // Skip header row
                {

                    int actualQty;
                    var actualQtyCell = row.Cell(8);
                    if (actualQtyCell.TryGetValue(out actualQty))
                    {
                        // If Unrestr field contains a numeric value, set ActualQty to 0
                        actualQty = 0;
                    }

                    string plant = row.Cell(1).Value.ToString();
                    string Description = "";

                    // Tambahkan logika kondisional di sini
                    switch (plant)
                    {
                        case "RIFA":
                            Description = "PMI-AUDIO";
                            break;
                        case "RIFC":
                            Description = "PMI-AIR CONDITIONER (AC)";
                            break;
                        case "RIFR":
                            Description = "PMI-REFRIGRATOR";
                            break;
                        case "RIFW":
                            Description = "PMI-IAQ";
                            break;
                        default:
                            Description = "Default Description"; // Deskripsi default jika tidak ada kondisi yang cocok
                            break;
                    }

                    ProductionItem item = new ProductionItem
                    {
                        ProductionId = $"{row.Cell(1).Value.ToString()}{row.Cell(2).Value.ToString()}{row.Cell(3).Value.ToString()}{row.Cell(6).Value.ToString()}",
                        Plant = row.Cell(1).Value.ToString(),
                        Sloc = row.Cell(2).Value.ToString(),
                        Month = row.Cell(3).Value.ToString(),
                        SerialNo = row.Cell(4).Value.ToString(),
                        TagNo = row.Cell(5).Value.ToString(),
                        Material = row.Cell(6).Value.ToString(),
                        MaterialDesc = row.Cell(7).Value.ToString(),
                        ActualQty = actualQty,
                        QualInsp = row.Cell(9).Value.ToString(),
                        Blocked = row.Cell(10).Value.ToString(),
                        Unit = row.Cell(11).Value.ToString(),
                        IssuePlanner = row.Cell(12).Value.ToString(),
                        Description = Description,
                        ProdId = $"PROD-{row.Cell(1).Value.ToString()}-{row.Cell(2).Value.ToString()}",
                        AccessPlant = $"PROD-{row.Cell(1).Value.ToString()}"
                    };

                    uploadedData.Add(item);
                }
            }

            // Hapus file temporary setelah selesai membaca data
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            return uploadedData;
        }


        [HttpGet]
        public IActionResult DownloadExcel(string serialNo)
        {

            string accessPlant = ViewBag.AccessPlant?.ToString() ?? "";
            // Fetch data from the database based on the serial number
            List<ProductionItem> data = GetDataFromDatabase(serialNo, accessPlant);

            // Create the Excel file
            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Data");

            // Add the header row
            worksheet.Cell(1, 1).Value = "Plant";
            worksheet.Cell(1, 2).Value = "Sloc";
            worksheet.Cell(1, 3).Value = "Month";
            worksheet.Cell(1, 4).Value = "Serial No";
            worksheet.Cell(1, 5).Value = "Tag No";
            worksheet.Cell(1, 6).Value = "Material";
            worksheet.Cell(1, 7).Value = "Material Desc";
            worksheet.Cell(1, 8).Value = "Actual Qty";
            worksheet.Cell(1, 9).Value = "QualInsp";
            worksheet.Cell(1, 10).Value = "Blocked";
            worksheet.Cell(1, 11).Value = "Unit";
            worksheet.Cell(1, 12).Value = "Issue Planner";

            // Add data rows
            for (int i = 0; i < data.Count; i++)
            {
                var item = data[i];
                worksheet.Cell(i + 2, 1).Value = item.Plant;
                worksheet.Cell(i + 2, 2).Value = item.Sloc;
                worksheet.Cell(i + 2, 3).Value = item.Month;
                worksheet.Cell(i + 2, 4).Value = item.SerialNo;
                worksheet.Cell(i + 2, 5).Value = item.TagNo;
                worksheet.Cell(i + 2, 6).Value = item.Material;
                worksheet.Cell(i + 2, 7).Value = item.MaterialDesc;
                worksheet.Cell(i + 2, 8).Value = item.ActualQty;
                worksheet.Cell(i + 2, 9).Value = item.QualInsp;
                worksheet.Cell(i + 2, 10).Value = item.Blocked;
                worksheet.Cell(i + 2, 11).Value = item.Unit;
                worksheet.Cell(i + 2, 12).Value = item.IssuePlanner;
            }

            // Set the response headers for the download

            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;
            string excelFileName = "Production" + serialNo + ".xlsx";

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelFileName);
        }


        //private List<ProductionItem> GetDataFromDatabaseForDownload(string serialNo)
        //{
        //    // Fetch data from the database
        //    var data = _dbContext.ProductionItems.AsQueryable();

        //    if (!string.IsNullOrEmpty(serialNo))
        //    {
        //        data = data.Where(w => w.SerialNo == serialNo);
        //    }

        //    return data.OrderBy(w => w.SerialNo).ToList();
        //}


        //  ///////////// Chart Data //////////////





        [HttpGet]
        public IActionResult GetChartData(string selectedMonth)
        {
            if (string.IsNullOrEmpty(selectedMonth))
            {
                return BadRequest("Selected month is missing or invalid.");
            }

            if (!int.TryParse(selectedMonth, out int selectedMonthValue))
            {
                return BadRequest("Selected month is not a valid integer.");
            }

            int totalProductionItems = _dbContext.ProductionItems.Count(item => item.LastUpload.Month == selectedMonthValue);

            var chartData = new { TotalItems = totalProductionItems };

            return Json(chartData);
        }


        [HttpGet]
        public IActionResult GetChart2Data(string selectedMonth)
        {
            if (string.IsNullOrEmpty(selectedMonth))
            {
                return BadRequest("Selected month is missing or invalid.");
            }

            if (!int.TryParse(selectedMonth, out int selectedMonthValue))
            {
                return BadRequest("Selected month is not a valid integer.");
            }

            int monthlyActualQty = _dbContext.ProductionItems
               .Where(item =>item.LastUpload.Month == selectedMonthValue)
               .Sum(item => item.ActualQty);

            var chartData = new { TotalActualQty = monthlyActualQty};

            return Json(chartData);
        }




        [HttpGet]
        public IActionResult GetChart3Data(string selectedMonth)
        {
            if (string.IsNullOrEmpty(selectedMonth))
            {
                return BadRequest("Selected month is missing or invalid.");
            }

            if (!int.TryParse(selectedMonth, out int selectedMonthValue))
            {
                return BadRequest("Selected month is not a valid integer.");
            }

            try
            {
                int totalKUnits = _dbContext.ProductionItems
                    .Where(item => item.LastUpload.Month == selectedMonthValue && item.Unit == "K")
                    .Count();

                int totalPcsUnits = _dbContext.ProductionItems
                    .Where(item => item.LastUpload.Month == selectedMonthValue && item.Unit == "PC")
                    .Count();

                int totalSetUnits = _dbContext.ProductionItems
                    .Where(item => item.LastUpload.Month == selectedMonthValue && item.Unit == "SET")
                    .Count();

                int totalGUnits = _dbContext.ProductionItems
                    .Where(item => item.LastUpload.Month == selectedMonthValue && item.Unit == "G")
                    .Count();

                int totalKGUnits = _dbContext.ProductionItems
                    .Where(item => item.LastUpload.Month == selectedMonthValue && item.Unit == "KG")
                    .Count();

                int totalMUnits = _dbContext.ProductionItems
                    .Where(item => item.LastUpload.Month == selectedMonthValue && item.Unit == "M")
                    .Count();

                var chartData = new[] { totalKUnits, totalPcsUnits, totalSetUnits, totalGUnits, totalKGUnits, totalMUnits };

                return Json(chartData);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while fetching chart data.");
            }
        }

        //  ///////////// Close Chart Data //////////////
    }
}