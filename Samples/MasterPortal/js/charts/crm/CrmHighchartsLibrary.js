/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

Type.registerNamespace('Microsoft.Crm.Client');

Microsoft.Crm.Client.DataSourceFactory = function Microsoft_Crm_Client_DataSourceFactory() {
}
Microsoft.Crm.Client.DataSourceFactory.get_instance = function Microsoft_Crm_Client_DataSourceFactory$get_instance() {
	if (!Microsoft.Crm.Client.DataSourceFactory._instance$p) {
		Microsoft.Crm.Client.DataSourceFactory._instance$p = new Microsoft.Crm.Client.DataSourceFactory();
	}
	return Microsoft.Crm.Client.DataSourceFactory._instance$p;
}
Microsoft.Crm.Client.DataSourceFactory.prototype = {

	getChartingDataSource: function Microsoft_Crm_Client_DataSourceFactory$getChartingDataSource() {
		return new Microsoft.Crm.Client.Core.Storage.DataApi.RetrieveEntityMetadataDataSource();
	}
}


Type.registerNamespace('Microsoft.Crm.Client.Core.Framework');

Microsoft.Crm.Client.Core.Framework.ChartDisplayMode = function () { }
Microsoft.Crm.Client.Core.Framework.ChartDisplayMode.prototype = {
	normal: 0,
	error: 1
}
Microsoft.Crm.Client.Core.Framework.ChartDisplayMode.registerEnum('Microsoft.Crm.Client.Core.Framework.ChartDisplayMode', false);


Microsoft.Crm.Client.Core.Framework.IRootModel = function () { }
Microsoft.Crm.Client.Core.Framework.IRootModel.registerInterface('Microsoft.Crm.Client.Core.Framework.IRootModel');


Microsoft.Crm.Client.Core.Framework.INotifyPropertyChanged = function () { }
Microsoft.Crm.Client.Core.Framework.INotifyPropertyChanged.registerInterface('Microsoft.Crm.Client.Core.Framework.INotifyPropertyChanged');


Microsoft.Crm.Client.Core.Framework.ITrace = function () { }
Microsoft.Crm.Client.Core.Framework.ITrace.registerInterface('Microsoft.Crm.Client.Core.Framework.ITrace');


Microsoft.Crm.Client.Core.Framework.IUserDateTimeUtils = function () { }
Microsoft.Crm.Client.Core.Framework.IUserDateTimeUtils.registerInterface('Microsoft.Crm.Client.Core.Framework.IUserDateTimeUtils');


Microsoft.Crm.Client.Core.Framework.ChartError = function Microsoft_Crm_Client_Core_Framework_ChartError(title, message) {
	Microsoft.Crm.Client.Core.Framework.ChartError.initializeBase(this, [Microsoft.Crm.Client.Core.Framework.ChartError.typeName, message]);
	this.set_title(title);
	this.set_description(message);
}
Microsoft.Crm.Client.Core.Framework.ChartError.isInstanceOfType = function Microsoft_Crm_Client_Core_Framework_ChartError$isInstanceOfType(ex) {
	return Microsoft.Crm.Client.Core.Framework.ErrorInfo.isInstanceOfType(ex, Microsoft.Crm.Client.Core.Framework.ChartError.typeName);
}
Microsoft.Crm.Client.Core.Framework.ChartError.fromException = function Microsoft_Crm_Client_Core_Framework_ChartError$fromException(ex) {
	return Microsoft.Crm.Client.Core.Framework.ErrorInfo.fromException(Microsoft.Crm.Client.Core.Framework.ChartError, ex, Microsoft.Crm.Client.Core.Framework.ChartError.typeName);
}
Microsoft.Crm.Client.Core.Framework.ChartError.prototype = {

	get_title: function Microsoft_Crm_Client_Core_Framework_ChartError$get_title() {
		return this.get_dictionary()[Microsoft.Crm.Client.Core.Framework.ChartError._errorTitleKey$p];
	},

	set_title: function Microsoft_Crm_Client_Core_Framework_ChartError$set_title(value) {
		this.get_dictionary()[Microsoft.Crm.Client.Core.Framework.ChartError._errorTitleKey$p] = value;
		return value;
	},

	get_description: function Microsoft_Crm_Client_Core_Framework_ChartError$get_description() {
		return this.get_dictionary()[Microsoft.Crm.Client.Core.Framework.ChartError._errorDescriptionKey$p];
	},

	set_description: function Microsoft_Crm_Client_Core_Framework_ChartError$set_description(value) {
		this.get_dictionary()[Microsoft.Crm.Client.Core.Framework.ChartError._errorDescriptionKey$p] = value;
		return value;
	}
}


Microsoft.Crm.Client.Core.Framework.ChartErrorInformation = function Microsoft_Crm_Client_Core_Framework_ChartErrorInformation() {
}
Microsoft.Crm.Client.Core.Framework.ChartErrorInformation.prototype = {
	_errorType$p$0: null,
	_errorDescription$p$0: null,

	get_ErrorType: function Microsoft_Crm_Client_Core_Framework_ChartErrorInformation$get_ErrorType() {
		return this._errorType$p$0;
	},

	set_ErrorType: function Microsoft_Crm_Client_Core_Framework_ChartErrorInformation$set_ErrorType(value) {
		this._errorType$p$0 = value;
		return value;
	},

	get_ErrorDescription: function Microsoft_Crm_Client_Core_Framework_ChartErrorInformation$get_ErrorDescription() {
		return this._errorDescription$p$0;
	},

	set_ErrorDescription: function Microsoft_Crm_Client_Core_Framework_ChartErrorInformation$set_ErrorDescription(value) {
		this._errorDescription$p$0 = value;
		return value;
	}
}


Microsoft.Crm.Client.Core.Framework.ErrorInfo = function Microsoft_Crm_Client_Core_Framework_ErrorInfo(name, message) {
	Microsoft.Crm.Client.Core.Framework.Utils.ExceptionHelpers.throwOnNullOrEmptyArgument(name, 'name');
	this._dictionary$p$0 = {};
	this.set_errorName(name);
	this.set_message(name + ': ' + message);
}
Microsoft.Crm.Client.Core.Framework.ErrorInfo.isInstanceOfType = function Microsoft_Crm_Client_Core_Framework_ErrorInfo$isInstanceOfType(ex, typeName) {
	return !_Script.isNullOrUndefined(ex) && typeName === ex[Microsoft.Crm.Client.Core.Framework.ErrorInfo._nameKey$p];
}
Microsoft.Crm.Client.Core.Framework.ErrorInfo.fromException = function Microsoft_Crm_Client_Core_Framework_ErrorInfo$fromException(TError, ex, typeName) {
	Microsoft.Crm.Client.Core.Framework.Utils.ExceptionHelpers.throwOnNullArgument(ex, 'ex');
	if (Microsoft.Crm.Client.Core.Framework.ErrorInfo.isInstanceOfType(ex, typeName)) {
		var error = new (TError)();
		error._dictionary$p$0 = ex;
		return error;
	}
	else {
		throw Error.argument('ex', String.format('Exception must be of type \'{0}\' to successfully convert.', typeName));
	}
}
Microsoft.Crm.Client.Core.Framework.ErrorInfo.prototype = {
	_dictionary$p$0: null,

	get_errorName: function Microsoft_Crm_Client_Core_Framework_ErrorInfo$get_errorName() {
		return this.get_dictionary()[Microsoft.Crm.Client.Core.Framework.ErrorInfo._nameKey$p];
	},

	set_errorName: function Microsoft_Crm_Client_Core_Framework_ErrorInfo$set_errorName(value) {
		this.get_dictionary()[Microsoft.Crm.Client.Core.Framework.ErrorInfo._nameKey$p] = value;
		return value;
	},

	get_message: function Microsoft_Crm_Client_Core_Framework_ErrorInfo$get_message() {
		return this.get_dictionary()[Microsoft.Crm.Client.Core.Framework.ErrorInfo._messageKey$p];
	},

	set_message: function Microsoft_Crm_Client_Core_Framework_ErrorInfo$set_message(value) {
		this.get_dictionary()[Microsoft.Crm.Client.Core.Framework.ErrorInfo._messageKey$p] = value;
		return value;
	},

	get_dictionary: function Microsoft_Crm_Client_Core_Framework_ErrorInfo$get_dictionary() {
		return this._dictionary$p$0;
	},

	toException: function Microsoft_Crm_Client_Core_Framework_ErrorInfo$toException() {
		return Error.create(this.get_message(), this.get_dictionary());
	}
}


Microsoft.Crm.Client.Core.Framework.FeatureName = function Microsoft_Crm_Client_Core_Framework_FeatureName() {
}


Microsoft.Crm.Client.Core.Framework.DictionaryWrapper = function Microsoft_Crm_Client_Core_Framework_DictionaryWrapper(dictionary) {
	this._dictionary$p$0 = dictionary;
}
Microsoft.Crm.Client.Core.Framework.DictionaryWrapper.prototype = {
	_dictionary$p$0: null,

	get_dictionary: function Microsoft_Crm_Client_Core_Framework_DictionaryWrapper$get_dictionary() {
		return this._dictionary$p$0;
	}
}


Microsoft.Crm.Client.Core.Framework.DisposableBase = function Microsoft_Crm_Client_Core_Framework_DisposableBase() {
}
Microsoft.Crm.Client.Core.Framework.DisposableBase.prototype = {
	_isDisposed$p$0: false,

	get_isDisposed: function Microsoft_Crm_Client_Core_Framework_DisposableBase$get_isDisposed() {
		return this._isDisposed$p$0;
	},

	dispose: function Microsoft_Crm_Client_Core_Framework_DisposableBase$dispose() {
		if (!this._isDisposed$p$0) {
			this.internalDispose();
			this._isDisposed$p$0 = true;
		}
	}
}


Type.registerNamespace('Microsoft.Crm.Client.Core.Storage.Common');

Microsoft.Crm.Client.Core.Storage.Common.IAttributeMetadata = function () { }
Microsoft.Crm.Client.Core.Storage.Common.IAttributeMetadata.registerInterface('Microsoft.Crm.Client.Core.Storage.Common.IAttributeMetadata');


Microsoft.Crm.Client.Core.Storage.Common.IColumnSet = function () { }
Microsoft.Crm.Client.Core.Storage.Common.IColumnSet.registerInterface('Microsoft.Crm.Client.Core.Storage.Common.IColumnSet');


Microsoft.Crm.Client.Core.Storage.Common.IEntityMetadata = function () { }
Microsoft.Crm.Client.Core.Storage.Common.IEntityMetadata.registerInterface('Microsoft.Crm.Client.Core.Storage.Common.IEntityMetadata');


Microsoft.Crm.Client.Core.Storage.Common.StorageConstants = function Microsoft_Crm_Client_Core_Storage_Common_StorageConstants() {
}


Microsoft.Crm.Client.Core.Storage.Common.EntityAttributeMetadataPair = function Microsoft_Crm_Client_Core_Storage_Common_EntityAttributeMetadataPair(entityMetadata, attributeMetadataCollection) {
	this._entityMetadata$p$0 = entityMetadata;
	this._attributeMetadataCollection$p$0 = attributeMetadataCollection;
}
Microsoft.Crm.Client.Core.Storage.Common.EntityAttributeMetadataPair.prototype = {
	_entityMetadata$p$0: null,
	_attributeMetadataCollection$p$0: null,

	get_entityMetadata: function Microsoft_Crm_Client_Core_Storage_Common_EntityAttributeMetadataPair$get_entityMetadata() {
		return this._entityMetadata$p$0;
	},

	get_attributeMetadataCollection: function Microsoft_Crm_Client_Core_Storage_Common_EntityAttributeMetadataPair$get_attributeMetadataCollection() {
		return this._attributeMetadataCollection$p$0;
	}
}


Microsoft.Crm.Client.Core.Storage.Common.AllColumns = function Microsoft_Crm_Client_Core_Storage_Common_AllColumns() {
}
Microsoft.Crm.Client.Core.Storage.Common.AllColumns.get_instance = function Microsoft_Crm_Client_Core_Storage_Common_AllColumns$get_instance() {
	return Microsoft.Crm.Client.Core.Storage.Common.AllColumns._instance$p || (Microsoft.Crm.Client.Core.Storage.Common.AllColumns._instance$p = new Microsoft.Crm.Client.Core.Storage.Common.AllColumns());
}
Microsoft.Crm.Client.Core.Storage.Common.AllColumns.prototype = {

	get_isEmpty: function Microsoft_Crm_Client_Core_Storage_Common_AllColumns$get_isEmpty() {
		return false;
	},

	getDifference: function Microsoft_Crm_Client_Core_Storage_Common_AllColumns$getDifference(otherColumnSet) {
		return new Microsoft.Crm.Client.Core.Storage.Common.ColumnSet(new Array(0));
	},

	toString: function Microsoft_Crm_Client_Core_Storage_Common_AllColumns$toString() {
		return Microsoft.Crm.Client.Core.Framework._String.empty;
	}
}


Microsoft.Crm.Client.Core.Storage.Common.ColumnSet = function Microsoft_Crm_Client_Core_Storage_Common_ColumnSet(columnNames) {
	this._columns$p$0 = columnNames;
}
Microsoft.Crm.Client.Core.Storage.Common.ColumnSet.createFromObjectData = function Microsoft_Crm_Client_Core_Storage_Common_ColumnSet$createFromObjectData(data) {
	return new Microsoft.Crm.Client.Core.Storage.Common.ColumnSet(data['columns']);
}
Microsoft.Crm.Client.Core.Storage.Common.ColumnSet.prototype = {
	_columns$p$0: null,

	get_columns: function Microsoft_Crm_Client_Core_Storage_Common_ColumnSet$get_columns() {
		return this._columns$p$0;
	},

	get_isEmpty: function Microsoft_Crm_Client_Core_Storage_Common_ColumnSet$get_isEmpty() {
		return !(this._columns$p$0.length > 0);
	},

	getObjectData: function Microsoft_Crm_Client_Core_Storage_Common_ColumnSet$getObjectData() {
		var data = {};
		data['columns'] = this._columns$p$0;
		return data;
	},

	getDifference: function Microsoft_Crm_Client_Core_Storage_Common_ColumnSet$getDifference(otherColumnSet) {
		if (Microsoft.Crm.Client.Core.Storage.Common.AllColumns.isInstanceOfType(otherColumnSet)) {
			return otherColumnSet;
		}
		var otherColumns = otherColumnSet;
		var missing = new (Microsoft.Crm.Client.Core.Framework.List$1.$$(String))();
		for (var $$arr_3 = otherColumns.get_columns(), $$len_4 = $$arr_3.length, $$idx_5 = 0; $$idx_5 < $$len_4; ++$$idx_5) {
			var column = $$arr_3[$$idx_5];
			if (!Array.contains(this.get_columns(), column)) {
				missing.add(column);
			}
		}
		return new Microsoft.Crm.Client.Core.Storage.Common.ColumnSet(missing.toArray());
	},

	toString: function Microsoft_Crm_Client_Core_Storage_Common_ColumnSet$toString() {
		var columnString = '[';
		if (this._columns$p$0.length > 0) {
			columnString += this._columns$p$0[0];
			for (var i = 1; i < this._columns$p$0.length - 1; i++) {
				columnString += ',' + this._columns$p$0[i];
			}
		}
		columnString += ']';
		return columnString;
	}
}


Microsoft.Crm.Client.Core.Storage.Common.CrmFormatter = function Microsoft_Crm_Client_Core_Storage_Common_CrmFormatter(timeZoneOffsetMinutes, adjusters) {
	this._adjusters$p$0 = [];
	this._userDateTimeUtils$p$0 = null;
	this._timeZoneOffsetMinutes$p$0 = timeZoneOffsetMinutes;
	if (!_Script.isNullOrUndefined(adjusters)) {
		this._adjusters$p$0 = adjusters;
	}
}
Microsoft.Crm.Client.Core.Storage.Common.CrmFormatter.get_instance = function Microsoft_Crm_Client_Core_Storage_Common_CrmFormatter$get_instance() {
	return Microsoft.Crm.Client.Core.Storage.Common.CrmFormatter._instance$p;
}
Microsoft.Crm.Client.Core.Storage.Common.CrmFormatter.set_instance = function Microsoft_Crm_Client_Core_Storage_Common_CrmFormatter$set_instance(value) {
	Microsoft.Crm.Client.Core.Storage.Common.CrmFormatter._instance$p = value;
	return value;
}
Microsoft.Crm.Client.Core.Storage.Common.CrmFormatter.isDateTime = function Microsoft_Crm_Client_Core_Storage_Common_CrmFormatter$isDateTime(value) {
	return Date.isInstanceOfType(value) || (Object.prototype.toString.call)(value) === '[object Date]';
}
Microsoft.Crm.Client.Core.Storage.Common.CrmFormatter.prototype = {
	_timeZoneOffsetMinutes$p$0: 0,
	_userDateTimeUtils$p$0: null,

	parseCurrencyValue: function Microsoft_Crm_Client_Core_Storage_Common_CrmFormatter$parseCurrencyValue(value, currencySymbol, precisionValue) {
		var currencySymbolRegex = new RegExp('\\' + currencySymbol);
		value = value.replace(currencySymbolRegex, Microsoft.Crm.Client.Core.Framework._String.empty);
		return this.parseDecimalValue(value, precisionValue);
	},

	parseIntegerValue: function Microsoft_Crm_Client_Core_Storage_Common_CrmFormatter$parseIntegerValue(value) {
		var parsedDecimalValue = this.parseDecimalValue(value);
		return (!_Script.isNullOrUndefined(parsedDecimalValue)) ? Math.floor(parsedDecimalValue) : Microsoft.Crm.Client.Core.Framework.Undefined.int32Value;
	},

	parseDecimalValue: function Microsoft_Crm_Client_Core_Storage_Common_CrmFormatter$parseDecimalValue(value, precisionValue) {
		var parsedValue = Microsoft.Crm.Client.Core.Framework.Undefined.doubleValue;
		if (!_Script.isNullOrUndefined(value)) {
			var whiteSpaceAndLeftToRightMarkRegEx = new RegExp('(\\s|' + Microsoft.Crm.Client.Core.Storage.Common.CrmFormatter.leftToRightMark + '|' + Microsoft.Crm.Client.Core.Storage.Common.CrmFormatter.rightToLeftMark + ')', 'g');
			value = value.replace(whiteSpaceAndLeftToRightMarkRegEx, Microsoft.Crm.Client.Core.Framework._String.empty);
			var isNegative = !!((/^\(.*\)$/).test(value) ^ (!!((/^\-/).test(value) ^ (/\-$/).test(value))));
			if (isNegative) {
				value = value.replace(/[\(\)\-]/g, Microsoft.Crm.Client.Core.Framework._String.empty);
			}
			parsedValue = Number.parseLocale(value);
			if (!isNaN(parsedValue)) {
				if (isNegative) {
					parsedValue = -parsedValue;
				}
				if (!_Script.isNullOrUndefined(precisionValue)) {
					parsedValue = parseFloat(parsedValue.toFixed(precisionValue));
				}
			}
			else {
				parsedValue = Microsoft.Crm.Client.Core.Framework.Undefined.doubleValue;
			}
		}
		return parsedValue;
	},

	formatIntegerValue: function Microsoft_Crm_Client_Core_Storage_Common_CrmFormatter$formatIntegerValue(value) {
		var formattedValue = Microsoft.Crm.Client.Core.Framework.Undefined.stringValue;
		if (!_Script.isNullOrUndefined(value)) {
			var intValue = parseInt(value.toString());
			if (!isNaN(intValue)) {
				formattedValue = (intValue < 0) ? this._formatNegativeDecimal$p$0(intValue, 0) : this._formatPositiveDecimal$p$0(intValue, 0);
			}
		}
		return formattedValue;
	},

	formatDecimalValue: function Microsoft_Crm_Client_Core_Storage_Common_CrmFormatter$formatDecimalValue(value, precisionValue) {
		var formattedValue = Microsoft.Crm.Client.Core.Framework.Undefined.stringValue;
		if (!_Script.isNullOrUndefined(value)) {
			var numericValue = parseFloat(value.toString());
			if (!isNaN(numericValue)) {
				formattedValue = (numericValue < 0) ? this._formatNegativeDecimal$p$0(numericValue, precisionValue) : this._formatPositiveDecimal$p$0(numericValue, precisionValue);
			}
		}
		return formattedValue;
	},

	formatCurrencyValue: function Microsoft_Crm_Client_Core_Storage_Common_CrmFormatter$formatCurrencyValue(value, currencySymbol, precisionValue) {
		var formattedValue = Microsoft.Crm.Client.Core.Framework.Undefined.stringValue;
		if (!_Script.isNullOrUndefined(value)) {
			var numericValue = parseFloat(value.toString());
			if (!isNaN(numericValue)) {
				formattedValue = this._formatCurrency$p$0(numericValue, currencySymbol, precisionValue);
			}
		}
		return formattedValue;
	},

	formatShortDateValue: function Microsoft_Crm_Client_Core_Storage_Common_CrmFormatter$formatShortDateValue(value, behavior) {
		var formattedValue = Microsoft.Crm.Client.Core.Framework.Undefined.stringValue;
		if (Microsoft.Crm.Client.Core.Storage.Common.CrmFormatter.isDateTime(value)) {
			var dateTime = this._userDateTimeUtils$p$0.formatTimeToBehaviorDisplay(value, this._timeZoneOffsetMinutes$p$0, behavior, this._adjusters$p$0);
			formattedValue = dateTime.localeFormat('d');
		}
		return formattedValue;
	},

	formatLongDateValue: function Microsoft_Crm_Client_Core_Storage_Common_CrmFormatter$formatLongDateValue(value, behavior) {
		var formattedValue = Microsoft.Crm.Client.Core.Framework.Undefined.stringValue;
		if (Microsoft.Crm.Client.Core.Storage.Common.CrmFormatter.isDateTime(value)) {
			var dateTime = this._userDateTimeUtils$p$0.formatTimeToBehaviorDisplay(value, this._timeZoneOffsetMinutes$p$0, behavior, this._adjusters$p$0);
			formattedValue = dateTime.localeFormat('D');
		}
		return formattedValue;
	},

	formatSortableDateValue: function Microsoft_Crm_Client_Core_Storage_Common_CrmFormatter$formatSortableDateValue(value, behavior) {
		var formattedValue = this.formatSortableDateTimeValue(value, behavior);
		return formattedValue.split('T')[0];
	},

	formatSortableDateTimeValue: function Microsoft_Crm_Client_Core_Storage_Common_CrmFormatter$formatSortableDateTimeValue(value, behavior) {
		var formattedValue = Microsoft.Crm.Client.Core.Framework.Undefined.stringValue;
		if (Microsoft.Crm.Client.Core.Storage.Common.CrmFormatter.isDateTime(value)) {
			var dateTime = this._userDateTimeUtils$p$0.formatTimeToBehaviorDisplay(value, this._timeZoneOffsetMinutes$p$0, behavior, this._adjusters$p$0);
			formattedValue = dateTime.localeFormat('s');
		}
		return formattedValue;
	},

	formatShortDateTimeValue: function Microsoft_Crm_Client_Core_Storage_Common_CrmFormatter$formatShortDateTimeValue(value, behavior) {
		var formattedValue = Microsoft.Crm.Client.Core.Framework.Undefined.stringValue;
		if (Microsoft.Crm.Client.Core.Storage.Common.CrmFormatter.isDateTime(value)) {
			var shortDatePattern = Sys.CultureInfo.CurrentCulture.dateTimeFormat['ShortDatePattern'];
			var shortTimePattern = Sys.CultureInfo.CurrentCulture.dateTimeFormat['ShortTimePattern'];
			var dateTime = this._userDateTimeUtils$p$0.formatTimeToBehaviorDisplay(value, this._timeZoneOffsetMinutes$p$0, behavior, this._adjusters$p$0);
			formattedValue = String.localeFormat('{0:' + shortDatePattern + ' ' + shortTimePattern + '}', dateTime);
		}
		return formattedValue;
	},

	formatDateLongAbbreviated: function Microsoft_Crm_Client_Core_Storage_Common_CrmFormatter$formatDateLongAbbreviated(value, behavior) {
		var formattedValue = Microsoft.Crm.Client.Core.Framework.Undefined.stringValue;
		if (Microsoft.Crm.Client.Core.Storage.Common.CrmFormatter.isDateTime(value)) {
			var dateTime = this._userDateTimeUtils$p$0.formatTimeToBehaviorDisplay(value, this._timeZoneOffsetMinutes$p$0, behavior, this._adjusters$p$0);
			var longDatePattern = Sys.CultureInfo.CurrentCulture.dateTimeFormat['LongDatePattern'];
			longDatePattern = longDatePattern.replace('MMMM', 'MMM');
			longDatePattern = longDatePattern.replace('dddd', 'ddd');
			formattedValue = String.localeFormat('{0:' + longDatePattern + '}', dateTime);
		}
		return formattedValue;
	},

	formatDateYearMonthValue: function Microsoft_Crm_Client_Core_Storage_Common_CrmFormatter$formatDateYearMonthValue(value, behavior) {
		var formattedValue = Microsoft.Crm.Client.Core.Framework.Undefined.stringValue;
		if (Microsoft.Crm.Client.Core.Storage.Common.CrmFormatter.isDateTime(value)) {
			var dateTime = this._userDateTimeUtils$p$0.formatTimeToBehaviorDisplay(value, this._timeZoneOffsetMinutes$p$0, behavior, this._adjusters$p$0);
			formattedValue = dateTime.localeFormat('Y');
		}
		return formattedValue;
	},

	_formatPositiveDecimal$p$0: function Microsoft_Crm_Client_Core_Storage_Common_CrmFormatter$_formatPositiveDecimal$p$0(value, decimalPrecision) {
		return Math.abs(value).localeFormat('N' + decimalPrecision.toString());
	},

	_formatNegativeDecimal$p$0: function Microsoft_Crm_Client_Core_Storage_Common_CrmFormatter$_formatNegativeDecimal$p$0(value, decimalPrecision) {
		var negativeNumberFormatCode = Sys.CultureInfo.CurrentCulture.numberFormat['NumberNegativePattern'];
		return Microsoft.Crm.Client.Core.Framework._String.format(this._getNegativeNumberFormatString$p$0(negativeNumberFormatCode), this._formatPositiveDecimal$p$0(value, decimalPrecision));
	},

	_formatCurrency$p$0: function Microsoft_Crm_Client_Core_Storage_Common_CrmFormatter$_formatCurrency$p$0(value, currencySymbol, decimalPrecision) {
		if (value < 0) {
			var negativeCurrencyFormatCode = Sys.CultureInfo.CurrentCulture.numberFormat['CurrencyNegativePattern'];
			return Microsoft.Crm.Client.Core.Framework._String.format(this._getNegativeCurrencyFormatString$p$0(negativeCurrencyFormatCode), currencySymbol, this._formatPositiveDecimal$p$0(value, decimalPrecision));
		}
		var positiveCurrencyFormatCode = Sys.CultureInfo.CurrentCulture.numberFormat['CurrencyPositivePattern'];
		return Microsoft.Crm.Client.Core.Framework._String.format(this._getPositiveCurrencyFormatString$p$0(positiveCurrencyFormatCode), currencySymbol, this._formatPositiveDecimal$p$0(value, decimalPrecision));
	},

	_getNegativeNumberFormatString$p$0: function Microsoft_Crm_Client_Core_Storage_Common_CrmFormatter$_getNegativeNumberFormatString$p$0(negativeNumberFormatCode) {
		switch (negativeNumberFormatCode) {
			case 0:
				return '(' + Microsoft.Crm.Client.Core.Storage.Common.CrmFormatter.zeroWidthNoBreakSpace + '{0}' + Microsoft.Crm.Client.Core.Storage.Common.CrmFormatter.zeroWidthNoBreakSpace + ')';
			case 1:
				return '-' + Microsoft.Crm.Client.Core.Storage.Common.CrmFormatter.zeroWidthNoBreakSpace + '{0}';
			case 2:
				return '-' + Microsoft.Crm.Client.Core.Storage.Common.CrmFormatter.noBreakSpace + '{0}';
			case 3:
				return '{0}' + Microsoft.Crm.Client.Core.Storage.Common.CrmFormatter.zeroWidthNoBreakSpace + '-';
			case 4:
				return '{0}' + Microsoft.Crm.Client.Core.Storage.Common.CrmFormatter.noBreakSpace + '-';
			default:
				return '(' + Microsoft.Crm.Client.Core.Storage.Common.CrmFormatter.zeroWidthNoBreakSpace + '{0}' + Microsoft.Crm.Client.Core.Storage.Common.CrmFormatter.zeroWidthNoBreakSpace + ')';
		}
	},

	_getPositiveCurrencyFormatString$p$0: function Microsoft_Crm_Client_Core_Storage_Common_CrmFormatter$_getPositiveCurrencyFormatString$p$0(currencyFormatCode) {
		switch (currencyFormatCode) {
			case 0:
				return '{0}' + Microsoft.Crm.Client.Core.Storage.Common.CrmFormatter.zeroWidthNoBreakSpace + '{1}';
			case 1:
				return '{1}' + Microsoft.Crm.Client.Core.Storage.Common.CrmFormatter.zeroWidthNoBreakSpace + '{0}';
			case 2:
				return '{0}' + Microsoft.Crm.Client.Core.Storage.Common.CrmFormatter.noBreakSpace + '{1}';
			case 3:
				return '{1}' + Microsoft.Crm.Client.Core.Storage.Common.CrmFormatter.noBreakSpace + '{0}';
			default:
				return '{0}' + Microsoft.Crm.Client.Core.Storage.Common.CrmFormatter.zeroWidthNoBreakSpace + '{1}';
		}
	},

	_getNegativeCurrencyFormatString$p$0: function Microsoft_Crm_Client_Core_Storage_Common_CrmFormatter$_getNegativeCurrencyFormatString$p$0(negativeCurrencyFormatCode) {
		switch (negativeCurrencyFormatCode) {
			case 0:
				return '({0}' + Microsoft.Crm.Client.Core.Storage.Common.CrmFormatter.zeroWidthNoBreakSpace + '{1})';
			case 1:
				return '-{0}' + Microsoft.Crm.Client.Core.Storage.Common.CrmFormatter.zeroWidthNoBreakSpace + '{1}';
			case 2:
				return '{0}-{1}';
			case 3:
				return '{0}' + Microsoft.Crm.Client.Core.Storage.Common.CrmFormatter.zeroWidthNoBreakSpace + '{1}-';
			case 4:
				return '({1}' + Microsoft.Crm.Client.Core.Storage.Common.CrmFormatter.zeroWidthNoBreakSpace + '{0})';
			case 5:
				return '-{1}' + Microsoft.Crm.Client.Core.Storage.Common.CrmFormatter.zeroWidthNoBreakSpace + '{0}';
			case 6:
				return '{1}-{0}';
			case 7:
				return '{1}' + Microsoft.Crm.Client.Core.Storage.Common.CrmFormatter.zeroWidthNoBreakSpace + '{0}-';
			case 8:
				return '-{1}' + Microsoft.Crm.Client.Core.Storage.Common.CrmFormatter.noBreakSpace + '{0}';
			case 9:
				return '-{0}' + Microsoft.Crm.Client.Core.Storage.Common.CrmFormatter.noBreakSpace + '{1}';
			case 10:
				return '{1}' + Microsoft.Crm.Client.Core.Storage.Common.CrmFormatter.noBreakSpace + '{0}-';
			case 11:
				return '{0}' + Microsoft.Crm.Client.Core.Storage.Common.CrmFormatter.noBreakSpace + '{1}-';
			case 12:
				return '{0}' + Microsoft.Crm.Client.Core.Storage.Common.CrmFormatter.noBreakSpace + '-{1}';
			case 13:
				return '{1}-' + Microsoft.Crm.Client.Core.Storage.Common.CrmFormatter.noBreakSpace + '{0}';
			case 14:
				return '({0}' + Microsoft.Crm.Client.Core.Storage.Common.CrmFormatter.noBreakSpace + '{1})';
			case 15:
				return '({1}' + Microsoft.Crm.Client.Core.Storage.Common.CrmFormatter.noBreakSpace + '{0})';
			default:
				return '({0}' + Microsoft.Crm.Client.Core.Storage.Common.CrmFormatter.zeroWidthNoBreakSpace + '{1})';
		}
	}
}


Microsoft.Crm.Client.Core.Storage.Common.MergeFetchXmlFilterXml = function Microsoft_Crm_Client_Core_Storage_Common_MergeFetchXmlFilterXml() {
}
Microsoft.Crm.Client.Core.Storage.Common.MergeFetchXmlFilterXml.getFetchNodeAttributeNames = function Microsoft_Crm_Client_Core_Storage_Common_MergeFetchXmlFilterXml$getFetchNodeAttributeNames() {
	return ['version', 'count', 'page', 'paging-cookie', 'utc-offset', 'aggregate', 'distinct', 'top', 'mapping', 'min-active-row-version', 'output-format', 'returntotalrecordcount'];
}
Microsoft.Crm.Client.Core.Storage.Common.MergeFetchXmlFilterXml.getEntityNodeAttributeNames = function Microsoft_Crm_Client_Core_Storage_Common_MergeFetchXmlFilterXml$getEntityNodeAttributeNames() {
	return ['name'];
}
Microsoft.Crm.Client.Core.Storage.Common.MergeFetchXmlFilterXml.getLinkedEntityAttributeNames = function Microsoft_Crm_Client_Core_Storage_Common_MergeFetchXmlFilterXml$getLinkedEntityAttributeNames() {
	return ['name', 'from', 'to', 'visible', 'link-type'];
}
Microsoft.Crm.Client.Core.Storage.Common.MergeFetchXmlFilterXml.mergeFetchXmlFilterExpressionXml = function Microsoft_Crm_Client_Core_Storage_Common_MergeFetchXmlFilterXml$mergeFetchXmlFilterExpressionXml(viewFetchXml, filterExpressionXml) {
	if (!viewFetchXml) {
		return null;
	}
	if (Microsoft.Crm.Client.Core.Framework._String.isNullOrEmpty(filterExpressionXml)) {
		return viewFetchXml;
	}
	var mergedFetchXml = Microsoft.Crm.Client.Core.Framework._String.empty;
	var wrapperFilterStart = '<filter type =\"and\">';
	var wrapperFilterEnd = '</filter>';
	var fetchNodeAttributes = Microsoft.Crm.Client.Core.Framework._String.empty;
	var entityNodeAttributes = Microsoft.Crm.Client.Core.Framework._String.empty;
	var viewFetchXMLDoc = Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlNodeFactory.parseXmlDocument(viewFetchXml);
	var viewFetchXMLFetchNode = Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlNodeFactory.create(viewFetchXMLDoc).selectSingleNode('fetch');
	var viewFetchXMLFetchEntityNode = Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlNodeFactory.create(viewFetchXMLDoc).selectSingleNode('fetch/entity');
	var isViewFetchXMLFetchEntityFilterNodePresent = !!Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlNodeFactory.create(viewFetchXMLDoc).selectSingleNode('fetch/entity/filter');
	var viewFetchXMLFetchEntityChildren = viewFetchXMLFetchEntityNode.childNodes();
	var childXMLNode = null;
	var fetchNodeAttributeNames = Microsoft.Crm.Client.Core.Storage.Common.MergeFetchXmlFilterXml.getFetchNodeAttributeNames();
	for (var $$arr_E = fetchNodeAttributeNames, $$len_F = $$arr_E.length, $$idx_G = 0; $$idx_G < $$len_F; ++$$idx_G) {
		var attribute = $$arr_E[$$idx_G];
		var attributeValue = viewFetchXMLFetchNode.getAttribute(attribute);
		if (!_Script.isNullOrUndefined(attributeValue)) {
			fetchNodeAttributes += String.format(' {0}=\"{1}\"', attribute, attributeValue);
		}
	}
	var entityNodeAttributeNames = Microsoft.Crm.Client.Core.Storage.Common.MergeFetchXmlFilterXml.getEntityNodeAttributeNames();
	for (var $$arr_K = entityNodeAttributeNames, $$len_L = $$arr_K.length, $$idx_M = 0; $$idx_M < $$len_L; ++$$idx_M) {
		var attribute = $$arr_K[$$idx_M];
		var attributeValue = viewFetchXMLFetchEntityNode.getAttribute(attribute);
		if (!_Script.isNullOrUndefined(attributeValue)) {
			entityNodeAttributes += String.format(' {0}=\"{1}\"', attribute, attributeValue);
		}
	}
	mergedFetchXml += '<fetch' + fetchNodeAttributes + '>';
	mergedFetchXml += '<entity' + entityNodeAttributes + '>';
	for (var i = 0; i < viewFetchXMLFetchEntityChildren.get_count() ; i++) {
		childXMLNode = viewFetchXMLFetchEntityChildren.get_item(i);
		if (childXMLNode.get_tagName() === 'filter') {
			mergedFetchXml += wrapperFilterStart + childXMLNode.get_outerXml() + filterExpressionXml + wrapperFilterEnd;
		}
		else {
			mergedFetchXml += childXMLNode.get_outerXml();
		}
	}
	if (!isViewFetchXMLFetchEntityFilterNodePresent) {
		mergedFetchXml += filterExpressionXml;
	}
	mergedFetchXml += '</entity></fetch>';
	return mergedFetchXml;
}
Microsoft.Crm.Client.Core.Storage.Common.MergeFetchXmlFilterXml.mergeFetchXmlWithFilterExpressionXmlByEntity = function Microsoft_Crm_Client_Core_Storage_Common_MergeFetchXmlFilterXml$mergeFetchXmlWithFilterExpressionXmlByEntity(viewFetchXml, filterExpressionXmlbyEntity, linkEntityFetchXMLByEntity) {
	if (!viewFetchXml) {
		return null;
	}
	if (_Script.isNullOrUndefined(filterExpressionXmlbyEntity)) {
		return viewFetchXml;
	}
	var mergedFetchXml = Microsoft.Crm.Client.Core.Framework._String.empty;
	var wrapperFilterStart = '<filter type =\"and\">';
	var wrapperFilterEnd = '</filter>';
	var fetchNodeAttributes = Microsoft.Crm.Client.Core.Framework._String.empty;
	var entityNodeAttributes = Microsoft.Crm.Client.Core.Framework._String.empty;
	var viewFetchXMLDoc = Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlNodeFactory.parseXmlDocument(viewFetchXml);
	var viewFetchXMLFetchNode = Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlNodeFactory.create(viewFetchXMLDoc).selectSingleNode('fetch');
	var viewFetchXMLFetchEntityNode = Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlNodeFactory.create(viewFetchXMLDoc).selectSingleNode('fetch/entity');
	var isViewFetchXMLFetchEntityFilterNodePresent = !!Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlNodeFactory.create(viewFetchXMLDoc).selectSingleNode('fetch/entity/filter');
	var viewFetchXMLFetchEntityChildren = viewFetchXMLFetchEntityNode.childNodes();
	var childXMLNode = null;
	var fetchNodeAttributeNames = Microsoft.Crm.Client.Core.Storage.Common.MergeFetchXmlFilterXml.getFetchNodeAttributeNames();
	for (var $$arr_F = fetchNodeAttributeNames, $$len_G = $$arr_F.length, $$idx_H = 0; $$idx_H < $$len_G; ++$$idx_H) {
		var attribute = $$arr_F[$$idx_H];
		var attributeValue = viewFetchXMLFetchNode.getAttribute(attribute);
		if (!_Script.isNullOrUndefined(attributeValue)) {
			fetchNodeAttributes += String.format(' {0}=\"{1}\"', attribute, attributeValue);
		}
	}
	var entityNodeAttributeNames = Microsoft.Crm.Client.Core.Storage.Common.MergeFetchXmlFilterXml.getEntityNodeAttributeNames();
	var linkedEntityAttributeNames = Microsoft.Crm.Client.Core.Storage.Common.MergeFetchXmlFilterXml.getLinkedEntityAttributeNames();
	var primaryEntityName = Microsoft.Crm.Client.Core.Framework._String.empty;
	for (var $$arr_N = entityNodeAttributeNames, $$len_O = $$arr_N.length, $$idx_P = 0; $$idx_P < $$len_O; ++$$idx_P) {
		var attribute = $$arr_N[$$idx_P];
		var attributeValue = viewFetchXMLFetchEntityNode.getAttribute(attribute);
		if (!_Script.isNullOrUndefined(attributeValue)) {
			entityNodeAttributes += String.format(' {0}=\"{1}\"', attribute, attributeValue);
			if (attribute === 'name') {
				primaryEntityName = attributeValue;
			}
		}
	}
	mergedFetchXml += '<fetch' + fetchNodeAttributes + '>';
	mergedFetchXml += '<entity' + entityNodeAttributes + '>';
	for (var i = 0; i < viewFetchXMLFetchEntityChildren.get_count() ; i++) {
		childXMLNode = viewFetchXMLFetchEntityChildren.get_item(i);
		if (childXMLNode.get_tagName() === 'filter' && !_Script.isNullOrUndefined(filterExpressionXmlbyEntity)) {
			var filterExpression = Microsoft.Crm.Client.Core.Framework._String.empty;
			var $$dict_W = filterExpressionXmlbyEntity;
			for (var $$key_X in $$dict_W) {
				var entry = { key: $$key_X, value: $$dict_W[$$key_X] };
				var expression = entry.value;
				if (!Microsoft.Crm.Client.Core.Framework._String.isNullOrEmpty(expression)) {
					filterExpression += wrapperFilterStart + expression + wrapperFilterEnd;
				}
			}
			mergedFetchXml += wrapperFilterStart + childXMLNode.get_outerXml() + filterExpression + wrapperFilterEnd;
		}
		else if (childXMLNode.get_tagName() === 'link-entity') {
			var linkedEntityAttributes = Microsoft.Crm.Client.Core.Framework._String.empty;
			var linkedEntityName = Microsoft.Crm.Client.Core.Framework._String.empty;
			for (var $$arr_a = linkedEntityAttributeNames, $$len_b = $$arr_a.length, $$idx_c = 0; $$idx_c < $$len_b; ++$$idx_c) {
				var attribute = $$arr_a[$$idx_c];
				var attributeValue = childXMLNode.getAttribute(attribute);
				if (!_Script.isNullOrUndefined(attributeValue)) {
					linkedEntityAttributes += String.format(' {0}=\"{1}\"', attribute, attributeValue);
					if (attribute === 'name') {
						linkedEntityName = attributeValue;
					}
				}
			}
			if (((linkedEntityName) in filterExpressionXmlbyEntity) && !Microsoft.Crm.Client.Core.Framework._String.isNullOrEmpty(filterExpressionXmlbyEntity[linkedEntityName])) {
				linkedEntityAttributes += String.format(' {0}=\"{1}__alias\"', 'alias', linkedEntityName);
			}
			mergedFetchXml += '<link-entity' + linkedEntityAttributes + '>';
			mergedFetchXml += childXMLNode.get_innerHtml() + '</link-entity>';
		}
		else {
			mergedFetchXml += childXMLNode.get_outerXml();
		}
	}
	if (!isViewFetchXMLFetchEntityFilterNodePresent && !_Script.isNullOrUndefined(filterExpressionXmlbyEntity)) {
		var filterExpression = Microsoft.Crm.Client.Core.Framework._String.empty;
		var $$dict_i = filterExpressionXmlbyEntity;
		for (var $$key_j in $$dict_i) {
			var entry = { key: $$key_j, value: $$dict_i[$$key_j] };
			var expression = entry.value;
			if (!Microsoft.Crm.Client.Core.Framework._String.isNullOrEmpty(expression)) {
				filterExpression += wrapperFilterStart + expression + wrapperFilterEnd;
			}
		}
		mergedFetchXml += wrapperFilterStart + filterExpression + wrapperFilterEnd;
	}
	var $$dict_l = linkEntityFetchXMLByEntity;
	for (var $$key_m in $$dict_l) {
		var linkedEntity = { key: $$key_m, value: $$dict_l[$$key_m] };
		if (((linkedEntity.key) in filterExpressionXmlbyEntity)) {
			mergedFetchXml += linkedEntity.value;
		}
	}
	mergedFetchXml += '</entity></fetch>';
	return mergedFetchXml;
}


Type.registerNamespace('Microsoft.Crm.Client.Core.Framework.Common');

Microsoft.Crm.Client.Core.Framework.Common.IFeatureEnabledContainer = function () { }
Microsoft.Crm.Client.Core.Framework.Common.IFeatureEnabledContainer.registerInterface('Microsoft.Crm.Client.Core.Framework.Common.IFeatureEnabledContainer');


Microsoft.Crm.Client.Core.Framework.Common.ResourceManager = function Microsoft_Crm_Client_Core_Framework_Common_ResourceManager() {
}
Microsoft.Crm.Client.Core.Framework.Common.ResourceManager.updateResources = function Microsoft_Crm_Client_Core_Framework_Common_ResourceManager$updateResources(updatedResources) {
	if (!_Script.isNullOrUndefined(updatedResources)) {
		Microsoft.Crm.Client.Core.Framework.Common.ResourceManager._resources$p = updatedResources;
	}
}
Microsoft.Crm.Client.Core.Framework.Common.ResourceManager.hasLocalizedString = function Microsoft_Crm_Client_Core_Framework_Common_ResourceManager$hasLocalizedString(resourceId) {
	return ((Microsoft.Crm.Client.Core.Framework.Common.ResourceManager._getDisplayModeDependentResourceId$p(resourceId)) in Microsoft.Crm.Client.Core.Framework.Common.ResourceManager._resources$p);
}
Microsoft.Crm.Client.Core.Framework.Common.ResourceManager.getLocalizedString = function Microsoft_Crm_Client_Core_Framework_Common_ResourceManager$getLocalizedString(resourceId) {
	var displayModeDependentResourceId = Microsoft.Crm.Client.Core.Framework.Common.ResourceManager._getDisplayModeDependentResourceId$p(resourceId);
	if (((displayModeDependentResourceId) in Microsoft.Crm.Client.Core.Framework.Common.ResourceManager._resources$p)) {
		return Microsoft.Crm.Client.Core.Framework.Common.ResourceManager._resources$p[displayModeDependentResourceId];
	}
	else {
		return displayModeDependentResourceId;
	}
}
Microsoft.Crm.Client.Core.Framework.Common.ResourceManager.hasErrorTitle = function Microsoft_Crm_Client_Core_Framework_Common_ResourceManager$hasErrorTitle(errorCode) {
	return ((Microsoft.Crm.Client.Core.Framework.Common.ResourceManager.errorTitleIdPrefix + Microsoft.Crm.Client.Core.Framework.Common.ResourceManager.getHexErrorCode(errorCode)) in Microsoft.Crm.Client.Core.Framework.Common.ResourceManager._resources$p);
}
Microsoft.Crm.Client.Core.Framework.Common.ResourceManager.hasErrorMessage = function Microsoft_Crm_Client_Core_Framework_Common_ResourceManager$hasErrorMessage(errorCode) {
	return ((Microsoft.Crm.Client.Core.Framework.Common.ResourceManager.errorMessageIdPrefix + Microsoft.Crm.Client.Core.Framework.Common.ResourceManager.getHexErrorCode(errorCode)) in Microsoft.Crm.Client.Core.Framework.Common.ResourceManager._resources$p);
}
Microsoft.Crm.Client.Core.Framework.Common.ResourceManager.getErrorTitle = function Microsoft_Crm_Client_Core_Framework_Common_ResourceManager$getErrorTitle(errorCode, parameters) {
	var resourceId = Microsoft.Crm.Client.Core.Framework.Common.ResourceManager._getErrorTitleResourceId$p(errorCode);
	return String.format.apply(null, [Microsoft.Crm.Client.Core.Framework.Common.ResourceManager.getLocalizedString(resourceId)].concat(parameters));
}
Microsoft.Crm.Client.Core.Framework.Common.ResourceManager.getErrorMessage = function Microsoft_Crm_Client_Core_Framework_Common_ResourceManager$getErrorMessage(errorCode, parameters) {
	var errorMessage;
	var resourceId = Microsoft.Crm.Client.Core.Framework.Common.ResourceManager._getErrorMessageResourceId$p(errorCode);
	if (!((resourceId) in Microsoft.Crm.Client.Core.Framework.Common.ResourceManager._resources$p)) {
		errorMessage = String.format.apply(null, [Microsoft.Crm.Client.Core.Framework.Common.ResourceManager.getLocalizedString(Microsoft.Crm.Client.Core.Framework.Common.ResourceManager.defaultErrorMessageId), '0x' + errorCode.toString(16)].concat(parameters));
	}
	else if (!_Script.isNullOrUndefined(parameters)) {
		errorMessage = String.format.apply(null, [Microsoft.Crm.Client.Core.Framework.Common.ResourceManager.getLocalizedString(resourceId)].concat(parameters));
	}
	else {
		errorMessage = Microsoft.Crm.Client.Core.Framework.Common.ResourceManager.getLocalizedString(resourceId);
	}
	return errorMessage;
}
Microsoft.Crm.Client.Core.Framework.Common.ResourceManager.getErrorMessageFromErrorStatus = function Microsoft_Crm_Client_Core_Framework_Common_ResourceManager$getErrorMessageFromErrorStatus(errorStatus) {
	if (errorStatus.get_errorCode() === Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.isvAborted && !_Script.isNullOrUndefined(errorStatus.get_message())) {
		return errorStatus.get_message();
	}
	else {
		return Microsoft.Crm.Client.Core.Framework.Common.ResourceManager.getErrorMessage(errorStatus.get_errorCode());
	}
}
Microsoft.Crm.Client.Core.Framework.Common.ResourceManager._getErrorTitleResourceId$p = function Microsoft_Crm_Client_Core_Framework_Common_ResourceManager$_getErrorTitleResourceId$p(errorCode) {
	if (_Script.isNullOrUndefined(errorCode)) {
		return Microsoft.Crm.Client.Core.Framework.Common.ResourceManager.defaultErrorTitleId;
	}
	var resourceId = Microsoft.Crm.Client.Core.Framework.Common.ResourceManager.errorTitleIdPrefix + Microsoft.Crm.Client.Core.Framework.Common.ResourceManager.getHexErrorCode(errorCode);
	if (!((resourceId) in Microsoft.Crm.Client.Core.Framework.Common.ResourceManager._resources$p)) {
		resourceId = Microsoft.Crm.Client.Core.Framework.Common.ResourceManager.defaultErrorTitleId;
	}
	return resourceId;
}
Microsoft.Crm.Client.Core.Framework.Common.ResourceManager._getErrorMessageResourceId$p = function Microsoft_Crm_Client_Core_Framework_Common_ResourceManager$_getErrorMessageResourceId$p(errorCode) {
	var resourceId = Microsoft.Crm.Client.Core.Framework.Common.ResourceManager.errorMessageIdPrefix + Microsoft.Crm.Client.Core.Framework.Common.ResourceManager.getHexErrorCode(errorCode);
	return resourceId;
}
Microsoft.Crm.Client.Core.Framework.Common.ResourceManager.getHexErrorCode = function Microsoft_Crm_Client_Core_Framework_Common_ResourceManager$getHexErrorCode(errorCode) {
	var code = errorCode;
	if (code < 0) {
		code = (code + 4294967295 + 1);
	}
	return code.toString(16);
}
Microsoft.Crm.Client.Core.Framework.Common.ResourceManager._getDisplayModeDependentResourceId$p = function Microsoft_Crm_Client_Core_Framework_Common_ResourceManager$_getDisplayModeDependentResourceId$p(resourceId) {
	var displayModeDependentResourceId = resourceId;
	return displayModeDependentResourceId;
}


Type.registerNamespace('Microsoft.Crm.Client.Core.Models');

Microsoft.Crm.Client.Core.Models.ChartTitle = function Microsoft_Crm_Client_Core_Models_ChartTitle() {
	this.Text = Microsoft.Crm.Client.Core.Framework._String.empty;
	this.HorizontalAlignment = 'center';
	this.VerticalAlignment = null;
}


Microsoft.Crm.Client.Core.Models.DataLabels = function Microsoft_Crm_Client_Core_Models_DataLabels() {
	this.Enabled = false;
	this.X = 0;
	this.Y = 0;
	this.Align = 'left';
	this.VerticalAlign = 'middle';
	this.LabelFormatter = null;
}


Microsoft.Crm.Client.Core.Models.DataPoint = function Microsoft_Crm_Client_Core_Models_DataPoint() {
	this.Value = 0;
	this.FormattedValue = null;
	this.Aggregators = null;
}


Microsoft.Crm.Client.Core.Models.Legend = function Microsoft_Crm_Client_Core_Models_Legend() {
	this.Enabled = false;
	this.Floating = false;
}


Microsoft.Crm.Client.Core.Models.Series = function Microsoft_Crm_Client_Core_Models_Series() {
	this.DataPoints = null;
	this.dataLabels = null;
	this.ChartType = 'line';
	this.YAxisNumber = 0;
	this.XAxisNumber = 0;
	this.Title = Microsoft.Crm.Client.Core.Framework._String.empty;
	this.Color = Microsoft.Crm.Client.Core.Framework._String.empty;
	this.BorderColor = Microsoft.Crm.Client.Core.Framework._String.empty;
	this.BorderWidth = 0;
	this.CustomProperties = null;
}


Microsoft.Crm.Client.Core.Models.XAxis = function Microsoft_Crm_Client_Core_Models_XAxis() {
	this.Values = null;
	this.Title = Microsoft.Crm.Client.Core.Framework._String.empty;
}


Microsoft.Crm.Client.Core.Models.YAxis = function Microsoft_Crm_Client_Core_Models_YAxis() {
	this.Title = Microsoft.Crm.Client.Core.Framework._String.empty;
}


Microsoft.Crm.Client.Core.Models.IModel = function () { }
Microsoft.Crm.Client.Core.Models.IModel.registerInterface('Microsoft.Crm.Client.Core.Models.IModel');


Microsoft.Crm.Client.Core.Models.IEntityRecordContainer = function () { }
Microsoft.Crm.Client.Core.Models.IEntityRecordContainer.registerInterface('Microsoft.Crm.Client.Core.Models.IEntityRecordContainer');


Microsoft.Crm.Client.Core.Models.IRecordCollectionModel = function () { }
Microsoft.Crm.Client.Core.Models.IRecordCollectionModel.registerInterface('Microsoft.Crm.Client.Core.Models.IRecordCollectionModel');


Microsoft.Crm.Client.Core.Models.DataPointAggregator = function Microsoft_Crm_Client_Core_Models_DataPointAggregator() {
	Microsoft.Crm.Client.Core.Models.DataPointAggregator.initializeBase(this);
}
Microsoft.Crm.Client.Core.Models.DataPointAggregator.prototype = {
	Value: null
}


Microsoft.Crm.Client.Core.Models.DataPointAggregatorBase = function Microsoft_Crm_Client_Core_Models_DataPointAggregatorBase() {
}
Microsoft.Crm.Client.Core.Models.DataPointAggregatorBase.prototype = {
	FieldName: null
}


Microsoft.Crm.Client.Core.Models.DataPointDateTimeRangeAggregator = function Microsoft_Crm_Client_Core_Models_DataPointDateTimeRangeAggregator() {
	Microsoft.Crm.Client.Core.Models.DataPointDateTimeRangeAggregator.initializeBase(this);
}
Microsoft.Crm.Client.Core.Models.DataPointDateTimeRangeAggregator.prototype = {
	MinDate: null,
	MaxDate: null
}


Microsoft.Crm.Client.Core.Models.DataPointFiscalPeriodAggregator = function Microsoft_Crm_Client_Core_Models_DataPointFiscalPeriodAggregator() {
	Microsoft.Crm.Client.Core.Models.DataPointFiscalPeriodAggregator.initializeBase(this);
}
Microsoft.Crm.Client.Core.Models.DataPointFiscalPeriodAggregator.prototype = {
	FiscalType: null,
	Year: 0,
	Period: 0
}


Microsoft.Crm.Client.Core.Models.DataPointFiscalYearAggregator = function Microsoft_Crm_Client_Core_Models_DataPointFiscalYearAggregator() {
	Microsoft.Crm.Client.Core.Models.DataPointFiscalYearAggregator.initializeBase(this);
}
Microsoft.Crm.Client.Core.Models.DataPointFiscalYearAggregator.prototype = {
	FiscalType: null,
	Year: 0
}


Microsoft.Crm.Client.Core.Models._chartAggregatorsContainer = function Microsoft_Crm_Client_Core_Models__chartAggregatorsContainer() {
	this._dateRangeAggregatorPrecision$p$0 = Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.DateTimeGroupingInfo.year;
	this._dataPointAggregators$p$0 = new Array(0);
}
Microsoft.Crm.Client.Core.Models._chartAggregatorsContainer._daysInMonth$p = function Microsoft_Crm_Client_Core_Models__chartAggregatorsContainer$_daysInMonth$p(year, month) {
	return new Date(year, month, 0).getDate();
}
Microsoft.Crm.Client.Core.Models._chartAggregatorsContainer.prototype = {
	_dateTimeAggregatorDay$p$0: 0,
	_dateTimeAggregatorWeek$p$0: 0,
	_dateTimeAggregatorMonth$p$0: 0,
	_dateTimeAggregatorYear$p$0: 0,
	_dateTimeAggregatorFieldName$p$0: null,
	_dataPointFiscalPeriodAggregator$p$0: null,
	_dataPointFiscalYearAggregator$p$0: null,

	addAggregator: function Microsoft_Crm_Client_Core_Models__chartAggregatorsContainer$addAggregator(fieldName, value, dateTimeGroupingType) {
		if (dateTimeGroupingType >= Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.DateTimeGroupingInfo.day && dateTimeGroupingType < Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.DateTimeGroupingInfo.fiscalPeriod) {
			this._addDateTimeRangeAggregatorPart$p$0(fieldName, value, dateTimeGroupingType);
		}
		else if (dateTimeGroupingType === Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.DateTimeGroupingInfo.fiscalPeriod) {
			this._addDataPointFiscalPeriodAggregator$p$0(fieldName, value);
		}
		else if (dateTimeGroupingType === Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.DateTimeGroupingInfo.fiscalYear) {
			this._addDataPointFiscalYearAggregator$p$0(fieldName, value);
		}
		else {
			var aggregator = new Microsoft.Crm.Client.Core.Models.DataPointAggregator();
			aggregator.FieldName = fieldName;
			if (value) {
				if (Object.getType(value) === Xrm.Objects.EntityReference) {
					aggregator.Value = (value).Id.toString();
				}
				else if (Object.getType(value).implementsInterface(Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.OptionMetadata)) {
					aggregator.Value = (value).get_value().toString();
				}
				else {
					aggregator.Value = value.toString();
				}
			}
			else {
				aggregator.Value = null;
			}
			this._dataPointAggregators$p$0[this._dataPointAggregators$p$0.length] = aggregator;
		}
	},

	_addDataPointFiscalPeriodAggregator$p$0: function Microsoft_Crm_Client_Core_Models__chartAggregatorsContainer$_addDataPointFiscalPeriodAggregator$p$0(fieldName, value) {
		if (!this._dataPointFiscalPeriodAggregator$p$0) {
			if (!value) {
				return;
			}
			this._dataPointFiscalPeriodAggregator$p$0 = new Microsoft.Crm.Client.Core.Models.DataPointFiscalPeriodAggregator();
			this._dataPointFiscalPeriodAggregator$p$0.FiscalType = 'in-fiscal-period-and-year';
			this._dataPointFiscalPeriodAggregator$p$0.FieldName = fieldName;
			Microsoft.Crm.Client.Core.Framework.Debug.assert(Object.getType(value) === String, 'value should be string');
			Microsoft.Crm.Client.Core.Framework.Debug.assert((value).indexOf('-') > 0, 'value should have 2013-2 format');
			var fiscalYearAndPeriod = (value).split('-');
			this._dataPointFiscalPeriodAggregator$p$0.Year = Number.parseInvariant(fiscalYearAndPeriod[0]);
			this._dataPointFiscalPeriodAggregator$p$0.Period = Number.parseInvariant(fiscalYearAndPeriod[1]);
		}
		else {
			throw Error.create('only one Fiscal period aggregator is supported');
		}
	},

	_addDataPointFiscalYearAggregator$p$0: function Microsoft_Crm_Client_Core_Models__chartAggregatorsContainer$_addDataPointFiscalYearAggregator$p$0(fieldName, value) {
		if (!this._dataPointFiscalYearAggregator$p$0) {
			this._dataPointFiscalYearAggregator$p$0 = new Microsoft.Crm.Client.Core.Models.DataPointFiscalYearAggregator();
			this._dataPointFiscalYearAggregator$p$0.FiscalType = 'in-fiscal-year';
			this._dataPointFiscalYearAggregator$p$0.FieldName = fieldName;
			Microsoft.Crm.Client.Core.Framework.Debug.assert(!_Script.isNullOrUndefined(value), 'value should not be null');
			Microsoft.Crm.Client.Core.Framework.Debug.assert(Object.getType(value) === Number, 'value should be int');
			this._dataPointFiscalYearAggregator$p$0.Year = value;
		}
		else {
			throw Error.create('only one Fiscal year aggregator is supported');
		}
	},

	_addDateTimeRangeAggregatorPart$p$0: function Microsoft_Crm_Client_Core_Models__chartAggregatorsContainer$_addDateTimeRangeAggregatorPart$p$0(fieldName, value, dateTimeGroupingType) {
		if (!this._dateTimeAggregatorFieldName$p$0) {
			this._dateTimeAggregatorFieldName$p$0 = fieldName;
		}
		else {
			if (this._dateTimeAggregatorFieldName$p$0 !== fieldName) {
				throw Error.create('only one DateTime aggregator is supported');
			}
		}
		switch (dateTimeGroupingType) {
			case Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.DateTimeGroupingInfo.day:
				this._dateTimeAggregatorDay$p$0 = value;
				break;
			case Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.DateTimeGroupingInfo.week:
				this._dateTimeAggregatorWeek$p$0 = value;
				break;
			case Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.DateTimeGroupingInfo.month:
				this._dateTimeAggregatorMonth$p$0 = value;
				break;
			case Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.DateTimeGroupingInfo.quarter:
				this._dateTimeAggregatorMonth$p$0 = 3 * value;
				break;
			case Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.DateTimeGroupingInfo.year:
				this._dateTimeAggregatorYear$p$0 = value;
				break;
			default:
				throw Error.create('not supported');
		}
		this._dateRangeAggregatorPrecision$p$0 = Math.min(dateTimeGroupingType, this._dateRangeAggregatorPrecision$p$0);
	},

	_getDateOfWeekSunday$p$0: function Microsoft_Crm_Client_Core_Models__chartAggregatorsContainer$_getDateOfWeekSunday$p$0(week, year) {
		var simpleStartOfWeek = new Date(year, 0, 1 + ((week - 1) * 7));
		var sundayWeekStart = simpleStartOfWeek;
		sundayWeekStart.setDate(simpleStartOfWeek.getDate() - simpleStartOfWeek.getDay());
		return sundayWeekStart;
	},

	_getDataPointDateTimeRangeAggregator$p$0: function Microsoft_Crm_Client_Core_Models__chartAggregatorsContainer$_getDataPointDateTimeRangeAggregator$p$0() {
		if (!this._dateTimeAggregatorFieldName$p$0 || !this._dateTimeAggregatorYear$p$0) {
			return null;
		}
		var dataPointDateTimeRangeAggregator = new Microsoft.Crm.Client.Core.Models.DataPointDateTimeRangeAggregator();
		dataPointDateTimeRangeAggregator.FieldName = this._dateTimeAggregatorFieldName$p$0;
		this._dateTimeAggregatorDay$p$0 = (!this._dateTimeAggregatorDay$p$0) ? Microsoft.Crm.Client.Core.Models._chartAggregatorsContainer._daysInMonth$p(this._dateTimeAggregatorYear$p$0, this._dateTimeAggregatorMonth$p$0) : this._dateTimeAggregatorDay$p$0;
		this._dateTimeAggregatorMonth$p$0 = (!this._dateTimeAggregatorMonth$p$0) ? 12 : this._dateTimeAggregatorMonth$p$0;
		dataPointDateTimeRangeAggregator.MaxDate = new Date(this._dateTimeAggregatorYear$p$0, this._dateTimeAggregatorMonth$p$0 - 1, this._dateTimeAggregatorDay$p$0);
		switch (this._dateRangeAggregatorPrecision$p$0) {
			case Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.DateTimeGroupingInfo.day:
				dataPointDateTimeRangeAggregator.MaxDate.setDate(dataPointDateTimeRangeAggregator.MaxDate.getDate() + ((dataPointDateTimeRangeAggregator.MaxDate.getTimezoneOffset() < 0) ? 1 : 0));
				dataPointDateTimeRangeAggregator.MinDate = new Date(dataPointDateTimeRangeAggregator.MaxDate.getFullYear(), dataPointDateTimeRangeAggregator.MaxDate.getMonth(), dataPointDateTimeRangeAggregator.MaxDate.getDate());
				break;
			case Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.DateTimeGroupingInfo.week:
				var endOfWeek = this._getDateOfWeekSunday$p$0(this._dateTimeAggregatorWeek$p$0 + 1, this._dateTimeAggregatorYear$p$0);
				endOfWeek.setDate(endOfWeek.getDate() - 1);
				dataPointDateTimeRangeAggregator.MaxDate = endOfWeek;
				dataPointDateTimeRangeAggregator.MinDate = new Date(dataPointDateTimeRangeAggregator.MaxDate.getFullYear(), dataPointDateTimeRangeAggregator.MaxDate.getMonth(), dataPointDateTimeRangeAggregator.MaxDate.getDate() - 6);
				if (dataPointDateTimeRangeAggregator.MaxDate.getTimezoneOffset() < 0) {
					dataPointDateTimeRangeAggregator.MaxDate.setDate(dataPointDateTimeRangeAggregator.MaxDate.getDate() + 1);
					dataPointDateTimeRangeAggregator.MinDate.setDate(dataPointDateTimeRangeAggregator.MinDate.getDate() + 1);
				}
				break;
			case Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.DateTimeGroupingInfo.month:
				dataPointDateTimeRangeAggregator.MinDate = new Date(dataPointDateTimeRangeAggregator.MaxDate.getFullYear(), dataPointDateTimeRangeAggregator.MaxDate.getMonth() - 1, Microsoft.Crm.Client.Core.Models._chartAggregatorsContainer._daysInMonth$p(this._dateTimeAggregatorYear$p$0, dataPointDateTimeRangeAggregator.MaxDate.getMonth()) + 1);
				if (dataPointDateTimeRangeAggregator.MaxDate.getTimezoneOffset() < 0) {
					dataPointDateTimeRangeAggregator.MaxDate.setDate(dataPointDateTimeRangeAggregator.MaxDate.getDate() + 1);
					dataPointDateTimeRangeAggregator.MinDate.setDate(dataPointDateTimeRangeAggregator.MinDate.getDate() + 1);
				}
				break;
			case Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.DateTimeGroupingInfo.quarter:
				dataPointDateTimeRangeAggregator.MinDate = new Date(dataPointDateTimeRangeAggregator.MaxDate.getFullYear(), dataPointDateTimeRangeAggregator.MaxDate.getMonth() - 3, Microsoft.Crm.Client.Core.Models._chartAggregatorsContainer._daysInMonth$p(this._dateTimeAggregatorYear$p$0, dataPointDateTimeRangeAggregator.MaxDate.getMonth() - 2) + 1);
				if (dataPointDateTimeRangeAggregator.MaxDate.getTimezoneOffset() < 0) {
					dataPointDateTimeRangeAggregator.MaxDate.setDate(dataPointDateTimeRangeAggregator.MaxDate.getDate() + 1);
					dataPointDateTimeRangeAggregator.MinDate.setDate(dataPointDateTimeRangeAggregator.MinDate.getDate() + 1);
				}
				break;
			case Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.DateTimeGroupingInfo.year:
				dataPointDateTimeRangeAggregator.MaxDate.setDate(dataPointDateTimeRangeAggregator.MaxDate.getDate() + ((dataPointDateTimeRangeAggregator.MaxDate.getTimezoneOffset() < 0) ? 1 : 0));
				dataPointDateTimeRangeAggregator.MinDate = new Date(dataPointDateTimeRangeAggregator.MaxDate.getFullYear() - 1, dataPointDateTimeRangeAggregator.MaxDate.getMonth(), dataPointDateTimeRangeAggregator.MaxDate.getDate() + 1);
				break;
			default:
				throw Error.create('not supported');
		}
		return dataPointDateTimeRangeAggregator;
	},

	getAggregators: function Microsoft_Crm_Client_Core_Models__chartAggregatorsContainer$getAggregators() {
		var combinedAggregators = this._dataPointAggregators$p$0;
		var dateTimeRangeAggregator = this._getDataPointDateTimeRangeAggregator$p$0();
		if (dateTimeRangeAggregator) {
			combinedAggregators = combinedAggregators.concat(dateTimeRangeAggregator);
		}
		if (this._dataPointFiscalPeriodAggregator$p$0) {
			combinedAggregators = combinedAggregators.concat(this._dataPointFiscalPeriodAggregator$p$0);
		}
		if (this._dataPointFiscalYearAggregator$p$0) {
			combinedAggregators = combinedAggregators.concat(this._dataPointFiscalYearAggregator$p$0);
		}
		return combinedAggregators;
	}
}


Type.registerNamespace('Microsoft.Crm.Client.Core.Models.Chart');

Microsoft.Crm.Client.Core.Models.Chart.ChartCategory = function Microsoft_Crm_Client_Core_Models_Chart_ChartCategory(categoryNode, dataDefinition) {
	this._chartDataDefinition$p$0 = dataDefinition;
	this._measureCollections$p$0 = new (Microsoft.Crm.Client.Core.Framework.List$1.$$(Microsoft.Crm.Client.Core.Models.Chart.ChartMeasureCollectionInfo))();
	this._primaryGroupByAlias$p$0 = categoryNode.getAttribute('alias');
	this._primaryGroupBy$p$0 = null;
	var nodeList = categoryNode.selectNodes('measurecollection');
	for (var i = 0; i < nodeList.get_count() ; i++) {
		this._measureCollections$p$0.add(new Microsoft.Crm.Client.Core.Models.Chart.ChartMeasureCollectionInfo(nodeList.get_item(i), this._chartDataDefinition$p$0, this));
	}
}
Microsoft.Crm.Client.Core.Models.Chart.ChartCategory.prototype = {
	_primaryGroupByAlias$p$0: null,
	_primaryGroupBy$p$0: null,
	_measureCollections$p$0: null,
	_chartDataDefinition$p$0: null,

	get_primaryGroupByAlias: function Microsoft_Crm_Client_Core_Models_Chart_ChartCategory$get_primaryGroupByAlias() {
		return this._primaryGroupByAlias$p$0;
	},

	get_measureCollections: function Microsoft_Crm_Client_Core_Models_Chart_ChartCategory$get_measureCollections() {
		return this._measureCollections$p$0;
	},

	get_primaryGroupBy: function Microsoft_Crm_Client_Core_Models_Chart_ChartCategory$get_primaryGroupBy() {
		return this._primaryGroupBy$p$0;
	},

	set_primaryGroupBy: function Microsoft_Crm_Client_Core_Models_Chart_ChartCategory$set_primaryGroupBy(value) {
		this._primaryGroupBy$p$0 = value;
		return value;
	}
}


Microsoft.Crm.Client.Core.Models.Chart.ChartDataDefinitionDescription = function Microsoft_Crm_Client_Core_Models_Chart_ChartDataDefinitionDescription() {
	this._metadataPairCollection$p$0 = {};
	this._eventHandlers$p$0 = new Sys.EventHandlerList();
	this._allAttributeNamesByEntity$p$0 = {};
	this._groupByList$p$0 = new (Microsoft.Crm.Client.Core.Framework.List$1.$$(Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.AttributeQueryExpression))();
	this._allMeasureInfo$p$0 = {};
	this._allAttributes$p$0 = {};
	this._chartingDataSource$p$0 = Microsoft.Crm.Client.DataSourceFactory.get_instance().getChartingDataSource();
}
Microsoft.Crm.Client.Core.Models.Chart.ChartDataDefinitionDescription.prototype = {
	_dataXmlDoc$p$0: null,
	_category$p$0: null,
	_isComparisonChart$p$0: false,
	_entityExpression$p$0: null,
	_groupByList$p$0: null,
	_allMeasureInfo$p$0: null,
	_hasOrderBy$p$0: false,
	_parserData$p$0: null,
	_eventHandlers$p$0: null,
	_allAttributeNamesByEntity$p$0: null,
	_allAttributes$p$0: null,
	_chartingDataSource$p$0: null,

	get_entityExpression: function Microsoft_Crm_Client_Core_Models_Chart_ChartDataDefinitionDescription$get_entityExpression() {
		return this._entityExpression$p$0;
	},

	get_category: function Microsoft_Crm_Client_Core_Models_Chart_ChartDataDefinitionDescription$get_category() {
		return this._category$p$0;
	},

	get_allMeasureInfo: function Microsoft_Crm_Client_Core_Models_Chart_ChartDataDefinitionDescription$get_allMeasureInfo() {
		return this._allMeasureInfo$p$0;
	},

	get_groupByList: function Microsoft_Crm_Client_Core_Models_Chart_ChartDataDefinitionDescription$get_groupByList() {
		return this._groupByList$p$0;
	},

	get_allAttributeNamesByEntity: function Microsoft_Crm_Client_Core_Models_Chart_ChartDataDefinitionDescription$get_allAttributeNamesByEntity() {
		return this._allAttributeNamesByEntity$p$0;
	},

	add_onDataDefinitionReady: function Microsoft_Crm_Client_Core_Models_Chart_ChartDataDefinitionDescription$add_onDataDefinitionReady(value) {
		this._eventHandlers$p$0.addHandler(Microsoft.Crm.Client.Core.Models.Chart.ChartDataDefinitionDescription.onDataDefinitionReadyEventName, value);
	},

	remove_onDataDefinitionReady: function Microsoft_Crm_Client_Core_Models_Chart_ChartDataDefinitionDescription$remove_onDataDefinitionReady(value) {
		this._eventHandlers$p$0.removeHandler(Microsoft.Crm.Client.Core.Models.Chart.ChartDataDefinitionDescription.onDataDefinitionReadyEventName, value);
	},

	get_isOrderPresent: function Microsoft_Crm_Client_Core_Models_Chart_ChartDataDefinitionDescription$get_isOrderPresent() {
		if (this._entityExpression$p$0.get_orderByAttributes().length > 0) {
			return true;
		}
		var linkedEntities = this._entityExpression$p$0.get_linkedEntities();
		for (var i = 0; i < linkedEntities.length; i++) {
			if (linkedEntities[i].get_orderByAttributes().length > 0) {
				return true;
			}
		}
		return false;
	},

	get_categoryColumns: function Microsoft_Crm_Client_Core_Models_Chart_ChartDataDefinitionDescription$get_categoryColumns() {
		var categoryColumns = new (Microsoft.Crm.Client.Core.Framework.List$1.$$(Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.AttributeQueryExpression))();
		if (!this._category$p$0.get_primaryGroupBy()) {
			for (var i = 0; i < this._entityExpression$p$0.get_attributes().length; i++) {
				var attribute = this._entityExpression$p$0.get_attributes()[i];
				var aliasName = (!attribute.get_aliasName()) ? attribute.get_name() : attribute.get_aliasName();
				if (!((aliasName) in this.get_allMeasureInfo())) {
					categoryColumns.add(attribute);
				}
			}
		}
		else {
			if (this._category$p$0.get_primaryGroupBy().get_groupAttribute().get_name() !== this._category$p$0.get_primaryGroupBy().get_groupAttribute().get_entity().get_metadataPair().get_entityMetadata().get_primaryIdAttribute()) {
				categoryColumns.add(this._category$p$0.get_primaryGroupBy().get_groupAttribute());
			}
			for (var i = 0; i < this._category$p$0.get_primaryGroupBy().get_extendedGroupBys().get_Count() ; i++) {
				categoryColumns.add(this._category$p$0.get_primaryGroupBy().get_extendedGroupBys().get_item(i));
			}
		}
		return categoryColumns;
	},

	get_allAttributes: function Microsoft_Crm_Client_Core_Models_Chart_ChartDataDefinitionDescription$get_allAttributes() {
		return this._allAttributes$p$0;
	},

	get_isComparisonChart: function Microsoft_Crm_Client_Core_Models_Chart_ChartDataDefinitionDescription$get_isComparisonChart() {
		return this._isComparisonChart$p$0;
	},

	get_metadataPairCollection: function Microsoft_Crm_Client_Core_Models_Chart_ChartDataDefinitionDescription$get_metadataPairCollection() {
		return this._metadataPairCollection$p$0;
	},

	get__callingContext$p$0: function Microsoft_Crm_Client_Core_Models_Chart_ChartDataDefinitionDescription$get__callingContext$p$0() {
		return Microsoft.Crm.Client.Core.Framework.DefaultContext.tryCreate(Microsoft.Crm.Client.Core.Framework._String.empty, 'ChartDataDefinitionDescription');
	},

	get_chartingDataSource: function Microsoft_Crm_Client_Core_Models_Chart_ChartDataDefinitionDescription$get_chartingDataSource() {
		return this._chartingDataSource$p$0;
	},

	parserDataDefinition: function Microsoft_Crm_Client_Core_Models_Chart_ChartDataDefinitionDescription$parserDataDefinition(dataXml) {
		this._parserData$p$0 = new Microsoft.Crm.Client.Core.Framework.PerformanceStopwatch('ChartDataDefinitionDescription:ParserDataDescription');
		this._parserData$p$0.start();
		this._dataXmlDoc$p$0 = Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlNodeFactory.parseXmlDocument(dataXml);
		this._validateDataDescription$p$0(this._dataXmlDoc$p$0);
		this._entityExpression$p$0 = new Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.EntityQueryExpression(null);
		var fetchNode = Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlNodeFactory.create(this._dataXmlDoc$p$0).selectSingleNode('datadefinition/fetchcollection/fetch');
		this._entityExpression$p$0.initialize(fetchNode);
		this._buildMeasureInformationFromFetch$p$0(this._entityExpression$p$0);
	},

	_validateDataDescription$p$0: function Microsoft_Crm_Client_Core_Models_Chart_ChartDataDefinitionDescription$_validateDataDescription$p$0(xmlDocument) {
		var errorNode = Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlNodeFactory.create(xmlDocument).selectSingleNode('Error');
		if (!errorNode) {
			return;
		}
		throw new Microsoft.Crm.Client.Core.Framework.ChartError(Microsoft.Crm.Client.Core.Framework.Common.ResourceManager.getLocalizedString('Chart_UnreadableDefinition_Title'), Microsoft.Crm.Client.Core.Framework.Common.ResourceManager.getLocalizedString('Chart_UnreadableDefinition_Message')).toException();
	},

	buildCategory: function Microsoft_Crm_Client_Core_Models_Chart_ChartDataDefinitionDescription$buildCategory() {
		this._buildChartMeasures$p$0();
		this._isComparisonChart$p$0 = this._category$p$0.get_measureCollections().get_item(0).get_secondaryGroupBys().get_Count() > 0;
	},

	retrieveEntityMetadataDeferred: function Microsoft_Crm_Client_Core_Models_Chart_ChartDataDefinitionDescription$retrieveEntityMetadataDeferred() {
		var arrayList = [];
		var $$dict_2 = this._allAttributeNamesByEntity$p$0;
		for (var $$key_3 in $$dict_2) {
			var entry = { key: $$key_3, value: $$dict_2[$$key_3] };
			if (!((entry.key) in this._metadataPairCollection$p$0)) {
				Array.add(arrayList, this._retrieveEntityMetadata$p$0(entry.key));
			}
		}
		return arrayList;
	},

	setMetadata: function Microsoft_Crm_Client_Core_Models_Chart_ChartDataDefinitionDescription$setMetadata(metadata) {
		if (metadata) {
			for (var i = 0; i < metadata.length; i++) {
				var entityMetadata = metadata[i];
				this._metadataPairCollection$p$0[entityMetadata.get_logicalName()] = new Microsoft.Crm.Client.Core.Storage.Common.EntityAttributeMetadataPair(entityMetadata, new Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadataCollection(entityMetadata.get_logicalName()));
			}
		}
		this._setMetadataForEntityExpression$p$0(this._entityExpression$p$0, this._metadataPairCollection$p$0);
	},

	_setMetadataForEntityExpression$p$0: function Microsoft_Crm_Client_Core_Models_Chart_ChartDataDefinitionDescription$_setMetadataForEntityExpression$p$0(expression, metadata) {
		if (((expression.get_entityName()) in metadata)) {
			expression.set_metadataPair(metadata[expression.get_entityName()]);
		}
		else {
			throw Error.format('Entity metadata missed!');
		}
		var linkedEntities = expression.get_linkedEntities();
		for (var i = 0; i < linkedEntities.length; i++) {
			this._setMetadataForEntityExpression$p$0(linkedEntities[i], metadata);
		}
	},

	retrieveAttributeMetadataDeferred: function Microsoft_Crm_Client_Core_Models_Chart_ChartDataDefinitionDescription$retrieveAttributeMetadataDeferred() {
		var arrayList = [];
		var $$dict_7 = this._allAttributeNamesByEntity$p$0;
		for (var $$key_8 in $$dict_7) {
			var entry = { key: $$key_8, value: $$dict_7[$$key_8] };
			var attributeNames = new Array(0);
			var attributes = entry.value;
			var $$dict_5 = attributes;
			for (var $$key_6 in $$dict_5) {
				var attribute = { key: $$key_6, value: $$dict_5[$$key_6] };
				attributeNames[attributeNames.length] = (attribute.value).get_name();
			}
			if (attributeNames.length > 0) {
				Array.add(arrayList, this._retrieveAttributeMetadata$p$0(entry.key, attributeNames));
			}
		}
		return arrayList;
	},

	_retrieveEntityMetadata$p$0: function Microsoft_Crm_Client_Core_Models_Chart_ChartDataDefinitionDescription$_retrieveEntityMetadata$p$0(entityLogicalName) {
		var entityDeferred = Microsoft.Crm.Client.Core.Imported.MscrmComponents.deferredPromiseFactory(Microsoft.Crm.Client.Core.Storage.Common.IEntityMetadata, Microsoft.Crm.Client.Core.Framework.ErrorStatus);
		var $$t_4 = this, $$t_5 = this;
		this._chartingDataSource$p$0.retrieveEntityMetadata(entityLogicalName, this.get__callingContext$p$0()).then(function (metadata) {
			entityDeferred.resolve(metadata);
		}, function (error) {
			entityDeferred.reject(error);
		});
		return entityDeferred.promise();
	},

	_retrieveAttributeMetadata$p$0: function Microsoft_Crm_Client_Core_Models_Chart_ChartDataDefinitionDescription$_retrieveAttributeMetadata$p$0(entityLogicalName, attributeNames) {
		var attributeDeferred = Microsoft.Crm.Client.Core.Imported.MscrmComponents.deferredPromiseFactory(Microsoft.Crm.Client.Core.Framework.CallbackSafeArray$1.$$(Microsoft.Crm.Client.Core.Storage.Common.IAttributeMetadata), Microsoft.Crm.Client.Core.Framework.ErrorStatus);
		var $$t_5 = this, $$t_6 = this;
		this._chartingDataSource$p$0.retrieveMultipleAttributeMetadata(new Microsoft.Crm.Client.Core.Storage.DataApi.AttributeMetadataQuery(entityLogicalName, attributeNames), this.get__callingContext$p$0()).then(function (attributeMetadata) {
			attributeDeferred.resolve(attributeMetadata);
		}, function (error) {
			attributeDeferred.reject(error);
		});
		return attributeDeferred.promise();
	},

	_buildMeasureInformationFromFetch$p$0: function Microsoft_Crm_Client_Core_Models_Chart_ChartDataDefinitionDescription$_buildMeasureInformationFromFetch$p$0(entity) {
		var entityName = entity.get_entityName();
		var attributeByName;
		if (!((entityName) in this._allAttributeNamesByEntity$p$0)) {
			attributeByName = {};
			this._allAttributeNamesByEntity$p$0[entityName] = attributeByName;
		}
		else {
			attributeByName = this._allAttributeNamesByEntity$p$0[entityName];
		}
		var attributes = entity.get_attributes();
		for (var i = 0; i < attributes.length; i++) {
			var attribute = attributes[i];
			attributeByName[attribute.get_name()] = attribute;
			var attributeAliasName = (attribute.get_aliasName()) ? attribute.get_aliasName() : attribute.get_name();
			this._allAttributes$p$0[attributeAliasName] = attribute;
			if (attribute.get_hasGroupBy()) {
				this._groupByList$p$0.add(attribute);
			}
			else {
				this._allMeasureInfo$p$0[this._getAliasName$p$0(attribute)] = new Microsoft.Crm.Client.Core.Models.Chart.ChartMeasureInfo(attribute, this._groupByList$p$0);
			}
		}
		for (var i = 0; i < entity.get_linkedEntities().length; i++) {
			this._buildMeasureInformationFromFetch$p$0(entity.get_linkedEntities()[i]);
		}
		this._hasOrderBy$p$0 = this._hasOrderBy$p$0 || entity.get_orderByAttributes().length > 0;
	},

	_getAliasName$p$0: function Microsoft_Crm_Client_Core_Models_Chart_ChartDataDefinitionDescription$_getAliasName$p$0(attribute) {
		if (attribute.get_aliasName()) {
			return attribute.get_aliasName();
		}
		return attribute.get_name();
	},

	_buildChartMeasures$p$0: function Microsoft_Crm_Client_Core_Models_Chart_ChartDataDefinitionDescription$_buildChartMeasures$p$0() {
		var categoryXPath = 'datadefinition/categorycollection/category';
		var categoryNode = Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlNodeFactory.create(this._dataXmlDoc$p$0).selectSingleNode(categoryXPath);
		Microsoft.Crm.Client.Core.Framework.Debug.assert(!_Script.isNull(categoryNode), 'data xml missing category node!');
		this._category$p$0 = new Microsoft.Crm.Client.Core.Models.Chart.ChartCategory(categoryNode, this);
	}
}


Microsoft.Crm.Client.Core.Models.Chart.ChartDataQueryDecorator = function Microsoft_Crm_Client_Core_Models_Chart_ChartDataQueryDecorator(dataDefinitionXml, viewDefinitionXml) {
	this._dataDefinition$p$0 = new Microsoft.Crm.Client.Core.Models.Chart.ChartDataDefinitionDescription();
	this._dataDefinition$p$0.parserDataDefinition(dataDefinitionXml);
	if (!_Script.isNullOrUndefined(viewDefinitionXml)) {
		this._viewDefinition$p$0 = new Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.EntityQueryExpression(null);
		var fetchXMLDoc = Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlNodeFactory.parseXmlDocument(viewDefinitionXml);
		var fetchNode = Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlNodeFactory.create(fetchXMLDoc).selectSingleNode('fetch');
		this._viewDefinition$p$0.initialize(fetchNode);
	}
}
Microsoft.Crm.Client.Core.Models.Chart.ChartDataQueryDecorator.prototype = {
	_dataDefinition$p$0: null,
	_viewDefinition$p$0: null,

	get_dataDefinition: function Microsoft_Crm_Client_Core_Models_Chart_ChartDataQueryDecorator$get_dataDefinition() {
		return this._dataDefinition$p$0;
	},

	get_fetchXml: function Microsoft_Crm_Client_Core_Models_Chart_ChartDataQueryDecorator$get_fetchXml() {
		return this._dataDefinition$p$0.get_entityExpression().get_fetchXml();
	},

	get_viewXml: function Microsoft_Crm_Client_Core_Models_Chart_ChartDataQueryDecorator$get_viewXml() {
		if (!_Script.isNullOrUndefined(this._viewDefinition$p$0)) {
			return this._viewDefinition$p$0.get_fetchXml();
		}
		return null;
	},

	setupAggregationQueries: function Microsoft_Crm_Client_Core_Models_Chart_ChartDataQueryDecorator$setupAggregationQueries() {
		this.get_dataDefinition().buildCategory();
		var queryFromView = this._viewDefinition$p$0;
		if (!this._dataDefinition$p$0.get_isOrderPresent() && queryFromView.get_orderByAttributes().length > 0) {
			var order = queryFromView.get_orderByAttributes()[0];
			var chartEntityExpression = this._dataDefinition$p$0.get_entityExpression();
			if (chartEntityExpression.get_hasAggregate()) {
				for (var i = 0; i < chartEntityExpression.get_groupByAttributes().length; i++) {
					var groupBy = chartEntityExpression.get_groupByAttributes()[i];
					if (groupBy.get_name() === order.get_name()) {
						chartEntityExpression.insertOrderBy(new Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.AttributeQueryExpression(groupBy.get_name(), groupBy.get_aliasName(), chartEntityExpression), true, order.get_descending(), -1);
						break;
					}
				}
			}
			else {
				chartEntityExpression.insertOrderBy(new Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.AttributeQueryExpression(order.get_name(), order.get_aliasName(), chartEntityExpression), false, order.get_descending(), -1);
			}
		}
		queryFromView.removeAllAttributesAndOrders();
	},

	getEntityFetchXmlWithAliasSet: function Microsoft_Crm_Client_Core_Models_Chart_ChartDataQueryDecorator$getEntityFetchXmlWithAliasSet(linkEntityDictionary) {
		var fetchXML = this._dataDefinition$p$0.get_entityExpression().get_fetchXml();
		var resultXML = Microsoft.Crm.Client.Core.Framework._String.empty;
		var fetchNodeAttributes = Microsoft.Crm.Client.Core.Framework._String.empty;
		var entityNodeAttributes = Microsoft.Crm.Client.Core.Framework._String.empty;
		var viewFetchXMLDoc = Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlNodeFactory.parseXmlDocument(fetchXML);
		var viewFetchXMLFetchNode = Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlNodeFactory.create(viewFetchXMLDoc).selectSingleNode('fetch');
		var viewFetchXMLFetchEntityNode = Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlNodeFactory.create(viewFetchXMLDoc).selectSingleNode('fetch/entity');
		var viewFetchXMLFetchEntityChildren = viewFetchXMLFetchEntityNode.childNodes();
		var childXMLNode = null;
		var fetchNodeAttributeNames = Microsoft.Crm.Client.Core.Storage.Common.MergeFetchXmlFilterXml.getFetchNodeAttributeNames();
		for (var $$arr_B = fetchNodeAttributeNames, $$len_C = $$arr_B.length, $$idx_D = 0; $$idx_D < $$len_C; ++$$idx_D) {
			var attribute = $$arr_B[$$idx_D];
			var attributeValue = viewFetchXMLFetchNode.getAttribute(attribute);
			if (!_Script.isNullOrUndefined(attributeValue)) {
				fetchNodeAttributes += String.format(' {0}=\"{1}\"', attribute, attributeValue);
			}
		}
		var entityNodeAttributeNames = Microsoft.Crm.Client.Core.Storage.Common.MergeFetchXmlFilterXml.getEntityNodeAttributeNames();
		var linkedEntityAttributeNames = Microsoft.Crm.Client.Core.Storage.Common.MergeFetchXmlFilterXml.getLinkedEntityAttributeNames();
		for (var $$arr_I = entityNodeAttributeNames, $$len_J = $$arr_I.length, $$idx_K = 0; $$idx_K < $$len_J; ++$$idx_K) {
			var attribute = $$arr_I[$$idx_K];
			var attributeValue = viewFetchXMLFetchEntityNode.getAttribute(attribute);
			if (!_Script.isNullOrUndefined(attributeValue)) {
				entityNodeAttributes += String.format(' {0}=\"{1}\"', attribute, attributeValue);
			}
		}
		resultXML += '<fetch' + fetchNodeAttributes + '>';
		resultXML += '<entity' + entityNodeAttributes + '>';
		var linkedEntityList = new (Microsoft.Crm.Client.Core.Framework.List$1.$$(String))();
		for (var i = 0; i < viewFetchXMLFetchEntityChildren.get_count() ; i++) {
			childXMLNode = viewFetchXMLFetchEntityChildren.get_item(i);
			if (childXMLNode.get_tagName() === 'link-entity') {
				var linkedEntityAttributes = Microsoft.Crm.Client.Core.Framework._String.empty;
				var linkedEntityName = Microsoft.Crm.Client.Core.Framework._String.empty;
				for (var $$arr_R = linkedEntityAttributeNames, $$len_S = $$arr_R.length, $$idx_T = 0; $$idx_T < $$len_S; ++$$idx_T) {
					var attribute = $$arr_R[$$idx_T];
					var attributeValue = childXMLNode.getAttribute(attribute);
					if (!_Script.isNullOrUndefined(attributeValue)) {
						linkedEntityAttributes += String.format(' {0}=\"{1}\"', attribute, attributeValue);
						if (attribute === 'name') {
							linkedEntityName = attributeValue;
							linkedEntityList.add(linkedEntityName);
						}
					}
				}
				linkedEntityAttributes += String.format(' {0}=\"{1}__alias\"', 'alias', linkedEntityName);
				resultXML += '<link-entity' + linkedEntityAttributes + '>';
				resultXML += Microsoft.Crm.Client.Core.Framework.Utils.XmlParser.getInnerXml(childXMLNode) + '</link-entity>';
			}
			else {
				resultXML += childXMLNode.get_outerXml();
			}
		}
		var $$dict_X = linkEntityDictionary;
		for (var $$key_Y in $$dict_X) {
			var entry = { key: $$key_Y, value: $$dict_X[$$key_Y] };
			if (!linkedEntityList.contains(entry.key)) {
				resultXML += entry.value;
			}
		}
		resultXML += '</entity></fetch>';
		return resultXML;
	}
}


Microsoft.Crm.Client.Core.Models.Chart.ChartGroupBy = function Microsoft_Crm_Client_Core_Models_Chart_ChartGroupBy(groupByAttribute, dataDescription) {
	this._attribute$p$0 = groupByAttribute;
	this._extendedGroupBys$p$0 = new (Microsoft.Crm.Client.Core.Framework.List$1.$$(Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.AttributeQueryExpression))();
	this._chartDefinition$p$0 = dataDescription;
	this._addExtendedGroupBys$p$0();
	this._initializeOrderType$p$0();
}
Microsoft.Crm.Client.Core.Models.Chart.ChartGroupBy.createGroupByFromEntity = function Microsoft_Crm_Client_Core_Models_Chart_ChartGroupBy$createGroupByFromEntity(entity, dataDescription) {
	if (entity.get_hasAggregate()) {
		return null;
	}
	var primaryKeyField = new Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.AttributeQueryExpression(entity.get_metadataPair().get_entityMetadata().get_primaryIdAttribute(), null, entity);
	entity.insertAttributeNode(primaryKeyField.get_name(), null);
	entity.get_attributes()[entity.get_attributes().length] = primaryKeyField;
	entity.get_attributesByName()[primaryKeyField.get_name()] = primaryKeyField;
	if (((entity.get_entityName()) in dataDescription.get_allAttributeNamesByEntity())) {
		(dataDescription.get_allAttributeNamesByEntity()[entity.get_entityName()])[primaryKeyField.get_name()] = primaryKeyField;
	}
	var groupBy = new Microsoft.Crm.Client.Core.Models.Chart.ChartGroupBy(primaryKeyField, dataDescription);
	return groupBy;
}
Microsoft.Crm.Client.Core.Models.Chart.ChartGroupBy.prototype = {
	_attribute$p$0: null,
	_chartDefinition$p$0: null,
	_hasOrder$p$0: false,
	_descending$p$0: false,
	_extendedGroupBys$p$0: null,

	get_groupAttribute: function Microsoft_Crm_Client_Core_Models_Chart_ChartGroupBy$get_groupAttribute() {
		return this._attribute$p$0;
	},

	get_extendedGroupBys: function Microsoft_Crm_Client_Core_Models_Chart_ChartGroupBy$get_extendedGroupBys() {
		return this._extendedGroupBys$p$0;
	},

	_initializeOrderType$p$0: function Microsoft_Crm_Client_Core_Models_Chart_ChartGroupBy$_initializeOrderType$p$0() {
		var orders = this._attribute$p$0.get_entity().get_orderByAttributes();
		for (var i = 0; i < orders.length; ++i) {
			var order = orders[i];
			if (order.get_aliasName() === this._attribute$p$0.get_aliasName()) {
				this._hasOrder$p$0 = true;
				this._descending$p$0 = order.get_descending();
				return;
			}
		}
	},

	_addExtendedGroupBys$p$0: function Microsoft_Crm_Client_Core_Models_Chart_ChartGroupBy$_addExtendedGroupBys$p$0() {
		this._addPrimaryFieldGroupByForPrimaryKey$p$0();
		this._addDateGroupBys$p$0();
	},

	_addDateGroupBys$p$0: function Microsoft_Crm_Client_Core_Models_Chart_ChartGroupBy$_addDateGroupBys$p$0() {
		if (null === this._attribute$p$0.get_dateTimeGrouping()) {
			return;
		}
		var existingOrderIndex = -1;
		var orders = this._attribute$p$0.get_entity().get_orderByAttributes();
		for (var i = 0; i < orders.length; ++i) {
			var order = orders[i];
			if (order.get_aliasName() === this._attribute$p$0.get_aliasName()) {
				existingOrderIndex = i;
				this._hasOrder$p$0 = true;
				this._descending$p$0 = order.get_descending();
				break;
			}
		}
		if (existingOrderIndex === -1) {
			this._attribute$p$0.get_entity().insertOrderBy(this._attribute$p$0, true, false, orders.length);
			existingOrderIndex = orders.length - 1;
			this._hasOrder$p$0 = true;
			this._descending$p$0 = false;
		}
		switch (this._attribute$p$0.get_dateTimeGrouping().get_groupingType()) {
			case Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.DateTimeGroupingInfo.fiscalYear:
			case Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.DateTimeGroupingInfo.fiscalPeriod:
			case Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.DateTimeGroupingInfo.year:
				return;
			case Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.DateTimeGroupingInfo.week:
			case Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.DateTimeGroupingInfo.month:
			case Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.DateTimeGroupingInfo.quarter:
				this._addGroupingAndOrder$p$0(Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.DateTimeGroupingInfo.year, this._descending$p$0, existingOrderIndex);
				break;
			case Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.DateTimeGroupingInfo.day:
				this._addGroupingAndOrder$p$0(Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.DateTimeGroupingInfo.month, this._descending$p$0, existingOrderIndex);
				this._addGroupingAndOrder$p$0(Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.DateTimeGroupingInfo.year, this._descending$p$0, existingOrderIndex);
				break;
		}
	},

	_addPrimaryFieldGroupByForPrimaryKey$p$0: function Microsoft_Crm_Client_Core_Models_Chart_ChartGroupBy$_addPrimaryFieldGroupByForPrimaryKey$p$0() {
		var parentEntity = this._attribute$p$0.get_entity();
		if (this._attribute$p$0.get_name() !== parentEntity.get_metadataPair().get_entityMetadata().get_primaryIdAttribute()) {
			return;
		}
		if (!parentEntity.get_metadataPair().get_entityMetadata().get_primaryNameAttribute()) {
			return;
		}
		var primaryField = new Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.AttributeQueryExpression(parentEntity.get_metadataPair().get_entityMetadata().get_primaryNameAttribute(), Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.AttributeQueryExpression.generateUniqueAliasName(parentEntity.get_entityName(), parentEntity.get_metadataPair().get_entityMetadata().get_primaryNameAttribute()), parentEntity);
		if (this._attribute$p$0.get_hasGroupBy()) {
			for (var i = 0; i < parentEntity.get_groupByAttributes().length; i++) {
				var groupBy = parentEntity.get_groupByAttributes()[i];
				if (groupBy.get_name() === parentEntity.get_metadataPair().get_entityMetadata().get_primaryNameAttribute()) {
					this._extendedGroupBys$p$0.add(groupBy);
					return;
				}
			}
			primaryField.set_hasGroupBy(true);
			parentEntity.insertAttributeNode(primaryField.get_name(), primaryField.get_aliasName(), 'true', null, null, null);
			parentEntity.get_groupByAttributes()[parentEntity.get_groupByAttributes().length] = primaryField;
		}
		else {
			parentEntity.insertAttributeNode(primaryField.get_name(), primaryField.get_aliasName(), null, null, null, null);
		}
		parentEntity.get_attributes()[parentEntity.get_attributes().length] = primaryField;
		parentEntity.get_attributesByName()[primaryField.get_aliasName()] = primaryField;
		if (((parentEntity.get_entityName()) in this._chartDefinition$p$0.get_allAttributeNamesByEntity())) {
			(this._chartDefinition$p$0.get_allAttributeNamesByEntity()[parentEntity.get_entityName()])[primaryField.get_name()] = primaryField;
		}
		this._extendedGroupBys$p$0.add(primaryField);
	},

	_addGroupingAndOrder$p$0: function Microsoft_Crm_Client_Core_Models_Chart_ChartGroupBy$_addGroupingAndOrder$p$0(groupingType, isDescending, orderIndex) {
		var parentEntity = this._attribute$p$0.get_entity();
		var grouping = new Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.AttributeQueryExpression(this._attribute$p$0.get_name(), Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.AttributeQueryExpression.generateUniqueAliasName(parentEntity.get_entityName(), this._attribute$p$0.get_name()), parentEntity, new Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.DateTimeGroupingInfo(groupingType));
		grouping.set_hasGroupBy(true);
		parentEntity.get_groupByAttributes()[parentEntity.get_groupByAttributes().length] = grouping;
		parentEntity.get_attributes()[parentEntity.get_attributes().length] = grouping;
		parentEntity.get_attributesByName()[grouping.get_aliasName()] = grouping;
		parentEntity.insertOrderBy(grouping, true, isDescending, orderIndex);
		parentEntity.insertAttributeNode(grouping.get_name(), grouping.get_aliasName(), 'true', null, Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.DateTimeGroupingInfo.fromGroupingTypeToName(groupingType), 'true');
		this._extendedGroupBys$p$0.add(grouping);
	},

	get_hasOrder: function Microsoft_Crm_Client_Core_Models_Chart_ChartGroupBy$get_hasOrder() {
		return this._hasOrder$p$0;
	},

	get_descending: function Microsoft_Crm_Client_Core_Models_Chart_ChartGroupBy$get_descending() {
		return this._descending$p$0;
	}
}


Microsoft.Crm.Client.Core.Models.Chart.ChartMeasure = function Microsoft_Crm_Client_Core_Models_Chart_ChartMeasure(measureNode, dataDefinition, parentMeasureCollection, parentCategory) {
	var alias = measureNode.getAttribute('alias');
	var meaureInfo = dataDefinition.get_allMeasureInfo()[alias];
	this._measureAttribute$p$0 = meaureInfo.get_attributeExpression();
	this._parentMeasureCollection$p$0 = parentMeasureCollection;
	this._parentCategory$p$0 = parentCategory;
	this._initializeGroupBys$p$0(meaureInfo, dataDefinition);
}
Microsoft.Crm.Client.Core.Models.Chart.ChartMeasure.prototype = {
	_measureAttribute$p$0: null,
	_resultAliasName$p$0: null,
	_parentMeasureCollection$p$0: null,
	_parentCategory$p$0: null,

	get_measureAttribute: function Microsoft_Crm_Client_Core_Models_Chart_ChartMeasure$get_measureAttribute() {
		return this._measureAttribute$p$0;
	},

	get_resultAliasName: function Microsoft_Crm_Client_Core_Models_Chart_ChartMeasure$get_resultAliasName() {
		return this._resultAliasName$p$0;
	},

	get_parentCategory: function Microsoft_Crm_Client_Core_Models_Chart_ChartMeasure$get_parentCategory() {
		return this._parentCategory$p$0;
	},

	get_parentMeasureCollection: function Microsoft_Crm_Client_Core_Models_Chart_ChartMeasure$get_parentMeasureCollection() {
		return this._parentMeasureCollection$p$0;
	},

	_initializeGroupBys$p$0: function Microsoft_Crm_Client_Core_Models_Chart_ChartMeasure$_initializeGroupBys$p$0(measureInfo, dataDefinition) {
		if (null === this._parentCategory$p$0.get_primaryGroupBy()) {
			if (null === measureInfo.get_groupByList() || measureInfo.get_groupByList().get_Count() <= 0) {
				this._parentCategory$p$0.set_primaryGroupBy(Microsoft.Crm.Client.Core.Models.Chart.ChartGroupBy.createGroupByFromEntity(measureInfo.get_attributeExpression().get_entity(), dataDefinition));
			}
			else {
				if (this._parentCategory$p$0.get_primaryGroupByAlias()) {
					for (var i = 0; i < measureInfo.get_groupByList().get_Count() ; i++) {
						var groupBy = measureInfo.get_groupByList().get_item(i);
						if (groupBy.get_aliasName() === this._parentCategory$p$0.get_primaryGroupByAlias()) {
							this._parentCategory$p$0.set_primaryGroupBy(new Microsoft.Crm.Client.Core.Models.Chart.ChartGroupBy(groupBy, dataDefinition));
							break;
						}
					}
				}
				else {
					this._parentCategory$p$0.set_primaryGroupBy(new Microsoft.Crm.Client.Core.Models.Chart.ChartGroupBy(measureInfo.get_groupByList().get_item(0), dataDefinition));
				}
			}
		}
		if (null === this._parentMeasureCollection$p$0.get_secondaryGroupBys()) {
			this._parentMeasureCollection$p$0.set_secondaryGroupBys(new (Microsoft.Crm.Client.Core.Framework.List$1.$$(Microsoft.Crm.Client.Core.Models.Chart.ChartGroupBy))());
			if (null !== measureInfo.get_groupByList() && measureInfo.get_groupByList().get_Count() > 1) {
				for (var i = 0; i < measureInfo.get_groupByList().get_Count() ; ++i) {
					var groupByAttribute = measureInfo.get_groupByList().get_item(i);
					if (groupByAttribute !== this._parentCategory$p$0.get_primaryGroupBy().get_groupAttribute()) {
						this._parentMeasureCollection$p$0.get_secondaryGroupBys().add(new Microsoft.Crm.Client.Core.Models.Chart.ChartGroupBy(groupByAttribute, dataDefinition));
					}
				}
			}
		}
	}
}


Microsoft.Crm.Client.Core.Models.Chart.ChartMeasureCollectionInfo = function Microsoft_Crm_Client_Core_Models_Chart_ChartMeasureCollectionInfo(measureCollectionNode, dataDefinition, parentCategory) {
	this._measures$p$0 = new (Microsoft.Crm.Client.Core.Framework.List$1.$$(Microsoft.Crm.Client.Core.Models.Chart.ChartMeasure))();
	this._chartDataDefinition$p$0 = dataDefinition;
	this._measures$p$0.add(new Microsoft.Crm.Client.Core.Models.Chart.ChartMeasure(measureCollectionNode.selectSingleNode('measure'), this._chartDataDefinition$p$0, this, parentCategory));
}
Microsoft.Crm.Client.Core.Models.Chart.ChartMeasureCollectionInfo.prototype = {
	_measures$p$0: null,
	_chartDataDefinition$p$0: null,
	_secondaryGroupBys$p$0: null,

	get_measures: function Microsoft_Crm_Client_Core_Models_Chart_ChartMeasureCollectionInfo$get_measures() {
		return this._measures$p$0;
	},

	get_secondaryGroupBys: function Microsoft_Crm_Client_Core_Models_Chart_ChartMeasureCollectionInfo$get_secondaryGroupBys() {
		return this._secondaryGroupBys$p$0;
	},

	set_secondaryGroupBys: function Microsoft_Crm_Client_Core_Models_Chart_ChartMeasureCollectionInfo$set_secondaryGroupBys(value) {
		this._secondaryGroupBys$p$0 = value;
		return value;
	}
}


Microsoft.Crm.Client.Core.Models.Chart.ChartMeasureInfo = function Microsoft_Crm_Client_Core_Models_Chart_ChartMeasureInfo(attributeExpression, groupByList) {
	this._attributeExpression$p$0 = attributeExpression;
	this._groupByList$p$0 = groupByList;
}
Microsoft.Crm.Client.Core.Models.Chart.ChartMeasureInfo.prototype = {
	_groupByList$p$0: null,
	_attributeExpression$p$0: null,

	get_attributeExpression: function Microsoft_Crm_Client_Core_Models_Chart_ChartMeasureInfo$get_attributeExpression() {
		return this._attributeExpression$p$0;
	},

	get_groupByList: function Microsoft_Crm_Client_Core_Models_Chart_ChartMeasureInfo$get_groupByList() {
		return this._groupByList$p$0;
	}
}


Microsoft.Crm.Client.Core.Models.Chart.ChartQueryModel = function Microsoft_Crm_Client_Core_Models_Chart_ChartQueryModel() {
	this._displayMode$p$0 = 0;
}
Microsoft.Crm.Client.Core.Models.Chart.ChartQueryModel.prototype = {
	_title$p$0: null,
	_legend$p$0: null,
	_xAxes$p$0: null,
	_yAxes$p$0: null,
	_seriesList$p$0: null,
	_subTitle$p$0: null,
	_colors$p$0: null,
	_errorInformation$p$0: null,
	_enableDrilldown$p$0: true,

	get_SubTitle: function Microsoft_Crm_Client_Core_Models_Chart_ChartQueryModel$get_SubTitle() {
		return this._subTitle$p$0;
	},

	set_SubTitle: function Microsoft_Crm_Client_Core_Models_Chart_ChartQueryModel$set_SubTitle(value) {
		this._subTitle$p$0 = value;
		return value;
	},

	get_Title: function Microsoft_Crm_Client_Core_Models_Chart_ChartQueryModel$get_Title() {
		return this._title$p$0;
	},

	set_Title: function Microsoft_Crm_Client_Core_Models_Chart_ChartQueryModel$set_Title(value) {
		this._title$p$0 = value;
		return value;
	},

	get_Legend: function Microsoft_Crm_Client_Core_Models_Chart_ChartQueryModel$get_Legend() {
		return this._legend$p$0;
	},

	set_Legend: function Microsoft_Crm_Client_Core_Models_Chart_ChartQueryModel$set_Legend(value) {
		this._legend$p$0 = value;
		return value;
	},

	get_XAxes: function Microsoft_Crm_Client_Core_Models_Chart_ChartQueryModel$get_XAxes() {
		return this._xAxes$p$0;
	},

	set_XAxes: function Microsoft_Crm_Client_Core_Models_Chart_ChartQueryModel$set_XAxes(value) {
		this._xAxes$p$0 = value;
		return value;
	},

	get_YAxes: function Microsoft_Crm_Client_Core_Models_Chart_ChartQueryModel$get_YAxes() {
		return this._yAxes$p$0;
	},

	set_YAxes: function Microsoft_Crm_Client_Core_Models_Chart_ChartQueryModel$set_YAxes(value) {
		this._yAxes$p$0 = value;
		return value;
	},

	get_SeriesList: function Microsoft_Crm_Client_Core_Models_Chart_ChartQueryModel$get_SeriesList() {
		return this._seriesList$p$0;
	},

	set_SeriesList: function Microsoft_Crm_Client_Core_Models_Chart_ChartQueryModel$set_SeriesList(value) {
		this._seriesList$p$0 = value;
		return value;
	},

	get_Colors: function Microsoft_Crm_Client_Core_Models_Chart_ChartQueryModel$get_Colors() {
		return this._colors$p$0;
	},

	set_Colors: function Microsoft_Crm_Client_Core_Models_Chart_ChartQueryModel$set_Colors(value) {
		this._colors$p$0 = value;
		return value;
	},

	get_displayMode: function Microsoft_Crm_Client_Core_Models_Chart_ChartQueryModel$get_displayMode() {
		return this._displayMode$p$0;
	},

	set_displayMode: function Microsoft_Crm_Client_Core_Models_Chart_ChartQueryModel$set_displayMode(value) {
		this._displayMode$p$0 = value;
		return value;
	},

	get_errorInformation: function Microsoft_Crm_Client_Core_Models_Chart_ChartQueryModel$get_errorInformation() {
		return this._errorInformation$p$0;
	},

	set_errorInformation: function Microsoft_Crm_Client_Core_Models_Chart_ChartQueryModel$set_errorInformation(value) {
		this._errorInformation$p$0 = value;
		return value;
	},

	get_enableDrilldown: function Microsoft_Crm_Client_Core_Models_Chart_ChartQueryModel$get_enableDrilldown() {
		return this._enableDrilldown$p$0;
	},

	set_enableDrilldown: function Microsoft_Crm_Client_Core_Models_Chart_ChartQueryModel$set_enableDrilldown(value) {
		this._enableDrilldown$p$0 = value;
		return value;
	}
}


Microsoft.Crm.Client.Core.Models.Chart.ChartQueryModel._chartQueryType = function () { }
Microsoft.Crm.Client.Core.Models.Chart.ChartQueryModel._chartQueryType.prototype = {
	queryFromDirectFetch: 0,
	queryFromIds: 1
}
Microsoft.Crm.Client.Core.Models.Chart.ChartQueryModel._chartQueryType.registerEnum('Microsoft.Crm.Client.Core.Models.Chart.ChartQueryModel._chartQueryType', false);


Type.registerNamespace('Microsoft.Crm.Client.Core.Storage.Common.ObjectModel');

Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeAccessRights = function () { }
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeAccessRights.prototype = {
	none: 0,
	canCreate: 1,
	canRead: 2,
	canUpdate: 4
}
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeAccessRights.registerEnum('Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeAccessRights', true);


Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeSourceType = function () { }
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeSourceType.prototype = {
	unknown: -1,
	persistent: 0,
	calculated: 1,
	rollup: 2
}
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeSourceType.registerEnum('Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeSourceType', false);


Type.registerNamespace('Xrm.Objects');

Xrm.Objects.AttributeType = function () { }
Xrm.Objects.AttributeType.prototype = {
	unknown: -1,
	boolean: 0,
	customer: 1,
	dateTime: 2,
	decimal: 3,
	double: 4,
	integer: 5,
	lookup: 6,
	memo: 7,
	money: 8,
	owner: 9,
	partyList: 10,
	picklist: 11,
	state: 12,
	status: 13,
	string: 14,
	uniqueIdentifier: 15,
	calendarRules: 16,
	virtual: 17,
	bigInt: 18,
	managedProperty: 19,
	entityName: 20,
	aliasedValue: 21,
	arrayOfString: 22
}
Xrm.Objects.AttributeType.registerEnum('Xrm.Objects.AttributeType', false);


Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.MoneyPrecisionSource = function () { }
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.MoneyPrecisionSource.prototype = {
	attribute: 0,
	organization: 1,
	currency: 2
}
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.MoneyPrecisionSource.registerEnum('Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.MoneyPrecisionSource', false);


Type.registerNamespace('Xrm.Gen');

Xrm.Gen.PrivilegeType = function () { }
Xrm.Gen.PrivilegeType.prototype = {
	none: 0,
	create: 1,
	read: 2,
	write: 3,
	Delete: 4,
	assign: 5,
	share: 6,
	append: 7,
	appendTo: 8
}
Xrm.Gen.PrivilegeType.registerEnum('Xrm.Gen.PrivilegeType', false);


Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.RequiredLevel = function () { }
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.RequiredLevel.prototype = {
	unknown: -1,
	none: 0,
	systemRequired: 1,
	applicationRequired: 2,
	recommended: 3
}
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.RequiredLevel.registerEnum('Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.RequiredLevel', false);


Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.IApplicationMetadata = function () { }
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.IApplicationMetadata.registerInterface('Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.IApplicationMetadata');


Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.IChartingAttributeMetaData = function () { }
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.IChartingAttributeMetaData.registerInterface('Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.IChartingAttributeMetaData');


Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.IChartingMetaData = function () { }
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.IChartingMetaData.registerInterface('Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.IChartingMetaData');


Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.IChartingMetaDataAggregator = function () { }
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.IChartingMetaDataAggregator.registerInterface('Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.IChartingMetaDataAggregator');


Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.IEntityRecord = function () { }
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.IEntityRecord.registerInterface('Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.IEntityRecord');


Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeFormat = function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_AttributeFormat() {
}


Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata = function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_AttributeMetadata(logicalName, id, entityLogicalName, type, displayName, isSecured, isValidForCreate, isValidForRead, isValidForUpdate, requiredLevel, maxLength, minValue, maxValue, precision, precisionSource, format, behavior, defaultFormValue, defaultValue, optionSet, isBaseCurrency, targets, attributeOf, hasChanged, imeMode, isSortableEnabled, inheritsFrom, sourceType, isLocalizable) {
	Microsoft.Crm.Client.Core.Framework.Utils.ExceptionHelpers.throwOnNullOrUndefinedArgument(logicalName, 'logicalName');
	Microsoft.Crm.Client.Core.Framework.Utils.ExceptionHelpers.throwOnNullOrUndefinedArgument(id, 'id');
	Microsoft.Crm.Client.Core.Framework.Utils.ExceptionHelpers.throwOnNullOrUndefinedArgument(type, 'type');
	Microsoft.Crm.Client.Core.Framework.Utils.ExceptionHelpers.throwOnNullOrUndefinedArgument(requiredLevel, 'requiredLevel');
	Microsoft.Crm.Client.Core.Framework.Utils.ExceptionHelpers.throwOnNullOrUndefinedArgument(isSecured, 'isSecured');
	Microsoft.Crm.Client.Core.Framework.Utils.ExceptionHelpers.throwOnNullOrUndefinedArgument(isValidForCreate, 'isValidForCreate');
	Microsoft.Crm.Client.Core.Framework.Utils.ExceptionHelpers.throwOnNullOrUndefinedArgument(isValidForRead, 'isValidForRead');
	Microsoft.Crm.Client.Core.Framework.Utils.ExceptionHelpers.throwOnNullOrUndefinedArgument(isValidForUpdate, 'isValidForUpdate');
	this._logicalName$p$0 = logicalName;
	this._id$p$0 = id;
	this._entityLogicalName$p$0 = entityLogicalName;
	this._type$p$0 = type;
	this._sourceType$p$0 = sourceType;
	this._displayName$p$0 = displayName;
	this._isSecured$p$0 = isSecured;
	this._isValidForCreate$p$0 = isValidForCreate;
	this._isValidForRead$p$0 = isValidForRead;
	this._isValidForUpdate$p$0 = isValidForUpdate;
	this._requiredLevel$p$0 = requiredLevel;
	this._maxLength$p$0 = maxLength;
	this._minValue$p$0 = minValue;
	this._maxValue$p$0 = maxValue;
	this._precision$p$0 = precision;
	this._precisionSource$p$0 = precisionSource;
	this._format$p$0 = format;
	this._behavior$p$0 = behavior;
	this._defaultFormValue$p$0 = defaultFormValue;
	this._defaultValue$p$0 = defaultValue;
	this._optionSet$p$0 = optionSet;
	this._targets$p$0 = targets;
	this._isBaseCurrency$p$0 = isBaseCurrency;
	this._attributeOf$p$0 = attributeOf;
	this._hasChanged$p$0 = hasChanged;
	this._isSortableEnabled$p$0 = isSortableEnabled;
	this._imeMode$p$0 = imeMode;
	this._inheritsFrom$p$0 = inheritsFrom;
	this._isLocalizable$p$0 = isLocalizable;
}
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.extractKey = function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_AttributeMetadata$extractKey(data) {
	return Microsoft.Crm.Client.Core.Framework.Guid.createFromObjectData((data)[Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.idPath]).toString();
}
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.createFromObjectData = function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_AttributeMetadata$createFromObjectData(data) {
	var logicalName = data[Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.logicalNamePath];
	var id = Microsoft.Crm.Client.Core.Framework.Guid.createFromObjectData(data[Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.idPath]);
	var entityLogicalName = data[Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.entityLogicalNamePath];
	var type = data[Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.typePath];
	var sourceType = data[Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.sourceTypePath];
	var displayName = data[Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.displayNamePath];
	var requiredLevel = data[Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.requiredLevelPath];
	var isSecured = data[Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.isSecuredPath];
	var isValidForCreate = data[Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.isValidForCreatePath];
	var isValidForUpdate = data[Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.isValidForUpdatePath];
	var isValidForRead = data[Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.isValidForReadPath];
	var maxLength = data[Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.maxLengthPath];
	var minValue = data[Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.minValuePath];
	var maxValue = data[Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.maxValuePath];
	var precision = data[Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.precisionPath];
	var precisionSource = data[Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.precisionSourcePath];
	var format = data[Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.formatPath];
	var behavior = data[Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.behaviorPath];
	var defaultFormValue = data[Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.defaultFormValuePath];
	var defaultValue = data[Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.defaultValuePath];
	var isBaseCurrency = data[Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.isBaseCurrencyPath];
	var attributeOf = data[Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.attributeOfPath];
	var hasChanged = data[Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.hasChangedPath];
	var isSortableEnabled = data[Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.isSortableEnabledPath];
	var imeMode = data[Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.imeModePath];
	var inheritsFrom = data[Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.inheritsFromPath];
	var isLocalizable = data[Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.isLocalizablePath];
	var optionSet = null;
	var optionSetData = data[Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.optionSetPath];
	if (!_Script.isNullOrUndefined(optionSetData)) {
		optionSet = Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.OptionSetMetadata.createFromObjectData(optionSetData);
	}
	var targets = data[Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.targetsPath];
	return new Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata(logicalName, id, entityLogicalName, type, displayName, isSecured, isValidForCreate, isValidForRead, isValidForUpdate, requiredLevel, maxLength, minValue, maxValue, precision, precisionSource, format, behavior, defaultFormValue, defaultValue, optionSet, isBaseCurrency, targets, attributeOf, hasChanged, imeMode, isSortableEnabled, inheritsFrom, sourceType, isLocalizable);
}
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.prototype = {
	_id$p$0: null,
	_logicalName$p$0: null,
	_type$p$0: 0,
	_sourceType$p$0: 0,
	_entityLogicalName$p$0: null,
	_displayName$p$0: null,
	_isSecured$p$0: false,
	_isValidForCreate$p$0: false,
	_isValidForRead$p$0: false,
	_isValidForUpdate$p$0: false,
	_requiredLevel$p$0: 0,
	_maxLength$p$0: 0,
	_minValue$p$0: 0,
	_maxValue$p$0: 0,
	_precision$p$0: 0,
	_precisionSource$p$0: 0,
	_format$p$0: null,
	_behavior$p$0: 0,
	_defaultFormValue$p$0: 0,
	_defaultValue$p$0: false,
	_isBaseCurrency$p$0: false,
	_optionSet$p$0: null,
	_targets$p$0: null,
	_attributeOf$p$0: null,
	_hasChanged$p$0: false,
	_isSortableEnabled$p$0: false,
	_imeMode$p$0: 0,
	_inheritsFrom$p$0: null,
	_isLocalizable$p$0: false,

	get_id: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_AttributeMetadata$get_id() {
		return this._id$p$0;
	},

	get_logicalName: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_AttributeMetadata$get_logicalName() {
		return this._logicalName$p$0;
	},

	get_entityLogicalName: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_AttributeMetadata$get_entityLogicalName() {
		return this._entityLogicalName$p$0;
	},

	get_type: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_AttributeMetadata$get_type() {
		return this._type$p$0;
	},

	get_sourceType: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_AttributeMetadata$get_sourceType() {
		return this._sourceType$p$0;
	},

	get_displayName: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_AttributeMetadata$get_displayName() {
		return this._displayName$p$0;
	},

	get_isValidForCreate: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_AttributeMetadata$get_isValidForCreate() {
		return this._isValidForCreate$p$0;
	},

	get_isSecured: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_AttributeMetadata$get_isSecured() {
		return this._isSecured$p$0;
	},

	get_isValidForUpdate: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_AttributeMetadata$get_isValidForUpdate() {
		return this._isValidForUpdate$p$0;
	},

	get_isValidForRead: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_AttributeMetadata$get_isValidForRead() {
		return this._isValidForRead$p$0;
	},

	get_requiredLevel: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_AttributeMetadata$get_requiredLevel() {
		return this._requiredLevel$p$0;
	},

	get_maxLength: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_AttributeMetadata$get_maxLength() {
		return this._maxLength$p$0;
	},

	get_minValue: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_AttributeMetadata$get_minValue() {
		return this._minValue$p$0;
	},

	get_maxValue: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_AttributeMetadata$get_maxValue() {
		return this._maxValue$p$0;
	},

	get_precision: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_AttributeMetadata$get_precision() {
		return this._precision$p$0;
	},

	get_precisionSource: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_AttributeMetadata$get_precisionSource() {
		return this._precisionSource$p$0;
	},

	get_format: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_AttributeMetadata$get_format() {
		return this._format$p$0;
	},

	get_behavior: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_AttributeMetadata$get_behavior() {
		return this._behavior$p$0;
	},

	get_isBaseCurrency: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_AttributeMetadata$get_isBaseCurrency() {
		return this._isBaseCurrency$p$0;
	},

	get_defaultFormValue: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_AttributeMetadata$get_defaultFormValue() {
		return this._defaultFormValue$p$0;
	},

	get_defaultValue: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_AttributeMetadata$get_defaultValue() {
		return this._defaultValue$p$0;
	},

	get_optionSet: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_AttributeMetadata$get_optionSet() {
		return this._optionSet$p$0;
	},

	get_targets: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_AttributeMetadata$get_targets() {
		return this._targets$p$0;
	},

	get_attributeOf: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_AttributeMetadata$get_attributeOf() {
		return this._attributeOf$p$0;
	},

	get_hasChanged: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_AttributeMetadata$get_hasChanged() {
		return this._hasChanged$p$0;
	},

	get_isSortableEnabled: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_AttributeMetadata$get_isSortableEnabled() {
		return this._isSortableEnabled$p$0;
	},

	get_imeMode: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_AttributeMetadata$get_imeMode() {
		return this._imeMode$p$0;
	},

	get_inheritsFrom: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_AttributeMetadata$get_inheritsFrom() {
		return this._inheritsFrom$p$0;
	},

	get_isLocalizable: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_AttributeMetadata$get_isLocalizable() {
		return this._isLocalizable$p$0;
	},

	getObjectData: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_AttributeMetadata$getObjectData() {
		var data = {};
		data[Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.logicalNamePath] = this._logicalName$p$0;
		data[Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.idPath] = this._id$p$0.getObjectData();
		data[Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.entityLogicalNamePath] = this._entityLogicalName$p$0;
		data[Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.entityAttributeLogicalNamesPath] = this._entityLogicalName$p$0 + Microsoft.Crm.Client.Core.Storage.Common.StorageConstants.compositeIndexDelimiter + this._logicalName$p$0;
		data[Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.typePath] = this._type$p$0;
		data[Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.sourceTypePath] = this._sourceType$p$0;
		data[Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.displayNamePath] = this._displayName$p$0;
		data[Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.isSecuredPath] = this._isSecured$p$0;
		data[Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.isSortableEnabledPath] = this._isSortableEnabled$p$0;
		data[Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.isValidForCreatePath] = this._isValidForCreate$p$0;
		data[Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.isValidForUpdatePath] = this._isValidForUpdate$p$0;
		data[Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.isValidForReadPath] = this._isValidForRead$p$0;
		data[Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.requiredLevelPath] = this._requiredLevel$p$0;
		data[Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.maxLengthPath] = this._maxLength$p$0;
		data[Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.minValuePath] = this._minValue$p$0;
		data[Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.maxValuePath] = this._maxValue$p$0;
		data[Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.precisionPath] = this._precision$p$0;
		data[Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.precisionSourcePath] = this._precisionSource$p$0;
		data[Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.defaultFormValuePath] = this._defaultFormValue$p$0;
		data[Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.defaultValuePath] = this._defaultValue$p$0;
		data[Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.formatPath] = this._format$p$0;
		data[Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.behaviorPath] = this._behavior$p$0;
		data[Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.isBaseCurrencyPath] = this._isBaseCurrency$p$0;
		data[Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.attributeOfPath] = this._attributeOf$p$0;
		data[Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.hasChangedPath] = this._hasChanged$p$0;
		data[Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.imeModePath] = this._imeMode$p$0;
		data[Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.inheritsFromPath] = this._inheritsFrom$p$0;
		data[Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.isLocalizablePath] = this._isLocalizable$p$0;
		if (!_Script.isNullOrUndefined(this._optionSet$p$0)) {
			data[Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.optionSetPath] = this._optionSet$p$0.getObjectData();
		}
		if (!_Script.isNullOrUndefined(this._targets$p$0)) {
			data[Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.targetsPath] = this._targets$p$0;
		}
		return data;
	},

	populateFromCache: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_AttributeMetadata$populateFromCache(cachedAttributeMetadata) {
		this._entityLogicalName$p$0 = cachedAttributeMetadata.get_entityLogicalName();
		this._isSecured$p$0 = cachedAttributeMetadata.get_isSecured();
		this._isValidForCreate$p$0 = cachedAttributeMetadata.get_isValidForCreate();
		this._isValidForRead$p$0 = cachedAttributeMetadata.get_isValidForRead();
		this._isValidForUpdate$p$0 = cachedAttributeMetadata.get_isValidForUpdate();
		this._requiredLevel$p$0 = cachedAttributeMetadata.get_requiredLevel();
		this._maxLength$p$0 = cachedAttributeMetadata.get_maxLength();
		this._minValue$p$0 = cachedAttributeMetadata.get_minValue();
		this._maxValue$p$0 = cachedAttributeMetadata.get_maxValue();
		this._precision$p$0 = cachedAttributeMetadata.get_precision();
		this._precisionSource$p$0 = cachedAttributeMetadata.get_precisionSource();
		this._format$p$0 = cachedAttributeMetadata.get_format();
		this._behavior$p$0 = cachedAttributeMetadata.get_behavior();
		this._defaultFormValue$p$0 = cachedAttributeMetadata.get_defaultFormValue();
		this._defaultValue$p$0 = cachedAttributeMetadata.get_defaultValue();
		this._targets$p$0 = cachedAttributeMetadata.get_targets();
		this._isBaseCurrency$p$0 = cachedAttributeMetadata.get_isBaseCurrency();
		this._attributeOf$p$0 = cachedAttributeMetadata.get_attributeOf();
		this._hasChanged$p$0 = cachedAttributeMetadata.get_hasChanged();
		this._isSortableEnabled$p$0 = cachedAttributeMetadata.get_isSortableEnabled();
		this._imeMode$p$0 = cachedAttributeMetadata.get_imeMode();
		this._inheritsFrom$p$0 = cachedAttributeMetadata.get_inheritsFrom();
		this._sourceType$p$0 = cachedAttributeMetadata.get_sourceType();
		this._isLocalizable$p$0 = cachedAttributeMetadata.get_isLocalizable();
		if (_Script.isNullOrUndefined(this._optionSet$p$0)) {
			this._optionSet$p$0 = cachedAttributeMetadata.get_optionSet();
		}
		if (_Script.isNullOrUndefined(this._displayName$p$0)) {
			this._displayName$p$0 = cachedAttributeMetadata.get_displayName();
		}
	}
}


Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadataCollection = function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_AttributeMetadataCollection(associatedEntityLogicalName) {
	this._associatedEntityLogicalName$p$0 = associatedEntityLogicalName;
	this._attributes$p$0 = new Array(0);
	this._attributesByName$p$0 = new (Microsoft.Crm.Client.Core.Framework.TypedDictionary$1.$$(Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata))();
}
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadataCollection.prototype = {
	_associatedEntityLogicalName$p$0: null,
	_attributes$p$0: null,
	_attributesByName$p$0: null,
	_allAttributes$p$0: false,

	get_associatedEntityLogicalName: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_AttributeMetadataCollection$get_associatedEntityLogicalName() {
		return this._associatedEntityLogicalName$p$0;
	},

	get_attributes: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_AttributeMetadataCollection$get_attributes() {
		return this._attributes$p$0;
	},

	get_attributesByName: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_AttributeMetadataCollection$get_attributesByName() {
		return this._attributesByName$p$0;
	},

	get_count: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_AttributeMetadataCollection$get_count() {
		return this._attributes$p$0.length;
	},

	get_allAttributes: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_AttributeMetadataCollection$get_allAttributes() {
		return this._allAttributes$p$0;
	},

	mergeAttributeMetadata: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_AttributeMetadataCollection$mergeAttributeMetadata(attributeMetadata, isAllAttributes) {
		this._allAttributes$p$0 = this._allAttributes$p$0 || isAllAttributes;
		if (_Script.isNullOrUndefined(attributeMetadata)) {
			return;
		}
		var mergeAttributes = true;
		if (!this._attributes$p$0.length || isAllAttributes) {
			this._attributes$p$0 = attributeMetadata;
			this._attributesByName$p$0 = new (Microsoft.Crm.Client.Core.Framework.TypedDictionary$1.$$(Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata))();
			mergeAttributes = false;
		}
		for (var i = 0; i < attributeMetadata.length; i++) {
			var attribute = attributeMetadata[i];
			if (mergeAttributes) {
				if (!this._attributesByName$p$0.containsKey(attribute.get_logicalName())) {
					this._attributes$p$0[this._attributes$p$0.length] = attribute;
				}
				else {
					for (var j = 0; j < this._attributes$p$0.length; j++) {
						if (this._attributes$p$0[j].get_logicalName() === attribute.get_logicalName()) {
							this._attributes$p$0[j] = attribute;
						}
					}
				}
			}
			this._attributesByName$p$0.set_item(attribute.get_logicalName(), attribute);
		}
	},

	purgeAttributeMetadata: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_AttributeMetadataCollection$purgeAttributeMetadata() {
		this._allAttributes$p$0 = false;
		this._attributes$p$0 = new Array(0);
		if (_Script.isNullOrUndefined(this._attributesByName$p$0)) {
			this._attributesByName$p$0 = new (Microsoft.Crm.Client.Core.Framework.TypedDictionary$1.$$(Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata))();
		}
		else {
			this._attributesByName$p$0.clear();
		}
	}
}


Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributePrivilege = function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_AttributePrivilege(attributeId, canCreate, canRead, canUpdate) {
	this._attributeId$p$0 = attributeId;
	this._canCreate$p$0 = canCreate;
	this._canRead$p$0 = canRead;
	this._canUpdate$p$0 = canUpdate;
	this._accessRightsMask$p$0 = 0;
	this._accessRightsMask$p$0 = (this._canCreate$p$0) ? this.get_accessRightsMask() | Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeAccessRights.canCreate : this._accessRightsMask$p$0;
	this._accessRightsMask$p$0 = (this._canRead$p$0) ? this.get_accessRightsMask() | Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeAccessRights.canRead : this._accessRightsMask$p$0;
	this._accessRightsMask$p$0 = (this._canUpdate$p$0) ? this.get_accessRightsMask() | Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeAccessRights.canUpdate : this._accessRightsMask$p$0;
}
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributePrivilege.createFromObjectData = function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_AttributePrivilege$createFromObjectData(data) {
	var attributeid = Microsoft.Crm.Client.Core.Framework.Guid.createFromObjectData(data['attributeid']);
	var canCreate = data['cancreate'];
	var canRead = data['canread'];
	var canUpdate = data['canupdate'];
	return new Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributePrivilege(attributeid, canCreate, canRead, canUpdate);
}
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributePrivilege.prototype = {
	_attributeId$p$0: null,
	_canCreate$p$0: false,
	_canRead$p$0: false,
	_canUpdate$p$0: false,
	_accessRightsMask$p$0: 0,

	get_attributeId: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_AttributePrivilege$get_attributeId() {
		return this._attributeId$p$0;
	},

	get_canCreate: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_AttributePrivilege$get_canCreate() {
		return this._canCreate$p$0;
	},

	get_canRead: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_AttributePrivilege$get_canRead() {
		return this._canRead$p$0;
	},

	get_canUpdate: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_AttributePrivilege$get_canUpdate() {
		return this._canUpdate$p$0;
	},

	get_accessRightsMask: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_AttributePrivilege$get_accessRightsMask() {
		return this._accessRightsMask$p$0;
	},

	getObjectData: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_AttributePrivilege$getObjectData() {
		var data = {};
		data['attributeid'] = this._attributeId$p$0.getObjectData();
		data['cancreate'] = this._canCreate$p$0;
		data['canread'] = this._canRead$p$0;
		data['canupdate'] = this._canUpdate$p$0;
		return data;
	}
}


Xrm.Objects.EntityReference = function Xrm_Objects_EntityReference(logicalName, id, name, rowVersion) {
	Microsoft.Crm.Client.Core.Framework.Utils.ExceptionHelpers.throwOnNullOrUndefinedArgument(logicalName, 'logicalName');
	Microsoft.Crm.Client.Core.Framework.Utils.ExceptionHelpers.throwOnNullOrUndefinedArgument(id, 'id');
	this.LogicalName = logicalName;
	this.Id = id;
	this.Name = OptionalParameter.getValue(String, name);
	this.set_rowVersion(rowVersion);
	this.TypeName = logicalName;
	this.TypeDisplayName = logicalName;
}
Xrm.Objects.EntityReference.get_empty = function Xrm_Objects_EntityReference$get_empty() {
	return Xrm.Objects.EntityReference._empty$p || (Xrm.Objects.EntityReference._empty$p = new Xrm.Objects.EntityReference(Microsoft.Crm.Client.Core.Framework._String.empty, Microsoft.Crm.Client.Core.Framework.Guid.get_empty(), Microsoft.Crm.Client.Core.Framework._String.empty));
}
Xrm.Objects.EntityReference.createFromObjectData = function Xrm_Objects_EntityReference$createFromObjectData(data) {
	var logicalName = data['logicalname'];
	var id = Microsoft.Crm.Client.Core.Framework.Guid.createFromObjectData(data['id']);
	var name = data['name'];
	if ((('rowversion') in data) && data['rowversion']) {
		var rowVersion = data['rowversion'];
		return new Xrm.Objects.EntityReference(logicalName, id, name, rowVersion);
	}
	return new Xrm.Objects.EntityReference(logicalName, id, name);
}
Xrm.Objects.EntityReference._uppercaseFirstCharacter$p = function Xrm_Objects_EntityReference$_uppercaseFirstCharacter$p(text) {
	return text.substr(0, 1).toUpperCase() + text.substr(1);
}
Xrm.Objects.EntityReference.prototype = {
	Id: null,
	LogicalName: null,
	Name: null,
	TypeCode: 0,
	TypeDisplayName: null,
	TypeName: null,
	_rowVersion$p$0: null,

	get_key: function Xrm_Objects_EntityReference$get_key() {
		return this.Id.toString();
	},

	get_identifier: function Xrm_Objects_EntityReference$get_identifier() {
		return this.Id.toString();
	},

	get_modelType: function Xrm_Objects_EntityReference$get_modelType() {
		return this.LogicalName;
	},

	get_displayName: function Xrm_Objects_EntityReference$get_displayName() {
		return this.Name;
	},

	_$$pf_RowVersion$p$0: null,

	get_rowVersion: function Xrm_Objects_EntityReference$get_rowVersion() {
		return this._$$pf_RowVersion$p$0;
	},

	set_rowVersion: function Xrm_Objects_EntityReference$set_rowVersion(value) {
		this._$$pf_RowVersion$p$0 = value;
		return value;
	},

	getObjectData: function Xrm_Objects_EntityReference$getObjectData() {
		var data = {};
		data['logicalname'] = this.LogicalName;
		data['id'] = this.Id.getObjectData();
		data['name'] = this.Name;
		if (!_Script.isNullOrUndefined(this.get_rowVersion())) {
			data['rowversion'] = this.get_rowVersion();
		}
		return data;
	},

	equals: function Xrm_Objects_EntityReference$equals(other) {
		if (Xrm.Objects.EntityReference.isInstanceOfType(other)) {
			var otherReference = other;
			return otherReference.LogicalName === this.LogicalName && this.Id.equals(otherReference.Id) && this.Name === otherReference.Name;
		}
		return false;
	},

	getHashCode: function Xrm_Objects_EntityReference$getHashCode() {
		return (!Microsoft.Crm.Client.Core.Framework._String.isNullOrEmpty(this.LogicalName)) ? Microsoft.Crm.Client.Core.Framework._String.hashCode(this.LogicalName) ^ this.Id.getHashCode() : 0;
	},

	toString: function Xrm_Objects_EntityReference$toString() {
		return String.format('{0}:{1}', this.LogicalName, this.Id.toString());
	},

	getValue: function Xrm_Objects_EntityReference$getValue(fieldName) {
		return this[Xrm.Objects.EntityReference._uppercaseFirstCharacter$p(fieldName)];
	},

	setValue: function Xrm_Objects_EntityReference$setValue(fieldName, value) {
		this[Xrm.Objects.EntityReference._uppercaseFirstCharacter$p(fieldName)] = value;
	}
}


Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.OptionMetadata = function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_OptionMetadata(label, value, state, defaultStatus, allowedStatusTransition, color, invariantName) {
	this.get_ValueString = this.get_valueString;
	this.set_ValueString = this.set_valueString;
	this.get_Label = this.get_label;
	this.set_Label = this.set_label;
	this._label$p$0 = label;
	this._val$p$0 = value;
	this._state$p$0 = state;
	this._defaultStatus$p$0 = defaultStatus;
	this._allowedStatusTransitions$p$0 = allowedStatusTransition;
	this._color$p$0 = color;
	this._invariantName$p$0 = invariantName;
}
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.OptionMetadata.createFromPicklistItem = function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_OptionMetadata$createFromPicklistItem(item) {
	if (Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.OptionMetadata.isInstanceOfType(item)) {
		return item;
	}
	else {
		var label = item.get_Label();
		var value = Number.parseInvariant(item.get_ValueString());
		var state = -1;
		var defaultStatus = -1;
		var allowedStatusTransition = null;
		var color = null;
		var invariantName = null;
		return new Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.OptionMetadata(label, value, state, defaultStatus, allowedStatusTransition, color, invariantName);
	}
}
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.OptionMetadata.createFromObjectData = function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_OptionMetadata$createFromObjectData(data) {
	var label = data['label'];
	var value = data['value'];
	var state = (('state') in data) ? data['state'] : -1;
	var defaultStatus = (('defaultstatus') in data) ? data['defaultstatus'] : -1;
	var allowedStatusTransition = (('allowedstatustransitions') in data) ? data['allowedstatustransitions'] : null;
	var color = (('color') in data) ? data['color'] : null;
	var invariantName = data['invariantname'];
	return new Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.OptionMetadata(label, value, state, defaultStatus, allowedStatusTransition, color, invariantName);
}
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.OptionMetadata.prototype = {
	_label$p$0: null,
	_val$p$0: 0,
	_state$p$0: 0,
	_defaultStatus$p$0: 0,
	_allowedStatusTransitions$p$0: null,
	_color$p$0: null,
	_invariantName$p$0: null,

	get_label: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_OptionMetadata$get_label() {
		return this._label$p$0;
	},

	set_label: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_OptionMetadata$set_label(value) {
		this._label$p$0 = value;
		return value;
	},

	get_valueString: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_OptionMetadata$get_valueString() {
		return this._val$p$0.toString();
	},

	set_valueString: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_OptionMetadata$set_valueString(value) {
		this._val$p$0 = Number.parseInvariant(value);
		return value;
	},

	get_allowedStatusTransitions: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_OptionMetadata$get_allowedStatusTransitions() {
		return this._allowedStatusTransitions$p$0;
	},

	set_allowedStatusTransitions: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_OptionMetadata$set_allowedStatusTransitions(value) {
		this._allowedStatusTransitions$p$0 = value;
		return value;
	},

	get_color: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_OptionMetadata$get_color() {
		return this._color$p$0;
	},

	set_color: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_OptionMetadata$set_color(value) {
		this._color$p$0 = value;
		return value;
	},

	get_value: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_OptionMetadata$get_value() {
		return this._val$p$0;
	},

	set_value: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_OptionMetadata$set_value(value) {
		this._val$p$0 = value;
		return value;
	},

	get_state: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_OptionMetadata$get_state() {
		return this._state$p$0;
	},

	set_state: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_OptionMetadata$set_state(value) {
		this._state$p$0 = value;
		return value;
	},

	get_defaultStatus: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_OptionMetadata$get_defaultStatus() {
		return this._defaultStatus$p$0;
	},

	set_defaultStatus: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_OptionMetadata$set_defaultStatus(value) {
		this._defaultStatus$p$0 = value;
		return value;
	},

	get_invariantName: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_OptionMetadata$get_invariantName() {
		return this._invariantName$p$0;
	},

	set_invariantName: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_OptionMetadata$set_invariantName(value) {
		this._invariantName$p$0 = value;
		return value;
	},

	getObjectData: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_OptionMetadata$getObjectData() {
		var data = {};
		data['label'] = this._label$p$0;
		data['value'] = this._val$p$0;
		data['state'] = this._state$p$0;
		data['defaultstatus'] = this._defaultStatus$p$0;
		data['allowedstatustransitions'] = this._allowedStatusTransitions$p$0;
		data['color'] = this._color$p$0;
		data['invariantname'] = this._invariantName$p$0;
		return data;
	},

	getValue: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_OptionMetadata$getValue(fieldName) {
		if (fieldName === Microsoft.Crm.Client.Core.Framework.FieldFormat.label || fieldName === 'label') {
			return this.get_label();
		}
		else if (fieldName === Microsoft.Crm.Client.Core.Framework.FieldFormat.value || fieldName === 'value') {
			return this.get_value();
		}
		else if (fieldName === Microsoft.Crm.Client.Core.Framework.FieldFormat.state || fieldName === 'state') {
			return this.get_state();
		}
		else if (fieldName === Microsoft.Crm.Client.Core.Framework.FieldFormat.defaultStatus || fieldName === 'defaultstatus') {
			return this._defaultStatus$p$0;
		}
		else if (fieldName === Microsoft.Crm.Client.Core.Framework.FieldFormat.allowedStatusTransitions || fieldName === 'allowedstatustransitions') {
			return this.get_allowedStatusTransitions();
		}
		else if (fieldName === Microsoft.Crm.Client.Core.Framework.FieldFormat.color || fieldName === 'color') {
			return this.get_color();
		}
		else {
			throw Error.argumentOutOfRange('fieldName', fieldName);
		}
	},

	toString: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_OptionMetadata$toString() {
		return this.get_state().toString();
	}
}


Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.OptionSetMetadata = function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_OptionSetMetadata(options) {
	Microsoft.Crm.Client.Core.Framework.Utils.ExceptionHelpers.throwOnNullOrUndefinedArgument(options, 'options');
	this._optionsInDisplayOrder$p$0 = options;
	this._options$p$0 = Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.OptionSetMetadata._createDictionaryFromOptionsInDisplayOrder$p(options);
}
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.OptionSetMetadata.createFromObjectData = function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_OptionSetMetadata$createFromObjectData(data) {
	var options = {};
	var $$dict_4 = data['optionsindisplayorder'];
	for (var $$key_5 in $$dict_4) {
		var entry = { key: $$key_5, value: $$dict_4[$$key_5] };
		var option = Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.OptionMetadata.createFromObjectData(entry.value);
		options[entry.key] = option;
	}
	return new Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.OptionSetMetadata(options);
}
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.OptionSetMetadata._createDictionaryFromOptionsInDisplayOrder$p = function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_OptionSetMetadata$_createDictionaryFromOptionsInDisplayOrder$p(data) {
	var options = {};
	var $$dict_4 = data;
	for (var $$key_5 in $$dict_4) {
		var entry = { key: $$key_5, value: $$dict_4[$$key_5] };
		var option = entry.value;
		options[option.get_valueString()] = option;
	}
	return options;
}
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.OptionSetMetadata.prototype = {
	_options$p$0: null,
	_optionsInDisplayOrder$p$0: null,

	get_options: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_OptionSetMetadata$get_options() {
		return this._options$p$0;
	},

	get_optionsInDisplayOrder: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_OptionSetMetadata$get_optionsInDisplayOrder() {
		return this._optionsInDisplayOrder$p$0;
	},

	createSimpleForm: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_OptionSetMetadata$createSimpleForm() {
		var simpleData = new Array(0);
		var $$dict_3 = this.get_optionsInDisplayOrder();
		for (var $$key_4 in $$dict_3) {
			var entry = { key: $$key_4, value: $$dict_3[$$key_4] };
			var optionData = {};
			optionData['Label'] = (entry.value).get_label();
			optionData['Value'] = (entry.value).get_value();
			optionData['Color'] = (entry.value).get_color();
			simpleData.push(optionData);
		}
		return simpleData;
	},

	getObjectData: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_OptionSetMetadata$getObjectData() {
		var data = {};
		if (!_Script.isNullOrUndefined(this._optionsInDisplayOrder$p$0)) {
			var optionData = {};
			var $$dict_4 = this._optionsInDisplayOrder$p$0;
			for (var $$key_5 in $$dict_4) {
				var entry = { key: $$key_5, value: $$dict_4[$$key_5] };
				var option = entry.value;
				optionData[entry.key] = option.getObjectData();
			}
			data['optionsindisplayorder'] = optionData;
		}
		return data;
	}
}


Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AliasedValue = function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_AliasedValue(entityLogicalName, attributeLogicalName, attributeType) {
	this._entityLogicalName$p$0 = entityLogicalName;
	this._attributeLogicalName$p$0 = attributeLogicalName;
	this._attributeType$p$0 = attributeType;
	this._innerValue$p$0 = null;
}
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AliasedValue.createFromObjectData = function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_AliasedValue$createFromObjectData(data) {
	var attributeLogicalName = data['attributeLogicalName'];
	var entityLogicalName = data['entityLogicalName'];
	var attributeType = data['attributeType'];
	var aliasedValue = new Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AliasedValue(entityLogicalName, attributeLogicalName, attributeType);
	aliasedValue.set_value(Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AliasedValue._createFieldFromObjectData$p(data['value'], attributeType));
	return aliasedValue;
}
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AliasedValue._createFieldFromObjectData$p = function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_AliasedValue$_createFieldFromObjectData$p(fieldValue, fieldType) {
	if (_Script.isNullOrUndefined(fieldValue)) {
		return null;
	}
	else {
		switch (fieldType) {
			case Xrm.Objects.AttributeType.aliasedValue:
				return Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AliasedValue.createFromObjectData(fieldValue);
			case Xrm.Objects.AttributeType.customer:
			case Xrm.Objects.AttributeType.lookup:
			case Xrm.Objects.AttributeType.owner:
				return Xrm.Objects.EntityReference.createFromObjectData(fieldValue);
			case Xrm.Objects.AttributeType.uniqueIdentifier:
				return Microsoft.Crm.Client.Core.Framework.Guid.createFromObjectData(fieldValue);
			case Xrm.Objects.AttributeType.dateTime:
				return new Date(Date.parse(fieldValue));
			case Xrm.Objects.AttributeType.status:
			case Xrm.Objects.AttributeType.state:
			case 0:
			case Xrm.Objects.AttributeType.picklist:
				return Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.OptionMetadata.createFromObjectData(fieldValue);
			default:
				return fieldValue;
		}
	}
}
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AliasedValue.prototype = {
	_entityLogicalName$p$0: null,
	_attributeLogicalName$p$0: null,
	_attributeType$p$0: 0,
	_innerValue$p$0: null,

	get_entityLogicalName: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_AliasedValue$get_entityLogicalName() {
		return this._entityLogicalName$p$0;
	},

	get_attributeLogicalName: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_AliasedValue$get_attributeLogicalName() {
		return this._attributeLogicalName$p$0;
	},

	get_attributeType: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_AliasedValue$get_attributeType() {
		return this._attributeType$p$0;
	},

	get_value: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_AliasedValue$get_value() {
		return this._innerValue$p$0;
	},

	set_value: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_AliasedValue$set_value(value) {
		this._innerValue$p$0 = value;
		return value;
	},

	getObjectData: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_AliasedValue$getObjectData() {
		var data = {};
		data['entityLogicalName'] = this._entityLogicalName$p$0;
		data['attributeLogicalName'] = this._attributeLogicalName$p$0;
		data['attributeType'] = this._attributeType$p$0;
		data['value'] = Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AliasedValue._createFieldFromObjectData$p(this._innerValue$p$0, this._attributeType$p$0);
		return data;
	}
}


Type.registerNamespace('Microsoft.Crm.Client.Core.Storage.DataApi');

Microsoft.Crm.Client.Core.Storage.DataApi.IChartableUserContext = function () { }
Microsoft.Crm.Client.Core.Storage.DataApi.IChartableUserContext.registerInterface('Microsoft.Crm.Client.Core.Storage.DataApi.IChartableUserContext');


Microsoft.Crm.Client.Core.Storage.DataApi.IRetrieveEntityMetadataDataSource = function () { }
Microsoft.Crm.Client.Core.Storage.DataApi.IRetrieveEntityMetadataDataSource.registerInterface('Microsoft.Crm.Client.Core.Storage.DataApi.IRetrieveEntityMetadataDataSource');


Microsoft.Crm.Client.Core.Storage.DataApi.AttributeMetadataQuery = function Microsoft_Crm_Client_Core_Storage_DataApi_AttributeMetadataQuery(entityLogicalName, attributeNames) {
	this._validateNames$p$0(entityLogicalName, attributeNames);
}
Microsoft.Crm.Client.Core.Storage.DataApi.AttributeMetadataQuery.prototype = {
	_entityLogicalName$p$0: null,
	_attributeNames$p$0: null,

	get_entityLogicalName: function Microsoft_Crm_Client_Core_Storage_DataApi_AttributeMetadataQuery$get_entityLogicalName() {
		return this._entityLogicalName$p$0;
	},

	get_validAttributeRegExp: function Microsoft_Crm_Client_Core_Storage_DataApi_AttributeMetadataQuery$get_validAttributeRegExp() {
		if (!Microsoft.Crm.Client.Core.Storage.DataApi.AttributeMetadataQuery._validAttributeRegExp$p) {
			Microsoft.Crm.Client.Core.Storage.DataApi.AttributeMetadataQuery._validAttributeRegExp$p = new RegExp('^[A-Za-z]+[A-Za-z0-9_]*$');
		}
		return Microsoft.Crm.Client.Core.Storage.DataApi.AttributeMetadataQuery._validAttributeRegExp$p;
	},

	get_attributeNames: function Microsoft_Crm_Client_Core_Storage_DataApi_AttributeMetadataQuery$get_attributeNames() {
		return this._attributeNames$p$0;
	},

	toString: function Microsoft_Crm_Client_Core_Storage_DataApi_AttributeMetadataQuery$toString() {
		var sb = new Sys.StringBuilder();
		sb.append(this._entityLogicalName$p$0);
		if (!_Script.isNullOrUndefined(this._attributeNames$p$0)) {
			sb.append(' : ');
			sb.append(this._attributeNames$p$0.join(', '));
		}
		return sb.toString();
	},

	_validateNames$p$0: function Microsoft_Crm_Client_Core_Storage_DataApi_AttributeMetadataQuery$_validateNames$p$0(entityLogicalName, attributeNames) {
		this._entityLogicalName$p$0 = entityLogicalName;
		if (Microsoft.Crm.Client.Core.Storage.Common.AllColumns.isInstanceOfType(attributeNames)) {
			this._attributeNames$p$0 = null;
		}
		else if (Microsoft.Crm.Client.Core.Storage.Common.ColumnSet.isInstanceOfType(attributeNames)) {
			this._attributeNames$p$0 = (attributeNames).get_columns();
		}
		else {
			this._attributeNames$p$0 = attributeNames;
		}
		if (!_Script.isNullOrUndefined(this._attributeNames$p$0)) {
			for (var i = 0; i < this._attributeNames$p$0.length; i++) {
				var isValid = this.get_validAttributeRegExp().test(this._attributeNames$p$0[i]);
				if (!isValid) {
					throw Error.argument(this._attributeNames$p$0[i], 'Invalid attribute name');
				}
			}
		}
	}
}


Type.registerNamespace('Microsoft.Crm.Client.Core.ViewModels');

Microsoft.Crm.Client.Core.ViewModels.IChartConfigurableViewModel = function () { }
Microsoft.Crm.Client.Core.ViewModels.IChartConfigurableViewModel.registerInterface('Microsoft.Crm.Client.Core.ViewModels.IChartConfigurableViewModel');


Type.registerNamespace('Microsoft.Crm.Client.Core.ViewModels.Controls');

function ChartConfigObject() {
	this.title = {};
	this.legend = {};
	this.chart = Microsoft.Crm.Client.Core.ViewModels.Controls.HighchartStubFactory.createHighchart();
	this.xAxis = [];
	this.yAxis = [];
	this.series = Microsoft.Crm.Client.Core.ViewModels.Controls.HighchartStubFactory.createHighchartSeriesArray();
	this.credits = {};
	this.colors = [];
	this.tooltip = {};
	this.plotOptions = null;
	this.CrmConfiguration = {};
	this.HighchartSeriesPointClicked = null;
	this.HighchartSeriesPointSelected = null;
	this.HighchartSeriesPointUnselected = null;
	this.chart.events = Microsoft.Crm.Client.Core.ViewModels.Controls.HighchartStubFactory.createHighchartEvents();
}


Microsoft.Crm.Client.Core.ViewModels.Controls.ChartType = function () { }
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartType.prototype = {
	none: 0,
	normalChart: 1,
	comparisonChart: 2,
	dateTimeChart: 3
}
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartType.registerEnum('Microsoft.Crm.Client.Core.ViewModels.Controls.ChartType', false);


Microsoft.Crm.Client.Core.ViewModels.Controls.ChartBuilder = function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartBuilder() {
	Microsoft.Crm.Client.Core.ViewModels.Controls.ChartBuilder.initializeBase(this);
}
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartBuilder.createChartBuilder = function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartBuilder$createChartBuilder(viewRecord, visualizationRecord, dataDefinition, chartData, visualizationRecordPresentationXML) {
	var chartBuilder = new Microsoft.Crm.Client.Core.ViewModels.Controls.ChartBuilder();
	chartBuilder._chartData$p$1 = chartData;
	chartBuilder._viewRecord$p$1 = viewRecord;
	chartBuilder._chartDataDescription$p$1 = dataDefinition;
	chartBuilder._visualizationRecord$p$1 = visualizationRecord;
	chartBuilder._visualizationRecordPresentationXML$p$1 = visualizationRecordPresentationXML;
	return chartBuilder;
}
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartBuilder.prototype = {
	_viewRecord$p$1: null,
	_visualizationRecord$p$1: null,
	_visualizationRecordPresentationXML$p$1: null,
	_chartData$p$1: null,
	_chartQueryModel$p$1: null,
	_chartType$p$1: 0,
	_chartPresentationSeries$p$1: null,
	_colors$p$1: null,
	_chartDataDescription$p$1: null,

	get_viewRecord: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartBuilder$get_viewRecord() {
		return this._viewRecord$p$1;
	},

	get_visualizationRecord: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartBuilder$get_visualizationRecord() {
		return this._visualizationRecord$p$1;
	},

	get_chartData: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartBuilder$get_chartData() {
		return this._chartData$p$1;
	},

	set_chartData: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartBuilder$set_chartData(value) {
		this._chartData$p$1 = value;
		return value;
	},

	internalDispose: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartBuilder$internalDispose() {
		if (!_Script.isNullOrUndefined(this._chartData$p$1)) {
			this._chartData$p$1.dispose();
			this._chartData$p$1 = null;
		}
		this._viewRecord$p$1 = null;
		this._chartDataDescription$p$1 = null;
		this._visualizationRecord$p$1 = null;
		this._chartQueryModel$p$1 = null;
	},

	buildChartModel: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartBuilder$buildChartModel() {
		this._chartQueryModel$p$1 = new Microsoft.Crm.Client.Core.Models.Chart.ChartQueryModel();
		if (this._chartData$p$1.get_count() > 0) {
			this._buildChartHeading$p$1();
			try {
				this._buildChartPresentation$p$1();
				this._buildChart$p$1();
			}
			catch (ex) {
				if (Microsoft.Crm.Client.Core.Framework.ChartError.isInstanceOfType(ex)) {
					var chartError = Microsoft.Crm.Client.Core.Framework.ErrorInfo.fromException(Microsoft.Crm.Client.Core.Framework.ChartError, ex, Microsoft.Crm.Client.Core.Framework.ChartError.typeName);
					this._buildErrorMessage$p$1(this._chartQueryModel$p$1, chartError);
				}
				else {
					throw ex;
				}
			}
		}
		else {
			this._buildDataEmptyMessage$p$1(this._chartQueryModel$p$1);
		}
		return this._chartQueryModel$p$1;
	},

	updateChartModel: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartBuilder$updateChartModel(updatedChartData) {
		this._chartData$p$1 = updatedChartData;
		if (this._chartData$p$1.get_count() > 0) {
			this._buildChartHeading$p$1();
			if (!this._chartPresentationSeries$p$1) {
				this._buildChartPresentation$p$1();
			}
			this._buildChart$p$1();
		}
		else {
			this._chartQueryModel$p$1 = new Microsoft.Crm.Client.Core.Models.Chart.ChartQueryModel();
			this._buildDataEmptyMessage$p$1(this._chartQueryModel$p$1);
		}
		return this._chartQueryModel$p$1;
	},

	get_primaryModelName: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartBuilder$get_primaryModelName() {
		return this._viewRecord$p$1.get_associatedEntityLogicalName();
	},

	_buildDataEmptyMessage$p$1: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartBuilder$_buildDataEmptyMessage$p$1(queryModel) {
		var chartHeading = Microsoft.Crm.Client.Core.Framework.Common.ResourceManager.getLocalizedString('Chart_NoData_Message');
		var title = new Microsoft.Crm.Client.Core.Models.ChartTitle();
		title.Text = chartHeading;
		title.HorizontalAlignment = 'center';
		title.VerticalAlignment = 'middle';
		queryModel.set_Title(title);
		queryModel.set_SubTitle(this._viewRecord$p$1.get_displayName());
		queryModel.set_displayMode(Microsoft.Crm.Client.Core.Framework.ChartDisplayMode.error);
		queryModel.set_errorInformation(new Microsoft.Crm.Client.Core.Framework.ChartErrorInformation());
		queryModel.get_errorInformation().set_ErrorType(null);
		queryModel.get_errorInformation().set_ErrorDescription(chartHeading);
	},

	_buildErrorMessage$p$1: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartBuilder$_buildErrorMessage$p$1(queryModel, chartError) {
		queryModel.set_Title(new Microsoft.Crm.Client.Core.Models.ChartTitle());
		queryModel.set_SubTitle(this._viewRecord$p$1.get_displayName());
		queryModel.set_displayMode(Microsoft.Crm.Client.Core.Framework.ChartDisplayMode.error);
		queryModel.set_errorInformation(new Microsoft.Crm.Client.Core.Framework.ChartErrorInformation());
		queryModel.get_errorInformation().set_ErrorType(chartError.get_title());
		queryModel.get_errorInformation().set_ErrorDescription(chartError.get_description());
	},

	_buildChartHeading$p$1: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartBuilder$_buildChartHeading$p$1() {
		var chartHeading = this._visualizationRecord$p$1.get_displayName();
		this._chartQueryModel$p$1.set_Title(new Microsoft.Crm.Client.Core.Models.ChartTitle());
		this._chartQueryModel$p$1.get_Title().Text = chartHeading;
		this._chartQueryModel$p$1.set_SubTitle(this._viewRecord$p$1.get_displayName());
	},

	_buildChartPresentation$p$1: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartBuilder$_buildChartPresentation$p$1() {
		var chartPresentationParser = new Microsoft.Crm.Client.Core.ViewModels.Controls.ChartPresentationParser(this._visualizationRecordPresentationXML$p$1);
		this._chartPresentationSeries$p$1 = chartPresentationParser.get_chartPresentationSeries();
		this._chartQueryModel$p$1.set_Legend(new Microsoft.Crm.Client.Core.Models.Legend());
		if (chartPresentationParser.get_chartPresentationLegendList().get_Count() > 0) {
			this._chartQueryModel$p$1.get_Legend().Floating = false;
			this._chartQueryModel$p$1.get_Legend().Enabled = chartPresentationParser.get_chartPresentationLegendList().get_item(0).get_enabled();
			this._chartQueryModel$p$1.get_Legend().HorizontalAlignment = chartPresentationParser.get_chartPresentationLegendList().get_item(0).get_alignment().toLowerCase();
			this._chartQueryModel$p$1.get_Legend().VerticalAlignment = chartPresentationParser.get_chartPresentationLegendList().get_item(0).get_docking().toLowerCase();
		}
		else {
			this._chartQueryModel$p$1.get_Legend().Enabled = false;
		}
		this._colors$p$1 = chartPresentationParser.get_colorsList();
	},

	_buildChart$p$1: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartBuilder$_buildChart$p$1() {
		this._chartType$p$1 = this._getChartType$p$1();
		var chartDataParser = Microsoft.Crm.Client.Core.ViewModels.Controls.ChartDataParserFactory.obtainParser(this._chartType$p$1);
		chartDataParser.initialize(this._chartData$p$1, this._chartDataDescription$p$1, this._chartPresentationSeries$p$1, this._chartQueryModel$p$1);
		this._chartQueryModel$p$1.set_Colors(this._colors$p$1);
		chartDataParser.parse();
	},

	_getChartType$p$1: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartBuilder$_getChartType$p$1() {
		if (this._chartDataDescription$p$1.get_isComparisonChart()) {
			return Microsoft.Crm.Client.Core.ViewModels.Controls.ChartType.comparisonChart;
		}
		if (this._chartDataDescription$p$1.get_category().get_primaryGroupBy().get_groupAttribute().get_dateTimeGrouping()) {
			return Microsoft.Crm.Client.Core.ViewModels.Controls.ChartType.dateTimeChart;
		}
		return Microsoft.Crm.Client.Core.ViewModels.Controls.ChartType.normalChart;
	}
}


Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator = function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartConfigGenerator(chart, metaDataAggregator, trace) {
	this.$$d_seriesPointLegendItemClicked = Function.createDelegate(this, this.seriesPointLegendItemClicked);
	if (_Script.isNullOrUndefined(chart)) {
		return;
	}
	this._chartConfigurationObject$p$0 = new ChartConfigObject();
	if (chart.get_isInteractionCentricDashboard()) {
		this._isFilterGraphContext$p$0 = true;
		this._secondaryChartGroupByAttributeName$p$0 = chart.get_secondaryGroupByAttributeName();
		if (!Microsoft.Crm.Client.Core.Framework._String.isNullOrEmpty(this._secondaryChartGroupByAttributeName$p$0)) {
			this._dataPointColorSettings$p$0 = false;
		}
		this._primaryModelName$p$0 = chart.get_primaryModelName();
	}
	this._metaDataAggregator$p$0 = metaDataAggregator;
	this._trace$p$0 = trace;
	this._setChartTitleOptions$p$0(chart.get_title());
	this._setChartOptions$p$0();
	this._chartConfigurationObject$p$0.colors = chart.get_colors();
	this._setChartXAxisOptions$p$0(chart.get_xAxes());
	this._setChartYAxisOptions$p$0(chart.get_yAxes());
	this._setChartSeriesOptions$p$0(chart.get_seriesList(), chart.get_xAxes(), chart.get_allowPointSelect());
	this._setChartLegendOptions$p$0(chart.get_legend());
	this._disableCredits$p$0();
	this._setTooltipState$p$0();
	if (this._isFilterGraphContext$p$0) {
		this._setPointerFollow$p$0();
	}
	this._postProcess$p$0();
}
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator.generateConfigurationObject = function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartConfigGenerator$generateConfigurationObject(chart, metaDataAggregator, trace) {
	var configGenerator = new Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator(chart, metaDataAggregator, trace);
	return configGenerator._chartConfigurationObject$p$0;
}
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._getChartType$p = function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartConfigGenerator$_getChartType$p(chartType, stackingType) {
	stackingType.val = Microsoft.Crm.Client.Core.Framework._String.empty;
	switch (chartType.toUpperCase()) {
		case 'BAR':
			return 'bar';
		case 'COLUMN':
			return 'column';
		case 'PIE':
			return 'pie';
		case 'STACKEDCOLUMN':
			stackingType.val = 'normal';
			return 'column';
		case 'STACKEDCOLUMN100':
			stackingType.val = 'percent';
			return 'column';
		case 'STACKEDBAR':
			stackingType.val = 'normal';
			return 'bar';
		case 'STACKEDBAR100':
			stackingType.val = 'percent';
			return 'bar';
		case 'LINE':
			return 'line';
		case 'FUNNEL':
			return 'funnel';
		case 'AREA':
			return 'area';
		case 'STACKEDAREA':
			stackingType.val = 'normal';
			return 'area';
		case 'STACKEDAREA100':
			stackingType.val = 'percent';
			return 'area';
		default:
			throw new Microsoft.Crm.Client.Core.Framework.ChartError(Microsoft.Crm.Client.Core.Framework.Common.ResourceManager.getLocalizedString('Chart_Unsupported_Title'), Microsoft.Crm.Client.Core.Framework.Common.ResourceManager.getLocalizedString('Chart_Unsupported_Message')).toException();
	}
}
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._setPropertyValueIfValid$p = function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartConfigGenerator$_setPropertyValueIfValid$p(propertyDictionary, propertyName, value) {
	if (!_Script.isNullOrUndefined(value)) {
		(propertyDictionary)[propertyName] = value;
		return true;
	}
	return false;
}
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator.prototype = {
	_chartConfigurationObject$p$0: null,
	_yAxesCount$p$0: 0,
	_xAxesCount$p$0: 0,
	_isFilterGraphContext$p$0: false,
	_dataPointColorSettings$p$0: true,
	_secondaryChartGroupByAttributeName$p$0: null,
	_primaryModelName$p$0: null,
	_metaDataAggregator$p$0: null,
	_trace$p$0: null,

	_setChartTitleOptions$p$0: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartConfigGenerator$_setChartTitleOptions$p$0(title) {
		var titleConfig = this._chartConfigurationObject$p$0.title;
		if (title) {
			titleConfig[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._textPropertyName$p] = title.Text.toUpperCase();
		}
		else {
			titleConfig[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._textPropertyName$p] = Microsoft.Crm.Client.Core.Framework._String.empty;
		}
		titleConfig[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._alignPropertyName$p] = 'left';
		titleConfig[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._verticalAlignPropertyName$p] = 'top';
		titleConfig[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._floatingPropertyName$p] = true;
		titleConfig[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._useHTMLPropertyName$p] = true;
		var titleStyle = {};
		titleStyle[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._colorPropertyName$p] = Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._titleColor$p;
		titleStyle[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._fontSizePropertyName$p] = Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._titleSize$p;
		titleStyle[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._fontFamilyPropertyName$p] = (this._isFilterGraphContext$p$0) ? Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._ishSegoeUiSemiBoldFontFamily$p : Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._segoeUiSemiBoldFontFamily$p;
		titleStyle[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._widthPropertyName$p] = Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._titleWidth$p;
		titleConfig[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._stylePropertyName$p] = titleStyle;
	},

	_setChartOptions$p$0: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartConfigGenerator$_setChartOptions$p$0() {
		this._chartConfigurationObject$p$0.plotOptions = Microsoft.Crm.Client.Core.ViewModels.Controls.HighchartStubFactory.createHighchartPlotOptions();
		this._chartConfigurationObject$p$0.plotOptions.series = Microsoft.Crm.Client.Core.ViewModels.Controls.HighchartStubFactory.createHighchartSeries();
		this._chartConfigurationObject$p$0.plotOptions.series.animation = false;
		if (this._isFilterGraphContext$p$0) {
			this._chartConfigurationObject$p$0.plotOptions.series.point = {};
			this._chartConfigurationObject$p$0.plotOptions.series.point[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._eventsPropertyName$p] = {};
			(this._chartConfigurationObject$p$0.plotOptions.series.point[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._eventsPropertyName$p])[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._legendClickPropertyName$p] = this.$$d_seriesPointLegendItemClicked;
			this._chartConfigurationObject$p$0.plotOptions.series.events = {};
			this._chartConfigurationObject$p$0.plotOptions.series.events[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._legendClickPropertyName$p] = this.$$d_seriesPointLegendItemClicked;
		}
	},

	seriesPointLegendItemClicked: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartConfigGenerator$seriesPointLegendItemClicked() {
		return false;
	},

	_setChartXAxisOptions$p$0: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartConfigGenerator$_setChartXAxisOptions$p$0(xAxes) {
		if (xAxes && xAxes.length > 0) {
			for (var $$arr_1 = xAxes, $$len_2 = $$arr_1.length, $$idx_3 = 0; $$idx_3 < $$len_2; ++$$idx_3) {
				var xaxis = $$arr_1[$$idx_3];
				var xaxisConfig = {};
				xaxisConfig[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._gridlineDashStylePropertyName$p] = Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._gridlineDashStyle$p;
				xaxisConfig[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._gridlineColorPropertyName$p] = Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._gridlineColor$p;
				xaxisConfig[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._lineColorPropertyName$p] = Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._lineColor$p;
				xaxisConfig[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._endOnTickPropertyName$p] = true;
				xaxisConfig[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._startOnTickPropertyName$p] = true;
				var labelsStyle = {};
				labelsStyle[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._colorPropertyName$p] = (this._isFilterGraphContext$p$0) ? Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._icAxisLabelColor$p : Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._axisLabelColor$p;
				labelsStyle[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._fontSizePropertyName$p] = (this._isFilterGraphContext$p$0) ? Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._filterGraphFontSize$p : Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._defaultFontSize$p;
				if (this._isFilterGraphContext$p$0) {
					labelsStyle[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._lineHeightPropertyName$p] = Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._filterGraphLineHeight$p;
				}
				labelsStyle[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._fontFamilyPropertyName$p] = (this._isFilterGraphContext$p$0) ? Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._ishSegoeUiFontFamily$p : Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._segoeUiFontFamily$p;
				var labels = {};
				labels[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._stylePropertyName$p] = labelsStyle;
				labels[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._overflowPropertyName$p] = Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._overflowJustify$p;
				if (this._isFilterGraphContext$p$0) {
					var $$t_D = this;
					labels[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._formatterPropertyName$p] = function () {
						var label = this.value;
						var labelText = label.toString();
						if (!_Script.isNullOrUndefined(labelText)) {
							if (labelText.length > 10) {
								var ellipsis = Microsoft.Crm.Client.Core.Framework.Common.ResourceManager.getLocalizedString('ActivityContainerControl.SubjectEllipsesText');
								return labelText.substr(0, 7) + ellipsis;
							}
							return label.toString();
						}
						return Microsoft.Crm.Client.Core.Framework._String.empty;
					};
				}
				xaxisConfig[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._labelsPropertyName$p] = labels;
				var xaxisTitle = {};
				if (!Microsoft.Crm.Client.Core.Framework._String.isNullOrWhiteSpace(xaxis.Title) && !this._isFilterGraphContext$p$0) {
					xaxisTitle[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._textPropertyName$p] = xaxis.Title;
					var titleStyle = {};
					titleStyle[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._colorPropertyName$p] = (this._isFilterGraphContext$p$0) ? Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._icAxisLabelColor$p : Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._axisLabelColor$p;
					titleStyle[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._fontSizePropertyName$p] = Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._defaultFontSize$p;
					titleStyle[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._fontFamilyPropertyName$p] = (this._isFilterGraphContext$p$0) ? Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._ishSegoeUiSemiBoldFontFamily$p : Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._segoeUiSemiBoldFontFamily$p;
					titleStyle[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._fontWeightPropertyName$p] = Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._fontWeightNormal$p;
					xaxisTitle[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._stylePropertyName$p] = titleStyle;
					xaxisTitle[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._marginPropertyName$p] = 15;
					xaxisTitle[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._alignPropertyName$p] = 'low';
					xaxisTitle[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._xPropertyName$p] = -4;
				}
				else {
					xaxisTitle[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._textPropertyName$p] = null;
				}
				xaxisConfig[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._titlePropertyName$p] = xaxisTitle;
				Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._setPropertyValueIfValid$p(xaxisConfig, Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._categoriesPropertyName$p, xaxis.Values);
				if (this._xAxesCount$p$0 % 2) {
					xaxisConfig[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._oppositePropertyName$p] = true;
				}
				this._xAxesCount$p$0++;
				Array.add(this._chartConfigurationObject$p$0.xAxis, xaxisConfig);
			}
		}
	},

	_setChartYAxisOptions$p$0: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartConfigGenerator$_setChartYAxisOptions$p$0(yAxes) {
		if (yAxes && yAxes.length > 0) {
			for (var $$arr_1 = yAxes, $$len_2 = $$arr_1.length, $$idx_3 = 0; $$idx_3 < $$len_2; ++$$idx_3) {
				var yaxis = $$arr_1[$$idx_3];
				var yaxisConfig = {};
				yaxisConfig[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._gridlineDashStylePropertyName$p] = Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._gridlineDashStyle$p;
				yaxisConfig[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._gridlineColorPropertyName$p] = Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._gridlineColor$p;
				yaxisConfig[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._lineWidthPropertyName$p] = '1';
				yaxisConfig[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._lineColorPropertyName$p] = Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._lineColor$p;
				yaxisConfig[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._endOnTickPropertyName$p] = true;
				yaxisConfig[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._startOnTickPropertyName$p] = true;
				var labelsStyle = {};
				labelsStyle[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._colorPropertyName$p] = (this._isFilterGraphContext$p$0) ? Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._icAxisLabelColor$p : Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._axisLabelColor$p;
				labelsStyle[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._fontSizePropertyName$p] = Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._defaultFontSize$p;
				labelsStyle[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._fontFamilyPropertyName$p] = (this._isFilterGraphContext$p$0) ? Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._ishSegoeUiFontFamily$p : Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._segoeUiFontFamily$p;
				var labels = {};
				labels[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._stylePropertyName$p] = labelsStyle;
				labels[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._overflowPropertyName$p] = Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._overflowJustify$p;
				var $$t_A = this;
				labels[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._formatterPropertyName$p] = function () {
					return Microsoft.Crm.Client.Core.Framework._String.empty + String.localeFormat("{0:N}", this.value);
				};
				if (this._isFilterGraphContext$p$0) {
					yaxisConfig[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._gridlineWidthPropertyName$p] = 0;
					labels[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._enabledPropertyName$p] = false;
				}
				yaxisConfig[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._labelsPropertyName$p] = labels;
				var yaxisTitle = {};
				if (!Microsoft.Crm.Client.Core.Framework._String.isNullOrWhiteSpace(yaxis.Title) && !this._isFilterGraphContext$p$0) {
					yaxisTitle[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._textPropertyName$p] = yaxis.Title;
					var titleStyle = {};
					titleStyle[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._colorPropertyName$p] = Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._axisLabelColor$p;
					titleStyle[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._fontSizePropertyName$p] = Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._defaultFontSize$p;
					titleStyle[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._fontFamilyPropertyName$p] = (this._isFilterGraphContext$p$0) ? Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._ishSegoeUiSemiBoldFontFamily$p : Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._segoeUiSemiBoldFontFamily$p;
					titleStyle[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._fontWeightPropertyName$p] = Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._fontWeightNormal$p;
					yaxisTitle[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._stylePropertyName$p] = titleStyle;
					yaxisTitle[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._marginPropertyName$p] = 15;
					yaxisTitle[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._alignPropertyName$p] = 'low';
					yaxisTitle[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._xPropertyName$p] = -4;
				}
				else {
					yaxisTitle[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._textPropertyName$p] = null;
				}
				yaxisConfig[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._titlePropertyName$p] = yaxisTitle;
				if (this._yAxesCount$p$0 % 2) {
					yaxisConfig[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._oppositePropertyName$p] = true;
				}
				this._yAxesCount$p$0++;
				Array.add(this._chartConfigurationObject$p$0.yAxis, yaxisConfig);
			}
		}
	},

	_setChartSeriesColorOptions$p$0: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartConfigGenerator$_setChartSeriesColorOptions$p$0(seriesConfig) {
		seriesConfig.color = this._fetchColorForLabel$p$0(this._secondaryChartGroupByAttributeName$p$0, seriesConfig.name);
	},

	_setChartSeriesOptions$p$0: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartConfigGenerator$_setChartSeriesOptions$p$0(chartSeriesList, xaxes, allowPointSelect) {
		if (chartSeriesList && chartSeriesList.length > 0) {
			for (var $$arr_3 = chartSeriesList, $$len_4 = $$arr_3.length, $$idx_5 = 0; $$idx_5 < $$len_4; ++$$idx_5) {
				var series = $$arr_3[$$idx_5];
				var seriesConfig = Microsoft.Crm.Client.Core.ViewModels.Controls.HighchartStubFactory.createHighchartSeries();
				seriesConfig.color = series.Color;
				seriesConfig.borderWidth = series.BorderWidth;
				seriesConfig.borderColor = series.BorderColor;
				if (!_Script.isNullOrUndefined(series.Title)) {
					seriesConfig.name = series.Title;
				}
				if (this._isFilterGraphContext$p$0 && !this._dataPointColorSettings$p$0) {
					this._setChartSeriesColorOptions$p$0(seriesConfig);
				}
				if (!_Script.isNullOrUndefined(series.YAxisNumber) && series.YAxisNumber < this._yAxesCount$p$0 && series.YAxisNumber > 0) {
					seriesConfig.yAxis = series.YAxisNumber;
				}
				if (!_Script.isNullOrUndefined(series.XAxisNumber) && series.XAxisNumber < this._xAxesCount$p$0 && series.XAxisNumber > 0) {
					seriesConfig.xAxis = series.XAxisNumber;
				}
				var stackingType;
				var $$t_C, $$t_D;
				var chartType = (($$t_D = Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._getChartType$p(series.ChartType, ($$t_C = { 'val': stackingType }))), stackingType = $$t_C.val, $$t_D);
				seriesConfig.type = chartType;
				if (!_Script.isNullOrUndefined(stackingType) && stackingType.length) {
					seriesConfig.stacking = stackingType;
				}
				var addCategoriesToData = false;
				this._setSeriesOptionsDataLabel$p$0(seriesConfig, series.dataLabels);
				seriesConfig.allowPointSelect = allowPointSelect;
				seriesConfig.states = {};
				seriesConfig.states['select'] = Microsoft.Crm.Client.Core.ViewModels.Controls.HighchartStubFactory.createHighchartSeriesSelectState();
				if (chartType === 'pie') {
					this._setSeriesPieOptions$p$0(seriesConfig);
					addCategoriesToData = true;
				}
				else if (chartType === 'funnel') {
					this._setSeriesFunnelOptions$p$0(seriesConfig);
					addCategoriesToData = true;
				}
				if (this._isFilterGraphContext$p$0) {
					(seriesConfig)[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._cursorPropertyName$p] = 'pointer';
				}
				var data = this._getSeriesData$p$0(series, xaxes, addCategoriesToData);
				seriesConfig.data = data;
				this._chartConfigurationObject$p$0.series[this._chartConfigurationObject$p$0.series.length] = seriesConfig;
			}
		}
	},

	_setDataPointColorsOptions$p$0: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartConfigGenerator$_setDataPointColorsOptions$p$0(point, dataPoint) {
		try {
			var aggregator = dataPoint.Aggregators[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._firstAggregatorIndex$p];
			point.color = this._fetchColorForValue$p$0(aggregator.FieldName, Number.parseInvariant(aggregator.Value));
		}
		catch (e) {
			this._trace$p$0.executeLogWarning(Microsoft.Crm.Client.Core.Framework.TraceComponent.localDataSource, e.message);
			point.color = Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._defaultColor$p;
		}
	},

	_getSeriesData$p$0: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartConfigGenerator$_getSeriesData$p$0(series, xaxes, addXValueToDataPoint) {
		Microsoft.Crm.Client.Core.Framework.Debug.assert(!_Script.isNullOrUndefined(series), 'Series should not be null or undefined');
		Microsoft.Crm.Client.Core.Framework.Debug.assert(!addXValueToDataPoint || (!_Script.isNullOrUndefined(xaxes) && !_Script.isNullOrUndefined(xaxes[0].Values)), 'XAxes[0] should have Values array for pie chart');
		var data = new Array(0);
		if (series.DataPoints) {
			Microsoft.Crm.Client.Core.Framework.Debug.assert(series.DataPoints.length <= xaxes[0].Values.length, 'The number of data points cannot be more than the XAxes Values');
			for (var i = 0; i < series.DataPoints.length; i++) {
				if (!_Script.isNullOrUndefined(series.DataPoints[i].Value)) {
					var point = Microsoft.Crm.Client.Core.ViewModels.Controls.HighchartStubFactory.createHighchartSeriesData();
					if (addXValueToDataPoint) {
						point.name = xaxes[0].Values[i];
					}
					point.y = series.DataPoints[i].Value;
					point.Aggregators = series.DataPoints[i].Aggregators;
					point.FormattedValue = series.DataPoints[i].FormattedValue;
					point.events = Microsoft.Crm.Client.Core.ViewModels.Controls.HighchartStubFactory.createHighchartEvents();
					var $$t_D = this;
					point.events.click = function (eevent) {
						if (!_Script.isNullOrUndefined($$t_D._chartConfigurationObject$p$0.HighchartSeriesPointClicked)) {
							var highchartClickedPoint = this;
							$$t_D._chartConfigurationObject$p$0.HighchartSeriesPointClicked(highchartClickedPoint);
						}
					};
					var $$t_E = this;
					point.events.select = function (eevent) {
						if (!_Script.isNullOrUndefined($$t_E._chartConfigurationObject$p$0.HighchartSeriesPointSelected)) {
							var highchartSelectedPoint = this;
							$$t_E._chartConfigurationObject$p$0.HighchartSeriesPointSelected(highchartSelectedPoint);
						}
					};
					var $$t_F = this;
					point.events.unselect = function (eevent) {
						if (!_Script.isNullOrUndefined($$t_F._chartConfigurationObject$p$0.HighchartSeriesPointUnselected)) {
							var highchartUnselectedPoint = this;
							$$t_F._chartConfigurationObject$p$0.HighchartSeriesPointUnselected(highchartUnselectedPoint);
						}
					};
					if (this._isFilterGraphContext$p$0 && this._dataPointColorSettings$p$0) {
						this._setDataPointColorsOptions$p$0(point, series.DataPoints[i]);
					}
					data[data.length] = point;
				}
				else {
					var point = Microsoft.Crm.Client.Core.ViewModels.Controls.HighchartStubFactory.createHighchartSeriesData();
					point.y = null;
					data[data.length] = point;
				}
			}
		}
		return data;
	},

	_setSeriesPieOptions$p$0: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartConfigGenerator$_setSeriesPieOptions$p$0(seriesConfig) {
		Microsoft.Crm.Client.Core.Framework.Debug.assert(!_Script.isNullOrUndefined(seriesConfig), 'Series config should not be null or undefined');
		seriesConfig[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._cursorPropertyName$p] = 'pointer';
		seriesConfig[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._borderWidthPropertyName$p] = 1;
		seriesConfig[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._borderColorPropertyName$p] = Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._defaultBorderColor$p;
		seriesConfig[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._showInLegendPropertyName$p] = true;
		var dataLabels = {};
		dataLabels[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._enabledPropertyName$p] = true;
		dataLabels['softConnector'] = true;
		dataLabels['distance'] = 18;
		var $$t_3 = this;
		dataLabels[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._formatterPropertyName$p] = function () {
			return Microsoft.Crm.Client.Core.Framework._String.empty + this.point.FormattedValue;
		};
		var style = {};
		style[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._colorPropertyName$p] = (this._isFilterGraphContext$p$0) ? Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._icDefaultLegendColor$p : Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._specialLegendColor$p;
		style[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._fontSizePropertyName$p] = Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._defaultFontSize$p;
		style[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._fontFamilyPropertyName$p] = (this._isFilterGraphContext$p$0) ? Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._ishSegoeUiFontFamily$p : Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._segoeUiFontFamily$p;
		dataLabels[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._stylePropertyName$p] = style;
		seriesConfig[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._dataLabelsPropertyName$p] = dataLabels;
	},

	_setSeriesOptionsDataLabel$p$0: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartConfigGenerator$_setSeriesOptionsDataLabel$p$0(seriesConfig, dataLabelsOption) {
		if (dataLabelsOption && dataLabelsOption.Enabled) {
			var dataLabels = {};
			dataLabels[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._enabledPropertyName$p] = dataLabelsOption.Enabled;
			if (dataLabelsOption.LabelFormatter) {
				dataLabels[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._formatterPropertyName$p] = dataLabelsOption.LabelFormatter;
			}
			var style = {};
			style[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._colorPropertyName$p] = Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._specialLegendColor$p;
			style[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._fontSizePropertyName$p] = (this._isFilterGraphContext$p$0) ? Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._filterGraphFontSize$p : Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._defaultFontSize$p;
			style[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._fontFamilyPropertyName$p] = (this._isFilterGraphContext$p$0) ? Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._ishSegoeUiFontFamily$p : Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._segoeUiFontFamily$p;
			dataLabels[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._stylePropertyName$p] = style;
			seriesConfig[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._dataLabelsPropertyName$p] = dataLabels;
		}
		if (this._isFilterGraphContext$p$0) {
			var dataLabels = {};
			dataLabels[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._enabledPropertyName$p] = true;
			var style = {};
			style[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._colorPropertyName$p] = Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._icDefaultLegendColor$p;
			style[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._fontSizePropertyName$p] = Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._filterGraphFontSize$p;
			dataLabels[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._stylePropertyName$p] = style;
			if (!_Script.isNullOrUndefined(dataLabelsOption) && !_Script.isNullOrUndefined(!!dataLabelsOption.LabelFormatter)) {
				dataLabels[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._formatterPropertyName$p] = dataLabelsOption.LabelFormatter;
			}
			else {
				var $$t_6 = this;
				dataLabels[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._formatterPropertyName$p] = function () {
					return this.point.FormattedValue != null ? this.point.FormattedValue : '0';
				};
			}
			seriesConfig[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._dataLabelsPropertyName$p] = dataLabels;
		}
	},

	_setSeriesFunnelOptions$p$0: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartConfigGenerator$_setSeriesFunnelOptions$p$0(seriesConfig) {
		Microsoft.Crm.Client.Core.Framework.Debug.assert(!_Script.isNullOrUndefined(seriesConfig), 'Series config should not be null or undefined');
		seriesConfig['neckHeight'] = '20%';
		seriesConfig['neckWidth'] = '40%';
		seriesConfig[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._borderWidthPropertyName$p] = 1;
		seriesConfig[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._borderColorPropertyName$p] = Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._defaultBorderColor$p;
		seriesConfig[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._showInLegendPropertyName$p] = true;
		var dataLabels = {};
		dataLabels[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._enabledPropertyName$p] = true;
		dataLabels['softConnector'] = false;
		dataLabels['distance'] = 1;
		var $$t_3 = this;
		dataLabels[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._formatterPropertyName$p] = function () {
			return Microsoft.Crm.Client.Core.Framework._String.empty + this.point.FormattedValue;
		};
		var style = {};
		style[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._colorPropertyName$p] = (this._isFilterGraphContext$p$0) ? Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._icDefaultLegendColor$p : Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._specialLegendColor$p;
		style[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._fontSizePropertyName$p] = Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._defaultFontSize$p;
		style[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._fontFamilyPropertyName$p] = (this._isFilterGraphContext$p$0) ? Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._ishSegoeUiFontFamily$p : Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._segoeUiFontFamily$p;
		dataLabels[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._stylePropertyName$p] = style;
		seriesConfig[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._dataLabelsPropertyName$p] = dataLabels;
		this._chartConfigurationObject$p$0.chart.marginBottom = 62;
		this._chartConfigurationObject$p$0.chart.marginRight = 70;
		this._chartConfigurationObject$p$0.chart.marginLeft = 35;
	},

	_setChartLegendOptions$p$0: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartConfigGenerator$_setChartLegendOptions$p$0(chartLegend) {
		if (chartLegend) {
			Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._setPropertyValueIfValid$p(this._chartConfigurationObject$p$0.legend, Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._enabledPropertyName$p, chartLegend.Enabled);
			this._chartConfigurationObject$p$0.legend[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._alignPropertyName$p] = 'left';
			this._chartConfigurationObject$p$0.legend[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._verticalAlignPropertyName$p] = 'bottom';
			this._chartConfigurationObject$p$0.legend[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._floatingPropertyName$p] = true;
			this._chartConfigurationObject$p$0.legend['layout'] = (this._isFilterGraphContext$p$0) ? 'horizontal' : 'vertical';
			var itemStyle = {};
			itemStyle[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._colorPropertyName$p] = (this._isFilterGraphContext$p$0) ? Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._icDefaultLegendColor$p : Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._defaultLegendColor$p;
			itemStyle[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._fontSizePropertyName$p] = (this._isFilterGraphContext$p$0) ? Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._filterGraphFontSize$p : Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._defaultFontSize$p;
			itemStyle[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._fontFamilyPropertyName$p] = (this._isFilterGraphContext$p$0) ? Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._ishSegoeUiFontFamily$p : Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._segoeUiFontFamily$p;
			if (this._isFilterGraphContext$p$0) {
				itemStyle['fontWeight'] = 'normal';
			}
			this._chartConfigurationObject$p$0.legend[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._borderWidthPropertyName$p] = 0;
			this._chartConfigurationObject$p$0.legend[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._itemStylePropertyName$p] = itemStyle;
			this._chartConfigurationObject$p$0.legend[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._paddingPropertyName$p] = 13;
			this._chartConfigurationObject$p$0.legend[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._symbolWidthPropertyName$p] = (this._isFilterGraphContext$p$0) ? 12 : 15;
			this._chartConfigurationObject$p$0.legend[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._symbolPaddingPropertyName$p] = 5;
			this._chartConfigurationObject$p$0.legend[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._xPropertyName$p] = (this._isFilterGraphContext$p$0) ? 0 : -13;
			this._chartConfigurationObject$p$0.legend[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._itemMarginTopPropertyName$p] = (this._isFilterGraphContext$p$0) ? 10 : 12;
			if (this._isFilterGraphContext$p$0) {
				this._chartConfigurationObject$p$0.legend[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._enabledPropertyName$p] = true;
				this._chartConfigurationObject$p$0.legend[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._verticalAlignPropertyName$p] = 'top';
				this._chartConfigurationObject$p$0.legend['maxHeight'] = 60;
				var $$t_7 = this;
				this._chartConfigurationObject$p$0.legend['labelFormatter'] = function () {
					var legendName = this.name;
					var legendNameText = Microsoft.Crm.Client.Core.Framework._String.empty;
					var ellipsis = Microsoft.Crm.Client.Core.Framework.Common.ResourceManager.getLocalizedString('ActivityContainerControl.SubjectEllipsesText');
					if (!_Script.isNullOrUndefined(legendName)) {
						legendNameText = legendName.toString();
						if (legendNameText.length > Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._maxLengthLegend$p) {
							legendNameText = legendNameText.substr(0, Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._maxLengthLegend$p - ellipsis.length) + ellipsis;
						}
					}
					return legendNameText;
				};
				var onHoverStyle = {};
				onHoverStyle[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._colorPropertyName$p] = Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._icDefaultLegendColor$p;
				this._chartConfigurationObject$p$0.legend[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._hoverStylePropertyName$p] = onHoverStyle;
				var navigationProps = {};
				navigationProps['activeColor'] = '#FFFFFF';
				navigationProps['inactiveColor'] = '#444444';
				navigationProps['style'] = {};
				(navigationProps['style'])['color'] = '#999999';
				this._chartConfigurationObject$p$0.legend['navigation'] = navigationProps;
				(this._chartConfigurationObject$p$0.legend[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._itemStylePropertyName$p])[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._cursorPropertyName$p] = 'default';
			}
		}
	},

	_disableCredits$p$0: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartConfigGenerator$_disableCredits$p$0() {
		this._chartConfigurationObject$p$0.credits[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._enabledPropertyName$p] = false;
	},

	_setTooltipState$p$0: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartConfigGenerator$_setTooltipState$p$0() {
		this._chartConfigurationObject$p$0.tooltip[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._enabledPropertyName$p] = (this._isFilterGraphContext$p$0) ? true : false;
		if (this._isFilterGraphContext$p$0) {
			var toolTipPointFormat;
			if (this._chartConfigurationObject$p$0.series.length && (this._chartConfigurationObject$p$0.series[0].type === 'pie' || this._chartConfigurationObject$p$0.series[0].type === 'funnel')) {
				toolTipPointFormat = 'Series:\"{series.name}\"  Point:\"{point.name}\"<br/> Value: \"{point.y}\"';
			}
			else {
				toolTipPointFormat = 'Series:\"{series.name}\"  Point:\"{point.category}\"<br/> Value: \"{point.y}\"';
			}
			this._chartConfigurationObject$p$0.tooltip[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._toolTipPointFormatPropertyName$p] = toolTipPointFormat;
			var toolTipHeaderFormat = Microsoft.Crm.Client.Core.Framework._String.empty;
			this._chartConfigurationObject$p$0.tooltip[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._toolTipHeaderFormatPropertyName$p] = toolTipHeaderFormat;
		}
	},

	_setPointerFollow$p$0: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartConfigGenerator$_setPointerFollow$p$0() {
		if (this._chartConfigurationObject$p$0.series[0].type === 'bar' || this._chartConfigurationObject$p$0.series[0].type === 'column' || this._chartConfigurationObject$p$0.series[0].type === 'pie' || this._chartConfigurationObject$p$0.series[0].type === 'funnel') {
			this._chartConfigurationObject$p$0.tooltip['followPointer'] = true;
		}
	},

	_postProcess$p$0: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartConfigGenerator$_postProcess$p$0() {
		var maxLengthCategoryName = 0;
		if (this._chartConfigurationObject$p$0.series && this._chartConfigurationObject$p$0.series.length > 0) {
			for (var seriesIndex = 0; seriesIndex < this._chartConfigurationObject$p$0.series.length; seriesIndex++) {
				var series = this._chartConfigurationObject$p$0.series[seriesIndex];
				var xAxisDictionary = this._chartConfigurationObject$p$0.xAxis[seriesIndex];
				if (series.type === 'bar') {
					series.data.reverse();
					if (!_Script.isNullOrUndefined(xAxisDictionary)) {
						if (((Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._categoriesPropertyName$p) in xAxisDictionary)) {
							(xAxisDictionary[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._categoriesPropertyName$p]).reverse();
						}
					}
				}
				var horizontalAxisDictionary = null;
				var verticalAxisDictionary = null;
				if (series.type === 'bar') {
					horizontalAxisDictionary = this._chartConfigurationObject$p$0.yAxis[seriesIndex];
					verticalAxisDictionary = xAxisDictionary;
				}
				else {
					horizontalAxisDictionary = xAxisDictionary;
					verticalAxisDictionary = this._chartConfigurationObject$p$0.yAxis[seriesIndex];
				}
				maxLengthCategoryName = this._processHorizontalAxis$p$0(horizontalAxisDictionary, maxLengthCategoryName);
				this._processVerticalAxis$p$0(verticalAxisDictionary);
			}
		}
		this._chartConfigurationObject$p$0.CrmConfiguration['maxLengthCategoryName'] = maxLengthCategoryName;
	},

	_processHorizontalAxis$p$0: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartConfigGenerator$_processHorizontalAxis$p$0(horizontalAxisDictionary, maxLengthCategoryName) {
		if (!_Script.isNullOrUndefined(horizontalAxisDictionary)) {
			var rotateSeriesLabels = false;
			var categoriesExist = ((Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._categoriesPropertyName$p) in horizontalAxisDictionary);
			if (categoriesExist) {
				var categories = horizontalAxisDictionary[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._categoriesPropertyName$p];
				if (categories.length < 2) {
					return maxLengthCategoryName;
				}
				if (categories.length > 15) {
					var labelsDictionary = horizontalAxisDictionary[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._labelsPropertyName$p];
					labelsDictionary[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._enabledPropertyName$p] = false;
					return maxLengthCategoryName;
				}
				var maxLabelLength = 3;
				if (categories.length > 9) {
					maxLabelLength = 1;
				}
				var ellipsis = Microsoft.Crm.Client.Core.Framework.Common.ResourceManager.getLocalizedString('ActivityContainerControl.SubjectEllipsesText');
				for (var loopIndex = 0; loopIndex < categories.length; loopIndex++) {
					if (categories[loopIndex].length > maxLabelLength) {
						if (!this._isFilterGraphContext$p$0) {
							if (categories[loopIndex].length > 20) {
								categories[loopIndex] = categories[loopIndex].substr(0, 17) + ellipsis;
							}
						}
						rotateSeriesLabels = true;
						maxLengthCategoryName = (maxLengthCategoryName < categories[loopIndex].length) ? categories[loopIndex].length : maxLengthCategoryName;
					}
				}
			}
			if (rotateSeriesLabels || !categoriesExist) {
				var labelsDictionary = horizontalAxisDictionary[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._labelsPropertyName$p];
				var titleDictionary = horizontalAxisDictionary[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._titlePropertyName$p];
				labelsDictionary[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._rotationPropertyName$p] = (this._isFilterGraphContext$p$0) ? -45 : 270;
				labelsDictionary[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._alignPropertyName$p] = 'right';
				titleDictionary[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._marginPropertyName$p] = 15;
			}
		}
		return maxLengthCategoryName;
	},

	_processVerticalAxis$p$0: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartConfigGenerator$_processVerticalAxis$p$0(verticalAxisDictionary) {
		if (!_Script.isNullOrUndefined(verticalAxisDictionary)) {
			if (((Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._categoriesPropertyName$p) in verticalAxisDictionary)) {
				var maxLabelLength = 20;
				var categories = verticalAxisDictionary[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._categoriesPropertyName$p];
				if (categories.length > 31) {
					var labelsDictionary = verticalAxisDictionary[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._labelsPropertyName$p];
					labelsDictionary[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._enabledPropertyName$p] = false;
					return;
				}
				if (categories.length > 9) {
					maxLabelLength = 11;
				}
				var ellipsis = Microsoft.Crm.Client.Core.Framework.Common.ResourceManager.getLocalizedString('ActivityContainerControl.SubjectEllipsesText');
				for (var loopIndex = 0; loopIndex < categories.length; loopIndex++) {
					if (categories[loopIndex].length > maxLabelLength) {
						categories[loopIndex] = categories[loopIndex].substr(0, maxLabelLength - ellipsis.length) + ellipsis;
					}
				}
				if (this._isFilterGraphContext$p$0 && this._chartConfigurationObject$p$0.series[0].type === 'bar') {
					var labelsDictionary = verticalAxisDictionary[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._labelsPropertyName$p];
					if (labelsDictionary) {
						labelsDictionary[Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._rotationPropertyName$p] = (this._isFilterGraphContext$p$0) ? 0 : 270;
					}
				}
			}
		}
	},

	_fetchColorForValue$p$0: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartConfigGenerator$_fetchColorForValue$p$0(attribute, value) {
		if (_Script.isNullOrUndefined(value)) {
			return Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._defaultColor$p;
		}
		try {
			var attributeMetaData = this._getOptionSetMetadata$p$0(attribute);
			var $$dict_5 = attributeMetaData.getChartingMetaDataInDisplayOrder();
			for (var $$key_6 in $$dict_5) {
				var entry = { key: $$key_6, value: $$dict_5[$$key_6] };
				var chartingMetaData = entry.value;
				if (chartingMetaData.get_value() === value) {
					return (!chartingMetaData.get_color()) ? Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._defaultColor$p : chartingMetaData.get_color();
				}
			}
		}
		catch (e) {
			this._trace$p$0.executeLogWarning(Microsoft.Crm.Client.Core.Framework.TraceComponent.localDataSource, e.message);
		}
		return Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._defaultColor$p;
	},

	_fetchColorForLabel$p$0: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartConfigGenerator$_fetchColorForLabel$p$0(attribute, label) {
		if (_Script.isNullOrUndefined(label)) {
			return Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._defaultColor$p;
		}
		try {
			var attributeMetaData = this._getOptionSetMetadata$p$0(attribute);
			var $$dict_5 = attributeMetaData.getChartingMetaDataInDisplayOrder();
			for (var $$key_6 in $$dict_5) {
				var entry = { key: $$key_6, value: $$dict_5[$$key_6] };
				var chartingMetaData = entry.value;
				if (chartingMetaData.get_label() === label) {
					return (!chartingMetaData.get_color()) ? Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._defaultColor$p : chartingMetaData.get_color();
				}
			}
		}
		catch (e) {
			this._trace$p$0.executeLogWarning(Microsoft.Crm.Client.Core.Framework.TraceComponent.localDataSource, e.message);
		}
		return Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._defaultColor$p;
	},

	_getOptionSetMetadata$p$0: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartConfigGenerator$_getOptionSetMetadata$p$0(attribute) {
		var chartingAttributeMetaData = this._metaDataAggregator$p$0.getChartingAttributeMetadata(this._primaryModelName$p$0, attribute);
		if (_Script.isNullOrUndefined(chartingAttributeMetaData)) {
			return null;
		}
		return chartingAttributeMetaData;
	}
}


Microsoft.Crm.Client.Core.ViewModels.Controls.ChartDataField = function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartDataField(attributeExpression, primaryChartGroupBy) {
	this._attribute$p$0 = attributeExpression;
	this._primaryGroupBy$p$0 = primaryChartGroupBy;
	this._aliasName$p$0 = (!this._attribute$p$0.get_aliasName()) ? this._attribute$p$0.get_name() : this._attribute$p$0.get_aliasName();
	if (this._primaryGroupBy$p$0 && this._primaryGroupBy$p$0.get_groupAttribute() && this._primaryGroupBy$p$0.get_groupAttribute().get_dateTimeGrouping()) {
		this._isDateTimeChart$p$0 = true;
	}
}
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartDataField.prototype = {
	_rawValue$p$0: null,
	_attribute$p$0: null,
	_primaryGroupBy$p$0: null,
	_isDateTimeChart$p$0: false,
	_aliasName$p$0: null,

	getValue: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartDataField$getValue(model) {
		var returnValue = (this._isDateTimeChart$p$0) ? this.getDateTimeValue(model) : null;
		if (!returnValue || Object.getType(returnValue) !== String) {
			returnValue = (model).getIEntityRecord().getFormattedValue(this._aliasName$p$0);
			if (returnValue && Object.getType(returnValue) === Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AliasedValue) {
				returnValue = (returnValue).get_value();
			}
			if (returnValue && Object.getType(returnValue) === Xrm.Objects.EntityReference) {
				returnValue = (returnValue).get_displayName();
			}
			returnValue = returnValue || Microsoft.Crm.Client.Core.Framework._String.empty;
			if (Object.getType(returnValue) !== String) {
				returnValue = returnValue.toString();
			}
		}
		return (!(returnValue).length) ? Microsoft.Crm.Client.Core.Framework.Common.ResourceManager.getLocalizedString('Web.Visualization.EmptyAxisLabel') : returnValue;
	},

	getComparisonCode: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartDataField$getComparisonCode(model) {
		if (!this._attribute$p$0 || !this._attribute$p$0.get_entity().get_metadataPair() || _Script.isNullOrUndefined(this._attribute$p$0.get_entity().get_metadataPair().get_attributeMetadataCollection().get_attributesByName().get_item(this._attribute$p$0.get_name()))) {
			return this.getValue(model);
		}
		var type = this._attribute$p$0.get_entity().get_metadataPair().get_attributeMetadataCollection().get_attributesByName().get_item(this._attribute$p$0.get_name()).get_type();
		switch (type) {
			case Xrm.Objects.AttributeType.picklist:
			case Xrm.Objects.AttributeType.status:
			case Xrm.Objects.AttributeType.state:
			case 0:
				var returnValue = (model).getIEntityRecord().getValue(this._aliasName$p$0);
				if (returnValue && Object.getType(returnValue) === Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AliasedValue) {
					returnValue = (returnValue).get_value();
				}
				if (returnValue && Object.getType(returnValue).implementsInterface(Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.OptionMetadata)) {
					returnValue = (returnValue).get_value();
				}
				returnValue = returnValue || 0;
				return (returnValue).toString(2);
			default:
				return this.getValue(model);
		}
	},

	get_rawValue: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartDataField$get_rawValue() {
		return this._rawValue$p$0;
	},

	getDateTimeValue: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartDataField$getDateTimeValue(record) {
		var value = record.GetValue(this._aliasName$p$0);
		if (_Script.isNullOrUndefined(value) || value === Microsoft.Crm.Client.Core.Framework._String.empty) {
			return Microsoft.Crm.Client.Core.Framework._String.empty;
		}
		switch (this._attribute$p$0.get_dateTimeGrouping().get_groupingType()) {
			case Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.DateTimeGroupingInfo.day:
				return this.getXDayValue(record);
			case Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.DateTimeGroupingInfo.week:
				return this.getXWeekValue(record);
			case Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.DateTimeGroupingInfo.month:
				return this.getXMonthValue(record);
			case Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.DateTimeGroupingInfo.quarter:
				return this.getXQuarterValue(record);
			case Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.DateTimeGroupingInfo.fiscalPeriod:
				return this.getXFiscalPeriodValue(record);
			case Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.DateTimeGroupingInfo.year:
				return this.getXYearValue(record);
			case Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.DateTimeGroupingInfo.fiscalYear:
				return this.getXFiscalYearValue(record);
		}
		return record.GetValue(this._aliasName$p$0);
	},

	get_dateTimeFieldBehavior: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartDataField$get_dateTimeFieldBehavior() {
		var metadata = this._attribute$p$0.get_entity().get_metadataPair().get_attributeMetadataCollection().get_attributesByName().get_item(this._attribute$p$0.get_name());
		return metadata.get_behavior();
	},

	get_xAxisTitle: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartDataField$get_xAxisTitle() {
		return Microsoft.Crm.Client.Core.ViewModels.Controls._chartFormatter.formatAxisTitle(this._attribute$p$0);
	},

	getXYearValue: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartDataField$getXYearValue(record) {
		var year = record.GetValue(this._aliasName$p$0 + Microsoft.Crm.Client.Core.Framework.FieldFormat.raw);
		return Microsoft.Crm.Client.Core.ViewModels.Controls._chartFormatter.formatDate(year, -1, -1, -1);
	},

	getXMonthValue: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartDataField$getXMonthValue(record) {
		var month = record.GetValue(this._aliasName$p$0 + Microsoft.Crm.Client.Core.Framework.FieldFormat.raw);
		var year = record.GetValue(this._primaryGroupBy$p$0.get_extendedGroupBys().get_item(0).get_aliasName() + Microsoft.Crm.Client.Core.Framework.FieldFormat.raw);
		return Microsoft.Crm.Client.Core.ViewModels.Controls._chartFormatter.formatDate(year, month, -1, -1);
	},

	getXDayValue: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartDataField$getXDayValue(record) {
		var day = record.GetValue(this._aliasName$p$0 + Microsoft.Crm.Client.Core.Framework.FieldFormat.raw);
		var month = record.GetValue(this._primaryGroupBy$p$0.get_extendedGroupBys().get_item(0).get_aliasName() + Microsoft.Crm.Client.Core.Framework.FieldFormat.raw);
		var year = record.GetValue(this._primaryGroupBy$p$0.get_extendedGroupBys().get_item(1).get_aliasName() + Microsoft.Crm.Client.Core.Framework.FieldFormat.raw);
		return Microsoft.Crm.Client.Core.ViewModels.Controls._chartFormatter.formatDate(year, month, -1, day);
	},

	getXWeekValue: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartDataField$getXWeekValue(record) {
		var year = record.GetValue(this._primaryGroupBy$p$0.get_extendedGroupBys().get_item(0).get_aliasName() + Microsoft.Crm.Client.Core.Framework.FieldFormat.raw);
		var week = record.GetValue(this._aliasName$p$0 + Microsoft.Crm.Client.Core.Framework.FieldFormat.raw);
		return Microsoft.Crm.Client.Core.ViewModels.Controls._chartFormatter.formatDate(year, -1, week, -1);
	},

	getXFiscalYearValue: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartDataField$getXFiscalYearValue(record) {
		var year = record.GetValue(this._aliasName$p$0 + Microsoft.Crm.Client.Core.Framework.FieldFormat.raw);
		return Microsoft.Crm.Client.Core.ViewModels.Controls._chartFormatter.formatFiscal(year, -1);
	},

	getXFiscalPeriodValue: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartDataField$getXFiscalPeriodValue(record) {
		return record.GetValue(this._aliasName$p$0 + Microsoft.Crm.Client.Core.Framework.FieldFormat.raw);
	},

	getXQuarterValue: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartDataField$getXQuarterValue(record) {
		var year = record.GetValue(this._primaryGroupBy$p$0.get_extendedGroupBys().get_item(0).get_aliasName() + Microsoft.Crm.Client.Core.Framework.FieldFormat.raw);
		var quarter = record.GetValue(this._aliasName$p$0 + Microsoft.Crm.Client.Core.Framework.FieldFormat.raw);
		return Microsoft.Crm.Client.Core.ViewModels.Controls._chartFormatter.formatFiscal(year, quarter);
	}
}


Microsoft.Crm.Client.Core.ViewModels.Controls.ChartDataParser = function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartDataParser() {
}
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartDataParser.isInteractionCentricMultiEntityChartsFeatureSupported = function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartDataParser$isInteractionCentricMultiEntityChartsFeatureSupported(featureControlManager) {
	if (!featureControlManager) {
		return false;
	}
	return featureControlManager.isFeatureEnabled(Microsoft.Crm.Client.Core.Framework.FeatureName.interactionCentricMultiEntityChartsFeature);
}
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartDataParser._getRawFieldName$p = function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartDataParser$_getRawFieldName$p(attributeQueryExpression) {
	return ((Microsoft.Crm.Client.Core.Framework._String.isNullOrWhiteSpace(attributeQueryExpression.get_aliasName())) ? attributeQueryExpression.get_name() : attributeQueryExpression.get_aliasName()) + Microsoft.Crm.Client.Core.Framework.FieldFormat.raw;
}
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartDataParser.prototype = {
	_queryModel$p$0: null,
	_chartData$p$0: null,
	_categoryColumns$p$0: null,
	_chartPresentationSeries$p$0: null,
	_chartDataDescription$p$0: null,
	_isInitialized$p$0: false,
	_featureControlManager$p$0: null,

	get_isInitialized: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartDataParser$get_isInitialized() {
		return this._isInitialized$p$0;
	},

	set_isInitialized: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartDataParser$set_isInitialized(value) {
		this._isInitialized$p$0 = value;
		return value;
	},

	get_chartData: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartDataParser$get_chartData() {
		return this._chartData$p$0;
	},

	set_chartData: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartDataParser$set_chartData(value) {
		this._chartData$p$0 = value;
		return value;
	},

	get_featureControlManager: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartDataParser$get_featureControlManager() {
		return this._featureControlManager$p$0;
	},

	set_featureControlManager: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartDataParser$set_featureControlManager(value) {
		this._featureControlManager$p$0 = value;
		return value;
	},

	get_categoryColumn: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartDataParser$get_categoryColumn() {
		return this._categoryColumns$p$0;
	},

	set_categoryColumn: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartDataParser$set_categoryColumn(value) {
		this._categoryColumns$p$0 = value;
		return value;
	},

	get_chartPresentationSeries: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartDataParser$get_chartPresentationSeries() {
		return this._chartPresentationSeries$p$0;
	},

	set_chartPresentationSeries: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartDataParser$set_chartPresentationSeries(value) {
		this._chartPresentationSeries$p$0 = value;
		return value;
	},

	get_primaryGroupBy: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartDataParser$get_primaryGroupBy() {
		return this._chartDataDescription$p$0.get_category().get_primaryGroupBy();
	},

	get_chartDataDescription: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartDataParser$get_chartDataDescription() {
		return this._chartDataDescription$p$0;
	},

	get_queryModel: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartDataParser$get_queryModel() {
		return this._queryModel$p$0;
	},

	initialize: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartDataParser$initialize(chartDataRecord, dataDefinition, presentationSeriesList, chartQueryModel) {
		this._chartData$p$0 = chartDataRecord;
		this._chartDataDescription$p$0 = dataDefinition;
		this._categoryColumns$p$0 = this._chartDataDescription$p$0.get_categoryColumns();
		this._chartPresentationSeries$p$0 = presentationSeriesList;
		this._queryModel$p$0 = chartQueryModel;
		this._isInitialized$p$0 = true;
	},

	retrieveChartYAxis: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartDataParser$retrieveChartYAxis() {
		var chartYaxis = new Array(1);
		chartYaxis[0] = new Microsoft.Crm.Client.Core.Models.YAxis();
		chartYaxis[0].Title = (this._queryModel$p$0.get_SeriesList() && this._queryModel$p$0.get_SeriesList().length === 1) ? this._queryModel$p$0.get_SeriesList()[0].Title : Microsoft.Crm.Client.Core.Framework._String.empty;
		return chartYaxis;
	},

	retrieveChartCategory: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartDataParser$retrieveChartCategory() {
		var chartCategory = new Array(1);
		var field = null;
		if (this._categoryColumns$p$0.get_Count() > 0) {
			field = new Microsoft.Crm.Client.Core.ViewModels.Controls.ChartDataField(this._categoryColumns$p$0.get_item(0), this._chartDataDescription$p$0.get_category().get_primaryGroupBy());
		}
		chartCategory[0] = new Microsoft.Crm.Client.Core.Models.XAxis();
		var xValueData = new Array(this._chartData$p$0.get_count());
		for (var xValueIndex = 0; xValueIndex < this._chartData$p$0.get_count() ; xValueIndex++) {
			var record = this._chartData$p$0.get_itemsAsList().get_item(xValueIndex);
			if (!field) {
				xValueData[xValueIndex] = Microsoft.Crm.Client.Core.ViewModels.Controls._chartFormatter.getAllDataLocalizedString();
			}
			else {
				xValueData[xValueIndex] = field.getValue(record);
				if (xValueData[xValueIndex] === '-1') {
					xValueData[xValueIndex] = Microsoft.Crm.Client.Core.Framework.Common.ResourceManager.getLocalizedString('ChartAttributeValueMissing');
				}
			}
		}
		if (field) {
			chartCategory[0].Title = field.get_xAxisTitle();
		}
		chartCategory[0].Values = xValueData;
		return chartCategory;
	},

	retrieveChartSeries: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartDataParser$retrieveChartSeries(xAxisIndex) {
		var chartSeries = new Array(this._chartPresentationSeries$p$0.get_Count());
		for (var seriesIndex = 0; seriesIndex < this._chartPresentationSeries$p$0.get_Count() ; seriesIndex++) {
			var measureAttribute = this._chartDataDescription$p$0.get_category().get_measureCollections().get_item(seriesIndex).get_measures().get_item(0).get_measureAttribute();
			var seriesName = (!measureAttribute.get_aliasName()) ? measureAttribute.get_name() : measureAttribute.get_aliasName();
			var showValueAsLabel = this._chartPresentationSeries$p$0.get_item(seriesIndex).get_isValueShownAsLabel();
			var fieldAggregators = null;
			var seriesDisplayName = Microsoft.Crm.Client.Core.ViewModels.Controls._chartFormatter.formatAxisTitle(measureAttribute);
			this._queryModel$p$0.set_enableDrilldown(true);
			chartSeries[seriesIndex] = this.cloneBlueprintSeriesStyle(this._chartPresentationSeries$p$0.get_item(seriesIndex));
			if (measureAttribute.get_hasAggregate() && measureAttribute.get_entity() && measureAttribute.get_entity().get_groupByAttributes() && measureAttribute.get_entity().get_groupByAttributes().length > 0) {
				fieldAggregators = new Array(measureAttribute.get_entity().get_groupByAttributes().length);
				for (var aggregatorIndex = 0; aggregatorIndex < measureAttribute.get_entity().get_groupByAttributes().length; aggregatorIndex++) {
					fieldAggregators[aggregatorIndex] = measureAttribute.get_entity().get_groupByAttributes()[aggregatorIndex];
				}
			}
			else {
				if (this.get_primaryGroupBy() && this.get_primaryGroupBy().get_groupAttribute()) {
					fieldAggregators = new Array(0);
					if (!Microsoft.Crm.Client.Core.Framework._String.isNullOrWhiteSpace(this.get_primaryGroupBy().get_groupAttribute().get_name()) || !Microsoft.Crm.Client.Core.Framework._String.isNullOrWhiteSpace(this.get_primaryGroupBy().get_groupAttribute().get_aliasName())) {
						if (!Microsoft.Crm.Client.Core.ViewModels.Controls.ChartDataParser.isInteractionCentricMultiEntityChartsFeatureSupported(this._featureControlManager$p$0)) {
							if (!this.get_primaryGroupBy().get_groupAttribute().get_entity().get_parentEntity()) {
								fieldAggregators[0] = this.get_primaryGroupBy().get_groupAttribute();
							}
							else {
								this._queryModel$p$0.set_enableDrilldown(false);
							}
						}
						else {
							fieldAggregators[0] = this.get_primaryGroupBy().get_groupAttribute();
						}
					}
				}
			}
			var dp = new Array(this._chartData$p$0.get_count());
			var fieldRawName = seriesName + Microsoft.Crm.Client.Core.Framework.FieldFormat.raw;
			var fieldFormattedName = seriesName;
			for (var dataIndex = 0; dataIndex < this._chartData$p$0.get_count() ; dataIndex++) {
				var record = this._chartData$p$0.get_itemsAsList().get_item(dataIndex);
				dp[dataIndex] = new Microsoft.Crm.Client.Core.Models.DataPoint();
				var dataPointValue = record.GetValue(fieldRawName);
				dp[dataIndex].Value = (!dataPointValue) ? 0 : dataPointValue;
				dp[dataIndex].FormattedValue = (record).getIEntityRecord().getFormattedValue(fieldFormattedName) || 0;
				this.attachAggregators(dp[dataIndex], record, fieldAggregators);
			}
			if (showValueAsLabel) {
				var dataLabel = new Microsoft.Crm.Client.Core.Models.DataLabels();
				dataLabel.Enabled = true;
				var $$t_G = this;
				dataLabel.LabelFormatter = function () {
					return Microsoft.Crm.Client.Core.Framework._String.empty + this.point.FormattedValue;
				};
				chartSeries[seriesIndex].dataLabels = dataLabel;
			}
			chartSeries[seriesIndex].DataPoints = dp;
			chartSeries[seriesIndex].Title = this._chartPresentationSeries$p$0.get_item(seriesIndex).get_name() || seriesDisplayName;
			chartSeries[seriesIndex].XAxisNumber = xAxisIndex;
		}
		return chartSeries;
	},

	cloneBlueprintSeriesStyle: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartDataParser$cloneBlueprintSeriesStyle(blueprintSeries) {
		var chartSeries = new Microsoft.Crm.Client.Core.Models.Series();
		chartSeries.ChartType = blueprintSeries.get_chartType();
		chartSeries.Color = blueprintSeries.get_color();
		chartSeries.BorderColor = blueprintSeries.get_borderColor();
		chartSeries.BorderWidth = blueprintSeries.get_borderWidth();
		chartSeries.CustomProperties = blueprintSeries.get_customProperties();
		return chartSeries;
	},

	attachAggregators: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartDataParser$attachAggregators(dataPoint, record, fieldAggregators) {
		if (fieldAggregators) {
			var aggregationContainer = new Microsoft.Crm.Client.Core.Models._chartAggregatorsContainer();
			for (var aggregatorIndex = 0; aggregatorIndex < fieldAggregators.length; aggregatorIndex++) {
				var fieldAggregator = fieldAggregators[aggregatorIndex];
				aggregationContainer.addAggregator(fieldAggregator.get_name(), record.GetValue(Microsoft.Crm.Client.Core.ViewModels.Controls.ChartDataParser._getRawFieldName$p(fieldAggregator)), (fieldAggregator.get_dateTimeGrouping()) ? fieldAggregator.get_dateTimeGrouping().get_groupingType() : -1);
			}
			dataPoint.Aggregators = aggregationContainer.getAggregators();
		}
	}
}


Microsoft.Crm.Client.Core.ViewModels.Controls.ChartDataParserFactory = function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartDataParserFactory() {
}
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartDataParserFactory.obtainParser = function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartDataParserFactory$obtainParser(chartType) {
	var chartDataParser = null;
	switch (chartType) {
		case Microsoft.Crm.Client.Core.ViewModels.Controls.ChartType.comparisonChart:
			chartDataParser = new Microsoft.Crm.Client.Core.ViewModels.Controls.ComparisonChartDataParser();
			break;
		case Microsoft.Crm.Client.Core.ViewModels.Controls.ChartType.dateTimeChart:
			chartDataParser = new Microsoft.Crm.Client.Core.ViewModels.Controls.DateTimeChartDataParser();
			break;
		case Microsoft.Crm.Client.Core.ViewModels.Controls.ChartType.normalChart:
		default:
			chartDataParser = new Microsoft.Crm.Client.Core.ViewModels.Controls.NormalChartDataParser();
			break;
	}
	chartDataParser.set_featureControlManager(null);
	return chartDataParser;
}


Microsoft.Crm.Client.Core.ViewModels.Controls._chartFormatter = function Microsoft_Crm_Client_Core_ViewModels_Controls__chartFormatter() {
}
Microsoft.Crm.Client.Core.ViewModels.Controls._chartFormatter.get_monthNames = function Microsoft_Crm_Client_Core_ViewModels_Controls__chartFormatter$get_monthNames() {
	if (!Microsoft.Crm.Client.Core.ViewModels.Controls._chartFormatter._monthNames$p) {
		Microsoft.Crm.Client.Core.ViewModels.Controls._chartFormatter._monthNames$p = Microsoft.Crm.Client.Core.Framework.Common.ResourceManager.getLocalizedString('Calendar_Short_Months').split(',');
		Microsoft.Crm.Client.Core.Framework.Debug.assert(Microsoft.Crm.Client.Core.ViewModels.Controls._chartFormatter._monthNames$p.length === 12, 'Expected 12 months! Actual:' + Microsoft.Crm.Client.Core.ViewModels.Controls._chartFormatter._monthNames$p.length);
	}
	return Microsoft.Crm.Client.Core.ViewModels.Controls._chartFormatter._monthNames$p;
}
Microsoft.Crm.Client.Core.ViewModels.Controls._chartFormatter.formatDate = function Microsoft_Crm_Client_Core_ViewModels_Controls__chartFormatter$formatDate(year, month, week, day) {
	var bDay = day >= 1 && week < 1 && month >= 1;
	var bWeek = day < 1 && week >= 1 && month < 1;
	var bMonth = day < 1 && week < 1 && month >= 1;
	var bYear = week < 1 && month < 1;
	if (bDay) {
		var d = new Date(year, month - 1, day);
		return Microsoft.Crm.Client.Core.Storage.Common.CrmFormatter.get_instance().formatShortDateValue(d, 0);
	}
	if (bWeek) {
		return String.format(Microsoft.Crm.Client.Core.Framework.Common.ResourceManager.getLocalizedString('Web.Visualization.Year.Week'), week, year);
	}
	if (bMonth) {
		return String.format('{0} {1}', Microsoft.Crm.Client.Core.ViewModels.Controls._chartFormatter.get_monthNames()[month - 1], year);
	}
	if (bYear) {
		return year.toString();
	}
	return Microsoft.Crm.Client.Core.Framework._String.empty;
}
Microsoft.Crm.Client.Core.ViewModels.Controls._chartFormatter.formatFiscal = function Microsoft_Crm_Client_Core_ViewModels_Controls__chartFormatter$formatFiscal(year, quarter) {
	if (quarter > 0) {
		return String.format(Microsoft.Crm.Client.Core.Framework.Common.ResourceManager.getLocalizedString('Web.Visualization.Year.Quarter'), quarter, year);
	}
	else {
		return String.format(Microsoft.Crm.Client.Core.Framework.Common.ResourceManager.getLocalizedString('FiscalYear_Format_String_NoSpace'), year);
	}
}
Microsoft.Crm.Client.Core.ViewModels.Controls._chartFormatter.formatAxisTitle = function Microsoft_Crm_Client_Core_ViewModels_Controls__chartFormatter$formatAxisTitle(groupAttribute) {
	var attributeMetadata = groupAttribute.get_entity().get_metadataPair().get_attributeMetadataCollection().get_attributesByName().get_item(groupAttribute.get_name());
	var displayName = (!_Script.isNullOrUndefined(attributeMetadata)) ? attributeMetadata.get_displayName() : null;
	if (!displayName) {
		displayName = groupAttribute.get_name();
	}
	var measureTitle = displayName;
	if (groupAttribute.get_dateTimeGrouping()) {
		var dateGroup = Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.DateTimeGroupingInfo.fromGroupingTypeToName(groupAttribute.get_dateTimeGrouping().get_groupingType()).replace('-', Microsoft.Crm.Client.Core.Framework._String.empty).toUpperCase();
		measureTitle = String.format(Microsoft.Crm.Client.Core.Framework.Common.ResourceManager.getLocalizedString('Web.Visualization.AxisTitle.DateGrouping'), Microsoft.Crm.Client.Core.Framework.Common.ResourceManager.getLocalizedString('Web.Visualization.AxisTitle.DateGrouping.' + dateGroup.toUpperCase()), displayName);
	}
	else {
		if (groupAttribute.get_hasAggregate() && !Microsoft.Crm.Client.Core.Framework._String.isNullOrEmpty(groupAttribute.get_aggregateType())) {
			measureTitle = String.format(Microsoft.Crm.Client.Core.Framework.Common.ResourceManager.getLocalizedString('Web.Visualization.AxisTitle'), Microsoft.Crm.Client.Core.Framework.Common.ResourceManager.getLocalizedString('Web.Visualization.AxisTitle.' + groupAttribute.get_aggregateType().toUpperCase()), displayName);
		}
	}
	if (attributeMetadata && attributeMetadata.get_type() === Xrm.Objects.AttributeType.money) {
		var chartUserContext = Microsoft.Crm.Client.DataSourceFactory.get_instance().getChartingDataSource();
		var currencySymbol = chartUserContext.getAttributeTransactionCurrencySymbol(attributeMetadata, groupAttribute.get_hasAggregate());
		if (!Microsoft.Crm.Client.Core.Framework._String.isNullOrEmpty(currencySymbol)) {
			measureTitle = String.format(Microsoft.Crm.Client.Core.Framework.Common.ResourceManager.getLocalizedString('Web.Visualization.AxisTitle.CurrencySymbol'), measureTitle, currencySymbol);
		}
	}
	return measureTitle;
}
Microsoft.Crm.Client.Core.ViewModels.Controls._chartFormatter.getAllDataLocalizedString = function Microsoft_Crm_Client_Core_ViewModels_Controls__chartFormatter$getAllDataLocalizedString() {
	return Microsoft.Crm.Client.Core.Framework.Common.ResourceManager.getLocalizedString('Web.Visualization.EmptyAxisLabel');
}


Microsoft.Crm.Client.Core.ViewModels.Controls.ChartPresentationParser = function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartPresentationParser(presentationXml) {
	this._presentationXMLDoc$p$0 = Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlNodeFactory.parseXmlDocument(presentationXml);
	this._validateDataPresentation$p$0(this._presentationXMLDoc$p$0);
	this._chartPresentationSeries$p$0 = this._retriveChartPresentationSeries$p$0();
	this._chartPresentationLegendList$p$0 = this._retrieveLegendList$p$0();
	this._colorsList$p$0 = this._retrievePaletteCustomColors$p$0();
}
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartPresentationParser.prototype = {
	_presentationXMLDoc$p$0: null,
	_chartPresentationSeries$p$0: null,
	_chartPresentationLegendList$p$0: null,
	_colorsList$p$0: null,

	get_chartPresentationSeries: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartPresentationParser$get_chartPresentationSeries() {
		return this._chartPresentationSeries$p$0;
	},

	get_chartPresentationLegendList: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartPresentationParser$get_chartPresentationLegendList() {
		return this._chartPresentationLegendList$p$0;
	},

	get_colorsList: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartPresentationParser$get_colorsList() {
		return this._colorsList$p$0;
	},

	_retriveChartPresentationSeries$p$0: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartPresentationParser$_retriveChartPresentationSeries$p$0() {
		var seriesList = new (Microsoft.Crm.Client.Core.Framework.List$1.$$(Microsoft.Crm.Client.Core.ViewModels.Controls.PresentationSeries))();
		var presentationXMLNodes = Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlNodeFactory.create(this._presentationXMLDoc$p$0).selectNodes('Chart/Series/Series');
		for (var presentationSeriesIndex = 0; presentationSeriesIndex < presentationXMLNodes.get_count() ; presentationSeriesIndex++) {
			var seriesXmlNode = presentationXMLNodes.get_item(presentationSeriesIndex);
			var presentationSeries = new Microsoft.Crm.Client.Core.ViewModels.Controls.PresentationSeries();
			presentationSeries.set_chartType(seriesXmlNode.getAttribute('ChartType'));
			presentationSeries.set_color(seriesXmlNode.getAttribute('Color'));
			presentationSeries.set_name(seriesXmlNode.getAttribute('Name'));
			presentationSeries.set_customProperties(seriesXmlNode.getAttribute('CustomProperties'));
			presentationSeries.set_borderColor(seriesXmlNode.getAttribute('BorderColor'));
			presentationSeries.set_borderWidth(seriesXmlNode.getAttribute('BorderWidth'));
			presentationSeries.set_isValueShownAsLabel(seriesXmlNode.getAttribute('IsValueShownAsLabel') === 'True');
			seriesList.add(presentationSeries);
		}
		return seriesList;
	},

	_retrieveLegendList$p$0: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartPresentationParser$_retrieveLegendList$p$0() {
		var presentationLegendList = new (Microsoft.Crm.Client.Core.Framework.List$1.$$(Microsoft.Crm.Client.Core.ViewModels.Controls.PresentationLegend))();
		var legendXmlNodes = Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlNodeFactory.create(this._presentationXMLDoc$p$0).selectNodes('Chart/Legends/Legend');
		for (var legendIndex = 0; legendIndex < legendXmlNodes.get_count() ; legendIndex++) {
			var legendXmlNode = legendXmlNodes.get_item(legendIndex);
			var presentationLegend = new Microsoft.Crm.Client.Core.ViewModels.Controls.PresentationLegend();
			presentationLegend.set_alignment(legendXmlNode.getAttribute('Alignment'));
			presentationLegend.set_docking(legendXmlNode.getAttribute('Docking'));
			presentationLegend.set_foreColor(legendXmlNode.getAttribute('ForeColor'));
			var legendEnabled = legendXmlNode.getAttribute('Enabled');
			presentationLegend.set_enabled(!legendEnabled || legendEnabled);
			presentationLegendList.add(presentationLegend);
		}
		return presentationLegendList;
	},

	_retrievePaletteCustomColors$p$0: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartPresentationParser$_retrievePaletteCustomColors$p$0() {
		var colors = [];
		var chartNode = Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlNodeFactory.create(this._presentationXMLDoc$p$0).selectSingleNode('Chart');
		var paletteCustomColors = chartNode.getAttribute('PaletteCustomColors');
		if (paletteCustomColors) {
			for (var $$arr_3 = paletteCustomColors.split(';'), $$len_4 = $$arr_3.length, $$idx_5 = 0; $$idx_5 < $$len_4; ++$$idx_5) {
				var paletteCustomColor = $$arr_3[$$idx_5];
				Array.add(colors, String.format(Microsoft.Crm.Client.Core.ViewModels.Controls.ChartPresentationParser.colorStringFormat, paletteCustomColor.trim()));
			}
		}
		return colors;
	},

	_validateDataPresentation$p$0: function Microsoft_Crm_Client_Core_ViewModels_Controls_ChartPresentationParser$_validateDataPresentation$p$0(xmlDocument) {
		var errorNode = Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlNodeFactory.create(xmlDocument).selectSingleNode('Error');
		if (!errorNode) {
			return;
		}
		throw new Microsoft.Crm.Client.Core.Framework.ChartError(Microsoft.Crm.Client.Core.Framework.Common.ResourceManager.getLocalizedString('Chart_Unsupported_Title'), Microsoft.Crm.Client.Core.Framework.Common.ResourceManager.getLocalizedString('Chart_Unsupported_Message')).toException();
	}
}


Microsoft.Crm.Client.Core.ViewModels.Controls.ComparisonChartDataParser = function Microsoft_Crm_Client_Core_ViewModels_Controls_ComparisonChartDataParser() {
	Microsoft.Crm.Client.Core.ViewModels.Controls.ComparisonChartDataParser.initializeBase(this);
}
Microsoft.Crm.Client.Core.ViewModels.Controls.ComparisonChartDataParser.prototype = {

	parse: function Microsoft_Crm_Client_Core_ViewModels_Controls_ComparisonChartDataParser$parse() {
		var perfRetrieveData = new Microsoft.Crm.Client.Core.Framework.PerformanceStopwatch('ComparisonChartDataParser:Parse');
		perfRetrieveData.start();
		try {
			var fieldAggregators = new Array(0);
			var comparisonCodeToName = {};
			Microsoft.Crm.Client.Core.Framework.Debug.assert(this.get_isInitialized(), 'The instance of the ComparisonChartDataParser class is not properly initialized.');
			var category = this.get_chartDataDescription().get_category();
			var measureCollection = category.get_measureCollections().get_item(0);
			Microsoft.Crm.Client.Core.Framework.Debug.assert(measureCollection.get_secondaryGroupBys().get_Count() > 0, 'Comparison chart can only be plotted with secondary groups\'s');
			if (measureCollection.get_measures() && measureCollection.get_measures().get_Count() > 0 && measureCollection.get_measures().get_item(0).get_parentCategory() && measureCollection.get_measures().get_item(0).get_parentCategory().get_primaryGroupBy()) {
				if (measureCollection.get_measures().get_item(0).get_parentCategory().get_primaryGroupBy().get_groupAttribute()) {
					fieldAggregators[fieldAggregators.length] = measureCollection.get_measures().get_item(0).get_parentCategory().get_primaryGroupBy().get_groupAttribute();
				}
				if (measureCollection.get_measures().get_item(0).get_parentCategory().get_primaryGroupBy().get_extendedGroupBys()) {
					for (var i = 0; i < measureCollection.get_measures().get_item(0).get_parentCategory().get_primaryGroupBy().get_extendedGroupBys().get_Count() ; i++) {
						fieldAggregators[fieldAggregators.length] = measureCollection.get_measures().get_item(0).get_parentCategory().get_primaryGroupBy().get_extendedGroupBys().get_item(i);
					}
				}
			}
			Microsoft.Crm.Client.Core.Framework.Debug.assert(this.get_chartPresentationSeries().get_Count() === 1, 'Comparison chart should have only one series');
			var secondaryAttribute = measureCollection.get_secondaryGroupBys().get_item(0).get_groupAttribute();
			var secondaryDataField = new Microsoft.Crm.Client.Core.ViewModels.Controls.ChartDataField(secondaryAttribute, measureCollection.get_secondaryGroupBys().get_item(0));
			if (secondaryAttribute) {
				fieldAggregators[fieldAggregators.length] = secondaryAttribute;
			}
			var measureAttribute = measureCollection.get_measures().get_item(0).get_measureAttribute();
			var measureName = measureAttribute.get_aliasName() || measureAttribute.get_name();
			var fieldRawName = measureName + Microsoft.Crm.Client.Core.Framework.FieldFormat.raw;
			var fieldFormattedName = measureName;
			var primaryAttribute = category.get_primaryGroupBy().get_groupAttribute();
			var primaryDataField = new Microsoft.Crm.Client.Core.ViewModels.Controls.ChartDataField(primaryAttribute, category.get_primaryGroupBy());
			var comparisonChartSeriesCodes = new (Microsoft.Crm.Client.Core.Framework.List$1.$$(String))();
			var xChartValues = new (Microsoft.Crm.Client.Core.Framework.List$1.$$(String))();
			var chartSeriesValues = {};
			for (var dataIndex = 0; dataIndex < this.get_chartData().get_count() ; dataIndex++) {
				var record = this.get_chartData().get_itemsAsList().get_item(dataIndex);
				var xValue = primaryDataField.getValue(record);
				var seriesName = secondaryDataField.getValue(record);
				var seriesCode;
				if (!seriesName) {
					seriesName = Microsoft.Crm.Client.Core.ViewModels.Controls._chartFormatter.getAllDataLocalizedString();
					seriesCode = seriesName;
				}
				else {
					seriesCode = secondaryDataField.getComparisonCode(record);
				}
				comparisonCodeToName[seriesCode] = seriesName;
				if (!xChartValues.contains(xValue)) {
					xChartValues.add(xValue);
				}
				if (!comparisonChartSeriesCodes.contains(seriesCode)) {
					comparisonChartSeriesCodes.add(seriesCode);
				}
			}
			for (var $$arr_M = comparisonChartSeriesCodes.get_Items(), $$len_N = $$arr_M.length, $$idx_O = 0; $$idx_O < $$len_N; ++$$idx_O) {
				var series = $$arr_M[$$idx_O];
				var dp = new Array(xChartValues.get_Count());
				chartSeriesValues[series] = dp;
			}
			for (var dataIndex = 0; dataIndex < this.get_chartData().get_count() ; dataIndex++) {
				var record = this.get_chartData().get_itemsAsList().get_item(dataIndex);
				var thisSeriesCode = secondaryDataField.getComparisonCode(record);
				if (!thisSeriesCode) {
					thisSeriesCode = Microsoft.Crm.Client.Core.ViewModels.Controls._chartFormatter.getAllDataLocalizedString();
				}
				var dp = chartSeriesValues[thisSeriesCode];
				var xValue = primaryDataField.getValue(record);
				var dpIndex = xChartValues.indexOf(xValue);
				dp[dpIndex] = new Microsoft.Crm.Client.Core.Models.DataPoint();
				dp[dpIndex].Value = record.GetValue(fieldRawName);
				dp[dpIndex].FormattedValue = (record).getIEntityRecord().getFormattedValue(fieldFormattedName);
				this.attachAggregators(dp[dpIndex], record, fieldAggregators);
			}
			for (var $$arr_X = comparisonChartSeriesCodes.get_Items(), $$len_Y = $$arr_X.length, $$idx_Z = 0; $$idx_Z < $$len_Y; ++$$idx_Z) {
				var series = $$arr_X[$$idx_Z];
				var dp = chartSeriesValues[series];
				for (var dataPointIndex = 0; dataPointIndex < xChartValues.get_Count() ; dataPointIndex++) {
					if (_Script.isNullOrUndefined(dp[dataPointIndex])) {
						dp[dataPointIndex] = new Microsoft.Crm.Client.Core.Models.DataPoint();
					}
				}
			}
			var xComparisonChartAxis = new Array(1);
			xComparisonChartAxis[0] = new Microsoft.Crm.Client.Core.Models.XAxis();
			xComparisonChartAxis[0].Title = primaryDataField.get_xAxisTitle();
			xComparisonChartAxis[0].Values = xChartValues.toArray();
			if (measureCollection.get_secondaryGroupBys().get_item(0).get_descending()) {
				var $$t_o = this;
				comparisonChartSeriesCodes.sort(function (a, b) {
					return (a).localeCompare(b);
				});
			}
			else {
				var $$t_p = this;
				comparisonChartSeriesCodes.sort(function (a, b) {
					return (b).localeCompare(a);
				});
			}
			var seriesComparisonChart = new Array(comparisonChartSeriesCodes.get_Count());
			var revertColor = !!this.get_queryModel().get_Colors() && this.get_queryModel().get_Colors().length > 0;
			var color = (revertColor) ? this.get_queryModel().get_Colors()[0] : null;
			for (var seriesIndex = 0; seriesIndex < comparisonChartSeriesCodes.get_Count() ; seriesIndex++) {
				var seriesUniqueCode = comparisonChartSeriesCodes.get_Items()[seriesIndex];
				seriesComparisonChart[seriesIndex] = this.cloneBlueprintSeriesStyle(this.get_chartPresentationSeries().get_item(0));
				seriesComparisonChart[seriesIndex].Title = comparisonCodeToName[seriesUniqueCode];
				seriesComparisonChart[seriesIndex].DataPoints = chartSeriesValues[seriesUniqueCode];
				if (revertColor) {
					var colorIndex = seriesIndex % this.get_queryModel().get_Colors().length;
					if (seriesIndex === comparisonChartSeriesCodes.get_Count() - 1) {
						this.get_queryModel().get_Colors()[colorIndex] = color;
					}
					else {
						this.get_queryModel().get_Colors()[colorIndex] = this.get_queryModel().get_Colors()[(comparisonChartSeriesCodes.get_Count() - seriesIndex - 1) % this.get_queryModel().get_Colors().length];
					}
				}
			}
			this.get_queryModel().set_XAxes(xComparisonChartAxis);
			this.get_queryModel().set_YAxes(this._retrieveChartYAxis$p$1(measureAttribute));
			this.get_queryModel().set_SeriesList(seriesComparisonChart);
		}
		finally {
			perfRetrieveData.stop();
			perfRetrieveData = null;
		}
	},

	_retrieveChartYAxis$p$1: function Microsoft_Crm_Client_Core_ViewModels_Controls_ComparisonChartDataParser$_retrieveChartYAxis$p$1(measureAttribute) {
		var yComparisonChartAxis = new Array(1);
		yComparisonChartAxis[0] = new Microsoft.Crm.Client.Core.Models.YAxis();
		yComparisonChartAxis[0].Title = Microsoft.Crm.Client.Core.ViewModels.Controls._chartFormatter.formatAxisTitle(measureAttribute);
		return yComparisonChartAxis;
	}
}


Microsoft.Crm.Client.Core.ViewModels.Controls.DateTimeChartDataParser = function Microsoft_Crm_Client_Core_ViewModels_Controls_DateTimeChartDataParser() {
	Microsoft.Crm.Client.Core.ViewModels.Controls.DateTimeChartDataParser.initializeBase(this);
}
Microsoft.Crm.Client.Core.ViewModels.Controls.DateTimeChartDataParser.prototype = {

	parse: function Microsoft_Crm_Client_Core_ViewModels_Controls_DateTimeChartDataParser$parse() {
		if (!this.get_categoryColumn() || !this.get_categoryColumn().get_Count()) {
			return;
		}
		this.get_queryModel().set_XAxes(this.retrieveChartCategory());
		this.get_queryModel().set_SeriesList(this.retrieveChartSeries(0));
		this.get_queryModel().set_YAxes(this.retrieveChartYAxis());
	},

	retrieveChartCategory: function Microsoft_Crm_Client_Core_ViewModels_Controls_DateTimeChartDataParser$retrieveChartCategory() {
		var chartCategory = new Array(1);
		if (this.get_chartData().get_count() < 1) {
			return chartCategory;
		}
		var groupByAttribute = this.get_primaryGroupBy().get_groupAttribute();
		chartCategory[0] = new Microsoft.Crm.Client.Core.Models.XAxis();
		var xValueData = new Array(this.get_chartData().get_count());
		var dataField = new Microsoft.Crm.Client.Core.ViewModels.Controls.ChartDataField(groupByAttribute, this.get_primaryGroupBy());
		chartCategory[0].Title = dataField.get_xAxisTitle();
		for (var xValueIndex = 0; xValueIndex < this.get_chartData().get_count() ; xValueIndex++) {
			var record = this.get_chartData().get_itemsAsList().get_item(xValueIndex);
			xValueData[xValueIndex] = dataField.getValue(record);
		}
		chartCategory[0].Values = xValueData;
		return chartCategory;
	}
}


Microsoft.Crm.Client.Core.ViewModels.Controls.HighchartStubFactory = function Microsoft_Crm_Client_Core_ViewModels_Controls_HighchartStubFactory() {
}
Microsoft.Crm.Client.Core.ViewModels.Controls.HighchartStubFactory.createHighchart = function Microsoft_Crm_Client_Core_ViewModels_Controls_HighchartStubFactory$createHighchart() {
	var highchart = {};
	highchart.marginTop = 60;
	highchart.marginBottom = 78;
	highchart.spacingBottom = 13;
	highchart.spacingLeft = 0;
	highchart.marginRight = 0;
	highchart.spacingTop = 13;
	return highchart;
}
Microsoft.Crm.Client.Core.ViewModels.Controls.HighchartStubFactory.createHighchartEvents = function Microsoft_Crm_Client_Core_ViewModels_Controls_HighchartStubFactory$createHighchartEvents() {
	return {};
}
Microsoft.Crm.Client.Core.ViewModels.Controls.HighchartStubFactory.createHighchartSeries = function Microsoft_Crm_Client_Core_ViewModels_Controls_HighchartStubFactory$createHighchartSeries() {
	var highchartSeries = {};
	highchartSeries.borderRadius = 0;
	highchartSeries.shadow = false;
	return highchartSeries;
}
Microsoft.Crm.Client.Core.ViewModels.Controls.HighchartStubFactory.createHighchartSeriesData = function Microsoft_Crm_Client_Core_ViewModels_Controls_HighchartStubFactory$createHighchartSeriesData() {
	return {};
}
Microsoft.Crm.Client.Core.ViewModels.Controls.HighchartStubFactory.createHighchartSeriesArray = function Microsoft_Crm_Client_Core_ViewModels_Controls_HighchartStubFactory$createHighchartSeriesArray() {
	return [];
}
Microsoft.Crm.Client.Core.ViewModels.Controls.HighchartStubFactory.createHighchartSeriesState = function Microsoft_Crm_Client_Core_ViewModels_Controls_HighchartStubFactory$createHighchartSeriesState() {
	return {};
}
Microsoft.Crm.Client.Core.ViewModels.Controls.HighchartStubFactory.createHighchartSeriesSelectState = function Microsoft_Crm_Client_Core_ViewModels_Controls_HighchartStubFactory$createHighchartSeriesSelectState() {
	var state = Microsoft.Crm.Client.Core.ViewModels.Controls.HighchartStubFactory.createHighchartSeriesState();
	state.borderColor = 'black';
	state.borderWidth = 3;
	state.color = null;
	return state;
}
Microsoft.Crm.Client.Core.ViewModels.Controls.HighchartStubFactory.createHighchartPlotOptions = function Microsoft_Crm_Client_Core_ViewModels_Controls_HighchartStubFactory$createHighchartPlotOptions() {
	return {};
}
Microsoft.Crm.Client.Core.ViewModels.Controls.HighchartStubFactory.createHighchartSeriesAnimation = function Microsoft_Crm_Client_Core_ViewModels_Controls_HighchartStubFactory$createHighchartSeriesAnimation() {
	return {};
}


Microsoft.Crm.Client.Core.ViewModels.Controls.NormalChartDataParser = function Microsoft_Crm_Client_Core_ViewModels_Controls_NormalChartDataParser() {
	Microsoft.Crm.Client.Core.ViewModels.Controls.NormalChartDataParser.initializeBase(this);
}
Microsoft.Crm.Client.Core.ViewModels.Controls.NormalChartDataParser.prototype = {

	parse: function Microsoft_Crm_Client_Core_ViewModels_Controls_NormalChartDataParser$parse() {
		Microsoft.Crm.Client.Core.Framework.Debug.assert(this.get_isInitialized(), 'The instance of the NormalChartDataParser class is not properly initialized.');
		if (!this.get_categoryColumn() || !this.get_categoryColumn().get_Count()) {
			return;
		}
		this.get_queryModel().set_XAxes(this.retrieveChartCategory());
		this.get_queryModel().set_SeriesList(this.retrieveChartSeries(0));
		this.get_queryModel().set_YAxes(this.retrieveChartYAxis());
	}
}


Microsoft.Crm.Client.Core.ViewModels.Controls.PresentationSeries = function Microsoft_Crm_Client_Core_ViewModels_Controls_PresentationSeries() {
}
Microsoft.Crm.Client.Core.ViewModels.Controls.PresentationSeries.prototype = {
	_chartType$p$0: null,
	_color$p$0: null,
	_name$p$0: null,
	_customProperties$p$0: null,
	_borderWidth$p$0: 0,
	_borderColor$p$0: null,
	_isValueShownAsLabel$p$0: false,

	get_chartType: function Microsoft_Crm_Client_Core_ViewModels_Controls_PresentationSeries$get_chartType() {
		return this._chartType$p$0;
	},

	set_chartType: function Microsoft_Crm_Client_Core_ViewModels_Controls_PresentationSeries$set_chartType(value) {
		this._chartType$p$0 = value || 'Column';
		return value;
	},

	get_color: function Microsoft_Crm_Client_Core_ViewModels_Controls_PresentationSeries$get_color() {
		return this._color$p$0;
	},

	set_color: function Microsoft_Crm_Client_Core_ViewModels_Controls_PresentationSeries$set_color(value) {
		this._color$p$0 = (!value) ? null : String.format(Microsoft.Crm.Client.Core.ViewModels.Controls.ChartPresentationParser.colorStringFormat, value.trim());
		return value;
	},

	get_isValueShownAsLabel: function Microsoft_Crm_Client_Core_ViewModels_Controls_PresentationSeries$get_isValueShownAsLabel() {
		return this._isValueShownAsLabel$p$0;
	},

	set_isValueShownAsLabel: function Microsoft_Crm_Client_Core_ViewModels_Controls_PresentationSeries$set_isValueShownAsLabel(value) {
		this._isValueShownAsLabel$p$0 = value;
		return value;
	},

	get_name: function Microsoft_Crm_Client_Core_ViewModels_Controls_PresentationSeries$get_name() {
		return this._name$p$0;
	},

	set_name: function Microsoft_Crm_Client_Core_ViewModels_Controls_PresentationSeries$set_name(value) {
		this._name$p$0 = value;
		return value;
	},

	get_customProperties: function Microsoft_Crm_Client_Core_ViewModels_Controls_PresentationSeries$get_customProperties() {
		return this._customProperties$p$0;
	},

	set_customProperties: function Microsoft_Crm_Client_Core_ViewModels_Controls_PresentationSeries$set_customProperties(value) {
		this._customProperties$p$0 = value;
		return value;
	},

	get_borderWidth: function Microsoft_Crm_Client_Core_ViewModels_Controls_PresentationSeries$get_borderWidth() {
		return this._borderWidth$p$0;
	},

	set_borderWidth: function Microsoft_Crm_Client_Core_ViewModels_Controls_PresentationSeries$set_borderWidth(value) {
		this._borderWidth$p$0 = value;
		return value;
	},

	get_borderColor: function Microsoft_Crm_Client_Core_ViewModels_Controls_PresentationSeries$get_borderColor() {
		return this._borderColor$p$0;
	},

	set_borderColor: function Microsoft_Crm_Client_Core_ViewModels_Controls_PresentationSeries$set_borderColor(value) {
		this._borderColor$p$0 = (!value) ? null : String.format(Microsoft.Crm.Client.Core.ViewModels.Controls.ChartPresentationParser.colorStringFormat, value.trim());
		return value;
	}
}


Microsoft.Crm.Client.Core.ViewModels.Controls.PresentationLegend = function Microsoft_Crm_Client_Core_ViewModels_Controls_PresentationLegend() {
}
Microsoft.Crm.Client.Core.ViewModels.Controls.PresentationLegend.prototype = {
	_alignment$p$0: null,
	_docking$p$0: null,
	_foreColor$p$0: null,
	_enabled$p$0: false,

	get_alignment: function Microsoft_Crm_Client_Core_ViewModels_Controls_PresentationLegend$get_alignment() {
		return this._alignment$p$0;
	},

	set_alignment: function Microsoft_Crm_Client_Core_ViewModels_Controls_PresentationLegend$set_alignment(value) {
		this._alignment$p$0 = value;
		return value;
	},

	get_docking: function Microsoft_Crm_Client_Core_ViewModels_Controls_PresentationLegend$get_docking() {
		return this._docking$p$0;
	},

	set_docking: function Microsoft_Crm_Client_Core_ViewModels_Controls_PresentationLegend$set_docking(value) {
		this._docking$p$0 = value;
		return value;
	},

	get_foreColor: function Microsoft_Crm_Client_Core_ViewModels_Controls_PresentationLegend$get_foreColor() {
		return this._foreColor$p$0;
	},

	set_foreColor: function Microsoft_Crm_Client_Core_ViewModels_Controls_PresentationLegend$set_foreColor(value) {
		this._foreColor$p$0 = (!value) ? null : String.format(Microsoft.Crm.Client.Core.ViewModels.Controls.ChartPresentationParser.colorStringFormat, value.trim());
		return value;
	},

	get_enabled: function Microsoft_Crm_Client_Core_ViewModels_Controls_PresentationLegend$get_enabled() {
		return this._enabled$p$0;
	},

	set_enabled: function Microsoft_Crm_Client_Core_ViewModels_Controls_PresentationLegend$set_enabled(value) {
		this._enabled$p$0 = value;
		return value;
	}
}


Type.registerNamespace('Microsoft.Crm.Client.Core.Storage.Common.FetchExpression');

Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.ICondition = function () { }
Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.ICondition.registerInterface('Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.ICondition');


Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.IFilterExpression = function () { }
Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.IFilterExpression.registerInterface('Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.IFilterExpression');


Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.LogicalOperator = function () { }
Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.LogicalOperator.prototype = {
	and: 0,
	or: 1,
	not: 2
}
Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.LogicalOperator.registerEnum('Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.LogicalOperator', false);


Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.AttributeQueryExpression = function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_AttributeQueryExpression(name, alias, parentEntity, dateTimeGrouping) {
	this._name$p$0 = name;
	this._aliasName$p$0 = alias;
	this._entity$p$0 = parentEntity;
	this._dateTimeGrouping$p$0 = (_Script.isUndefined(dateTimeGrouping)) ? null : dateTimeGrouping;
}
Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.AttributeQueryExpression.generateUniqueAliasName = function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_AttributeQueryExpression$generateUniqueAliasName(entityName, attributeName) {
	Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.AttributeQueryExpression._sequence$p++;
	if (Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.AttributeQueryExpression._sequence$p > Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.AttributeQueryExpression._maxSequence$p) {
		Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.AttributeQueryExpression._sequence$p = 0;
	}
	return String.format('{0}_{1}{2}_{3}', entityName, Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.AttributeQueryExpression._sequence$p, attributeName, new Date().getTime());
}
Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.AttributeQueryExpression.prototype = {
	_name$p$0: null,
	_aliasName$p$0: null,
	_hasGroupBy$p$0: false,
	_hasAggregate$p$0: false,
	_aggregateType$p$0: null,
	_entity$p$0: null,
	_dateTimeGrouping$p$0: null,

	get_entity: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_AttributeQueryExpression$get_entity() {
		return this._entity$p$0;
	},

	get_name: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_AttributeQueryExpression$get_name() {
		return this._name$p$0;
	},

	get_aliasName: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_AttributeQueryExpression$get_aliasName() {
		return this._aliasName$p$0;
	},

	get_aggregateType: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_AttributeQueryExpression$get_aggregateType() {
		return this._aggregateType$p$0;
	},

	set_aggregateType: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_AttributeQueryExpression$set_aggregateType(value) {
		this._aggregateType$p$0 = value;
		return value;
	},

	get_hasGroupBy: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_AttributeQueryExpression$get_hasGroupBy() {
		return this._hasGroupBy$p$0;
	},

	set_hasGroupBy: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_AttributeQueryExpression$set_hasGroupBy(value) {
		this._hasGroupBy$p$0 = value;
		return value;
	},

	get_hasAggregate: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_AttributeQueryExpression$get_hasAggregate() {
		return this._hasAggregate$p$0;
	},

	set_hasAggregate: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_AttributeQueryExpression$set_hasAggregate(value) {
		this._hasAggregate$p$0 = value;
		return value;
	},

	get_dateTimeGrouping: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_AttributeQueryExpression$get_dateTimeGrouping() {
		return this._dateTimeGrouping$p$0;
	}
}


Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.Condition = function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_Condition(attributeName, passedOperator, value) {
	this.set_attributeName(attributeName);
	this.set_operator(passedOperator);
	this.set_value(value);
}
Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.Condition.parseXMLNode = function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_Condition$parseXMLNode(conditionNode, filterExpression) {
	var value = (_Script.isNullOrUndefined(conditionNode.getAttribute('value'))) ? null : conditionNode.getAttribute('value').toString();
	var entityName = (_Script.isNullOrUndefined(conditionNode.getAttribute('entityname'))) ? null : conditionNode.getAttribute('entityname').toString();
	if (_Script.isNullOrUndefined(value)) {
		var valueList = conditionNode.selectNodes('value');
		if (valueList.get_count() > 0) {
			var values = new (Microsoft.Crm.Client.Core.Framework.List$1.$$(String))();
			for (var j = 0; j < valueList.get_count() ; j++) {
				values.add(valueList.get_item(j).get_innerText());
			}
			if (_Script.isNullOrUndefined(entityName)) {
				filterExpression.addCondition(conditionNode.getAttribute('attribute').toString(), conditionNode.getAttribute('operator').toString(), values);
			}
			else {
				(filterExpression).addCondition(conditionNode.getAttribute('attribute').toString(), conditionNode.getAttribute('operator').toString(), values, conditionNode.getAttribute('entityname').toString());
			}
			return;
		}
	}
	if (_Script.isNullOrUndefined(entityName)) {
		filterExpression.addCondition(conditionNode.getAttribute('attribute').toString(), conditionNode.getAttribute('operator').toString(), value);
	}
	else {
		(filterExpression).addCondition(conditionNode.getAttribute('attribute').toString(), conditionNode.getAttribute('operator').toString(), value, conditionNode.getAttribute('entityname').toString());
	}
}
Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.Condition.prototype = {
	_entityName$p$0: null,

	get_entityName: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_Condition$get_entityName() {
		if (_Script.isNullOrUndefined(this._entityName$p$0)) {
			this._entityName$p$0 = Microsoft.Crm.Client.Core.Framework._String.empty;
		}
		return this._entityName$p$0;
	},

	set_entityName: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_Condition$set_entityName(value) {
		this._entityName$p$0 = value;
		return value;
	},

	_$$pf_AttributeName$p$0: null,

	get_attributeName: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_Condition$get_attributeName() {
		return this._$$pf_AttributeName$p$0;
	},

	set_attributeName: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_Condition$set_attributeName(value) {
		this._$$pf_AttributeName$p$0 = value;
		return value;
	},

	_$$pf_Operator$p$0: null,

	get_operator: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_Condition$get_operator() {
		return this._$$pf_Operator$p$0;
	},

	set_operator: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_Condition$set_operator(value) {
		this._$$pf_Operator$p$0 = value;
		return value;
	},

	_$$pf_Value$p$0: null,

	get_value: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_Condition$get_value() {
		return this._$$pf_Value$p$0;
	},

	set_value: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_Condition$set_value(value) {
		this._$$pf_Value$p$0 = value;
		return value;
	},

	_$$pf_ConditionName$p$0: null,

	get_conditionName: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_Condition$get_conditionName() {
		return this._$$pf_ConditionName$p$0;
	},

	set_conditionName: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_Condition$set_conditionName(value) {
		this._$$pf_ConditionName$p$0 = value;
		return value;
	},

	clone: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_Condition$clone() {
		var condition = new Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.Condition(this.get_attributeName(), this.get_operator(), this.get_value());
		condition.set_entityName(this.get_entityName());
		condition.set_conditionName(this.get_conditionName());
		return condition;
	},

	equals: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_Condition$equals(condition) {
		if (!condition) {
			return false;
		}
		if (this.get_attributeName() === condition.get_attributeName() && this.get_operator() === condition.get_operator() && this.get_entityName() === condition.get_entityName()) {
			if (!condition.get_value() && !this.get_value()) {
				return true;
			}
			else if (!condition.get_value() || !this.get_value() || (Object.getType(condition.get_value()) !== Object.getType(this.get_value()))) {
				return false;
			}
			else if (Object.getType(condition.get_value()) === String) {
				return this.get_value() === condition.get_value();
			}
			else if (Object.getType(condition.get_value()) === Microsoft.Crm.Client.Core.Framework.List$1.$$(String)) {
				var argValues = condition.get_value();
				var thisValues = this.get_value();
				if (argValues.get_Count() !== thisValues.get_Count()) {
					return false;
				}
				else {
					for (var i = 0; i < argValues.get_Count() ; i++) {
						if (argValues.get_item(i) !== thisValues.get_item(i)) {
							return false;
						}
					}
					return true;
				}
			}
		}
		return false;
	}
}


Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.DateTimeGroupingInfo = function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_DateTimeGroupingInfo(type) {
	if (type < Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.DateTimeGroupingInfo.day || type > Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.DateTimeGroupingInfo.fiscalYear) {
		throw Error.argumentOutOfRange('type', type);
	}
	this._groupingType$p$0 = type;
}
Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.DateTimeGroupingInfo.fromGroupingTypeNameToType = function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_DateTimeGroupingInfo$fromGroupingTypeNameToType(typeName) {
	if (!typeName) {
		return -1;
	}
	switch (typeName) {
		case 'day':
			return Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.DateTimeGroupingInfo.day;
		case 'week':
			return Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.DateTimeGroupingInfo.week;
		case 'month':
			return Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.DateTimeGroupingInfo.month;
		case 'year':
			return Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.DateTimeGroupingInfo.year;
		case 'quarter':
			return Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.DateTimeGroupingInfo.quarter;
		case 'fiscal-period':
			return Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.DateTimeGroupingInfo.fiscalPeriod;
		case 'fiscal-year':
			return Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.DateTimeGroupingInfo.fiscalYear;
		default:
			return -1;
	}
}
Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.DateTimeGroupingInfo.fromGroupingTypeToName = function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_DateTimeGroupingInfo$fromGroupingTypeToName(type) {
	switch (type) {
		case Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.DateTimeGroupingInfo.day:
			return 'day';
		case Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.DateTimeGroupingInfo.week:
			return 'week';
		case Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.DateTimeGroupingInfo.month:
			return 'month';
		case Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.DateTimeGroupingInfo.year:
			return 'year';
		case Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.DateTimeGroupingInfo.quarter:
			return 'quarter';
		case Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.DateTimeGroupingInfo.fiscalPeriod:
			return 'fiscal-period';
		case Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.DateTimeGroupingInfo.fiscalYear:
			return 'fiscal-year';
		default:
			return null;
	}
}
Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.DateTimeGroupingInfo.prototype = {
	_groupingType$p$0: 0,

	get_groupingType: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_DateTimeGroupingInfo$get_groupingType() {
		return this._groupingType$p$0;
	}
}


Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.EntityQueryExpression = function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_EntityQueryExpression(entityName, parent, joinType) {
	this.top = Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.EntityQueryExpression.defaultTopValue;
	this.offset = Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.EntityQueryExpression.defaultOffsetValue;
	this.entityName = entityName;
	this.tableAliasCount = 0;
	this.tableAliases = new (Microsoft.Crm.Client.Core.Framework.List$1.$$(String))();
	this.tableAlias = this.generateTableAlias(this.entityName);
	this.attributes = new Array(0);
	this.attributesByName = {};
	this.orderByAttributes = new Array(0);
	this.groupByAttributes = new Array(0);
	this.aggregateAttributes = new Array(0);
	this.linkedEntities = new Array(0);
	this.parentEntity = parent;
	if (parent) {
		this.baseEntity = parent.baseEntity;
		this.joinType = (_Script.isNullOrUndefined(joinType)) ? Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.EntityQueryExpression.innerJoin : joinType;
	}
	else {
		this.baseEntity = this;
	}
}
Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.EntityQueryExpression.prototype = {
	joinType: 0,
	fetchXmlDoc: null,
	entityName: null,

	get_top: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_EntityQueryExpression$get_top() {
		return this.top;
	},

	get_offset: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_EntityQueryExpression$get_offset() {
		return this.offset;
	},

	set_offset: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_EntityQueryExpression$set_offset(value) {
		this.offset = value;
		return value;
	},

	attributesByName: null,
	attributes: null,
	orderByAttributes: null,
	groupByAttributes: null,
	aggregateAttributes: null,
	linkedEntities: null,
	parentEntity: null,
	baseEntity: null,
	metadataPair: null,
	filterExpression: null,
	hasAggregate: false,
	tableAlias: null,
	tableAliasCount: 0,
	tableAliases: null,
	useLimitStatement: true,

	get_linkedEntities: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_EntityQueryExpression$get_linkedEntities() {
		return this.linkedEntities;
	},

	get_aggregateAttributes: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_EntityQueryExpression$get_aggregateAttributes() {
		return this.aggregateAttributes;
	},

	get_joinType: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_EntityQueryExpression$get_joinType() {
		return this.joinType;
	},

	get_groupByAttributes: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_EntityQueryExpression$get_groupByAttributes() {
		return this.groupByAttributes;
	},

	get_orderByAttributes: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_EntityQueryExpression$get_orderByAttributes() {
		return this.orderByAttributes;
	},

	get_attributesByName: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_EntityQueryExpression$get_attributesByName() {
		return this.attributesByName;
	},

	get_attributes: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_EntityQueryExpression$get_attributes() {
		return this.attributes;
	},

	get_entityName: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_EntityQueryExpression$get_entityName() {
		return this.entityName;
	},

	get_fetchXml: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_EntityQueryExpression$get_fetchXml() {
		return Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlNodeFactory.create(this.fetchXmlDoc.firstChild).get_outerXml();
	},

	get_hasAggregate: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_EntityQueryExpression$get_hasAggregate() {
		return this.hasAggregate;
	},

	get_parentEntity: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_EntityQueryExpression$get_parentEntity() {
		return this.parentEntity;
	},

	set_parentEntity: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_EntityQueryExpression$set_parentEntity(value) {
		this.parentEntity = value;
		return value;
	},

	get_baseEntity: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_EntityQueryExpression$get_baseEntity() {
		return this.baseEntity;
	},

	set_baseEntity: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_EntityQueryExpression$set_baseEntity(value) {
		this.baseEntity = value;
		return value;
	},

	get_filterExpression: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_EntityQueryExpression$get_filterExpression() {
		return this.filterExpression;
	},

	set_filterExpression: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_EntityQueryExpression$set_filterExpression(value) {
		this.filterExpression = value;
		return value;
	},

	get_metadataPair: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_EntityQueryExpression$get_metadataPair() {
		return this.metadataPair;
	},

	set_metadataPair: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_EntityQueryExpression$set_metadataPair(value) {
		this.metadataPair = value;
		return value;
	},

	get_tableAlias: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_EntityQueryExpression$get_tableAlias() {
		return this.tableAlias;
	},

	set_tableAlias: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_EntityQueryExpression$set_tableAlias(value) {
		this.tableAlias = value;
		return value;
	},

	get_useLimitStatement: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_EntityQueryExpression$get_useLimitStatement() {
		return this.useLimitStatement;
	},

	set_useLimitStatement: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_EntityQueryExpression$set_useLimitStatement(value) {
		this.useLimitStatement = value;
		return value;
	},

	generateTableAlias: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_EntityQueryExpression$generateTableAlias(entityName) {
		if (!Microsoft.Crm.Client.Core.Framework._String.isNullOrEmpty(entityName)) {
			var alias = String.format('{0}{1}', entityName, this.tableAliasCount++);
			this.tableAliases.add(alias);
			return alias;
		}
		return Microsoft.Crm.Client.Core.Framework._String.empty;
	},

	initialize: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_EntityQueryExpression$initialize(rootNode) {
		this.fetchXmlDoc = Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlNodeFactory.parseXmlDocument(rootNode.get_outerXml());
		this.hasAggregate = this.getAttributeValue(rootNode, 'aggregate') === 'true';
		var countString = this.getAttributeValue(rootNode, 'count');
		if (countString) {
			this.top = Number.parseInvariant(countString);
		}
		var entityNode = rootNode.selectSingleNode('entity');
		if (_Script.isNullOrUndefined(entityNode) && rootNode.get_tagName() === 'link-entity') {
			entityNode = rootNode;
		}
		if (_Script.isNullOrUndefined(this.parentEntity)) {
			var rootEntityName = this.getAttributeValue(entityNode, 'name');
			this.tableAliasCount = 0;
			this.tableAliases = new (Microsoft.Crm.Client.Core.Framework.List$1.$$(String))();
			this.tableAlias = this.generateTableAlias(rootEntityName);
			this.baseEntity = this;
		}
		this.parser(entityNode);
	},

	insertFilterExpressionIntoFetch: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_EntityQueryExpression$insertFilterExpressionIntoFetch(passedFilterExpression) {
		if (!passedFilterExpression) {
			return;
		}
		this.set_filterExpression(passedFilterExpression);
		var filterNode = this._xmlFilterNode$p$0(this.get_filterExpression());
		var entityNode = this.fetchXmlDoc.getElementsByTagName('entity')[0];
		var oldFilterNode = this.fetchXmlDoc.getElementsByTagName('filter')[0];
		if (!oldFilterNode) {
			entityNode.appendChild(filterNode);
		}
		else {
			entityNode.replaceChild(filterNode, oldFilterNode);
		}
	},

	_xmlFilterNode$p$0: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_EntityQueryExpression$_xmlFilterNode$p$0(passedFilterExpression) {
		var filterNode = null;
		if (!passedFilterExpression || ((!passedFilterExpression.get_filterExpressions() || !passedFilterExpression.get_filterExpressions().get_Count()) && (!passedFilterExpression.get_conditions() || !passedFilterExpression.get_conditions().get_Count()))) {
			return filterNode;
		}
		else if (passedFilterExpression.get_conditions()) {
			filterNode = this.fetchXmlDoc.createElement('filter');
			filterNode.attributes.setNamedItem(Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlNode.createAttribute(this.fetchXmlDoc, 'type', Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.LogicalOperator.toString(passedFilterExpression.get_filterOperator()).toLowerCase()));
			for (var i = 0; i < passedFilterExpression.get_conditions().get_Count() ; i++) {
				var condition = passedFilterExpression.get_conditions().get_item(i);
				var conditionNode = this.fetchXmlDoc.createElement('condition');
				conditionNode.attributes.setNamedItem(Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlNode.createAttribute(this.fetchXmlDoc, 'attribute', condition.get_attributeName()));
				conditionNode.attributes.setNamedItem(Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlNode.createAttribute(this.fetchXmlDoc, 'operator', condition.get_operator()));
				if (!condition.get_value()) {
					filterNode.appendChild(conditionNode);
				}
				else if (Object.getType(condition.get_value()) === String) {
					conditionNode.attributes.setNamedItem(Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlNode.createAttribute(this.fetchXmlDoc, 'value', condition.get_value()));
					filterNode.appendChild(conditionNode);
				}
				else {
					var argValues = condition.get_value();
					for (var j = 0; j < argValues.get_Count() ; j++) {
						var valueNode = this.fetchXmlDoc.createElement('value');
						var textValue = this.fetchXmlDoc.createTextNode('settingValue');
						textValue.nodeValue = argValues.get_item(j);
						valueNode.appendChild(textValue);
						conditionNode.appendChild(valueNode);
					}
					filterNode.appendChild(conditionNode);
				}
			}
		}
		else {
			filterNode = this.fetchXmlDoc.createElement('filter');
			filterNode.attributes.setNamedItem(Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlNode.createAttribute(this.fetchXmlDoc, 'type', Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.LogicalOperator.toString(passedFilterExpression.get_filterOperator()).toLowerCase()));
			if (passedFilterExpression.get_filterExpressions().get_Count() === 1) {
				filterNode = this._xmlFilterNode$p$0(passedFilterExpression.get_filterExpressions().get_item(0));
			}
			else {
				for (var i = 0; i < passedFilterExpression.get_filterExpressions().get_Count() ; i++) {
					filterNode.appendChild(this._xmlFilterNode$p$0(passedFilterExpression.get_filterExpressions().get_item(i)));
				}
			}
		}
		return filterNode;
	},

	insertOrderByAttributeIntoFetch: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_EntityQueryExpression$insertOrderByAttributeIntoFetch(name, aliasName, descending, orderIndex) {
		var orderNode = this.fetchXmlDoc.createElement('order');
		if (name) {
			orderNode.attributes.setNamedItem(Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlNode.createAttribute(this.fetchXmlDoc, 'attribute', name));
		}
		if (aliasName) {
			orderNode.attributes.setNamedItem(Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlNode.createAttribute(this.fetchXmlDoc, 'alias', aliasName));
		}
		orderNode.attributes.setNamedItem(Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlNode.createAttribute(this.fetchXmlDoc, 'descending', (descending) ? 'true' : 'false'));
		var entityNode = this.getEntityNode();
		if (orderIndex < 0) {
			entityNode.appendChild(orderNode);
			return;
		}
		var nodes = entityNode.childNodes;
		var orderNodeIndex = -1;
		for (var i = 0; i < nodes.length; i++) {
			var node = nodes[i];
			var nodeName = node.nodeName;
			if (nodeName && nodeName.toLowerCase() === 'order') {
				orderNodeIndex++;
				if (orderNodeIndex === orderIndex) {
					entityNode.insertBefore(orderNode, node);
					return;
				}
			}
		}
		entityNode.appendChild(orderNode);
	},

	insertOrderBy: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_EntityQueryExpression$insertOrderBy(attributeExpression, usingAliasName, descending, orderIndex) {
		var name = (usingAliasName) ? null : (!attributeExpression.get_name()) ? attributeExpression.get_aliasName() : attributeExpression.get_name();
		var aliasName = (!attributeExpression.get_aliasName()) ? attributeExpression.get_name() : attributeExpression.get_aliasName();
		this.insertOrderByAttributeIntoFetch(name, aliasName, descending, orderIndex);
		var order = new Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.OrderByQueryExpression(name, aliasName, descending, this);
		if (orderIndex === -1 || orderIndex >= this.get_orderByAttributes().length) {
			this.get_orderByAttributes()[this.get_orderByAttributes().length] = order;
		}
		else {
			for (var i = this.get_orderByAttributes().length; i >= 0; i--) {
				if (orderIndex === i) {
					this.get_orderByAttributes()[i] = order;
					break;
				}
				else {
					this.get_orderByAttributes()[i] = this.get_orderByAttributes()[i - 1];
				}
			}
		}
	},

	insertAttributeNode: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_EntityQueryExpression$insertAttributeNode(attributeLogicalName, alias, groupBy, aggregate, dateTimeGrouping, userTimeZone) {
		var attributeNode = this.fetchXmlDoc.createElement('attribute');
		attributeNode.attributes.setNamedItem(Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlNode.createAttribute(this.fetchXmlDoc, 'name', attributeLogicalName));
		if (!_Script.isNullOrUndefined(alias)) {
			attributeNode.attributes.setNamedItem(Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlNode.createAttribute(this.fetchXmlDoc, 'alias', alias));
		}
		if (!_Script.isNullOrUndefined(groupBy)) {
			attributeNode.attributes.setNamedItem(Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlNode.createAttribute(this.fetchXmlDoc, 'groupby', groupBy));
		}
		if (!_Script.isNullOrUndefined(aggregate)) {
			attributeNode.attributes.setNamedItem(Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlNode.createAttribute(this.fetchXmlDoc, 'aggregate', aggregate));
		}
		if (!_Script.isNullOrUndefined(dateTimeGrouping)) {
			attributeNode.attributes.setNamedItem(Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlNode.createAttribute(this.fetchXmlDoc, 'dategrouping', dateTimeGrouping));
			if (!_Script.isNullOrUndefined(userTimeZone)) {
				attributeNode.attributes.setNamedItem(Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlNode.createAttribute(this.fetchXmlDoc, 'usertimezone', userTimeZone));
			}
			else {
				attributeNode.attributes.setNamedItem(Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlNode.createAttribute(this.fetchXmlDoc, 'usertimezone', 'true'));
			}
		}
		var entityNode = this.getEntityNode();
		entityNode.appendChild(attributeNode);
	},

	removeAllAttributesAndOrders: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_EntityQueryExpression$removeAllAttributesAndOrders() {
		this.removeAttributesAndOrder(this.fetchXmlDoc.getElementsByTagName('entity')[0]);
	},

	removeAttributesAndOrder: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_EntityQueryExpression$removeAttributesAndOrder(rootNode) {
		var nodesToRemove = new Array(0);
		for (var i = 0, iLen = rootNode.childNodes.length; i < iLen; i++) {
			var node = rootNode.childNodes[i];
			switch (node.nodeName) {
				case 'attribute':
					nodesToRemove[nodesToRemove.length] = node;
					break;
				case 'order':
					nodesToRemove[nodesToRemove.length] = node;
					break;
				case 'link-entity':
					this.removeAttributesAndOrder(node);
					break;
			}
		}
		for (var i = 0; i < nodesToRemove.length; i++) {
			rootNode.removeChild(nodesToRemove[i]);
		}
	},

	addAttribute: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_EntityQueryExpression$addAttribute(name, alias, parentEntityQueryExpression) {
		var attribute = new Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.AttributeQueryExpression(name, null, this);
		this.get_attributesByName()[name] = attribute;
		this.get_attributes()[this.get_attributes().length] = attribute;
	},

	addLinkEntity: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_EntityQueryExpression$addLinkEntity(linkEntityName, attributeFrom, attributeTo, conditionOperator, joinOperator) {
		var linkEntity = new Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.LinkedEntityQueryExpression(linkEntityName, this);
		linkEntity.setLinkProperties(attributeFrom, attributeTo, conditionOperator, joinOperator);
		this.get_linkedEntities().push(linkEntity);
		return linkEntity;
	},

	parser: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_EntityQueryExpression$parser(rootNode) {
		this.entityName = this.getAttributeValue(rootNode, 'name');
		var nodes = rootNode.childNodes();
		for (var i = 0; i < nodes.get_count() ; i++) {
			var node = nodes.get_item(i);
			if (_Script.isNullOrUndefined(node.get_tagName())) {
				continue;
			}
			var nodeName = node.get_tagName().toLocaleLowerCase();
			switch (nodeName) {
				case 'attribute':
					var dateGrouping = null;
					var groupType = Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.DateTimeGroupingInfo.fromGroupingTypeNameToType(this.getAttributeValue(node, 'dategrouping'));
					if (groupType >= 0) {
						dateGrouping = new Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.DateTimeGroupingInfo(groupType);
					}
					var attributeName = this.getAttributeValue(node, 'name');
					var aliasName = this.getAttributeValue(node, 'alias');
					var attribute = new Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.AttributeQueryExpression(attributeName, aliasName, this, dateGrouping);
					this.attributesByName[(!aliasName) ? attributeName : aliasName] = attribute;
					this.attributes[this.attributes.length] = attribute;
					var aggregateName = this.getAttributeValue(node, 'aggregate');
					if (aggregateName) {
						attribute.set_hasAggregate(true);
						attribute.set_aggregateType(aggregateName);
						this.aggregateAttributes[this.aggregateAttributes.length] = attribute;
					}
					var groupbyName = this.getAttributeValue(node, 'groupby');
					if (groupbyName && groupbyName === 'true') {
						attribute.set_hasGroupBy(true);
						this.groupByAttributes[this.groupByAttributes.length] = attribute;
					}
					break;
				case 'order':
					var orderAliasName = this.getAttributeValue(node, 'alias');
					var orderName = this.getAttributeValue(node, 'attribute');
					var descending = this.getAttributeValue(node, 'descending');
					var orderBy = new Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.OrderByQueryExpression(orderName, orderAliasName, !!descending && descending === 'true', this);
					this.orderByAttributes[this.orderByAttributes.length] = orderBy;
					break;
				case 'link-entity':
					var linkEntityName = this.getAttributeValue(node, 'name');
					var nodeFromAttribute = this.getAttributeValue(node, 'from');
					var nodeToAttribute = this.getAttributeValue(node, 'to');
					var joinOperator = this.getAttributeValue(node, 'link-type');
					var expression = new Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.LinkedEntityQueryExpression(linkEntityName, this);
					expression.setLinkProperties(nodeFromAttribute, nodeToAttribute, Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.ConditionalOperator.equal, this.getLinkType(node));
					expression.initialize(node);
					this.linkedEntities[this.linkedEntities.length] = expression;
					break;
				case 'filter':
					var filterType = this.getAttributeValue(node, 'type');
					var filterOperator = 0;
					if (filterType && filterType.toLowerCase() === 'or') {
						filterOperator = Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.LogicalOperator.or;
					}
					this.filterExpression = new Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.FilterExpression(filterOperator, this.get_entityName());
					this.filterExpression = this.filterParser(node, this.filterExpression);
					break;
			}
		}
	},

	filterParser: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_EntityQueryExpression$filterParser(rootNode, filterExpression) {
		var nodes = rootNode.childNodes();
		for (var i = 0; i < nodes.get_count() ; i++) {
			var node = nodes.get_item(i);
			if (_Script.isNullOrUndefined(node.get_tagName())) {
				continue;
			}
			var nodeName = node.get_tagName().toLocaleLowerCase();
			switch (nodeName) {
				case 'filter':
					var filterType = this.getAttributeValue(node, 'type');
					var filterOperator = 0;
					if (filterType && filterType.toLowerCase() === 'or') {
						filterOperator = Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.LogicalOperator.or;
					}
					var tempFilterExpression = new Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.FilterExpression(filterOperator, 'tempValue');
					tempFilterExpression = this.filterParser(node, tempFilterExpression);
					filterExpression.addFilterExpression(tempFilterExpression);
					break;
				case 'condition':
					var conditionAttributeName = this.getAttributeValue(node, 'attribute');
					var conditionOperator = this.getAttributeValue(node, 'operator');
					var conditionValue = this.getAttributeValue(node, 'value');
					var conditionEntityName = this.getAttributeValue(node, 'uitype');
					if (!i) {
						filterExpression.set_filterExpressionId(conditionAttributeName);
					}
					if (Microsoft.Crm.Client.Core.Framework._String.isNullOrEmpty(conditionValue)) {
						var valueNodes = node.childNodes();
						if (valueNodes.get_count() > 0) {
							var multipleValues = new (Microsoft.Crm.Client.Core.Framework.List$1.$$(String))();
							for (var k = 0; k < valueNodes.get_count() ; k++) {
								var valueNode = valueNodes.get_item(k);
								if (_Script.isNullOrUndefined(valueNode.get_innerText())) {
									continue;
								}
								multipleValues.add(valueNodes.get_item(k).get_innerText());
							}
							filterExpression.addCondition(conditionAttributeName, conditionOperator, multipleValues);
						}
						else {
							filterExpression.addCondition(conditionAttributeName, conditionOperator);
						}
						if (filterExpression.get_conditions().get_Count() > 0) {
							var condition = filterExpression.get_conditions().get_Items()[filterExpression.get_conditions().get_Count() - 1];
							condition.set_entityName(conditionEntityName);
						}
					}
					else {
						filterExpression.addCondition(conditionAttributeName, conditionOperator, conditionValue);
					}
					break;
			}
		}
		return filterExpression;
	},

	getEntityNode: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_EntityQueryExpression$getEntityNode() {
		var entityNode = this.fetchXmlDoc.getElementsByTagName('entity')[0];
		if (_Script.isNullOrUndefined(entityNode)) {
			entityNode = this.fetchXmlDoc.getElementsByTagName('link-entity')[0];
		}
		return entityNode;
	},

	getAttributeValue: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_EntityQueryExpression$getAttributeValue(node, attributeName) {
		return node.getAttribute(attributeName);
	},

	getLinkType: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_EntityQueryExpression$getLinkType(node) {
		var linkType = this.getAttributeValue(node, 'link-type');
		if (!linkType) {
			return Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.EntityQueryExpression.innerJoin;
		}
		switch (linkType) {
			case 'natural':
				return Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.EntityQueryExpression.naturalJoin;
			case 'inner':
				return Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.EntityQueryExpression.innerJoin;
			case 'outer':
				return Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.EntityQueryExpression.leftOuterJoin;
			default:
				throw Error.create('EntityQueryExpression.GetLinkType: Invalid join type!');
		}
	}
}


Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.FilterExpression = function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_FilterExpression(filterOperator, filterExpressionId, isQuickFindFields) {
	this.set_filterOperator(filterOperator);
	this.set_filterExpressionId(filterExpressionId);
	this.set_filterExpressions(new (Microsoft.Crm.Client.Core.Framework.List$1.$$(Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.IFilterExpression))());
	if (_Script.isNullOrUndefined(isQuickFindFields)) {
		this.set_isQuickFindFields(false);
	}
	else {
		this.set_isQuickFindFields(isQuickFindFields);
	}
}
Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.FilterExpression._removeCondition$p = function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_FilterExpression$_removeCondition$p(filterExpression, condition) {
	if (filterExpression.get_conditions()) {
		for (var i = 0; i < filterExpression.get_conditions().get_Count() ; i++) {
			if (filterExpression.get_conditions().get_item(i).equals(condition)) {
				filterExpression.get_conditions().remove(filterExpression.get_conditions().get_item(i));
			}
		}
	}
}
Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.FilterExpression._removeFilterExpression$p = function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_FilterExpression$_removeFilterExpression$p(filterExpression, filterExpressionId) {
	if (filterExpression.get_filterExpressions()) {
		for (var i = 0; i < filterExpression.get_filterExpressions().get_Count() ; i++) {
			if (filterExpressionId === filterExpression.get_filterExpressions().get_item(i).get_filterExpressionId()) {
				filterExpression.get_filterExpressions().remove(filterExpression.get_filterExpressions().get_item(i));
				break;
			}
		}
	}
}
Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.FilterExpression._createXmlFilter$p = function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_FilterExpression$_createXmlFilter$p(passedFilterExpression) {
	var xml = Microsoft.Crm.Client.Core.Framework._String.empty;
	if (!passedFilterExpression || ((!passedFilterExpression.get_filterExpressions() || !passedFilterExpression.get_filterExpressions().get_Count()) && (!passedFilterExpression.get_conditions() || !passedFilterExpression.get_conditions().get_Count()))) {
		return xml;
	}
	else if (passedFilterExpression.get_conditions()) {
		if (passedFilterExpression.get_conditions().get_Count() > 0) {
			xml += '<filter type=\"' + Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.LogicalOperator.toString(passedFilterExpression.get_filterOperator()).toLowerCase() + '\"';
			if (passedFilterExpression.get_isQuickFindFields()) {
				xml += ' isquickfindfields=\"1\"';
			}
			xml += '>';
		}
		for (var i = 0; i < passedFilterExpression.get_conditions().get_Count() ; i++) {
			var condition = passedFilterExpression.get_conditions().get_item(i);
			xml += '<condition ';
			if (!Microsoft.Crm.Client.Core.Framework._String.isNullOrEmpty(condition.get_entityName())) {
				xml += 'entityname=\"' + condition.get_entityName() + '\" ';
			}
			if (!condition.get_value()) {
				xml += 'attribute=\"' + condition.get_attributeName() + '\" operator=\"' + condition.get_operator() + '\"/>';
			}
			else if (Object.getType(condition.get_value()) === String) {
				xml += 'attribute=\"' + condition.get_attributeName() + '\" operator=\"' + condition.get_operator() + '\" value=\"' + Microsoft.Crm.Client.Core.Storage.CrmSoapServiceProxy.EncodeDecode.xmlAttributeEncode(condition.get_value()) + '\" />';
			}
			else {
				var argValues = condition.get_value();
				xml += 'attribute=\"' + condition.get_attributeName() + '\" operator=\"' + condition.get_operator() + '\">';
				for (var j = 0; j < argValues.get_Count() ; j++) {
					xml += '<value>' + Microsoft.Crm.Client.Core.Storage.CrmSoapServiceProxy.EncodeDecode.xmlEncode(argValues.get_item(j)) + '</value>';
				}
				xml += '</condition>';
			}
		}
		if (passedFilterExpression.get_conditions().get_Count() > 0) {
			xml += '</filter>';
		}
	}
	else {
		if (passedFilterExpression.get_filterExpressions().get_Count() > 1) {
			xml += '<filter type=\"' + Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.LogicalOperator.toString(passedFilterExpression.get_filterOperator()).toLowerCase() + '\">';
		}
		for (var i = 0; i < passedFilterExpression.get_filterExpressions().get_Count() ; i++) {
			xml += Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.FilterExpression._createXmlFilter$p(passedFilterExpression.get_filterExpressions().get_item(i));
		}
		if (passedFilterExpression.get_filterExpressions().get_Count() > 1) {
			xml += '</filter>';
		}
	}
	return xml;
}
Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.FilterExpression.prototype = {
	_$$pf_FilterExpressionId$p$0: null,

	get_filterExpressionId: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_FilterExpression$get_filterExpressionId() {
		return this._$$pf_FilterExpressionId$p$0;
	},

	set_filterExpressionId: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_FilterExpression$set_filterExpressionId(value) {
		this._$$pf_FilterExpressionId$p$0 = value;
		return value;
	},

	_$$pf_FilterOperator$p$0: 0,

	get_filterOperator: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_FilterExpression$get_filterOperator() {
		return this._$$pf_FilterOperator$p$0;
	},

	set_filterOperator: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_FilterExpression$set_filterOperator(value) {
		this._$$pf_FilterOperator$p$0 = value;
		return value;
	},

	_$$pf_IsQuickFindFields$p$0: false,

	get_isQuickFindFields: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_FilterExpression$get_isQuickFindFields() {
		return this._$$pf_IsQuickFindFields$p$0;
	},

	set_isQuickFindFields: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_FilterExpression$set_isQuickFindFields(value) {
		this._$$pf_IsQuickFindFields$p$0 = value;
		return value;
	},

	_$$pf_Conditions$p$0: null,

	get_conditions: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_FilterExpression$get_conditions() {
		return this._$$pf_Conditions$p$0;
	},

	set_conditions: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_FilterExpression$set_conditions(value) {
		this._$$pf_Conditions$p$0 = value;
		return value;
	},

	_$$pf_FilterExpressions$p$0: null,

	get_filterExpressions: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_FilterExpression$get_filterExpressions() {
		return this._$$pf_FilterExpressions$p$0;
	},

	set_filterExpressions: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_FilterExpression$set_filterExpressions(value) {
		this._$$pf_FilterExpressions$p$0 = value;
		return value;
	},

	addCondition: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_FilterExpression$addCondition(attribute, operatorName, value, entityName, conditionName) {
		value = (_Script.isNullOrUndefined(value)) ? null : value;
		conditionName = (_Script.isNullOrUndefined(conditionName)) ? Microsoft.Crm.Client.Core.Framework._String.empty : conditionName;
		var condition = new Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.Condition(attribute, operatorName, value);
		condition.set_entityName(entityName);
		condition.set_conditionName(conditionName);
		if (!this.get_conditions()) {
			this.set_conditions(new (Microsoft.Crm.Client.Core.Framework.List$1.$$(Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.ICondition))());
			this.get_conditions().add(condition);
		}
		else {
			this.get_conditions().add(condition);
		}
		return this;
	},

	removeCondition: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_FilterExpression$removeCondition(condition) {
		Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.FilterExpression._removeCondition$p(this, condition);
		return this;
	},

	addFilterExpression: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_FilterExpression$addFilterExpression(passedFilterExpression) {
		if (!passedFilterExpression) {
			return this;
		}
		else if (!this.get_filterExpressions()) {
			this.set_filterExpressions(new (Microsoft.Crm.Client.Core.Framework.List$1.$$(Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.IFilterExpression))());
			this.get_filterExpressions().add(passedFilterExpression);
		}
		else {
			this.get_filterExpressions().add(passedFilterExpression);
		}
		return this;
	},

	removeFilterExpression: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_FilterExpression$removeFilterExpression(filterExpressionId) {
		Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.FilterExpression._removeFilterExpression$p(this, filterExpressionId);
		return this;
	},

	getFilterExpression: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_FilterExpression$getFilterExpression(filterExpressionId) {
		for (var i = 0; this.get_filterExpressions() && i < this.get_filterExpressions().get_Count() ; i++) {
			if (filterExpressionId === this.get_filterExpressions().get_item(i).get_filterExpressionId()) {
				return this.get_filterExpressions().get_item(i);
			}
		}
		return null;
	},

	reset: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_FilterExpression$reset(filterExpression) {
		if (!filterExpression) {
			this.clear();
		}
		else {
			this.set_filterOperator(filterExpression.get_filterOperator());
			this.set_filterExpressionId(filterExpression.get_filterExpressionId());
			this.set_conditions(filterExpression.get_conditions());
			this.set_filterExpressions(filterExpression.get_filterExpressions());
		}
		return this;
	},

	clear: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_FilterExpression$clear() {
		this.set_filterExpressionId(null);
		this.set_filterExpressions(null);
		this.set_conditions(null);
	},

	clone: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_FilterExpression$clone() {
		var createdFilterExpression = new Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.FilterExpression(this.get_filterOperator(), this.get_filterExpressionId());
		if (this.get_conditions()) {
			createdFilterExpression.set_conditions(new (Microsoft.Crm.Client.Core.Framework.List$1.$$(Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.ICondition))());
			for (var i = 0; i < this.get_conditions().get_Count() ; i++) {
				createdFilterExpression.get_conditions().add(this.get_conditions().get_item(i).clone());
			}
		}
		else {
			createdFilterExpression.set_conditions(null);
		}
		if (this.get_filterExpressions()) {
			createdFilterExpression.set_filterExpressions(new (Microsoft.Crm.Client.Core.Framework.List$1.$$(Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.IFilterExpression))());
			for (var i = 0; i < this.get_filterExpressions().get_Count() ; i++) {
				createdFilterExpression.get_filterExpressions().add(this.get_filterExpressions().get_item(i).clone());
			}
		}
		else {
			createdFilterExpression.set_filterExpressions(null);
		}
		return createdFilterExpression;
	},

	toFetchXml: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_FilterExpression$toFetchXml() {
		return Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.FilterExpression._createXmlFilter$p(this);
	}
}


Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.OrderByQueryExpression = function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_OrderByQueryExpression(name, aliasName, isDescending, parentEntity) {
	this._name$p$0 = name;
	this._aliasName$p$0 = aliasName;
	this._descending$p$0 = isDescending;
	this._entity$p$0 = parentEntity;
}
Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.OrderByQueryExpression.prototype = {
	_name$p$0: null,
	_aliasName$p$0: null,
	_descending$p$0: false,
	_entity$p$0: null,

	get_entity: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_OrderByQueryExpression$get_entity() {
		return this._entity$p$0;
	},

	get_descending: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_OrderByQueryExpression$get_descending() {
		return this._descending$p$0;
	},

	get_name: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_OrderByQueryExpression$get_name() {
		return this._name$p$0;
	},

	get_aliasName: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_OrderByQueryExpression$get_aliasName() {
		return this._aliasName$p$0;
	}
}


Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.ConditionalOperator = function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_ConditionalOperator() {
}


Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.LinkedEntityQueryExpression = function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_LinkedEntityQueryExpression(entityName, parent, joinType) {
	Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.LinkedEntityQueryExpression.initializeBase(this);
	this.entityName = entityName;
	this.attributes = new Array(0);
	this.attributesByName = {};
	this.orderByAttributes = new Array(0);
	this.groupByAttributes = new Array(0);
	this.aggregateAttributes = new Array(0);
	this.linkedEntities = new Array(0);
	this.parentEntity = parent;
	if (parent) {
		this.baseEntity = parent.get_baseEntity();
		this.tableAlias = this.baseEntity.generateTableAlias(this.entityName);
		this.joinType = (_Script.isNullOrUndefined(joinType)) ? Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.EntityQueryExpression.innerJoin : joinType;
	}
}
Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.LinkedEntityQueryExpression.prototype = {
	_linkToAttribute$p$1: null,
	_linkFromAttribute$p$1: null,
	_conditionOperator$p$1: null,

	get_linkToAttribute: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_LinkedEntityQueryExpression$get_linkToAttribute() {
		return this._linkToAttribute$p$1;
	},

	get_linkFromAttribute: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_LinkedEntityQueryExpression$get_linkFromAttribute() {
		return this._linkFromAttribute$p$1;
	},

	get_conditionOperator: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_LinkedEntityQueryExpression$get_conditionOperator() {
		return this._conditionOperator$p$1;
	},

	setLinkProperties: function Microsoft_Crm_Client_Core_Storage_Common_FetchExpression_LinkedEntityQueryExpression$setLinkProperties(attributeFrom, attributeTo, conditionOperator, joinType) {
		this._linkFromAttribute$p$1 = attributeFrom;
		this._linkToAttribute$p$1 = attributeTo;
		this._conditionOperator$p$1 = conditionOperator;
		this.joinType = joinType;
	}
}


Type.registerNamespace('Microsoft.Crm.Client.Core.Storage.CrmSoapServiceProxy');

Microsoft.Crm.Client.Core.Storage.CrmSoapServiceProxy.EncodeDecode = function Microsoft_Crm_Client_Core_Storage_CrmSoapServiceProxy_EncodeDecode() {
}
Microsoft.Crm.Client.Core.Storage.CrmSoapServiceProxy.EncodeDecode.multilineHtmlEncode = function Microsoft_Crm_Client_Core_Storage_CrmSoapServiceProxy_EncodeDecode$multilineHtmlEncode(value, replaceNewLineForHtml) {
	if (_Script.isNullOrUndefined(value)) {
		return Microsoft.Crm.Client.Core.Framework._String.empty;
	}
	if (_Script.isNullOrUndefined(replaceNewLineForHtml)) {
		replaceNewLineForHtml = false;
	}
	var lines = value.replace('\r\n', '\n').replace('\r', '\n').split('\n');
	for (var i = 0; i < lines.length; i++) {
		lines[i] = Microsoft.Crm.Client.Core.Storage.CrmSoapServiceProxy.EncodeDecode.htmlEncode(lines[i]);
	}
	if (replaceNewLineForHtml) {
		return lines.join('<br />');
	}
	else {
		return lines.join('\r\n');
	}
}
Microsoft.Crm.Client.Core.Storage.CrmSoapServiceProxy.EncodeDecode.xmlAttributeEncode = function Microsoft_Crm_Client_Core_Storage_CrmSoapServiceProxy_EncodeDecode$xmlAttributeEncode(value) {
	if (_Script.isNullOrUndefined(value)) {
		return Microsoft.Crm.Client.Core.Framework._String.empty;
	}
	return CrmEncodeDecode.CrmXmlAttributeEncode(value);
}
Microsoft.Crm.Client.Core.Storage.CrmSoapServiceProxy.EncodeDecode.javaScriptEncode = function Microsoft_Crm_Client_Core_Storage_CrmSoapServiceProxy_EncodeDecode$javaScriptEncode(value) {
	return CrmEncodeDecode.CrmJavaScriptEncode(value);
}
Microsoft.Crm.Client.Core.Storage.CrmSoapServiceProxy.EncodeDecode.htmlAttributeEncode = function Microsoft_Crm_Client_Core_Storage_CrmSoapServiceProxy_EncodeDecode$htmlAttributeEncode(value) {
	return CrmEncodeDecode.CrmHtmlAttributeEncode(value);
}
Microsoft.Crm.Client.Core.Storage.CrmSoapServiceProxy.EncodeDecode.htmlEncode = function Microsoft_Crm_Client_Core_Storage_CrmSoapServiceProxy_EncodeDecode$htmlEncode(value) {
	if (_Script.isNullOrUndefined(value)) {
		return Microsoft.Crm.Client.Core.Framework._String.empty;
	}
	return CrmEncodeDecode.CrmHtmlEncode(value);
}
Microsoft.Crm.Client.Core.Storage.CrmSoapServiceProxy.EncodeDecode.htmlDecode = function Microsoft_Crm_Client_Core_Storage_CrmSoapServiceProxy_EncodeDecode$htmlDecode(value) {
	if (_Script.isNullOrUndefined(value)) {
		return Microsoft.Crm.Client.Core.Framework._String.empty;
	}
	return CrmEncodeDecode.CrmHtmlDecode(value);
}
Microsoft.Crm.Client.Core.Storage.CrmSoapServiceProxy.EncodeDecode.xmlEncode = function Microsoft_Crm_Client_Core_Storage_CrmSoapServiceProxy_EncodeDecode$xmlEncode(value) {
	if (_Script.isNullOrUndefined(value)) {
		return Microsoft.Crm.Client.Core.Framework._String.empty;
	}
	return CrmEncodeDecode.CrmXmlEncode(value);
}
Microsoft.Crm.Client.Core.Storage.CrmSoapServiceProxy.EncodeDecode.urlEncode = function Microsoft_Crm_Client_Core_Storage_CrmSoapServiceProxy_EncodeDecode$urlEncode(value) {
	return CrmEncodeDecode.CrmUrlEncode(value);
}
Microsoft.Crm.Client.Core.Storage.CrmSoapServiceProxy.EncodeDecode.urlDecode = function Microsoft_Crm_Client_Core_Storage_CrmSoapServiceProxy_EncodeDecode$urlDecode(value) {
	return CrmEncodeDecode.CrmUrlDecode(value);
}
Microsoft.Crm.Client.Core.Storage.CrmSoapServiceProxy.EncodeDecode.isValidHref = function Microsoft_Crm_Client_Core_Storage_CrmSoapServiceProxy_EncodeDecode$isValidHref(value) {
	return !Microsoft.Crm.Client.Core.Storage.CrmSoapServiceProxy.EncodeDecode._invalidHRefCharacterPattern$p.test(value);
}


Microsoft.Crm.Client.DataSourceFactory.registerClass('Microsoft.Crm.Client.DataSourceFactory');
Microsoft.Crm.Client.Core.Framework.ErrorInfo.registerClass('Microsoft.Crm.Client.Core.Framework.ErrorInfo');
Microsoft.Crm.Client.Core.Framework.ChartError.registerClass('Microsoft.Crm.Client.Core.Framework.ChartError', Microsoft.Crm.Client.Core.Framework.ErrorInfo);
Microsoft.Crm.Client.Core.Framework.ChartErrorInformation.registerClass('Microsoft.Crm.Client.Core.Framework.ChartErrorInformation');
Microsoft.Crm.Client.Core.Framework.FeatureName.registerClass('Microsoft.Crm.Client.Core.Framework.FeatureName');
Microsoft.Crm.Client.Core.Framework.DictionaryWrapper.registerClass('Microsoft.Crm.Client.Core.Framework.DictionaryWrapper');
Microsoft.Crm.Client.Core.Framework.DisposableBase.registerClass('Microsoft.Crm.Client.Core.Framework.DisposableBase', null, Sys.IDisposable);
Microsoft.Crm.Client.Core.Storage.Common.StorageConstants.registerClass('Microsoft.Crm.Client.Core.Storage.Common.StorageConstants');
Microsoft.Crm.Client.Core.Storage.Common.EntityAttributeMetadataPair.registerClass('Microsoft.Crm.Client.Core.Storage.Common.EntityAttributeMetadataPair');
Microsoft.Crm.Client.Core.Storage.Common.AllColumns.registerClass('Microsoft.Crm.Client.Core.Storage.Common.AllColumns', null, Microsoft.Crm.Client.Core.Storage.Common.IColumnSet);
Microsoft.Crm.Client.Core.Storage.Common.ColumnSet.registerClass('Microsoft.Crm.Client.Core.Storage.Common.ColumnSet', null, Microsoft.Crm.Client.Core.Storage.Common.IColumnSet, Microsoft.Crm.Client.Core.Framework.ISerializable);
Microsoft.Crm.Client.Core.Storage.Common.CrmFormatter.registerClass('Microsoft.Crm.Client.Core.Storage.Common.CrmFormatter');
Microsoft.Crm.Client.Core.Storage.Common.MergeFetchXmlFilterXml.registerClass('Microsoft.Crm.Client.Core.Storage.Common.MergeFetchXmlFilterXml');
Microsoft.Crm.Client.Core.Framework.Common.ResourceManager.registerClass('Microsoft.Crm.Client.Core.Framework.Common.ResourceManager');
Microsoft.Crm.Client.Core.Models.ChartTitle.registerClass('Microsoft.Crm.Client.Core.Models.ChartTitle');
Microsoft.Crm.Client.Core.Models.DataLabels.registerClass('Microsoft.Crm.Client.Core.Models.DataLabels');
Microsoft.Crm.Client.Core.Models.DataPoint.registerClass('Microsoft.Crm.Client.Core.Models.DataPoint');
Microsoft.Crm.Client.Core.Models.Legend.registerClass('Microsoft.Crm.Client.Core.Models.Legend');
Microsoft.Crm.Client.Core.Models.Series.registerClass('Microsoft.Crm.Client.Core.Models.Series');
Microsoft.Crm.Client.Core.Models.XAxis.registerClass('Microsoft.Crm.Client.Core.Models.XAxis');
Microsoft.Crm.Client.Core.Models.YAxis.registerClass('Microsoft.Crm.Client.Core.Models.YAxis');
Microsoft.Crm.Client.Core.Models.DataPointAggregatorBase.registerClass('Microsoft.Crm.Client.Core.Models.DataPointAggregatorBase');
Microsoft.Crm.Client.Core.Models.DataPointAggregator.registerClass('Microsoft.Crm.Client.Core.Models.DataPointAggregator', Microsoft.Crm.Client.Core.Models.DataPointAggregatorBase);
Microsoft.Crm.Client.Core.Models.DataPointDateTimeRangeAggregator.registerClass('Microsoft.Crm.Client.Core.Models.DataPointDateTimeRangeAggregator', Microsoft.Crm.Client.Core.Models.DataPointAggregatorBase);
Microsoft.Crm.Client.Core.Models.DataPointFiscalPeriodAggregator.registerClass('Microsoft.Crm.Client.Core.Models.DataPointFiscalPeriodAggregator', Microsoft.Crm.Client.Core.Models.DataPointAggregatorBase);
Microsoft.Crm.Client.Core.Models.DataPointFiscalYearAggregator.registerClass('Microsoft.Crm.Client.Core.Models.DataPointFiscalYearAggregator', Microsoft.Crm.Client.Core.Models.DataPointAggregatorBase);
Microsoft.Crm.Client.Core.Models._chartAggregatorsContainer.registerClass('Microsoft.Crm.Client.Core.Models._chartAggregatorsContainer');
Microsoft.Crm.Client.Core.Models.Chart.ChartCategory.registerClass('Microsoft.Crm.Client.Core.Models.Chart.ChartCategory');
Microsoft.Crm.Client.Core.Models.Chart.ChartDataDefinitionDescription.registerClass('Microsoft.Crm.Client.Core.Models.Chart.ChartDataDefinitionDescription');
Microsoft.Crm.Client.Core.Models.Chart.ChartDataQueryDecorator.registerClass('Microsoft.Crm.Client.Core.Models.Chart.ChartDataQueryDecorator');
Microsoft.Crm.Client.Core.Models.Chart.ChartGroupBy.registerClass('Microsoft.Crm.Client.Core.Models.Chart.ChartGroupBy');
Microsoft.Crm.Client.Core.Models.Chart.ChartMeasure.registerClass('Microsoft.Crm.Client.Core.Models.Chart.ChartMeasure');
Microsoft.Crm.Client.Core.Models.Chart.ChartMeasureCollectionInfo.registerClass('Microsoft.Crm.Client.Core.Models.Chart.ChartMeasureCollectionInfo');
Microsoft.Crm.Client.Core.Models.Chart.ChartMeasureInfo.registerClass('Microsoft.Crm.Client.Core.Models.Chart.ChartMeasureInfo');
Microsoft.Crm.Client.Core.Models.Chart.ChartQueryModel.registerClass('Microsoft.Crm.Client.Core.Models.Chart.ChartQueryModel');
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeFormat.registerClass('Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeFormat');
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.registerClass('Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata', null, Microsoft.Crm.Client.Core.Storage.Common.IAttributeMetadata, Microsoft.Crm.Client.Core.Framework.ISerializable);
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadataCollection.registerClass('Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadataCollection');
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributePrivilege.registerClass('Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributePrivilege', null, Microsoft.Crm.Client.Core.Framework.ISerializable);
Xrm.Objects.EntityReference.registerClass('Xrm.Objects.EntityReference', null, Microsoft.Crm.Client.Core.Framework.IReference, Microsoft.Crm.Client.Core.Framework.ISerializable);
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.OptionMetadata.registerClass('Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.OptionMetadata', null, Microsoft.Crm.Client.Core.Framework.ISerializable, Microsoft.Crm.Client.Core.Framework.IPicklistItem, Microsoft.Crm.Client.Core.Framework.IOptionMetadata);
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.OptionSetMetadata.registerClass('Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.OptionSetMetadata', null, Microsoft.Crm.Client.Core.Framework.ISerializable, Microsoft.Crm.Client.Core.Framework.IOptionSetMetadata);
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AliasedValue.registerClass('Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AliasedValue', null, Microsoft.Crm.Client.Core.Framework.IAlias, Microsoft.Crm.Client.Core.Framework.ISerializable);
Microsoft.Crm.Client.Core.Storage.DataApi.AttributeMetadataQuery.registerClass('Microsoft.Crm.Client.Core.Storage.DataApi.AttributeMetadataQuery');
ChartConfigObject.registerClass('ChartConfigObject');
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartBuilder.registerClass('Microsoft.Crm.Client.Core.ViewModels.Controls.ChartBuilder', Microsoft.Crm.Client.Core.Framework.DisposableBase);
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator.registerClass('Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator');
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartDataField.registerClass('Microsoft.Crm.Client.Core.ViewModels.Controls.ChartDataField');
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartDataParser.registerClass('Microsoft.Crm.Client.Core.ViewModels.Controls.ChartDataParser');
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartDataParserFactory.registerClass('Microsoft.Crm.Client.Core.ViewModels.Controls.ChartDataParserFactory');
Microsoft.Crm.Client.Core.ViewModels.Controls._chartFormatter.registerClass('Microsoft.Crm.Client.Core.ViewModels.Controls._chartFormatter');
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartPresentationParser.registerClass('Microsoft.Crm.Client.Core.ViewModels.Controls.ChartPresentationParser');
Microsoft.Crm.Client.Core.ViewModels.Controls.ComparisonChartDataParser.registerClass('Microsoft.Crm.Client.Core.ViewModels.Controls.ComparisonChartDataParser', Microsoft.Crm.Client.Core.ViewModels.Controls.ChartDataParser);
Microsoft.Crm.Client.Core.ViewModels.Controls.DateTimeChartDataParser.registerClass('Microsoft.Crm.Client.Core.ViewModels.Controls.DateTimeChartDataParser', Microsoft.Crm.Client.Core.ViewModels.Controls.ChartDataParser);
Microsoft.Crm.Client.Core.ViewModels.Controls.HighchartStubFactory.registerClass('Microsoft.Crm.Client.Core.ViewModels.Controls.HighchartStubFactory');
Microsoft.Crm.Client.Core.ViewModels.Controls.NormalChartDataParser.registerClass('Microsoft.Crm.Client.Core.ViewModels.Controls.NormalChartDataParser', Microsoft.Crm.Client.Core.ViewModels.Controls.ChartDataParser);
Microsoft.Crm.Client.Core.ViewModels.Controls.PresentationSeries.registerClass('Microsoft.Crm.Client.Core.ViewModels.Controls.PresentationSeries');
Microsoft.Crm.Client.Core.ViewModels.Controls.PresentationLegend.registerClass('Microsoft.Crm.Client.Core.ViewModels.Controls.PresentationLegend');
Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.AttributeQueryExpression.registerClass('Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.AttributeQueryExpression');
Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.Condition.registerClass('Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.Condition', null, Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.ICondition);
Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.DateTimeGroupingInfo.registerClass('Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.DateTimeGroupingInfo');
Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.EntityQueryExpression.registerClass('Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.EntityQueryExpression');
Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.FilterExpression.registerClass('Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.FilterExpression', null, Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.IFilterExpression);
Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.OrderByQueryExpression.registerClass('Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.OrderByQueryExpression');
Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.ConditionalOperator.registerClass('Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.ConditionalOperator');
Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.LinkedEntityQueryExpression.registerClass('Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.LinkedEntityQueryExpression', Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.EntityQueryExpression);
Microsoft.Crm.Client.Core.Storage.CrmSoapServiceProxy.EncodeDecode.registerClass('Microsoft.Crm.Client.Core.Storage.CrmSoapServiceProxy.EncodeDecode');
Microsoft.Crm.Client.DataSourceFactory._instance$p = null;
Microsoft.Crm.Client.Core.Framework.ChartError.typeName = 'ChartError';
Microsoft.Crm.Client.Core.Framework.ChartError._errorTitleKey$p = 'ErrorTitle';
Microsoft.Crm.Client.Core.Framework.ChartError._errorDescriptionKey$p = 'ErrorDescription';
Microsoft.Crm.Client.Core.Framework.ErrorInfo._nameKey$p = 'name';
Microsoft.Crm.Client.Core.Framework.ErrorInfo._messageKey$p = 'message';
Microsoft.Crm.Client.Core.Framework.FeatureName.mobileClientMashup = 'MobileClientMashup';
Microsoft.Crm.Client.Core.Framework.FeatureName.officeMailApp = 'OfficeMailApp';
Microsoft.Crm.Client.Core.Framework.FeatureName.officeMailAppMobile = 'OfficeMailAppMobile';
Microsoft.Crm.Client.Core.Framework.FeatureName.officeMailAppMetadata = 'OfficeMailAppMetadata';
Microsoft.Crm.Client.Core.Framework.FeatureName.sssReliablePromote = 'SSSReliablePromote';
Microsoft.Crm.Client.Core.Framework.FeatureName.mobileClientOffline = 'MobileClientOffline';
Microsoft.Crm.Client.Core.Framework.FeatureName.mobileClientOfflineAutoOptin = 'MobileClientOfflineAutoOptin';
Microsoft.Crm.Client.Core.Framework.FeatureName.associatedGridURLAddressability = 'AssociatedGridURLAddressability';
Microsoft.Crm.Client.Core.Framework.FeatureName.lookupRelationshipFilters = 'LookupRelationshipFilters';
Microsoft.Crm.Client.Core.Framework.FeatureName.interactionCentricMultiEntityChartsFeature = 'InteractionCentricMultiEntityChartsFeature';
Microsoft.Crm.Client.Core.Framework.FeatureName.ishGuidedHelp = 'ISHGuidedHelp';
Microsoft.Crm.Client.Core.Framework.FeatureName.appModuleForOrganization = 'AppModuleForOrganization';
Microsoft.Crm.Client.Core.Framework.FeatureName.appModuleMOCAForScaleGroup = 'AppModuleMOCAForScaleGroup';
Microsoft.Crm.Client.Core.Framework.FeatureName.interactionCentricLookupAutoResolve = 'InteractionCentricLookupAutoResolve';
Microsoft.Crm.Client.Core.Framework.FeatureName.quickFindSearchOnISH = 'QuickFindSearchOnISH';
Microsoft.Crm.Client.Core.Framework.FeatureName.emailEngagement = 'EmailEngagement';
Microsoft.Crm.Client.Core.Framework.FeatureName.interactionCentricEmailLink = 'InteractionCentricEmailLink';
Microsoft.Crm.Client.Core.Framework.FeatureName.deviceIntegration = 'DeviceIntegration';
Microsoft.Crm.Client.Core.Framework.FeatureName.autoDataCapture = 'AutoDataCapture';
Microsoft.Crm.Client.Core.Framework.FeatureName.landingPage = 'LandingPage';
Microsoft.Crm.Client.Core.Framework.FeatureName.taskBasedFlow = 'TaskBasedFlow';
Microsoft.Crm.Client.Core.Storage.Common.StorageConstants.compositeIndexDelimiter = '_';
Microsoft.Crm.Client.Core.Storage.Common.AllColumns._instance$p = null;
Microsoft.Crm.Client.Core.Storage.Common.CrmFormatter.noBreakSpace = '\u00a0';
Microsoft.Crm.Client.Core.Storage.Common.CrmFormatter.zeroWidthNoBreakSpace = '\ufeff';
Microsoft.Crm.Client.Core.Storage.Common.CrmFormatter.leftToRightMark = '\u200e';
Microsoft.Crm.Client.Core.Storage.Common.CrmFormatter.rightToLeftMark = '\u200f';
Microsoft.Crm.Client.Core.Storage.Common.CrmFormatter._instance$p = null;
Microsoft.Crm.Client.Core.Framework.Common.ResourceManager._shortResourceKeyPostfix$p = '_Short';
Microsoft.Crm.Client.Core.Framework.Common.ResourceManager.defaultErrorTitleId = 'Error_Title_Generic';
Microsoft.Crm.Client.Core.Framework.Common.ResourceManager.defaultErrorMessageId = 'Error_Message_Generic_Mobile_Client';
Microsoft.Crm.Client.Core.Framework.Common.ResourceManager.errorTitleIdPrefix = 'Error_Title_0x';
Microsoft.Crm.Client.Core.Framework.Common.ResourceManager.errorMessageIdPrefix = 'Error_Message_0x';
Microsoft.Crm.Client.Core.Framework.Common.ResourceManager._resources$p = {};
Microsoft.Crm.Client.Core.Framework.Common.ResourceManager._isHorizontalMode$p = true;
Microsoft.Crm.Client.Core.Models.Chart.ChartDataDefinitionDescription.onDataDefinitionReadyEventName = 'OnDataDefinitionReady';
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeFormat.durationFormat = 'duration';
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeFormat.timeZoneFormat = 'timezone';
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeFormat.languageFormat = 'language';
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeFormat.dateAndTimeFormat = 'dateandtime';
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeFormat.dateOnlyFormat = 'dateonly';
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeFormat.tickerSymbolFormat = 'tickersymbol';
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.idPath = 'id';
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.entityLogicalNamePath = 'entitylogicalname';
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.logicalNamePath = 'logicalname';
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.typePath = 'type';
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.sourceTypePath = 'sourcetype';
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.displayNamePath = 'displayname';
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.isSecuredPath = 'issecured';
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.isSortableEnabledPath = 'issortableenabled';
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.isValidForCreatePath = 'isvalidforcreate';
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.isValidForUpdatePath = 'isvalidforupdate';
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.isValidForReadPath = 'isvalidforread';
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.requiredLevelPath = 'requiredlevel';
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.maxLengthPath = 'maxlength';
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.minValuePath = 'minvalue';
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.maxValuePath = 'maxvalue';
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.precisionPath = 'precision';
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.precisionSourcePath = 'precisionsource';
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.defaultFormValuePath = 'defaultformvalue';
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.defaultValuePath = 'defaultvalue';
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.formatPath = 'format';
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.behaviorPath = 'behavior';
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.isBaseCurrencyPath = 'isbasecurrency';
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.attributeOfPath = 'attributeof';
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.hasChangedPath = 'haschanged';
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.optionSetPath = 'optionset';
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.targetsPath = 'targets';
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.imeModePath = 'imemode';
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.inheritsFromPath = 'inheritsfrom';
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.isLocalizablePath = 'islocalizable';
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.entityAttributeLogicalNamesPath = 'entitylogicalname_logicalname';
Xrm.Objects.EntityReference._empty$p = null;
Microsoft.Crm.Client.Core.Storage.DataApi.AttributeMetadataQuery._validAttributeRegExp$p = null;
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._categoriesPropertyName$p = 'categories';
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._xPropertyName$p = 'x';
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._textPropertyName$p = 'text';
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._titlePropertyName$p = 'title';
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._stylePropertyName$p = 'style';
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._alignPropertyName$p = 'align';
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._colorPropertyName$p = 'color';
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._labelsPropertyName$p = 'labels';
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._marginPropertyName$p = 'margin';
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._paddingPropertyName$p = 'padding';
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._oppositePropertyName$p = 'opposite';
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._overflowPropertyName$p = 'overflow';
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._fontSizePropertyName$p = 'fontSize';
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._lineHeightPropertyName$p = 'lineheight';
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._floatingPropertyName$p = 'floating';
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._useHTMLPropertyName$p = 'useHTML';
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._rotationPropertyName$p = 'rotation';
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._lineWidthPropertyName$p = 'lineWidth';
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._itemStylePropertyName$p = 'itemStyle';
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._formatterPropertyName$p = 'formatter';
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._lineColorPropertyName$p = 'lineColor';
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._endOnTickPropertyName$p = 'endOnTick';
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._dataLabelsPropertyName$p = 'dataLabels';
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._dataLabelsPropertyPosition$p = 'inside';
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._enabledPropertyName$p = 'enabled';
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._fontWeightPropertyName$p = 'fontWeight';
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._fontFamilyPropertyName$p = 'fontFamily';
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._widthPropertyName$p = 'width';
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._borderWidthPropertyName$p = 'borderWidth';
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._symbolWidthPropertyName$p = 'symbolWidth';
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._borderColorPropertyName$p = 'borderColor';
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._verticalAlignPropertyName$p = 'verticalAlign';
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._startOnTickPropertyName$p = 'startOnTick';
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._showInLegendPropertyName$p = 'showInLegend';
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._gridlineColorPropertyName$p = 'gridLineColor';
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._symbolPaddingPropertyName$p = 'symbolPadding';
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._itemMarginTopPropertyName$p = 'itemMarginTop';
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._gridlineDashStylePropertyName$p = 'gridLineDashStyle';
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._gridlineWidthPropertyName$p = 'gridLineWidth';
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._toolTipPointFormatPropertyName$p = 'pointFormat';
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._toolTipHeaderFormatPropertyName$p = 'headerFormat';
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._hoverStylePropertyName$p = 'itemHoverStyle';
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._defaultColor$p = '#3F94E9';
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._firstAggregatorIndex$p = 0;
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._eventsPropertyName$p = 'events';
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._cursorPropertyName$p = 'cursor';
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._legendClickPropertyName$p = 'legendItemClick';
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._fontWeightNormal$p = 'normal';
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._segoeUiFontFamily$p = 'Segoe UI, SegoeUI';
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._ishSegoeUiFontFamily$p = '\"Segoe UI\", Tahoma, sans-serif';
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._segoeUiSemiBoldFontFamily$p = 'Segoe UI Semibold, SegoeUI-Semibold';
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._ishSegoeUiSemiBoldFontFamily$p = '\"Segoe UI Semibold\", Tahoma, sans-serif';
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._titleColor$p = '#d89a57';
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._titleSize$p = '#12px';
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._titleWidth$p = '#300px';
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._gridlineColor$p = '#d6d6d6';
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._lineColor$p = '#d6d6d6';
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._gridlineDashStyle$p = 'ShortDot';
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._axisLabelColor$p = '#666666';
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._icAxisLabelColor$p = '#FFFFFF';
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._defaultFontSize$p = '11px';
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._filterGraphFontSize$p = '12px';
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._filterGraphLineHeight$p = '18px';
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._overflowJustify$p = 'justify';
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._specialLegendColor$p = '#666666';
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._defaultLegendColor$p = '#000000';
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._icDefaultLegendColor$p = '#FFFFFF';
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._defaultBorderColor$p = '#ffffff';
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator._maxLengthLegend$p = 15;
Microsoft.Crm.Client.Core.ViewModels.Controls._chartFormatter._monthNames$p = null;
Microsoft.Crm.Client.Core.ViewModels.Controls.ChartPresentationParser.colorStringFormat = 'rgb({0})';
Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.AttributeQueryExpression._maxSequence$p = 65535;
Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.AttributeQueryExpression._sequence$p = 0;
Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.DateTimeGroupingInfo.day = 0;
Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.DateTimeGroupingInfo.week = 1;
Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.DateTimeGroupingInfo.month = 2;
Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.DateTimeGroupingInfo.quarter = 3;
Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.DateTimeGroupingInfo.year = 4;
Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.DateTimeGroupingInfo.fiscalPeriod = 5;
Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.DateTimeGroupingInfo.fiscalYear = 6;
Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.EntityQueryExpression.none = -1;
Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.EntityQueryExpression.innerJoin = 0;
Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.EntityQueryExpression.leftOuterJoin = 1;
Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.EntityQueryExpression.naturalJoin = 2;
Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.EntityQueryExpression.defaultTopValue = 5000;
Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.EntityQueryExpression.defaultOffsetValue = 0;
Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.ConditionalOperator.equal = 'eq';
Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.ConditionalOperator.notEqual = 'ne';
Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.ConditionalOperator.greaterThan = 'gt';
Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.ConditionalOperator.greaterThanEqual = 'ge';
Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.ConditionalOperator.lessThan = 'lt';
Microsoft.Crm.Client.Core.Storage.Common.FetchExpression.ConditionalOperator.lessThanEqual = 'le';
Microsoft.Crm.Client.Core.Storage.CrmSoapServiceProxy.EncodeDecode._invalidHRefCharacterPattern$p = new RegExp('(<|>|\\\\|\"|\\r|\\n)', 'i');
//@ sourceMappingURL=.srcmap
