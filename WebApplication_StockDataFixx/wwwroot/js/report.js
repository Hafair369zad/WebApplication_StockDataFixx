
//function filterDataBySerialNumber(serialNumber, table) {
//    var rowsToDownload = [];
//    for (var i = 1; i < table.rows.length; i++) {
//        var row = table.rows[i];
//        var rowData = row.cells[0].innerText;
//        if (rowData === serialNumber) {
//            var rowDataArray = [];
//            for (var j = 0; j < row.cells.length; j++) {
//                rowDataArray.push(row.cells[j].innerText);
//            }
//            rowsToDownload.push(rowDataArray);
//        }
//    }
//    return rowsToDownload;
//}

//function downloadExcel() {
//    // Dapatkan nilai input nomor seri
//    var serialNumberInput = document.getElementById("serialNumberInput").value;

//    // Dapatkan tabel data
//    var table = document.getElementById("data-table");

//    // Filter data berdasarkan nomor seri yang diinputkan
//    var filteredRows = filterDataBySerialNumber(serialNumberInput, table);

//    // Jika data yang difilter ditemukan, maka lakukan proses unduh
//    if (filteredRows.length > 0) {
//        // Buat workbook dan worksheet
//        var wb = XLSX.utils.book_new();
//        var ws = XLSX.utils.aoa_to_sheet([
//            ["Serial Number", "Tag No", "Material", "Material Description", "Actual Qty", "Unit", "Stock Type", "Vendor Code", "Vendor Name"]
//        ]);

//        // Tambahkan data yang sesuai ke worksheet
//        XLSX.utils.sheet_add_aoa(ws, filteredRows, { origin: -1 });

//        // Tambahkan worksheet ke workbook
//        XLSX.utils.book_append_sheet(wb, ws, "Data");

//        // Konversi workbook menjadi binary
//        var wbout = XLSX.write(wb, { bookType: 'xlsx', type: 'array' });

//        // Memulai unduhan dengan menetapkan nama file yang disesuaikan
//        var filename = serialNumberInput + ".xlsx";
//        var blob = new Blob([wbout], { type: 'application/octet-stream' });
//        var url = URL.createObjectURL(blob);
//        var link = document.createElement("a");
//        link.setAttribute("href", url);
//        link.setAttribute("download", filename);
//        document.body.appendChild(link); // Diperlukan untuk Firefox
//        link.click();
//    } else {
//        // Jika data yang difilter tidak ditemukan, tampilkan pesan peringatan
//        alert("Close filter search terlebih dahulu untuk menemukan serial number berikut'" + serialNumberInput + "'.");
//    }
//}


//function handleSelectedTypeChange() {
//    var selectedType = document.getElementById("selectedType").value;
//    var dataTable = document.getElementById("data-table");
//    var columns = dataTable.getElementsByTagName("th");

//    // Hide all columns
//    for (var i = 0; i < columns.length; i++) {
//        columns[i].style.display = "none";
//    }

//    // Display common columns
//    columns[0].style.display = ""; // Serial Number
//    columns[1].style.display = ""; // Tag No
//    columns[2].style.display = ""; // Material
//    columns[3].style.display = ""; // Material Description
//    columns[4].style.display = ""; // Actual Qty
//    columns[5].style.display = ""; // Unit

//    if (selectedType === "Production") {
//        // Display Production-specific columns
//        columns[6].style.display = "none"; // Stock Type
//        columns[7].style.display = "none"; // Vendor Code (hidden)
//        columns[8].style.display = "none"; // Vendor Name (hidden)
//    } else if (selectedType === "Warehouse") {
//        // Display Warehouse-specific columns
//        columns[6].style.display = ""; // Stock Type
//        columns[7].style.display = ""; // Vendor Code
//        columns[8].style.display = ""; // Vendor Name
//    }
//}