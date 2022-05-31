﻿using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Scriban;

namespace OpenIddict.Client.WebIntegration.Generators
{
    [Generator]
    public class OpenIddictClientWebIntegrationGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            var file = context.AdditionalFiles.Select(file => file.Path)
                .Where(path => string.Equals(Path.GetFileName(path), "OpenIddictClientWebIntegrationProviders.xml"))
                .SingleOrDefault();

            if (string.IsNullOrEmpty(file))
            {
                return;
            }

            var document = XDocument.Load(file, LoadOptions.None);

            context.AddSource(
                "OpenIddictClientWebIntegrationBuilder.generated.cs",
                SourceText.From(GenerateBuilderMethods(document), Encoding.UTF8));

            context.AddSource(
                "OpenIddictClientWebIntegrationConfiguration.generated.cs",
                SourceText.From(GenerateConfigurationClasses(document), Encoding.UTF8));

            context.AddSource(
                "OpenIddictClientWebIntegrationConstants.generated.cs",
                SourceText.From(GenerateConstants(document), Encoding.UTF8));

            context.AddSource(
                "OpenIddictClientWebIntegrationEnvironments.generated.cs",
                SourceText.From(GenerateEnvironments(document), Encoding.UTF8));

            context.AddSource(
                "OpenIddictClientWebIntegrationHelpers.generated.cs",
                SourceText.From(GenerateHelpers(document), Encoding.UTF8));

            context.AddSource(
                "OpenIddictClientWebIntegrationScopes.generated.cs",
                SourceText.From(GenerateScopes(document), Encoding.UTF8));

            context.AddSource(
                "OpenIddictClientWebIntegrationSettings.generated.cs",
                SourceText.From(GenerateSettings(document), Encoding.UTF8));

