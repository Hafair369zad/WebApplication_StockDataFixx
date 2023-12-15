using Microsoft.AspNetCore.Mvc;
using System.Data;
using WebApplication_StockDataFixx.Models.Domain;
using Newtonsoft.Json;
using WebApplication_StockDataFixx.Database;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using NPOI.SS.Formula.Functions;
using DocumentFormat.OpenXml.Vml.Spreadsheet;
using DocumentFormat.OpenXml.Bibliography;
using System.Collections;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Reflection.Metadata;
using ClosedXML.Excel;
using ExcelDataReader;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

        public ActionResult DownloadPageWrh()
        {
            
            return View();
        }


// =============================================================================================== READ DATA =============================================================================================== //



        // Main Method Report 
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
            else if (selectedType == "NON VMI")
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
                if (string.IsNullOrEmpty(selectedMonth) || selectedMonth == "Latest Uploaded Month")
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

        // Temp Method Report 
        [HttpGet]
        public ActionResult ReportTempWarehouse(string serialNo, string selectedType, string selectedMonth)
        {
            List<TempWarehouseItem> data;

            string accessPlant = HttpContext.Session.GetString("AccessPlant") ?? "";

            // Fetch data based on the selectedType (VMI or NonVMI) and selectedMonth
            if (selectedType == "VMI")
            {
                data = GetDataFromDatabaseTemp(serialNo, Isvmi: true, accessPlant);
            }
            else if (selectedType == "NON VMI")
            {
                data = GetDataFromDatabaseTemp(serialNo, Isvmi: false, accessPlant);
            }
            else
            {
                data = GetDataFromDatabaseTemp(serialNo, Isvmi: null, accessPlant); // Fetch all data
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
                if (string.IsNullOrEmpty(selectedMonth) || selectedMonth == "Latest Uploaded Month")
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
            ViewBag.LastUpload = GetUniqueMonthsTemp(accessPlant, selectedType);

            return View(data);
        }

        // Jika Menggunakan Kolom Last Upload Main
        private IEnumerable<DateTime> GetUniqueMonths(string accessPlant, string selectedType)
        {
            var uniqueMonths = _dbContext.WarehouseItems
                .Where(w => w.AccessPlant == accessPlant && (selectedType == "VMI" ? w.Isvmi == "VMI" : w.Isvmi == "NonVMI"))
                .Select(w => w.LastUpload.Date)
                .Distinct()
                .ToList();

            return uniqueMonths;
        }

        // Jika Menggunakan Kolom Last Upload Temp
        private IEnumerable<DateTime> GetUniqueMonthsTemp(string accessPlant, string selectedType)
        {
            var uniqueMonths = _dbContext.TempWarehouseItems
                .Where(w => w.AccessPlant == accessPlant && (selectedType == "VMI" ? w.Isvmi == "VMI" : w.Isvmi == "NonVMI"))
                .Select(w => w.LastUpload.Date)
                .Distinct()
                .ToList();

            return uniqueMonths;
        }

        // Method to Get Data from Database Main
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


        // Method to Get Data from Database Temp
        private List<TempWarehouseItem> GetDataFromDatabaseTemp(string serialNo, bool? Isvmi, string accessPlant)
        {
            // Fetch data from the warehouse database (adjust this based on your actual database context)
            var data = _dbContext.TempWarehouseItems.AsQueryable();

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
                TempData["UnsupportedFileFormat"] = true;
                return RedirectToAction("UploadDataWarehouse");
            }

            _dbContext.DeleteFromLog();
            List<TempWarehouseItem> uploadedData = ProcessExcelFile(file);

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
            return RedirectToAction("ReportTempWarehouse");
        }


        // main method for processing the uploaded Excel file extensions .XLS //
        ///////////////////////////////////////////////////////////////////////

        //private List<TempWarehouseItem> ProcessExcelFile(IFormFile file)
        //{
        //    List<TempWarehouseItem> uploadedData = new List<TempWarehouseItem>();

        //    // Read data from the Excel file using UTF-8 encoding
        //    using (var stream = file.OpenReadStream())
        //    {
        //        using (var reader = ExcelReaderFactory.CreateReader(stream))
        //        {
        //            // Dataset untuk menyimpan data dari Excel
        //            var dataSet = reader.AsDataSet();

        //            // Ambil tabel pertama (asumsi data berada di tabel pertama)
        //            var dataTable = dataSet.Tables[0];

        //            // Check if the Excel file is VMI or Non-VMI based on column names
        //            bool isVMI = IsVMIExcelFile(dataTable);

        //            if (isVMI)
        //            {
        //                uploadedData = ProcessExcelFileVMI(dataTable.Rows);
        //            }
        //            else
        //            {
        //                uploadedData = ProcessExcelFileNonVMI(dataTable.Rows);
        //            }
        //        }
        //    }
        //    return uploadedData;
        //}

        //// Check if the Excel file is VMI based on column names
        //private bool IsVMIExcelFile(DataTable dataTable)
        //{
        //    var columnNames = dataTable.Rows[0].ItemArray.Select(cell => cell.ToString().Trim()).ToList();

        //    return columnNames.Contains("Vendor") && columnNames.Contains("Vendor Name") && columnNames.Contains("Stock Typ");
        //}

        //// Method for processing the uploaded Excel file StorageType VMI 
        //private List<TempWarehouseItem> ProcessExcelFileVMI(DataRowCollection rows)
        //{
        //    List<TempWarehouseItem> uploadedData = new List<TempWarehouseItem>();

        //    foreach (DataRow row in rows.Cast<DataRow>().Skip(1)) // Skip header row
        //    {
        //        double actualQty;
        //        var actualQtyCell = row[10]; // Corrected index for the Unrestr field
        //        if (double.TryParse(actualQtyCell.ToString(), out actualQty))
        //        {
        //            // If Unrestr field contains a numeric value, set ActualQty to 0
        //            actualQty = 0;
        //        }

        //        string plant = row[0].ToString();
        //        string Description = "";

        //        // Tambahkan logika kondisional di sini
        //        switch (plant)
        //        {
        //            case "RIFA":
        //                Description = "PMI-AUDIO";
        //                break;
        //            case "RIFC":
        //                Description = "PMI-AIR CONDITIONER (AC)";
        //                break;
        //            case "RIFR":
        //                Description = "PMI-REFRIGRATOR";
        //                break;
        //            case "RIFW":
        //                Description = "PMI-IAQ";
        //                break;
        //            default:
        //                Description = "Default Description"; // Deskripsi default jika tidak ada kondisi yang cocok
        //                break;
        //        }

        //        TempWarehouseItem item = new TempWarehouseItem
        //        {
        //            WarehouseId = $"{row[0]}{row[1]}{row[2]}{row[6]}{row[8]}",
        //            Plant = row[0].ToString(),
        //            Sloc = row[1].ToString(),
        //            Month = row[2].ToString(),
        //            SerialNo = row[3].ToString(),
        //            TagNo = row[4].ToString(),
        //            StockType = row[5].ToString(),
        //            VendorCode = row[6].ToString(),
        //            VendorName = row[7].ToString(),
        //            Material = row[8].ToString(),
        //            MaterialDesc = row[9].ToString(),
        //            ActualQty = actualQty,
        //            QualInsp = row[11].ToString(),
        //            Blocked = row[12].ToString(),
        //            Unit = row[13].ToString(),
        //            IssuePlanner = row[14].ToString(),
        //            Isvmi = "VMI",
        //            Description = Description,
        //            WrhId = $"WRH-{row[0]}-{row[14]}",
        //            AccessPlant = $"WRH-{row[0]}"
        //        };

        //        uploadedData.Add(item);
        //    }

        //    return uploadedData;
        //}

        //// Method for processing the uploaded Excel file StorageType Non VMI 
        //private List<TempWarehouseItem> ProcessExcelFileNonVMI(DataRowCollection rows)
        //{
        //    List<TempWarehouseItem> uploadedData = new List<TempWarehouseItem>();

        //    foreach (var row in rows.Cast<DataRow>().Skip(1)) // Skip header row
        //    {
        //        double actualQty;
        //        var actualQtyCell = row[7]; // Corrected index for the Unrestr field
        //        if (double.TryParse(actualQtyCell.ToString(), out actualQty))
        //        {
        //            // If Unrestr field contains a numeric value, set ActualQty to 0
        //            actualQty = 0;
        //        }

        //        string plant = row[0].ToString();
        //        string Description = "";

        //        // Tambahkan logika kondisional di sini
        //        switch (plant)
        //        {
        //            case "RIFA":
        //                Description = "PMI-AUDIO";
        //                break;
        //            case "RIFC":
        //                Description = "PMI-AIR CONDITIONER (AC)";
        //                break;
        //            case "RIFR":
        //                Description = "PMI-REFRIGRATOR";
        //                break;
        //            case "RIFW":
        //                Description = "PMI-IAQ";
        //                break;
        //            default:
        //                Description = "Default Description"; // Deskripsi default jika tidak ada kondisi yang cocok
        //                break;
        //        }

        //        TempWarehouseItem item = new TempWarehouseItem
        //        {
        //            WarehouseId = $"{row[0]}{row[1]}{row[2]}{row[5]}",
        //            Plant = row[0].ToString(),
        //            Sloc = row[1].ToString(),
        //            Month = row[2].ToString(),
        //            SerialNo = row[3].ToString(),
        //            TagNo = row[4].ToString(),
        //            Material = row[5].ToString(),
        //            MaterialDesc = row[6].ToString(),
        //            ActualQty = actualQty,
        //            QualInsp = row[8].ToString(),
        //            Blocked = row[9].ToString(),
        //            Unit = row[10].ToString(),
        //            IssuePlanner = row[11].ToString(),
        //            StockType = "-", // Set to "-" as StockType, VendorCode, and VendorName are not available for NonVMI
        //            VendorCode = "-",
        //            VendorName = "-",
        //            Isvmi = "NonVMI",
        //            Description = Description,
        //            WrhId = $"WRH-{row[0]}-{row[11]}",
        //            AccessPlant = $"WRH-{row[0]}"
        //        };

        //        uploadedData.Add(item);
        //    }

        //    return uploadedData;
        //}
        //// END MAIN 


        // Main method for processing the uploaded Excel .xlsx file based on its content
        private List<TempWarehouseItem> ProcessExcelFile(IFormFile file)
        {
            List<TempWarehouseItem> uploadedData = new List<TempWarehouseItem>();

            // Read data from the Excel file using UTF-8 encoding
            using (var stream = file.OpenReadStream())
            {
                using (var workbook = new XLWorkbook(stream))
                {
                    var worksheet = workbook.Worksheet(1); // Assuming data is in the first worksheet
                    var rows = worksheet.RowsUsed();

                    // Check if the Excel file is VMI or Non-VMI based on column names
                    bool isVMI = IsVMIExcelFile(worksheet);

                    if (isVMI)
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

        // Check if the Excel file is VMI based on column names
        private bool IsVMIExcelFile(IXLWorksheet worksheet)
        {
            var headerRow = worksheet.Row(1);
            var columnNames = headerRow.Cells().Select(cell => cell.Value.ToString().Trim()).ToList();

            return columnNames.Contains("Vendor") && columnNames.Contains("Vendor Name") && columnNames.Contains("Stock Typ");
            //return columnNames.Contains("VendorCode") && columnNames.Contains("VendorName") && columnNames.Contains("StockType");
        }


        // Method for processing the uploaded Excel file StorageType VMI 
        private List<TempWarehouseItem> ProcessExcelFileVMI(IEnumerable<IXLRow> rows)
        {
            List<TempWarehouseItem> uploadedData = new List<TempWarehouseItem>();

            foreach (var row in rows.Skip(1)) // Skip header row
            {
                double actualQty;
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
                double actualQty;
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

        //// END MAIN 

        // Action method to handle the request for checking the status of saved data
        [HttpGet]
        public IActionResult CheckDataSaved()
        {
            // Example: Check if there is any WarehouseItem data in the database
            var isvmiValue = _dbContext.TempWarehouseItemLogs.FirstOrDefault(); // Mengambil data pertama dari tabel

            string result = "default"; // Default jika tidak ditemukan

            if (isvmiValue != null)
            {
                // Cek nilai ISVMI pada data yang diambil
                if (isvmiValue.Isvmi == "VMI")
                {
                    result = "VMI";
                }
                else if (isvmiValue.Isvmi == "NonVMI")
                {
                    result = "NonVMI";
                }
                else
                {
                    result = "Tidak Diketahui";
                }
            }

            return Json(new { statusISVMI = result });
        }

// =========================================================================================================================================================================================================== //





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
                worksheet.Cell(1, columnIndex).Value = "Stock Typ";
                columnIndex++;
                worksheet.Cell(1, columnIndex).Value = "Vendor";
                columnIndex++;
                worksheet.Cell(1, columnIndex).Value = "Vendor Name";
                columnIndex++;
            }

            worksheet.Cell(1, columnIndex).Value = "Material";
            columnIndex++;
            worksheet.Cell(1, columnIndex).Value = "Descr";
            columnIndex++;
            worksheet.Cell(1, columnIndex).Value = "Unrestr";
            columnIndex++;
            worksheet.Cell(1, columnIndex).Value = "Qual";
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
                worksheet.Cell(i + 2, 3).Value = int.TryParse(item.Month, out int monthValue) ? monthValue : 0;
                worksheet.Cell(i + 2, 4).Value = item.SerialNo;
                worksheet.Cell(i + 2, 5).Value = int.TryParse(item.TagNo, out int tagNoValue) ? tagNoValue : 0;

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
                worksheet.Cell(i + 2, columnIndex).Value = int.TryParse(item.QualInsp, out int qualValue) ? qualValue : 0;
                columnIndex++;
                worksheet.Cell(i + 2, columnIndex).Value = int.TryParse(item.Blocked, out int blockedValue) ? blockedValue : 0;
                columnIndex++;
                worksheet.Cell(i + 2, columnIndex).Value = item.Unit;
                columnIndex++;
                worksheet.Cell(i + 2, columnIndex).Value = item.IssuePlanner;
            }

            // Set the response headers for the download

            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;
            string excelFileName = "Warehouse_" + serialNo + ".xls";

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelFileName);
        }


// ========================================================================================================================================================================================================== //


// ================================================================================================ CHART DATA =============================================================================================== //

        // method progress bar count data all but not acces plant 
    
        [HttpGet]
        public IActionResult GetProgressData(string selectedMonth)
        {
            if (string.IsNullOrEmpty(selectedMonth))
            {
                return BadRequest("Selected month is missing or invalid.");
            }

            if (!int.TryParse(selectedMonth, out int selectedMonthValue))
            {
                return BadRequest("Selected month is not a valid integer.");
            }

            double vmiCount = _dbContext.WarehouseItems
                .Count(item => item.Isvmi == "VMI" && item.LastUpload.Month == selectedMonthValue);

            double nonVmiCount = _dbContext.WarehouseItems
                .Count(item => item.Isvmi == "NonVMI" && item.LastUpload.Month == selectedMonthValue);

            double totalCount = vmiCount + nonVmiCount;

            var monthlyCountProgressData = new[] { vmiCount, nonVmiCount, totalCount };
            
            return Json(monthlyCountProgressData);
        }


        // Method to display Chart for count data Vmi or Non Vmi 
        [HttpGet]
        public IActionResult GetChartData(int year)
        {
            if (year <= 0)
            {
                return BadRequest("Invalid year.");
            }

            string accessPlant = HttpContext.Session.GetString("AccessPlant") ?? "";

            var monthlyCountData = new List<double[]>();

            for (int month = 1; month <= 12; month++)
            {

                double vmiCount = _dbContext.WarehouseItems
                    .Count(item => item.Isvmi == "VMI" && item.LastUpload.Year == year && item.LastUpload.Month == month && item.AccessPlant == accessPlant);

                double nonVmiCount = _dbContext.WarehouseItems
                    .Count(item => item.Isvmi == "NonVMI" && item.LastUpload.Year == year && item.LastUpload.Month == month && item.AccessPlant == accessPlant);

                double totalCount = vmiCount + nonVmiCount;

                monthlyCountData.Add(new[] { vmiCount, nonVmiCount, totalCount });
            }
            return Json(monthlyCountData);
        }


        // Actual Qty 
        [HttpGet]
        public IActionResult GetChart2Data(int year)
        {
            if (year <= 0)
            {
                return BadRequest("Invalid year.");
            }

            string accessPlant = HttpContext.Session.GetString("AccessPlant") ?? "";

            var monthlyData = new List<double[]>();

            for (int month = 1; month <= 12; month++)
            {
                double vmiActualQty = _dbContext.WarehouseItems
                    .Where(item => item.Isvmi == "VMI" && item.LastUpload.Year == year && item.LastUpload.Month == month && item.AccessPlant == accessPlant)
                    .Sum(item => item.ActualQty);

                double nonVmiActualQty = _dbContext.WarehouseItems
                    .Where(item => item.Isvmi == "NonVMI" && item.LastUpload.Year == year && item.LastUpload.Month == month && item.AccessPlant == accessPlant)
                    .Sum(item => item.ActualQty);

                double totalActualQty = vmiActualQty + nonVmiActualQty;

                monthlyData.Add(new[] { vmiActualQty, nonVmiActualQty, totalActualQty });
            }

            return Json(monthlyData);
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
                var unitData = _dbContext.WarehouseItems
                    .Where(item => item.LastUpload.Month == selectedMonthValue && item.Isvmi == isvmiType && item.AccessPlant == accessPlant)
                    .GroupBy(item => item.Unit)
                    .Select(group => new
                    {
                        Unit = group.Key,
                        Count = group.Count()
                    })
                    .ToList();

                var labels = unitData.Select(data => data.Unit).ToArray();
                var values = unitData.Select(data => data.Count).ToArray();

                var chartData = new { labels, values };

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