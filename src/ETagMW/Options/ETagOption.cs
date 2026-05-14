// ***********************************************************************
//  Assembly          : RzR.MiddleWares.ETagMW
//  Author            : RzR
//  Created           : 16-08-2023 08:08
// 
//  Last Modified By : RzR
//  Last Modified On : 13-05-2026 23:16
//  ***********************************************************************
//  <copyright file="ETagOption.cs" company="RzR SOFT & TECH">
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
using Microsoft.AspNetCore.Http;

#endregion

namespace RzR.Web.Middleware.ETag.Options
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     ETag options.
    /// </summary>
    /// =================================================================================================
    public class ETagOption
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     (Immutable)
        ///     Default max body size allowed for ETag buffering.
        /// </summary>
        /// =================================================================================================
        public const long DefaultMaxBodySize = 1024 * 1024;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Resolve a request-specific ETag.
        /// </summary>
        /// <value>
        ///     A function delegate that yields a string.
        /// </value>
        /// =================================================================================================
        public Func<HttpContext, string> ETagFactory { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Resolve a request-specific Last-Modified value.
        /// </summary>
        /// <value>
        ///     A function delegate that yields a DateTimeOffset?
        /// </value>
        /// =================================================================================================
        public Func<HttpContext, DateTimeOffset?> LastModifiedFactory { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Use own ETag value.
        /// </summary>
        /// <value>
        ///     True if use own tag, false if not.
        /// </value>
        /// =================================================================================================
        [Obsolete("Use ETagFactory instead.")]
        public bool UseOwnTag { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Own ETag value.
        /// </summary>
        /// <value>
        ///     The own tag.
        /// </value>
        /// =================================================================================================
        [Obsolete("Use ETagFactory instead.")]
        public string OwnTag { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     HTTP methods that are eligible for ETag processing.
        /// </summary>
        /// <value>
        ///     The supported methods.
        /// </value>
        /// =================================================================================================
        public ICollection<string> SupportedMethods { get; set; } =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase) { HttpMethods.Get };

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Maximum buffered response body size used for ETag generation.
        /// </summary>
        /// <value>
        ///     The maximum size of the body.
        /// </value>
        /// =================================================================================================
        public long MaxBodySize { get; set; } = DefaultMaxBodySize;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Indicates whether ETag processing applies to eligible endpoints by default.
        /// </summary>
        /// <value>
        ///     True if apply e tag by default, false if not.
        /// </value>
        /// =================================================================================================
        public bool ApplyETagByDefault { get; set; } = true;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Current resolved max body size.
        /// </summary>
        /// <value>
        ///     The size of the effective maximum body.
        /// </value>
        /// =================================================================================================
        internal long EffectiveMaxBodySize => MaxBodySize > 0 ? MaxBodySize : DefaultMaxBodySize;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Resolve an explicitly configured ETag for the current request.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when one or more required arguments are null.
        /// </exception>
        /// <param name="context">Current HTTP context.</param>
        /// <returns>
        ///     A string.
        /// </returns>
        /// =================================================================================================
        internal string ResolveConfiguredETag(HttpContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var configuredETag = ETagFactory?.Invoke(context);

#pragma warning disable CS0618
            if (string.IsNullOrWhiteSpace(configuredETag) && UseOwnTag && !string.IsNullOrWhiteSpace(OwnTag))
                configuredETag = OwnTag;
#pragma warning restore CS0618

            return configuredETag;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Resolve an explicitly configured Last-Modified value for the current request.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when one or more required arguments are null.
        /// </exception>
        /// <param name="context">Current HTTP context.</param>
        /// <returns>
        ///     A DateTimeOffset?
        /// </returns>
        /// =================================================================================================
        internal DateTimeOffset? ResolveLastModified(HttpContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var configuredLastModified = LastModifiedFactory?.Invoke(context);

            if (!configuredLastModified.HasValue)
                return null;

            return NormalizeLastModified(configuredLastModified.Value);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Clone current option values to isolate runtime usage from configuration mutations.
        /// </summary>
        /// <remarks>
        ///     Per-request route overrides mutate the cloned instance, so any reference-type option
        ///     added to <see cref="ETagOption" /> in the future MUST be deep-copied here. Otherwise a
        ///     route-level override would mutate the shared singleton option instance and leak across
        ///     requests.
        /// </remarks>
        /// <returns>
        ///     A copy of this object.
        /// </returns>
        /// =================================================================================================
        internal ETagOption Clone()
        {
            var option = new ETagOption
            {
                ApplyETagByDefault = ApplyETagByDefault,
                ETagFactory = ETagFactory,
                LastModifiedFactory = LastModifiedFactory,
                MaxBodySize = MaxBodySize,
                SupportedMethods = SupportedMethods == null
                    ? null
                    : new HashSet<string>(SupportedMethods, StringComparer.OrdinalIgnoreCase)
            };

#pragma warning disable CS0618
            option.UseOwnTag = UseOwnTag;
            option.OwnTag = OwnTag;
#pragma warning restore CS0618

            return option;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Normalize Last-Modified values to RFC 7232 second precision in UTC.
        /// </summary>
        /// <param name="lastModified">Current Last-Modified value.</param>
        /// <returns>
        ///     A DateTimeOffset.
        /// </returns>
        /// =================================================================================================
        private static DateTimeOffset NormalizeLastModified(DateTimeOffset lastModified)
        {
            var normalizedValue = lastModified.ToUniversalTime();
            var normalizedTicks = normalizedValue.Ticks - normalizedValue.Ticks % TimeSpan.TicksPerSecond;

            return new DateTimeOffset(normalizedTicks, TimeSpan.Zero);
        }
    }
}