/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System.ComponentModel;
using System.Security.Permissions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Adxstudio.Xrm.Tagging;
using System.Security;

namespace Adxstudio.Xrm.Web.UI.WebControls
{
	/// <summary>
	/// Data source control providing tag cloud information (tags, weight, frequency, etc.) for a given tag namespace.
	/// </summary>
	/// <seealso cref="TagCloudDataSourceView"/>
	/// <seealso cref="TagCloudData"/>
	/// <seealso cref="TagCloudDataItem"/>
	[PersistChildren(false)]
	[Description("Data source control providing tag cloud information (tags, weight, frequency, etc.) for a given tag namespace")]
	[ParseChildren(true)]
	[SecurityCritical]
	public class TagCloudDataSource : DataSourceControl
	{
		public const int DefaultNumberOfWeights = 6;
		public const string DefaultWeightCssClassPrefix = "tag-weight-";

		private TagCloudDataSourceView _view;
		private string _weightCssClassPrefix;
		private int? _weights;

		/// <summary>
		/// Gets of sets the number of weight groups/tiers into which results will be divided.
		/// </summary>
		/// <remarks>
		/// The default value of this property is <see cref="DefaultNumberOfWeights"/>.
		/// </remarks>
		[Description("The number of weight groups/tiers into which results will be divided")]
		[Bindable(false)]
		[Category("Data")]
		[DefaultValue(DefaultNumberOfWeights)]
		public int NumberOfWeights
		{
			get { return _weights.HasValue ? _weights.Value : DefaultNumberOfWeights; }
			set { _weights = value; }
		}

		/// <summary>
		/// Gets or sets the <see cref="SortDirection">direction</see> in which to sort results.
		/// </summary>
		[Description("The direction in which to sort results")]
		[Bindable(false)]
		[Category("Data")]
		[DefaultValue(SortDirection.Ascending)]
		public SortDirection SortDirection { get; set; }

		/// <summary>
		/// Gets or sets the expression by which to sort results ("Name" or "TaggedItemCount").
		/// </summary>
		/// <remarks>
		/// <para>
		/// The sort of results defaults to sorting by tag name.
		/// </para>
		/// </remarks>
		[Description("The expression by which to sort results")]
		[Bindable(false)]
		[Category("Data")]
		[DefaultValue((string)null)]
		public string SortExpression { get; set; }

		/// <summary>
		/// Gets or sets the CSS class prefix to which the weight of a tag result item will be appended.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This value defaults to "tag-weight-". So, for example, items returned by this control, by default,
		/// will have a CssClass property of "tag-weight-1", "tag-weight-2", etc.
		/// </para>
		/// </remarks>
		[Description("The CSS class prefix to which the weight of a tag result item will be appended")]
		[Bindable(false)]
		[Category("Data")]
		[DefaultValue(DefaultWeightCssClassPrefix)]
		public string WeightCssClassPrefix
		{
			get { return string.IsNullOrEmpty(_weightCssClassPrefix) ? DefaultWeightCssClassPrefix : _weightCssClassPrefix; }
			set { _weightCssClassPrefix = value; }
		}

		protected override DataSourceView GetView(string viewName)
		{
			return _view ?? (_view = new TagCloudDataSourceView(this, viewName));
		}

		protected override void LoadViewState(object savedState)
		{
			var state = (Pair)savedState;

			if (savedState == null)
			{
				base.LoadViewState(null);
			}
			else
			{
				base.LoadViewState(state.First);

				var parameters = state.Second as object[];

				if (parameters != null)
				{
					NumberOfWeights = (int)parameters[0];
					SortDirection = (SortDirection)parameters[1];
					SortExpression = parameters[2] as string;
					WeightCssClassPrefix = parameters[3] as string;
				}
			}
		}

		protected override object SaveViewState()
		{
			var state = new Pair
			{
				First = base.SaveViewState(),
				Second = new object[]
				{
					NumberOfWeights,
					SortDirection,
					SortExpression,
					WeightCssClassPrefix
				}
			};

			if ((state.First == null) && (state.Second == null))
			{
				return null;
			}

			return state;
		}
	}
}
