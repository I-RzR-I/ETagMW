// ***********************************************************************
//  Assembly         : RzR.MiddleWares.ETagMW
//  Author           : RzR
//  Created On       : 2023-08-16 16:15
// 
//  Last Modified By : RzR
//  Last Modified On : 2023-08-16 18:55
// ***********************************************************************
//  <copyright file="ByteExtensions.cs" company="">
//   Copyright (c) RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using System;
using CodeSource;

#endregion

namespace ETagMW.Extensions
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
        [CodeSource(
            "https://raw.githubusercontent.com/I-RzR-I/DomainCommonExtensions/main/src/DomainCommonExtensions/DataTypeExtensions/ByteExtensions.cs",
            "RzR", 1D)]
        internal static string ToBase64String(this byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            return Convert.ToBase64String(bytes, 0, bytes.Length);
        }
    }
}