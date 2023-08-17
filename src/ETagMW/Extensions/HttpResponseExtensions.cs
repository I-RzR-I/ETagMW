// ***********************************************************************
//  Assembly         : RzR.MiddleWares.ETagMW
//  Author           : RzR
//  Created On       : 2023-08-16 15:31
// 
//  Last Modified By : RzR
//  Last Modified On : 2023-08-16 18:55
// ***********************************************************************
//  <copyright file="HttpResponseExtensions.cs" company="">
//   Copyright (c) RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

#endregion

namespace ETagMW.Extensions
{
    /// <summary>
    ///     HttpResponse extension
    /// </summary>
    internal static class HttpResponseExtensions
    {
        /// <summary>
        ///     Check if response support ETag
        /// </summary>
        /// <param name="response">Current HTTP response</param>
        /// <returns></returns>
        internal static bool IsEtagSupported(this HttpResponse response)
        {
            if (response.StatusCode != StatusCodes.Status200OK)
                return false;

            if (response.Headers.ContainsKey(HeaderNames.ETag))
                return false;

            return true;
        }
    }
}