using System.Collections.Generic;

namespace dnt.core
{
    public static class Constants
    {
        public static class ElasticSearchIndices
        {
            public const string CostsIndexName = "costs";
            public const string ProjectsIndexName = "projects";
            public const string CostUsersIndexName = "cost_users";
            public const string AgencyIndexName = "agencies";
            public const string VendorIndexName = "vendors";
            public const string DictionaryEntriesIndexName = "dictionary_entries";
        }

        public static class Application
        {
            public const string EmailParentObject = "adcosts";
        }

        public class AccessObjectType
        {
            public const string Smo = "Smo";
            public const string Region = "Region";
            public const string Client = "Client";
            public const string Agency = "Agency";
            public const string Module = "Module";
        }

        /// <summary>
        /// This string value must match the expected activity type in Paper Pusher for the Adcosts application.
        /// </summary>
        public static class EmailNotificationActionType
        {
            /// <summary>
            /// When both brand and technical approvals are complete.
            /// </summary>
            public const string AllApprovalsApproved = "allApprovalsApproved";
            public const string Cancelled = "notifyCancelled";
            public const string Recalled = "notifyRecalled";
            public const string Rejected = "notifyRejected";
            public const string ReopenRejected = "notifyReopenRejected";
            public const string ReopenApproved = "notifyReopenApproved";
            public const string ReopenRequested = "notifyReopenRequested";
            public const string Submitted = "notifySubmitted";
            public const string BrandApproverAssigned = "brandApproverAssigned";
            public const string BrandApprovalApproved = "brandApprovalApproved";
            public const string TechnicalApproverAssigned = "technicalApproverAssigned";
            public const string TechnicalApprovalApproved = "technicalApprovalApproved";
            public const string GenericAdCost = "genericAdcostMail";
            public const string BrandApproverSendReminder = "brandApproverSendReminder";
            public const string TechnicalApproverSendReminder = "technicalApproverSendReminder";
            public const string ApproverUnassigned = "approverUnassigned";
            public const string CostOwnerChanged = "notifyCostOwnerChanged";
            public const string CostStatus = "notifyCostStatus";
            public const string SubmissionFailed = "notifyCostSubmissionFailed";
        }

        public static class EmailApprovalType
        {
            public const string Brand = "Brand";
            public const string IPM = "IPM";
            public const string CC = "CC";
        }

        public static class EmailNotification
        {
            public const string Application = "adcosts";
            public const string ObjectType = "cost";
        }

        /// <summary>
        /// Maps to the ObjectId column in the TemplateItems table for Mail Service.
        /// </summary>
        public static class EmailNotificationParents
        {
            public const string Approver = "approver";
            public const string CostOwner = "costowner";
            public const string InsuranceUser = "insuranceuser";
            public const string FinanceManagement = "financemanagement";
            public const string BrandApprover = "brandapprover";
            public const string MyPurchases = "mypurchases";
            public const string TechnicalApprover = "technicalapprover";
            public const string Coupa = "coupa";
        }

        public static class SupportingDocuments
        {
            public const string BudgetForm = "BudgetForm";
            public const string AdditionalDocument = "AdditionalDocument";
        }

        public static class BudgetFormExcelPropertyNames
        {
            public const string LookupGroup = "mapping key";
            public const string ContentType = "content type";
            public const string Production = "production";
        }

        public static class BusinessUnit
        {
            public const string CostModulePrimaryLabelPrefix = "CM_Prime_";
            public const string GlobalAgencyRegionLabelPrefix = "CM_AgencyName_";
        }

        public static class ActivityLogData
        {
            public const string MessageId = "messageId";
            public const string ObjectId = "objectId";
            public const string SubjectId = "subjectId";
            public const string Timestamp = "timestamp";
            public const string Username = "username";
            public const string UserId = "userId";
            public const string CostId = "costId";
            public const string IpAddress = "ipAddress";
            public const string CostUserEmail = "emailAddress";
            public const string ApprovalUsername = "approvalUsername";
            public const string OldApprovalUsername = "oldApprovalUsername";
            public const string NewApprovalUsername = "newApprovalUsername";
            public const string WatcherUsername = "watcherUsername";
            public const string OldWatcherUsername = "oldWatcherUsername";
            public const string NewWatcherUsername = "newWatcherUsername";
            public const string Revision = "revision";
            public const string StageGate = "stageGate";
            public const string RejectionComment = "rejectionComment";
            public const string PoNumber = "poNumber";
            public const string IoNumber = "ioNumber";
            public const string GoodsReceipt = "goodsReceipt";
            public const string RequisitionNumber = "requisitionNumber";
            public const string AdId = "adId";
            public const string AssetId = "assetId";
            public const string AssetName = "assetName";
            public const string PolicyExceptionId = "policyExceptionId";
            public const string PolicyExceptionType = "policyExceptionType";
            public const string SupportingDocumentId = "supportingDocumentId";
            public const string SupportingDocumentFilename = "supportingDocumentFilename";
            public const string BudgetFormId = "budgetFormId";
            public const string BudgetFormFilename = "budgetFormFilename";
            public const string TravelId = "travelId";
            public const string TravelName = "travelName";
            public const string RoleName = "roleName";
            public const string UserRoleAssigned = "userRoleAssigned";
            public const string OldRoleName = "oldRoleName";
            public const string NewRoleName = "newRoleName";
            public const string CostVersionId = "costVersionId";
            public const string StageName = "stageName";
            public const string OldOwnerUsername = "oldOwnerUsername";
            public const string NewOwnerUsername = "newOwnerUsername";
            public const string AgencyAdminUserEmail = "agencyAdminUser";
        }
        public class CostSection
        {
            public const string CostTotal = "CostTotal";
        }

        /// <summary>
        /// ADC-2597 - defined features that we can switch on/off
        /// </summary>        
        public static class Features
        {
            public const string PolicyExceptions = "PolicyExceptions";
            public const string RejectionDetailsForCoupaRequisitioner = "RejectionDetailsForCoupaRequisitioner";
            public const string Aipe = "Aipe";
        }

        public static class CostStageConstants
        {
            public const string OriginalEstimate = "OriginalEstimate";
            public const string FirstPresentation = "FirstPresentation";
            public const string FinalActual = "FinalActual";

            /// <summary>
            /// The statuses that can be generated GR# and shown the payment amount on payment summary
            /// </summary>
            public static readonly string[] GrStatuses = { OriginalEstimate, FirstPresentation, FinalActual };
        }

        public static class CostError
        {
            public const string MissingFormData = "Form details section is missing data";
        }

        public static class CostForm
        {
            public const string ProductionDetails = "ProductionDetails";
            public const string StageDetail = "StageDetail";
        }
    }
}
