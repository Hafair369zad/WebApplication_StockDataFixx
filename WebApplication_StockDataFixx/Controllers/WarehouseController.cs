using Microsoft.AspNetCore.Mvc;
using System.Data;
using WebApplication_StockDataFixx.Models.Domain;
using ClosedXML.Excel;
using Newtonsoft.Json;
using WebApplication_StockDataFixx.Database;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using NPOI.SS.Formula.Functions;
using DocumentFormat.OpenXml.Vml.Spreadsheet;

namespace WebApplication_StockDataFixx.Controllers
{
    public class WarehouseController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly PMIDbContext _dbContext;

        public WarehouseController(IConfiguration configuration, IWebHostEnvironment webHostEnvironment, PMIDbContext dbContext)
        {
            _configuration = configuration;
            _webHostEnvironment = webHostEnvironment;
            _dbContext = dbContext;
        }

        // Method to display the DashboardWarehouse page
        [HttpGet]
        public ActionResult DashboardWarehouse()
        {
            return View();
        }


        // Method to display the UploadDataWarehouse page
        [HttpGet]
        public ActionResult UploadDataWarehouse()
        {
            // Tampilkan halaman upload file warehouse
            return View();
        }


        // =============================================================================================== READ DATA =============================================================================================== //

        // Method to display based on access plant, storage type dll..
        [HttpGet]
        public ActionResult ReportWarehouse(string serialNo, string selectedType, string selectedMonth)
        {
            List<WarehouseItem> data;

            string accessPlant = HttpContext.Session.GetString("AccessPlant") ?? "";

            // Fetch data based on the selectedType (VMI or NonVMI) and selectedMonth
            if (selectedType == "VMI")
            {
                data = GetDataFromDatabase(serialNo, Isvmi: true, accessPlant);
            }
            else if (selectedType == "NON_VMI")
            {
                data = GetDataFromDatabase(serialNo, Isvmi: false, accessPlant);
            }
            else
            {
                data = GetDataFromDatabase(serialNo, Isvmi: null, accessPlant); // Fetch all data
            }

            if (!string.IsNullOrEmpty(accessPlant))
            {
                // Filter data based on the AccessPlant value
                data = data.Where(w => w.AccessPlant == accessPlant).ToList();
            }

            // Check if there are any elements in the filtered data
            if (data.Any())
            {
                // Jika selectedMonth tidak ada atau kosong, ambil data terbaru seperti di ReportProduction
                if (string.IsNullOrEmpty(selectedMonth) || selectedMonth == "Latest Update")
                {
                    // Filter data based on the latest upload month and year
                    var latestUploadDate = data.Max(item => item.LastUpload.Date);
                    data = data.Where(wi => wi.LastUpload.Year == latestUploadDate.Year && wi.LastUpload.Month == latestUploadDate.Month && wi.AccessPlant == accessPlant)
                               .ToList();
                }
                else
                {
                    // Ambil data berdasarkan bulan dan tahun yang dipilih pada selectedMonth
                    if (DateTime.TryParseExact(selectedMonth, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedMonth))
                    {
                        data = data.Where(w => w.LastUpload.Year == parsedMonth.Year && w.LastUpload.Month == parsedMonth.Month && w.AccessPlant == accessPlant)
                                   .ToList();
                    }
                    else
                    {
                        // Handle kesalahan jika selectedMonth tidak valid
                        // Misalnya, lempar pengecualian atau kembalikan data kosong, sesuai dengan kebutuhan Anda.
                        throw new ArgumentException("selectedMonth is not in the correct format.");
                    }
                }
            }
            else
            {
                // Handle the case where there are no elements in the filtered data
            }

            ViewBag.SerialNo = serialNo;
            ViewBag.LastUpload = GetUniqueMonths(accessPlant, selectedType);

            return View(data);
        }


        // Jika Menggunakan Kolom Last Upload 
        private IEnumerable<DateTime> GetUniqueMonths(string accessPlant, string selectedType)
        {
            var uniqueMonths = _dbContext.WarehouseItems
                .Where(w => w.AccessPlant == accessPlant && (selectedType == "VMI" ? w.Isvmi == "VMI" : w.Isvmi == "NonVMI"))
                .Select(w => w.LastUpload.Date)
                .Distinct()
                .ToList();

            return uniqueMonths;
        }


