// ***********************************************************************
//  Assembly         : RzR.MiddleWares.ETagMW
//  Author           : RzR
//  Created On       : 2023-08-16 08:23
// 
//  Last Modified By : RzR
//  Last Modified On : 2023-08-16 18:55
// ***********************************************************************
//  <copyright file="DependencyInjection.cs" company="">
//   Copyright (c) RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using System;
using ETagMW.Middleware;
using ETagMW.Options;
using Microsoft.AspNetCore.Builder;

#endregion

namespace ETagMW
{
    /// <summary>
    ///     Middleware extension
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        ///     Use ETag response middleware
        /// </summary>
        /// <param name="app">Application builder</param>
        /// <remarks></remarks>
        public static IApplicationBuilder UseETag(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ETagMiddleware>(new ETagOption {UseOwnTag = false});
        }

        /// <summary>
        ///     Use ETag response middleware
        /// </summary>
        /// <param name="app">Application builder</param>
        /// <param name="configureOptions">Configuration option</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static IApplicationBuilder UseETag(this IApplicationBuilder app,
            ETagOption configureOptions)
        {
            return app.UseMiddleware<ETagMiddleware>(configureOptions);
        }

        /// <summary>
        ///     Use ETag response middleware
        /// </summary>
        /// <param name="app">Application builder</param>
        /// <param name="configureOptions">Configuration option</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static IApplicationBuilder UseETag(this IApplicationBuilder app,
            Action<ETagOption> configureOptions)
        {
            var options = new ETagOption();
            configureOptions(options);

            return app.UseMiddleware<ETagMiddleware>(options);
        }
    }
}