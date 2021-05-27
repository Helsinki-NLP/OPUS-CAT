let currentTranslation;
let contentScriptTab;
/*let portFromCS;


function connected(port) {
  if (port.name === "opuscat-port")
  {
      console.log("connected background");
      portFromCS = port;
      //portFromCS.postMessage({greeting: "hi there content script!"});
      portFromCS.onMessage.addListener(function(m) {
        if ("sourceText" in m) {
            var params = {tokenCode:0,input:m.sourceText,srcLangCode:"en",trgLangCode:"fi",modelTag:""};
            let url = new URL('http://localhost:8500/MTRestService/TranslateJson');
            url.search = new URLSearchParams(params).toString();
            fetch(url).then(response => response.json()).then(data => currentTranslation = data["translation"]);
           
            //portFromCS.postMessage({greeting: "In background script, received message from content script:" + m.greeting});
        }
      });
  }
  else
  {
      console.log("some other connection");
  }
}
*/
chrome.runtime.onMessage.addListener(
  function(request, sender, sendResponse) {
    if ("opusCatSourceText" in request) {
        contentScriptTab = sender.tab;
        var params = {tokenCode:0,input:request.opusCatSourceText,srcLangCode:"en",trgLangCode:"fi",modelTag:""};
        let url = new URL('http://localhost:8500/MTRestService/TranslateJson');
        url.search = new URLSearchParams(params).toString();
        fetch(url).then(response => response.json()).then(data => currentTranslation = data["translation"]);
    }
  }
);


chrome.commands.onCommand.addListener(function (command)
{
    console.log("commands");
    if (command == "copy-opuscat-mt")
    {
        if (currentTranslation && contentScriptTab) {
             chrome.tabs.sendMessage(contentScriptTab.id, {opusCatTranslationToPaste: currentTranslation});
        }
    };
});

/*
chrome.runtime.onConnect.addListener(connected);
*/