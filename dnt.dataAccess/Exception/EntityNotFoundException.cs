namespace dnt.dataAccess.Exception
{
    using System;
    using Entity;

    public class EntityNotFoundException<TEntity> : Exception 
        where TEntity : IEntity
    {
        private readonly Guid _id;
        private const string Template = "Couldn't find '{0}' with id '{1}'";
        private readonly string _customMessage;

        public EntityNotFoundException()
        { }

        public EntityNotFoundException(Guid id)
        {
            _id = id;
        }

        public EntityNotFoundException(string message)
        {
            _customMessage = message;
        }

        public override string Message => _customMessage ?? string.Format(Template, typeof(IEntity).Name, _id);
    }
}
