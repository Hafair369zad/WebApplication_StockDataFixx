function downloadExcel() {
    // Dapatkan nilai input nomor seri
    var serialNoInput = document.getElementById("serialNoInput").value;

    // Redirect to the Warehouse/DownloadExcel action with the serial number as a query parameter
    window.location.href = "/Warehouse/DownloadExcel?serialNo=" + encodeURIComponent(serialNoInput);
}

function validateSerialNumber() {
    var serialNoInput = document.getElementById("serialNoInput").value;
    if (!serialNoInput) {
        alert("Please enter a serial number before downloading the file.");
    } else {
        // Serial number is entered, proceed with the download
        downloadExcel();
    }
}


// Dereng wonten per accesplant nipun
// Function to update the download button state for Data Warehouse
function updateDownloadButtonStateWrh() {
    var downloadButton = document.querySelector(".main-dpWrh button");
    var downloadStateWrh = localStorage.getItem('downloadStateWrh');

    if (downloadStateWrh === 'open') {
        downloadButton.removeAttribute("disabled");
    } else {
        downloadButton.setAttribute("disabled", "disabled");
    }
}

// Function to update the download button state for Data Production


// Initial download button state update for Data Warehouse
document.addEventListener("DOMContentLoaded", function () {
    updateDownloadButtonStateWrh();
});


// Wonten accesplant nipun 
//// Function to update the download button state for Data Warehouse
//function updateDownloadButtonStateWrh(accessPlant) {
//    var downloadButton = document.querySelector(".main-dpWrh button");
//    var downloadStateWrh = localStorage.getItem('downloadStateWrh');

//    // Check if AccessPlant is present and matches the current user's AccessPlant
//    if (accessPlant && downloadStateWrh === 'open') {
//        downloadButton.removeAttribute("disabled");
//    } else {
//        downloadButton.setAttribute("disabled", "disabled");
//    }
//}

//// Initial download button state update for Data Warehouse
//document.addEventListener("DOMContentLoaded", function () {
//    var accessPlant = sessionStorage.getItem('AccessPlant');
//    updateDownloadButtonStateWrh(accessPlant);
//});



