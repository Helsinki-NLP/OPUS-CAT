---
layout: page
title: Wordfast integration
permalink: /wordfast
---
## Using OPUS-CAT in Wordfast Classic and Wordfast Pro

OPUS-CAT can be used in different Wordfast CAT tools by using the **Custom MT** functionality offered by these products.
**NOTE:** Make sure you use latest version of OPUS-CAT MT Engine with Wordfast. Earlier versions of OPUS-CAT MT Engine required extra configuration to enable it to be used with Wordfast.

### Installing the OPUS-CAT MT Engine

1. Download the [OPUS-CAT MT Engine](https://github.com/Helsinki-NLP/OPUS-CAT/releases/download/engine_v1.2.0/OpusCatMtEngine_v1.2.0.zip) and install it to your computer by extracting it in a folder on your computer. **IMPORTANT**: OPUS-CAT MT Engine generates the machine translation, and all OPUS-CAT plugins require that the OPUS-CAT MT Engine is installed on the same computer and running when the plugins are used. 
2. Start the OPUS-CAT MT Engine application by clicking OpusCatMTEngine.exe in the extraction folder (note that the file extension .exe may be hidden in Windows Explorer, in that case the file is shown as OpusCatMTEngine).
3. Install models from the OPUS model repository for the language pairs that you require.
   - Click **Install OPUS model from Web**.
       <img src="./images/InstallOnlineModel.PNG?raw=true" alt="drawing" width="100%"/>
   - Next, OPUS-CAT MT Engine fetches a list of models available online. The text _Fetching list of online models, please wait..._ is displayed in the top part of the window.
       <img src="./images/FetchingModels.png?raw=true" alt="drawing" width="100%"/>
   - When the text in the top part of the window changes to _Downloadable online models_, enter source and target languages (or parts of them) in the filtering boxes on the top row. From the filtered models, select a model to install (it's usually best to select the one with latest date suffix) and then click **Install locally**.
       <img src="./images/ModelList.PNG?raw=true" alt="drawing" width="100%"/>
4. After the model has been downloaded and installed, test that it works.
    - Select model and click **Translate with model**.
       <img src="./images/TestModel.PNG?raw=true" alt="drawing" width="100%"/>
    - Enter translation in the **Source text** area, click **Translate** and wait for a translation to appear in the **Translation** area (producing the first translation may take some time, as the model needs to be initialized, subsequent translations are faster).
       <img src="./images/TestTranslation.PNG?raw=true" alt="drawing" width="100%"/>
 
### Using OPUS-CAT in Wordfast Pro

1. Make sure that OPUS-CAT MT Engine is running.
2. Click the **Preferences** icon in the Wordfast Pro UI: 
<img src="./images/WordfastProPrefs.png?raw=true" alt="drawing" width="75%"/>
3. Select **Machine translation** from the left and check the **Enable Custom MT** checkbox:
<img src="./images/WordfastProPrefsMt.png?raw=true" alt="drawing" width="75%"/>
4. Enter the URL for accessing the HTTP API of OPUS-CAT MT Engine into the **URL** field, and enter *translation* into the **JSON Key** field:
<img src="./images/WordfastProPrefsMtFields.png?raw=true" alt="drawing" width="75%"/>

The value of the **URL** should have the following format:

```
http://localhost:8500/MTRestService/TranslatePost?tokenCode=0&input={ss}&srcLangCode=en&trgLangCode=fi&modelTag=
```
**NOTE:** The *srcLangCode* and *trgLangCode* values in the URL should be replaced with the two-letter codes of the source and target language of the project. So if the source language of the project is e.g. French and the target language English, the URL should be the following:

```
http://localhost:8500/MTRestService/TranslatePost?tokenCode=0&input={ss}&srcLangCode=fr&trgLangCode=en&modelTag=
```
Other versions of Wordfast support inserting the project's source and target language automatically into Custom MT URLs, but for some reason Wordfast Pro does not. Machine translations are generated only if a machine translation model has been installed for the specified language pair. You can check installed models from the **Models** tab of OPUS-CAT MT Engine.

When you have entered the values to the **URL** and **JSON Key** fields, close the **Preferences** window by clicking **OK*, and open a file in the editor. The OPUS-CAT machine translation should now be displayed when a segment is opened:

<img src="./images/WordfastProEditor.png?raw=true" alt="drawing" width="75%"/>

The appearance of the first machine translation may take some time, as the MT model has to be initialized. Later translations will be produced faster. Machine translations can also be pregenerated for all segments by selecting the **Use primary MT on no match segments** checkbox in the project creation wizard:

 <img src="./images/WordfastProPreMt.png?raw=true" alt="drawing" width="75%"/>

Make sure to set **Custom MT** as the primary MT in the preferences before using this option.

### Using OPUS-CAT in Wordfast Classic

**Note**: These instructions are for the current version of Wordfast Classic (8.85), courtesy of Jamie Lucero. Instructions for older versions of Wordfast Classic are available [here](./wordfast_classic_old).

1. Make sure that OPUS-CAT MT Engine is running.
2. Open Wordfast Classic user interface by pressing Ctlr+Alt+W in Word and select the **Machine translation** tab and the **Customize MT** subtab, and then select the **MT name** dropdown and choose **Custom (add)**: 
<img src="./images/WordfastClassicJL.png?raw=true" alt="drawing" width="75%"/>
3. Provide a preferred name, add the JSON key and URL (use the URL value below), and click **Remote MT** tab (or another tab) to initiate the save changes dialog:
```
http://localhost:8500/MTRestService/TranslateJson?tokenCode=&input={ss}&srcLangCode={sl}&trgLangCode={tl}
```
<img src="./images/WordfastClassicJL2.png?raw=true" alt="drawing" width="75%"/>

4. After saving, select the new connection from the **MT provider** dropdown:
<img src="./images/WordfastClassicJL3.png?raw=true" alt="drawing" width="75%"/>
5. You can now test OPUS-CAT by clicking the **Test** button. If the test produced a translation, close the Wordfast Classic user interface and open some source file. When you now open a segment, Wordfast Classic should display a machine translation from OPUS-CAT (unless a TM match is available):
 <img src="./images/WordfastClassicMtSegment.png?raw=true" alt="drawing" width="75%"/>

### Using OPUS-CAT with Wordfast Server

These instructions are also courtesy of Jamie Lucero. With Wordfast Server and Opus-CAT running on the same machine, Opus-CAT can be used from outside the LAN with a connection to Wordfast Server.

1. Go to **Setup** and click **MT Engines** to add the **MT Name** and configure the rest:
```
URL: http://localhost:8500/MTRestService/TranslateJson
Request data: tokenCode=&input={ss}&srcLangCode={sl}&trgLangCode={tl}&modelTag=
```
<img src="./images/WordfastServerJL1.png?raw=true" alt="drawing" width="75%"/>

2. Under **Accounts**, set up a new account or choose an existing one, then click the **MT engine** button to open the dialog and choose the desired engine:
<img src="./images/WordfastServerJL2.png?raw=true" alt="drawing" width="75%"/>
