// ***********************************************************************
//  Assembly          : RzR.MiddleWares.ETagMW
//  Author            : RzR
//  Created           : 07-05-2026 23:05
// 
//  Last Modified By : RzR
//  Last Modified On : 13-05-2026 23:08
//  ***********************************************************************
//  <copyright file="EndpointETagResolution.cs" company="RzR SOFT & TECH">
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

namespace RzR.Web.Middleware.ETag.Internal
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     Result of a single endpoint-metadata walk: precomputed eligibility plus the effective
    ///     option instance used by the rest of the middleware.
    /// </summary>
    /// =================================================================================================
    internal readonly struct EndpointETagResolution
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Initializes a new instance of the <see cref="EndpointETagResolution" /> struct.
        /// </summary>
        /// <param name="option">Effective per-request option (cloned when overrides apply).</param>
        /// <param name="isEnabled">Whether ETag processing is enabled for the current endpoint.</param>
        /// =================================================================================================
        internal EndpointETagResolution(ETagOption option, bool isEnabled)
        {
            Option = option;
            IsEnabled = isEnabled;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Effective option instance to use for the current request.
        /// </summary>
        /// <value>
        ///     The option.
        /// </value>
        /// =================================================================================================
        internal ETagOption Option { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Whether ETag processing is enabled for the current endpoint.
        /// </summary>
        /// <value>
        ///     True if this object is enabled, false if not.
        /// </value>
        /// =================================================================================================
        internal bool IsEnabled { get; }
    }
}