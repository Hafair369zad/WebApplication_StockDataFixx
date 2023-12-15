function downloadExcel() {
    // Dapatkan nilai input nomor seri
    var serialNoInput = document.getElementById("serialNoInput").value;

    // Redirect to the Warehouse/DownloadExcel action with the serial number as a query parameter
    window.location.href = "/Production/DownloadExcel?serialNo=" + encodeURIComponent(serialNoInput);
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

// Dereng wonten Accesplant nipun
function updateDownloadButtonStateProd() {
    var downloadButton = document.querySelector(".main-dpProd button");
    var downloadStateProd = localStorage.getItem('downloadStateProd');

    if (downloadStateProd === 'open') {
        downloadButton.removeAttribute("disabled");
    } else {
        downloadButton.setAttribute("disabled", "disabled");
    }
}

/* Add similar event listener for Data Production page if needed*/
document.addEventListener("DOMContentLoaded", function () {
    updateDownloadButtonStateProd();
});



// Wonten AccesPlant nipun
//// Function to update the download button state for Data Production
//function updateDownloadButtonStateProd(accessPlant) {
//    var downloadButton = document.querySelector(".main-dpProd button");
//    var downloadStateProd = localStorage.getItem('downloadStateProd');

//    // Check if AccessPlant is present and matches the current user's AccessPlant
//    if (accessPlant && downloadStateProd === 'open') {
//        downloadButton.removeAttribute("disabled");
//    } else {
//        downloadButton.setAttribute("disabled", "disabled");
//    }
//}

//// Initial download button state update for Data Production
//document.addEventListener("DOMContentLoaded", function () {
//    var accessPlant = sessionStorage.getItem('AccessPlant');
//    updateDownloadButtonStateProd(accessPlant);
//});