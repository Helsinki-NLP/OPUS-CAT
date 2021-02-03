## Fine-tuning a model in the OPUS-CAT Trados plugin

Fine-tuning is a method of adapting an MT model to a given domain or style. Fine-tuning requires a collection of bilingual sentences (with same source and target languages as the model to be fine-tuned), which represent the domain or style that the MT model should adapt to. For instance, a model can be adapted for medical translation by fine-tuning it with bilingual medical texts. Fine-tuning may take several hours, but it can improve MT quality significantly.

OPUS-CAT MT models can be fine-tuned by using a custom batch task contained in the OPUS-CAT Trados plugin. (Models can also be [fine-tuned directly in the OPUS-CAT MT Engine](https://helsinki-nlp.github.io/OPUS-CAT/enginefinetune), but the Trados plugin fine-tuning allows for more targeted selection of fine-tuning sentences).

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

- Fine-tuned model tag:
- Include placeholder tags as texts
- Include tag pairs as texts
- Maximum amount of sentences for fine-tuning
- Extract fuzzies to use as fine-tuning material




### Projects with multiple target languages
**OPUS-CAT Finetune** batch task can be performed for only one target language at a time. If you have multiple target languages, deselect the files for all but one of the target languages before proceeding. If there are too many files to deselect, open the **Files** view of Trados Studio:

 <img src="./images/FilesViewFinetune.png?raw=true" alt="drawing" width="75%"/>

In the **Files** view only files of single target language are shown at a time (you can change the target language on the left). Select all files for the target language, then right-click one of the selected files, select **Batch tasks** and then select **OPUS-CAT Finetune**. When a batch is initiated in this way, the **Files** page of the **Batch Processing** window will be skipped and the batch task is performed only for the files selected in the **Files** view.
