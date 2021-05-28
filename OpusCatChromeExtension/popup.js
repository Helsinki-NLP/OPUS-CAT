const defaultUrl = "http://localhost:8500/MTRestService/TranslateJson";

document.getElementById("saveSettings").addEventListener("click", function() {
    var value = document.getElementById("opusCatUrl").value;
    chrome.storage.local.set({opusCatUrlSetting: value}, function() {
});}
);

document.getElementById("restoreDefaults").addEventListener("click", function() {
    document.getElementById("opusCatUrl").value = defaultUrl;
    chrome.storage.local.set({opusCatUrlSetting: defaultUrl}, function() {
});}
);

document.addEventListener("DOMContentLoaded", function(event) {
    chrome.storage.local.get(['opusCatUrlSetting'], function(result) {
        if (result.opusCatUrlSetting) {
            document.getElementById("opusCatUrl").value = result.opusCatUrlSetting;
        }
        else
        {
            document.getElementById("opusCatUrl").value = defaultUrl;
        }
    });
});