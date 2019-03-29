using System;
using dnt.dataAccess.Entity;

namespace dnt.dataAccess.Extensions
{
    public static class ModifiableExtensions
    {
        public static T SetModifiedNow<T>(this T entity)
            where T : IModifiable
        {
            entity.Modified = DateTime.UtcNow;
            return entity;
        }

        public static T SetCreatedNow<T>(this T entity, long userId)
            where T : IModifiable
        {
            SetModifiedNow(entity);
            entity.Created = DateTime.UtcNow;
            entity.CreatedById = userId;
            return entity;
        }
    }
}
