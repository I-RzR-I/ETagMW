// ***********************************************************************
//  Assembly          : RzR.MiddleWares.ETagMW
//  Author            : RzR
//  Created           : 07-05-2026 18:05
// 
//  Last Modified By : RzR
//  Last Modified On : 13-05-2026 23:09
//  ***********************************************************************
//  <copyright file="ETagMiddlewareMetrics.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      https://iamrzr.dev/contact
//  </contact>
//  <summary></summary>
//  ***********************************************************************

#region U S I N G

using Microsoft.AspNetCore.Http;
#if NET6_0_OR_GREATER
using System.Collections.Generic;
using System.Diagnostics.Metrics;
#endif

#endregion

namespace RzR.Web.Middleware.ETag.Internal
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     Middleware metrics recorder.
    /// </summary>
    /// =================================================================================================
    internal sealed class ETagMiddlewareMetrics
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Record a generated ETag.
        /// </summary>
        /// <param name="context">Current HTTP context.</param>
        /// =================================================================================================
        internal void RecordGenerated(HttpContext context)
        {
#if NET6_0_OR_GREATER
            _generatedCounter.Add(1, CreateTags(context));
#endif
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Record a 304 response.
        /// </summary>
        /// <param name="context">Current HTTP context.</param>
        /// =================================================================================================
        internal void RecordNotModified(HttpContext context)
        {
#if NET6_0_OR_GREATER
            _notModifiedCounter.Add(1, CreateTags(context));
#endif
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Record a 412 response.
        /// </summary>
        /// <param name="context">Current HTTP context.</param>
        /// =================================================================================================
        internal void RecordPreconditionFailed(HttpContext context)
        {
#if NET6_0_OR_GREATER
            _preconditionFailedCounter.Add(1, CreateTags(context));
#endif
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Record a skipped ETag.
        /// </summary>
        /// <param name="context">Current HTTP context.</param>
        /// <param name="reason">Skip reason.</param>
        /// =================================================================================================
        internal void RecordSkipped(HttpContext context, string reason)
        {
#if NET6_0_OR_GREATER
            _skippedCounter.Add(1, CreateTags(context, reason));
#endif
        }

#if NET6_0_OR_GREATER
        /// <summary>
        ///     Create metric tags.
        /// </summary>
        /// <param name="context">Current HTTP context</param>
        /// <param name="reason">Optional skip reason</param>
        /// <returns></returns>
        private static KeyValuePair<string, object>[] CreateTags(HttpContext context, string reason = null)
        {
            var tags = new List<KeyValuePair<string, object>>(3);

            if (context != null)
            {
                tags.Add(new KeyValuePair<string, object>("method", context.Request.Method));
                tags.Add(new KeyValuePair<string, object>("path", context.Request.Path.HasValue
                    ? context.Request.Path.Value
                    : string.Empty));
            }

            if (!string.IsNullOrWhiteSpace(reason))
                tags.Add(new KeyValuePair<string, object>("reason", reason));

            return tags.ToArray();
        }
#endif

#if NET6_0_OR_GREATER
        /// <summary>
        ///     Shared middleware meter.
        /// </summary>
        private static readonly Meter Meter = new("RzR.Web.Middleware.ETag");

        /// <summary>
        ///     Generated ETag counter.
        /// </summary>
        private readonly Counter<long> _generatedCounter = Meter.CreateCounter<long>("etag.generated");

        /// <summary>
        ///     304 not modified counter.
        /// </summary>
        private readonly Counter<long> _notModifiedCounter = Meter.CreateCounter<long>("etag.not_modified");

        /// <summary>
        ///     412 precondition failed counter.
        /// </summary>
        private readonly Counter<long> _preconditionFailedCounter =
            Meter.CreateCounter<long>("etag.precondition_failed");

        /// <summary>
        ///     Skipped ETag counter.
        /// </summary>
        private readonly Counter<long> _skippedCounter = Meter.CreateCounter<long>("etag.skipped");
#endif
    }
}