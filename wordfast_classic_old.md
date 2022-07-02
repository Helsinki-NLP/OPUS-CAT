---
layout: page
title: Wordfast integration
permalink: /wordfast_classic_old
---
## Using OPUS-CAT in older versions of Wordfast Classic


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
