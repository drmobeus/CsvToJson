# CSV to JSON Converter Function

## Overview

This Azure Function will trigger on new or updated files in the source Blob container, and if they are valid CSV files (with a valid .csv suffix), then it will attempt to process the content into JSON format and save it to an output Blob container.

The source file is removed from the source container upon successful completion.

Should the file be an invalid format, or it is unable to process the CSV into JSON, then it will move that file to an Errors Blob container for investigation.

The output files contain only 1 row from the input, and if there is more than one row, then the output files are numerically appended.

*For example:*

Input: **test.csv** (container 3 rows)

Output: **test1.json**, **test2.json** and **test3.json**

***NOTE***:

If the same file is repeatedly dropped into the source container, then the output will overwrite the previously existing output. This is in effect an Upsert.

If this behavior is not desired, then it is merely a matter of putting a check in to skip the writing to the JSON container if the file already exists. A test, set to *ignore* at present, already exists for this.

---

## Deployment

Deployment follows normal standard Azure function deployment practices.

---

#Architecture

![](.\images\CSVToJSON-Architecture.png)

![](.\images\csv-to-json-flowchart.drawio.png)

---

# Settings

To apply writing to AppInsights, add these settings:

    "APPINSIGHTS_INSTRUMENTATIONKEY": "<Instrumentation key guid",
    "WEBSITE_SITE_NAME": "CsvToJsonFunction" 

The WEBSITE_SITE_NAME will set the Cloud Role Name in AppInsights for easier searching by application name.



TODO: explain the rest of the settings!