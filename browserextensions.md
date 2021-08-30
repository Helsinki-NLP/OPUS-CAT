---
layout: page
title: Browser extensions
permalink: /browserextensions
---

OPUS-CAT be used in certain browser-based CAT tools by installing a browser extension. Currently a browser extension is available only for the **Chrome** browser (a Firefox extension may become available later). The extension can be used in **Memsource** and **XTM** CAT tools. 

**Note:** OPUS-CAT's browser extensions assume that the supported browsers and browser-based CAT tools work in a specific way. Significant version updates made to the browsers and browser-based CAT tools may cause the extensions to stop functioning. If the extensions stop functioning after a version update, please post a report to the [OPUS-CAT issue section](https://github.com/Helsinki-NLP/OPUS-CAT/issues).

## Installing the Chrome extension
1. Download the [zipped Chrome extension file](https://github.com/Helsinki-NLP/OPUS-CAT/releases/download/chrome_extension_v1.0.0/OpusCatChromeExtension_v1.0.0.zip).
2. Extract the zip file to its own folder.
3. In the Chrome browser, enter *chrome://extensions/* in the address bar and press Enter to open Chrome's **Extensions** page.
4. On Chrome's **Extensions** page, click the **Load unpacked** button.
5. Browse to the extraction folder from step 2 and click **Select folder**.
6. Verify that **OPUS-CAT extension** has been added to Chrome's **Extensions** page.

## Using the Chrome extension
Once installed, the OPUS-CAT extension for Chrome is displayed in the extension list of Chrome, but the extension is only active on those web pages in which it can be used. Currently this includes the editor pages of Memsource and XTM.

Using the extension requires that OPUS-CAT MT Engine is installed and running on the same machine as the browser ([OPUS-CAT installation instructions](https://helsinki-nlp.github.io/OPUS-CAT/install)). The extension extracts source text from the currently active editor row and sends it to the MT engine for translation. The translation is displayed by using the overlay functionality of OPUS-CAT MT Engine. The overlay is disabled by default, so it needs to be enabled in the **Settings** tab of the MT engine:

<img src="./images/EnableOverlay.png?raw=true" alt="drawing" width="75%"/>

The translation displayed in the overlay can be copied into the browser by using the shortcut **Ctrl+Shift+K** (this shortcut can be changed by navigating to *chrome://extensions//shortcuts* in Chrome). The overlay may be pinned as the topmost window in Windows by checking the **Show overlay on top** checkbox:

<img src="./images/OverlayOnTop.png?raw=true" alt="drawing" width="75%"/>

**IMPORTANT:** the browser extension requires that one of the MT models installed in the OPUS-CAT MT Engine **must** be set as an override model. This model is used to produce the translations that are displayed in the overlay, so if there is no override model, no translations are produced. You can set a model as an override model by selecting it and clicking **Set as override model**.

<img src="./images/OverrideModel.png?raw=true" alt="drawing" width="75%"/>

When an override model has been selected, it is displayed on the top row of the model list with a thin red border:

<img src="./images/OverrideModel2.png?raw=true" alt="drawing" width="75%"/>

## Chrome extension settings
The OPUS-CAT Chrome extension has a setting window, where the address of the MT engine can be specified. The window can be opened by selecting OPUS-CAT from Chrome's extension list:

<img src="./images/ChromeSettings.png?raw=true" alt="drawing" width="75%"/>

The default address is the same as OPUS-CAT MT Engine's default address, so the setting does not need to be changed unless OPUS-CAT MT Engine's HTTP port has been changed.
