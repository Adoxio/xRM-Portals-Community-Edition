/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Cms
{
	using System;

	/// <summary>
	/// Solution info
	/// </summary>
	public class SolutionInfo
	{
		/// <summary>
		/// Name of the solution
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Solution version
		/// </summary>
		public Version SolutionVersion { get; set; }

		/// <summary>
		/// Date when solution was installed
		/// </summary>
		public DateTime InstalledOn { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="SolutionInfo"/> class>
		/// </summary>
		/// <param name="name">Name of the solution</param>
		/// <param name="version">Version of the solution</param>
		/// <param name="installedOn">Date when solution was installed</param>
		public SolutionInfo(string name, Version version, DateTime installedOn)
		{
			this.Name = name;
			this.SolutionVersion = version;
			this.InstalledOn = installedOn;
		}
	}
}
