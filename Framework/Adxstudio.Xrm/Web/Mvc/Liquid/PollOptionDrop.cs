/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using Adxstudio.Xrm.Cms;

namespace Adxstudio.Xrm.Web.Mvc.Liquid
{
	public class PollOptionDrop : EntityDrop
	{
		public PollOptionDrop(IPortalLiquidContext poll, IPollOption pollOption)
			: base(poll, pollOption.Entity)
		{
			Option = pollOption;
		}

		protected IPollOption Option { get; private set; }

		public string Answer
		{
			get { return Option.Answer; }
		}

		public int Votes
		{
			get { return Option.Votes.GetValueOrDefault(0); }
		}

		public decimal Percentage
		{
			get { return Option.Percentage; }
		}
	}
}
