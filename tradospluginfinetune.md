## Fine-tuning a model in the OPUS-CAT Trados plugin

Fine-tuning is a method of adapting an MT model to a given domain or style. Fine-tuning requires a collection of bilingual sentences (with same source and target languages as the model to be fine-tuned), which represent the domain or style that the MT model should adapt to. For instance, a model can be adapted for medical translation by fine-tuning it with bilingual medical texts. Fine-tuning may take several hours, but it can improve MT quality significantly.

OPUS-CAT MT models can be fine-tuned by using a custom batch task contained in the OPUS-CAT Trados plugin. (Models can also be [fine-tuned directly in the OPUS-CAT MT Engine](https://helsinki-nlp.github.io/OPUS-CAT/enginefinetune), but the Trados plugin fine-tuning allows for more targeted selection of fine-tuning sentences).

The name of the fine-tuning custom batch is **OPUS-CAT Finetune**. This task extracts sentence pairs to use as fine-tuning material and sends them to the OPUS-CAT MT Engine, which performs the fine-tuning. Because the actual fine-tuning work happens in the OPUS-CAT MT Engine, Trados can be used normally during the duration of the fine-tuning. The progress of the fine-tuning can be [monitored in the OPUS-CAT MT Engine](https://helsinki-nlp.github.io/OPUS-CAT/finetuneprogress).

**OPUS-CAT Finetune** task extracts sentence pairs for fine-tuning from two different sources:

- Existing translations present in the translation project
- File-based translation memories included in the project

The existing translations are the primary source, since they are directly relevant to the translation project. If the project does not contain enough existing translations to meet the specified amount of fine-tuning sentence pairs, sentence pairs from the translation memories are extracted, starting with full and fuzzy matches for the source sentences in the project. Fine-tuning requires a minimum of 500 sentence pairs (although results will probably not be good with such a low amount). The maximum amount of sentence pairs for fine-tuning is 50,000.

When the fine-tuning is complete, the fine-tuned model can be used in the Trados plugin by selecting the tag of the fine-tuned model in the settings of the OPUS-CAT translation provider.

### Accessing the OPUS-CAT Finetune batch task

The **OPUS-CAT Finetune** batch task can be accessed in Trados Studio by right-clicking a project in the **Projects** view of Trados Studio and selecting **Batch tasks** and **OPUS-CAT Finetune**:

  <img src="./images/SelectFinetuneTask.png?raw=true" alt="drawing" width="75%"/>

An alternative way is to select a project in the **Projects** view, click the **Batch Tasks** icon on the **Home** ribbon, and then select **OPUS-CAT Finetune**:

  <img src="./images/SelectFinetuneTaskRibbon.png?raw=true" alt="drawing" width="75%"/>

### Selecting files for the OPUS-CAT Finetune batch task

After you select **OPUS-CAT Finetune**, the **Batch Processing** window opens up:

  <img src="./images/TradosBatchProcessing.png?raw=true" alt="drawing" width="75%"/>

In the **Description** field there is a note about making sure that the target files are segmented before running the task. If you are not sure whether the files are segmented, running the **Pre-Translate Files** batch task will segment them. If you have created the project with one of the **Prepare** task sequences (the default task sequence), **Pre-Translate Files** has already been performed, and the files are segmented. If you are sure the files are segmented, Click **Next**

On the **Files** page of **Batch Processing** window, you can select the files which will be used in the fine-tuning process. All files are selected by default, and this is usually the best option, but you can exclude some files by clearing their checkboxes:

 <img src="./images/BatchProcessingFiles.png?raw=true" alt="drawing" width="75%"/>

**OPUS-CAT Finetune** batch task can be performed for only one target language at a time. If the project contains multiple target languages, refer to [Projects with multiple target languages](#projects-with-multiple-target-languages).

### The settings of the OPUS-CAT Finetune batch task
When you click **Next** on the **Files** page of **Batch Processing** window, the **Settings** page opens and **OPUS-CAT Finetune** settings are displayed.

 <img src="./images/finetunesettings1.png?raw=true" alt="drawing" width="75%"/>

The main settings are located at the top of the window:

 <img src="./images/finetunesettings2.png?raw=true" alt="drawing" width="75%"/>

- **Fine-tuned model tag**: The text entered in this field will be used as the initial tag of the fine-tuned model. Tags can be used to specify that a certain model should be used for producing translations. The name of the fine-tuned model is formed by adding the text entered here as a suffix to the name of the base model. For example, if the name of the original model is *opus-2021-02-04* and the *finetuned* is entered here, the name of the fine-tuned model will be *opus-2021-02-04_finetuned*.
- **Include placeholder tags as texts** and **Include tag pairs as texts**: Bilingual files may contain tags. The base OPUS models that OPUS-CAT uses do not support translation of tagged text, instead the tags are stripped away before the text is translated, and the resulting machine translation contains no tags. However, when fine-tuning a model, it is possible to specify that tags are to be included in the fine-tuning material as text markers. This allows the fine-tuned model to learn how to place the text markers in the translation. If a model has been fine-tuned with these options, the Trados plugin will attempt to place tags to the translation based on these text markers. This is an experimental feature, so it is disabled by default.
- **Maximum amount of sentences for fine-tuning**: The maximum amount of sentence pairs to be extracted from the project files can be specified here. This option can be used to limit the duration of fine-tuning (the more sentences pairs there are, the longer the fine-tuning will take).  
- **Extract fuzzies to use as fine-tuning material**: If this option is selected, matches from the translation memories added to the project will also be extracted as fine-tuning material (up to the specified maximum amount of sentence pairs).




### Projects with multiple target languages
**OPUS-CAT Finetune** batch task can be performed for only one target language at a time. If you have multiple target languages, deselect the files for all but one of the target languages before proceeding. If there are too many files to deselect, open the **Files** view of Trados Studio:

 <img src="./images/FilesViewFinetune.png?raw=true" alt="drawing" width="75%"/>

In the **Files** view only files of single target language are shown at a time (you can change the target language on the left). Select all files for the target language, then right-click one of the selected files, select **Batch tasks** and then select **OPUS-CAT Finetune**. When a batch is initiated in this way, the **Files** page of the **Batch Processing** window will be skipped and the batch task is performed only for the files selected in the **Files** view.
