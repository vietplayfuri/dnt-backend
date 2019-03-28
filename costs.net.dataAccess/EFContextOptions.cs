namespace dnt.dataAccess
{
    using System;
    using Microsoft.EntityFrameworkCore;

    public class EFContextOptions
    {
        public DbContextOptions DbContextOptions { get; set; }

        public long SystemUserId { get; set; }

        public bool IsLoggingEnabled { get; set; }
    }
}
