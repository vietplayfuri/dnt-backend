using System;

namespace dnt.dataAccess.Entity
{
    public interface IModifiable
    {
        long CreatedById { get; set; }

        DateTime Created { get; set; }

        DateTime? Modified { get; set; }
    }
}
