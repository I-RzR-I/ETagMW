// ***********************************************************************
//  Assembly         : RzR.MiddleWares.ETagMW
//  Author           : RzR
//  Created On       : 2023-08-16 08:23
// 
//  Last Modified By : RzR
//  Last Modified On : 2023-08-16 18:55
// ***********************************************************************
//  <copyright file="GeneralAssemblyInfo.cs" company="">
//   Copyright (c) RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using System.Reflection;
using System.Resources;

#endregion

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif

[assembly: AssemblyCompany("RzR ®")]
[assembly: AssemblyProduct("ETag middleware")]
[assembly: AssemblyCopyright("Copyright © 2023 RzR All rights reserved.")]
[assembly: AssemblyTrademark("® RzR™")]
[assembly: AssemblyDescription("Add ETag variable in HTTP response")]

[assembly: AssemblyMetadata("TermsOfService", "")]

[assembly: AssemblyMetadata("ContactUrl", "")]
[assembly: AssemblyMetadata("ContactName", "RzR")]
[assembly: AssemblyMetadata("ContactEmail", "ddpRzR@hotmail.com")]

[assembly: NeutralResourcesLanguage("en-US", UltimateResourceFallbackLocation.MainAssembly)]

[assembly: AssemblyVersion("1.0.1.1758")]
[assembly: AssemblyFileVersion("1.0.1.1758")]
[assembly: AssemblyInformationalVersion("1.0.1.x")]