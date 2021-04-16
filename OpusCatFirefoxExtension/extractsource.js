/*
Listen for changes in focus.
*/
/*
document.addEventListener("focusin",notifyEngine,false,true);
console.log("test3");

var focused;
function notifyEngine(e) {
    console.log("test2");
	focused = e.target;	
	browser.runtime.sendMessage({greeting: e});
}

browser.runtime.onMessage.addListener(insert);

function insert(message) {
  console.log("test1");
  focused.append(message.newtext);
}*/

let myPort = browser.runtime.connect({name:"port-from-cs"});

myPort.onMessage.addListener(function(m) {
  console.log("In content script, received message from background script: ");
  console.log(m.greeting);
});


document.addEventListener("selectionchange",notifyEngine);
function notifyEngine(e) {
    const activeSegmentPair = document.getElementsByClassName("twe_active")[0];
    const sourceSegment = activeSegmentPair.getElementsByClassName("twe_source")[0].getElementsByClassName("te_txt")[0];
	myPort.postMessage({greeting: sourceSegment.textContent});
}