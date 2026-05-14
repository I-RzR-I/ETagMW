// ***********************************************************************
//  Assembly          : RzR.MiddleWares.ETagMW
//  Author            : RzR
//  Created           : 07-05-2026 23:05
// 
//  Last Modified By : RzR
//  Last Modified On : 13-05-2026 21:21
//  ***********************************************************************
//  <copyright file="IETagOptionsConfigurator.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      https://iamrzr.dev/contact
//  </contact>
//  <summary></summary>
//  ***********************************************************************

#region U S I N G

using RzR.Web.Middleware.ETag.Options;

#endregion

namespace RzR.Web.Middleware.ETag.Abstractions
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     DI-resolvable per-route ETag option configurator.
    /// </summary>
    /// =================================================================================================
    public interface IETagOptionsConfigurator
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Apply the route-specific ETag option overrides.
        /// </summary>
        /// <param name="options">Cloned per-request option instance.</param>
        /// =================================================================================================
        void Configure(ETagOption options);
    }
}