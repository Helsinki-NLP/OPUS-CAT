---
layout: page
title: Using automatic edit rules OPUS-CAT MT Engine
permalink: /editrules
---

Automatic edit rules can be used to edit the source text before translating it with MT models (_pre-edit rules_), or to edit the machine translation (_post-edit rules_). Regular expressions ([.NET regex flavor](https://docs.microsoft.com/en-us/dotnet/standard/base-types/regular-expression-language-quick-reference)) can be used in the rules. Rules are organized into _rule collections_, which can contain any number of rules. 

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

The **Edit rules in collection** window contains a field for the name of the collection, and a list of rules in the collection. You can use the buttons on the right side of the list to manage the rules in the collection:

  <img src="./images/editrules9.png?raw=true" alt="drawing" width="100%"/>

  - **Create rule**: Creates a new rule in the collection. This button opens the same **Create edit rule** window as the **Create rule** button in the **Edit rules** tab. The only difference is that a rule created with this button will be added to the existing collection.
  - **Edit rule**: When you select a rule from the list and click this, the rule opens in the **Create edit rule** window, and the rule can be modified.
  - **Delete rule**: Deletes the rule from the collection. **Note**: If you have deleted a rule with this button, the rule may still be recovered by clicking **Cancel** in the lower right corner of the **Edit rules in collection** window. If you click **Save** in the lower left corner of the window, the rule will be permanently deleted.

The **Edit rules in collection** window also contains a field for modifying the name of the collection, and a checkbox labeled **Global collection**. If the **Global collection** checkbox is checked, the collection can be added to other models by using the **Add rule collection** button in the **Edit rules** tab. The checkbox is unchecked by default for newly created models, to avoid cluttering the list of global models.

After you have edited the collection by adding, deleting, or modifying rules or by changing the collection name or its global status, you can either save the modifications by clicking the **Save** button, or reject them by clicking the **Cancel** button. 

### Post-edit rules

Post-edit rules are used to edit the output of the machine translation produced by the model. You can create a post-edit rule by clicking on the **Create rule** button in the **Post-edit rule collections** section of the **Edit rules** tab:

  <img src="./images/editrules11.png?raw=true" alt="drawing" width="100%"/>

When you click on the button, the **Create post-edit rule** window opens:

  <img src="./images/editrules10.png?raw=true" alt="drawing" width="100%"/>

As you can see from the image above, the **Create post-edit rule** window is also divided into two sections, the upper **Define post-edit rule** section and the lower tester section. Post-edit rules are very similar to the pre-edit rules described in the Quickstart section, the only difference is that a post-edit rule contains three fields instead of the two for the pre-edit rule (see [Advanced rule usage](#advanced) for more information about using source patterns):

  1. **Use source pattern**: This is a pattern that is searched for in the original source text of the machine translation (i.e. the source text as it is before the application of pre-edit rules, if any). This field is optional, and it has two use cases:
     - a trigger for the application of the rule (the rule is applied only if the pattern is found in the original source text).
     - to copy text from the original source text directly into the MT output. 
  2. **Post-edit pattern**: This is the pattern that the rule will search for in the source text.
  3. **Post-edit replacement**: This is the text that will replace the part of the MT output that has been matched by the **Post-edit pattern**.

Here's a simple example rule, where all three fields are used:

  <img src="./images/editrules14.png?raw=true" alt="drawing" width="100%"/>

This rule replaces the word _vika_ in the MT output with the word _virhe_, but only if the original source text contained the word _fault_. So in this rule, the source pattern is used as a condition for the replacement.

As in the **Create pre-edit rule** window, the lower section contains a tester, which can be used to verify that the rule works as intended. If a source pattern has been defined, the tester will contain an additional field for entering the test source text, against which the source pattern will be matched:

  <img src="./images/editrules12.png?raw=true" alt="drawing" width="100%"/>

Once the post-edit rule has been defined, it can be saved by clicking the **Save** button in the lower left corner. The rule will appear in a new rule collection in the **Post-edit rule collections** section of the **Edit rules** tab:

  <img src="./images/editrules13.png?raw=true" alt="drawing" width="100%"/>

### <a name="advanced"></a>Advanced rule usage