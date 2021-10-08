Opus MT plugin for OmegaT
=========================

Opus MT can be used in OmegaT with this plugin.

**Installation**

Install the plugin in the *plugins* subfolder of the OmegaT installation folder.

**Usage**

Once the plugin has been installed, it should show up in the **Options** -> **Machine Translate** menu when a project is open in OmegaT. Make sure that the OPUS MT engine is running on the same machine and that a MT model has been installed for the language pair of the project (see instructions here: https://helsinki-nlp.github.io/OPUS-CAT/install). If the MT engine is not running, you will see a **Connection refused** error. If the MT engine is running but a model for the language pair has not been installed, you will see a **400: Bad Request** error.

**Licensing**

This plugin is based on [OmegaT FakeMT plugin](https://github.com/briacp/omegat-plugin-fake-mt). Unlike the other parts of OPUS-CAT, which are licensed under the MIT license, the OmegaT plugin uses the GPL-3.0 license. If you wish to modify and build the plugin, use the build scripts in [OmegaT FakeMT plugin](https://github.com/briacp/omegat-plugin-fake-mt), substituting OpusMT.java for FakeMT.java in the **src/main/java/net/briac/omegat/** directory.
