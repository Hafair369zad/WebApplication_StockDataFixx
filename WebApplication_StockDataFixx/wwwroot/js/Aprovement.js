//// Function to set the state in localStorage
//function setDownloadState(state) {
//    localStorage.setItem('downloadState', state);
//    updateStatusDisplay(state);
//}

//function updateStatusDisplay(state) {
//    var statusElement = document.getElementById("sttsFtrDow");

//    if (state === 'open') {
//        statusElement.textContent = 'OPEN';
//        statusElement.classList.remove('close');
//        statusElement.classList.add('open');
//    } else if (state === 'close') {
//        statusElement.textContent = 'CLOSE';
//        statusElement.classList.remove('open');
//        statusElement.classList.add('close');
//    }
//}

//// Function to handle the button clicks in the Aprove Data Warehouse section
//document.addEventListener("DOMContentLoaded", function () {
//    var openButton = document.getElementById("openBtnFtrWrh");
//    var closeButton = document.getElementById("closeBtnFtrWrh");

//    openButton.addEventListener("click", function () {
//        setDownloadState('open');
//    });

//    closeButton.addEventListener("click", function () {
//        setDownloadState('close');
//    });

//    // Initial display update based on the saved state
//    var initialState = localStorage.getItem('downloadState');
//    if (initialState) {
//        updateStatusDisplay(initialState);
//    }
//});





// =============================== BELUM ADA PER PLANT NYA ===============================
// Function to set the state in localStorage for Data Warehouse
function setDownloadStateWrh(state) {
    localStorage.setItem('downloadStateWrh', state);
    updateStatusDisplay('sttsFtrDowWrh', state);
    updateDownloadButtonStateWrh();
}

// Function to set the state in localStorage for Data Production
function setDownloadStateProd(state) {
    localStorage.setItem('downloadStateProd', state);
    updateStatusDisplay('sttsFtrDowProd', state);
    updateDownloadButtonStateProd();
}

// Function to update the status display for both Warehouse and Production
function updateStatusDisplay(statusElementId, state) {
    var statusElement = document.getElementById(statusElementId);

    if (state === 'open') {
        statusElement.textContent = 'OPEN';
        statusElement.classList.remove('close');
        statusElement.classList.add('open');
    } else if (state === 'close') {
        statusElement.textContent = 'CLOSE';
        statusElement.classList.remove('open');
        statusElement.classList.add('close');
    }
}

// Function to handle the button clicks in the Aprove Data Warehouse section
document.addEventListener("DOMContentLoaded", function () {
    var openButtonWrh = document.getElementById("openBtnFtrWrh");
    var closeButtonWrh = document.getElementById("closeBtnFtrWrh");

    openButtonWrh.addEventListener("click", function () {
        setDownloadStateWrh('open');
    });

    closeButtonWrh.addEventListener("click", function () {
        setDownloadStateWrh('close');
    });

    // Initial display update based on the saved state for Warehouse
    var initialStateWrh = localStorage.getItem('downloadStateWrh');
    if (initialStateWrh) {
        updateStatusDisplay('sttsFtrDowWrh', initialStateWrh);
    }

    // Repeat the process for Data Production
    var openButtonProd = document.getElementById("openBtnFtrProd");
    var closeButtonProd = document.getElementById("closeBtnFtrProd");

    openButtonProd.addEventListener("click", function () {
        setDownloadStateProd('open');
    });

    closeButtonProd.addEventListener("click", function () {
        setDownloadStateProd('close');
    });

    // Initial display update based on the saved state for Production
    var initialStateProd = localStorage.getItem('downloadStateProd');
    if (initialStateProd) {
        updateStatusDisplay('sttsFtrDowProd', initialStateProd);
    }
});
// ===============================================================================================







// Wonten AccesPlant nipun 

//function setDownloadStateWrh(state) {
//    localStorage.setItem('downloadStateWrh', state);

//    // Retrieve the AccessPlant from the session
//    var accessPlant = sessionStorage.getItem('AccessPlant');

//    // Store the AccessPlant in localStorage
//    localStorage.setItem('accessPlant', accessPlant);

//    updateStatusDisplay('sttsFtrDowWrh', state);
//    updateDownloadButtonStateWrh(accessPlant);
//}

//// Function to set the state in localStorage for Data Production
//function setDownloadStateProd(state) {
//    localStorage.setItem('downloadStateProd', state);

//    // Retrieve the AccessPlant from the session
//    var accessPlant = sessionStorage.getItem('AccessPlant');

//    // Store the AccessPlant in localStorage
//    localStorage.setItem('accessPlant', accessPlant);

//    updateStatusDisplay('sttsFtrDowProd', state);
//    updateDownloadButtonStateProd(accessPlant);
//}

//// Function to update the status display for both Warehouse and Production
//function updateStatusDisplay(statusElementId, state) {
//    var statusElement = document.getElementById(statusElementId);

//    if (state === 'open') {
//        statusElement.textContent = 'OPEN';
//        statusElement.classList.remove('close');
//        statusElement.classList.add('open');
//    } else if (state === 'close') {
//        statusElement.textContent = 'CLOSE';
//        statusElement.classList.remove('open');
//        statusElement.classList.add('close');
//    }
//}

//// Function to handle the button clicks in the Approve Data Warehouse section
//document.addEventListener("DOMContentLoaded", function () {
//    var openButtonWrh = document.getElementById("openBtnFtrWrh");
//    var closeButtonWrh = document.getElementById("closeBtnFtrWrh");

//    openButtonWrh.addEventListener("click", function () {
//        setDownloadStateWrh('open');
//    });

//    closeButtonWrh.addEventListener("click", function () {
//        setDownloadStateWrh('close');
//    });

//    // Initial display update based on the saved state for Warehouse
//    var initialStateWrh = localStorage.getItem('downloadStateWrh');
//    if (initialStateWrh) {
//        updateStatusDisplay('sttsFtrDowWrh', initialStateWrh);
//        var accessPlant = sessionStorage.getItem('AccessPlant');
//        updateDownloadButtonStateWrh(accessPlant);
//    }

//    // Repeat the process for Data Production
//    var openButtonProd = document.getElementById("openBtnFtrProd");
//    var closeButtonProd = document.getElementById("closeBtnFtrProd");

//    openButtonProd.addEventListener("click", function () {
//        setDownloadStateProd('open');
//    });

//    closeButtonProd.addEventListener("click", function () {
//        setDownloadStateProd('close');
//    });

//    // Initial display update based on the saved state for Production
//    var initialStateProd = localStorage.getItem('downloadStateProd');
//    if (initialStateProd) {
//        updateStatusDisplay('sttsFtrDowProd', initialStateProd);
//        var accessPlant = sessionStorage.getItem('AccessPlant');
//        updateDownloadButtonStateProd(accessPlant);
//    }
//});