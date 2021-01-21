## Fine-tuning a model in the OPUS-CAT MT Engine

Fine-tuning is a method of adapting an MT model to a given domain or style. Fine-tuning requires a collection of bilingual sentences (with same source and target languages as the model to be fine-tuned), which represent the domain or style that the MT model should adapt to. For instance, a model can be adapted for medical translation by fine-tuning it with bilingual medical texts. Fine-tuning may take several hours, but it can improve MT quality significantly.

OPUS-CAT MT models can be fine-tuned directly in the OPUS-CAT MT Engine (fine-tuning can also be initiated from the Trados plugin, which is better suited for fine-tuning a model for a specific translation job). Models can be fine-tuned with a TMX translation memory or with a pair of source and target language files with aligned lines.

Most CAT tools support exporting translation memories in the TMX format, see the documentation of your CAT tool for instructions.

**Initiating fine-tuning from the OPUS-CAT MT Engine with a TMX translation memory:**

1. Select a model to fine-tune from the **Models** tab of the OPUS-CAT MT Engine and click **Fine-tune model**:

    <img src="./images/enginefinetune1.png?raw=true" alt="drawing" width="75%"/>


2. The **Fine-tune model** tab opens. Click **Browse** next to the **Tmx file** box:

    <img src="./images/enginefinetune2.png?raw=true" alt="drawing" width="75%"/>


3. In the file dialog that opens, select a .tmx file and click **Open**:

    <img src="./images/enginefinetune3.png?raw=true" alt="drawing" width="75%"/>


4. Path of the selected .tmx file should now appear in the **Tmx file** box. Enter a label for the fine-tuned model in the **Fine-tuned model label** box and click **Fine-tune**:

    <img src="./images/enginefinetune4.png?raw=true" alt="drawing" width="75%"/>


5. After you have clicked **Fine-tune**, the **Fine-tune** button should disappear. When you switch to the **Models** tab, the fine-tuned model should be visible in the model list (its name is a combination of the base model name and the fine-tuned model label):

      <img src="./images/enginefinetune5.png?raw=true" alt="drawing" width="75%"/>

See [here](finetuneprogress.md) for information about monitoring the progress of fine-tuning.

**Initiating fine-tuning from the OPUS-CAT MT Engine with an aligned pair of source and target language files:**

The process is otherwise the same as with TMX fine-tuning, except that instead of selecting a TMX file (steps 2 and 3), you click the radio button labeled **Text (separate files for source target)** and browse for the source and target files.

**Validation files**

Validation files are used to track the progress of fine-tuning. During fine-tuning the OPUS-CAT MT Engine will periodically translate the source validation files and compare the results to the target validation file. The progress of fine-tuning can be tracked by checking whether the machine translations that the fine-tuned model produces become closer to the translations in the target validation file.

By default, validation files are picked from the TMX or the source and target files that are used for fine-tuning. It is also possible to specify validation by selecting the radio button labeled **Select separate files** in the **Validation files** section.
