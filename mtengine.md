---
layout: page
title: OPUS-CAT MT Engine settings
permalink: /enginesettings
---

In the **Settings** tab of OPUS-CAT MT Engine you can modify the default fine-tuning settings, and change the ports used by the two APIs that the MT Engine exposes.

<img src="./images/EngineSettings.png?raw=true" alt="drawing" width="75%"/>

Advanced users can edit the default fine-tuning settings clicking the **Open finetune settings in text editor**. The default settings are intended to be suitable for all computers, but the fine-tuning can be sped up by changing the configuration file. The values of **cpu-threads** and **workspace** parameters have the largest effect on the fine-tuning speed.

The **MT service net.tcp port** setting determines which port OPUS-CAT MT Engine uses for its net.tcp API. This value should only be changed if the default port is unavailable for some reason.

The **MT service HTTP port** setting determines which port OPUS-CAT MT Engine uses for its HTTP API. This value should only be changed if the default port is unavailable for some reason.