            static string GenerateBuilderMethods(XDocument document)
            {
                var template = Template.Parse(@"#nullable enable

using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OpenIddict.Client;
using OpenIddict.Client.WebIntegration;
using static OpenIddict.Client.WebIntegration.OpenIddictClientWebIntegrationConstants;

namespace Microsoft.Extensions.DependencyInjection;

public partial class OpenIddictClientWebIntegrationBuilder
{
    {{~ for provider in providers ~}}
    /// <summary>
    /// Enables {{ provider.name }} integration using the specified settings.
    /// </summary>
    {{~ if provider.documentation ~}}
    /// <remarks>
    /// For more information about {{ provider.name }} integration, visit <see href=""{{ provider.documentation }}"">the official website</see>.
    /// </remarks>
    {{~ end ~}}
    /// <param name=""settings"">The provider settings.</param>
    /// <returns>The <see cref=""OpenIddictClientWebIntegrationBuilder""/>.</returns>
    public OpenIddictClientWebIntegrationBuilder Add{{ provider.name }}(OpenIddictClientWebIntegrationSettings.{{ provider.name }} settings)
    {
        if (settings is null)
        {
            throw new ArgumentNullException(nameof(settings));
        }

        // Note: TryAddEnumerable() is used here to ensure the initializer is registered only once.
        Services.TryAddEnumerable(ServiceDescriptor.Singleton<
            IConfigureOptions<OpenIddictClientOptions>, OpenIddictClientWebIntegrationConfiguration.{{ provider.name }}>());

        return Configure(options => options.Providers.Add(new OpenIddictClientWebIntegrationProvider(Providers.{{ provider.name }}, settings)));
    }
    {{~ end ~}}
}
");
                return template.Render(new
                {
                    Providers = document.Root.Elements("Provider")
                        .Select(provider => new
                        {
                            Name = (string) provider.Attribute("Name"),
                            Documentation = (string?) provider.Attribute("Documentation")
                        })
                        .ToList()
                });
            }

            static string GenerateConstants(XDocument document)
            {
                var template = Template.Parse(@"#nullable enable

namespace OpenIddict.Client.WebIntegration;

public static partial class OpenIddictClientWebIntegrationConstants
{
    public static class Providers
    {
        {{~ for provider in providers ~}}
        public const string {{ provider.name }} = ""{{ provider.name }}"";
        {{~ end ~}}
    }
}
");
                return template.Render(new
                {
                    Providers = document.Root.Elements("Provider")
                        .Select(provider => new { Name = (string) provider.Attribute("Name") })
                        .ToList()
                });
            }

            static string GenerateEnvironments(XDocument document)
            {
                var template = Template.Parse(@"#nullable enable

namespace OpenIddict.Client.WebIntegration;

public partial class OpenIddictClientWebIntegrationEnvironments
{
    {{~ for provider in providers ~}}
    /// <summary>
    /// Exposes the environments supported by the {{ provider.name }} provider.
    /// </summary>
    public enum {{ provider.name }}
    {
        {{~ for environment in provider.environments ~}}
        {{ environment.name }},
        {{~ end ~}}
    }
    {{~ end ~}}
}
");
                return template.Render(new
                {
                    Providers = document.Root.Elements("Provider")
                        .Select(provider => new
                        {
                            Name = (string) provider.Attribute("Name"),

                            Environments = provider.Elements("Environment").Select(environment => new
                            {
                                Name = (string?) environment.Attribute("Name") ?? "Production"
                            })
                            .ToList()
                        })
                        .ToList()
                });
            }

            static string GenerateConfigurationClasses(XDocument document)
            {
                var template = Template.Parse(@"#nullable enable

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Client;
using SmartFormat;
using SmartFormat.Core.Settings;
using Properties = OpenIddict.Client.WebIntegration.OpenIddictClientWebIntegrationConstants.Properties;
using static OpenIddict.Client.WebIntegration.OpenIddictClientWebIntegrationConstants;

namespace OpenIddict.Client.WebIntegration;

public partial class OpenIddictClientWebIntegrationConfiguration
{
    {{~ for provider in providers ~}}
    /// <summary>
    /// Contains the methods required to register the {{ provider.name }} integration in the OpenIddict client options.
    /// </summary>
    public class {{ provider.name }} : IConfigureOptions<OpenIddictClientOptions>
    {
        private readonly IOptions<OpenIddictClientWebIntegrationOptions> _options;

        /// <summary>
        /// Creates a new instance of the <see cref=""OpenIddictClientWebIntegrationConfiguration.{{ provider.name }}"" /> class.
        /// </summary>
        /// <param name=""options"">The OpenIddict client web integration options.</param>
        /// <exception cref=""ArgumentException""><paramref name=""options""/> is null.</exception>
        public {{ provider.name }}(IOptions<OpenIddictClientWebIntegrationOptions> options)
            => _options = options ?? throw new ArgumentNullException(nameof(options));

        /// <summary>
        /// Ensures the {{ provider.name }} configuration is in a consistent and valid state
        /// and registers the {{ provider.name }} integration in the OpenIddict client options.
        /// </summary>
        /// <param name=""options"">The options instance to initialize.</param>
        public void Configure(OpenIddictClientOptions options)
        {
            foreach (var provider in _options.Value.Providers)
            {
                if (provider.Name is not Providers.{{ provider.name }})
                {
                    continue;
                }

                if (provider.Settings is not OpenIddictClientWebIntegrationSettings.{{ provider.name }} settings)
                {
                    throw new InvalidOperationException(SR.FormatID0331(Providers.{{ provider.name }}));
                }

                if (string.IsNullOrEmpty(settings.ClientId))
                {
                    throw new InvalidOperationException(SR.FormatID0332(nameof(settings.ClientId), Providers.{{ provider.name }}));
                }

                if (settings.RedirectUri is null)
                {
                    throw new InvalidOperationException(SR.FormatID0332(nameof(settings.RedirectUri), Providers.{{ provider.name }}));
                }

                {{~ for setting in provider.settings ~}}
                {{~ if setting.required ~}}
                {{~ if setting.type == 'string' ~}} 
                if (string.IsNullOrEmpty(settings.{{ setting.name }}))
                {{~ else ~}}
                if (settings.{{ setting.name }} is null)
                {{~ end ~}}
                {
                    throw new InvalidOperationException(SR.FormatID0332(nameof(settings.{{ setting.name }}), Providers.{{ provider.name }}));
                }
                {{~ end ~}}
                {{~ end ~}}

                var formatter = Smart.CreateDefaultSmartFormat(new SmartSettings
                {
                    CaseSensitivity = CaseSensitivityType.CaseInsensitive
                });

                var registration = new OpenIddictClientRegistration
                {
                    Issuer = settings.Environment switch
                    {
                        {{~ for environment in provider.environments ~}}
                        OpenIddictClientWebIntegrationEnvironments.{{ provider.name }}.{{ environment.name }}
                            => new Uri(formatter.Format(""{{ environment.issuer }}"", settings), UriKind.Absolute),
                        {{~ end ~}}

                        _ => throw new InvalidOperationException(SR.FormatID0194(nameof(settings.Environment)))
                    },

                    ClientId = settings.ClientId,
                    ClientSecret = settings.ClientSecret,
                    RedirectUri = settings.RedirectUri,

                    Configuration = settings.Environment switch
                    {
                        {{~ for environment in provider.environments ~}}
                        {{~ if environment.configuration ~}}
                        OpenIddictClientWebIntegrationEnvironments.{{ provider.name }}.{{ environment.name }} => new OpenIddictConfiguration
                        {
                            {{~ if environment.configuration.authorization_endpoint ~}}
                            AuthorizationEndpoint = new Uri(formatter.Format(""{{ environment.configuration.authorization_endpoint }}"", settings), UriKind.Absolute),
                            {{~ end ~}}

                            {{~ if environment.configuration.token_endpoint ~}}
                            TokenEndpoint = new Uri(formatter.Format(""{{ environment.configuration.token_endpoint }}"", settings), UriKind.Absolute),
                            {{~ end ~}}

                            {{~ if environment.configuration.userinfo_endpoint ~}}
                            UserinfoEndpoint = new Uri(formatter.Format(""{{ environment.configuration.userinfo_endpoint }}"", settings), UriKind.Absolute),
                            {{~ end ~}}

                            CodeChallengeMethodsSupported =
                            {
                                {{~ for method in environment.configuration.code_challenge_methods_supported ~}}
                                ""{{ method }}"",
                                {{~ end ~}}
                            },

                            GrantTypesSupported =
                            {
                                {{~ for type in environment.configuration.grant_types_supported ~}}
                                ""{{ type }}"",
                                {{~ end ~}}
                            },

                            ResponseModesSupported =
                            {
                                {{~ for mode in environment.configuration.response_modes_supported ~}}
                                ""{{ mode }}"",
                                {{~ end ~}}
                            },

                            ResponseTypesSupported =
                            {
                                {{~ for type in environment.configuration.response_types_supported ~}}
                                ""{{ type }}"",
                                {{~ end ~}}
                            },

                            ScopesSupported =
                            {
                                {{~ for scope in environment.configuration.scopes_supported ~}}
                                ""{{ scope }}"",
                                {{~ end ~}}
                            },

                            TokenEndpointAuthMethodsSupported =
                            {
                                {{~ for method in environment.configuration.token_endpoint_auth_methods_supported ~}}
                                ""{{ method }}"",
                                {{~ end ~}}
                            }
                        },
                        {{~ else ~}}
                        OpenIddictClientWebIntegrationEnvironments.{{ provider.name }}.{{ environment.name }} => null,
                        {{~ end ~}}
                        {{~ end ~}}

                        _ => throw new InvalidOperationException(SR.FormatID0194(nameof(settings.Environment)))
                    },

                    EncryptionCredentials =
                    {
                        {{~ for setting in provider.settings ~}}
                        {{~ if setting.encryption_algorithm ~}}
                        new EncryptingCredentials(settings.{{ setting.name }}, ""{{ setting.encryption_algorithm }}"", SecurityAlgorithms.Aes256CbcHmacSha512),
                        {{~ end ~}}
                        {{~ end ~}}
                    },

                    SigningCredentials =
                    {
                        {{~ for setting in provider.settings ~}}
                        {{~ if setting.signing_algorithm ~}}
                        new SigningCredentials(settings.{{ setting.name }}, ""{{ setting.signing_algorithm }}""),
                        {{~ end ~}}
                        {{~ end ~}}
                    },

                    Properties =
                    {
                        [Properties.ProviderName] = Providers.{{ provider.name }},
                        [Properties.ProviderSettings] = settings
                    }
                };

                registration.Scopes.UnionWith(settings.Scopes);

                {{~ for environment in provider.environments ~}}
                if (settings.Environment is OpenIddictClientWebIntegrationEnvironments.{{ provider.name }}.{{ environment.name }})
                {
                    {{~ for scope in environment.scopes ~}}
                    {{~ if scope.required ~}}
                    registration.Scopes.Add(""{{ scope.name }}"");
                    {{~ end ~}}

                    {{~ if scope.default ~}}
                    if (registration.Scopes.Count is 0)
                    {
                        registration.Scopes.Add(""{{ scope.name }}"");
                    }
                    {{~ end ~}}
                    {{~ end ~}}
                }
                {{~ end ~}}

                options.Registrations.Add(registration);
            }
        }
    }
    {{~ end ~}}
}
");
                return template.Render(new
                {
                    Providers = document.Root.Elements("Provider")
                        .Select(provider => new
                        {
                            Name = (string) provider.Attribute("Name"),

                            Environments = provider.Elements("Environment").Select(environment => new
                            {
                                Name = (string?) environment.Attribute("Name") ?? "Production",
                                Issuer = (string) environment.Attribute("Issuer"),
                                Configuration = environment.Element("Configuration") switch
                                {
                                    XElement configuration => new
                                    {
                                        AuthorizationEndpoint = (string?) configuration.Attribute("AuthorizationEndpoint"),
                                        TokenEndpoint = (string?) configuration.Attribute("TokenEndpoint"),
                                        UserinfoEndpoint = (string?) configuration.Attribute("UserinfoEndpoint"),

                                        CodeChallengeMethodsSupported = configuration.Elements("CodeChallengeMethod").ToList() switch
                                        {
                                            { Count: > 0 } methods => methods.Select(type => (string?) type.Attribute("Value")).ToList(),

                                            _ => (IList<string>) Array.Empty<string>()
                                        },

                                        GrantTypesSupported = configuration.Elements("GrantType").ToList() switch
                                        {
                                            { Count: > 0 } types => types.Select(type => (string?) type.Attribute("Value")).ToList(),

                                            // If no explicit grant type was set, assume the provider only supports the code flow.
                                            _ => (IList<string>) new[] { GrantTypes.AuthorizationCode }
                                        },

                                        ResponseModesSupported = configuration.Elements("ResponseMode").ToList() switch
                                        {
                                            { Count: > 0 } modes => modes.Select(type => (string?) type.Attribute("Value")).ToList(),

                                            // If no explicit response mode was set, assume the provider only supports the query response mode.
                                            _ => (IList<string>) new[] { ResponseModes.Query }
                                        },

                                        ResponseTypesSupported = configuration.Elements("ResponseType").ToList() switch
                                        {
                                            { Count: > 0 } types => types.Select(type => (string?) type.Attribute("Value")).ToList(),

                                            // If no explicit response type was set, assume the provider only supports the code flow.
                                            _ => (IList<string>) new[] { ResponseTypes.Code }
                                        },

                                        ScopesSupported = configuration.Elements("Scope").ToList() switch
                                        {
                                            { Count: > 0 } types => types.Select(type => (string?) type.Attribute("Value")).ToList(),

                                            _ => (IList<string>) Array.Empty<string>()
                                        },

                                        TokenEndpointAuthMethodsSupported = configuration.Elements("TokenEndpointAuthMethod").ToList() switch
                                        {
                                            { Count: > 0 } methods => methods.Select(type => (string?) type.Attribute("Value")).ToList(),

                                            // If no explicit response type was set, assume the provider only supports
                                            // flowing the client credentials as part of the token request payload.
                                            _ => (IList<string>) new[] { ClientAuthenticationMethods.ClientSecretPost }
                                        }
                                    },

                                    _ => null
                                },

                                Scopes = environment.Elements("Scope").Select(setting => new
                                {
                                    Name = (string) setting.Attribute("Name"),
                                    Default = (bool?) setting.Attribute("Default") ?? false,
                                    Required = (bool?) setting.Attribute("Required") ?? false
                                })
                            })
                            .ToList(),

                            Settings = provider.Elements("Setting").Select(setting => new
                            {
                                Name = (string) setting.Attribute("Name"),
                                Type = (string) setting.Attribute("Type"),
                                Required = (bool?) setting.Attribute("Required") ?? false,
                                EncryptionAlgorithm = (string?) setting.Attribute("EncryptionAlgorithm"),
                                SigningAlgorithm = (string?) setting.Attribute("SigningAlgorithm")
                            })
                            .ToList()
                        })
                        .ToList()
                });
            }

            static string GenerateHelpers(XDocument document)
            {
                var template = Template.Parse(@"#nullable enable

using Microsoft.IdentityModel.Tokens;
using OpenIddict.Client;
using OpenIddict.Client.WebIntegration;
using SmartFormat;
using SmartFormat.Core.Settings;
using Properties = OpenIddict.Client.WebIntegration.OpenIddictClientWebIntegrationConstants.Properties;
using static OpenIddict.Client.WebIntegration.OpenIddictClientWebIntegrationConstants;

namespace OpenIddict.Client.WebIntegration;

public partial class OpenIddictClientWebIntegrationHelpers
{
    {{~ for provider in providers ~}}
    /// <summary>
    /// Resolves the {{ provider.name }} provider settings from the specified registration.
    /// </summary>
    /// <param name=""registration"">The client registration.</param>
    /// <returns>The {{ provider.name }} provider settings.</returns>
    /// <exception cref=""InvalidOperationException"">The provider settings cannot be resolved.</exception>
    public static OpenIddictClientWebIntegrationSettings.{{ provider.name }} Get{{ provider.name }}Settings(this OpenIddictClientRegistration registration)
        => registration.GetProviderSettings<OpenIddictClientWebIntegrationSettings.{{ provider.name }}>() ??
            throw new InvalidOperationException(SR.FormatID0333(Providers.{{ provider.name }}));
    {{~ end ~}}
}
");
                return template.Render(new
                {
                    Providers = document.Root.Elements("Provider")
                        .Select(provider => new { Name = (string) provider.Attribute("Name") })
                        .ToList()
                });
            }

            static string GenerateScopes(XDocument document)
            {
                var template = Template.Parse(@"#nullable enable

namespace OpenIddict.Client.WebIntegration;

public static partial class OpenIddictClientWebIntegrationScopes
{
    {{~ for provider in providers ~}}
    /// <summary>
    /// Exposes the scopes supported by the {{ provider.name }} provider.
    /// </summary>
    public static class {{ provider.name }}
    {
        {{~ for scope in provider.scopes ~}}
        {{~ if scope.description ~}}
        /// <summary>
        /// {{ scope.description }}
        /// </summary>
        {{~ end ~}}
        public const string {{ scope.clr_name }} = ""{{ scope.name }}"";
        {{~ end ~}}
    }
    {{~ end ~}}
}
");
                return template.Render(new
                {
                    Providers = document.Root.Elements("Provider")
                        .Select(provider => new
                        {
                            Name = (string) provider.Attribute("Name"),

                            Scopes = provider.Elements("Environment")
                                .SelectMany(environment => environment.Elements("Scope"))
                                .Select(scope => new
                                {
                                    Name = (string) scope.Attribute("Name"),
                                    ClrName = Regex.Replace((string) scope.Attribute("Name"), "(?:^|_| +)(.)",
                                        match => match.Groups[1].Value.ToUpper(CultureInfo.InvariantCulture)),
                                    Description = (string?) scope.Attribute("Description")
                                })
                                .Distinct(scope => scope.ClrName)
                                .ToList()
                        })
                        .ToList()
                });
            }

            static string GenerateSettings(XDocument document)
            {
                var template = Template.Parse(@"#nullable enable

using Microsoft.IdentityModel.Tokens;

namespace OpenIddict.Client.WebIntegration;

public partial class OpenIddictClientWebIntegrationSettings
{
    {{~ for provider in providers ~}}
    /// <summary>
    /// Provides various settings needed to configure the {{ provider.name }} integration.
    /// </summary>
    public class {{ provider.name }} : OpenIddictClientWebIntegrationSettings
    {
        {{~ for setting in provider.settings ~}}
        {{~ if setting.description ~}}
        /// <summary>
        /// {{ setting.description }}
        /// </summary>
        {{~ end ~}}
        public {{ setting.type }}? {{ setting.name }} { get; set; }
        {{~ end ~}}

        /// <summary>
        /// Gets or sets the environment that determines the endpoints to use.
        /// </summary>
        public OpenIddictClientWebIntegrationEnvironments.{{ provider.name }} Environment { get; set; }
    }
    {{~ end ~}}
}
");
                return template.Render(new
                {
                    Providers = document.Root.Elements("Provider")
                        .Select(provider => new
                        {
                            Name = (string) provider.Attribute("Name"),

                            Settings = provider.Elements("Setting").Select(setting => new
                            {
                                Type = (string) setting.Attribute("Type"),
                                Name = (string) setting.Attribute("Name"),
                                Description = (string) setting.Attribute("Description")
                            })
                            .ToList()
                        })
                        .ToList()
                });
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }
    }
}
