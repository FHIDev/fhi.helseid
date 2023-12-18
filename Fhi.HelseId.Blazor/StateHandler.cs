using Microsoft.Extensions.DependencyInjection;

namespace Fhi.HelseId.Blazor
{
    public interface IStateHandler
    {
        Task Populate();
    }

    public class StateHandler : IStateHandler
    {
        private readonly StateHandlerOptions options;
        private readonly BlazorContextHandler contextHandler;
        private readonly IServiceProvider provider;

        public StateHandler(StateHandlerOptions options, BlazorContextHandler contextHandler, IServiceProvider provider)
        {
            this.options = options;
            this.provider = provider;
            this.contextHandler = contextHandler;
        }

        public async Task Populate()
        {
            await contextHandler.NewContext(async (context) =>
            {
                foreach (var stateType in options.StateTypes)
                {
                    var state = (IScopedState)provider.GetRequiredService(stateType);
                    await state.Populate(context);
                }
            });
        }
    }
}
