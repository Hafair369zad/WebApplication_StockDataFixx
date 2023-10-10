

// Humbuger Active
var hamburger = document.querySelector(".hamburger");
hamburger.addEventListener("click", function () {
    document.querySelector("body").classList.toggle("active");
})



// Nav Link Active 
$(document).ready(function () {
    // Get the current URL path
    var currentPath = window.location.pathname;

    // Remove the trailing slash (if any)
    currentPath = currentPath.replace(/\/$/, '');

    // Find the corresponding menu item and add the "active" class
    $('#nav_accordion li').each(function () {
        var menuItem = $(this).find('a');
        var menuItemUrl = menuItem.attr('href');

        // Check if the current path matches the menu item URL
        if (currentPath === menuItemUrl) {
            menuItem.addClass('active');
        }
    });
});

