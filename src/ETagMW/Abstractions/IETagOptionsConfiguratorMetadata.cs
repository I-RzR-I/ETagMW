// ***********************************************************************
//  Assembly          : RzR.MiddleWares.ETagMW
//  Author            : RzR
//  Created           : 07-05-2026 23:05
// 
//  Last Modified By : RzR
//  Last Modified On : 13-05-2026 21:21
//  ***********************************************************************
//  <copyright file="IETagOptionsConfiguratorMetadata.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      https://iamrzr.dev/contact
//  </contact>
//  <summary></summary>
//  ***********************************************************************

#region U S I N G

using System;

#endregion

namespace RzR.Web.Middleware.ETag.Abstractions
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     Endpoint metadata that points at a DI-resolvable <see cref="IETagOptionsConfigurator" />.
    /// </summary>
    /// =================================================================================================
    public interface IETagOptionsConfiguratorMetadata
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Type implementing <see cref="IETagOptionsConfigurator" /> resolved from the request
        ///     services.
        /// </summary>
        /// <value>
        ///     The type of the configurator.
        /// </value>
        /// =================================================================================================
        Type ConfiguratorType { get; }
    }
}