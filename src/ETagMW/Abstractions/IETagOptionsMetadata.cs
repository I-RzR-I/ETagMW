// ***********************************************************************
//  Assembly          : RzR.MiddleWares.ETagMW
//  Author            : RzR
//  Created           : 07-05-2026 22:05
// 
//  Last Modified By : RzR
//  Last Modified On : 13-05-2026 21:21
//  ***********************************************************************
//  <copyright file="IETagOptionsMetadata.cs" company="RzR SOFT & TECH">
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
    ///     Endpoint metadata contract for applying route-specific ETag option overrides.
    /// </summary>
    /// <remarks>
    ///     Implementing <see cref="IETagOptionsMetadata" /> on its own does NOT enable the
    ///     middleware for an endpoint when <see cref="ETagOption.ApplyETagByDefault" /> is <c>false</c>.
    ///     The middleware first asks <see cref="IETagPolicyMetadata" /> (last-wins) for an 
    ///     opt-in/opt-out decision; only then are <see cref="IETagOptionsMetadata" /> implementations
    ///     applied to the cloned option. If a custom metadata type carries route-specific overrides
    ///     but no policy signal, the route will be silently skipped under <c>ApplyETagByDefault =
    ///     false</c>. Either also implement <see cref="IETagPolicyMetadata" /> on the same type
    ///     (returning
    ///     <c>IsEnabled = true</c>), or attach an <c>EnableETagAttribute</c> / <c>WithETag()</c>
    ///     helper to the same endpoint.
    /// </remarks>
    /// =================================================================================================
    public interface IETagOptionsMetadata
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Apply the route-specific ETag option overrides.
        /// </summary>
        /// <param name="options">Current request option clone.</param>
        /// =================================================================================================
        void Configure(ETagOption options);
    }
}