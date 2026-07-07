using Microsoft.Extensions.DependencyInjection;

namespace ModernBoxes.Infrastructure.Ai
{
    public static class AiServiceCollectionExtensions
    {
        public static IServiceCollection AddModernBoxesAi(this IServiceCollection services)
        {
            services.AddSingleton<ChatClientService>();
            services.AddSingleton<AiPromptService>();
            return services;
        }
    }
}
