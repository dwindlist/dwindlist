// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
$(document).ready(function () {
    function resetErrors() {
        $(".custom-alert").text("");
        $(".add-alert").text("");
    }

    const genericError = "An error occurred. Please reload the page if the problem persists."
    const emptyError = "Label can't be empty."
    const longError = "Label is too long."

    $("#search-box-confirm").click(function(){
        let searchBoxVal = $("#search-box").val();
        if (searchBoxVal === "") { return; }
        window.location.replace(`/TodoItem/Search/${searchBoxVal}`);
    });

    $(".toggle-expand").click(function() {
        resetErrors();
        let thisElement = $(`#${$(this).attr("id")}`);
        let thisId = $(thisElement).val();
        thisElement.prop("disabled", true);

        $.ajax({
            type: "PUT",
            url: `/todoitem/toggleexpanded/${thisId}`,
            success: function (response) {
                // Handle the API response as needed
                console.log("Todo list item toggled successfully");
                parentItem = $(`#parent-item${thisId}`);
                parentItem.prop("hidden", !parentItem.prop("hidden"));
                thisElement.text(thisElement.text() == ">" ? "v" : ">");
                thisElement.prop("disabled", false);
            },
            error: function (error) {
                // Handle API error
                console.error("Error adding toggling todo list item:", error);
                $(`#alert${thisId}`).text(genericError);
                thisElement.prop("disabled", false);
            }
        });
    });

    $(".toggle-status").click(function() {
        resetErrors();
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
                $(`.filtered-item${thisId}`).remove();
            },
            error: function (error) {
                // Handle API error
                console.error("Error adding toggling todo list item:", error);
                $(`#alert${thisId}`).text(genericError);
                thisElement.prop("disabled", false);
            }
        });
    });

    $(".add-item").click(function () {
        resetErrors();
        let thisElement = $(`#${$(this).attr("id")}`);
        let thisId = $(thisElement).val();

        let trimmedInput = $(`#new-item-label${thisId}`).val().trim();

        if (trimmedInput.length < 1) {
            $(`#add-alert${thisId}`).text(emptyError);
            return;
        }

        if (trimmedInput.length > 64) {
            $(`#add-alert${thisId}`).text(longError);
            return;
        }

        thisElement.prop("disabled", true);

        let userData = {
            Label: trimmedInput,
            ParentId: thisId,
            Status: 'i'
        };

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
                $(`#add-alert${thisId}`).text(genericError);
                thisElement.prop("disabled", false);
            }
        });
    });

    $(".edit-item").click(function() {
        resetErrors();
        let thisElement = $(`#${$(this).attr("id")}`);
        let thisId = $(thisElement).val();
        thisElement.val($(`#edit-item-label${thisId}`).val());

        thisElement.prop("hidden", true);

        $(`#cancel-item${thisId}`).prop("hidden", false);
        $(`#save-item${thisId}`).prop("hidden", false);
        $(`#edit-item-label${thisId}`).prop("disabled", false);
    });

    $(".cancel-item").click(function() {
        resetErrors();
        let thisElement = $(`#${$(this).attr("id")}`);
        let thisId = $(thisElement).val();
        let thisEdit = $(`#edit-item${thisId}`);
        let thisLabel = $(`#edit-item-label${thisId}`);

        thisLabel.val($(`#edit-item${thisId}`).val());
        thisEdit.val(thisId);

        $(`#save-item${thisId}`).prop("hidden", true);
        thisElement.prop("hidden", true);
        thisEdit.prop("hidden", false);
        thisLabel.prop("disabled", true);
    });

    $(".save-item").click(function() {
        resetErrors();
        let thisElement = $(`#${$(this).attr("id")}`);
        let thisId = $(thisElement).val();

        let thisLabel = $(`#edit-item-label${thisId}`);
        let trimmedInput = thisLabel.val().trim();

        if (trimmedInput.length < 1) {
            $(`#alert${thisId}`).text(emptyError);
            $thisLabel.val($(`#edit-item${thisId}`).val());
            return;
        }

        if (trimmedInput.length > 64) {
            $(`#alert${thisId}`).text(longError);
            $thisLabel.val($(`#edit-item${thisId}`).val());
            return;
        }

        thisLabel.prop("disabled", true);

        let userData = {
            Label: trimmedInput
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
                thisElement.prop("hidden", true);
                $(`#cancel-item${thisId}`).prop("hidden", true);
                $(`#edit-item${thisId}`).prop("hidden", false);
            },
            error: function (error) {
                // Handle API error
                console.error("Error updating todo list item:", error);
                $(`#alert${thisId}`).text(genericError);
                $(`#edit-item-label${thisId}`).val($(`#edit-item${thisId}`).val());
                thisElement.prop("hidden", true);
                $(`#cancel-item${thisId}`).prop("hidden", true);
                $(`#edit-item${thisId}`).prop("hidden", false);
            }
        });
    });

    $(".delete-item").click(function() {
        resetErrors();
        let thisElement = $(`#${$(this).attr("id")}`);
        let thisId = $(thisElement).val();
        let thisText = $(`#edit-item-label${thisId}`).val();

        $("#delete-modal-name").text(thisText);
        $("#confirm-delete").val(thisId);
        $("#exampleModalCenter").modal("show");
    });

    $(".cancel-delete").click(function() {
        resetErrors();
        let thisElement = $(`#${$(this).attr("id")}`);
        let thisId = $(thisElement).val();

        $("#delete-modal-name").text("");
        $("#confirm-delete").val("");
        $("#exampleModalCenter").modal("hide");
    });

    $("#confirm-delete").click(function() {
        resetErrors();
        let thisElement = $(this);
        let thisId = $(thisElement).val();

        console.log(`Deleting: ${thisId}`);
        thisElement.prop("disabled", true);

        let userData = {
            Label: thisElement.val()
        };

        $.ajax({
            type: "PUT",
            url: `/todoitem/delete/${thisId}`,
            data: JSON.stringify(userData),
            contentType: "application/json; charset=utf-8",
            success: function (response) {
                // Handle the API response as needed
                console.log("Todo list item deleted successfully");
                $("#delete-modal-name").text("");
                $("#confirm-delete").val("");
                $("#confirm-delete").prop("disabled", false);
                $("#exampleModalCenter").modal("hide");
                $(`.todo-item${thisId}`).remove();
                $(`.filtered-item${thisId}`).remove();
            },
            error: function (error) {
                // Handle API error
                console.error("Error deleting todo list item:", error);
                $(`#alert${thisId}`).text(genericError);
            }
        });
    });
});
