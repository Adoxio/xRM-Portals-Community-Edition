/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Services.Query
{
	using System.Collections.Generic;

	/// <summary>
	/// Class to handle generation of deterministic, unique aliases for link entities.
	/// </summary>
	public class LinkEntityAliasGenerator
	{
		/// <summary>
		/// Internal mapping dictionary.
		/// </summary>
		private readonly Dictionary<string, int> mapping;

		/// <summary>
		/// Prevents a default instance of the <see cref="LinkEntityAliasGenerator"/> class from being created.
		/// </summary>
		private LinkEntityAliasGenerator()
		{
			this.mapping = new Dictionary<string, int>();
		}

		/// <summary>
		/// Create a LinkEntityAliasGenerator.
		/// </summary>
		/// <returns>a new LinkEntityAliasGenerator</returns>
		public static LinkEntityAliasGenerator CreateInstance()
		{
			return new LinkEntityAliasGenerator();
		}

		/// <summary>
		/// Create a LinkEntityAliasGenerator from pre-existing fetch.
		/// </summary>
		/// <param name="fetch">fetch to consume</param>
		/// <returns>a populated LinkEntityAliasGenerator</returns>
		public static LinkEntityAliasGenerator CreateInstance(Fetch fetch)
		{
			LinkEntityAliasGenerator linkEntityAliasGenerator = LinkEntityAliasGenerator.CreateInstance();
			if (fetch.Entity != null && fetch.Entity.Links != null)
			{
				linkEntityAliasGenerator.PopulateHandler(fetch.Entity.Links);
			}

			return linkEntityAliasGenerator;
		}

		/// <summary>
		/// Iterate over the fetch and insert alias/names into the mapping dictionary.
		/// </summary>
		/// <param name="links">list of links to iterate</param>
		private void PopulateHandler(IEnumerable<Link> links)
		{
			foreach (var link in links)
			{
				if (!string.IsNullOrEmpty(link.Alias) && !string.IsNullOrEmpty(link.Name))
				{
					this.IncrementIndex(link.Name);
				}

				if (link.Links != null)
				{
					this.PopulateHandler(link.Links);
				}
			}
		}

		/// <summary>
		/// Return the current index of the aliasPrefix, inserts 0 if it doesn't exist.
		/// </summary>
		/// <param name="aliasPrefix">alias prefix to get the index of</param>
		/// <returns>Current index WRT to the aliasPrefix</returns>
		private int GetIndex(string aliasPrefix)
		{
			if (!this.mapping.ContainsKey(aliasPrefix))
			{
				this.mapping.Add(aliasPrefix, 0);
			}

			return this.mapping[aliasPrefix];
		}

		/// <summary>
		/// Increments the index within the mapping dictionary.
		/// </summary>
		/// <param name="aliasPrefix">alias prefix to increment</param>
		private void IncrementIndex(string aliasPrefix)
		{
			if (!this.mapping.ContainsKey(aliasPrefix))
			{
				this.mapping.Add(aliasPrefix, 0);
			}

			this.mapping[aliasPrefix]++;
		}

		/// <summary>
		/// Returns the current index whilst incrementing it.
		/// </summary>
		/// <param name="aliasPrefix">alias prefix to get the index of and increment</param>
		/// <returns>Current index WRT to the aliasPrefix</returns>
		private int GetIndexAndIncrement(string aliasPrefix)
		{
			int index = this.GetIndex(aliasPrefix);
			this.IncrementIndex(aliasPrefix);
			return index;
		}

		/// <summary>
		/// Creates the unique deterministic alias for the given aliasPrefix.
		/// </summary>
		/// <param name="aliasPrefix">alias prefix name to use for unique comparison</param>
		/// <returns>alias name</returns>
		public string CreateUniqueAlias(string aliasPrefix)
		{
			return string.Format("generated_alias_{0}_{1}", aliasPrefix, this.GetIndexAndIncrement(aliasPrefix));
		}
	}
}
