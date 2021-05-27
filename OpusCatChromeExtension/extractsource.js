/*let myPort = chrome.runtime.connect({name:"opuscat-port"});

myPort.onMessage.addListener(function(m) {
  //console.log("received message in content script");
  if ("translation" in m) {
    var blob = new Blob([m.translation], {type: 'text/plain'});
    var item = new ClipboardItem({'text/plain': blob});
    navigator.clipboard.write([item]).then(() => document.execCommand('paste'));
  }
});



myPort.onDisconnect.addListener(function(port) {
    console.log("port disconnected");
    myPort = chrome.runtime.connect({name:"opuscat-port"});
});*/

chrome.runtime.onMessage.addListener(
  function(request, sender, sendResponse) {
    if ("opusCatTranslationToPaste" in request) {
        var blob = new Blob([request.opusCatTranslationToPaste], {type: 'text/plain'});
        var item = new ClipboardItem({'text/plain': blob});
        navigator.clipboard.write([item]).then(() => document.execCommand('paste'));
    }
  }
);

document.addEventListener("selectionchange",notifyEngine);
function notifyEngine(e) {
    
    console.log("extract source");
    const activeSegmentPair = document.getElementsByClassName("twe_active")[0];
    const sourceSegment = activeSegmentPair.getElementsByClassName("twe_source")[0].getElementsByClassName("te_txt")[0];
    chrome.runtime.sendMessage({opusCatSourceText: sourceSegment.textContent});
	//myPort.postMessage({sourceText: sourceSegment.textContent});
    /*var params = {tokenCode:0,input:sourceSegment.textContent,srcLangCode:"en",trgLangCode:"fi",modelTag:""};
    let url = new URL('http://localhost:8500/MTRestService/TranslateJson');
    url.search = new URLSearchParams(params).toString();
    var response = fetch(url);*/
    
}