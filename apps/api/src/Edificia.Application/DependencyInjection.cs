using Edificia.Application.Behaviors;
using Edificia.Application.Export.Services;
using Edificia.Application.Interfaces;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Edificia.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        // Register MediatR + pipeline behaviors
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(assembly);
            config.AddOpenBehavior(typeof(LoggingBehavior<,>));
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        // Register all FluentValidation validators from this assembly
        services.AddValidatorsFromAssembly(assembly);

        // Application services
        services.AddScoped<ITemplateParameterResolver, TemplateParameterResolver>();
        services.AddScoped<ITemplatePlaceholderService, TemplatePlaceholderService>();

        return services;
    }
}
