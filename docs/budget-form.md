# Budget Form


## Purpose

Allow end users to write Cost Line Items (or Cost Items) in Excel then upload to the Cost module via the Cost Items page.

### What is a Budget Form Template file?

An Excel spreadsheet.

### Where are the Budget Form Templates files?

For the Cost module user they can be downloaded on the Cost overview screen by selecting one from the dropdown. The dropdown is located next the 'New Production Cost' button.  
For a developer, they can be found [here](https://git.adstream.com/adstream-costs/costs.net/tree/develop/docs/budget-form-templates).

### Which Budget Form Template do I choose?

| Content Type | Production Type | Template Name |
|:-------:|:----------------:|:-------:|
| Audio | Any | Audio - All - Summary And Detailed Bid |
| Audio | Any | Audio - All - Summary Only |
| Digital | Any | Digital - All - Summary And Detailed Bid |
| Digital | Any | Digital - All - Summary Only |
| Still Image | Full Production | Still Image - Full Production - Summary And Detailed Bid |
| Still Image | Full Production | Still Image - Full Production - Summary Only |
| Video | CGI Animation | Video - Post Production - Summary And Detailed Bid |
| Video | CGI Animation | Video - Post Production - Summary Only |
| Video | Full Production | Video - Full Production - Summary And Detailed Bid |
| Video | Full Production | Video - Full Production - Summary Only |
| Video | Post Production | Video - Post Production - Summary And Detailed Bid |
| Video | Post Production | Video - Post Production - Summary Only |

### What happens if I upload the wrong type of file?

The Cost module will display a Toast letting the end user know they've uploaded the wrong type of file.

### Where are the Budget Form files stored for each Cost?

When a Cost module user uploads a budget form on the Cost Items page, the Excel file is uploaded and stored using GDN (Global Delivery Network).

### .Net Services

* IBudgetFormService
* IBudgetFormTemplateService
* IBudgetFormPropertyValidator
* ICostLineItemUpdater
* ICostCurrencyUpdater

### Database Tables

* budget_form_template - names of each supported budget form template.
* flat_file - budget form templates stored as a Base64-encoded string.
* excel_cell_lookup - the mappings from cost line item to sheet, column and row index in the Excel spreadsheet.

### JIRA Epics and Tickets

* ADC-161
* ADC-2270
