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
    var locationUrl = new URL(location);
    
    if (locationUrl.hostname.endsWith("memsource.com")) {
        var activesegs = document.getElementsByClassName("twe_active");
        if (activesegs.length > 0) {
            const activeSegmentPair = document.getElementsByClassName("twe_active")[0];
            const sourceSegment = activeSegmentPair.getElementsByClassName("twe_source")[0].getElementsByClassName("te_txt")[0];
            chrome.runtime.sendMessage({opusCatSourceText: sourceSegment.textContent});
        }
    }
    if (locationUrl.hostname.endsWith("xtm.cloud") || locationUrl.hostname.endsWith("xtm-intl.com")) {
        var activesegs = document.getElementsByClassName("trans-unit--active");
        if (activesegs.length > 0) {
            const activeSegmentPair = activesegs[0];
            const sourceSegment = activeSegmentPair.getElementsByClassName("source-cell")[0];
            chrome.runtime.sendMessage({opusCatSourceText: sourceSegment.textContent});
        }
    }
}