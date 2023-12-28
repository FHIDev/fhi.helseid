using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Fhi.HelseId.Blazor
{
    public class StateHandlerBuilder
    {
        public StateHandlerOptions Options;

        private WebApplicationBuilder Builder;

        public StateHandlerBuilder(WebApplicationBuilder builder, StateHandlerOptions options)
        {
            Options = options;
            Builder = builder;

            builder.Services.AddSingleton(Options);
            builder.Services.AddScoped<IStateHandler, StateHandler>();
        }

        public StateHandlerBuilder AddScopedState<T>() where T : class, IScopedState
        {
            Options.StateTypes.Add(typeof(T));
            Builder.Services.AddScoped<T>();
            return this;
        }
    }

    public class StateHandlerOptions
    {
        public List<Type> StateTypes = new();
    }
}
