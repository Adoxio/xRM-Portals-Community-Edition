/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Web.UI;
using System.ComponentModel;
using System.Drawing.Design;
using System.Web.Caching;
using System.Globalization;
using Microsoft.Xrm.Client;

namespace Microsoft.Xrm.Portal.Web.UI.WebControls
{
	public sealed class CacheParameters : IStateManager // MSBug #120086: Can't make internal, public on CrmMetadataDataSource.
	{
		private DateTime _absoluteExpiration;
		
		[TypeConverter(typeof(AbsoluteExpirationConverter))]
		[DefaultValue("NoAbsoluteExpiration")]
		public DateTime AbsoluteExpiration
		{
			get { return _absoluteExpiration; }

			set
			{
				// there is a bug in the TypeConverter where NoAbsoluteExpiration is off by 1 ms
				if ((Cache.NoAbsoluteExpiration - value) < TimeSpan.FromMilliseconds(1))
				{
					_absoluteExpiration = Cache.NoAbsoluteExpiration;
				}
				else
				{
					_absoluteExpiration = value;
				}
			}
		}

		private TimeSpan _slidingExpiration;

		[TypeConverter(typeof(SlidingExpirationConverter))]
		[DefaultValue("NoSlidingExpiration")]
		public TimeSpan SlidingExpiration
		{
			get { return _slidingExpiration; }
			set { _slidingExpiration = value; }
		}

		private CacheItemPriority _priority;

		[DefaultValue(CacheItemPriority.Default)]
		public CacheItemPriority Priority
		{
			get { return _priority; }
			set { _priority = value; }
		}

		private TimeSpan _duration;

		public TimeSpan Duration
		{
			get { return _duration; }
			set { _duration = value; }
		}

		private bool _enabled;

		[DefaultValue(true)]
		public bool Enabled
		{
			get { return _enabled; }
			set { _enabled = value; }
		}

		private CacheKey _cacheKey;

		public CacheKey CacheKey
		{
			get
			{
				if (_cacheKey == null)
				{
					_cacheKey = new CacheKey();

					if (IsTrackingViewState)
					{
						((IStateManager)_cacheKey).TrackViewState();
					}
				}

				return _cacheKey;
			}
		}

		private StateManagedCollection<CacheKeyDependency> _dependencies;

