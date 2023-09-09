using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using ClosedXML.Excel;
using System.Linq;
using WebApplication_StockDataFixx.Database;
using WebApplication_StockDataFixx.Models.Domain;
using Microsoft.AspNetCore.Mvc.Rendering;


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

        [HttpGet]
        public ActionResult DashboardManagement()
        {
            // Tampilan halaman dashboard manajemen
            return View();
        }

        [HttpGet]
        public ActionResult ReportManagement()
        {
            return View(); // Mengirim data ke halaman "ReportManagement.cshtml"
        }

        public PartialViewResult PartialPage(string type, string month)
        {
            object data = null;
            string partialViewName = null;

            if (type == "Warehouse_VMI")
            {
                data = GetWarehouseData("VMI", month);
                partialViewName = "_WarehouseVMIPartial";
            }
            else if (type == "Warehouse_NVMI")
            {
                data = GetWarehouseData("NonVMI", month);
                partialViewName = "_WarehouseNonVMIPartial";
            }
            else if (type == "Production")
            {
                data = GetProductionData(month);
                partialViewName = "_ProductionPartial";
            }

            ViewBag.SelectedType = type;

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

        private List<DateTime> GetDistinctMonths()
        {
            var distinctMonths = _dbContext.ProductionItems
                .Select(item => item.LastUpload)
                .Concat(_dbContext.WarehouseItems.Select(item => item.LastUpload))
                .Distinct()
                .OrderByDescending(date => date)
                .ToList();

            return distinctMonths;
        }



        [HttpGet]
        public IActionResult DownloadExcel(string type, string serialNo)
        {
            if (type == "Production")
            {
                return DownloadProductionExcel(serialNo);
            }
            else if (type == "Warehouse_VMI" || type == "Warehouse_NVMI")
            {
                string month = "thisMonth"; // Default to this month for Warehouse data download
                return DownloadWarehouseExcel(type, month, serialNo);
            }
            else
            {
                return BadRequest("Invalid selected type.");
            }
        }

        private IActionResult DownloadProductionExcel(string serialNo)
        {
            // Fetch production data based on the serial number
            List<ProductionItem> data = GetProductionData(serialNo);

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
            string excelFileName = "Production" + serialNo + ".xlsx";

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelFileName);
        }


        private IActionResult DownloadWarehouseExcel(string type, string month, string serialNo)
        {
            string isvmi = (type == "Warehouse_VMI") ? "VMI" : "NonVMI";
            List<WarehouseItem> data = GetWarehouseData(isvmi, month);

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
            string excelFileName = serialNo + ".xlsx";

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelFileName);
        }



        private List<WarehouseItem> GetWarehouseData(string isvmi, string month)
        {
            DateTime now = DateTime.Now;
            DateTime lastMonth = now.AddMonths(-1);

            IQueryable<WarehouseItem> data = _dbContext.WarehouseItems;

            if (!string.IsNullOrEmpty(isvmi))
            {
                data = data.Where(item => item.Isvmi == isvmi);
            }

            if (month == "thisMonth")
            {
                data = data.Where(item => item.LastUpload.Month == now.Month && item.LastUpload.Year == now.Year);
            }
            else if (month == "lastMonth")
            {
                data = data.Where(item => item.LastUpload.Month == lastMonth.Month && item.LastUpload.Year == lastMonth.Year);
            }

            return data.OrderBy(w => w.SerialNo).ToList();
        }

        private List<ProductionItem> GetProductionData(string month)
        {
            DateTime now = DateTime.Now;
            DateTime lastMonth = now.AddMonths(-1);




        //  ///////////// Chart Data //////////////

        [HttpGet]
        public IActionResult GetChartDataWrh(string selectedMonth)
        {
            if (string.IsNullOrEmpty(selectedMonth))
            {
                return BadRequest("Selected month is missing or invalid.");
            }

            if (!int.TryParse(selectedMonth, out int selectedMonthValue))
            {
                return BadRequest("Selected month is not a valid integer.");
            }

            int vmiCount = _dbContext.WarehouseItems.Count(item => item.Isvmi == "VMI" && item.LastUpload.Month == selectedMonthValue);
            int nonVmiCount = _dbContext.WarehouseItems.Count(item => item.Isvmi == "NonVMI" && item.LastUpload.Month == selectedMonthValue);

            var chartData = new[] { vmiCount, nonVmiCount };

            return Json(chartData);
        }

        [HttpGet]
        public IActionResult GetChart2DataWrh(string selectedMonth)
        {
            if (string.IsNullOrEmpty(selectedMonth))
            {
                return BadRequest("Selected month is missing or invalid.");
            }

            if (!int.TryParse(selectedMonth, out int selectedMonthValue))
            {
                return BadRequest("Selected month is not a valid integer.");
            }

            int vmiActualQty = _dbContext.WarehouseItems
                .Where(item => item.Isvmi == "VMI" && item.LastUpload.Month == selectedMonthValue)
                .Sum(item => item.ActualQty);

            int nonVmiActualQty = _dbContext.WarehouseItems
                .Where(item => item.Isvmi == "NonVMI" && item.LastUpload.Month == selectedMonthValue)
                .Sum(item => item.ActualQty);

            var chartData = new[] { vmiActualQty, nonVmiActualQty };

            return Json(chartData);
        }


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

            try
            {
                int totalKUnits = _dbContext.WarehouseItems
                    .Where(item => item.LastUpload.Month == selectedMonthValue && item.Unit == "K" && item.Isvmi == isvmiType)
                    .Count();

                int totalPcsUnits = _dbContext.WarehouseItems
                    .Where(item => item.LastUpload.Month == selectedMonthValue && item.Unit == "PC" && item.Isvmi == isvmiType)
                    .Count();

                int totalSetUnits = _dbContext.WarehouseItems
                    .Where(item => item.LastUpload.Month == selectedMonthValue && item.Unit == "SET" && item.Isvmi == isvmiType)
                    .Count();

                int totalGUnits = _dbContext.WarehouseItems
                    .Where(item => item.LastUpload.Month == selectedMonthValue && item.Unit == "G" && item.Isvmi == isvmiType)
                    .Count();

                int totalKGUnits = _dbContext.WarehouseItems
                    .Where(item => item.LastUpload.Month == selectedMonthValue && item.Unit == "KG" && item.Isvmi == isvmiType)
                    .Count();

                int totalMUnits = _dbContext.WarehouseItems
                    .Where(item => item.LastUpload.Month == selectedMonthValue && item.Unit == "M" && item.Isvmi == isvmiType)
                    .Count();

                var chartData = new[] { totalKUnits, totalPcsUnits, totalSetUnits, totalGUnits, totalKGUnits, totalMUnits };

                return Json(chartData);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while fetching chart data.");
            }
        }


        //  ///////////// Chart Data Production //////////////

        [HttpGet]
        public IActionResult GetChartDataProd(string selectedMonth)
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
        public IActionResult GetChart2DataProd(string selectedMonth)
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
               .Where(item => item.LastUpload.Month == selectedMonthValue)
               .Sum(item => item.ActualQty);

            var chartData = new { TotalActualQty = monthlyActualQty };

            return Json(chartData);
        }



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

    }
}
