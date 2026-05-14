// ***********************************************************************
//  Assembly          : RzR.MiddleWares.ETagMW
//  Author            : RzR
//  Created           : 16-08-2023 08:08
// 
//  Last Modified By : RzR
//  Last Modified On : 13-05-2026 23:13
//  ***********************************************************************
//  <copyright file="ETagMiddleware.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      https://iamrzr.dev/contact
//  </contact>
//  <summary></summary>
//  ***********************************************************************

#region U S I N G

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Net.Http.Headers;
using RzR.Web.Middleware.ETag.Abstractions;
using RzR.Web.Middleware.ETag.Extensions;
using RzR.Web.Middleware.ETag.Generators;
using RzR.Web.Middleware.ETag.Internal;
using RzR.Web.Middleware.ETag.Options;

#endregion

// ReSharper disable ClassNeverInstantiated.Global

namespace RzR.Web.Middleware.ETag.Middleware
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     ETag middleware.
    /// </summary>
    /// =================================================================================================
    public class ETagMiddleware
    {
        /* 2023-08-16
         * Middleware is implemented based on auth initial idea.
         * https://gist.github.com/madskristensen/36357b1df9ddbfd123162cd4201124c4 (Mads Kristensen)
         */

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     (Immutable)
        ///     ETag generator.
        /// </summary>
        ///
        /// ### <remarks>
        ///     .
        /// </remarks>
        /// =================================================================================================
        private readonly IETagGenerator _eTagGenerator;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     (Immutable)
        ///     Middleware logger.
        /// </summary>
        ///
        /// ### <remarks>
        ///     .
        /// </remarks>
        /// =================================================================================================
        private readonly ILogger<ETagMiddleware> _logger;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     (Immutable)
        ///     Middleware metrics.
        /// </summary>
        ///
        /// ### <remarks>
        ///     .
        /// </remarks>
        /// =================================================================================================
        private readonly ETagMiddlewareMetrics _metrics;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     (Immutable)
        ///     Request delegate.
        /// </summary>
        ///
        /// ### <remarks>
        ///     .
        /// </remarks>
        /// =================================================================================================
        private readonly RequestDelegate _next;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     (Immutable)
        ///     ETag options.
        /// </summary>
        ///
        /// ### <remarks>
        ///     .
        /// </remarks>
        /// =================================================================================================
        private readonly ETagOption _option;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Initializes a new instance of the <see cref="ETagMiddleware" /> class.
        /// </summary>
        /// <param name="next">Request delegate.</param>
        /// <param name="option">ETag option.</param>
        /// =================================================================================================
        public ETagMiddleware(RequestDelegate next, ETagOption option)
            : this(next, option, new Sha256ETagGenerator(), NullLogger<ETagMiddleware>.Instance,
                new ETagMiddlewareMetrics())
        {
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Initializes a new instance of the <see cref="ETagMiddleware" /> class.
        /// </summary>
        /// <param name="next">Request delegate.</param>
        /// <param name="option">ETag option.</param>
        /// <param name="eTagGenerator">ETag generator.</param>
        /// =================================================================================================
        public ETagMiddleware(RequestDelegate next, ETagOption option, IETagGenerator eTagGenerator)
            : this(next, option, eTagGenerator, NullLogger<ETagMiddleware>.Instance,
                new ETagMiddlewareMetrics())
        {
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Initializes a new instance of the <see cref="ETagMiddleware" /> class.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when one or more required arguments are null.
        /// </exception>
        /// <param name="next">Request delegate.</param>
        /// <param name="option">ETag option.</param>
        /// <param name="eTagGenerator">ETag generator.</param>
        /// <param name="logger">Middleware logger.</param>
        /// <param name="metrics">Middleware metrics.</param>
        /// =================================================================================================
        internal ETagMiddleware(RequestDelegate next, ETagOption option, IETagGenerator eTagGenerator,
            ILogger<ETagMiddleware> logger, ETagMiddlewareMetrics metrics)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _option = option ?? throw new ArgumentNullException(nameof(option));
            _eTagGenerator = eTagGenerator ?? throw new ArgumentNullException(nameof(eTagGenerator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Invoke task.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when one or more required arguments are null.
        /// </exception>
        /// <param name="context">HttpContext.</param>
        /// <returns>
        ///     A Task.
        /// </returns>
        /// =================================================================================================
        public async Task Invoke(HttpContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var resolution = context.ResolveETagResolution(_option);
            var currentOption = resolution.Option;

            if (!resolution.IsEnabled || !context.Request.CanProcessEtag(context, currentOption))
            {
                _logger.LogDebug("Skipping ETag generation for {Method} {Path} because the request is not eligible.",
                    context.Request.Method, context.Request.Path);
                _metrics.RecordSkipped(context, "request_not_supported");

                await _next(context);

                return;
            }

            var response = context.Response;
            var originalStream = response.Body;
            var originalResponseFeature = context.Features.Get<IHttpResponseFeature>();
#if NET5_0_OR_GREATER
            var originalResponseBodyFeature = context.Features.Get<IHttpResponseBodyFeature>();
#endif
            BufferingResponseFeature bufferingResponseFeature = null;

            using var bufferingStream = new ResponseBufferingStream(
                originalStream,
                currentOption.EffectiveMaxBodySize,
                () => bufferingResponseFeature?.MarkResponseStarted());

            bufferingResponseFeature = new BufferingResponseFeature(originalResponseFeature, bufferingStream);

            response.Body = bufferingStream;

            context.Features.Set(bufferingResponseFeature);
#if NET5_0_OR_GREATER
            context.Features.Set<IHttpResponseBodyFeature>(
                new StreamResponseBodyFeature(bufferingStream, originalResponseBodyFeature));
#endif

            try
            {
                await _next(context);

                if (!bufferingStream.IsBufferingEnabled)
                {
                    _logger.LogDebug(
                        "Skipping ETag generation for {Method} {Path} because buffering was disabled during the response.",
                        context.Request.Method, context.Request.Path);
                    _metrics.RecordSkipped(context, "buffering_disabled");

                    return;
                }

                if (!response.IsEtagSupported(currentOption, bufferingStream.BufferedLength))
                {
                    _logger.LogDebug(
                        "Skipping ETag generation for {Method} {Path} because the response is not eligible. StatusCode: {StatusCode}.",
                        context.Request.Method, context.Request.Path, response.StatusCode);
                    _metrics.RecordSkipped(context, "response_not_supported");

                    await bufferingStream.CopyBufferedToInnerAsync(context.RequestAborted);

                    return;
                }

                var checksum = currentOption.ResolveConfiguredETag(context);

                if (string.IsNullOrWhiteSpace(checksum))
                    checksum = _eTagGenerator.Generate(context, bufferingStream.Buffer);

                checksum = checksum.EnsureQuotedEntityTag();
                var lastModified = currentOption.ResolveLastModified(context);

                try
                {
                    response.Headers[HeaderNames.ETag] = checksum;

                    if (lastModified.HasValue)
                        response.SetLastModifiedHeader(lastModified.Value);

                    _logger.LogDebug("Generated ETag {ETag} for {Method} {Path}.", checksum, context.Request.Method,
                        context.Request.Path);
                    _metrics.RecordGenerated(context);

                    if (context.Request.HasIfMatchHeader() && !context.Request.MatchesIfMatch(checksum))
                    {
                        response.StatusCode = StatusCodes.Status412PreconditionFailed;
                        response.ClearBodyHeadersForConditionalResponse();

                        _logger.LogDebug(
                            "Returning 412 Precondition Failed for {Method} {Path} because If-Match did not match the current validator.",
                            context.Request.Method, context.Request.Path);
                        _metrics.RecordPreconditionFailed(context);

                        return;
                    }

                    if (context.Request.HasIfNoneMatchHeader())
                    {
                        if (context.Request.MatchesIfNoneMatch(checksum))
                        {
                            if (context.Request.IsSafeConditionalMethod())
                            {
                                response.StatusCode = StatusCodes.Status304NotModified;
                                response.ClearBodyHeadersForConditionalResponse();

                                _logger.LogDebug(
                                    "Returning 304 Not Modified for {Method} {Path} because If-None-Match matched the current validator.",
                                    context.Request.Method, context.Request.Path);
                                _metrics.RecordNotModified(context);

                                return;
                            }

                            response.StatusCode = StatusCodes.Status412PreconditionFailed;
                            response.ClearBodyHeadersForConditionalResponse();

                            _logger.LogDebug(
                                "Returning 412 Precondition Failed for {Method} {Path} because If-None-Match matched on an unsafe request method.",
                                context.Request.Method, context.Request.Path);
                            _metrics.RecordPreconditionFailed(context);

                            return;
                        }
                    }
                    else if (lastModified.HasValue && context.Request.IsNotModifiedSince(lastModified.Value))
                    {
                        response.StatusCode = StatusCodes.Status304NotModified;
                        response.ClearBodyHeadersForConditionalResponse();

                        _logger.LogDebug(
                            "Returning 304 Not Modified for {Method} {Path} because If-Modified-Since matched the current validator.",
                            context.Request.Method, context.Request.Path);
                        _metrics.RecordNotModified(context);

                        return;
                    }
                }
                catch (InvalidOperationException exception)
                {
                    _logger.LogDebug(exception,
                        "Skipping ETag generation for {Method} {Path} because response headers can no longer be modified.",
                        context.Request.Method, context.Request.Path);
                    _metrics.RecordSkipped(context, "headers_started");

                    await bufferingStream.CopyBufferedToInnerAsync(context.RequestAborted);

                    return;
                }

                await bufferingStream.CopyBufferedToInnerAsync(context.RequestAborted);
            }
            finally
            {
#if NET5_0_OR_GREATER
                context.Features.Set(originalResponseBodyFeature);
#endif
                context.Features.Set(originalResponseFeature);
                response.Body = originalStream;
            }
        }
    }
}