﻿import axios from "axios";
import { ITabModel, ToolBox } from "../toolbox";

$(document).ready(() => {
    // request our model and set the controls
    axios.get("/bloom/api/book/settings").then(result => {
        var settings = result.data;

        // Only show this if we are editing a shell book. Otherwise, it's already not locked.
        if (!settings.isRecordedAsLockedDown) {
            $(".showOnlyWhenBookWouldNormallyBeLocked").css("display", "none");
            $("input[name='isTemplateBook']").prop("checked", settings.isTemplateBook);
        }
        else {
            $(".showOnlyIfBookIsNeverLocked").css("display", "none");
            // enhance: this is just dirt-poor binding of 1 checkbox for now
            $("input[name='unlockShellBook']").prop("checked", settings.unlockShellBook);
        }
    });
});

export function handleBookSettingCheckboxClick(clickedButton: any) {
    // read our controls and send the model back to c#
    // enhance: this is just dirt-poor serialization of checkboxes for now
    var inputs = $("#bookSettings :input");
    var o = {};
    var settings = $.map(inputs, (input, i) => {
        o[input.name] = $(input).prop("checked");
        return o;
    })[0];
    axios.post("/bloom/api/book/settings", settings);
}

// We need a minimal model to get ourselves loaded
class BookSettings implements ITabModel {
    beginRestoreSettings(settings: string): JQueryPromise<void> {
        // Nothing to do, so return an already-resolved promise.
        var result = $.Deferred<void>();
        result.resolve();
        return result;
    }
    configureElements(container: HTMLElement) {
    }
    showTool() {
    }
    hideTool() {
    }
    updateMarkup() {
    }
    name(): string {
        return "bookSettings";
    }
    hasRestoredSettings: boolean;
    isAlwaysEnabled(): boolean {
        return true;
    }
    finishTabPaneLocalization(pane: HTMLElement) {
    }
}

// Make the one instance of this class and register it with the master toolbox.
ToolBox.getTabModels().push(new BookSettings());
