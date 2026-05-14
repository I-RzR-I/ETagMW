// ***********************************************************************
//  Assembly          : RzR.MiddleWares.ETagMW
//  Author            : RzR
//  Created           : 07-05-2026 18:05
// 
//  Last Modified By : RzR
//  Last Modified On : 13-05-2026 21:21
//  ***********************************************************************
//  <copyright file="IETagGenerator.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      https://iamrzr.dev/contact
//  </contact>
//  <summary></summary>
//  ***********************************************************************

#region U S I N G

using System.IO;
using Microsoft.AspNetCore.Http;

#endregion

namespace RzR.Web.Middleware.ETag.Abstractions
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     Abstraction for ETag generation.
    /// </summary>
    /// =================================================================================================
    public interface IETagGenerator
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Generate an entity tag for the current response body.
        /// </summary>
        /// <param name="context">Current HTTP context.</param>
        /// <param name="responseBody">Buffered response body.</param>
        /// <returns>
        ///     A string.
        /// </returns>
        /// =================================================================================================
        string Generate(HttpContext context, Stream responseBody);
    }
}