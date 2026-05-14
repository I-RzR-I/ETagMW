// ***********************************************************************
//  Assembly          : RzR.MiddleWares.ETagMW
//  Author            : RzR
//  Created           : 07-05-2026 22:05
// 
//  Last Modified By : RzR
//  Last Modified On : 13-05-2026 23:03
//  ***********************************************************************
//  <copyright file="DisableETagAttribute.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      https://iamrzr.dev/contact
//  </contact>
//  <summary></summary>
//  ***********************************************************************

#region U S I N G

using System;
using RzR.Web.Middleware.ETag.Abstractions;

#endregion

namespace RzR.Web.Middleware.ETag.Attributes
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     Explicitly disable ETag processing for an endpoint.
    /// </summary>
    /// <seealso cref="T:Attribute"/>
    /// <seealso cref="T:RzR.Web.Middleware.ETag.Abstractions.IETagPolicyMetadata"/>
    /// =================================================================================================
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class DisableETagAttribute : Attribute, IETagPolicyMetadata
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets a value indicating that ETag processing is disabled.
        /// </summary>
        /// <value>
        ///     True if this object is enabled, false if not.
        /// </value>
        /// =================================================================================================
        public bool IsEnabled => false;
    }
}