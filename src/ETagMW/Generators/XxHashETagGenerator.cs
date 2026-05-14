#if NET6_0_OR_GREATER

// ***********************************************************************
//  Assembly          : RzR.MiddleWares.ETagMW
//  Author            : RzR
//  Created           : 07-05-2026 18:05
// 
//  Last Modified By : RzR
//  Last Modified On : 13-05-2026 23:07
//  ***********************************************************************
//  <copyright file="XxHashETagGenerator.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      https://iamrzr.dev/contact
//  </contact>
//  <summary></summary>
//  ***********************************************************************

#region U S A G E S

using System;
using System.IO;
using System.IO.Hashing;
using Microsoft.AspNetCore.Http;
using RzR.Web.Middleware.ETag.Abstractions;
using RzR.Web.Middleware.ETag.Extensions;

#endregion

namespace RzR.Web.Middleware.ETag.Generators
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     Optional XXHash-based ETag generator.
    /// </summary>
    /// <seealso cref="T:RzR.Web.Middleware.ETag.Abstractions.IETagGenerator"/>
    /// =================================================================================================
    public sealed class XxHashETagGenerator : IETagGenerator
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Generate an entity tag for the current buffered response body.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when one or more required arguments are null.
        /// </exception>
        /// <param name="context">Current HTTP context.</param>
        /// <param name="responseBody">Buffered response body.</param>
        /// <returns>
        ///     A string.
        /// </returns>
        /// =================================================================================================
        public string Generate(HttpContext context, Stream responseBody)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (responseBody == null)
                throw new ArgumentNullException(nameof(responseBody));

            var originalPosition = responseBody.CanSeek ? responseBody.Position : 0;

            try
            {
                if (responseBody.CanSeek)
                    responseBody.Position = 0;

                using var buffer = new MemoryStream();
                responseBody.CopyTo(buffer);

                var hashBytes = XxHash64.Hash(buffer.ToArray());

                return hashBytes.ToBase64String().EnsureQuotedEntityTag();
            }
            finally
            {
                if (responseBody.CanSeek)
                    responseBody.Position = originalPosition;
            }
        }
    }
}

#endif