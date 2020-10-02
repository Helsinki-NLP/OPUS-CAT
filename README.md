# Fiskmo MT Engine and CAT plugins

Fiskmo MT Engine is a Windows-based machine translation system built on the [Marian NMT framework](https://marian-nmt.github.io/). Fiskmo MT Engine makes it possible to use a large selection of advanced neural machine translation models natively on Windows computers. The primary purpose of Fiskmo MT Engine is to provide professional translators local, secure, and confidential neural machine translation in computer-assisted translation tools (CAT tools), which are usually Windows-based. To that end, there are plugins available for two of the most popular CAT tools, SDL Trados Studio and memoQ (Fiskmo MT can also be used in the Wordfast CAT tool as a custom provider). Fiskmo MT Engine provides pretrained MT models for a very wide selection of language pairs, courtesy of the OPUS MT project ([listing of OPUS MT models](https://github.com/Helsinki-NLP/Opus-MT-train/tree/master/models)).

## Quickstart for translators

1. Download the [Fiskmo MT Engine](https://github.com/Helsinki-NLP/fiskmo-trados/raw/develop/build/2020-05-26/FiskmoMTEngine.zip) and install it to your computer by extracting it in a folder on your computer.
2. Download the plugin for your translation software and install it:
  - [Trados 2017](https://github.com/Helsinki-NLP/fiskmo-trados/raw/develop/build/2020-05-26/FiskmoTranslationProvider2017.sdlplugin): double-click plugin file to install.
  - [Trados 2019](https://github.com/Helsinki-NLP/fiskmo-trados/raw/develop/build/2020-05-26/FiskmoTranslationProvider.sdlplugin): double-click plugin file to install.
  - [memoQ](https://github.com/Helsinki-NLP/fiskmo-trados/raw/develop/build/2020-05-26/FiskmoMTPlugin.dll): Copy the plugin file to the Addins subfolder in the memoQ installation folder. **IMPORTANT**: Make sure to unblock the dll file before copying it (right-click file, choose **Properties**, check the **Unblock** box in the bottom right of the **Properties** window). [Detailed memoQ instructions](#using-the-fiskmo-plugin-in-memoq).
  - Wordfast: there is currently no plugin for Wordfast, but Fiskmo MT can be used in Wordfast as a custom MT provider (this requires some configuration work, so contact Fiskmo project for assistance if you wish to use Fiskmo with Wordfast).
3. Start the Fiskmo MT Engine application by clicking FiskmoMTEngine.exe in the extraction folder.
4. Install models from the Fiskmo model repository for the language pairs that you require.
   - Click **Install OPUS model from Web**.
       - <img src="/images/InstallOnlineModel.PNG?raw=true" alt="drawing" width="75%"/>
   - Enter source and target language codes in the filtering boxes on the top row, select a model (usually the one with latest date suffix) and then click **Install locally**.
       - <img src="/images/ModelList.PNG?raw=true" alt="drawing" width="75%"/>
5. After the model has been downloaded and installed, test that it works.
    - Select model and click **Test selected model**.
       - <img src="/images/TestModel.PNG?raw=true" alt="drawing" width="75%"/>
    - Enter translation in the **Source text** area, click **Translate** and wait for a translation to appear in the **Translation** area (producing the first translation may take some time, as the model needs to be initialized, subsequent translations are faster).
       - <img src="/images/TestTranslation.PNG?raw=true" alt="drawing" width="75%"/>
6. Start your CAT tool, and add the Fiskmo plugin to a translation project.

NOTE: Fiskmo MT Engine application needs to be running on the computer, when the plugins are used.

## Fine-tuning models with the Trados Studio plugin

[Fiskmö Trados 2019 plugin](https://github.com/Helsinki-NLP/fiskmo-trados/raw/develop/build/2020-05-26/FiskmoTranslationProvider.sdlplugin) includes a functionality for fine-tuning models with bilingual material that is specific to a domain or style of translation.

The base models that can be downloaded from the OPUS model repository generally produce useful machine translations. However, since the base models have been trained on a mix of different bilingual texts from various textual domains and styles, the translations may not be suitable for many contexts. In a professional translation setting in particular, there is an expectation of terminological and phraseological consistency within documents and adherence to style guides and glossaries.

By fine-tuning a model with suitable fine-tuning material (bilingual texts from the relevant domain), a base model can be modified to produce translations that adhere to the conventions used in the fine-tuning material.

The fine-tuning functionality in the Trados 2019 plugin is implemented as a custom batch task. The batch task can be invoked via the Batch Tasks button on the Home ribbon (or via context menus):

<img src="/images/BatchTask.PNG?raw=true" alt="drawing" width="75%"/>

The fine-tune batch task can also be used to pregenerate the translations for the project. Note that this means pregenerating the translations in the cache of the Fiskmö MT engine so that they will be instantly available as matches when translating in Studio, the translations will not be copied in the sdlxliff files by the custom task. You may choose to fine-tune and translate, fine-tune only, or translate only (the model will have to be selected in that case). The different modes are shown in the following picture:

<img src="/images/FineTune.PNG?raw=true" alt="drawing" width="75%"/>

Fine-tuned models are identified by model tags (see the **Model tag** box in the picture above), which usually describe the model. Fine-tuning settings can defined more specifically in the **Fine-tuning settings** tab:

<img src="/images/FinetuneSettings.PNG?raw=true" alt="drawing" width="75%"/>

Generally the most useful fine-tuning material is the translated text that is already present in the project (i.e. exact match segments). These segments will always be extracted for fine-tuning. However, you can also extract fuzzy matches from the TM to use as fine-tuning material, which is especially important in cases where there's not enough translated text present in the project. You can set the minimum fuzzy percentage and the maximum amount of fuzzies to extract for each segment in the **Fine-tuning settings** tab.

When you are ready to start fine-tuning, click **Finish** in the Trados **Batch Processing** screen. Extracting the fine-tuning material may take a while, depending on project and TM size. Once the fine-tuning material has been extracted, the batch task will send it over to the Fiskmo MT engine running on the same computer in order to start the fine-tuning.

<img src="/images/FinetuneInProgress.PNG?raw=true" alt="drawing" width="75%"/>

The fine-tuned model will be shown in the local model list with a status of **Customizing**, and the text **Fine-tuning in progress** is displayed in the bottom of the MT engine screen. The fine-tuning will generally take several hours, and once it's finished, the status of model will change to **OK** and the text at the bottom will disappear.

You can use a fine-tuned model for translation by selecting the tag of the model in the settings of the Fiskmo translation provider in Trados:

<img src="/images/SelectingModel.PNG?raw=true" alt="drawing" width="75%"/>

If the fine-tuning is still in progress for the model with chosen tag, the connection status message will state so.

If the project contains a lot tags, it's possible to include textual representations of tags into the fine-tuning material, in which case the fine-tuned model learns to transfer tags from the source text into the target text. The tag learning feature is experimental, so it might not work optimally in all cases (the fine-tuning material needs to contain enough tags to learn from, and the tag structure should be relatively simple). You can choose to learn  placeholder tag positions (**Include placeholder tags as text**) and tag pairs (**Include tag pairs as text**).

## Using the Fiskmo plugin in memoQ

### Installing the memoQ plugin

1. First download the [memoQ plugin file](https://github.com/Helsinki-NLP/fiskmo-trados/raw/develop/build/2020-05-26/FiskmoMTPlugin.dll).
2. **IMPORTANT**: Unblock the *FiskmoMTPlugin.dll* file:
  - Right-click the file and choose **Properties**.

    - <img src="/images/DllProperties1.PNG?raw=true" alt="drawing" width="75%"/>

  - Check the **Unblock** box in the bottom right and click **OK**.

    - <img src="/images/DllProperties2.PNG?raw=true" alt="drawing" width="75%"/>

3. Copy the *FiskmoMTPlugin.dll* file to the **Addins** subfolder in the memoQ installation folder.

The path of the installation folder varies in different memoQ versions, for memoQ 9 the default installation folder is *C:\Program Files\memoQ\memoQ-9*.

Here's the easiest way of locating the memoQ installation folder:
1. Press the Windows key to open the Start menu with the cursor in the search box.
2. Type "memoq" into the search box.
3. You should see a link to the memoQ application as the top result.
4. Right-click the memoQ link and choose **Open file location**.
5. The folder containing the link opens.
6. Right-click the link and choose **Open file location** again.
7. The memoQ installation folder containing the **Addins** subfolder should now open.

Once the FiskmoMTPlugin.dll file has been copied to the **Addins** folder, the plugin should be loaded when memoQ is started. As the Fiskmo plugin for memoQ is an unsigned plugin, you need to confirm that it can be loaded with memoQ. Select **Yes** in the dialog box:

<img src="/images/Unsigned.PNG?raw=true" alt="drawing" width="75%"/>

### Setting up the Fiskmo plugin for memoQ

MT is used in memoQ by first defining a set of MT settings and then attaching those settings to a project. You can create a set of MT settings by performing the following steps:
1. Open Resource Console by pressing Alt + R.
2. Select **MT Settings** from the bottom of the **Resource** pane on the left and select **Create new** from the bottom right pane.
- <img src="/images/memoQResourceConsole.png?raw=true" alt="drawing" width="75%"/>
3. Enter a name and a description for this set of MT settings, and then click **OK**.
- <img src="/images/MtSettings.PNG?raw=true" alt="drawing" width="75%"/>
4. Select the MT settings you just created in the Resource Console and click **Edit settings**.
- <img src="/images/EditSettings1.PNG?raw=true" alt="drawing" width="75%"/>
5. Scroll to the bottom of the list of MT plugins and then check the box next to **Fiskmo MT Plugin**.
- <img src="/images/EditSettings2.PNG?raw=true" alt="drawing" width="75%"/>
6. Click the gear icon next to **Configure plugin**.
- <img src="/images/EditSettings3.PNG?raw=true" alt="drawing" width="75%"/>
7. In the **Fiskmo MT plugin settings** window you can check that the Fiskmo MT Engine is running on the same computer by clicking **Retrieve language pair information**.
- <img src="/images/EditSettings4.PNG?raw=true" alt="drawing" width="75%"/>
8. If memoQ displays an error when you click **Retrieve language pair information**, Fiskmo MT Engine is probably not running on your computer. In that case, start Fiskmo MT Engine as instructed in steps 3 to 5 in [Quickstart for translators](#quickstart-for-translators).
9. Click **OK** to finish editing the settings (the **OK** button may be hidden by the Windows taskbar, but you can still select it by pressing Alt + O). Close Resource Console by clicking **Close**.

### Adding the MT settings to a translation project
To use MT during translation, you need to add the MT settings that you created in the previous section to the translation project. You can do this performing the following steps:

1. Open the project and select **Settings** from the bottom of the **Project home** pane on the left.
- <img src="/images/AddSettings1.PNG?raw=true" alt="drawing" width="75%"/>
2. Select the **MT** icon from the ribbon in the **Settings** pane and check the box next to the MT settings you defined previously (here named *Fiskmo MT settings*).
- <img src="/images/AddSettings2.PNG?raw=true" alt="drawing" width="75%"/>
3. When you now open a document for translation, translations from the Fiskmo MT Engine should be displayed in the **Translation results** pane (if not, close the file and open it again).
- <img src="/images/AddSettings3.PNG?raw=true" alt="drawing" width="75%"/>

### Pretranslating with Fiskmo plugin for memoQ
Often it's best to pretranslate documents with the Fiskmo MT plugin before starting translation, since this means that the translations are available immediately (when translating segment by segment in the editor, generating the MT may take some time).

You can pretranslate documents with the Fiskmo MT plugin in memoQ by performing the following steps:

1. Open the project, select **Translations** from the **Project home** pane on the left. Right-click the document you want to pretranslate and select **Tasks** and **Pre-Translate**.
- <img src="/images/Pretranslate1.PNG?raw=true" alt="drawing" width="75%"/>
2. In the **Pre-translate and statistics** window check the **Use machine translation if there is no TM or corpus match ** box in the **Machine translation** section. *Fiskmo MT* should be listed as a MT plugin in this section.
- <img src="/images/Pretranslate2.PNG?raw=true" alt="drawing" width="75%"/>
3. Click **OK** to start pretranslation. When you open the document again, each segment with no TM matches should contain a machine translation with a match percentage of 0.
- <img src="/images/Pretranslate3.PNG?raw=true" alt="drawing" width="75%"/>

## About

This work is part of the [fiskmö project](https://blogs.helsinki.fi/fiskmo-project/). It implements a self-contained MT plugin for SDL Trados Studio that runs a translation engine based on [MarianNMT](https://marian-nmt.github.io) locally within the plugin. Please acknowledge the project if you use our tools and resources.

Source:

```
git clone https://github.com/Helsinki-NLP/fiskmo-trados.git
```


## MIT License

Copyright 2019 Tommi Nieminen

Permission is hereby granted, free of charge, to any person obtaining
a copy of this software and associated documentation files (the
"Software"), to deal in the Software without restriction, including
without limitation the rights to use, copy, modify, merge, publish,
distribute, sublicense, and/or sell copies of the Software, and to
permit persons to whom the Software is furnished to do so, subject to
the following conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
