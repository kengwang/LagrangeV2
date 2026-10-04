using System.Globalization;
using FastEndpoints;
using Lagrange_Milky;

namespace Lagrange.Milky.Extensions;

/// <summary>Configures request binding without runtime-generated scalar parsers.</summary>
public static class MilkyBindingExtensions
{
    /// <summary>Registers generated JSON and DTO metadata and statically compiled scalar parsers.</summary>
    /// <param name="configuration">The FastEndpoints application configuration.</param>
    public static void ConfigureMilkyBinding(this Config configuration)
    {
        configuration.Serializer.Options.AddSerializerContextsFromLagrange_Milky();
        configuration.Binding.ReflectionCache.AddFromLagrangeMilky();

        // FastEndpoints' fallback builds an Expression.Convert(StringValues, string)
        // for TryParse methods. Its implicit operator can be trimmed in Native AOT.
        // Register the scalar types used by our DTOs explicitly, including nullable
        // properties (FastEndpoints looks up their underlying type).
        Register<int>(configuration.Binding);
        Register<uint>(configuration.Binding);
        Register<long>(configuration.Binding);
        Register<ulong>(configuration.Binding);
        Register<bool>(configuration.Binding);
        Register<DateTime>(configuration.Binding);
    }

    private static void Register<T>(BindingOptions binding) where T : IParsable<T>
        => binding.ValueParserFor<T>(static input =>
            new(T.TryParse(input.ToString(), CultureInfo.InvariantCulture, out var result), result));
}
