/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using Adxstudio.Xrm.Cms;
using Adxstudio.Xrm.Web.Mvc;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;

namespace Adxstudio.Xrm.Metadata
{
	/// <summary>
	/// Provides custom formatting for <see cref="Money"/> objects based on the
	/// current CRM organization base currency.
	/// </summary>
	public class BaseCurrencyMoneyFormatter : ICustomFormatter, IFormatProvider
	{
		private readonly CultureInfo _culture;
		private readonly Lazy<string> _currencySymbol;
		private readonly Lazy<bool> _currencySymbolComesFirst;
		private readonly Lazy<int> _precision;
		
		public BaseCurrencyMoneyFormatter(IOrganizationMoneyFormatInfo organization, CultureInfo culture = null)
		{
			if (organization == null) throw new ArgumentNullException("organization");

			Organization = organization;
			_culture = culture ?? CultureInfo.CurrentCulture;

			_currencySymbol = new Lazy<string>(GetCurrencySymbol, LazyThreadSafetyMode.None);
			_currencySymbolComesFirst = new Lazy<bool>(GetCurrencySymbolComesFirst, LazyThreadSafetyMode.None);
			_precision = new Lazy<int>(GetPrecision, LazyThreadSafetyMode.None);
		}

		/// <summary>
		/// Gets the currency symbol for the current format.
		/// </summary>
		public string CurrencySymbol
		{
			get { return _currencySymbol.Value; }
		}

		/// <summary>
		/// Gets whether or not the currency symbol comes before or after the value in the current format.
		/// </summary>
		public bool CurrencySymbolComesFirst
		{
			get { return _currencySymbolComesFirst.Value; }
		}

		/// <summary>
		/// Gets currency format metadata for the CRM organization.
		/// </summary>
		protected IOrganizationMoneyFormatInfo Organization { get; private set; }

		/// <summary>
		/// Gets the configured decimal precision for the current format.
		/// </summary>
		protected int Precision
		{
			get { return _precision.Value; }
		}

		/// <summary>
		/// Converts the value of a specified object to an equivalent string representation using specified
		/// format and culture-specific formatting information.
		/// </summary>
		/// <param name="format">A format string containing formatting specifications.</param>
		/// <param name="arg">An object to format.</param>
		/// <param name="formatProvider">An object that supplies format information about the current instance.</param>
		/// <returns>The formatted string value of the given object.</returns>
		public string Format(string format, object arg, IFormatProvider formatProvider)
		{
			var money = arg as Money;

			// This formatter only directly handles Money values.
			if (money == null)
			{
				return HandleOtherFormats(format, arg);
			}

			// The "N" format string allows formatting a currency value without the currency symbol.
			if (string.IsNullOrEmpty(CurrencySymbol) || string.Equals(format, "N", StringComparison.OrdinalIgnoreCase))
			{
				return money.Value.ToString("N{0}".FormatWith(Precision), _culture);
			}

			// Format the currency value according to .NET culture currency formatting, but replace
			// the currency symbol provided by .NET with the correct symbol from CRM.
			return money.Value
				.ToString("C{0}".FormatWith(Precision), _culture)
				.Replace(_culture.NumberFormat.CurrencySymbol, CurrencySymbol);
		}

		/// <summary>
		/// Returns an object that provides formatting services for the specified type.
		/// </summary>
		/// <param name="formatType">An object that specifies the type of format object to return.</param>
		/// <returns>
		/// An instance of the object specified by formatType, if the IFormatProvider implementation can
		/// supply that type of object; otherwise, null.
		/// </returns>
		public object GetFormat(Type formatType)
		{
			return formatType == typeof(ICustomFormatter) ? this : null;
		}

		/// <summary>
		/// Gets the currency symbol for the current format.
		/// </summary>
		/// <remarks>
		/// This is computed once per instance, and then the result is stored for subsequent uses.
		/// </remarks>
		protected virtual string GetCurrencySymbol()
		{
			return Organization.CurrencySymbol;
		}

