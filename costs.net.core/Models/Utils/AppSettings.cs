namespace dnt.core.Models.Utils
{
    public class AppSettings
    {
        public string ServiceUrl { get; set; }
        public string FileServiceUrl { get; set; }
        public string AdminUser { get; set; }
        public string AdminAgency { get; set; }
        public string GdamCoreHost { get; set; }
        public string GdnHost { get; set; }
        public string BuildNumber { get; set; }
        public string GitBranch { get; set; }
        public string GitCommit { get; set; }
        public string GdnUseSsl { get; set; }
        public string FrontendUrl { get; set; }
        public string GdamFrontendUrl { get; set; }
        public string SupportEmailAddress { get; set; }
        public string CostsAdminUserId { get; set; }
        public string[] BrandPrefix { get; set; }
        public string[] ClientTags { get; set; }
        public string HostName { get; set; }
        public string CoupaApprovalEmail { get; set; }
        public int? ActivityLogMaxRetry { get; set; }
        public int? ActivityLogAgeInDays { get; set; }
        public int? ActivityLogPageSize { get; set; }
        public int ElasticBatchSize { get; set; }
        public string EnvironmentEmailSubjectPrefix { get; set; }
        public int MaxCostsInProjectExport { get; set; }
        /// <summary>
        /// Max size of the file that can be uploaded in MB
        /// </summary>
        public long MaxFileUploadSize { get; set; }
    }
}