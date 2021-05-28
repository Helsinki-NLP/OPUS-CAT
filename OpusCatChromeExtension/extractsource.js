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
    var activesegs = document.getElementsByClassName("twe_active");
    if (activesegs.length > 0) {
        const activeSegmentPair = document.getElementsByClassName("twe_active")[0];
        const sourceSegment = activeSegmentPair.getElementsByClassName("twe_source")[0].getElementsByClassName("te_txt")[0];
        chrome.runtime.sendMessage({opusCatSourceText: sourceSegment.textContent});
    }    
}