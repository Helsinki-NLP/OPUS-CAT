let currentTranslation;
let contentScriptTab;


chrome.runtime.onMessage.addListener(function(request, sender, sendResponse) {
    if ("opusCatSourceText" in request) {
        contentScriptTab = sender.tab;
        var params = {tokenCode:0,input:request.opusCatSourceText,srcLangCode:"en",trgLangCode:"fi",modelTag:""};
        
        chrome.storage.local.get(['opusCatUrlSetting'], function(result) {
            let url = new URL(result.opusCatUrlSetting);
            url.search = new URLSearchParams(params).toString();
            fetch(url).then(response => response.json()).then(data => currentTranslation = data["translation"]);
        });
    }
  }
);


chrome.commands.onCommand.addListener(function (command)
{
    if (command == "copy-opuscat-mt")
    {
        if (currentTranslation && contentScriptTab) {
             chrome.tabs.sendMessage(contentScriptTab.id, {opusCatTranslationToPaste: currentTranslation});
        }
    };
});
