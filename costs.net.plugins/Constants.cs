namespace costs.net.plugins
{
    using costs.net.dataAccess.Entity;
    using costs.net.plugins.PG.Models.Stage;
    using System.Collections.Generic;

    public static class Constants
    {
        public class CostSection
        {
            public const string ProductionInsurance = "productionInsuranceNotCovered";
            public const string PostProductionInsurance = "postProductionInsuranceNotCovered";
            public const string InsuranceTotal = "insuranceNotCovered";

            public const string PostProduction = "postProduction";
            public const string Production = "production";
            public const string TechnicalFee = "technicalFee";
            public const string TalentFees = "talentFees";
            public const string TargetBudgetTotal = "TargetBudgetTotal";
            public const string Other = "AllOtherCosts";
            public const string CostTotal = "CostTotal";
        }

        public class ContentType
        {
            public const string Video = "Video";
            public const string Photography = "Photography";
            public const string Audio = "Audio";
            public const string Digital = "Digital";
        }

        public class MediaType
        {
            public const string Multiple = "Multiple";
            public const string NA = "N/A";
            public const string NotForAir = "not for air";
            public const string InStore = "In-store";
            public const string DirectToCustomer = "Direct to consumer";
            public const string PRInfluencer = "PR/Influencer";
            public const string StreamingAudio = "streaming audio";
            public const string Radio = "Radio";
            public const string OutOfHome = "Out of home";
            public const string Cinema = "Cinema";
            public const string Digital = "Digital";
            public const string Tv = "Tv";
        }

        public class BudgetRegion
        {
            public const string China = "GREATER CHINA AREA";
            public const string NorthAmerica = "NORTHERN AMERICA AREA";
            public const string AsiaPacific = "AAK (Asia)";
            public const string Japan = "JAPAN";
            public const string Europe = "EUROPE AREA";
            public const string IndiaAndMiddleEastAfrica = "INDIA & MIDDLE EAST AFRICA AREA";
            public const string Latim = "LATIN AMERICA AREA";
        }

        public class VendorCategory
        {
            public const string UsageBuyoutContract = "UsageBuyoutContract";
            public const string DistributionTrafficking = "DistributionTrafficking";
            public const string PostProductionCompany = "PostProductionCompany";
            public const string ProductionCompany = "ProductionCompany";
            public const string TalentCompany = "TalentCompany";
            public const string CGICompany = "CGICompany";
            public const string MusicCompany = "MusicCompany";
            public const string AudioMusicCompany = "AudioMusicCompany";
            public const string DigitalDevelopmentCompany = "DigitalDevelopmentCompany";
            public const string PhotographyCompany = "PhotographyCompany";
            public const string RetouchingCompany = "RetouchingCompany";
            public const string CastingCompany = "CastingCompany";
        }

        public class AgencyRegion
        {
            /// <summary>
            /// Matches geo_regions.json
            /// </summary>
            public const string NorthAmerica = "North America";
            /// <summary>
            /// Matches geo_regions.json
            /// </summary>
            public const string Europe = "Europe";
        }

        public class BusinessRole
        {
            public const string Ipm = "Integrated Production Manager";
            public const string BrandManager = "Brand Manager";
            public const string FinanceManager = "Finance Manager";
            public const string RegionSupport = "Region Support";
            public const string PurchasingSupport = "Purchasing Support";
            public const string CostConsultant = "Cost Consultant";
            public const string InsuranceUser = "Insurance User";
            public const string RegionalAgencyUser = "Regional Agency User";
            public const string AgencyOwner = "Agency Owner";
            public const string CentralAdaptationSupplier = "Central Adaptation Supplier";
            public const string AgencyFinanceManager = "Agency Finance Manager";
            public const string GovernanceManager = "Governance Manager";
            public const string AgencyAdmin = "Agency Admin";
            public const string AdstreamAdmin = "Adstream Admin";

            public static readonly string[] BusinessRoles =
            {
                FinanceManager,
                PurchasingSupport,
                Ipm,
                CostConsultant,
                BrandManager,
                GovernanceManager,
                AgencyAdmin,
                RegionSupport,
                InsuranceUser,
                RegionalAgencyUser,
                AgencyOwner,
                CentralAdaptationSupplier,
                AgencyFinanceManager,
                AdstreamAdmin
            };
        }

        public class ProductionType
        {
            public const string FullProduction = "Full Production";
            public const string PostProductionOnly = "Post Production Only";
            public const string CgiAnimation = "CGI/Animation";

            public static readonly List<string> ProductionTypeList = new List<string>
            {
                FullProduction,
                PostProductionOnly,
                CgiAnimation,
            };
        }

        public class Agency
        {
            public const string CycloneLabel = "Cyclone";
            public const string PAndGLabel = "costPG";
            public const string PgOwnerLabel = "CM_Prime_P&G";
            public const string AdstreamOwnerLabel = "CM_Prime_ADS";
        }

        public class Region
        {
            public const string NorthAmericanArea = "NORTHERN AMERICA AREA";
        }

        public class Smo
        {
            public const string WesternEurope = "OTHER EUROPE";
        }

        public static class DictionaryNames
        {
            public const string CostType = "CostType";
            public const string ContentType = "ContentType";
            public const string ProductionType = "ProductionType";
            public const string MediaType = "MediaType/TouchPoints";
            public const string OvalType = "OvalType";
            public const string UsageType = "UsageType";
            public const string UsageBuyoutType = "UsageBuyoutType";
        }

        public static class Miscellaneous
        {
            public const string NotApplicable = "N/A";
        }

        public static class UsageBuyoutType
        {
            public const string Contract = "Contract";
            public const string Buyout = "Buyout";
            public const string Usage = "Usage";
        }

        public static class UsageType
        {
            public const string BrandResidual = "Brand Residuals SAG (USA only)";
            public const string VoiceOver = "Voice-Over";
            public const string CountryAiringRights = "Country Airing Rights(Brazil Russia and Ukraine Only)";
            public const string Ilustrator = "Ilustrator";
            public const string Footage = "Footage(stock pack-shots UGC  etc)";
            public const string Actor = "Actor";
            public const string Music = "Music";
            public const string Film = "Film";
            public const string Organization = "Organization";
            public const string Athletes = "Athletes";
            public const string Model = "Model";
            public const string Celebrity = "Celebrity";
            public const string Photography = "Photography";
        }

        public static class BillingExpenseItem
        {
            public const string NumberOfMonthsFY = "noOfMonthsFY";
            public const string BalancePrepaid = "balancePrepaid";
            public const string OtherCosts = "otherCosts";
            public const string AgencyFee = "agencyFee";
            public const string Bonus = "bonus";
            public const string PensionAndHealth = "pensionAndHealth";
            public const string UsageBuyoutFee = "usageBuyoutFee";
        }

        public static class BillingExpenseSection
        {
            public const string Header = "header";
            public const string ContractTerms = "contractTerms";
            public const string IncurredCosts = "incurredCosts";
        }

        public static class BillingExpenseSectionTotal
        {
            public const string ContractTerms = "totalContractTerms";
            public const string IncurredCosts = "totalIncurredCosts";
        }

        public static class BillingExpenseSummaryItem
        {
            public const string TotalContractTermsAndIncurredCosts = "totalContractTermsAndIncurredCosts";
            public const string ExpensePerFY = "expensePerFY";
            public const string BalanceToBeMoved = "balanceToBeMoved";
        }

        public static class Rules
        {
            public static class Stage
            {
                public const string SkipFirstPresentation = "SkipFirstPresentation";
            }
        }

        public static class CostType
        {
            /// <summary>
            /// Normal production cost
            /// </summary>
            public const string Production = "Production";

            /// <summary>
            /// usage/buyout cost
            /// </summary>
            public const string Buyout = "Buyout";

            /// <summary>
            /// trafficking/distribution cost
            /// </summary>
            public const string Trafficking = "Trafficking";
        }

        public static class PurchaseOrder
        {
            public const string VendorSapIdLabelPrefix = "PGSAPVENDORID_";
        }

        public static readonly string[] RevisionStages = {
            CostStages.OriginalEstimateRevision.ToString(),
            CostStages.FirstPresentationRevision.ToString()
        };

        /// <summary>
        /// Dont have payment calculated at these stages
        /// </summary>
        public static readonly CostStageRevisionStatus[] SkippedStatusForPayment = new[] { CostStageRevisionStatus.PendingReopen, CostStageRevisionStatus.Reopened, CostStageRevisionStatus.ReopenRejected };
        /// <summary>
        /// Approved statues after user submit the cost
        /// </summary>
        public static readonly CostStageRevisionStatus[] ApprovedStatuses = new[] { CostStageRevisionStatus.Approved, CostStageRevisionStatus.PendingBrandApproval, CostStageRevisionStatus.PendingTechnicalApproval };
    }
}
