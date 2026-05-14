// ***********************************************************************
//  Assembly          : RzR.MiddleWares.ETagMW
//  Author            : RzR
//  Created           : 07-05-2026 17:05
// 
//  Last Modified By : RzR
//  Last Modified On : 13-05-2026 23:08
//  ***********************************************************************
//  <copyright file="BufferingResponseFeature.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      https://iamrzr.dev/contact
//  </contact>
//  <summary></summary>
//  ***********************************************************************

#region U S I N G

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

#endregion

namespace RzR.Web.Middleware.ETag.Internal
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     Response feature wrapper used to delay HasStarted while buffering is active.
    /// </summary>
    /// <seealso cref="T:Microsoft.AspNetCore.Http.Features.IHttpResponseFeature"/>
    /// =================================================================================================
    internal sealed class BufferingResponseFeature : IHttpResponseFeature
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     (Immutable)
        ///     Inner response feature.
        /// </summary>
        /// =================================================================================================
        private readonly IHttpResponseFeature _innerFeature;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Flag indicating whether the wrapped response started.
        /// </summary>
        /// =================================================================================================
        private bool _hasStarted;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Initializes a new instance of the <see cref="BufferingResponseFeature" /> class.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when one or more required arguments are null.
        /// </exception>
        /// <param name="innerFeature">Inner response feature.</param>
        /// <param name="body">Response body stream.</param>
        /// =================================================================================================
        internal BufferingResponseFeature(IHttpResponseFeature innerFeature, Stream body)
        {
            _innerFeature = innerFeature ?? throw new ArgumentNullException(nameof(innerFeature));
            Body = body ?? throw new ArgumentNullException(nameof(body));
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets or sets the response status code.
        /// </summary>
        /// <value>
        ///     The status code.
        /// </value>
        /// =================================================================================================
        public int StatusCode
        {
            get => _innerFeature.StatusCode;
            set => _innerFeature.StatusCode = value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets or sets the response reason phrase.
        /// </summary>
        /// <value>
        ///     The reason phrase.
        /// </value>
        /// =================================================================================================
        public string ReasonPhrase
        {
            get => _innerFeature.ReasonPhrase;
            set => _innerFeature.ReasonPhrase = value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets or sets response headers.
        /// </summary>
        /// <value>
        ///     The headers.
        /// </value>
        /// =================================================================================================
        public IHeaderDictionary Headers
        {
            get => _innerFeature.Headers;
            set => _innerFeature.Headers = value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets or sets response body.
        /// </summary>
        /// <value>
        ///     The body.
        /// </value>
        /// =================================================================================================
        public Stream Body { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets a value indicating whether the response has started.
        /// </summary>
        /// <value>
        ///     True if this object has started, false if not.
        /// </value>
        /// =================================================================================================
        public bool HasStarted => _hasStarted || _innerFeature.HasStarted;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Register on starting callback.
        /// </summary>
        /// <param name="callback">Callback.</param>
        /// <param name="state">Callback state.</param>
        /// =================================================================================================
        public void OnStarting(Func<object, Task> callback, object state)
        {
            _innerFeature.OnStarting(callback, state);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Register on completed callback.
        /// </summary>
        /// <param name="callback">Callback.</param>
        /// <param name="state">Callback state.</param>
        /// =================================================================================================
        public void OnCompleted(Func<object, Task> callback, object state)
        {
            _innerFeature.OnCompleted(callback, state);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Mark response as started.
        /// </summary>
        /// =================================================================================================
        internal void MarkResponseStarted()
        {
            _hasStarted = true;
        }
    }
}