using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using ClosedXML.Excel;
using System.Linq;
using System.Globalization;
using WebApplication_StockDataFixx.Database;
using WebApplication_StockDataFixx.Models.Domain;
using Microsoft.AspNetCore.Mvc.Rendering;
using DocumentFormat.OpenXml.Office.CustomUI;
using DocumentFormat.OpenXml.Bibliography;

namespace WebApplication_StockDataFixx.Controllers
{
    public class ManagementController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly PMIDbContext _dbContext;

        public ManagementController(IConfiguration configuration, IWebHostEnvironment webHostEnvironment, PMIDbContext dbContext)
        {
            _configuration = configuration;
            _webHostEnvironment = webHostEnvironment;
            _dbContext = dbContext;
        }


        // Method display dashboard page 
        [HttpGet]
        public ActionResult DashboardManagement()
        {
            // Tampilan halaman dashboard manajemen
            return View();
        }

        // Method dispaly report page
        [HttpGet]
        public ActionResult ReportManagement(string type)
        {
            // Get distinct months from the data and create a list of month names
            var distinctMonths = GetDistinctMonths();
            List<SelectListItem> monthList = distinctMonths.Select(m => new SelectListItem
            {
                Text = m.ToString("MMMM yyyy"),
                Value = m.ToString("yyyy-MM")
            }).ToList();

            ViewBag.MonthList = monthList;

            return View();
        }

        // Method to display partial page  
        public PartialViewResult PartialPage(string type, string selectedMonth)
        {
            object data = null;
            string partialViewName = null;

            if (type == "Warehouse_VMI")
            {
                data = GetWarehouseData("VMI", selectedMonth);
                partialViewName = "_WarehouseVMIPartial";
            }
            else if (type == "Warehouse_NVMI")
            {
                data = GetWarehouseData("NonVMI", selectedMonth);
                partialViewName = "_WarehouseNonVMIPartial";
            }
            else if (type == "Production")
            {
                data = GetProductionData(selectedMonth);
                partialViewName = "_ProductionPartial";
            }

            ViewBag.SelectedType = type;
            ViewBag.SelectedMonth = selectedMonth; // Menyimpan bulan yang dipilih

            // Get distinct months from the data and create a list of month names
            var distinctMonths = GetDistinctMonths();
            List<SelectListItem> monthList = distinctMonths.Select(m => new SelectListItem
            {
                Text = m.ToString("MMMM yyyy"),
                Value = m.ToString("yyyy-MM")
            }).ToList();

            ViewBag.MonthList = monthList;

            return PartialView(partialViewName, data);
        }

        // Initial GetUniqueMonths Method: to filter data by coloumn LastUpload for display data //
        private List<DateTime> GetDistinctMonths()
        {
            var distinctMonths = _dbContext.ProductionItems
                .Select(item => item.LastUpload.Date)
                .Concat(_dbContext.WarehouseItems.Select(item => item.LastUpload.Date))
                .Distinct()
                .OrderByDescending(date => date)
                .ToList();

            return distinctMonths;
        }


        // baru sudah norml tapi error sedikit 

        //private List<DateTime> GetDistinctMonths(string type)
        //{
        //    var distinctMonths = _dbContext.ProductionItems
        //        .Select(item => item.LastUpload)
        //        .Concat(_dbContext.WarehouseItems
        //            .Where(item =>
        //                (type == "Production") ||
        //                (type == "Warehouse_VMI" && item.Isvmi == "VMI") ||
        //                (type == "Warehouse_NVMI" && item.Isvmi == "NonVMI")
        //            )
        //            .Select(item => item.LastUpload))
        //        .Distinct()
        //        .OrderByDescending(date => date)
        //        .ToList();

        //    return distinctMonths;
        //}


        //private List<DateTime> GetDistinctMonths(string type)
        //{
        //    var productionMonths = _dbContext.ProductionItems
        //        .Select(item => item.LastUpload)
        //        .Distinct();

        //    var warehouseMonths = _dbContext.WarehouseItems
        //        .Where(item =>
        //            (type == "Production") ||
        //            (type == "Warehouse_VMI" && item.Isvmi == "VMI") ||
        //            (type == "Warehouse_NVMI" && item.Isvmi == "NonVMI")
        //        )
        //        .Select(item => item.LastUpload)
        //        .Distinct();

        //    var distinctMonths = productionMonths.Concat(warehouseMonths)
        //        .OrderByDescending(date => date)
        //        .ToList();

        //    return distinctMonths;
        //}

        //private List<DateTime> GetDistinctMonths(string type)
        //{
        //    var productionMonths = _dbContext.ProductionItems
        //        .Select(item => item.LastUpload)
        //        .Distinct();

        //    var warehouseMonths = _dbContext.WarehouseItems
        //        .Where(item =>
        //            (type == "Production") ||
        //            (type == "Warehouse_VMI" && item.Isvmi == "VMI") ||
        //            (type == "Warehouse_NVMI" && item.Isvmi == "NonVMI")
        //        )
        //        .Select(item => item.LastUpload)
        //        .Distinct();

        //    var distinctMonths = productionMonths.Concat(warehouseMonths)
        //        .OrderByDescending(date => date)
        //        .Distinct()
        //        .ToList();

        //    return distinctMonths;
        //}



