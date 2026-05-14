// ***********************************************************************
//  Assembly          : RzR.MiddleWares.ETagMW.Tests
//  Author            : RzR
//  Created           : 07-05-2026 20:05
// 
//  Last Modified By : RzR
//  Last Modified On : 13-05-2026 23:19
//  ***********************************************************************
//  <copyright file="TestETagGenerator.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      https://iamrzr.dev/contact
//  </contact>
//  <summary></summary>
//  ***********************************************************************

#region U S I N G

using Microsoft.AspNetCore.Http;
using RzR.Web.Middleware.ETag.Abstractions;

#endregion

namespace ETagMW.Tests.Helpers
{
    internal sealed class TestETagGenerator : IETagGenerator
    {
        private readonly string _entityTag;

        public TestETagGenerator(string entityTag)
        {
            _entityTag = entityTag;
        }

        public string Generate(HttpContext context, Stream responseBody)
        {
            return _entityTag;
        }
    }
}