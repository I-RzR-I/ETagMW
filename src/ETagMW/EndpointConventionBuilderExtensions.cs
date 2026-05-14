// ***********************************************************************
//  Assembly          : RzR.MiddleWares.ETagMW
//  Author            : RzR
//  Created           : 07-05-2026 22:05
// 
//  Last Modified By : RzR
//  Last Modified On : 13-05-2026 23:17
//  ***********************************************************************
//  <copyright file="EndpointConventionBuilderExtensions.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      https://iamrzr.dev/contact
//  </contact>
//  <summary></summary>
//  ***********************************************************************

#if NET5_0_OR_GREATER

#region U S I N G

using System;
using RzR.Web.Middleware.ETag.Attributes;
using RzR.Web.Middleware.ETag.Metadata;
using RzR.Web.Middleware.ETag.Options;

#endregion

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Builder
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     Endpoint metadata helpers for ETag policy.
    /// </summary>
    /// =================================================================================================
    public static class EndpointConventionBuilderExtensions
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     (Immutable)
        ///     Shared enable metadata instance.
        /// </summary>
        /// =================================================================================================
        private static readonly EnableETagAttribute EnableMetadata = new();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     (Immutable)
        ///     Shared disable metadata instance.
        /// </summary>
        /// =================================================================================================
        private static readonly DisableETagAttribute DisableMetadata = new();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Explicitly enable ETag processing for an endpoint.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when one or more required arguments are null.
        /// </exception>
        /// <typeparam name="TBuilder">Endpoint builder type.</typeparam>
        /// <param name="builder">Endpoint convention builder.</param>
        /// <returns>
        ///     A TBuilder.
        /// </returns>
        /// =================================================================================================
        public static TBuilder WithETag<TBuilder>(this TBuilder builder)
            where TBuilder : IEndpointConventionBuilder
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder.Add(endpointBuilder => endpointBuilder.Metadata.Add(EnableMetadata));

            return builder;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Explicitly enable ETag processing for an endpoint and apply route-specific option
        ///     overrides.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when one or more required arguments are null.
        /// </exception>
        /// <typeparam name="TBuilder">Endpoint builder type.</typeparam>
        /// <param name="builder">Endpoint convention builder.</param>
        /// <param name="configureOptions">Route-specific option overrides.</param>
        /// <returns>
        ///     A TBuilder.
        /// </returns>
        /// =================================================================================================
        public static TBuilder WithETag<TBuilder>(this TBuilder builder, Action<ETagOption> configureOptions)
            where TBuilder : IEndpointConventionBuilder
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            if (configureOptions == null)
                throw new ArgumentNullException(nameof(configureOptions));

            builder.Add(endpointBuilder => endpointBuilder.Metadata.Add(new ETagOptionsMetadata(configureOptions)));

            return builder;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Explicitly disable ETag processing for an endpoint.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when one or more required arguments are null.
        /// </exception>
        /// <typeparam name="TBuilder">Endpoint builder type.</typeparam>
        /// <param name="builder">Endpoint convention builder.</param>
        /// <returns>
        ///     A TBuilder.
        /// </returns>
        /// =================================================================================================
        public static TBuilder WithoutETag<TBuilder>(this TBuilder builder)
            where TBuilder : IEndpointConventionBuilder
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder.Add(endpointBuilder => endpointBuilder.Metadata.Add(DisableMetadata));

            return builder;
        }
    }
}

#endif