// ***********************************************************************
//  Assembly          : RzR.MiddleWares.ETagMW
//  Author            : RzR
//  Created           : 16-08-2023 15:08
// 
//  Last Modified By : RzR
//  Last Modified On : 13-05-2026 23:06
//  ***********************************************************************
//  <copyright file="HttpResponseExtensions.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      https://iamrzr.dev/contact
//  </contact>
//  <summary></summary>
//  ***********************************************************************

#region U S I N G

using System;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using RzR.Web.Middleware.ETag.Options;

#endregion

namespace RzR.Web.Middleware.ETag.Extensions
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     HttpResponse extension.
    /// </summary>
    /// =================================================================================================
    internal static class HttpResponseExtensions
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Check if response support ETag.
        /// </summary>
        /// <param name="response">Current HTTP response.</param>
        /// <param name="option">Current ETag options.</param>
        /// <param name="bufferedLength">Buffered response body length.</param>
        /// <returns>
        ///     True if etag supported, false if not.
        /// </returns>
        /// =================================================================================================
        internal static bool IsEtagSupported(this HttpResponse response, ETagOption option, long bufferedLength)
        {
            if (response.StatusCode != StatusCodes.Status200OK)
                return false;

            if (response.Headers.ContainsKey(HeaderNames.ETag))
                return false;

            if (response.ContentLength.HasValue && response.ContentLength.Value > option.EffectiveMaxBodySize)
                return false;

            if (bufferedLength > option.EffectiveMaxBodySize)
                return false;

            if (!string.IsNullOrWhiteSpace(response.ContentType) &&
                response.ContentType.StartsWith("text/event-stream", StringComparison.OrdinalIgnoreCase))
                return false;

            return true;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Remove body-specific headers from a 304 response.
        /// </summary>
        /// <param name="response">Current HTTP response.</param>
        /// =================================================================================================
        internal static void ClearBodyHeadersFor304(this HttpResponse response)
        {
            ClearBodyHeaders(response);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Remove body-specific headers from a conditional response.
        /// </summary>
        /// <param name="response">Current HTTP response.</param>
        /// =================================================================================================
        internal static void ClearBodyHeadersForConditionalResponse(this HttpResponse response)
        {
            ClearBodyHeaders(response);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Set Last-Modified header value.
        /// </summary>
        /// <param name="response">Current HTTP response.</param>
        /// <param name="lastModified">Current Last-Modified value.</param>
        /// =================================================================================================
        internal static void SetLastModifiedHeader(this HttpResponse response, DateTimeOffset lastModified)
        {
            response.Headers[HeaderNames.LastModified] = lastModified.ToUniversalTime()
                .ToString("R", CultureInfo.InvariantCulture);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Clears the body headers described by response.
        /// </summary>
        /// <param name="response">Current HTTP response.</param>
        /// =================================================================================================
        private static void ClearBodyHeaders(HttpResponse response)
        {
            response.Headers.Remove(HeaderNames.ContentLength);
            response.Headers.Remove(HeaderNames.ContentType);
            response.Headers.Remove(HeaderNames.ContentEncoding);
            response.Headers.Remove(HeaderNames.ContentLanguage);
            response.Headers.Remove(HeaderNames.ContentRange);
            response.Headers.Remove(HeaderNames.TransferEncoding);
        }
    }
}