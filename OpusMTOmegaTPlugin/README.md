Opus MT plugin for OmegaT
=========================

Opus MT can be used in OmegaT with this plugin.

**Preparations**

The plugin communicates with Opus MT engine via the HTTP API, which is not enabled by default. You can enable the HTTP API by opening a command prompt as an administrator (type _cmd_ into the Windows search bar, right-click the best match, and select **Run as administrator**). Type the following command:

_netsh http add urlacl url=http://+:8500/ user=DOMAIN\user_

Substitute your domain and username for _DOMAIN\user_. If the computer does not belong to a domain, substitute the computer name for domain (you can find this out by typing _about your pc_ in the Windows search bar and clicking the best match). 8500 is the port number used by the HTTP API (it can be modified in the MT engine settings).

You can read more about enabling the HTTP API from [here] (https://github.com/Helsinki-NLP/OPUS-CAT#using-the-http-api-for-integrations)

**Installation**

Install the plugin in the *plugins* subfolder of the OmegaT installation folder.

**Usage**

Once the plugin has been installed, it should show up in the **Options** -> **Machine Translate** menu when a project is open in OmegaT. Make sure that the OPUS MT engine is running on the same machine and that a MT model has been installed for the language pair of the project. If the MT engine is not running, you will see a **Connection refused** error. If the MT engine is running but a model for the language pair has not been installed, you will see a **400: Bad Request** error.