        // Method to Get Data from Database
        private List<WarehouseItem> GetDataFromDatabase(string serialNo, bool? Isvmi, string accessPlant)
        {
            // Fetch data from the warehouse database (adjust this based on your actual database context)
            var data = _dbContext.WarehouseItems.AsQueryable();

            if (!string.IsNullOrEmpty(serialNo))
            {
                data = data.Where(w => w.SerialNo == serialNo);
            }

            if (Isvmi.HasValue)
            {
                data = data.Where(w => w.Isvmi == (Isvmi.Value ? "VMI" : "NonVMI"));
            }

            if (!string.IsNullOrEmpty(accessPlant))
            {
                data = data.Where(w => w.AccessPlant == accessPlant);
            }

            return data.OrderBy(w => w.SerialNo).ToList();
        }

        // ============================================================================================================================================================================================================ //


        // ================================================================================================ UPLOAD DATA =============================================================================================== //

        // Method to Process Upload File  
        [HttpPost]
        public IActionResult UploadFile(IFormFile file)
        {
            if (file == null || file.Length <= 0)
            {
                TempData["Message"] = "Error: File tidak ada atau kosong.";
                return RedirectToAction("UploadDataWarehouse");
            }

            bool Isvmi = Request.Form["Storage_Type"] == "VMI";

            List<TempWarehouseItem> uploadedData = ProcessExcelFile(file, Isvmi);

            // Group uploaded data by Plant, Month, Sloc, and StockType
            var groupedData = uploadedData.GroupBy(item => new { item.Plant, item.Month, item.Sloc, item.StockType });

            foreach (var group in groupedData)
            {
                // Find and remove all existing data with the same Plant, Month, Sloc, and StockType
                var existingData = _dbContext.TempWarehouseItems
                    .Where(w => w.Plant == group.Key.Plant &&
                                w.Month == group.Key.Month &&
                                w.Sloc == group.Key.Sloc &&
                                w.StockType == group.Key.StockType)
                    .ToList();

                _dbContext.TempWarehouseItems.RemoveRange(existingData); // Remove all existing data

                // Add the new data to the database
                _dbContext.TempWarehouseItems.AddRange(group);
            }

            _dbContext.SaveChanges();

            // Redirect to the ReportWarehouse action
            return RedirectToAction("ReportWarehouse");
        }

        // Main method for processing the uploaded Excel file based on the selected storage type
        private List<TempWarehouseItem> ProcessExcelFile(IFormFile file, bool Isvmi)
        {
            List<TempWarehouseItem> uploadedData = new List<TempWarehouseItem>();

            // Read data from the Excel file using UTF-8 encoding
            using (var stream = file.OpenReadStream())
            {
                using (var workbook = new XLWorkbook(stream))
                {
                    var worksheet = workbook.Worksheet(1); // Assuming data is in the first worksheet
                    var rows = worksheet.RowsUsed();

                    if (Isvmi)
                    {
                        uploadedData = ProcessExcelFileVMI(rows);
                    }
                    else
                    {
                        uploadedData = ProcessExcelFileNonVMI(rows);
                    }
                }
            }
            return uploadedData;
        }


        // Method for processing the uploaded Excel file StorageType VMI 
        private List<TempWarehouseItem> ProcessExcelFileVMI(IEnumerable<IXLRow> rows)
        {
            List<TempWarehouseItem> uploadedData = new List<TempWarehouseItem>();

            foreach (var row in rows.Skip(1)) // Skip header row
            {
                int actualQty;
                var actualQtyCell = row.Cell(11); // Corrected index for the Unrestr field
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

                TempWarehouseItem item = new TempWarehouseItem
                {
                    WarehouseId = $"{row.Cell(1).Value.ToString()}{row.Cell(2).Value.ToString()}{row.Cell(3).Value.ToString()}{row.Cell(7).Value.ToString()}{row.Cell(9).Value.ToString()}",
                    Plant = row.Cell(1).Value.ToString(),
                    Sloc = row.Cell(2).Value.ToString(),
                    Month = row.Cell(3).Value.ToString(),
                    SerialNo = row.Cell(4).Value.ToString(),
                    TagNo = row.Cell(5).Value.ToString(),
                    StockType = row.Cell(6).Value.ToString(),
                    VendorCode = row.Cell(7).Value.ToString(),
                    VendorName = row.Cell(8).Value.ToString(),
                    Material = row.Cell(9).Value.ToString(),
                    MaterialDesc = row.Cell(10).Value.ToString(),
                    ActualQty = actualQty,
                    QualInsp = row.Cell(12).Value.ToString(),
                    Blocked = row.Cell(13).Value.ToString(),
                    Unit = row.Cell(14).Value.ToString(),
                    IssuePlanner = row.Cell(15).Value.ToString(),
                    Isvmi = "VMI",
                    Description = Description,
                    WrhId = $"WRH-{row.Cell(1).Value.ToString()}-{row.Cell(15).Value.ToString()}",
                    AccessPlant = $"WRH-{row.Cell(1).Value.ToString()}"
                };

                uploadedData.Add(item);
            }

            return uploadedData;
        }


