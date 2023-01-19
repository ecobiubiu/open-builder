﻿/*
 * Licensed under the Apache License, Version 2.0 (http://www.apache.org/licenses/LICENSE-2.0)
 * See https://github.com/openiddict/openiddict-core for more information concerning
 * the license and the contributors participating to this project.
 */

using System.ComponentModel;
using Microsoft.Extensions.Logging;

namespace OpenIddict.Validation;

/// <summary>
/// Represents the context associated with an OpenID Connect validation request.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Advanced)]
public sealed class OpenIddictValidationTransaction
{
    /// <summary>
    /// Gets or sets the cancellation token that will be
    /// used to determine if the operation was aborted.
    /// </summary>
    /// <remarks>
    /// Note: for security reasons, this property shouldn't be used by event
    /// handlers to abort security-sensitive operations. As such, it is
    /// recommended to use this property only for user-dependent operations.
    /// </remarks>
    public CancellationToken CancellationToken { get; set; }

    /// <summary>
    /// Gets or sets the type of the endpoint processing the current request.
    /// </summary>
    public OpenIddictValidationEndpointType EndpointType { get; set; }

    /// <summary>
    /// Gets or sets the request <see cref="Uri"/> of the current transaction, if available.
    /// </summary>
    public Uri? RequestUri { get; set; }

    /// <summary>
    /// Gets or sets the base <see cref="Uri"/> of the host, if available.
    /// </summary>
    public Uri? BaseUri { get; set; }

    /// <summary>
    /// Gets or sets the logger associated with the current request.
    /// </summary>
    public ILogger Logger { get; set; } = default!;

    /// <summary>
    /// Gets or sets the options associated with the current request.
    /// </summary>
    public OpenIddictValidationOptions Options { get; set; } = default!;

    /// <summary>
    /// Gets the additional properties associated with the current request.
    /// </summary>
    public Dictionary<string, object?> Properties { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets or sets the server configuration used for the current request.
    /// </summary>
    public OpenIddictConfiguration Configuration { get; set; } = default!;

    /// <summary>
    /// Gets or sets the current OpenID Connect request.
    /// </summary>
    public OpenIddictRequest? Request { get; set; }

    /// <summary>
    /// Gets or sets the current OpenID Connect response being returned.
    /// </summary>
    public OpenIddictResponse? Response { get; set; }
}
