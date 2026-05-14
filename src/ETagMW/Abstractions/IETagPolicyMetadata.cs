// ***********************************************************************
//  Assembly          : RzR.MiddleWares.ETagMW
//  Author            : RzR
//  Created           : 07-05-2026 22:05
// 
//  Last Modified By : RzR
//  Last Modified On : 13-05-2026 21:21
//  ***********************************************************************
//  <copyright file="IETagPolicyMetadata.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      https://iamrzr.dev/contact
//  </contact>
//  <summary></summary>
//  ***********************************************************************

namespace RzR.Web.Middleware.ETag.Abstractions
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     Endpoint metadata contract for ETag enable/disable policy.
    /// </summary>
    /// =================================================================================================
    public interface IETagPolicyMetadata
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets a value indicating whether ETag processing is enabled for the endpoint.
        /// </summary>
        /// <value>
        ///     True if this object is enabled, false if not.
        /// </value>
        /// =================================================================================================
        bool IsEnabled { get; }
    }
}