		/// <summary>
		/// Gets the configured decimal precision for the current format.
		/// </summary>
		/// <remarks>
		/// This is computed once per instance, and then the result is stored for subsequent uses.
		/// </remarks>
		protected virtual int GetPrecision()
		{
			return Organization.CurrencyPrecision.GetValueOrDefault(2);
		}

		/// <summary>
		/// Determines whether the currency symbol is rendered before or after the currency value
		/// in the currency formatting for the given culture.
		/// </summary>
		/// <remarks>
		/// This is computed once per instance, and then the result is stored for subsequent uses.
		/// </remarks>
		private bool GetCurrencySymbolComesFirst()
		{
			return Regex.IsMatch(
				new decimal(1.00).ToString("C", _culture),
				string.Format(@"^\s*{0}", Regex.Escape(_culture.NumberFormat.CurrencySymbol)));
		}

		/// <summary>
		/// Handles culture-aware formatting of all non-currency values.
		/// </summary>
		/// <param name="format">A format string containing formatting specifications.</param>
		/// <param name="arg">An object to format.</param>
		/// <returns>The formatted string value of the given object.</returns>
		private string HandleOtherFormats(string format, object arg)
		{
			var formattable = arg as IFormattable;

			if (formattable != null)
			{
				return formattable.ToString(format, _culture);
			}

			return arg != null ? arg.ToString() : string.Empty;
		}
	}

	/// <summary>
	/// Provides custom formatting for <see cref="Money"/> objects based on the
	/// transaction currency of a given CRM entity record.
	/// </summary>
	public class MoneyFormatter : BaseCurrencyMoneyFormatter
	{
		private readonly int? _attributePrecision;
		private readonly bool _isBaseCurrency;
		private readonly int? _precisionSource;
		private readonly IMoneyFormatInfo _record;
		
		public MoneyFormatter(IOrganizationMoneyFormatInfo organization, IMoneyFormatInfo record, int? attributePrecision = null, int? precisionSource = null, bool isBaseCurrency = false, CultureInfo culture = null)
			: base(organization, culture)
		{
			if (record == null) throw new ArgumentNullException("record");

			_attributePrecision = attributePrecision;
			_precisionSource = precisionSource;
			_record = record;
			_isBaseCurrency = isBaseCurrency;
		}

		public MoneyFormatter(IOrganizationMoneyFormatInfo organization, IMoneyFormatInfo record, MoneyAttributeMetadata attributeMetadata, CultureInfo culture = null)
			: this(organization, record, attributeMetadata.Precision, attributeMetadata.PrecisionSource, attributeMetadata.IsBaseCurrency.GetValueOrDefault(), culture)
		{
			if (attributeMetadata == null) throw new ArgumentNullException("attributeMetadata");
		}

		/// <summary>
		/// Gets the currency symbol for the current format.
		/// </summary>
		/// <remarks>
		/// This is computed once per instance, and then the result is stored for subsequent uses.
		/// </remarks>
		protected override string GetCurrencySymbol()
		{
			return _isBaseCurrency ? Organization.CurrencySymbol : _record.CurrencySymbol;
		}

		/// <summary>
		/// Gets the configured decimal precision for the current format.
		/// </summary>
		/// <remarks>
		/// This is computed once per instance, and then the result is stored for subsequent uses.
		/// </remarks>
		protected override int GetPrecision()
		{
			switch (_precisionSource)
			{
				// Use field-level precision setting.
				case 0:
					return _attributePrecision.GetValueOrDefault(2);
				// Use organization pricing precision setting.
				case 1:
					return Organization.PricingDecimalPrecision.GetValueOrDefault(2);
				// _precisionSource == 2, use currency precision setting.
				default:
					return (_isBaseCurrency ? Organization.CurrencyPrecision : _record.CurrencyPrecision).GetValueOrDefault(2);
			}
		}
	}

