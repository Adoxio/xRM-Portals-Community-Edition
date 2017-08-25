//http://stackoverflow.com/questions/20101603/how-do-i-convert-between-date-formats-used-by-jquery-ui-datepicker-and-moment-js
var dateFormatConverter;
(function (dateFormatConverter) {
	if (!String.prototype.replaceAll) {
		String.prototype.replaceAll = function (pattern, replacement) {
			return this.split(pattern).join(replacement);
		};
	}
	if (!String.prototype.contains) {
		String.prototype.contains = function (part) {
			return this.indexOf(part) >= 0;
		};
	}
	if (!Array.prototype.first) {
		Array.prototype.first = function (callback) {
			if (!callback)
				return this.length ? this[0] : null;

			for (var i = 0; i < this.length; i++) {
				var item = this[i];
				if (callback(item)) {
					return item;
				}
			}

			return null;
		};
	}
	if (!Array.prototype.map) {
		Array.prototype.map = function (callback, thisArg) {
			var T, A, k;
			if (this == null) {
				throw new TypeError(' this is null or not defined');
			}
			var O = Object(this);
			var len = O.length >>> 0;
			if (typeof callback !== 'function') {
				throw new TypeError(callback + ' is not a function');
			}
			if (arguments.length > 1) {
				T = thisArg;
			}
			A = new Array(len);
			k = 0;
			while (k < len) {
				var kValue, mappedValue;
				if (k in O) {
					kValue = O[k];
					mappedValue = callback.call(T, kValue, k, O);
					A[k] = mappedValue;
				}
				k++;
			}
			return A;
		};
	}
	if (!Object.keys) {
		Object.keys = (function () {
			'use strict';
			var hasOwnProperty = Object.prototype.hasOwnProperty,
				hasDontEnumBug = !({ toString: null }).propertyIsEnumerable('toString'),
				dontEnums = [
				  'toString',
				  'toLocaleString',
				  'valueOf',
				  'hasOwnProperty',
				  'isPrototypeOf',
				  'propertyIsEnumerable',
				  'constructor'
				],
				dontEnumsLength = dontEnums.length;

			return function (obj) {
				if (typeof obj !== 'object' && (typeof obj !== 'function' || obj === null)) {
					throw new TypeError('Object.keys called on non-object');
				}

				var result = [], prop, i;

				for (prop in obj) {
					if (hasOwnProperty.call(obj, prop)) {
						result.push(prop);
					}
				}

				if (hasDontEnumBug) {
					for (i = 0; i < dontEnumsLength; i++) {
						if (hasOwnProperty.call(obj, dontEnums[i])) {
							result.push(dontEnums[i]);
						}
					}
				}
				return result;
			};
		}());
	}

	function convert(format, sourceRules, destRules) {
		if (sourceRules == destRules)
			return format;

		format = format || '';

		var result = '';
		var index = 0;
		var destTokens = getTokens(destRules);
		var sourceMap = getTokenMap(getTokens(sourceRules));
		while (index < format.length) {
			var part = locateNextToken(sourceRules, format, index);
			if (part.literal.length > 0)
				result += destRules.MakeLiteral(part.literal);
			if (part.token.length > 0)
				result += destTokens[sourceMap[part.token]];
			index = part.nextBegin;
		}

		return result;
	}

	dateFormatConverter.convert = convert;

	function locateNextToken(rules, format, begin) {
		var literal = '';
		var index = begin;
		var sequence = getTokenSequence(getTokenMap(getTokens(rules)));
		while (index < format.length) {
			var escaped = rules.ReadEscapedPart(format, index);
			if (escaped.length > 0) {
				literal += escaped.value;
				index += escaped.length;
				continue;
			}

			var token = sequence.first(function(x) {
				return format.indexOf(x, index) == index;
			});
			if (!token) {
				literal += format.charAt(index);
				index++;
				continue;
			}

			return {
				token: token,
				literal: literal,
				nextBegin: index + token.length
			};
		}

		return {
			token: '',
			literal: literal,
			nextBegin: index
		};
	}

	function getTokens(rules) {
		return [
			rules.DayOfMonthShort,
			rules.DayOfMonthLong,
			rules.DayOfWeekShort,
			rules.DayOfWeekLong,
			rules.DayOfYearShort,
			rules.DayOfYearLong,
			rules.MonthOfYearShort,
			rules.MonthOfYearLong,
			rules.MonthNameShort,
			rules.MonthNameLong,
			rules.YearShort,
			rules.YearLong,
			rules.AmPm,
			rules.Hour24Short,
			rules.Hour24Long,
			rules.Hour12Short,
			rules.Hour12Long,
			rules.MinuteShort,
			rules.MinuteLong,
			rules.SecondShort,
			rules.SecondLong,
			rules.FractionalSecond1,
			rules.FractionalSecond2,
			rules.FractionalSecond3,
			rules.TimeZone,
			rules.UnixTimestamp
		].map(function(x) {
			return x || '';
		});
	}

	function getTokenMap(tokens) {
		var map = {};
		for (var i = 0; i < tokens.length; i++) {
			var token = tokens[i];
			if (token) {
				map[token] = i;
			}
		}
		return map;
	}

	function getTokenSequence(map) {
		var tokens = Object.keys(map);
		tokens.sort(function(a, b) {
			return b.length - a.length;
		});
		return tokens;
	}

	function indexOfAny(s, chars) {
		for (var i = 0; i < s.length; i++) {
			var c = s.charAt(i);
			for (var j = 0; j < chars.length; j++) {
				if (c === chars.charAt(j))
					return i;
			}
		}
		return -1;
	}

	dateFormatConverter.standard = {
		DayOfMonthShort: 'd',
		DayOfMonthLong: 'dd',
		DayOfWeekShort: 'ddd',
		DayOfWeekLong: 'dddd',
		DayOfYearShort: 'D',
		DayOfYearLong: 'DD',
		MonthOfYearShort: 'M',
		MonthOfYearLong: 'MM',
		MonthNameShort: 'MMM',
		MonthNameLong: 'MMMM',
		YearShort: 'yy',
		YearLong: 'yyyy',
		AmPm: 'tt',
		Hour24Short: 'H',
		Hour24Long: 'HH',
		Hour12Short: 'h',
		Hour12Long: 'hh',
		MinuteShort: 'm',
		MinuteLong: 'mm',
		SecondShort: 's',
		SecondLong: 'ss',
		FractionalSecond1: 'f',
		FractionalSecond2: 'ff',
		FractionalSecond3: 'fff',
		TimeZone: 'Z',
		UnixTimestamp: 'X',
		MakeLiteral: function(literal) {
			var reserved = 'dDMytHhmsfZX';
			if (indexOfAny(literal, reserved) < 0)
				return literal;

			var result = '';
			for (var i = 0; i < literal.length; i++) {
				var c = literal.charAt(i);
				if (reserved.contains(c))
					result += '\\';
				result += c;
			}
			return result;
		},
		ReadEscapedPart: function(format, startIndex) {
			var result = '';
			var index = startIndex;
			while (index < format.length) {
				var c = format.charAt(index);

				if (c == '\\') {
					result += index == format.length - 1 ? '\\' : format[++index];
					index++;
					continue;
				}
				break;
			}

			return {
				value: result,
				length: index - startIndex
			};
		}
	};

	dateFormatConverter.dotNet = {
		DayOfMonthShort: 'd',
		DayOfMonthLong: 'dd',
		DayOfWeekShort: 'ddd',
		DayOfWeekLong: 'dddd',
		DayOfYearShort: null,
		DayOfYearLong: null,
		MonthOfYearShort: 'M',
		MonthOfYearLong: 'MM',
		MonthNameShort: 'MMM',
		MonthNameLong: 'MMMM',
		YearShort: 'yy',
		YearLong: 'yyyy',
		AmPm: 'tt',
		Hour24Short: 'H',
		Hour24Long: 'HH',
		Hour12Short: 'h',
		Hour12Long: 'hh',
		MinuteShort: 'm',
		MinuteLong: 'mm',
		SecondShort: 's',
		SecondLong: 'ss',
		FractionalSecond1: 'f',
		FractionalSecond2: 'ff',
		FractionalSecond3: 'fff',
		TimeZone: 'zzz',
		UnixTimestamp: null,
		MakeLiteral: function(literal) {
			var reserved = 'dfFghHKmMstyz\'"';
			if (indexOfAny(literal, reserved) < 0)
				return literal;

			var result = '';
			for (var i = 0; i < literal.length; i++) {
				var c = literal.charAt(i);
				if (reserved.contains(c))
					result += '\\';
				result += c;
			}
			return result;
		},
		ReadEscapedPart: function(format, startIndex) {
			var result = '';
			var index = startIndex;
			while (index < format.length) {
				var c = format.charAt(index);

				if (c == '\\') {
					result += index == format.length - 1 ? '\\' : format[++index];
					index++;
					continue;
				}

				if (c == '"') {
					while (++index < format.length) {
						var cc = format.charAt(index);
						if (cc == '"')
							break;

						if (cc == '\\') {
							result += index == format.length - 1 ? '\\' : format[++index];
						} else {
							result += cc;
						}
					}
					index++;
					continue;
				}

				if (c == "'") {
					while (++index < format.length) {
						var cc = format.charAt(index);
						if (cc == "'")
							break;

						if (cc == '\\') {
							result += index == format.length - 1 ? '\\' : format[++index];
						} else {
							result += cc;
						}
					}
					index++;
					continue;
				}

				break;
			}

			return {
				value: result,
				length: index - startIndex
			};
		}
	};

	dateFormatConverter.momentJs = {
		DayOfMonthShort: 'D',
		DayOfMonthLong: 'DD',
		DayOfWeekShort: 'ddd',
		DayOfWeekLong: 'dddd',
		DayOfYearShort: 'DDD',
		DayOfYearLong: 'DDDD',
		MonthOfYearShort: 'M',
		MonthOfYearLong: 'MM',
		MonthNameShort: 'MMM',
		MonthNameLong: 'MMMM',
		YearShort: 'YY',
		YearLong: 'YYYY',
		AmPm: 'A',
		Hour24Short: 'H',
		Hour24Long: 'HH',
		Hour12Short: 'h',
		Hour12Long: 'hh',
		MinuteShort: 'm',
		MinuteLong: 'mm',
		SecondShort: 's',
		SecondLong: 'ss',
		FractionalSecond1: 'S',
		FractionalSecond2: 'SS',
		FractionalSecond3: 'SSS',
		TimeZone: 'Z',
		UnixTimestamp: 'X',
		MakeLiteral: function(literal) {
			var reserved = 'MoDdeEwWYgGAaHhmsSzZX';

			literal = literal.replaceAll("[", "(").replaceAll("]", ")");
			if (indexOfAny(literal, reserved) < 0)
				return literal;

			return '[' + literal + ']';
		},
		ReadEscapedPart: function(format, startIndex) {
			if (format.charAt(startIndex) != '[')
				return { value: '', length: 0 };

			var result = '';
			var index = startIndex;
			while (index < format.length) {
				var c = format.charAt(index);

				if (c == ']') {
					break;
				}

				result += c;
			}

			return {
				value: result,
				length: index - startIndex
			};
		}
	};
})(dateFormatConverter || (dateFormatConverter = {}));

$(document).ready(function () {
    $.each($('h3>a'), function (i, v) {
        var title = $(v).attr('title');
        if (title!=null && !title.trim()) {
            $(v).attr('title', $(v).text());
        }
    });
});