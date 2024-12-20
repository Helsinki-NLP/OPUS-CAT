---
layout: page
title: Trados plugin
permalink: /tradosplugin
---
## Quickstart for translators

1. Download the [OPUS-CAT MT Engine](https://github.com/Helsinki-NLP/OPUS-CAT/releases/download/engine_v1.2.0/OpusCatMtEngine_v1.2.0.zip) and install it to your computer by extracting it in a folder on your computer. **IMPORTANT**: OPUS-CAT MT Engine generates the machine translation, and all OPUS-CAT plugins require that the OPUS-CAT MT Engine is installed on the same computer and running when the plugins are used. 
2. Download the plugin for your version of Trados Studio from the [RWS AppStore](https://appstore.rws.com/Plugin/11) and double-click .sdlplugin file to install.
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
6. Start your version of Trados Studio, and add the OPUS-CAT plugin to a translation project.
    - Select the project and click the **Project Settings** button in  the **Home** tab of the Studio ribbon.
      <img src="./images/ProjectSettings.png?raw=true" alt="drawing" width="75%"/>
    - The **Project Settings** window opens. Select **Language Pairs** and then select **All Language Pairs** (or a specific language pair, if the project has been set up to use different translation providers for different language pairs), and then select **Translation Memory and Automated Translation**. Click **Use** and select **OPUS-CAT** from the list.
      <img src="./images/TpSettings.png?raw=true" alt="drawing" width="75%"/>
7. Add OPUS-CAT to the project by clicking the **Save** button (you can also modify the default settings here if needed):
      <img src="./images/SaveSettings.png?raw=true" alt="drawing" width="75%"/>
8. **OPUS-CAT** should now appear in the list of translation providers:
      <img src="./images/OpusCatAsTranslationProvider.png?raw=true" alt="drawing" width="75%"/>
9. When you now open the project in Trados editor, OPUS-CAT MT suggestions should be displayed for new segments:
      <img src="./images/TradosSuggestion.png?raw=true" alt="drawing" width="75%"/>