	/// <summary>
	/// Provides metadata for formatting a currency value.
	/// </summary>
	public interface IMoneyFormatInfo
	{
		/// <summary>
		/// Gets the configured decimal precision for a currency format.
		/// </summary>
		int? CurrencyPrecision { get; }

		/// <summary>
		/// Gets the configured currency symbol for a currency format.
		/// </summary>
		string CurrencySymbol { get; }
	}

	/// <summary>
	/// Provides metadata for formatting currency values based on CRM organization settings.
	/// </summary>
	public interface IOrganizationMoneyFormatInfo : IMoneyFormatInfo
	{
		/// <summary>
		/// Gets the pricingdecimalprecision setting for a CRM organization.
		/// </summary>
		int? PricingDecimalPrecision { get; }
	}

	public abstract class MoneyFormatInfo : IMoneyFormatInfo
	{
		private readonly Lazy<Entity> _currency;
		private readonly Lazy<EntityReference> _currencyReference;
		private readonly Func<OrganizationServiceContext> _getServiceContext;

		protected MoneyFormatInfo(IPortalViewContext portalViewContext) : this()
		{
			if (portalViewContext == null) throw new ArgumentNullException("portalViewContext");

			_getServiceContext = portalViewContext.CreateServiceContext;
		}

		protected MoneyFormatInfo(IDataAdapterDependencies dataAdapterDependencies) : this()
		{
			if (dataAdapterDependencies == null) throw new ArgumentNullException("dataAdapterDependencies");

			_getServiceContext = dataAdapterDependencies.GetServiceContext;
		}

		protected MoneyFormatInfo(OrganizationServiceContext serviceContext) : this()
		{
			if (serviceContext == null) throw new ArgumentNullException("serviceContext");

			_getServiceContext = () => serviceContext;
		}

		private MoneyFormatInfo()
		{
			_currency = new Lazy<Entity>(GetCurrency, LazyThreadSafetyMode.None);
			_currencyReference = new Lazy<EntityReference>(GetCurrencyReference, LazyThreadSafetyMode.None);
		}

		/// <summary>
		/// Gets the configured decimal precision for a currency format.
		/// </summary>
		public virtual string CurrencySymbol
		{
			get { return Currency == null ? null : Currency.GetAttributeValue<string>("currencysymbol"); }
		}

		/// <summary>
		/// Gets the configured currency symbol for a currency format.
		/// </summary>
		public virtual int? CurrencyPrecision
		{
			get { return Currency == null ? null : Currency.GetAttributeValue<int?>("currencyprecision"); }
		}

		/// <summary>
		/// Gets the CRM transactioncurrency record for a currency format.
		/// </summary>
		protected Entity Currency
		{
			get { return _currency.Value; }
		}

		/// <summary>
		/// Executes a CRM SDK request.
		/// </summary>
		protected TResponse Execute<TResponse>(OrganizationRequest request) where TResponse : OrganizationResponse
		{
			return (TResponse)_getServiceContext().Execute(request);
		}

		/// <summary>
		/// Gets a reference to the relevant CRM transactioncurrency for this format.
		/// </summary>
		/// <remarks>
		/// This is computed once per instance, and then the result is stored for subsequent uses.
		/// </remarks>
		protected abstract EntityReference GetCurrencyReference();

		/// <summary>
		/// Retrives the CRM transactioncurrency for this format, with necessary attributes.
		/// </summary>
		private Entity GetCurrency()
		{
			if (_currencyReference.Value == null)
			{
				return null;
			}

			var response = Execute<RetrieveResponse>(new RetrieveRequest
			{
				Target = _currencyReference.Value,
				ColumnSet = new ColumnSet("currencysymbol", "currencyprecision")
			});

			return response == null
				? null
				: response.Entity;
		}
	}

	/// <summary>
	/// Provides metadata for formatting currency values based on CRM organization base currency and settings.
	/// </summary>
	public class OrganizationMoneyFormatInfo : MoneyFormatInfo, IOrganizationMoneyFormatInfo
	{
		private readonly Lazy<Entity> _organization;

