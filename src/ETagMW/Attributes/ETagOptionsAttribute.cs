// ***********************************************************************
//  Assembly          : RzR.MiddleWares.ETagMW
//  Author            : RzR
//  Created           : 07-05-2026 23:05
// 
//  Last Modified By : RzR
//  Last Modified On : 13-05-2026 23:03
//  ***********************************************************************
//  <copyright file="ETagOptionsAttribute.cs" company="RzR SOFT & TECH">
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
    ///     MVC-friendly attribute that enables ETag processing for an endpoint and applies 
    ///     route-specific option overrides through a DI-resolved <see cref="IETagOptionsConfigurator" />. 
    /// </summary>
    /// <seealso cref="T:Attribute"/>
    /// <seealso cref="T:RzR.Web.Middleware.ETag.Abstractions.IETagPolicyMetadata"/>
    /// <seealso cref="T:RzR.Web.Middleware.ETag.Abstractions.IETagOptionsConfiguratorMetadata"/>
    /// =================================================================================================
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class ETagOptionsAttribute : Attribute, IETagPolicyMetadata, IETagOptionsConfiguratorMetadata
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Initializes a new instance of the <see cref="ETagOptionsAttribute" /> class.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when one or more required arguments are null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when one or more arguments have unsupported or illegal values.
        /// </exception>
        /// <param name="configuratorType">
        ///     A type implementing <see cref="IETagOptionsConfigurator" />.
        /// </param>
        /// =================================================================================================
        public ETagOptionsAttribute(Type configuratorType)
        {
            if (configuratorType == null)
                throw new ArgumentNullException(nameof(configuratorType));

            if (!typeof(IETagOptionsConfigurator).IsAssignableFrom(configuratorType))
                throw new ArgumentException(
                    $"Type '{configuratorType.FullName}' must implement {nameof(IETagOptionsConfigurator)}.",
                    nameof(configuratorType));

            ConfiguratorType = configuratorType;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Type implementing <see cref="IETagOptionsConfigurator" /> resolved from the request
        ///     services.
        /// </summary>
        /// <value>
        ///     The type of the configurator.
        /// </value>
        /// =================================================================================================
        public Type ConfiguratorType { get; }

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