        // Method for processing the uploaded Excel file StorageType Non VMI 
        private List<TempWarehouseItem> ProcessExcelFileNonVMI(IEnumerable<IXLRow> rows)
        {
            List<TempWarehouseItem> uploadedData = new List<TempWarehouseItem>();

            foreach (var row in rows.Skip(1)) // Skip header row
            {
                int actualQty;
                var actualQtyCell = row.Cell(8); // Corrected index for the Unrestr field
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

                TempWarehouseItem item = new TempWarehouseItem
                {
                    WarehouseId = $"{row.Cell(1).Value.ToString()}{row.Cell(2).Value.ToString()}{row.Cell(3).Value.ToString()}{row.Cell(6).Value.ToString()}",
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
                    StockType = "-", // Set to "-" as StockType, VendorCode, and VendorName are not available for NonVMI
                    VendorCode = "-",
                    VendorName = "-",
                    Isvmi = "NonVMI",
                    Description = Description,
                    WrhId = $"WRH-{row.Cell(1).Value.ToString()}-{row.Cell(12).Value.ToString()}",
                    AccessPlant = $"WRH-{row.Cell(1).Value.ToString()}"
                };

                uploadedData.Add(item);
            }

            return uploadedData;
        }



        // Action method to handle the request for checking the status of saved data
        [HttpGet]
        public IActionResult CheckDataSaved()
        {
            // Example: Check if there is any WarehouseItem data in the database
            bool dataSaved = _dbContext.TempWarehouseItems.Any();

            // Return the result in JSON format
            return Json(new { saved = dataSaved });
        }

        // =========================================================================================================================================================================================================== //



        // ============================================================================================== DOWNLOAD DATA ============================================================================================== //

