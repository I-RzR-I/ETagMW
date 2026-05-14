// ***********************************************************************
//  Assembly          : RzR.MiddleWares.ETagMW
//  Author            : RzR
//  Created           : 16-08-2023 16:08
// 
//  Last Modified By : RzR
//  Last Modified On : 13-05-2026 23:05
//  ***********************************************************************
//  <copyright file="ByteExtensions.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      https://iamrzr.dev/contact
//  </contact>
//  <summary></summary>
//  ***********************************************************************

#region U S I N G

using System;

#endregion

namespace RzR.Web.Middleware.ETag.Extensions
{
    /// <summary>
    ///     Byte extension
    /// </summary>
    internal static class ByteExtensions
    {
        /// <summary>
        ///     Convert byte[] to string BASE64
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        internal static string ToBase64String(this byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            return Convert.ToBase64String(bytes, 0, bytes.Length);
        }
    }
}