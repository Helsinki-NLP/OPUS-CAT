# Fiskmo MT Engine and CAT plugins

Fiskmo MT Engine is a Windows-based machine translation system built on the Marian NMT framework. Fiskmo MT Engine makes it possible to use a large selection of advanced neural machine translation models natively on Windows computers. The primary purpose of Fiskmo MT Engine is to provide professional translator local, secure, and confidential neural machine translation in computer-assisted translation tools (CAT tools), which are usually Windows-based. To that end, there are plugins available for two of the most popular CAT tools, SDL Trados Studio and memoQ.

## Quickstart for translators

1. Download the installer for Fiskmo MT Engine and install to your computer.
2. Download the plugin for your translation software and install it:
  - Trados 2017/2019: double-click plugin file to install.
  - memoQ: Copy the plugin file to the Addins subfolder in the memoQ installation folder.
3. Start the Fiskmo MT Engine application, and install models from the Fiskmo model repository for the language pairs that you require.
4. Start your CAT tool, and add the Fiskmo plugin to a translation project.

NOTE: Fiskmo MT Engine application needs to be running on the computer, when the plugins are used.

## About

This work is part of the [fiskmö project](https://blogs.helsinki.fi/fiskmo-project/). It implements a self-contained MT plugin for SDL Trados Studio that runs a translation engine based on [MarianNMT](https://marian-nmt.github.io) locally within the plugin. Please acknowledge the project if you use our tools and resources.


## Download

The latest compiled plugin:

* [SDL Trados Studio 2019](https://github.com/Helsinki-NLP/fiskmo-trados/raw/master/build/2019-10-22/FiskmoTranslationProvider.sdlplugin)
* [SDL Trados Studio 2017](https://github.com/Helsinki-NLP/fiskmo-trados/raw/master/build/2019-10-22/FiskmoTranslationProvider2017.sdlplugin)


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
