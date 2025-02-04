---
layout: page
title: Secure use
permalink: /secure
---
## Secure use of OPUS-CAT

OPUS-CAT is inherently secure, as it runs on the local machine and uses no external connections during translation. The model downloader does load models from repositories located on the servers of [CSC](https://csc.fi/en/about-us/what-csc/) and some text files from the Tatoeba-Challenge project in Github. However, models can also be installed from separately downloaded zip files, so OPUS-CAT can be used entirely offline.

### Translating sensitive information securely

If you wish to translate sensitive infomation, such as non-anonymized medical data, some extra precautions should be taken. For sensitive data translation, the computer system in which OPUS-CAT runs needs to be secure overall. If you have an IT department, consult with them. If you are using your own computer, be aware that there may be malicious software installed on your computer, which may compromise the confidentiality of the data. If you still wish to translate the data locally with OPUS-CAT, you can increase security by running OPUS-CAT in Windows Sandbox (an isolated Windows environment available on current Windows operating systems):

1. [Install Windows Sandbox] (https://learn.microsoft.com/en-us/windows/security/application-security/application-isolation/windows-sandbox/windows-sandbox-install).
2. Launch Windows Sandbox (e.g. by pressing the Windows key, typing "Windows Sandbox" and pressing Enter).
3. When Windows Sandbox has opened, follow the instructions [here](https://helsinki-nlp.github.io/OPUS-CAT/install) inside the Sandbox environment:
  - Install OPUS-CAT.
  - Install machine translation models for the language directions that you require.
  - Optional: also [install OmegaT](https://omegat.org/) and the OPUS-CAT OmegaT plugin to make translating files easier).
  - Test that the models work by using the #Translate with model# functionality.
4. Transfer your data to the sandbox environment.
5. Optional: you may now disconnect your network connection to further ensure that the data cannot be compromised.
6. Translate your data using either OmegaT with the OPUS-CAT plugin or the #Translate with model# functionality of OPUS-CAT.
7. Transfer the translations out of the sandbox environment.
8. Close the sandbox. The files in the sandbox environment will be automatically removed when the environment is closed.

There are still weak points in the above process, since the confidential data needs to be moved to and from the sandbox environment. These weak points can be addressed by encrypting the data e.g. with [7-zip](https://www.7-zip.org/).