# Billing Expenses


### What is the Billing Expense Table?

The Billing Expense Table is a separate section on the Cost Items page. This section is hidden from the end user unless the following conditions are met:

* The Cost Type is **Usage or Buyout**
* The Budget Region is **North America**
* The Usage/buyout details section is filled in
* The Agency is **Cyclone**
* The Usage Type is **Athletes or Celebrity**
* The Usage/Buyout/Contract Type is **Contract**

### How are the Billing Expenses calculated?

They are calculated by this [class implementation](https://git.adstream.com/adstream-costs/costs.net/blob/develop/costs.net.plugins/PG/Services/BillingExpenses/BillingExpenseCalculator.cs)

### Yes, but how are they supposed to be calculated?

There is an Excel Spreadsheet in [here](https://git.adstream.com/adstream-costs/costs.net/tree/develop/docs/billing-expenses) which is used as a basis for the calculations.

### Does the Billing Expenses feature do anything other than calculate Billing Expenses?

Yes, the Cost Items related to certain fields in the Billing Expenses table are set to the values in the table and made readonly on the UI.  
The list of Cost Items are:

* Negotiation/broker agency fee
* Other services & fees
* Bonus (celebrity only)
* Base Compensation
* Pension & Health (e.g. SAG)

### .Net Services

* IBillingExpenseService
* IBillingExpenseBuilder
* IBillingExpenseCalculator
* IBillingExpenseInterpolator

### Database Tables

* billing_expense - every billing expense item is stored in this table

### JIRA Epics and Tickets

* ADC-2276