		[Description(""), Category("Data"), PersistenceMode(PersistenceMode.InnerProperty), DefaultValue((string)null), Editor("System.Web.UI.Design.WebControls.ParameterCollectionEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), MergableProperty(false)]
		public StateManagedCollection<CacheKeyDependency> Dependencies
		{
			get
			{
				if (_dependencies == null)
				{
					_dependencies = new StateManagedCollection<CacheKeyDependency>();

					if (IsTrackingViewState)
					{
						((IStateManager)_dependencies).TrackViewState();
					}
				}

				return _dependencies;
			}
		}

		private StateManagedCollection<CacheKeyDependency> _itemDependencies;

		[Description(""), Category("Data"), PersistenceMode(PersistenceMode.InnerProperty), DefaultValue((string)null), Editor("System.Web.UI.Design.WebControls.ParameterCollectionEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), MergableProperty(false)]
		public StateManagedCollection<CacheKeyDependency> ItemDependencies
		{
			get
			{
				if (_itemDependencies == null)
				{
					_itemDependencies = new StateManagedCollection<CacheKeyDependency>();

					if (IsTrackingViewState)
					{
						((IStateManager)_itemDependencies).TrackViewState();
					}
				}

				return _itemDependencies;
			}
		}

		public CacheParameters()
		{
			AbsoluteExpiration = Cache.NoAbsoluteExpiration;
			SlidingExpiration = Cache.NoSlidingExpiration;
			Duration = Cache.NoSlidingExpiration;
			Priority = CacheItemPriority.Default;
			Enabled = true;
		}

		/// <summary>
		/// Retrieves the absolute expiration or the duration expiration.
		/// </summary>
		/// <returns></returns>
		public DateTime GetExpiration()
		{
			if (AbsoluteExpiration != Cache.NoAbsoluteExpiration && Duration != Cache.NoSlidingExpiration)
			{
				throw new ArgumentException("'AbsoluteExpiration' must be '{0}' or 'Duration' must be '{1}'.".FormatWith("NoAbsoluteExpiration", Cache.NoSlidingExpiration));
			}

			if (Duration != Cache.NoSlidingExpiration)
			{
				return DateTime.Now + Duration;
			}
			else
			{
				return AbsoluteExpiration;
			}
		}

		#region IStateManager Members

		private bool _tracking;

		bool IStateManager.IsTrackingViewState
		{
			get { return IsTrackingViewState; }
		}

		public bool IsTrackingViewState
		{
			get { return _tracking; }
		}

		void IStateManager.LoadViewState(object savedState)
		{
			LoadViewState(savedState);
		}

		private void LoadViewState(object savedState)
		{
			object[] state = savedState as object[];

			if (state == null)
			{
				return;
			}

			Enabled = (bool)state[0];
			((IStateManager)Dependencies).LoadViewState(state[1]);
			((IStateManager)ItemDependencies).LoadViewState(state[2]);
		}

		object IStateManager.SaveViewState()
		{
			return SaveViewState();
		}

		private object SaveViewState()
		{
			object[] state = new object[3];
			state[0] = Enabled;
			state[1] = ((IStateManager)Dependencies).SaveViewState();
			state[2] = ((IStateManager)ItemDependencies).SaveViewState();

			return state;
		}

		void IStateManager.TrackViewState()
		{
			TrackViewState();
		}

		private void TrackViewState()
		{
			_tracking = true;
			((IStateManager)Dependencies).TrackViewState();
			((IStateManager)ItemDependencies).TrackViewState();
		}

		#endregion
	}

	public class SlidingExpirationConverter : TimeSpanConverter
	{
		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (value is string && string.Compare(value as string, "NoSlidingExpiration", StringComparison.InvariantCultureIgnoreCase) == 0)
			{
				return Cache.NoSlidingExpiration;
			}

			if (value is TimeSpan && ((TimeSpan)value) == Cache.NoSlidingExpiration)
			{
				return "NoSlidingExpiration";
			}

			return base.ConvertFrom(context, culture, value);
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == typeof(string) && (value is TimeSpan) && ((TimeSpan)value) == Cache.NoSlidingExpiration)
			{
				return "NoSlidingExpiration";
			}

			if (destinationType == typeof(TimeSpan) && (value is string) && string.Compare(value as string, "NoSlidingExpiration", StringComparison.InvariantCultureIgnoreCase) == 0)
			{
				return Cache.NoSlidingExpiration;
			}

			return base.ConvertTo(context, culture, value, destinationType);
		}
	}

	public class AbsoluteExpirationConverter : DateTimeConverter
	{
		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (value is string && string.Compare(value as string, "NoAbsoluteExpiration", StringComparison.InvariantCultureIgnoreCase) == 0)
			{
				return Cache.NoAbsoluteExpiration;
			}

			if (value is DateTime && ((DateTime)value) == Cache.NoAbsoluteExpiration)
			{
				return "NoAbsoluteExpiration";
			}

			return base.ConvertFrom(context, culture, value);
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == typeof(string) && (value is DateTime) && ((DateTime)value) == Cache.NoAbsoluteExpiration)
			{
				return "NoAbsoluteExpiration";
			}

			if (destinationType == typeof(DateTime) && (value is string) && string.Compare(value as string, "NoAbsoluteExpiration", StringComparison.InvariantCultureIgnoreCase) == 0)
			{
				return Cache.NoAbsoluteExpiration;
			}

			return base.ConvertTo(context, culture, value, destinationType);
		}
	}
}
