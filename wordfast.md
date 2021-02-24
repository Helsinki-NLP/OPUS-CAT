---
layout: page
title: Wordfast integration
permalink: /wordfast
---
## Using OPUS-CAT in Wordfast Classic and Wordfast Pro

OPUS-CAT can be used in different Wordfast CAT tools by using the **Custom MT** functionality offered by these products.
**NOTE:** Using OPUS-CAT with Wordfast requires some extra configuration compared to Trados and memoQ plugins. This is because the Wordfast integration requires access tothe HTTP API of OPUS-CAT MT Engine, which has to be separately configured. 

### Installing the OPUS-CAT MT Engine

Download the [OPUS-CAT MT Engine](https://github.com/Helsinki-NLP/OPUS-CAT/releases/download/engine_v1.0.0.5/OpusCatMTEngine_v1.0.0.5.zip) and install it to your computer by extracting it to a folder on your computer. **IMPORTANT**: OPUS-CAT MT Engine generates the machine translation, and all OPUS-CAT integrations require that the OPUS-CAT MT Engine is installed on the same computer and running when OPUS-CAT machine translation is used.

### Installing machine translation models

1. Start the OPUS-CAT MT Engine application by clicking *OpusCatMTEngine.exe* in the extraction folder.
2. Install models from the OPUS model repository for the language pairs that you require.
   - Click **Install OPUS model from Web**.
	<img src="./images/InstallOnlineModel.PNG?raw=true" alt="drawing" width="75%"/>
   - Enter source and target language codes in the filtering boxes on the top row, select a model (usually the one with latest date suffix) and then click **Install locally**.
       <img src="./images/ModelList.PNG?raw=true" alt="drawing" width="75%"/>
3. After the model has been downloaded and installed, test that it works.
    - Select model and click **Translate with model**.
       <img src="./images/TestModel.PNG?raw=true" alt="drawing" width="75%"/>
    - Enter translation in the **Source text** area, click **Translate** and wait for a translation to appear in the **Translation** area (producing the first translation may take some time, as the model needs to be initialized, subsequent translations are faster).
       <img src="./images/TestTranslation.PNG?raw=true" alt="drawing" width="75%"/>
 
### Configuring the HTTP API

Wordfast **Custom MT** fetches translations by using HTTP requests. OPUS-CAT MT Engine includes an HTTP API for this purpose, but it has to be granted a permission to receive requests before OPUS-CAT can be used in Wordfast. Granting the permission to receive requests requires system administrator permissions. If you do not have system administrator permissions on your computer, contact your IT support. 

To enable the HTTP API, first open the Windows command prompt as an administrator:

- Press Windows key, type *cmd*, right-click **Command prompt** app, and select **Run as administrator**.

Then use the **netsh** utility with the command prompt by typing the following command: 

```
netsh http add urlacl url=http://+:8500/ user=[USERNAME]
```

Substitute your username for *[USERNAME]* (you can find out your usename by typing *whoami* into the command prompt). 8500 is the port number which the HTTP API uses by default. This command specifies that port 8500 can be used by the named user, which makes it possible to use the HTTP API of OPUS-CAT MT Engine. 

To see whether the command was performed successfully, restart OPUS-CAT MT Engine, open your browser, and paste the following URL to the address bar:

```
http://localhost:8500/MTRestService/ListSupportedLanguagePairs?tokenCode=0 
```

You should see a list of language pairs for which models have been installed in OPUS-CAT MT Engine.

The *netsh* command only needs to be executed once, the permission to use the port remain unless it is deleted. If you want to delete the permission, type the following command into the command prompt:

```
*netsh http delete urlacl url=http://+:8500/*
```

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

1. Make sure that OPUS-CAT MT Engine is running.
2. Open Wordfast Classic user interface by pressing Ctlr+Alt+W in Word and select the **MT** tab: 
<img src="./images/WordfastClassicMt.png?raw=true" alt="drawing" width="75%"/>
3. Enter the following text into the empty field next to the topmost **No MT** entry in the **Web-based machine translation** section (also select the checkbox if it is not selected):
```
[id=FI:OPUS-CAT][url=http://localhost:8500/MTRestService/TranslateJson?tokenCode=&input={ss}&srcLangCode={sl}&trgLangCode={tl}][json=translation][type=A1]
```
<img src="./images/WordfastClassicMtDef.png?raw=true" alt="drawing" width="75%"/>
4. Click **Test**, and click **OK** in the testing window that opens:
<img src="./images/WordfastClassicMtTest.png?raw=true" alt="drawing" width="75%"/>
5. If the test produced a translation, close the Wordfast Classic user interface and open some source file. When you now open a segment, Wordfast Classic should display a machine translation from OPUS-CAT (unless a TM match is available):
 <img src="./images/WordfastClassicMtSegment.png?raw=true" alt="drawing" width="75%"/>
