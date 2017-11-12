/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Adxstudio.Xrm.Web.UI.WebControls
{
	/// <summary>
	/// Provide a visual indicator of the current step within a predefined number of steps on a multi-step process.  Progress trackers are designed to help users through a multi-step process and it is vital that such trackers be well designed in order to keep users informed about what section they are currently on, what section they have completed, and what tasks remain.
	/// </summary>
	[Description("Provide a visual indicator of the current step within a predefined number of steps on a multi-step process.")]
	[ToolboxData(@"<{0}:ProgresIndicator runat=""server""></{0}:ProgressIndicator>")]
	[DefaultProperty("")]
	public class ProgressIndicator : CompositeDataBoundControl
	{
		/// <summary>
		/// Property Name of the step index of the data item in the datasource.
		/// </summary>
		[Description("Property Name of the step index of the data item in the datasource.")]
		[DefaultValue("Index")]
		public string IndexDataPropertyName
		{
			get
			{
				var text = (string)ViewState["IndexDataPropertyName"];

				return string.IsNullOrWhiteSpace(text) ? "Index" : text;
			}
			set
			{
				ViewState["IndexDataPropertyName"] = value;
			}
		}

		/// <summary>
		/// Property Name of the step index of the data item in the datasource.
		/// </summary>
		[Description("Property Name of the step title of the data item in the datasource.")]
		[DefaultValue("Title")]
		public string TitleDataPropertyName
		{
			get
			{
				var text = (string)ViewState["TitleDataPropertyName"];

				return string.IsNullOrWhiteSpace(text) ? "Title" : text;
			}
			set
			{
				ViewState["TitleDataPropertyName"] = value;
			}
		}

		/// <summary>
		/// Property Name of the data item in the datasource of the step indicating the step is the current active step.
		/// </summary>
		[Description("Property Name of the data item in the datasource of the step indicating the step is the current active step.")]
		[DefaultValue("IsActive")]
		public string ActiveDataPropertyName
		{
			get
			{
				var text = (string)ViewState["ActiveDataPropertyName"];

				return string.IsNullOrWhiteSpace(text) ? "IsActive" : text;
			}
			set
			{
				ViewState["ActiveDataPropertyName"] = value;
			}
		}

		/// <summary>
		/// Property Name of the data item in the datasource of the step indicating the step is completed.
		/// </summary>
		[Description("Property Name of the data item in the datasource of the step indicating the step is completed.")]
		[DefaultValue("IsCompleted")]
		public string CompletedDataPropertyName
		{
			get
			{
				var text = (string)ViewState["CompletedDataPropertyName"];

				return string.IsNullOrWhiteSpace(text) ? "IsCompleted" : text;
			}
			set
			{
				ViewState["CompletedDataPropertyName"] = value;
			}
		}

		/// <summary>
		/// Position of the control. One of the following values; top, bottom, left, right.
		/// </summary>
		[Description("Position of the control. One of the following values; top, bottom, left, right.")]
		[DefaultValue("")]
		public string Position
		{
			get
			{
				var text = (string)ViewState["Position"];

				return string.IsNullOrWhiteSpace(text) ? string.Empty : text;
			}
			set
			{
				ViewState["Position"] = value;
			}
		}

		/// <summary>
		/// Indicates if the number/index of the step should be prepended to the step's title.
		/// </summary>
		[Description("Indicates if the number/index of the step should be prepended to the step's title.")]
		[DefaultValue(true)]
		public bool PrependStepIndexToTitle
		{
			get { return (bool)(ViewState["PrependStepIndexToTitle"] ?? true); }
			set { ViewState["PrependStepIndexToTitle"] = value; }
		}

		/// <summary>
		/// Indicates if the number/index of the start step is zero.
		/// </summary>
		[Description("Indicates if the number/index of the start step is zero.")]
		[DefaultValue(true)]
		public bool ZeroBasedIndex
		{
			get { return (bool)(ViewState["ZeroBasedIndex"] ?? true); }
			set { ViewState["ZeroBasedIndex"] = value; }
		}

		/// <summary>
		/// Type of the control. One of the following values; title, numeric, progressbar.
		/// </summary>
		[Description("Type of the control. One of the following values; title, numeric, progressbar.")]
		[DefaultValue("title")]
		public string Type
		{
			get
			{
				var text = (string)ViewState["Type"];

				return string.IsNullOrWhiteSpace(text) ? "title" : text;
			}
			set
			{
				ViewState["Type"] = value;
			}
		}

		/// <summary>
		/// Prefix used for the title of the 'numeric' type of progress control.
		/// </summary>
		[Description("Prefix used for the title of the 'numeric' type of progress control.")]
		[DefaultValue("Step")]
		public string NumericPrefix
		{
			get
			{
				var text = (string)ViewState["NumericPrefix"];

				return string.IsNullOrWhiteSpace(text) ? "Step" : text;
			}
			set
			{
				ViewState["NumericPrefix"] = value;
			}
		}

		/// <summary>
		/// Indicates if the last step should be included in computing the percent of progress.
		/// </summary>
		[Description("Indicates if the last step should be included in computing the percent of progress.")]
		[DefaultValue(false)]
		public bool CountLastStepInProgress
		{
			get { return (bool)(ViewState["CountLastStepInProgress"] ?? true); }
			set { ViewState["CountLastStepInProgress"] = value; }
		}

		protected override HtmlTextWriterTag TagKey
		{
			get { return HtmlTextWriterTag.Div; }
		}

		protected override int CreateChildControls(IEnumerable dataSource, bool dataBinding)
		{
			Controls.Clear();
			
			int count;

			switch (Type.ToLowerInvariant())
			{
				case "numeric":
					count = RenderTypeNumeric(dataSource);
					break;
				case "progressbar":
					count = RenderTypeProgressBar(dataSource);
					break;
				default:
					count = RenderTypeTitle(dataSource);
					break;
			}

			return count;
		}

		protected void AddStep(object dataItem, Control container)
		{
			if (dataItem == null)
			{
				return;
			}

			var title = DataBinder.GetPropertyValue(dataItem, TitleDataPropertyName, null);
			var indexValue = DataBinder.GetPropertyValue(dataItem, IndexDataPropertyName);
			var activeValue = DataBinder.GetPropertyValue(dataItem, ActiveDataPropertyName);
			var completedValue = DataBinder.GetPropertyValue(dataItem, CompletedDataPropertyName);
			var index = Convert.ToInt32(indexValue);
			var active = Convert.ToBoolean(activeValue);
			var completed = Convert.ToBoolean(completedValue);
			var step = new ProgressStep(index, title, active, completed);
			var item = new HtmlGenericControl("li");
			item.AddClass("list-group-item");

			item.InnerHtml = PrependStepIndexToTitle ? string.Format("<span class='number'>{0}</span>{1}", ZeroBasedIndex ? (step.Index + 1) : step.Index, step.Title) : step.Title;
			
			if (step.IsActive)
			{
				item.AddClass("active");
			} 
			else if (step.IsCompleted)
			{
				item.AddClass("text-muted list-group-item-success");
				item.InnerHtml += "<span class='glyphicon glyphicon-ok'></span>";
			}
			else
			{
				item.AddClass("incomplete");
			}

			container.Controls.Add(item);
		}

		protected int RenderTypeTitle(IEnumerable dataSource)
		{
			var count = 0;
			var className = "progress list-group";
			var controlContainer = new HtmlGenericControl("ol");
			var e = dataSource.GetEnumerator();

			if (string.IsNullOrWhiteSpace(Position))
			{
				controlContainer.Attributes["class"] = className;
			}
			else
			{
				switch (Position.ToLowerInvariant())
				{
					case "top":
						className += " top";
						break;
					case "bottom":
						className += " bottom";
						break;
					case "left":
						className += " left";
						CssClass += " col-sm-3 col-md-2";
						break;
					case "right":
						className += " right";
						CssClass += " col-sm-3 col-sm-push-9 col-md-2 col-md-push-10";
						break;
				}

				controlContainer.Attributes["class"] = className;
			}

			while (e.MoveNext())
			{
				AddStep(e.Current, controlContainer);
				count++;
			}

			Controls.Add(controlContainer);

			return count;
		}

		protected int RenderTypeNumeric(IEnumerable dataSource)
		{
			var count = 0;
			var progressIndex = 0;
			var className = "progress-numeric";
			
			var e = dataSource.GetEnumerator();

			if (string.IsNullOrWhiteSpace(Position))
			{
				CssClass = string.Join(" ", CssClass, className);
			}
			else
			{
				switch (Position.ToLowerInvariant())
				{
					case "top":
						className += " top";
						break;
					case "bottom":
						className += " bottom";
						break;
					case "left":
						className += " left";
						CssClass += " col-sm-3 col-md-2";
						break;
					case "right":
						className += " right";
						CssClass += " col-sm-3 col-sm-push-9 col-md-2 col-md-push-10";
						break;
				}

				CssClass = string.Join(" ", CssClass, className);
			}

			while (e.MoveNext())
			{
				if (e.Current == null)
				{
					continue;
				}

				var indexValue = DataBinder.GetPropertyValue(e.Current, IndexDataPropertyName);
				var activeValue = DataBinder.GetPropertyValue(e.Current, ActiveDataPropertyName);
				var index = Convert.ToInt32(indexValue);
				var active = Convert.ToBoolean(activeValue);

				if (active)
				{
					progressIndex = ZeroBasedIndex ? index + 1 : index;
				}

				count++;
			}

			var literal = new LiteralControl { Text = string.Format("{0} <span class='number'>{1}</span> of <span class='number total'>{2}</span>", NumericPrefix, progressIndex, count) };
			
			Controls.Add(literal);

			return count;
		}

		protected int RenderTypeProgressBar(IEnumerable dataSource)
		{
			var count = 0;
			var progressIndex = 0;
			var className = "progress";
			var controlContainer = new HtmlGenericControl("div");
			
			var e = dataSource.GetEnumerator();

			if (string.IsNullOrWhiteSpace(Position))
			{
				CssClass = string.Join(" ", CssClass, className);
			}
			else
			{
				switch (Position.ToLowerInvariant())
				{
					case "top":
						className += " top";
						break;
					case "bottom":
						className += " bottom";
						break;
					case "left":
						className += " left";
						CssClass += " col-sm-3 col-md-2";
						break;
					case "right":
						className += " right";
						CssClass += " col-sm-3 col-sm-push-9 col-md-2 col-md-push-10";
						break;
				}

				CssClass = string.Join(" ", CssClass, className);
			}

			while (e.MoveNext())
			{
				if (e.Current == null)
				{
					continue;
				}

				var indexValue = DataBinder.GetPropertyValue(e.Current, IndexDataPropertyName);
				var activeValue = DataBinder.GetPropertyValue(e.Current, ActiveDataPropertyName);
				var index = Convert.ToInt32(indexValue);
				var active = Convert.ToBoolean(activeValue);
				
				if (active)
				{
					if (ZeroBasedIndex)
					{
						index++;
					}

					progressIndex = index > 0 ? index - 1 : 0;
				}
				
				count++;
			}

			controlContainer.Attributes["class"] = "bar progress-bar";

			double percent = 0;

			if (count > 0)
			{
				if (!CountLastStepInProgress)
				{
					percent = (double)(progressIndex * 100) / (count - 1);
				}
				else
				{
					percent = (double)(progressIndex * 100) / count;
				}
			}

			var progress = Math.Floor(percent);

			controlContainer.Attributes["style"] = string.Format("width: {0}%;", progress);
			controlContainer.Attributes["role"] = "progressbar";
			controlContainer.Attributes["aria-valuemin"] = "0";
			controlContainer.Attributes["aria-valuemax"] = "100";
			controlContainer.Attributes["aria-valuenow"] = progress.ToString(CultureInfo.InvariantCulture);

			if (progress.CompareTo(0) == 0)
			{
				controlContainer.Attributes["class"] = controlContainer.Attributes["class"] + " zero";
			}

			controlContainer.InnerHtml = string.Format("{0}%", progress);

			Controls.Add(controlContainer);

			return count;
		}
	}

	/// <summary>
	/// Step of a progress indicator.
	/// </summary>
	public class ProgressStep
	{
		/// <summary>
		/// Initialization of the ProgressStep class.
		/// </summary>
		public ProgressStep()
		{
		}

		/// <summary>
		/// Initialization of the ProgressStep class.
		/// </summary>
		/// <param name="index">Step index/number</param>
		/// <param name="title">Title of the step</param>
		/// <param name="isActive">True if the step is the current active</param>
		/// <param name="isCompleted">True if the step has been completed</param>
		public ProgressStep(int index, string title, bool isActive, bool isCompleted)
		{
			Index = index;
			Title = title;
			IsActive = isActive;
			IsCompleted = isCompleted;
		}

		/// <summary>
		/// Number of the step.
		/// </summary>
		public int Index { get; set; }

		/// <summary>
		/// Label for the step.
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// Indicates this is the current step.
		/// </summary>
		public bool IsActive { get; set; }

		/// <summary>
		/// Indicates step has been completed.
		/// </summary>
		public bool IsCompleted { get; set; }
	}
}
