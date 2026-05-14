// ***********************************************************************
//  Assembly          : RzR.MiddleWares.ETagMW
//  Author            : RzR
//  Created           : 07-05-2026 18:05
// 
//  Last Modified By : RzR
//  Last Modified On : 13-05-2026 23:05
//  ***********************************************************************
//  <copyright file="EntityTagExtensions.cs" company="RzR SOFT & TECH">
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
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     Entity tag formatting helpers.
    /// </summary>
    /// =================================================================================================
    internal static class EntityTagExtensions
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Normalize a raw entity tag into a valid strong or weak ETag value.
        /// </summary>
        /// <param name="entityTag">Entity tag value.</param>
        /// <returns>
        ///     A string.
        /// </returns>
        /// =================================================================================================
        internal static string EnsureQuotedEntityTag(this string entityTag)
        {
            if (string.IsNullOrWhiteSpace(entityTag))
                return entityTag;

            var normalizedEntityTag = entityTag.Trim();

            if (normalizedEntityTag == "*")
                return normalizedEntityTag;

            if (normalizedEntityTag.StartsWith("W/", StringComparison.OrdinalIgnoreCase))
            {
                var weakEntityTag = normalizedEntityTag.Substring(2).Trim();
                weakEntityTag = TrimEntityTagQuotes(weakEntityTag);

                return $"W/\"{weakEntityTag}\"";
            }

            return $"\"{TrimEntityTagQuotes(normalizedEntityTag)}\"";
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Trim entity tag quotes.
        /// </summary>
        /// <param name="entityTag">Entity tag value.</param>
        /// <returns>
        ///     A string.
        /// </returns>
        /// =================================================================================================
        private static string TrimEntityTagQuotes(string entityTag)
        {
            var normalizedEntityTag = entityTag.Trim();

            if (normalizedEntityTag.Length >= 2 && normalizedEntityTag[0] == '"' &&
                normalizedEntityTag[normalizedEntityTag.Length - 1] == '"')
                return normalizedEntityTag.Substring(1, normalizedEntityTag.Length - 2);

            return normalizedEntityTag;
        }
    }
}