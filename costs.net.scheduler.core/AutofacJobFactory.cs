using Autofac;
using FluentScheduler;

namespace costs.net.scheduler.core
{
    public class AutofacJobFactory : IJobFactory
    {
        private readonly IContainer _container;

        public AutofacJobFactory(IContainer container)
        {
            _container = container;
        }

        public IJob GetJobInstance<T>() where T : IJob
        {
            return _container.Resolve<T>();
        }
    }
}
