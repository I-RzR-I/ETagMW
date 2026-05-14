// ***********************************************************************
//  Assembly          : RzR.MiddleWares.ETagMW
//  Author            : RzR
//  Created           : 07-05-2026 21:05
// 
//  Last Modified By : RzR
//  Last Modified On : 13-05-2026 23:08
//  ***********************************************************************
//  <copyright file="BufferingStreamManager.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      https://iamrzr.dev/contact
//  </contact>
//  <summary></summary>
//  ***********************************************************************

#region U S I N G

using System.IO;
using Microsoft.IO;

#endregion

namespace RzR.Web.Middleware.ETag.Internal
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     Shared recyclable buffer manager for middleware response buffering.
    /// </summary>
    /// =================================================================================================
    internal static class BufferingStreamManager
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     (Immutable)
        ///     Shared recyclable memory stream manager.
        /// </summary>
        /// =================================================================================================
        private static readonly RecyclableMemoryStreamManager Manager = new();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Create a pooled buffer stream.
        /// </summary>
        /// <returns>
        ///     The stream.
        /// </returns>
        /// =================================================================================================
        internal static MemoryStream GetStream()
        {
            return Manager.GetStream(nameof(ResponseBufferingStream));
        }
    }
}