namespace costs.net.integration.tests.Builders
{
    public abstract class BaseBuilder<T>
        where T : class, new()
    {
        protected readonly T Object;

        protected BaseBuilder()
        {
            Object = new T();
        }

        public T Build()
        {
            return Object;
        }
    }
}