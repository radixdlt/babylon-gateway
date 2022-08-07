using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace RadixDlt.NetworkGateway.Configuration;

public static class Extensions
{
    public static IServiceCollection AddValidatableOptionsAtSection<TOptions, TValidator>(this IServiceCollection services, string configSectionPath)
        where TOptions : class
        where TValidator : AbstractOptionsValidator<TOptions>
    {
        services
            .AddOptions<TOptions>()
            .ValidateUsing<TOptions, TValidator>()
            .ValidateOnStart()
            .BindConfiguration(configSectionPath);

        return services;
    }

    private static OptionsBuilder<TOptions> ValidateUsing<TOptions, TValidator>(this OptionsBuilder<TOptions> optionsBuilder)
        where TOptions : class
        where TValidator : AbstractOptionsValidator<TOptions>
    {
        optionsBuilder.Services.AddSingleton<IValidateOptions<TOptions>, TValidator>();

        return optionsBuilder;
    }
}
