// ***********************************************************************
//  Assembly          : RzR.MiddleWares.ETagMW
//  Author            : RzR
//  Created           : 07-05-2026 22:05
// 
//  Last Modified By : RzR
//  Last Modified On : 13-05-2026 23:12
//  ***********************************************************************
//  <copyright file="ETagOptionsMetadata.cs" company="RzR SOFT & TECH">
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
using RzR.Web.Middleware.ETag.Options;

#endregion

namespace RzR.Web.Middleware.ETag.Metadata
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     Endpoint metadata that enables ETag processing and applies route-specific option
    ///     overrides.
    /// </summary>
    /// <seealso cref="T:RzR.Web.Middleware.ETag.Abstractions.IETagPolicyMetadata"/>
    /// <seealso cref="T:RzR.Web.Middleware.ETag.Abstractions.IETagOptionsMetadata"/>
    /// =================================================================================================
    public sealed class ETagOptionsMetadata : IETagPolicyMetadata, IETagOptionsMetadata
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     (Immutable)
        ///     Route-specific option configurator.
        /// </summary>
        /// =================================================================================================
        private readonly Action<ETagOption> _configure;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Initializes a new instance of the <see cref="ETagOptionsMetadata" /> class.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when one or more required arguments are null.
        /// </exception>
        /// <param name="configure">Route-specific option configurator.</param>
        /// =================================================================================================
        public ETagOptionsMetadata(Action<ETagOption> configure)
        {
            _configure = configure ?? throw new ArgumentNullException(nameof(configure));
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Apply the route-specific ETag option overrides.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when one or more required arguments are null.
        /// </exception>
        /// <param name="options">Current request option clone.</param>
        /// =================================================================================================
        public void Configure(ETagOption options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            _configure(options);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets a value indicating that ETag processing is enabled.
        /// </summary>
        /// <value>
        ///     True if this object is enabled, false if not.
        /// </value>
        /// =================================================================================================
        public bool IsEnabled => true;
    }
}