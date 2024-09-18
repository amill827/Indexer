$(document).ready(function () {
    //Keyboard shortcuts
    
    //Copies the Application Date to the other date fields, requested by users
    Mousetrap.bind("alt+p", function (e) {
        var applDate = $('#ApplDate').val();
        $('#MALDate').val(applDate);
        $('#MARDate').val(applDate);

    }, 'keyup');

    Mousetrap.bind("ctrl+right", function (e) {
        GoToNextTab();
    }, 'keyup');

    Mousetrap.bind("ctrl+left", function (e) {
        GoToPreviousTab();
    }, 'keyup');


    function getActiveTabIndex() {
        return $('#tab-container .active').data('index');
    }

    function GoToTab(index) {
        var tab = $('button#tab-' + index)
        if (tab !== null) {
            tab.click();
        }
    }

    function GoToNextTab() {
        GoToTab(getActiveTabIndex() + 1)
    }
    function GoToPreviousTab() {
        GoToTab(getActiveTabIndex() - 1)
    }
});