        // Method to fetch data Warehouse from database 
        private List<WarehouseItem> GetWarehouseData(string isvmi, string selectedMonth)
        {
            DateTime now = DateTime.Now;
            DateTime lastMonth = now.AddMonths(-1);

            string accessPlant = HttpContext.Session.GetString("AccessPlant") ?? "";

            IQueryable<WarehouseItem> data = _dbContext.WarehouseItems;

            if (!string.IsNullOrEmpty(isvmi))
            {
                data = data.Where(item => item.Isvmi == isvmi);
            }

            if (selectedMonth == "Latest Update")
            {
                data = data.Where(item => item.LastUpload.Month == now.Month && item.LastUpload.Year == now.Year);
            }
            else
            {
                DateTime selectedMonthDate = DateTime.ParseExact(selectedMonth, "yyyy-MM", CultureInfo.InvariantCulture);
                data = data.Where(item => item.LastUpload.Month == selectedMonthDate.Month && item.LastUpload.Year == selectedMonthDate.Year);
            }

            if (accessPlant == "VIEW-RIFO")
            {
                // Tampilkan semua data dengan accessPlant WRH-RIFR, WRH-RIFA, WRH-RIFC, WRH-RIFL, WRH-RIFW
                data = data.Where(item => item.AccessPlant == "WRH-RIFA" || item.AccessPlant == "WRH-RIFC" || item.AccessPlant == "WRH-RIFL" || item.AccessPlant == "WRH-RIFR" || item.AccessPlant == "WRH-RIFW");
            }
            else if (accessPlant == "VIEW-RIFA")
            {
                // Tampilkan hanya data dengan accessPlant WRH-RIFW
                data = data.Where(item => item.AccessPlant == "WRH-RIFA");
            }
            else if (accessPlant == "VIEW-RIFC")
            {
                // Tampilkan hanya data dengan accessPlant WRH-RIFW
                data = data.Where(item => item.AccessPlant == "WRH-RIFC");
            }
            else if (accessPlant == "VIEW-RIFL")
            {
                // Tampilkan hanya data dengan accessPlant WRH-RIFW
                data = data.Where(item => item.AccessPlant == "WRH-RIFL");
            }
            else if (accessPlant == "VIEW-RIFR")
            {
                // Tampilkan hanya data dengan accessPlant WRH-RIFW
                data = data.Where(item => item.AccessPlant == "WRH-RIFR");
            }
            else if (accessPlant == "VIEW-RIFW")
            {
                // Tampilkan hanya data dengan accessPlant WRH-RIFW
                data = data.Where(item => item.AccessPlant == "WRH-RIFW");
            }
            // Tambahkan kondisi lain sesuai dengan nilai accessPlant lainnya di sini.

            return data.OrderBy(w => w.SerialNo).ToList();
        }


        // Method to fetch data Production from Database 
        private List<ProductionItem> GetProductionData(string selectedMonth)
        {
            DateTime now = DateTime.Now;
            DateTime lastMonth = now.AddMonths(-1);

            string accessPlant = HttpContext.Session.GetString("AccessPlant") ?? "";

            IQueryable<ProductionItem> data = _dbContext.ProductionItems;

            if (selectedMonth == "Latest Update")
            {
                data = data.Where(item => item.LastUpload.Month == now.Month && item.LastUpload.Year == now.Year);
            }
            else
            {
                DateTime selectedMonthDate = DateTime.ParseExact(selectedMonth, "yyyy-MM", CultureInfo.InvariantCulture);
                data = data.Where(item => item.LastUpload.Month == selectedMonthDate.Month && item.LastUpload.Year == selectedMonthDate.Year);
            }

            if (accessPlant == "VIEW-RIFO")
            {
                // Tampilkan semua data dengan accessPlant WRH-RIFR, WRH-RIFA, WRH-RIFC, WRH-RIFL, WRH-RIFW
                data = data.Where(item => item.AccessPlant == "PROD-RIFA" || item.AccessPlant == "PROD-RIFC" || item.AccessPlant == "PROD-RIFL" || item.AccessPlant == "PROD-RIFR" || item.AccessPlant == "PROD-RIFW");
            }
            else if (accessPlant == "VIEW-RIFA")
            {
                // Tampilkan hanya data dengan accessPlant WRH-RIFW
                data = data.Where(item => item.AccessPlant == "PROD-RIFA");
            }
            else if (accessPlant == "VIEW-RIFC")
            {
                // Tampilkan hanya data dengan accessPlant WRH-RIFW
                data = data.Where(item => item.AccessPlant == "PROD-RIFC");
            }
            else if (accessPlant == "VIEW-RIFL")
            {
                // Tampilkan hanya data dengan accessPlant WRH-RIFW
                data = data.Where(item => item.AccessPlant == "PROD-RIFL");
            }
            else if (accessPlant == "VIEW-RIFR")
            {
                // Tampilkan hanya data dengan accessPlant WRH-RIFW
                data = data.Where(item => item.AccessPlant == "PROD-RIFR");
            }
            else if (accessPlant == "VIEW-RIFW")
            {
                // Tampilkan hanya data dengan accessPlant WRH-RIFW
                data = data.Where(item => item.AccessPlant == "PROD-RIFW");
            }

            return data.OrderBy(w => w.SerialNo).ToList();
        }


