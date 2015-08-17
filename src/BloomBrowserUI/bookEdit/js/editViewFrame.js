// "region" ReaderSetup dialog
function CreateConfigDiv(title) {

    var dialogContents = $('<div id="synphonyConfig" title="' + title + '"/>').appendTo($("body"));

    var html = '<iframe id="settings_frame" src="/bloom/bookEdit/readerSetup/ReaderSetup.htm" scrolling="no" ' +
        'style="width: 100%; height: 100%; border-width: 0; margin: 0" ' +
        'onload="initializeReaderSetupDialog();"></iframe>';

    dialogContents.append(html);

    return dialogContents;
}

function showSetupDialog(showWhat) {

    var accordion = document.getElementById('accordion').contentWindow;
    accordion.localizationManager.loadStrings(getSettingsDialogLocalizedStrings(), null, function() {

        var title;
        if (showWhat == 'stages')
            title = accordion.localizationManager.getText('ReaderSetup.SetUpDecodableReaderTool', 'Set up Decodable Reader Tool');
        else
            title = accordion.localizationManager.getText('ReaderSetup.SetUpLeveledReaderTool', 'Set up Leveled Reader Tool');

        var dialogContents = CreateConfigDiv(title);



        accordion.model.setupType = showWhat;

        $(dialogContents).dialog({
            autoOpen: "true",
            modal: "true",
            buttons: {
                Help: {
                    // For consistency, I would have made this 'Common.Help', but we already had 'HelpMenu.Help Menu' translated
                    text: accordion.localizationManager.getText('HelpMenu.Help Menu', 'Help'),
                    class: 'left-button',
                    click: function() {
                        document.getElementById('settings_frame').contentWindow.postMessage('Help', '*');
                    }
                },
                OK: {
                    text: accordion.localizationManager.getText('Common.OK', 'OK'),
                    click: function () {
                        document.getElementById('settings_frame').contentWindow.postMessage('OK', '*');
                    }
                },

                Cancel: {
                    text: accordion.localizationManager.getText('Common.Cancel', 'Cancel'),
                    click: function () {
                        $(this).dialog("close");
                    }
                }
            },
            close: function() {
                $(this).remove();
               fireCSharpEvent('setModalStateEvent', 'false');
            },
            open: function () {
                $('#synphonyConfig').css('overflow', 'hidden');
                $('button span:contains("Help")').prepend('<i class="fa fa-question-circle"></i> ');
            }
//            height: 0,
//            width: 700
        });

        fireCSharpEvent('setModalStateEvent', 'true');
    });
}


function getSettingsDialogLocalizedStrings() {
    // Without preloading these, they are not available when the dialog is created
    var pairs = {};
    pairs['ReaderSetup.SetUpDecodableReaderTool'] = 'Set up Decodable Reader Tool';
    pairs['ReaderSetup.SetUpLeveledReaderTool'] = 'Set up Leveled Reader Tool';
    pairs['HelpMenu.Help Menu'] = 'Help';
    pairs['Common.OK'] = 'OK';
    pairs['Common.Cancel'] = 'Cancel';
    return pairs;
}

//noinspection JSUnusedGlobalSymbols
/**
 * Used by the settings_frame to initialize the setup dialog
 */
function initializeReaderSetupDialog() {

    var model = document.getElementById('accordion').contentWindow.model;

    var sourceMsg = 'Data\n' +  JSON.stringify(model.getSynphony().source);
    var fontMsg = 'Font\n' +  model.fontName;
    document.getElementById('settings_frame').contentWindow.postMessage(sourceMsg, '*');
    document.getElementById('settings_frame').contentWindow.postMessage(fontMsg, '*');
}

/**
 * Called by C# after the setup data has been saved, following Save click.
 */
function closeSetupDialog() {
    $('#synphonyConfig').dialog("close");
}
// "endregion" ReaderSetup dialog

// "region" Add Page dialog
function CreateAddPageDiv(templatesJSON, descriptionLabel, blankPreviewMsg) {

    var dialogContents = $('<div id="addPageConfig"/>').appendTo($('body'));

    var html = "<iframe id=\"addPage_frame\" src=\"/bloom/pageChooser/page-chooser-main.htm\" scrolling=\"no\" style=\"width: 100%; height: 100%; border: none; margin: 0\"></iframe>";

    dialogContents.append(html);

    // When the page chooser loads, get the iframe holding it to resize to what's inside
    $('#addPage_frame').load(function() {
        localizeDialogContents(dialogContents, descriptionLabel, blankPreviewMsg);
        initializeAddPageDialog(templatesJSON);
    });

    return dialogContents;
}

