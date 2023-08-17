// ***********************************************************************
//  Assembly         : RzR.MiddleWares.ETagMW
//  Author           : RzR
//  Created On       : 2023-08-16 08:23
// 
//  Last Modified By : RzR
//  Last Modified On : 2023-08-16 18:55
// ***********************************************************************
//  <copyright file="ETagMiddleware.cs" company="">
//   Copyright (c) RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using System;
using System.IO;
using System.Threading.Tasks;
using CodeSource;
using ETagMW.Extensions;
using ETagMW.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

#endregion

// ReSharper disable ClassNeverInstantiated.Global

namespace ETagMW.Middleware
{
    /// <summary>
    ///     ETag middleware
    /// </summary>
    [CodeSource("https://gist.github.com/madskristensen/36357b1df9ddbfd123162cd4201124c4",
        "Mads Kristensen", null, "2023-08-16", "Middleware is implemented based on auth initial idea.")]
    public class ETagMiddleware
    {
        /// <summary>
        ///     Request delegate
        /// </summary>
        /// <remarks></remarks>
        private readonly RequestDelegate _next;

        /// <summary>
        ///     ETag options
        /// </summary>
        /// <remarks></remarks>
        private readonly ETagOption _option;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ETagMW.Middleware.ETagMiddleware" /> class.
        /// </summary>
        /// <param name="next">Request delegate</param>
        /// <param name="option">ETag option</param>
        /// <remarks></remarks>
        public ETagMiddleware(RequestDelegate next, ETagOption option)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _option = option ?? throw new ArgumentNullException(nameof(option));
        }

        /// <summary>
        ///     Invoke task
        /// </summary>
        /// <param name="context">HttpContext</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public async Task Invoke(HttpContext context)
        {
            var response = context.Response;
            var originalStream = response.Body;

            using var ms = new MemoryStream();
            response.Body = ms;

            await _next(context);

            if (response.IsEtagSupported())
            {
                var checksum = _option.UseOwnTag ? _option.OwnTag : ms.CalculateChecksum();
                response.Headers[HeaderNames.ETag] = checksum;

                if (context.Request.Headers.TryGetValue(HeaderNames.IfNoneMatch, out var etag) && checksum == etag)
                {
                    response.StatusCode = StatusCodes.Status304NotModified;

                    return;
                }
            }

            ms.Position = 0;
            await ms.CopyToAsync(originalStream);
        }
    }
}