        // ==================================================================================================== DOWNLOAD DATA =========================================================================================== //

        // Method to process download excel file 
        [HttpGet]
        public IActionResult DownloadExcel(string type, string serialNo)
        {
            if (type == "Production")
            {
                return DownloadProductionExcel(serialNo);
            }
            else if (type == "Warehouse_VMI" || type == "Warehouse_NVMI")
            {
                return DownloadWarehouseExcel(type, serialNo);
            }
            else
            {
                return BadRequest("Invalid selected type.");
            }
        }

        // Method to fetch data production to run proccess download excel file production 
        private List<ProductionItem> GetDownloadProd(string serialNo)
        {
            // Fetch data from the database
            var data = _dbContext.ProductionItems.AsQueryable();

            if (!string.IsNullOrEmpty(serialNo))
            {
                data = data.Where(w => w.SerialNo == serialNo);
            }

            return data.OrderBy(w => w.SerialNo).ToList();
        }


        // Method to run proccess download excel file production
        private IActionResult DownloadProductionExcel(string serialNo)
        {
            // Fetch production data based on the serial number
            List<ProductionItem> data = GetDownloadProd(serialNo);

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
                if (item.SerialNo == serialNo)
                {
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
            }

            // Set the response headers for the download

            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;
            string excelFileName = "Production_" + serialNo + ".xlsx";

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelFileName);
        }

        private List<WarehouseItem> GetDownloadWrh(string serialNo, string isvmi)
        {
            // Fetch data from the database
            var data = _dbContext.WarehouseItems.AsQueryable();

            if (!string.IsNullOrEmpty(serialNo))
            {
                data = data.Where(w => w.SerialNo == serialNo);
            }

            if (!string.IsNullOrEmpty(isvmi))
            {
                data = data.Where(item => item.Isvmi == isvmi);
            }

            return data.OrderBy(w => w.SerialNo).ToList();
        }

        private IActionResult DownloadWarehouseExcel(string type, string serialNo)
        {
            string isvmi = (type == "Warehouse_VMI") ? "VMI" : "NonVMI";
            List<WarehouseItem> data = GetDownloadWrh(serialNo, isvmi);

            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Data");

            // Add the header row
            worksheet.Cell(1, 1).Value = "Plant";
            worksheet.Cell(1, 2).Value = "Sloc";
            worksheet.Cell(1, 3).Value = "Month";
            worksheet.Cell(1, 4).Value = "Serial No";
            worksheet.Cell(1, 5).Value = "Tag No";

            int columnIndex = 6; // Start index for columns after Tag No

            // If Isvmi is true, include Stock Type, Vendor Code, and Vendor Name in header
            if (isvmi == "VMI")
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

                // If Isvmi is true, include Stock Type, Vendor Code, and Vendor Name in data rows
                if (isvmi == "VMI")
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

        // ============================================================================================================================================================================================================== //



        // ================================================================================================ CHART DATA WAREHOUSE ======================================================================================== //

        // Count Data (grafik2Wrh)
        //[HttpGet]
        //public IActionResult GetChartDataWrh(int year)
        //{
        //    if (year <= 0)
        //    {
        //        return BadRequest("Invalid year.");
        //    }

        //    var monthlyCountData = new List<double[]>();

        //    for (int month = 1; month <= 12; month++)
        //    {

        //        double vmiCount = _dbContext.WarehouseItems
        //            .Count(item => item.Isvmi == "VMI" && item.LastUpload.Year == year && item.LastUpload.Month == month);

        //        double nonVmiCount = _dbContext.WarehouseItems
        //            .Count(item => item.Isvmi == "NonVMI" && item.LastUpload.Year == year && item.LastUpload.Month == month);

        //        double totalCount = vmiCount + nonVmiCount;

        //        monthlyCountData.Add(new[] { vmiCount, nonVmiCount, totalCount });
        //    }
        //    return Json(monthlyCountData);
        //}

        [HttpGet]
        public IActionResult GetChartDataWrh(int year)
        {
            if (year <= 0)
            {
                return BadRequest("Invalid year.");
            }

            string accessPlant = HttpContext.Session.GetString("AccessPlant") ?? "";

            var monthlyCountData = new List<double[]>();

            for (int month = 1; month <= 12; month++)
            {
                IQueryable<WarehouseItem> data = _dbContext.WarehouseItems
                    .Where(item => item.LastUpload.Year == year && item.LastUpload.Month == month);

                if (!string.IsNullOrEmpty(accessPlant))
                {
                    if (accessPlant == "VIEW-RIFO")
                    {
                        // Tampilkan data dengan accessPlant WRH-RIFR, WRH-RIFA, WRH-RIFC, WRH-RIFL, WRH-RIFW
                        data = data.Where(item => item.AccessPlant == "WRH-RIFA" || item.AccessPlant == "WRH-RIFC" || item.AccessPlant == "WRH-RIFL" || item.AccessPlant == "WRH-RIFR" || item.AccessPlant == "WRH-RIFW");
                    }
                    else if (accessPlant == "VIEW-RIFA")
                    {
                        // Tampilkan data dengan accessPlant WRH-RIFW
                        data = data.Where(item => item.AccessPlant == "WRH-RIFA");
                    }
                    else if (accessPlant == "VIEW-RIFC")
                    {
                        // Tampilkan data dengan accessPlant WRH-RIFW
                        data = data.Where(item => item.AccessPlant == "WRH-RIFC");
                    }
                    else if (accessPlant == "VIEW-RIFL")
                    {
                        // Tampilkan data dengan accessPlant WRH-RIFW
                        data = data.Where(item => item.AccessPlant == "WRH-RIFL");
                    }
                    else if (accessPlant == "VIEW-RIFR")
                    {
                        // Tampilkan data dengan accessPlant WRH-RIFW
                        data = data.Where(item => item.AccessPlant == "WRH-RIFR");
                    }
                    else if (accessPlant == "VIEW-RIFW")
                    {
                        // Tampilkan data dengan accessPlant WRH-RIFW
                        data = data.Where(item => item.AccessPlant == "WRH-RIFW");
                    }
                }

                double vmiCount = data.Count(item => item.Isvmi == "VMI");
                double nonVmiCount = data.Count(item => item.Isvmi == "NonVMI");
                double totalCount = vmiCount + nonVmiCount;

                monthlyCountData.Add(new[] { vmiCount, nonVmiCount, totalCount });
            }

            return Json(monthlyCountData);
        }



        // Actual Qty Data (grafik1Wrh)
        //[HttpGet]
        //public IActionResult GetChart2DataWrh(int year)
        //{
        //    if (year <= 0)
        //    {
        //        return BadRequest("Invalid year.");
        //    }

        //    var monthlyData = new List<double[]>();

        //    for (int month = 1; month <= 12; month++)
        //    {
        //        double vmiActualQty = _dbContext.WarehouseItems
        //            .Where(item => item.Isvmi == "VMI" && item.LastUpload.Year == year && item.LastUpload.Month == month)
        //            .Sum(item => item.ActualQty);

        //        double nonVmiActualQty = _dbContext.WarehouseItems
        //            .Where(item => item.Isvmi == "NonVMI" && item.LastUpload.Year == year && item.LastUpload.Month == month)
        //            .Sum(item => item.ActualQty);

        //        double totalActualQty = vmiActualQty + nonVmiActualQty;

        //        monthlyData.Add(new[] { vmiActualQty, nonVmiActualQty, totalActualQty });
        //    }

        //    return Json(monthlyData);
        //}

        [HttpGet]
        public IActionResult GetChart2DataWrh(int year)
        {
            if (year <= 0)
            {
                return BadRequest("Invalid year.");
            }

            string accessPlant = HttpContext.Session.GetString("AccessPlant") ?? "";

            var monthlyData = new List<double[]>();

            for (int month = 1; month <= 12; month++)
            {
                IQueryable<WarehouseItem> data = _dbContext.WarehouseItems
                    .Where(item => item.LastUpload.Year == year && item.LastUpload.Month == month);

                if (!string.IsNullOrEmpty(accessPlant))
                {
                    if (accessPlant == "VIEW-RIFO")
                    {
                        // Tampilkan data dengan accessPlant WRH-RIFR, WRH-RIFA, WRH-RIFC, WRH-RIFL, WRH-RIFW
                        data = data.Where(item => item.AccessPlant == "WRH-RIFA" || item.AccessPlant == "WRH-RIFC" || item.AccessPlant == "WRH-RIFL" || item.AccessPlant == "WRH-RIFR" || item.AccessPlant == "WRH-RIFW");
                    }
                    else if (accessPlant == "VIEW-RIFA")
                    {
                        // Tampilkan data dengan accessPlant WRH-RIFW
                        data = data.Where(item => item.AccessPlant == "WRH-RIFA");
                    }
                    else if (accessPlant == "VIEW-RIFC")
                    {
                        // Tampilkan data dengan accessPlant WRH-RIFW
                        data = data.Where(item => item.AccessPlant == "WRH-RIFC");
                    }
                    else if (accessPlant == "VIEW-RIFL")
                    {
                        // Tampilkan data dengan accessPlant WRH-RIFW
                        data = data.Where(item => item.AccessPlant == "WRH-RIFL");
                    }
                    else if (accessPlant == "VIEW-RIFR")
                    {
                        // Tampilkan data dengan accessPlant WRH-RIFW
                        data = data.Where(item => item.AccessPlant == "WRH-RIFR");
                    }
                    else if (accessPlant == "VIEW-RIFW")
                    {
                        // Tampilkan data dengan accessPlant WRH-RIFW
                        data = data.Where(item => item.AccessPlant == "WRH-RIFW");
                    }
                }

                double vmiActualQty = data.Where(item => item.Isvmi == "VMI").Sum(item => item.ActualQty);
                double nonVmiActualQty = data.Where(item => item.Isvmi == "NonVMI").Sum(item => item.ActualQty);
                double totalActualQty = vmiActualQty + nonVmiActualQty;

                monthlyData.Add(new[] { vmiActualQty, nonVmiActualQty, totalActualQty });
            }

            return Json(monthlyData);
        }



        //[HttpGet]
        //public IActionResult GetChart3DataWrh(string selectedMonth)
        //{
        //    return GetChartDataForUnitTypeWrh(selectedMonth, "VMI");
        //}

        //[HttpGet]
        //public IActionResult GetChart4DataWrh(string selectedMonth)
        //{
        //    return GetChartDataForUnitTypeWrh(selectedMonth, "NonVMI");
        //}

        //private IActionResult GetChartDataForUnitTypeWrh(string selectedMonth, string isvmiType)
        //{
        //    if (string.IsNullOrEmpty(selectedMonth))
        //    {
        //        return BadRequest("Selected month is missing or invalid.");
        //    }

        //    if (!int.TryParse(selectedMonth, out int selectedMonthValue))
        //    {
        //        return BadRequest("Selected month is not a valid integer.");
        //    }

        //    try
        //    {
        //        double totalKUnits = _dbContext.WarehouseItems
        //            .Where(item => item.LastUpload.Month == selectedMonthValue && item.Unit == "K" && item.Isvmi == isvmiType)
        //            .Count();

        //        double totalPcsUnits = _dbContext.WarehouseItems
        //            .Where(item => item.LastUpload.Month == selectedMonthValue && item.Unit == "PC" && item.Isvmi == isvmiType)
        //            .Count();

        //        double totalSetUnits = _dbContext.WarehouseItems
        //            .Where(item => item.LastUpload.Month == selectedMonthValue && item.Unit == "SET" && item.Isvmi == isvmiType)
        //            .Count();

        //        double totalGUnits = _dbContext.WarehouseItems
        //            .Where(item => item.LastUpload.Month == selectedMonthValue && item.Unit == "G" && item.Isvmi == isvmiType)
        //            .Count();

        //        double totalKGUnits = _dbContext.WarehouseItems
        //            .Where(item => item.LastUpload.Month == selectedMonthValue && item.Unit == "KG" && item.Isvmi == isvmiType)
        //            .Count();

        //        double totalMUnits = _dbContext.WarehouseItems
        //            .Where(item => item.LastUpload.Month == selectedMonthValue && item.Unit == "M" && item.Isvmi == isvmiType)
        //            .Count();
        //        double totalMLUnits = _dbContext.WarehouseItems
        //           .Where(item => item.LastUpload.Month == selectedMonthValue && item.Unit == "ML" && item.Isvmi == isvmiType)
        //           .Count();
        //        double totalROLLUnits = _dbContext.WarehouseItems
        //           .Where(item => item.LastUpload.Month == selectedMonthValue && item.Unit == "ROLL" && item.Isvmi == isvmiType)
        //           .Count();

        //        var chartData = new[] { totalKUnits, totalPcsUnits, totalSetUnits, totalGUnits, totalKGUnits, totalMUnits, totalMLUnits, totalROLLUnits };

        //        return Json(chartData);
        //    }
        //    catch (Exception)
        //    {
        //        return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while fetching chart data.");
        //    }
        //}

        [HttpGet]
        public IActionResult GetChart3DataWrh(string selectedMonth)
        {
            return GetChartDataForUnitTypeWrh(selectedMonth, "VMI");
        }

        [HttpGet]
        public IActionResult GetChart4DataWrh(string selectedMonth)
        {
            return GetChartDataForUnitTypeWrh(selectedMonth, "NonVMI");
        }

        private IActionResult GetChartDataForUnitTypeWrh(string selectedMonth, string isvmiType)
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
                IQueryable<WarehouseItem> data = _dbContext.WarehouseItems
                    .Where(item => item.LastUpload.Month == selectedMonthValue && item.Isvmi == isvmiType);

                if (!string.IsNullOrEmpty(accessPlant))
                {
                    if (accessPlant == "VIEW-RIFO")
                    {
                        // Tampilkan data dengan accessPlant WRH-RIFR, WRH-RIFA, WRH-RIFC, WRH-RIFL, WRH-RIFW
                        data = data.Where(item => item.AccessPlant == "WRH-RIFA" || item.AccessPlant == "WRH-RIFC" || item.AccessPlant == "WRH-RIFL" || item.AccessPlant == "WRH-RIFR" || item.AccessPlant == "WRH-RIFW");
                    }
                    else if (accessPlant == "VIEW-RIFA")
                    {
                        // Tampilkan data dengan accessPlant WRH-RIFW
                        data = data.Where(item => item.AccessPlant == "WRH-RIFA");
                    }
                    else if (accessPlant == "VIEW-RIFC")
                    {
                        // Tampilkan data dengan accessPlant WRH-RIFW
                        data = data.Where(item => item.AccessPlant == "WRH-RIFC");
                    }
                    else if (accessPlant == "VIEW-RIFL")
                    {
                        // Tampilkan data dengan accessPlant WRH-RIFW
                        data = data.Where(item => item.AccessPlant == "WRH-RIFL");
                    }
                    else if (accessPlant == "VIEW-RIFR")
                    {
                        // Tampilkan data dengan accessPlant WRH-RIFW
                        data = data.Where(item => item.AccessPlant == "WRH-RIFR");
                    }
                    else if (accessPlant == "VIEW-RIFW")
                    {
                        // Tampilkan data dengan accessPlant WRH-RIFW
                        data = data.Where(item => item.AccessPlant == "WRH-RIFW");
                    }
                }

                double totalKUnits = data.Count(item => item.Unit == "K");
                double totalPcsUnits = data.Count(item => item.Unit == "PC");
                double totalSetUnits = data.Count(item => item.Unit == "SET");
                double totalGUnits = data.Count(item => item.Unit == "G");
                double totalKGUnits = data.Count(item => item.Unit == "KG");
                double totalMUnits = data.Count(item => item.Unit == "M");
                double totalMLUnits = data.Count(item => item.Unit == "ML");
                double totalROLLUnits = data.Count(item => item.Unit == "ROLL");

                var chartData = new[] { totalKUnits, totalPcsUnits, totalSetUnits, totalGUnits, totalKGUnits, totalMUnits, totalMLUnits, totalROLLUnits };

                return Json(chartData);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while fetching chart data.");
            }
        }


        // ============================================================================================================================================================================================================== //


        // =============================================================================================== CHART DATA PRODUCTION ======================================================================================== //

        // total count data (grafik2Prod)
        //[HttpGet]
        //public IActionResult GetChartDataProd(int year)
        //{
        //    if (year <= 0)
        //    {
        //        return BadRequest("Invalid year.");
        //    }

        //    string accessPlant = HttpContext.Session.GetString("AccessPlant") ?? "";

        //    var monthlyCountData = new List<double[]>();

        //    for (int month = 1; month <= 12; month++)
        //    {

        //        double totalProductionItems = _dbContext.ProductionItems
        //            .Where(item => item.LastUpload.Year == year && item.LastUpload.Month == month)
        //            .Count();
        //        double totalAllProductionItems = _dbContext.ProductionItems
        //            .Where(item => item.LastUpload.Year == year && item.LastUpload.Month == month)
        //            .Count();

        //        monthlyCountData.Add(new[] { totalProductionItems, totalAllProductionItems });
        //    }
        //    return Json(monthlyCountData);
        //}

        [HttpGet]
        public IActionResult GetChartDataProd(int year)
        {
            if (year <= 0)
            {
                return BadRequest("Invalid year.");
            }

            string accessPlant = HttpContext.Session.GetString("AccessPlant") ?? "";

            var monthlyCountData = new List<double[]>();

            for (int month = 1; month <= 12; month++)
            {
                IQueryable<ProductionItem> data = _dbContext.ProductionItems
                    .Where(item => item.LastUpload.Year == year && item.LastUpload.Month == month);

                if (!string.IsNullOrEmpty(accessPlant))
                {
                    if (accessPlant == "VIEW-RIFO")
                    {
                        // Tampilkan data dengan accessPlant WRH-RIFR, WRH-RIFA, WRH-RIFC, WRH-RIFL, WRH-RIFW
                        data = data.Where(item => item.AccessPlant == "PROD-RIFA" || item.AccessPlant == "PROD-RIFC" || item.AccessPlant == "PROD-RIFL" || item.AccessPlant == "PROD-RIFR" || item.AccessPlant == "PROD-RIFW");
                    }
                    else if (accessPlant == "VIEW-RIFA")
                    {
                        // Tampilkan data dengan accessPlant WRH-RIFW
                        data = data.Where(item => item.AccessPlant == "PROD-RIFA");
                    }
                    else if (accessPlant == "VIEW-RIFC")
                    {
                        // Tampilkan data dengan accessPlant WRH-RIFW
                        data = data.Where(item => item.AccessPlant == "PROD-RIFC");
                    }
                    else if (accessPlant == "VIEW-RIFL")
                    {
                        // Tampilkan data dengan accessPlant WRH-RIFW
                        data = data.Where(item => item.AccessPlant == "PROD-RIFL");
                    }
                    else if (accessPlant == "VIEW-RIFR")
                    {
                        // Tampilkan data dengan accessPlant WRH-RIFW
                        data = data.Where(item => item.AccessPlant == "PROD-RIFR");
                    }
                    else if (accessPlant == "VIEW-RIFW")
                    {
                        // Tampilkan data dengan accessPlant WRH-RIFW
                        data = data.Where(item => item.AccessPlant == "PROD-RIFW");
                    }
                }

                double totalProductionItems = data.Count();
                double totalAllProductionItems = _dbContext.ProductionItems
                    .Count(item => item.LastUpload.Year == year && item.LastUpload.Month == month);

                monthlyCountData.Add(new[] { totalProductionItems, totalAllProductionItems });
            }
            return Json(monthlyCountData);
        }



        // total Actual Qty data (grafik1Prod)
        //[HttpGet]
        //public IActionResult GetChart2DataProd(int year)
        //{
        //    if (year <= 0)
        //    {
        //        return BadRequest("Invalid year.");
        //    }

        //    var monthlyActualQtyData = new List<double[]>();

        //    for (int month = 1; month <= 12; month++)
        //    {
        //        double monthlyActualQty = _dbContext.ProductionItems
        //            .Where(item => item.LastUpload.Year == year && item.LastUpload.Month == month)
        //            .Sum(item => item.ActualQty);

        //        double AllActualQty = _dbContext.ProductionItems
        //            .Where(item => item.LastUpload.Year == year && item.LastUpload.Month == month)
        //            .Sum(item => item.ActualQty);

        //        monthlyActualQtyData.Add(new[] { monthlyActualQty, AllActualQty });
        //    }

        //    return Json(monthlyActualQtyData);

        //}


        [HttpGet]
        public IActionResult GetChart2DataProd(int year)
        {
            if (year <= 0)
            {
                return BadRequest("Invalid year.");
            }

            string accessPlant = HttpContext.Session.GetString("AccessPlant") ?? "";

            var monthlyActualQtyData = new List<double[]>();

            for (int month = 1; month <= 12; month++)
            {
                IQueryable<ProductionItem> data = _dbContext.ProductionItems
                    .Where(item => item.LastUpload.Year == year && item.LastUpload.Month == month);

                if (!string.IsNullOrEmpty(accessPlant))
                {
                    if (accessPlant == "VIEW-RIFO")
                    {
                        // Tampilkan data dengan accessPlant WRH-RIFR, WRH-RIFA, WRH-RIFC, WRH-RIFL, WRH-RIFW
                        data = data.Where(item => item.AccessPlant == "PROD-RIFA" || item.AccessPlant == "PROD-RIFC" || item.AccessPlant == "PROD-RIFL" || item.AccessPlant == "PROD-RIFR" || item.AccessPlant == "PROD-RIFW");
                    }
                    else if (accessPlant == "VIEW-RIFA")
                    {
                        // Tampilkan data dengan accessPlant WRH-RIFW
                        data = data.Where(item => item.AccessPlant == "PROD-RIFA");
                    }
                    else if (accessPlant == "VIEW-RIFC")
                    {
                        // Tampilkan data dengan accessPlant WRH-RIFW
                        data = data.Where(item => item.AccessPlant == "PROD-RIFC");
                    }
                    else if (accessPlant == "VIEW-RIFL")
                    {
                        // Tampilkan data dengan accessPlant WRH-RIFW
                        data = data.Where(item => item.AccessPlant == "PROD-RIFL");
                    }
                    else if (accessPlant == "VIEW-RIFR")
                    {
                        // Tampilkan data dengan accessPlant WRH-RIFW
                        data = data.Where(item => item.AccessPlant == "PROD-RIFR");
                    }
                    else if (accessPlant == "VIEW-RIFW")
                    {
                        // Tampilkan data dengan accessPlant WRH-RIFW
                        data = data.Where(item => item.AccessPlant == "PROD-RIFW");
                    }
                }

                double monthlyActualQty = data.Sum(item => item.ActualQty);
                double AllActualQty = _dbContext.ProductionItems
                    .Where(item => item.LastUpload.Year == year && item.LastUpload.Month == month)
                    .Sum(item => item.ActualQty);

                monthlyActualQtyData.Add(new[] { monthlyActualQty, AllActualQty });
            }

            return Json(monthlyActualQtyData);
        }



        // Chart Data Per UoM
        //[HttpGet]
        //public IActionResult GetChart3DataProd(string selectedMonth)
        //{
        //    if (string.IsNullOrEmpty(selectedMonth))
        //    {
        //        return BadRequest("Selected month is missing or invalid.");
        //    }

        //    if (!int.TryParse(selectedMonth, out int selectedMonthValue))
        //    {
        //        return BadRequest("Selected month is not a valid integer.");
        //    }

        //    try
        //    {
        //        double totalKUnits = _dbContext.ProductionItems
        //            .Where(item => item.LastUpload.Month == selectedMonthValue && item.Unit == "K")
        //            .Count();

        //        double totalPcsUnits = _dbContext.ProductionItems
        //            .Where(item => item.LastUpload.Month == selectedMonthValue && item.Unit == "PC")
        //            .Count();

        //        double totalSetUnits = _dbContext.ProductionItems
        //            .Where(item => item.LastUpload.Month == selectedMonthValue && item.Unit == "SET")
        //            .Count();

        //        double totalGUnits = _dbContext.ProductionItems
        //            .Where(item => item.LastUpload.Month == selectedMonthValue && item.Unit == "G")
        //            .Count();

        //        double totalKGUnits = _dbContext.ProductionItems
        //            .Where(item => item.LastUpload.Month == selectedMonthValue && item.Unit == "KG")
        //            .Count();

        //        double totalMUnits = _dbContext.ProductionItems
        //            .Where(item => item.LastUpload.Month == selectedMonthValue && item.Unit == "M")
        //            .Count();

        //        double totalMLUnits = _dbContext.ProductionItems
        //           .Where(item => item.LastUpload.Month == selectedMonthValue && item.Unit == "ML")
        //           .Count();

        //        double totalROLLUnits = _dbContext.ProductionItems
        //           .Where(item => item.LastUpload.Month == selectedMonthValue && item.Unit == "ROLL")
        //           .Count();

        //        var chartData = new[] { totalKUnits, totalPcsUnits, totalSetUnits, totalGUnits, totalKGUnits, totalMUnits, totalMLUnits, totalROLLUnits };

        //        return Json(chartData);
        //    }
        //    catch (Exception)
        //    {
        //        return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while fetching chart data.");
        //    }
        //}

        [HttpGet]
        public IActionResult GetChart3DataProd(string selectedMonth)
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
                IQueryable<ProductionItem> data = _dbContext.ProductionItems
                    .Where(item => item.LastUpload.Month == selectedMonthValue);

                if (!string.IsNullOrEmpty(accessPlant))
                {
                    if (accessPlant == "VIEW-RIFO")
                    {
                        // Tampilkan data dengan accessPlant WRH-RIFR, WRH-RIFA, WRH-RIFC, WRH-RIFL, WRH-RIFW
                        data = data.Where(item => item.AccessPlant == "PROD-RIFA" || item.AccessPlant == "PROD-RIFC" || item.AccessPlant == "PROD-RIFL" || item.AccessPlant == "PROD-RIFR" || item.AccessPlant == "PROD-RIFW");
                    }
                    else if (accessPlant == "VIEW-RIFA")
                    {
                        // Tampilkan data dengan accessPlant WRH-RIFW
                        data = data.Where(item => item.AccessPlant == "PROD-RIFA");
                    }
                    else if (accessPlant == "VIEW-RIFC")
                    {
                        // Tampilkan data dengan accessPlant WRH-RIFW
                        data = data.Where(item => item.AccessPlant == "PROD-RIFC");
                    }
                    else if (accessPlant == "VIEW-RIFL")
                    {
                        // Tampilkan data dengan accessPlant WRH-RIFW
                        data = data.Where(item => item.AccessPlant == "PROD-RIFL");
                    }
                    else if (accessPlant == "VIEW-RIFR")
                    {
                        // Tampilkan data dengan accessPlant WRH-RIFW
                        data = data.Where(item => item.AccessPlant == "PROD-RIFR");
                    }
                    else if (accessPlant == "VIEW-RIFW")
                    {
                        // Tampilkan data dengan accessPlant WRH-RIFW
                        data = data.Where(item => item.AccessPlant == "PROD-RIFW");
                    }
                }

                double totalKUnits = data.Count(item => item.Unit == "K");
                double totalPcsUnits = data.Count(item => item.Unit == "PC");
                double totalSetUnits = data.Count(item => item.Unit == "SET");
                double totalGUnits = data.Count(item => item.Unit == "G");
                double totalKGUnits = data.Count(item => item.Unit == "KG");
                double totalMUnits = data.Count(item => item.Unit == "M");
                double totalMLUnits = data.Count(item => item.Unit == "ML");
                double totalROLLUnits = data.Count(item => item.Unit == "ROLL");

                var chartData = new[] { totalKUnits, totalPcsUnits, totalSetUnits, totalGUnits, totalKGUnits, totalMUnits, totalMLUnits, totalROLLUnits };

                return Json(chartData);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while fetching chart data.");
            }
        }


        // ============================================================================================================================================================================================================== //

    }
}