//noinspection JSUnusedGlobalSymbols
// method called from EditingModel.cs
// for 'templatesJSON', see property EditingModel.GetJsonTemplatePageObject
function showAddPageDialog(templatesJSON) {

    var theDialog;
    var parentElement = document.getElementById('page').contentWindow;

    parentElement.localizationManager.loadStrings(getAddPageDialogLocalizedStrings(), null, function() {

        var title = parentElement.localizationManager.getText('AddPageDialog.Title', 'Add Page...');
        var addButtonText = parentElement.localizationManager.getText('AddPageDialog.AddPageButton', 'Add This Page');
        var descriptionLabel = parentElement.localizationManager.getText('AddPageDialog.DescriptionLabel', 'Description');
        var blankPreviewMsg = parentElement.localizationManager.getText('AddPageDialog.PreviewMessage',
            'This will contain a preview of a template page when one is selected.');
        var dialogContents = CreateAddPageDiv(templatesJSON, descriptionLabel, blankPreviewMsg);

        theDialog = $(dialogContents).dialog({
            class: "addPageDialog",
            autoOpen: false,
            resizable: false,
            modal: true,
            width: 795,
            height: 550,
            position: {
                my: "left bottom", at: "left bottom", of: window
            },
            title: title,
            /*buttons: {
                OK: {
                    class: 'defaultButton',
                    text: addButtonText,
//                    we could reinstate this if we had the right color
//                    icons: {
//                        primary: "ui-icon-plusthick"
//                    },
                    click: function () {
                        fireCSharpEvent('setModalStateEvent', 'false');
                        document.getElementById('addPage_frame').contentWindow.postMessage('AddSelectedPage', '*');
                    }
                },
                Cancel: {
                    class: 'normalButton',
                    text: parentElement.localizationManager.getText('Common.Cancel', 'Cancel'),
                    click: function () {
                        $(this).dialog("close");
                    }
                }
            },*/
            close: function() {
                $(this).remove();
                fireCSharpEvent('setModalStateEvent', 'false');
            },
            open: function () {
                //adjustAddPageButton(addButtonText);
                setTimeout(function() {
                    setDialogInnerIframeSize();

                    jQuery('#addPageButton').click(function () {
                            alert("addpageclick");
                            fireCSharpEvent('setModalStateEvent', 'false');
                            document.getElementById('addPage_frame').contentWindow.postMessage('AddSelectedPage', '*');
                            return false;
                    });

          //          jQuery.notify("testing notify");
                }, 200);
            }
        });

        //TODO:  this doesn't work yet. We need to make it work, and then make it localizationManager.asyncGetText(...).done(translation => { do the insertion into the dialog });
        // theDialog.find('.ui-dialog-buttonpane').prepend("<div id='hint'>You can press ctrl+N to add the same page again, without opening this dialog.</div>");
     
        jQuery(document).on('click', 'body > .ui-widget-overlay', function () {
            $(".ui-dialog-titlebar-close").trigger('click');
            return false;
        });
        fireCSharpEvent('setModalStateEvent', 'true');
        theDialog.dialog('open');

        //parentElement.$.notify("testing notify",{});
    });
}

//function adjustAddPageButton(addButtonText) {
//    var iconAdjustment = 15;
//    var button = $('.ui-dialog-buttonpane').find('button:contains('+addButtonText+')');
//    button.width(button.width() + iconAdjustment); // make room for the 'plus' icon on the button
//}

function setDialogInnerIframeSize() {
    var $frame = $('#addPage_frame');
    var width = $frame.contents().find('#mainContainer').width();
    var height = $frame.contents().find('#mainContainer').height();
    $frame.height(height);
    $frame.width(width);
}

function localizeDialogContents(dialogContents, description, blankMessage) {
    $(dialogContents).find('iframe').contents().find('.DescriptionHeader').text(description);
    $(dialogContents).find('iframe').contents().find('iframe').contents().find('#innerBox').text(blankMessage);
}

function getAddPageDialogLocalizedStrings() {
    // Without preloading these, they are not available when the dialog is created
    var pairs = {};
    pairs['AddPageDialog.Title'] = 'Add Page...';
    pairs['AddPageDialog.DescriptionLabel'] = 'Description';
    pairs['AddPageDialog.PreviewMessage'] = 'This will contain a preview of a template page when one is selected.';
    //pairs['HelpMenu.Help Menu'] = 'Help';
    pairs['AddPageDialog.AddPageButton'] = 'Add This Page';
    pairs['Common.Cancel'] = 'Cancel';
    return pairs;
}

//noinspection JSUnusedGlobalSymbols
// Used by the addPage_frame to initialize the setup dialog with the available template pages
// 'templatesJSON' will be something like:
//([{ "templateBookFolderUrl": "/bloom/localhost//...(path to files).../factoryCollections/Templates/Basic Book/", 
//      "templateBookUrl": "/bloom/localhost/...(path to files).../factoryCollections/Templates/Basic Book/Basic Book.htm" }])
// See property EditingModel.GetJsonTemplatePageObject
function initializeAddPageDialog(templatesJSON) {
    var templateMsg = 'Data\n' + JSON.stringify(templatesJSON);
    document.getElementById('addPage_frame').contentWindow.postMessage(templateMsg, '*');
}
// "endregion" Add Page dialog

/**
 * Fires an event for C# to handle
 * @param {String} eventName
 * @param {String} eventData
 */
// Enhance: JT notes that this method pops up from time to time; can we consolidate?
function fireCSharpEvent(eventName, eventData) {

    var event = new MessageEvent(eventName, {'view' : window, 'bubbles' : true, 'cancelable' : true, 'data' : eventData});
    document.dispatchEvent(event);
    // For when we someday change this file to TypeScript... since the above ctor is not declared anywhere.
    // Solution III (works)
    //var event = new (<any>MessageEvent)(eventName, { 'view': window, 'bubbles': true, 'cancelable': true, 'data': eventData });
}