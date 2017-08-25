/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	/// <summary>
	/// Details of the product
	/// </summary>
	internal class ProductInfo
	{
		/// <summary>
		/// Details of an assemby
		/// </summary>
		internal class ProductAssembly
		{
			/// <summary>
			/// Name of the assembly
			/// </summary>
			public string Name { get; set; }

			/// <summary>
			/// Version of the assembly
			/// </summary>
			public Version Version { get; set; }

			/// <summary>
			/// Location of the assembly
			/// </summary>
			public string Location { get; set; }
		}
		
		/// <summary>
		/// Collection of assemblies referenced by the product
		/// </summary>
		public IEnumerable<ProductAssembly> Assemblies { get; private set; }
		
		/// <summary>
		/// The Adxstudio.Xrm assembly details
		/// </summary>
		public ProductAssembly Assembly { get; private set; }

		/// <summary>
		///  Initializes a new instance of the <see cref="ProductInfo" /> class.
		/// </summary>
		/// <param name="includeGac">Indicates whether to include assemblies from Global Assembly Cache. Default is false.</param>
		public ProductInfo(bool includeGac = false)
		{
			var assemblies =
				from assembly in AppDomain.CurrentDomain.GetAssemblies()
				let name = assembly.GetName()
				where includeGac || !assembly.GlobalAssemblyCache
				select name;

			this.Assemblies =
				assemblies.Select(a => new ProductAssembly { Name = a.Name, Version = a.Version, Location = a.EscapedCodeBase })
					.ToList();

			this.Assembly = this.Assemblies.FirstOrDefault(a => a.Name == "Adxstudio.Xrm");
		}
	}
}
