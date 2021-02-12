---
layout: page
title: Fine-tuning
permalink: /finetune
---
 
Fine-tuning is a method of adapting an MT model to a given domain or style. Fine-tuning requires a collection of bilingual sentences (with same source and target languages as the model to be fine-tuned), which represent the domain or style that the MT model should adapt to. For instance, a model can be adapted for medical translation by fine-tuning it with bilingual medical texts. Fine-tuning may take several hours, but it can improve MT quality significantly.

OPUS-CAT MT models can be fine-tuned with two different functionalities:
 - From Trados by using the [OPUS-CAT Fine-tune batch task](/tradospluginfinetune) contained in the OPUS-CAT Trados plugin.
 - Directly from the OPUS-CAT MT Engine by using the [Fine-tune model functionality](/enginefinetune).
