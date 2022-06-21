---
layout: page
title: Using automatic edit rules OPUS-CAT MT Engine
permalink: /editrules
---

Automatic edit rules can be used to edit the source text before translating it with MT models (pre-edit rules), or to edit the machine translation (post-edit rules). Regular expressions ([.NET regex flavor](https://docs.microsoft.com/en-us/dotnet/standard/base-types/regular-expression-language-quick-reference)) can be used in the rules. Rules are organized into _rule collections_, which can contain any number of rules. 

### Quickstart: Adding a simple pre-edit rule

1. Select a model from the **Models** tab and click **Edit Rules**.

    <img src="./images/editrules1.png?raw=true" alt="drawing" width="75%"/>


2. The **Edit rules** tab opens. Click **Create rule** in the **Pre-edit rule collections** section.

    <img src="./images/editrules2.png?raw=true" alt="drawing" width="75%"/>

3. The **Create edit rule** window opens.

    <img src="./images/editrules3.png?raw=true" alt="drawing" width="75%"/>
    
   The **Create edit rule** window has two sections. The rule is defined in the upper section, and rule can be tested in the lower section. There are three fields in the upper **Define pre-edit rule** section:

   - **Rule description**: Freeform description of the rule, preferably as informative as possible.
   - **Pre-edit pattern**: This is the pattern that the rule will search for in the source text.
   - **Pre-edit replacement**: This is the text that will replace the part of the source that has been matched by the **Pre-edit pattern**.

4. Let's create a simple rule. One use case for pre-edit rules is correcting common misspellings. Let's assume that the word _achieve_ and its derivatives are systematically spelled as _acheive_ in the source text, which results in the MT being incorrect. This problem can be fixed by applying the following rule:

    <img src="./images/editrules4.png?raw=true" alt="drawing" width="100%"/>

5. The lower section of the **Create edit rule** is the tester, which can be used to verify that the edit rule works as intended. To use the tester, enter a test sentence (in this case, any sentence containing "acheive" will do) into the **Source sentence** box, and then click the **Apply pre-edit rule to source text** button:

    <img src="./images/editrules5.png?raw=true" alt="drawing" width="100%"/>

6. The tester displays the result of the rule application. Parts of the source sentence that match the rule are highlighted, as are the replacements performed in the edited source sentence:

    <img src="./images/editrules6.png?raw=true" alt="drawing" width="100%"/>
    
7. Now that the rule is tested and verified to work, you can save it by clicking **Save** (note that testing the rule is not required, but it reduces the chance of mistakes, especially with more complex rules). When you click **Save**, the **Create edit rule** window is closed, and the new rule is displayed on the **Edit rules** tab of the MT Engine, and it will now be applied when machine translations are generated with this model:

    <img src="./images/editrules7.png?raw=true" alt="drawing" width="100%"/>

### Rule collections

As mentioned, automatic edit rules are organized into _rule collections_, which may contain any amount of rules. Every rule must be part of a rule collection, so when you create a rule using the **Create rule** button (as in the quickstart section above), a rule collection is also created to contain the rule. The default name of the created rule collection is the same as the description of the rule that it contains (_fix "acheive" misspelling_ in the example above). You can edit a rule collection by selecting it from the list and clicking **Edit rule collection**:

  <img src="./images/editrules8.png?raw=true" alt="drawing" width="100%"/>

