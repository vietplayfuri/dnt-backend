# Feature Toggles


## Purpose

Enable and disable features in the Cost module. This helps to prevent showing features to clients before they are fully developed.

## Features

| Name | Enabled | Comment |
|:------:|:-----:|:-------------:|
| PolicyExceptions | Yes | Allow end users to enter policy exceptions on the Budget screen |
| AddCostOwners | No | Add cost owners to an existing cost from the Cost Overview screen |
| AIPE | No | After Initial Presentation Expense functionality |
| RejectionDetailsForCoupaRequisitioner | No | Display rejection details on the Cost Review screen when a cost has been rejected in Coupa. |

## API

* v1/admin/feature/isenabled?name=FEATURE_NAME - Returns the true or false if feature is enabled.
* v1/admin/feature?enabled=true|false - Returns all features that are either enabled or disabled

## .Net Services

IFeatureService

## Database Tables

Feature

##How to Add new feature toggle

1. Add &lt;feature-toggle name="\{FEATURE_NAME}"&gt; around an Angular component.
2. Add an entry in the feature table and set the enabled column to **true**.
3. Done
