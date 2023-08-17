// ***********************************************************************
//  Assembly         : RzR.MiddleWares.ETagMW
//  Author           : RzR
//  Created On       : 2023-08-16 16:26
// 
//  Last Modified By : RzR
//  Last Modified On : 2023-08-16 18:55
// ***********************************************************************
//  <copyright file="MemoryStreamExtensions.cs" company="">
//   Copyright (c) RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using System.IO;
using System.Security.Cryptography;

#endregion

namespace ETagMW.Extensions
{
    /// <summary>
    ///     Memory stream extension
    /// </summary>
    internal static class MemoryStreamExtensions
    {
        /// <summary>
        ///     Calculate check sum for provided stream
        /// </summary>
        /// <param name="stream">Memory stream to be calculated</param>
        /// <returns></returns>
        internal static string CalculateChecksum(this MemoryStream stream)
        {
            using var hash = new SHA256CryptoServiceProvider();
            stream.Position = 0;
            var bytes = hash.ComputeHash(stream);

            return $"\"{bytes.ToBase64String()}\"";
        }
    }
}