namespace costs.net.dataAccess.Views
{
    public class SchemaVersion
    {
        public virtual int InstalledRank { get; set; }

        public virtual int ExecutionTime { get; set; }

        public virtual string Version { get; set; }

        public virtual string Description { get; set; }

        public virtual string Script { get; set; }
    }
}