		public OrganizationMoneyFormatInfo(IPortalViewContext portalViewContext) : base(portalViewContext)
		{
			_organization = new Lazy<Entity>(GetOrganization, LazyThreadSafetyMode.None);
		}

		public OrganizationMoneyFormatInfo(IDataAdapterDependencies dataAdapterDependencies) : base(dataAdapterDependencies)
		{
			_organization = new Lazy<Entity>(GetOrganization, LazyThreadSafetyMode.None);
		}

		public OrganizationMoneyFormatInfo(OrganizationServiceContext serviceContext) : base(serviceContext)
		{
			_organization = new Lazy<Entity>(GetOrganization, LazyThreadSafetyMode.None);
		}

		/// <summary>
		/// Gets the pricingdecimalprecision setting for a CRM organization.
		/// </summary>
		public int? PricingDecimalPrecision
		{
			get { return Organization == null ? null : Organization.GetAttributeValue<int?>("pricingdecimalprecision"); }
		}

		/// <summary>
		/// Gets the CRM organizaction record that defines this format.
		/// </summary>
		protected Entity Organization
		{
			get { return _organization.Value; }
		}
		
		/// <summary>
		/// Gets a reference to the relevant CRM transactioncurrency for this format.
		/// </summary>
		/// <remarks>
		/// This is computed once per instance, and then the result is stored for subsequent uses.
		/// </remarks>
		protected override EntityReference GetCurrencyReference()
		{
			return Organization == null
				? null
				: Organization.GetAttributeValue<EntityReference>("basecurrencyid");
		}

		/// <summary>
		/// Gets the CRM organizaction record that defines this format.
		/// </summary>
		/// <remarks>
		/// This is computed once per instance, and then the result is stored for subsequent uses.
		/// </remarks>
		private Entity GetOrganization()
		{
			var whoAmI = Execute<WhoAmIResponse>(new WhoAmIRequest());

			if (whoAmI == null)
			{
				return null;
			}

			var response = Execute<RetrieveResponse>(new RetrieveRequest
			{
				Target = new EntityReference("organization", whoAmI.OrganizationId),
				ColumnSet = new ColumnSet("basecurrencyid", "pricingdecimalprecision")
			});

			return response == null
				? null
				: response.Entity;
		}
	}

	/// <summary>
	/// Provides metadata for formatting currency values based on CRM entity record currency.
	/// </summary>
	public class EntityRecordMoneyFormatInfo : MoneyFormatInfo
	{
		private readonly Entity _record;

		public EntityRecordMoneyFormatInfo(IPortalViewContext portalViewContext, Entity record) : base(portalViewContext)
		{
			if (record == null) throw new ArgumentNullException("record");

			_record = record;
		}

		public EntityRecordMoneyFormatInfo(IDataAdapterDependencies dataAdapterDependencies, Entity record) : base(dataAdapterDependencies)
		{
			if (record == null) throw new ArgumentNullException("record");

			_record = record;
		}

		public EntityRecordMoneyFormatInfo(OrganizationServiceContext serviceContext, Entity record) : base(serviceContext)
		{
			if (record == null) throw new ArgumentNullException("record");

			_record = record;
		}

		/// <summary>
		/// Gets a reference to the relevant CRM transactioncurrency for this format.
		/// </summary>
		/// <remarks>
		/// This is computed once per instance, and then the result is stored for subsequent uses.
		/// </remarks>
		protected override EntityReference GetCurrencyReference()
		{
			var currency = _record.GetAttributeValue<EntityReference>("transactioncurrencyid");

			if (currency != null)
			{
				return currency;
			}

			var response = Execute<RetrieveResponse>(new RetrieveRequest
			{
				Target = _record.ToEntityReference(),
				ColumnSet = new ColumnSet("transactioncurrencyid")
			});

			return response == null
				? null
				: response.Entity.GetAttributeValue<EntityReference>("transactioncurrencyid");
		}
	}
}
