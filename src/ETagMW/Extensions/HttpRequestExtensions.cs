// ***********************************************************************
//  Assembly          : RzR.MiddleWares.ETagMW
//  Author            : RzR
//  Created           : 07-05-2026 17:05
// 
//  Last Modified By : RzR
//  Last Modified On : 13-05-2026 23:05
//  ***********************************************************************
//  <copyright file="HttpRequestExtensions.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      https://iamrzr.dev/contact
//  </contact>
//  <summary></summary>
//  ***********************************************************************

#region U S I N G

using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using RzR.Web.Middleware.ETag.Abstractions;
using RzR.Web.Middleware.ETag.Options;
using RzR.Web.Middleware.ETag.Internal;

#endregion

namespace RzR.Web.Middleware.ETag.Extensions
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     HttpRequest extension.
    /// </summary>
    /// =================================================================================================
    internal static class HttpRequestExtensions
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Check if current request can be processed for ETag generation.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when one or more required arguments are null.
        /// </exception>
        /// <param name="request">Current HTTP request.</param>
        /// <param name="context">Current HTTP context.</param>
        /// <param name="option">Current ETag options.</param>
        /// <returns>
        ///     True if we can process etag, false if not.
        /// </returns>
        /// =================================================================================================
        internal static bool CanProcessEtag(this HttpRequest request, HttpContext context, ETagOption option)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (option == null)
                throw new ArgumentNullException(nameof(option));

            if (!option.SupportsMethod(request.Method))
                return false;

            if (request.Headers.ContainsKey(HeaderNames.Range))
                return false;

            if (context.WebSockets.IsWebSocketRequest)
                return false;

            return true;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Single-pass walk of endpoint metadata that produces both the eligibility decision and the
        ///     effective per-request option (cloned only when overrides apply).
        /// </summary>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when one or more required arguments are null.
        /// </exception>
        /// <param name="context">Current HTTP context.</param>
        /// <param name="baseOption">Shared middleware option instance.</param>
        /// <returns>
        ///     An EndpointETagResolution.
        /// </returns>
        /// =================================================================================================
        internal static EndpointETagResolution ResolveETagResolution(this HttpContext context, ETagOption baseOption)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (baseOption == null)
                throw new ArgumentNullException(nameof(baseOption));

#if NET5_0_OR_GREATER
            var endpoint = context.GetEndpoint();

            if (endpoint == null)
                return new EndpointETagResolution(baseOption, baseOption.ApplyETagByDefault);

            bool? policyEnabled = null;
            ETagOption effectiveOption = null;

            foreach (var metadataItem in endpoint.Metadata)
            {
                if (metadataItem is IETagPolicyMetadata currentPolicy)
                    policyEnabled = currentPolicy.IsEnabled;

                if (metadataItem is IETagOptionsMetadata endpointOptionMetadata)
                {
                    effectiveOption ??= baseOption.Clone();
                    endpointOptionMetadata.Configure(effectiveOption);
                }

                if (metadataItem is IETagOptionsConfiguratorMetadata configuratorMetadata)
                {
                    var configurator = ResolveConfigurator(context, configuratorMetadata.ConfiguratorType);

                    effectiveOption ??= baseOption.Clone();
                    configurator.Configure(effectiveOption);
                }
            }

            return new EndpointETagResolution(
                effectiveOption ?? baseOption,
                policyEnabled ?? baseOption.ApplyETagByDefault);
#else
            return new EndpointETagResolution(baseOption, baseOption.ApplyETagByDefault);
#endif
        }

#if NET5_0_OR_GREATER

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Resolve configurator.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when the requested operation is invalid.
        /// </exception>
        /// <param name="context">The context.</param>
        /// <param name="configuratorType">Type of the configurator.</param>
        /// <returns>
        ///     An IETagOptionsConfigurator.
        /// </returns>
        /// =================================================================================================
        private static IETagOptionsConfigurator ResolveConfigurator(HttpContext context, Type configuratorType)
        {
            if (context.RequestServices == null)
                throw new InvalidOperationException(
                    "Request services are not available; cannot resolve IETagOptionsConfigurator. " +
                    "Ensure UseRouting() runs before UseETag() so endpoint metadata can be honored.");

            return (IETagOptionsConfigurator)context.RequestServices.GetRequiredService(configuratorType);
        }
