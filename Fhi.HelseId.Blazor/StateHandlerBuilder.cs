using Microsoft.Extensions.DependencyInjection;

namespace Fhi.HelseId.Blazor
{
    public class StateHandlerBuilder
    {
        public StateHandlerOptions Options;

        private IServiceCollection Services;

        public StateHandlerBuilder(IServiceCollection services, StateHandlerOptions options)
        {
            Options = options;
            Services = services;

            services.AddSingleton(Options);
            services.AddScoped<IStateHandler, StateHandler>();
        }

        public StateHandlerBuilder AddScopedState<T>()
            where T : class, IScopedState
        {
            Options.StateTypes.Add(typeof(T));
            Services.AddScoped<T>();
            return this;
        }
    }

    public class StateHandlerOptions
    {
        public List<Type> StateTypes = new();
    }
}
