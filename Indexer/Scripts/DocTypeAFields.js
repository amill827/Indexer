$(document).ready(function () {
    //Disable button when submitting form
    $(document).on('submit', '#work-item-form', function () {
        var button = $(this).find(':submit');
        setTimeout(function () {
            button.attr('disabled', 'disabled');
        }, 0);
    });

    //Enable button if form was invalid
    $(document).on('invalid-form.validate', '#work-item-form', function () {
        var button = $(this).find(':submit');
        setTimeout(function () {
            button.removeAttr('disabled');
        }, 1);
    });

    ToggleNotes();
    
    // Show/Hide notes when checkbox clicked
    $("#NotWorkable").click(function () {
        if ($(this).is(":checked")) {
            $("#notes-section").show(300);
            $("#Names_0__NotWorkable").val(true);//manually mark each name not workable so client validation updates properly ()
            $("#Names_1__NotWorkable").val(true);
            $("#Names_2__NotWorkable").val(true);
            $("#instrument-info").hide(100);
            $("#nameTypes").hide(100);
        }
        else {
            $("#notes-section").hide(200);
            $("#Names_0__NotWorkable").val(false);
            $("#Names_1__NotWorkable").val(false);
            $("#Names_2__NotWorkable").val(false);
            $("#instrument-info").show(100);
            $("#nameTypes").show(100);
        }
    });

    //hide the validation summary when there are no model level errors to show (workaround for a bug with mvc validation summary)
    //if ($(".validation-summary-errors li:visible").length === 0) {
    //    $(".validation-summary-errors").hide();
    //}

    function ToggleNotes() {
        //Show or Hide notes and other sections based on Not Workable checkbox
        if ($("#NotWorkable").is(":checked")) {
            $("#notes-section").show();
            $("#instrument-info").hide();
            $("#nameTypes").hide();
        }
        else {
            $("#notes-section").hide();
            $("#instrument-info").show();
            $("#nameTypes").show();
        }
    }
});