/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Resources;

namespace Adxstudio.Xrm.Web.UI.WebControls
{
	/// <summary>
	/// Dropdown populated with 1 minute, 5 minutes, 15minutes, 30 minutes, 1 hour, 1.5 hours - 8 hrs, 1 day, 2 days, 3 days.
	/// Value is stored in minutes.
	/// </summary>
	[ToolboxData("<{0}:CrmDuration runat=server></{0}:CrmDuration>")]
	public class CrmDuration : DropDownList
	{
		override protected void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (Items.Count > 0)
			{
				return;
			}

			var empty = new ListItem(string.Empty, string.Empty);
			empty.Attributes["label"] = " ";
			Items.Add(empty);
			Items.Add(new ListItem(string.Format(ResourceManager.GetString("No_Of_Minute_Add_In_Listitem"), 1), "1"));
			Items.Add(new ListItem(string.Format(ResourceManager.GetString("No_Of_Minutes_Add_In_Listitem"), 5), "5"));
			Items.Add(new ListItem(string.Format(ResourceManager.GetString("No_Of_Minutes_Add_In_Listitem"), 15), "15"));
			Items.Add(new ListItem(string.Format(ResourceManager.GetString("No_Of_Minutes_Add_In_Listitem"), 30), "30"));
			Items.Add(new ListItem(string.Format(ResourceManager.GetString("No_Of_Hour_Add_In_Listitem"), 1), "60"));
			Items.Add(new ListItem(string.Format(ResourceManager.GetString("No_Of_Hours_Add_In_Listitem"), 1.5), "90"));
			Items.Add(new ListItem(string.Format(ResourceManager.GetString("No_Of_Hours_Add_In_Listitem"), 2), "120"));
			Items.Add(new ListItem(string.Format(ResourceManager.GetString("No_Of_Hours_Add_In_Listitem"), 2.5), "150"));
			Items.Add(new ListItem(string.Format(ResourceManager.GetString("No_Of_Hours_Add_In_Listitem"), 3), "180"));
			Items.Add(new ListItem(string.Format(ResourceManager.GetString("No_Of_Hours_Add_In_Listitem"), 3.5), "210"));
			Items.Add(new ListItem(string.Format(ResourceManager.GetString("No_Of_Hours_Add_In_Listitem"), 4), "240"));
			Items.Add(new ListItem(string.Format(ResourceManager.GetString("No_Of_Hours_Add_In_Listitem"), 4.5), "270"));
			Items.Add(new ListItem(string.Format(ResourceManager.GetString("No_Of_Hours_Add_In_Listitem"), 5), "300"));
			Items.Add(new ListItem(string.Format(ResourceManager.GetString("No_Of_Hours_Add_In_Listitem"), 5.5), "330"));
			Items.Add(new ListItem(string.Format(ResourceManager.GetString("No_Of_Hours_Add_In_Listitem"), 6), "360"));
			Items.Add(new ListItem(string.Format(ResourceManager.GetString("No_Of_Hours_Add_In_Listitem"), 6.5), "390"));
			Items.Add(new ListItem(string.Format(ResourceManager.GetString("No_Of_Hours_Add_In_Listitem"), 7), "420"));
			Items.Add(new ListItem(string.Format(ResourceManager.GetString("No_Of_Hours_Add_In_Listitem"), 7.5), "450"));
			Items.Add(new ListItem(string.Format(ResourceManager.GetString("No_Of_Hours_Add_In_Listitem"), 8), "480"));
			Items.Add(new ListItem(string.Format(ResourceManager.GetString("No_Of_Day_Add_In_Listitem"), 1), "1440"));
			Items.Add(new ListItem(string.Format(ResourceManager.GetString("No_Of_Days_Add_In_Listitem"), 2), "2880"));
			Items.Add(new ListItem(string.Format(ResourceManager.GetString("No_Of_Days_Add_In_Listitem"), 3), "4320"));
		}
	}
}
