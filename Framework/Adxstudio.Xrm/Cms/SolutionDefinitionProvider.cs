/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.Collections.Generic;
using System.Linq;
using Adxstudio.Xrm.AspNet.Cms;

namespace Adxstudio.Xrm.Cms
{
	public class SolutionDefinitionProvider : ISolutionDefinitionProvider
	{
		protected PortalSolutions PortalSolutions { get; private set; }

		public SolutionDefinitionProvider(PortalSolutions portalSolutions)
		{
			this.PortalSolutions = portalSolutions;
		}

		public virtual IDictionary<string, object> GetQueryParameters()
		{
			return null;
		}

		public virtual SolutionDefinition GetSolution()
		{
			var crmSolutionInfos = this.PortalSolutions.Solutions;
			var solutionNames = crmSolutionInfos.Keys.ToList();
			var defaults = this.GetDefaultSolutions(solutionNames);
			var customs = this.GetCustomSolutions();
			var solutions = defaults.Concat(customs);

			var solution = solutions.Aggregate(Solutions.Base, (current, extension) => current.Union(extension));
			return GetFilteredSolution(solution, crmSolutionInfos);
		}

		/// <summary>
		/// Filters the entities/relationships and columns based on the MicrosoftCrmPortalBase solution version in CRM.
		/// </summary>
		/// <param name="solution"></param>
		/// <param name="crmSolutions"></param>
		/// <returns></returns>
		private static SolutionDefinition GetFilteredSolution(SolutionDefinition solution, IDictionary<string, SolutionInfo> crmSolutions)
		{
			if (solution.Solutions == null || !solution.Solutions.Any())
			{
				return null;
			}

			var filteredEntities = solution.GetFilteredEntities(crmSolutions);
			return new SolutionDefinition(solution.Solutions, filteredEntities, solution.ManyRelationships);
		}

		protected virtual IEnumerable<SolutionDefinition> GetCustomSolutions()
		{
			yield break;
		}

		protected virtual IEnumerable<SolutionDefinition> GetDefaultSolutions(List<string> solutionNames)
		{
			if (solutionNames.Contains(PortalSolutions.SolutionNames.CompleteSolutionName))
			{
				// return all the solutions

				foreach (var extension in Solutions.Definitions.Values)
				{
					yield return extension;
				}
			}
			else
			{
				foreach (var solutionName in solutionNames)
				{
					SolutionDefinition extension;

					if (Solutions.Definitions.TryGetValue(solutionName, out extension))
					{
						yield return extension;
					}
				}
			}
		}
	}
}
