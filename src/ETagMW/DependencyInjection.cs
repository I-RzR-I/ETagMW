// ***********************************************************************
//  Assembly          : RzR.MiddleWares.ETagMW
//  Author            : RzR
//  Created           : 09-10-2023 21:10
// 
//  Last Modified By : RzR
//  Last Modified On : 13-05-2026 23:16
//  ***********************************************************************
//  <copyright file="DependencyInjection.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      https://iamrzr.dev/contact
//  </contact>
//  <summary></summary>
//  ***********************************************************************

#region U S I N G

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using RzR.Web.Middleware.ETag.Abstractions;
using RzR.Web.Middleware.ETag.Generators;
using RzR.Web.Middleware.ETag.Internal;
using RzR.Web.Middleware.ETag.Middleware;
using RzR.Web.Middleware.ETag.Options;

#endregion

namespace RzR.Web.Middleware.ETag
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     Middleware extension.
    /// </summary>
    /// =================================================================================================
    public static class DependencyInjection
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     (Immutable)
        ///     Default ETag generator.
        /// </summary>
        /// =================================================================================================
        private static readonly IETagGenerator DefaultETagGenerator = new Sha256ETagGenerator();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     (Immutable)
        ///     Default logger.
        /// </summary>
        /// =================================================================================================
        private static readonly ILogger<ETagMiddleware> DefaultLogger = NullLogger<ETagMiddleware>.Instance;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     (Immutable)
        ///     Default metrics recorder.
        /// </summary>
        /// =================================================================================================
        private static readonly ETagMiddlewareMetrics DefaultMetrics = new();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Register ETag middleware services.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when one or more required arguments are null.
        /// </exception>
        /// <param name="services">Service collection.</param>
        /// <returns>
        ///     An IServiceCollection.
        /// </returns>
        /// =================================================================================================
        public static IServiceCollection AddETag(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddOptions<ETagOption>();
            services.TryAddSingleton<IETagGenerator, Sha256ETagGenerator>();
            services.TryAddSingleton<ETagMiddlewareMetrics>();

            return services;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Register ETag middleware services.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when one or more required arguments are null.
        /// </exception>
        /// <param name="services">Service collection.</param>
        /// <param name="configureOptions">Configuration action.</param>
        /// <returns>
        ///     An IServiceCollection.
        /// </returns>
        /// =================================================================================================
        public static IServiceCollection AddETag(this IServiceCollection services,
            Action<ETagOption> configureOptions)
        {
            if (configureOptions == null)
                throw new ArgumentNullException(nameof(configureOptions));

            services.AddETag();
            services.Configure(configureOptions);

            return services;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Register ETag middleware services.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when one or more required arguments are null.
        /// </exception>
        /// <param name="services">Service collection.</param>
        /// <param name="configuration">Configuration source.</param>
        /// <returns>
        ///     An IServiceCollection.
        /// </returns>
        /// =================================================================================================
        public static IServiceCollection AddETag(this IServiceCollection services,
            IConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            return services.AddETag(options => ApplyConfiguration(options, configuration));
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Use ETag response middleware.
        /// </summary>
        /// <param name="app">Application builder.</param>
        /// <returns>
        ///     An IApplicationBuilder.
        /// </returns>
        /// =================================================================================================
        public static IApplicationBuilder UseETag(this IApplicationBuilder app)
        {
            return app.UseETagInternal(null);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Use ETag response middleware.
        /// </summary>
        /// <param name="app">Application builder.</param>
        /// <param name="configureOptions">Configuration option.</param>
        /// <returns>
        ///     An IApplicationBuilder.
        /// </returns>
        /// =================================================================================================
        [Obsolete("Use services.AddETag(...) to configure middleware services, then call app.UseETag().")]
        public static IApplicationBuilder UseETag(this IApplicationBuilder app,
            ETagOption configureOptions)
        {
            return app.UseETagInternal(configureOptions);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Use ETag response middleware.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when one or more required arguments are null.
        /// </exception>
        /// <param name="app">Application builder.</param>
        /// <param name="configureOptions">Configuration option.</param>
        /// <returns>
        ///     An IApplicationBuilder.
        /// </returns>
        /// =================================================================================================
        [Obsolete("Use services.AddETag(...) to configure middleware services, then call app.UseETag().")]
        public static IApplicationBuilder UseETag(this IApplicationBuilder app,
            Action<ETagOption> configureOptions)
        {
            if (configureOptions == null)
                throw new ArgumentNullException(nameof(configureOptions));

            var options = new ETagOption();
            configureOptions(options);

            return app.UseETagInternal(options);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Register middleware in the HTTP pipeline.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when one or more required arguments are null.
        /// </exception>
        /// <param name="app">Application builder.</param>
        /// <param name="overrideOptions">Explicit runtime options.</param>
        /// <returns>
        ///     An IApplicationBuilder.
        /// </returns>
        /// =================================================================================================
        private static IApplicationBuilder UseETagInternal(this IApplicationBuilder app,
            ETagOption overrideOptions)
        {
            if (app == null)
                throw new ArgumentNullException(nameof(app));

            return app.Use(next => context =>
            {
                var serviceProvider = context.RequestServices ?? app.ApplicationServices;
                var middleware = new ETagMiddleware(
                    next,
                    ResolveOptions(serviceProvider, overrideOptions),
                    ResolveGenerator(serviceProvider),
                    ResolveLogger(serviceProvider),
                    ResolveMetrics(serviceProvider));

                return middleware.Invoke(context);
            });
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Resolve runtime ETag options.
        /// </summary>
        /// <param name="serviceProvider">Service provider.</param>
        /// <param name="overrideOptions">Explicit override options.</param>
        /// <returns>
        ///     An ETagOption.
        /// </returns>
        /// =================================================================================================
        private static ETagOption ResolveOptions(IServiceProvider serviceProvider,
            ETagOption overrideOptions)
        {
            if (overrideOptions != null)
                return overrideOptions.Clone();

            var optionsAccessor = serviceProvider?.GetService<IOptions<ETagOption>>();

            return (optionsAccessor?.Value ?? new ETagOption()).Clone();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Resolve ETag generator.
        /// </summary>
        /// <param name="serviceProvider">Service provider.</param>
        /// <returns>
        ///     An IETagGenerator.
        /// </returns>
        /// =================================================================================================
        private static IETagGenerator ResolveGenerator(IServiceProvider serviceProvider)
        {
            return serviceProvider?.GetService<IETagGenerator>() ?? DefaultETagGenerator;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Resolve middleware logger.
        /// </summary>
        /// <param name="serviceProvider">Service provider.</param>
        /// <returns>
        ///     An ILogger&lt;ETagMiddleware&gt;
        /// </returns>
        /// =================================================================================================
        private static ILogger<ETagMiddleware> ResolveLogger(IServiceProvider serviceProvider)
        {
            return serviceProvider?.GetService<ILogger<ETagMiddleware>>() ?? DefaultLogger;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Resolve middleware metrics recorder.
        /// </summary>
        /// <param name="serviceProvider">Service provider.</param>
        /// <returns>
        ///     The ETagMiddlewareMetrics.
        /// </returns>
        /// =================================================================================================
        private static ETagMiddlewareMetrics ResolveMetrics(IServiceProvider serviceProvider)
        {
            return serviceProvider?.GetService<ETagMiddlewareMetrics>() ?? DefaultMetrics;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Apply configuration values to the current options instance.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when one or more required arguments are null.
        /// </exception>
        /// <param name="options">Current options.</param>
        /// <param name="configuration">Configuration source.</param>
        /// =================================================================================================
        private static void ApplyConfiguration(ETagOption options, IConfiguration configuration)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            if (long.TryParse(configuration[nameof(ETagOption.MaxBodySize)], out var maxBodySize))
                options.MaxBodySize = maxBodySize;

            if (bool.TryParse(configuration[nameof(ETagOption.ApplyETagByDefault)], out var applyETagByDefault))
                options.ApplyETagByDefault = applyETagByDefault;

            var supportedMethods = configuration.GetSection(nameof(ETagOption.SupportedMethods));

            if (supportedMethods == null)
                return;

            var configuredMethods = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var method in supportedMethods.GetChildren())
                if (!string.IsNullOrWhiteSpace(method.Value))
                    configuredMethods.Add(method.Value.Trim());

            if (configuredMethods.Count > 0)
                options.SupportedMethods = configuredMethods;
        }
    }
}