        // Method Download File Excel 
        [HttpGet]
        public IActionResult DownloadExcel(string serialNo)
        {
            // Fetch data from the database based on the serial number and Isvmi condition
            bool Isvmi = _dbContext.WarehouseItems.Any(item => item.SerialNo == serialNo && item.Isvmi == "VMI");

            string accessPlant = ViewBag.AccessPlant?.ToString() ?? ""; // Menggunakan nilai default ("") jika null

            List<WarehouseItem> data = GetDataFromDatabase(serialNo, Isvmi, accessPlant);

            // Create the Excel file
            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Data");

            // Add the header row
            worksheet.Cell(1, 1).Value = "Plant";
            worksheet.Cell(1, 2).Value = "Sloc";
            worksheet.Cell(1, 3).Value = "Month";
            worksheet.Cell(1, 4).Value = "Serial No";
            worksheet.Cell(1, 5).Value = "Tag No";

            int columnIndex = 6; // Start index for columns after Tag No

            // If Isvmi is true, include Vendor Code and Vendor Name in header
            if (Isvmi)
            {
                worksheet.Cell(1, columnIndex).Value = "Stock Type";
                columnIndex++;
                worksheet.Cell(1, columnIndex).Value = "Vendor Code";
                columnIndex++;
                worksheet.Cell(1, columnIndex).Value = "Vendor Name";
                columnIndex++;
            }

            worksheet.Cell(1, columnIndex).Value = "Material";
            columnIndex++;
            worksheet.Cell(1, columnIndex).Value = "Material Desc";
            columnIndex++;
            worksheet.Cell(1, columnIndex).Value = "Unrestr";
            columnIndex++;
            worksheet.Cell(1, columnIndex).Value = "QualInsp";
            columnIndex++;
            worksheet.Cell(1, columnIndex).Value = "Blocked";
            columnIndex++;
            worksheet.Cell(1, columnIndex).Value = "Unit";
            columnIndex++;
            worksheet.Cell(1, columnIndex).Value = "Issue Planner";

            // Add data rows
            for (int i = 0; i < data.Count; i++)
            {
                var item = data[i];
                worksheet.Cell(i + 2, 1).Value = item.Plant;
                worksheet.Cell(i + 2, 2).Value = item.Sloc;
                worksheet.Cell(i + 2, 3).Value = item.Month;
                worksheet.Cell(i + 2, 4).Value = item.SerialNo;
                worksheet.Cell(i + 2, 5).Value = item.TagNo;

                columnIndex = 6; // Start index for columns after Tag No

                // If Isvmi is true, include Vendor Code and Vendor Name in data rows
                if (Isvmi)
                {
                    worksheet.Cell(i + 2, columnIndex).Value = item.StockType;
                    columnIndex++;
                    worksheet.Cell(i + 2, columnIndex).Value = item.VendorCode;
                    columnIndex++;
                    worksheet.Cell(i + 2, columnIndex).Value = item.VendorName;
                    columnIndex++;
                }

                worksheet.Cell(i + 2, columnIndex).Value = item.Material;
                columnIndex++;
                worksheet.Cell(i + 2, columnIndex).Value = item.MaterialDesc;
                columnIndex++;
                worksheet.Cell(i + 2, columnIndex).Value = item.ActualQty;
                columnIndex++;
                worksheet.Cell(i + 2, columnIndex).Value = item.QualInsp;
                columnIndex++;
                worksheet.Cell(i + 2, columnIndex).Value = item.Blocked;
                columnIndex++;
                worksheet.Cell(i + 2, columnIndex).Value = item.Unit;
                columnIndex++;
                worksheet.Cell(i + 2, columnIndex).Value = item.IssuePlanner;
            }

            // Set the response headers for the download

            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;
            string excelFileName = "Warehouse_" + serialNo + ".xlsx";

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelFileName);
        }

        // ========================================================================================================================================================================================================== //


        // ================================================================================================ CHART DATA =============================================================================================== //

        // Method to display Chart for count data Vmi or Non Vmi 
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

            string accessPlant = HttpContext.Session.GetString("AccessPlant") ?? "";

            int vmiCount = _dbContext.WarehouseItems.Count(item => item.Isvmi == "VMI" && item.LastUpload.Month == selectedMonthValue && item.AccessPlant == accessPlant);
            int nonVmiCount = _dbContext.WarehouseItems.Count(item => item.Isvmi == "NonVMI" && item.LastUpload.Month == selectedMonthValue && item.AccessPlant == accessPlant);

            var chartData = new[] { vmiCount, nonVmiCount };

            return Json(chartData);
        }


        // Method to display Chart for count ActualQty Vmi or Non Vmi 
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

            string accessPlant = HttpContext.Session.GetString("AccessPlant") ?? "";

            int vmiActualQty = _dbContext.WarehouseItems
                .Where(item => item.Isvmi == "VMI" && item.LastUpload.Month == selectedMonthValue && item.AccessPlant == accessPlant)
                .Sum(item => item.ActualQty);

            int nonVmiActualQty = _dbContext.WarehouseItems
                .Where(item => item.Isvmi == "NonVMI" && item.LastUpload.Month == selectedMonthValue && item.AccessPlant == accessPlant)
                .Sum(item => item.ActualQty);

            var chartData = new[] { vmiActualQty, nonVmiActualQty };

            return Json(chartData);
        }



        // Method to display Chart for count Unit (UoM) Vmi or Non Vmi 
        [HttpGet]
        public IActionResult GetChart3Data(string selectedMonth)
        {
            return GetChartDataForUnitType(selectedMonth, "VMI");
        }

        [HttpGet]
        public IActionResult GetChart4Data(string selectedMonth)
        {
            return GetChartDataForUnitType(selectedMonth, "NonVMI");
        }

        private IActionResult GetChartDataForUnitType(string selectedMonth, string isvmiType)
        {
            if (string.IsNullOrEmpty(selectedMonth))
            {
                return BadRequest("Selected month is missing or invalid.");
            }

            if (!int.TryParse(selectedMonth, out int selectedMonthValue))
            {
                return BadRequest("Selected month is not a valid integer.");
            }

            string accessPlant = HttpContext.Session.GetString("AccessPlant") ?? "";

            try
            {
                int totalKUnits = _dbContext.WarehouseItems
                    .Where(item => item.LastUpload.Month == selectedMonthValue && item.Unit == "K" && item.Isvmi == isvmiType && item.AccessPlant == accessPlant)
                    .Count();

                int totalPcsUnits = _dbContext.WarehouseItems
                    .Where(item => item.LastUpload.Month == selectedMonthValue && item.Unit == "PC" && item.Isvmi == isvmiType && item.AccessPlant == accessPlant)
                    .Count();

                int totalSetUnits = _dbContext.WarehouseItems
                    .Where(item => item.LastUpload.Month == selectedMonthValue && item.Unit == "SET" && item.Isvmi == isvmiType && item.AccessPlant == accessPlant)
                    .Count();

                int totalGUnits = _dbContext.WarehouseItems
                    .Where(item => item.LastUpload.Month == selectedMonthValue && item.Unit == "G" && item.Isvmi == isvmiType && item.AccessPlant == accessPlant)
                    .Count();

                int totalKGUnits = _dbContext.WarehouseItems
                    .Where(item => item.LastUpload.Month == selectedMonthValue && item.Unit == "KG" && item.Isvmi == isvmiType && item.AccessPlant == accessPlant)
                    .Count();

                int totalMUnits = _dbContext.WarehouseItems
                    .Where(item => item.LastUpload.Month == selectedMonthValue && item.Unit == "M" && item.Isvmi == isvmiType && item.AccessPlant == accessPlant)
                    .Count();
                int totalMLUnits = _dbContext.WarehouseItems
                   .Where(item => item.LastUpload.Month == selectedMonthValue && item.Unit == "ML" && item.Isvmi == isvmiType && item.AccessPlant == accessPlant)
                   .Count();
                int totalROLLLUnits = _dbContext.WarehouseItems
                   .Where(item => item.LastUpload.Month == selectedMonthValue && item.Unit == "ROLL" && item.Isvmi == isvmiType && item.AccessPlant == accessPlant)
                   .Count();


                var chartData = new[] { totalKUnits, totalPcsUnits, totalSetUnits, totalGUnits, totalKGUnits, totalMUnits, totalMLUnits, totalROLLLUnits };

                return Json(chartData);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while fetching chart data.");
            }
        }

        // =============================================================================================================================================================================================== //
    }
}








