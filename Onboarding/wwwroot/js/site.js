// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


// Notifications Bell
function showNotifications() {
    $.get('/Notification/GetNotifications', function (data) {
        $('#notificationsDropdown').html(data).toggle();
        $.post('/Notification/MarkNotificationsAsRead', function (response) {
            if (response.success) {
                $('.notification-bell .badge').remove();
            }
        });
    });
}

$(document).ready(function () {
    // Handle bell icon click
    $('#notification-bell').on('click', function (e) {
        e.preventDefault();
        var dropdown = $('#notifications-dropdown');

        if (dropdown.is(':visible')) {
            dropdown.hide();
        } else {
            $.ajax({
                url: '@Url.Action("GetNotifications", "Notification")',
                type: 'GET',
                success: function (data) {
                    dropdown.html(data).show();
                    markNotificationsAsRead();
                },
                error: function () {
                    dropdown.html('<p>Error loading notifications.</p>').show();
                }
            });
        }
    });

    // Mark notifications as read
    function markNotificationsAsRead() {
        $.ajax({
            url: '@Url.Action("MarkNotificationsAsRead", "Notification")',
            type: 'POST',
            success: function () {
                $('#notification-bell .badge').remove();
            }
        });
    }

    // Hide dropdown when clicking outside
    $(document).on('click', function (e) {
        if (!$(e.target).closest('#notification-bell, #notifications-dropdown').length) {
            $('#notifications-dropdown').hide();
        }
    });
});