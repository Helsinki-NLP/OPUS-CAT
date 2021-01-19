## Quickstart for translators

1. Download the [OPUS-CAT MT Engine](https://github.com/Helsinki-NLP/OPUS-CAT/releases/download/Trados-plugin-v1.0a/OpusCatMTEngine_v1.0.0.0.zip) and install it to your computer by extracting it in a folder on your computer. **IMPORTANT**: OPUS-CAT MT Engine generates the machine translation, and all OPUS-CAT plugins require that the OPUS-CAT MT Engine is installed on the same computer and running when the plugins are used. 
2. Download the plugin for your translation software and install it:
  - [Trados 2017](https://github.com/Helsinki-NLP/OPUS-CAT/releases/download/Trados-plugin-v1.0/OpusCatTrados2017Plugin_v1.0.0.0.zip): extract the zip file and double-click .sdlplugin file to install.
  - [Trados 2019](https://github.com/Helsinki-NLP/OPUS-CAT/releases/download/Trados-plugin-v1.0/OpusCatTrados2019Plugin_v1.0.0.0.zip): extract the zip file and double-click .sdlplugin file to install.
  - [Trados 2021](https://github.com/Helsinki-NLP/OPUS-CAT/releases/download/Trados-plugin-v1.0/OpusCatTrados2021Plugin_v1.0.0.0.zip): extract the zip file and double-click .sdlplugin file to install.
3. Start the OPUS-CAT MT Engine application by clicking OpusCatMTEngine.exe in the extraction folder.
4. Install models from the OPUS model repository for the language pairs that you require.
   - Click **Install OPUS model from Web**.
       - <img src="./images/InstallOnlineModel.PNG?raw=true" alt="drawing" width="75%"/>
   - Enter source and target language codes in the filtering boxes on the top row, select a model (usually the one with latest date suffix) and then click **Install locally**.
       - <img src="./images/ModelList.PNG?raw=true" alt="drawing" width="75%"/>
5. After the model has been downloaded and installed, test that it works.
    - Select model and click **Translate with model**.
       - <img src="./images/TestModel.PNG?raw=true" alt="drawing" width="75%"/>
    - Enter translation in the **Source text** area, click **Translate** and wait for a translation to appear in the **Translation** area (producing the first translation may take some time, as the model needs to be initialized, subsequent translations are faster).
       - <img src="./images/TestTranslation.PNG?raw=true" alt="drawing" width="75%"/>
6. Start your CAT tool, and add the OPUS-CAT plugin to a translation project.
