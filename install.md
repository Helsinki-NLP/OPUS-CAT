---
layout: page
title: Installation
permalink: /install
---
## Quickstart for translators

1. Download the [OPUS-CAT MT Engine](https://github.com/Helsinki-NLP/OPUS-CAT/releases/download/engine_v1.2.0/OpusCatMtEngine_v1.2.0.zip). Create an empty folder on your computer (for instance in your Documents folder), and extract the contents of the zip file there. OPUS-CAT MT Engine does not require separate installation, it will run directly from the folder in which it has been extracted. **IMPORTANT**: OPUS-CAT MT Engine generates the machine translation, and all OPUS-CAT plugins require that the OPUS-CAT MT Engine is running on the same computer when the plugins are used. 
2. Download the plugin for your translation software and install it:
  - Trados Studio 2017, 2019, 2021, 2022: Download the plugin from the [RWS AppStore](https://appstore.rws.com/language/app/opus-cat-nmt/1160/) and double-click .sdlplugin file to install.
  - MemoQ:
      * [Latest version](https://github.com/Helsinki-NLP/OPUS-CAT/releases/tag/memoq_v1.1). Tested to work on 9.7.28 and newer. Works with the cross-platform version of the OPUS-CAT MT Engine.
      * Older versions: [memoQ 9.2 or older](https://github.com/Helsinki-NLP/OPUS-CAT/raw/develop/build/2020-05-26/FiskmoMTPlugin.dll), [memoQ 9.3 or newer](https://github.com/Helsinki-NLP/OPUS-CAT/raw/develop/build/2020-10-07/FiskmoMTPlugin.dll) (NOTE: memoQ 9.7.10 has a bug which prevents the normal plugin from working, if you must use 9.7.10, you can use the [plugin for memoQ 9.7.10](https://github.com/Helsinki-NLP/OPUS-CAT/raw/develop/build/2021-05-27/FiskmoMTPlugin.dll))
      * Copy the plugin file to the _Addins_ subfolder in the memoQ installation folder. **IMPORTANT**: Make sure to unblock the dll file before copying it (right-click file, choose **Properties**, check the **Unblock** box in the bottom right of the **Properties** window). A pop-up window will be displayed when memoQ is started, you can remove the pop-up by following [these instructions](./memoq).
  - Wordfast: There is currently no plugin for Wordfast, but Fiskmo MT can be used in Wordfast as a [custom MT provider](./wordfast).
  - OmegaT: Copy the plugin file [omegat-plugin-opus-mt-1.0.0.jar](https://github.com/Helsinki-NLP/OPUS-CAT/raw/master/OmegaTPlugin/omegat-plugin-opus-mt-1.0.0.jar) to the *plugins* subfolder of the OmegaT installation folder (usually _C:\Program Files (x86)\OmegaT_).
  - Memsource and XTM: OPUS-CAT can be used with some browser-based CAT tools by [installing a browser extension](./browserextensions).
3. Start the OPUS-CAT MT Engine application by clicking OpusCatMTEngine.exe in the extraction folder (note that the file extension .exe may be hidden in Windows Explorer, in that case the file is shown as OpusCatMTEngine).
4. Install models from the OPUS model repository for the language pairs that you require.
   - Click **Install OPUS model from Web**.
       <img src="./images/InstallOnlineModel.PNG?raw=true" alt="drawing" width="100%"/>
   - Next, OPUS-CAT MT Engine fetches a list of models available online. The text _Fetching list of online models, please wait..._ is displayed in the top part of the window.
       <img src="./images/FetchingModels.png?raw=true" alt="drawing" width="100%"/>
   - When the text in the top part of the window changes to _Downloadable online models_, enter source and target languages (or parts of them) in the filtering boxes on the top row. From the filtered models, select a model to install (it's usually best to select the one with latest date suffix) and then click **Install locally**.
       <img src="./images/ModelList.PNG?raw=true" alt="drawing" width="100%"/>
5. After the model has been downloaded and installed, test that it works.
    - Select model and click **Translate with model**.
       <img src="./images/TestModel.PNG?raw=true" alt="drawing" width="100%"/>
    - Enter translation in the **Source text** area, click **Translate** and wait for a translation to appear in the **Translation** area (producing the first translation may take some time, as the model needs to be initialized, subsequent translations are faster).
       <img src="./images/TestTranslation.PNG?raw=true" alt="drawing" width="100%"/>
6. Start your CAT tool, and add the OPUS-CAT plugin to a translation project.