#endif

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Check if the current request contains a matching If-None-Match header.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when one or more required arguments are null.
        /// </exception>
        /// <param name="request">Current HTTP request.</param>
        /// <param name="currentEtag">Current generated ETag.</param>
        /// <returns>
        ///     True if matches if none match, false if not.
        /// </returns>
        /// =================================================================================================
        internal static bool MatchesIfNoneMatch(this HttpRequest request, string currentEtag)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrWhiteSpace(currentEtag))
                return false;

            if (!request.Headers.TryGetValue(HeaderNames.IfNoneMatch, out var headerValues))
                return false;

            var normalizedCurrentEtag = NormalizeEntityTag(currentEtag);

            foreach (var headerValue in headerValues)
            foreach (var entityTag in SplitEntityTagValues(headerValue))
            {
                if (entityTag == "*")
                    return true;

                if (string.Equals(NormalizeEntityTag(entityTag), normalizedCurrentEtag, StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Check if the current request contains an If-Match header.
        /// </summary>
        /// <param name="request">Current HTTP request.</param>
        /// <returns>
        ///     True if if match header, false if not.
        /// </returns>
        /// =================================================================================================
        internal static bool HasIfMatchHeader(this HttpRequest request)
        {
            return HasHeaderValue(request, HeaderNames.IfMatch);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Check if the current request contains an If-None-Match header.
        /// </summary>
        /// <param name="request">Current HTTP request.</param>
        /// <returns>
        ///     True if if none match header, false if not.
        /// </returns>
        /// =================================================================================================
        internal static bool HasIfNoneMatchHeader(this HttpRequest request)
        {
            return HasHeaderValue(request, HeaderNames.IfNoneMatch);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Check if the current request contains a strong matching If-Match header.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when one or more required arguments are null.
        /// </exception>
        /// <param name="request">Current HTTP request.</param>
        /// <param name="currentEtag">Current generated ETag.</param>
        /// <returns>
        ///     True if matches if match, false if not.
        /// </returns>
        /// =================================================================================================
        internal static bool MatchesIfMatch(this HttpRequest request, string currentEtag)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrWhiteSpace(currentEtag))
                return false;

            if (!request.Headers.TryGetValue(HeaderNames.IfMatch, out var headerValues))
                return false;

            var normalizedCurrentEtag = NormalizeEntityTag(currentEtag);

            if (IsWeakEntityTag(currentEtag))
                return false;

            foreach (var headerValue in headerValues)
            foreach (var entityTag in SplitEntityTagValues(headerValue))
            {
                if (entityTag == "*")
                    return true;

                if (IsWeakEntityTag(entityTag))
                    continue;

                if (string.Equals(NormalizeEntityTag(entityTag), normalizedCurrentEtag, StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Check if the current request method supports 304 responses.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when one or more required arguments are null.
        /// </exception>
        /// <param name="request">Current HTTP request.</param>
        /// <returns>
        ///     True if safe conditional method, false if not.
        /// </returns>
        /// =================================================================================================
        internal static bool IsSafeConditionalMethod(this HttpRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            return HttpMethods.IsGet(request.Method) || HttpMethods.IsHead(request.Method);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Check if If-Modified-Since should return 304 for the current response.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when one or more required arguments are null.
        /// </exception>
        /// <param name="request">Current HTTP request.</param>
        /// <param name="lastModified">Current Last-Modified value.</param>
        /// <returns>
        ///     True if not modified since, false if not.
        /// </returns>
        /// =================================================================================================
        internal static bool IsNotModifiedSince(this HttpRequest request, DateTimeOffset lastModified)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (!request.IsSafeConditionalMethod())
                return false;

            if (!TryGetIfModifiedSince(request, out var ifModifiedSince))
                return false;

            return lastModified <= ifModifiedSince;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     An ETagOption extension method that supports method.
        /// </summary>
        /// <param name="option">Current ETag options.</param>
        /// <param name="method">The method.</param>
        /// <returns>
        ///     True if it succeeds, false if it fails.
        /// </returns>
        /// =================================================================================================
        private static bool SupportsMethod(this ETagOption option, string method)
        {
            if (option.SupportedMethods == null)
                return false;

            foreach (var supportedMethod in option.SupportedMethods)
            {
                if (string.Equals(supportedMethod, method, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Query if 'request' has header value.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when one or more required arguments are null.
        /// </exception>
        /// <param name="request">Current HTTP request.</param>
        /// <param name="headerName">Name of the header.</param>
        /// <returns>
        ///     True if header value, false if not.
        /// </returns>
        /// =================================================================================================
        private static bool HasHeaderValue(HttpRequest request, string headerName)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (!request.Headers.TryGetValue(headerName, out var headerValues))
                return false;

            foreach (var headerValue in headerValues)
                if (!string.IsNullOrWhiteSpace(headerValue))
                    return true;

            return false;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Attempts to get if modified since a DateTimeOffset from the given HttpRequest.
        /// </summary>
        /// <param name="request">Current HTTP request.</param>
        /// <param name="ifModifiedSince">[out] if modified since.</param>
        /// <returns>
        ///     True if it succeeds, false if it fails.
        /// </returns>
        /// =================================================================================================
        private static bool TryGetIfModifiedSince(HttpRequest request, out DateTimeOffset ifModifiedSince)
        {
            ifModifiedSince = default;

            if (!request.Headers.TryGetValue(HeaderNames.IfModifiedSince, out var headerValues))
                return false;

            foreach (var headerValue in headerValues)
            {
                if (string.IsNullOrWhiteSpace(headerValue))
                    continue;

                if (!DateTimeOffset.TryParse(
                        headerValue,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                        out var parsedValue))
                    continue;

                var normalizedTicks = parsedValue.Ticks - parsedValue.Ticks % TimeSpan.TicksPerSecond;
                ifModifiedSince = new DateTimeOffset(normalizedTicks, TimeSpan.Zero);

                return true;
            }

            return false;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Enumerates split entity tag values in this collection.
        /// </summary>
        /// <param name="headerValue">The header value.</param>
        /// <returns>
        ///     An enumerator that allows foreach to be used to process split entity tag values in this
        ///     collection.
        /// </returns>
        /// =================================================================================================
        private static IEnumerable<string> SplitEntityTagValues(string headerValue)
        {
            if (string.IsNullOrWhiteSpace(headerValue))
                yield break;

            var isInsideQuotes = false;
            var currentSegmentStart = 0;

            for (var index = 0; index < headerValue.Length; index++)
            {
                var currentCharacter = headerValue[index];

                if (currentCharacter == '"' && (index == 0 || headerValue[index - 1] != '\\'))
                    isInsideQuotes = !isInsideQuotes;

                if (currentCharacter != ',' || isInsideQuotes)
                    continue;

                var currentEntityTag = headerValue.Substring(currentSegmentStart, index - currentSegmentStart).Trim();

                if (!string.IsNullOrEmpty(currentEntityTag))
                    yield return currentEntityTag;

                currentSegmentStart = index + 1;
            }

            var lastEntityTag = headerValue.Substring(currentSegmentStart).Trim();

            if (!string.IsNullOrEmpty(lastEntityTag))
                yield return lastEntityTag;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Normalize entity tag.
        /// </summary>
        /// <param name="entityTag">The entity tag.</param>
        /// <returns>
        ///     A string.
        /// </returns>
        /// =================================================================================================
        private static string NormalizeEntityTag(string entityTag)
        {
            var normalizedEntityTag = entityTag.Trim();

            if (normalizedEntityTag.StartsWith("W/", StringComparison.OrdinalIgnoreCase))
                normalizedEntityTag = normalizedEntityTag.Substring(2).TrimStart();

            return normalizedEntityTag;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Query if 'entityTag' is weak entity tag.
        /// </summary>
        /// <param name="entityTag">The entity tag.</param>
        /// <returns>
        ///     True if weak entity tag, false if not.
        /// </returns>
        /// =================================================================================================
        private static bool IsWeakEntityTag(string entityTag)
        {
            return entityTag.Trim().StartsWith("W/", StringComparison.OrdinalIgnoreCase);
        }
    }
}