// lama 
//public PartialViewResult PartialPage(string type, string month)
//{
//    object data = null;
//    string partialViewName = null;

//    if (type == "Warehouse_VMI")
//    {
//        data = GetWarehouseData("VMI", month);
//        partialViewName = "_WarehouseVMIPartial";
//    }
//    else if (type == "Warehouse_NVMI")
//    {
//        data = GetWarehouseData("NonVMI", month);
//        partialViewName = "_WarehouseNonVMIPartial";
//    }
//    else if (type == "Production")
//    {
//        data = GetProductionData(month);
//        partialViewName = "_ProductionPartial";
//    }

//    ViewBag.SelectedType = type;

//    // Get distinct months from the data and create a list of month names
//    var distinctMonths = GetDistinctMonths();
//    List<SelectListItem> monthList = distinctMonths.Select(m => new SelectListItem
//    {
//        Text = m.ToString("MMMM yyyy"),
//        Value = m.ToString("yyyy-MM")
//    }).ToList();

//    ViewBag.MonthList = monthList;

//    return PartialView(partialViewName, data);
//}

//private List<DateTime> GetDistinctMonths()
//{
//    var distinctMonths = _dbContext.ProductionItems
//        .Select(item => item.LastUpload)
//        .Concat(_dbContext.WarehouseItems.Select(item => item.LastUpload))
//        .Distinct()
//        .OrderByDescending(date => date)
//        .ToList();

