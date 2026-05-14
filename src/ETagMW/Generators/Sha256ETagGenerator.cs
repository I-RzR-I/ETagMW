// ***********************************************************************
//  Assembly          : RzR.MiddleWares.ETagMW
//  Author            : RzR
//  Created           : 07-05-2026 18:05
// 
//  Last Modified By : RzR
//  Last Modified On : 13-05-2026 23:07
//  ***********************************************************************
//  <copyright file="Sha256ETagGenerator.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      https://iamrzr.dev/contact
//  </contact>
//  <summary></summary>
//  ***********************************************************************

#region U S I N G

using System;
using System.IO;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using RzR.Web.Middleware.ETag.Abstractions;
using RzR.Web.Middleware.ETag.Extensions;

#endregion

namespace RzR.Web.Middleware.ETag.Generators
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     Default SHA-256 ETag generator.
    /// </summary>
    /// <seealso cref="T:RzR.Web.Middleware.ETag.Abstractions.IETagGenerator"/>
    /// =================================================================================================
    public sealed class Sha256ETagGenerator : IETagGenerator
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

            if (responseBody is MemoryStream memoryStream)
                return memoryStream.CalculateChecksum();

            var originalPosition = responseBody.CanSeek ? responseBody.Position : 0;

            try
            {
                using var hash = SHA256.Create();

                if (responseBody.CanSeek)
                    responseBody.Position = 0;

                var bytes = hash.ComputeHash(responseBody);

                return bytes.ToBase64String().EnsureQuotedEntityTag();
            }
            finally
            {
                if (responseBody.CanSeek)
                    responseBody.Position = originalPosition;
            }
        }
    }
}