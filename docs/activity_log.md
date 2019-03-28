# Activity Log


## Purpose

Send end user activities to Paper Pusher.

## Activities

| Number | PaperPusher Name | Message | Category |
|-------:|:----------------:|:-------:|:--------:|
| 1 | costCreated | Created Cost \{CostId} | Cost Creation |
| 2 | costDeleted | Deleted Cost \{CostId} | Cost Creation |
| 10 | costSubmittedForApproval | Submitted \{CostId} for approval by \{ApprovalUsername} | Approvals |
| 11 | approverAdded | Added \{ApproverUsername} as an approver to \{CostId} | Approvals |
| 12 | approverChanged | Changed approver from \{OldApproverUsername} to \{NewApproverUsername} | Approvals |
| 13 | watcherAdded | Added watcher \{WatcherUsername} to \{CostId} | Approvals |
| 14 | watcherChanged | Changed watcher from \{OldWatcherUsername} to \{NewWatcherUsername} | Approvals |
| 15 | watcherDeleted | Deleted watcher \{WatcherUsername} from \{CostId} | Approvals |
| 16 | approverDeleted | Deleted approver \{ApproverUsername} from \{CostId} | Approvals |
| 17 | costRecallRequest | Recall cost \{CostId} from \{ApproverUsername} | Approvals |
| 18 | costRecalled | Cost \{CostId} successfully recalled | Approvals |
| 19 | costFullyApproved | Cost \{CostId} approved by \{ApproverUsername} for stage gate/revision \{StageGate}\{RevisionNumber} | Approvals |
| 20 | costRejectedWithReason | Cost \{CostId} rejected by \{ApproverUsername} for the following reason: \{RejectComment} | Approvals |
| 21 | costReopened | Cost \{CostId} reopened | Approvals |
| 30 | poCreated | PO \{PONumber} created for Cost \{CostId} | Payment Details |
| 31 | ioNumberAddedNACyclone | IO number \{IONumber} added NA Cost Cyclone | Payment Details |
| 32 | ioNumberAdded | IO number \{IONumber} added | Payment Details |
| 33 | goodsReceipt | Goods Receipt \{GoodsReceipt} for Cost \{CostId} | Payment Details |
| 34 | requisitionNumber | Requisition Number \{RequisitionNumber} for Cost \{CostId} | Payment Details |
| 40 | costUpdated | Cost \{CostId} updated | Data changes |
| 50 | expectedAssetCreated | Expected asset \{AssetName} with ID \{AssetId} Created for Cost \{CostId} with AD-ID \{ADID} | Expected Assets |
| 51 | expectedAssetDeleted | Expected asset \{AssetName} with ID \{AssetId} Deleted for Cost \{CostId} with AD-ID \{ADID} | Expected Assets |
| 52 | expectedAssetUpdated | Expected asset \{AssetName} with ID \{AssetId} Updated for Cost \{CostId} with AD-ID \{ADID} | Expected Assets |
| 53 | adIdServiceUnavailable | AD-ID service is unavailable | Expected Assets |
| 54 | adIdAllocated | AD-ID \{AD-ID} retrieved for asset \{AssetId} \{AssetName} for Cost \{CostId} | Expected Assets |
| 60 | policyExceptionAdded | Policy exception \{PolicyExceptionType} ID \{PolicyExceptionId} added for Cost \{CostId} | Policy Exceptions |
| 61 | policyExceptionDeleted | Policy exception \{PolicyExceptionType} ID \{PolicyExceptionId} deleted for Cost \{CostId} | Policy Exceptions |
| 62 | policyExceptionUpdated | Policy exception \{PolicyExceptionType} ID \{PolicyExceptionId} updated for Cost \{CostId} | Policy Exceptions |
| 63 | policyExceptionApproved | Policy exception \{PolicyExceptionType} ID \{PolicyExceptionId} approved for Cost \{CostId} by \{ApproverUsername} | Policy Exceptions |
| 64 | policyExceptionRejected | Policy exception \{PolicyExceptionType} ID \{PolicyExceptionId} rejected for Cost \{CostId} by \{ApproverUsername} | Policy Exceptions |
| 70 | supportingDocumentAdded | Supporting document \{SupportingDocumentFilename} ID \{SupportingDocumentId} added to Cost \{CostId} | Supporting Documents |
| 71 | supportingDocumentDeleted | Supporting document \{SupportingDocumentFilename} ID \{SupportingDocumentId} deleted from Cost \{CostId} | Supporting Documents |
| 72 | supportingDocumentUpdated | Supporting document \{SupportingDocumentFilename} ID \{SupportingDocumentId} updated for Cost \{CostId}  | Supporting Documents |
| 80 | budgetFormTemplateUploaded | Budget form template \{BudgetFormFilename} uploaded from admin | Budget Form |
| 81 | budgetFormTemplateAdminDownloaded | Budget form template \{BudgetFormFilename} downloaded from admin | Budget Form |
| 82 | budgetFormUploaded | Budget form \{BudgetFormFilename} ID \{BudgetFormId} uploaded to Cost \{CostId} | Budget Form |
| 83 | budgetFormTemplateDownloaded | Budget form \{BudgetFormFilename} downloaded from Cost Overview | Budget Form |
| 90 | travelCreated | Travel \{TravelName} with ID \{TravelId} Created for Cost \{CostId} | Travel |
| 91 | travelDeleted | Travel \{TravelName} with ID \{TravelId} Deleted for Cost \{CostId} | Travel |
| 92 | travelUpdated | Travel \{TravelName} with ID \{TravelId} Updated for Cost \{CostId} | Travel |
| 100 | valueReportingEdited | Value reporting edited for Cost \{CostId} | Value Reporting |
| 110 | exchangeRateChanged | Exchange rate data changed | Admin |
| 111 | userRoleAssigned | User \{Username}\{UserId} was assigned user role \{RoleName} | Admin |
| 112 | userRoleRemoved | User \{Username}\{UserId} user role \{RoleName} was removed | Admin |
| 113 | userRoleUpdated | User \{Username}\{UserId} user role was updated from \{OldRole} to \{NewRole} | Admin |
| 114 | userUpdatedSSO | User \{Username}\{UserId} user role was updated via SSO from \{OldRole} to \{NewRole} | Admin |
| 120 | aipeSelected | AIPE Selected for Cost \{CostId} | AIPE |
| 130 | dpvSelected | DPV Selected for Cost \{CostId} | Direct Payment Vendor |
| 140 | costVersionCreated | Cost Stage \{StageName} Version \{CostVersionId} created for Cost \{CostId} | Revisions and Stages |
| 150 | сostOwnerChanged | Cost \{Costnumber} changed owner from \{oldOwnerUsername} to \{newOwnerUsername} by \{agencyAdminUsername} | Cost Owner |


