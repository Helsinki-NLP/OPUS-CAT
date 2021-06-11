var myWindowId;
const contentBox = document.querySelector("#content");

let portFromCS;

function connected(p) {
  portFromCS = p;
  portFromCS.postMessage({greeting: "hi there content script!"});
  // console.log(m.greeting);
  portFromCS.onMessage.addListener(function(m) {
    var params = {tokenCode:0,input:m.greeting,srcLangCode:"en",trgLangCode:"fi",modelTag:""};
    let url = new URL('http://localhost:8500/MTRestService/TranslateJson');
    url.search = new URLSearchParams(params).toString();
    fetch(url).then(response => response.json()).then(data => contentBox.innerHTML = data["translation"]);
    //portFromCS.postMessage({greeting: "In background script, received message from content script:" + m.greeting});
  });
}

browser.runtime.onConnect.addListener(connected);

/*
Make the content box editable as soon as the user mouses over the sidebar.
*/
/*
window.addEventListener("mouseover", () => {
  contentBox.setAttribute("contenteditable", true);
});

/*
When the user mouses out, save the current contents of the box.
*/
/*
window.addEventListener("mouseout", () => {
  contentBox.setAttribute("contenteditable", false);
  browser.tabs.query({windowId: myWindowId, active: true}).then((tabs) => {
    let contentToStore = {};
    contentToStore[tabs[0].url] = contentBox.textContent;
    browser.storage.local.set(contentToStore);
  });
});

/*
Update the sidebar's content.

1) Get the active tab in this sidebar's window.
2) Get its stored content.
3) Put it in the content box.
*/
/*
function updateContent() {
  browser.tabs.query({windowId: myWindowId, active: true})
    .then((tabs) => {
      return browser.storage.local.get(tabs[0].url);
    })
    .then((storedInfo) => {
      contentBox.textContent = storedInfo[Object.keys(storedInfo)[0]];
    });
}

/*
Update content when a new tab becomes active.
*/
//browser.tabs.onActivated.addListener(updateContent);

/*
Update content when a new page is loaded into a tab.
*/
//browser.tabs.onUpdated.addListener(updateContent);

/*
When the sidebar loads, get the ID of its window,
and update its content.
*/
/*browser.windows.getCurrent({populate: true}).then((windowInfo) => {
  myWindowId = windowInfo.id;
  updateContent();
});
*/