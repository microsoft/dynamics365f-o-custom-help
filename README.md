
# Tools in the toolkit

The toolkit is available at [https://github.com/microsoft/dynamics365f-o-custom-help/](https://github.com/microsoft/dynamics365f-o-custom-help/). The repo contains the following tools and the source code for the tools:

## HtmlFromRepoGenerator tool

HtmlFromRepoGenerator.exe provides functionality that supports the creation of custom Help based on source files from Microsoft. You can use HtmlFromRepoGenerator.exe to:

- Clone a Microsoft documentation repo
- Remove developer and administrator content from your clone of the Microsoft repo
- Update links to files that are no longer in the clone
- Update the **ms.locale** value to match the language options that are supported by the Finance and Operations client

    The client uses language descriptors that are different from the language descriptors used in the corresponding GitHub repos. For localized custom Help to be called, the language indicators in the content from GitHub must be changed so that they match how the client understands languages.
- Generate HTML files that can be used for publishing.

    The HTML files will be generated in the **d365F-O** subfolder. The files are generated based on stylesheets and templates that are part of the tool.

- Compare a localized Microsoft repo to the equivalent en-US repo to identify discrepancies and update links accordingly.

In the first version of the toolkit, this tool had the name ConsoleApp.exe.  

### Syntax

Here is the syntax for HtmlFromRepoGenerator.exe:  

```
HtmlFromRepoGenerator.exe --Json <Articles/> --Out <path> --ExternalText <text> [--DoNotClone <true|false>] [--Repo <URL>] [--RemoveGitFolder <true|false>] [--ReplaceUrl <URL>] [--LogsDir <.\logs>] [--EnRepo <URL>] [--EnOut <path>] [--Lng <language code>] [--Rtl] [--?[--]]
```

The following table provides an explanation of the parameters:

|Parameter   |Description  |
|------------|-------------|
|Json |Specifies a relative path for the location of the docfx.json file. In Microsoft documentation repos, this location is typically ```articles/```. |
|Out |Specifies the folder where your existing clone is, or the folder to clone the repo to. If you run HtmlFromRepoGenerator to clone a repo, this folder must not already exist. |
|ExternalText |Specifies text that must be added to the updated links if HtmlFromRepoGenerator must replace the original links.|
|DoNotClone |Set this parameter when you run the tool against previously cloned repos. |
|Repo |Specifies the repo URL. This parameter is not required if you run the tool based on a previously cloned repo. Examples of Microsoft documentation repo URLs include *https://github.com/MicrosoftDocs/Dynamics-365-Unified-Operations-public* for English (US) and *https://github.com/MicrosoftDocs/Dynamics-365-Operations.de-de* for German (Germany).|
|RemoveGitFolder|Specifies whether to remove the `.git` folder.|
|ReplaceUrl|Specifies the URL must replace links between files when the target files are not present. This parameter is intended to be used to turn relative links into absolute links.|
|LogsDir|Specifies the folder to save logs files to.|

The following additional parameters are used when the tool is run against the localized Microsoft documentation repos:

|Parameter   |Description  |
|------------|-------------|
|EnRepo|Specifies the URL of the en-US repo. This parameter is not required if you run the tool based on a previously cloned repo. The Microsoft documentation repo URL for English (US) is [https://github.com/MicrosoftDocs/Dynamics-365-Unified-Operations-public](https://github.com/MicrosoftDocs/Dynamics-365-Unified-Operations-public).|
|EnOut|Specifies the folder where the en-US repo exists, or the folder that it must be cloned to. This folder must not already exist if you run the tool based on a previously cloned repo.|
|Lng|Specifies the language value to use for `ms.locale` metadata in the generated HTML files. The value must correspond to the value that is specified in the Finance and Operations client's language settings. If this parameter is not set, the tool uses en-US.|
|Rtl|Set this parameter if the language uses right-to-left (RTL) formatting. Examples of RTL languages include Arabic and Hebrew.|

### Examples

> **NOTE** The Microsoft repos contain many files, so the process takes several minutes. If you run the tool against multiple localization repos, the process takes longer.

The following example clones the en-US repo and generates HTML files for en-US.

```
HtmlFromRepoGenerator.exe --json articles/ --out "D:\D365-Operations\en-US" --repo "https://github.com/MicrosoftDocs/Dynamics-365-unified-Operations-public" --externalText "(This is an external link)" --replaceUrl "https://docs.microsoft.com/en-us/dynamics365/supply-chain" --LogsDir D:\D365-Operations\logs\en-US
```

The following example uses a previously cloned en-US repo and generates HTML files for en-US.

```
HtmlFromRepoGenerator.exe --json articles/ --out "D:\D365-Operations\en-US" --externalText "(This is an external link)" --replaceUrl "https://docs.microsoft.com/en-us/dynamics365/supply-chain" --LogsDir D:\D365-
Operations\logs\en-US
```

The following example clones both the de-DE and en-US repos, and generates HTML files for de.

```
HtmlFromRepoGenerator.exe --json articles/ --out "D:\D365-Operations\de" --repo "https://github.com/MicrosoftDocs/Dynamics-365-Operations.de-de" --externalText "(This is an external link)" --EnRepo "https://github.com/MicrosoftDocs/Dynamics-365-unified-Operations-public" --EnOut "D:\D365-Operations\en-us" --replaceUrl "https://docs.microsoft.com/de-de/dynamics365/supply-chain" --lng "de" --LogsDir D:\D365-Operations\logs\de
```

The following example uses the existing de-DE and en-US repos, and then generates HTML files for de. Make sure that the de-DE repo is up to date if you use the existing repo.

```
HtmlFromRepoGenerator.exe --json articles/ --out "D:\D365-Operations\de" --DoNotClone --externalText "(This is an external link)" --enOut "D:\D365-Operations\en-us" --replaceUrl "https://docs.microsoft.com/de-de/dynamics365/supply-chain" --lng "de" --LogsDir D:\D365-Operations\logs\de
```

> **IMPORTANT** Do not run HtmlFromRepoGenerator.exe repeatedly on a previously-cloned repo. HtmlFromRepoGenerator modifies the links during processing, so running HtmlFromRepoGenerator more than once on the same content will result in incorrect links. If you want to rerun HtmlFromRepoGenerator, either use HtmlFromRepoGenerator to create a new clone of the repo, or revert all local changes to your existing clone.

## ConvertHtmlToJson tool

The ConvertHtmlToJson tool transforms HTML files into JSON files. You can then add the JSON files to the Azure Search service that will generate context-sensitive links to your Help content.  

The JSON files include metadata that is used by the indexer to identify the form and language that the target Help page is intended for.  

Here is the syntax for ConvertHtmlToJson.exe:  

```
ConvertHtmlToJson.exe --h <path> -j <path> --v <true|false>
```

Here is an explanation of the parameters:

|Parameter   |Description  |
|------------|-------------|
|h|Specifies the path to the HTML files that you want to process. |
|j|Specifies the folder that the JSON files will be saved to. The folder must already exist.|
|v|True to enable verbose logging; otherwise false.|

### Examples

The following example generates JSON files without verbose logging:

```
ConvertHtmlToJson.exe --h D:\D365-Operations\d365F-O\supply-chain\de -j D:\D365-Operations\json\supply-chain\de
```

## HtmlLocaleChanger tool

The **HtmlLocaleChanger** tool can update your HTML files with a new value for *ms.locale*. For example, if you have HTML files for German (Germany) and you want to make the same content available in German (Austria), then you can run the tool to change the setting from *ms.locale: de-de* to *ms.locale:de-at*.  

Here is the syntax for HtmlLocaleChanger.exe:  

```
HtmlLocaleChanger.exe --h <path> --l <locale> --v <true|false>
```

Here is an explanation of the parameters:

|Parameter   |Description  |
|------------|-------------|
|h|Specifies the path to the HTML files that you want to process. |
|l|New locale for the HTML files. |
|v|True to enable verbose logging; otherwise false.|

### Examples

The following example changes the locale to *de-at* with verbose logging:

```
HtmlLocaleChanger.exe --h D:\D365-Operations\d365F-O\supply-chain\de --l de-at --v
```

## "Help Pane extension" Visual Studio project

If you plan to deliver custom help content for a Finance and Operations solution, you can extend the Help pane to consume this content. It is a one-time configuration that requires the Finance and Operations development environment in Visual Studio. The result is that users can choose between tabs for Task guides, Microsoft's Help content, and your Help content.

For more information, see [Connect your Help website with the Help pane](https://docs.microsoft.com/en-us/dynamics365/fin-ops-core/dev-itpro/help/connect-help-pane)

## AX 2012 metadata scripts

The scripts in the **AX 2012 metadata scripts** folder can transform Dynamics AX 2012 HTML files so that they can be used in the custom Help environment. The script makes the following changes to the Dynamics AX 2012 HTML files:  

- Replaces the **Microsoft.Help.F1** metadata name with **ms.search.form**  

- Replaces the **Title** metadata name with **title**  

- Changes the file name extension from **.htm** to **.html**  

- Adds the following metadata:  

    ```html
    <meta name="ms.search.region" content="Global" />  
    <meta name="ms.search.scope" content="Operations, Core" />  
    <meta name="ms.dyn365.ops.version" content="AX 7.0.0" />  
    <meta name="ms.search.validFrom" content="2016-05-31" />  
    <meta name="ms.search.industry" content="cross" />  
    ```

# Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.microsoft.com.

When you submit a pull request, a CLA-bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., label, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
