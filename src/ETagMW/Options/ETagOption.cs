// ***********************************************************************
//  Assembly         : RzR.MiddleWares.ETagMW
//  Author           : RzR
//  Created On       : 2023-08-16 08:55
// 
//  Last Modified By : RzR
//  Last Modified On : 2023-08-16 18:55
// ***********************************************************************
//  <copyright file="ETagOption.cs" company="">
//   Copyright (c) RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

namespace ETagMW.Options
{
    /// <summary>
    ///     ETag options
    /// </summary>
    public class ETagOption
    {
        /// <summary>
        ///     Use own ETag value
        /// </summary>
        public bool UseOwnTag { get; set; } = false;

        /// <summary>
        ///     Own ETag value
        /// </summary>
        public string OwnTag { get; set; }
    }
}