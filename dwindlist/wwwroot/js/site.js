// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
$(document).ready(function () {
    $("#search-box-confirm").click(function(){
        window.location.replace(`/TodoItem/Search/${$("#search-box").val()}`);
    });

    $(".toggle-expand").click(function() {
        let thisElement = $(`#${$(this).attr("id")}`);
        let thisId = $(thisElement).val();

        $.ajax({
            type: "PUT",
            url: `/todoitem/toggleexpanded/${thisId}`,
            success: function (response) {
                // Handle the API response as needed
                console.log("Todo list item toggled successfully");
                parentItem = $(`#parent-item${thisId}`);
                parentItem.prop("hidden", !parentItem.prop("hidden"));
                thisElement.text(thisElement.text() == ">" ? "v" : ">");
            },
            error: function (error) {
                // Handle API error
                console.error("Error adding toggling todo list item:", error);
                $(`#alert${thisId}`).text("An error occurred");
            }
        });
    });

    $(".toggle-status").click(function() {
        let thisElement = $(`#${$(this).attr("id")}`);
        let thisId = $(thisElement).val();
        thisElement.prop("disabled", true);

        $.ajax({
            type: "PUT",
            url: `/todoitem/togglestatus/${thisId}`,
            success: function (response) {
                // Handle the API response as needed
                console.log("Todo list item toggled successfully");
                $(`#edit-item-label${thisId}`).toggleClass("text-decoration-line-through");
                thisElement.prop("disabled", false);
            },
            error: function (error) {
                // Handle API error
                console.error("Error adding toggling todo list item:", error);
                $(`#alert${thisId}`).text("An error occurred");
            }
        });
    });

    $(".add-item").click(function () {
        let thisElement = $(`#${$(this).attr("id")}`);
        let thisId = $(thisElement).val();

        thisElement.prop("disabled", true);

        let userData = {
            Label: $(`#new-item-label${thisId}`).val(),
            ParentId: thisId,
            Status: 'i'
        };

        console.log(userData);

        $.ajax({
            type: "POST",
            url: `/todoitem/add/${thisId}`,
            data: JSON.stringify(userData),
            contentType: "application/json; charset=utf-8",
            success: function (response) {
                // Handle the API response as needed
                console.log("Todo list item added successfully");
                window.location.reload();
            },
            error: function (error) {
                // Handle API error
                console.error("Error adding todo list item:", error);
                $(`#alert${thisId}`).text("An error occurred");
            }
        });
    });

    $(".edit-item").click(function() {
        let thisElement = $(`#${$(this).attr("id")}`);
        let thisId = $(thisElement).val();

        thisElement.prop("hidden", true);

        $(`#save-item${thisId}`).prop("hidden", false);
        $(`#edit-item-label${thisId}`).prop("disabled", false);
    });

    $(".save-item").click(function() {
        let thisElement = $(`#${$(this).attr("id")}`);
        let thisId = $(thisElement).val();

        thisElement.prop("disabled", true);
        thisLabel = $(`#edit-item-label${thisId}`);
        thisLabel.prop("disabled", true);

        let userData = {
            Label: thisLabel.val()
        };

        $.ajax({
            type: "PUT",
            url: `/todoitem/updatelabel/${thisId}`,
            data: JSON.stringify(userData),
            contentType: "application/json; charset=utf-8",
            success: function (response) {
                // Handle the API response as needed
                console.log("Todo list item updated successfully");
                $(`edit-item-label${thisId}`).text(response);
                thisElement.prop("disabled", false);
                thisElement.prop("hidden", true);
                $(`#edit-item${thisId}`).prop("hidden", false);
            },
            error: function (error) {
                // Handle API error
                console.error("Error updating todo list item:", error);
                $(`#alert${thisId}`).text("An error occurred");
            }
        });
    });
});