* Number corresponds to the ActivityLogType enum
* PaperPusher Name corresponds to the name of the activity in [PaperPusher](https://git.adstream.com/adbank-5/paper-pusher/blob/ADCOST_NEW/app/com/adstream/paperpusher/ActivityType.scala#L278)
* Message is the message field of the JSON sent to PaperPusher

## Activities that have not been added

| Number | PaperPusher Name | Reason |
|-------:|:----------------:|:-------:|
| 12 | approverChanged | Unable to figure out if approver has changed. |
| 14 | watcherChanged | Unable to figure out if watcher has changed. |
| 31 | ioNumberAddedNACyclone | Unable to verify if IO Number is for a Cost that is part of a Cyclone Agency |
| 112 | userRoleRemoved | The current implementation removes all roles |
| 113 | userRoleUpdated | The current implementation removes all roles, therefore, unable to figure out what role has changed  |
| 114 | userUpdatedSSO | Unable to test SSO logins |

### Parameters

* \{CostId} - This is the Cost Number
* \{UserId} - The logged in user's Id
* \{UserName} - The logged in user's Id

### Delivery Status

* 0 - New,
* 1 - Processing,
* 2 - Sent,
* 3 - Failed,
* 4 - MaxRetriesReached

## API

None

## .Net Services

IActivityLogService

## Database Tables

Activity_Log
Activity_Log_Delivery
Activity_Log_Message_Template

## JIRA Tickets

* ADC-1817
* ADC-1825
* ADC-1826
* ADC-1827
* ADC-1828

##How to Add new Activity

1. Add new entry to enum [ActivityLogType](../costs.net.dataAccess/Entity/ActivityLogType.cs) with an integer value.
2. Create a new DotLiquid template for the ActivityLogType to the [activity_log_message_template table](../costs.net.database/migration/schema/V1.1/V1.1.41__Create_activity_log_message_template_table.sql) in a [migration script](../costs.net.database/migration/schema/V1.1/V1.1.42__Add_cost_created_template.sql).
3. Create a new class that derives from [ActivityLogEntryBase](../costs.net.core/Models/ActivityLog/ActivityLogEntryBase.cs).
4. Use the new class written in Step 3 with the [IActivityLogService](../costs.net.core/Services/ActivityLog/IActivityLogService.cs) in the place where the [activity occurs](https://git.adstream.com/adstream-costs/costs.net/blob/develop/costs.net.core/Services/Workflow/WorkflowService.cs#L98-99).
5. [Extend PaperPusher](https://git.adstream.com/adbank-5/paper-pusher/merge_requests/221) to add the new activity. The name of the activity in PaperPusher must match the entry in the DotLiquid template.
6. Done
