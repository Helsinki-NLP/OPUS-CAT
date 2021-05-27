---
layout: page
title: Installation
permalink: /install
---
## Quickstart for translators

1. Download the [OPUS-CAT MT Engine](https://github.com/Helsinki-NLP/OPUS-CAT/releases/download/engine_v1.0.0.4/OpusCatMTEngine_v1.0.0.4.zip) and install it to your computer by extracting it in a folder on your computer. **IMPORTANT**: OPUS-CAT MT Engine generates the machine translation, and all OPUS-CAT plugins require that the OPUS-CAT MT Engine is installed on the same computer and running when the plugins are used. 
2. Download the plugin for your translation software and install it:
  - Trados Studio 2017, 2019 and 2021: Download the plugin from the [SDL AppStore](https://appstore.sdl.com/language/app/opus-cat-nmt/1160/) and double-click .sdlplugin file to install.
  - [memoQ 9.2 or older](https://github.com/Helsinki-NLP/OPUS-CAT/raw/develop/build/2020-05-26/FiskmoMTPlugin.dll), [memoQ 9.3 to 9.6](https://github.com/Helsinki-NLP/OPUS-CAT/raw/develop/build/2020-10-07/FiskmoMTPlugin.dll), [memoQ 9.7 or newer](https://github.com/Helsinki-NLP/OPUS-CAT/raw/develop/build/2021-05-27/FiskmoMTPlugin.dll): Copy the plugin file to the Addins subfolder in the memoQ installation folder. **IMPORTANT**: Make sure to unblock the dll file before copying it (right-click file, choose **Properties**, check the **Unblock** box in the bottom right of the **Properties** window).
  - Wordfast: There is currently no plugin for Wordfast, but Fiskmo MT can be used in Wordfast as a [custom MT provider](./wordfast).
  - OmegaT: There is an OmegaT plugin available for the development version of the MT engine. The plugin will be released shortly, but if you want to use the OmegaT plugin before that, contact us.
3. Start the OPUS-CAT MT Engine application by clicking OpusCatMTEngine.exe in the extraction folder (note that the file extension .exe may be hidden in Windows Explorer, in that case the file is shown as OpusCatMTEngine).
4. Install models from the OPUS model repository for the language pairs that you require.
   - Click **Install OPUS model from Web**.
       <img src="./images/InstallOnlineModel.PNG?raw=true" alt="drawing" width="75%"/>
   - Enter source and target language codes in the filtering boxes on the top row, select a model (usually the one with latest date suffix) and then click **Install locally**.
       <img src="./images/ModelList.PNG?raw=true" alt="drawing" width="75%"/>
5. After the model has been downloaded and installed, test that it works.
    - Select model and click **Translate with model**.
       <img src="./images/TestModel.PNG?raw=true" alt="drawing" width="75%"/>
    - Enter translation in the **Source text** area, click **Translate** and wait for a translation to appear in the **Translation** area (producing the first translation may take some time, as the model needs to be initialized, subsequent translations are faster).
       <img src="./images/TestTranslation.PNG?raw=true" alt="drawing" width="75%"/>
6. Start your CAT tool, and add the OPUS-CAT plugin to a translation project.
