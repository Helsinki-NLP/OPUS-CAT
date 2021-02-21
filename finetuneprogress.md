---
layout: page
title: Monitoring the progress of fine-tuning
permalink: /finetuneprogress
---

As fine-tuning can take a long time, it is useful to monitor its progress to make sure everything is working correctly.

### Fine-tuning status and estimated duration

The model list on the **Models** tab shows an indicator of progress in the **Status** column for each model that is currently being fine-tuned.

<img src="./images/enginefinetune5.png?raw=true" alt="drawing" width="75%"/>


The **Status** column displays a textual description of the ongoing phase of fine-tuning (*Preprocessing training files* in the screenshot above). The description is overlaid on a progress bar, which indicates progress by filling gradually with green color.

<img src="./images/finetuneprogress1.png?raw=true" alt="drawing" width="75%"/>

Once the preprocessing steps of the training are complete, the **Status** displays the text *Fine-tuning* and the estimated remaining duration of the fine-tuning process. The estimate is based on the speed of fine-tuning seen so far, so it may be inaccurate.

### Displaying the development of the validation scores of the fine-tuned model

<img src="./images/finetuneprogress2.png?raw=true" alt="drawing" width="75%"/>

By selecting the fine-tuned model and clicking **Show fine-tuning progress** you can view how the fine-tuned model scores against the in-domain validation set (blue line) as the fine-tuning progresses. The **Fine-tuning progress** tab also displays validation scores for an out-of-domain validation set (red line). The out-of-domain validation set has been extracted from the Tatoeba corpus, which contains mainly simple translations from different domains. The in-domain scores should gradually improve during fine-tuning, while the out-of-domain scores should stay stable or fall slightly.

<img src="./images/finetuneprogress3.png?raw=true" alt="drawing" width="75%"/>
