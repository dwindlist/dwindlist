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
        const searchBoxVal = $("#search-box").val();
        if (searchBoxVal === "") { return; }
        window.location.replace(`/TodoItem/Search/${searchBoxVal}`);
    });

    function getRelatedObjects(obj) {
        const thisItem = obj.closest(".todo-item");
        const id = thisItem.data("item-id");
        return {
            item: thisItem,
            id: id,
            type: thisItem.data("item-type"),
            status: thisItem.find(".status-checkbox").eq(0),
            label: thisItem.find(".item-label").eq(0),
            expand: thisItem.find(".item-label").eq(0),
            edit: thisItem.find(".edit-button").eq(0),
            cancel: thisItem.find(".cancel-button").eq(0),
            save: thisItem.find(".save-button").eq(0),
            delete: thisItem.find(".delete-button").eq(0),
            alert: thisItem.find(".custom-alert").eq(0),
            sublist: $(`*[data-parent-id="${id}"]`).eq(0)
        }
    }

    function getRelatedAddObjects(obj){
        const thisItem = obj.closest(".add-form");
        const id = thisItem.data("item-id");
        return {
            item: thisItem,
            id: id,
            label: thisItem.find(".item-label"),
            alert: thisItem.find(".add-alert"),
        }
    }

    $(".expand-button").click(function() {
        resetErrors();
        const thisElement = $(this);
        const related = getRelatedObjects(thisElement);

        thisElement.prop("disabled", true);

        $.ajax({
            type: "PUT",
            url: `/todoitem/toggleexpanded/${related.id}`,
            success: function (response) {
                // Handle the API response as needed
                console.log("Todo list item toggled successfully");
                related.sublist.prop("hidden", !related.sublist.prop("hidden"));
                thisElement.text(thisElement.text() == ">" ? "v" : ">");
                thisElement.prop("disabled", false);
            },
            error: function (error) {
                // Handle API error
                console.error("Error toggling todo list item:", error);
                related.alert.eq(0).text(genericError);
                thisElement.prop("disabled", false);
            }
        });
    });

    $(".status-checkbox").click(function() {
        resetErrors();
        const thisElement = $(this);
        const related = getRelatedObjects(thisElement);
        thisElement.prop("disabled", true);

        $.ajax({
            type: "PUT",
            url: `/todoitem/togglestatus/${related.id}`,
            success: function (response) {
                // Handle the API response as needed
                console.log("Todo list item toggled successfully");
                if (related.type == "filtered") {
                    related.item.remove();
                    return;
                };
                related.label.toggleClass("text-decoration-line-through");
                thisElement.prop("disabled", false);
            },
            error: function (error) {
                // Handle API error
                console.error("Error adding toggling todo list item:", error);
                related.alert.text(genericError);
                thisElement.prop("disabled", false);
            }
        });
    });

    $(".add-button").click(function () {
        resetErrors();
        const thisElement = $(this);
        const related = getRelatedAddObjects(thisElement);
        console.log(`Adding to item ${related.id}`);

        let trimmedInput = related.label.val().trim();

        if (trimmedInput.length < 1) {
            related.alert.text(emptyError);
            return;
        }

        if (trimmedInput.length > 64) {
            related.alert.text(longError);
            return;
        }

        thisElement.prop("disabled", true);

        const userData = {
            Label: trimmedInput,
            ParentId: related.id,
            Status: 'i'
        };

        $.ajax({
            type: "POST",
            url: `/todoitem/add/${related.id}`,
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
                related.alert.text(genericError);
                thisElement.prop("disabled", false);
            }
        });
    });

    $(".edit-button").click(function() {
        resetErrors();
        const thisElement = $(this);
        const related = getRelatedObjects(thisElement);

        thisElement.data("label", related.label.val());
        thisElement.prop("hidden", true);

        related.label.prop("disabled", false);
        related.cancel.prop("hidden", false);
        related.save.prop("hidden", false);
    });

    $(".cancel-button").click(function() {
        resetErrors();
        const thisElement = $(this);
        const related = getRelatedObjects(thisElement);

        related.label.val(related.edit.data("label"));
        related.edit.data("label", "");

        thisElement.prop("hidden", true);
        related.save.prop("hidden", true);
        related.edit.prop("hidden", false);
        related.label.prop("disabled", true);
    });

    $(".save-button").click(function() {
        resetErrors();
        const thisElement = $(this);
        const related = getRelatedObjects(thisElement);

        const trimmedInput = related.label.val().trim();

        if (trimmedInput.length < 1) {
            related.alert.text(emptyError);
            related.label.val(related.edit.data("label"));
            return;
        }

        if (trimmedInput.length > 64) {
            related.alert.text(longError);
            related.label.val(related.edit.data("label"));
            return;
        }

        related.label.prop("disabled", true);

        const userData = {
            Label: trimmedInput
        };

        $.ajax({
            type: "PUT",
            url: `/todoitem/updatelabel/${related.id}`,
            data: JSON.stringify(userData),
            contentType: "application/json; charset=utf-8",
            success: function (response) {
                // Handle the API response as needed
                console.log("Todo list item updated successfully");
                thisElement.prop("hidden", true);
                related.label.text(response);
                related.edit.prop("hidden", false);
                related.cancel.prop("hidden", true);
            },
            error: function (error) {
                // Handle API error
                console.error("Error updating todo list item:", error);
                thisElement.prop("hidden", true);
                related.alert.text(genericError);
                related.label.val(related.edit.data("label"));
                related.cancel.prop("hidden", true);
                related.save.prop("hidden", false);
            }
        });
    });

    $(".delete-button").click(function() {
        resetErrors();
        const thisElement = $(this);
        const related = getRelatedObjects(thisElement);

        $("#delete-modal-name").text(related.label.val());
        $("#confirm-delete").data("id", related.id);
        $("#exampleModalCenter").modal("show");
    });

    $(".cancel-delete").click(function() {
        resetErrors();

        $("#delete-modal-name").text("");
        $("#confirm-delete").val("");
        $("#exampleModalCenter").modal("hide");
    });

    $("#confirm-delete").click(function() {
        resetErrors();
        const thisElement = $(this);
        const related = getRelatedObjects(
            $(`.todo-item[data-item-id="${
                thisElement.data("id")
            }"]`)
        );

        console.log(`Deleting: ${related.id}`);
        thisElement.prop("disabled", true);

        let userData = {
            Label: thisElement.val()
        };

        $.ajax({
            type: "PUT",
            url: `/todoitem/delete/${related.id}`,
            data: JSON.stringify(userData),
            contentType: "application/json; charset=utf-8",
            success: function (response) {
                // Handle the API response as needed
                console.log("Todo list item deleted successfully");
                if (related.type == "filtered") {
                    related.item.remove();
                    return;
                };
                $("#delete-modal-name").text("");
                $("#confirm-delete").val("");
                $("#confirm-delete").prop("disabled", false);
                $("#exampleModalCenter").modal("hide");
                related.item.remove();
            },
            error: function (error) {
                // Handle API error
                console.error("Error deleting todo list item:", error);
                related.alert.text(genericError);
            }
        });
    });
});