//  Method jangan dihapus dulu yakk // 

//private bool IsUploadedFileNonVMI(IEnumerable<IXLRow> rows)
//{
//    // Check if the uploaded file is VMI
//    foreach (var row in rows.Skip(1)) // Skip header row
//    {
//        string stockType = row.Cell(6).Value.ToString();
//        string vendorCode = row.Cell(7).Value.ToString();
//        string vendorName = row.Cell(8).Value.ToString();

//        if (string.IsNullOrWhiteSpace(stockType) && string.IsNullOrWhiteSpace(vendorCode) && string.IsNullOrWhiteSpace(vendorName))
//        {
//            return true;  // If all three fields are not empty, the file is VMI
//        }
//    }
//    return false;
//}


//private bool IsUploadedFileVMI(IEnumerable<IXLRow> rows)
//{
//    // Check if the uploaded file is VMI
//    foreach (var row in rows.Skip(1)) // Skip header row
//    {
//        string stockType = row.Cell(6).Value.ToString();
//        string vendorCode = row.Cell(7).Value.ToString();
//        string vendorName = row.Cell(8).Value.ToString();

//        if (!string.IsNullOrWhiteSpace(stockType) && !string.IsNullOrWhiteSpace(vendorCode) && !string.IsNullOrWhiteSpace(vendorName))
//        {
//            return true;  // If all three fields are not empty, the file is VMI
//        }
//    }
//    return false;
//}

//\\\\\\\\\/\/\/////////////////\\\\\\\\///////////////////\\\\\\



// atur agar mengabaikan pilihan input pilih tipe storage pada UploadDataWarehouse.cshtml . dan untuk mendeteksi jenis tipe storage pada file excel menggunakan method IsUploadedFileVMI yang menandakan file tersebut bertipe VMI , jika tidak memenuhi method tersebut makan file tersebut bertipe NonVMI 