//    return distinctMonths;
//}


//private List<WarehouseItem> GetWarehouseData(string isvmi, string month)
//{
//    DateTime now = DateTime.Now;
//    DateTime lastMonth = now.AddMonths(-1);

//    IQueryable<WarehouseItem> data = _dbContext.WarehouseItems;

//    if (!string.IsNullOrEmpty(isvmi))
//    {
//        data = data.Where(item => item.Isvmi == isvmi);
//    }

//    if (month == "thisMonth")
//    {
//        data = data.Where(item => item.LastUpload.Month == now.Month && item.LastUpload.Year == now.Year);
//    }
//    else if (month == "lastMonth")
//    {
//        data = data.Where(item => item.LastUpload.Month == lastMonth.Month && item.LastUpload.Year == lastMonth.Year);
//    }

//    return data.OrderBy(w => w.SerialNo).ToList();
//}

//private List<ProductionItem> GetProductionData(string month)
//{
//    DateTime now = DateTime.Now;
//    DateTime lastMonth = now.AddMonths(-1);

//    IQueryable<ProductionItem> data = _dbContext.ProductionItems;

//    if (month == "thisMonth")
//    {
//        data = data.Where(item => item.LastUpload.Month == now.Month && item.LastUpload.Year == now.Year);
//    }
//    else if (month == "lastMonth")
//    {
//        data = data.Where(item => item.LastUpload.Month == lastMonth.Month && item.LastUpload.Year == lastMonth.Year);
//    }

//    return data.OrderBy(w => w.SerialNo).ToList();
//}