// If you want to suggest a new language you can use this file as a template.
// To reduce the file size you should remove the comment lines (the ones that start with // )
if(!window.calendar_languages) {
	window.calendar_languages = {};
}
// Here you define the language and Country code. Replace en-US with your own.
// First letters: the language code (lower case). See http://www.loc.gov/standards/iso639-2/php/code_list.php
// Last letters: the Country code (upper case). See http://www.iso.org/iso/home/standards/country_codes/country_names_and_code_elements.htm
window.calendar_languages['ru-RU'] = {
	error_noview: 'Календарь: представление {0} не обнаружено',
	error_dateformat: 'Календарь: неверный формат даты {0}. Дата должна иметь вид \"сейчас\" или \"гггг-мм-дд\"',
	error_loadurl: 'Календарь: URL-адрес события не задан',
	error_where: 'Календарь: неправильное направление перехода {0}. Возможны только значения \"вперед\", \"назад\" и \"сегодня\"',
	error_timedevide: 'Календарь: параметр деления времени должен делить 60 на целочисленные отрезки. Например, он может иметь значение 10, 15, 30.',

	no_events_in_day: 'Нет событий в этот день.',

	// {0} will be replaced with the year (example: 2013)
	title_year: '{0}',
	// {0} will be replaced with the month name (example: September)
	// {1} will be replaced with the year (example: 2013)
	title_month: '{0} {1}',
	// {0} will be replaced with the week number (example: 37)
	// {1} will be replaced with the year (example: 2013)
	title_week: 'Неделя {0} из {1}',
	// {0} will be replaced with the weekday name (example: Thursday)
	// {1} will be replaced with the day of the month (example: 12)
	// {2} will be replaced with the month name (example: September)
	// {3} will be replaced with the year (example: 2013)
	title_day: '{0} {1} {2}, {3}',

	week:'Неделя {0}',
	all_day:     'Целый день',
	time:        'Время',
	events:      'События',
	before_time: 'Завершение до графика',
	after_time:  'Запуск после графика',

	m0: 'Январь',
	m1: 'Февраль',
	m2: 'Март',
	m3: 'Апрель',
	m4: 'Май',
	m5: 'Июнь',
	m6: 'Июль',
	m7: 'Август',
	m8: 'Сентябрь',
	m9: 'Октябрь',
	m10: 'Ноябрь',
	m11: 'Декабрь',

	ms0: 'Янв',
	ms1: 'Фев',
	ms2: 'Мар',
	ms3: 'Апр',
	ms4: 'Май',
	ms5: 'Июн',
	ms6: 'Июл',
	ms7: 'Авг',
	ms8: 'Сен',
	ms9: 'Окт',
	ms10: 'Ноя',
	ms11: 'Дек',

	d0: 'Воскресенье',
	d1: 'Понедельник',
	d2: 'Вторник',
	d3: 'Среда',
	d4: 'Четверг',
	d5: 'Пятница',
	d6: 'Суббота',

	// Which is the first day of the week (2 for sunday, 1 for monday)
	first_day: 2,

	// The list of the holidays.
	// Each holiday has a date definition and a name (in your language)
	// For instance:
	// holidays: {
	// 	'date': 'name',
	// 	'date': 'name',
	// 	...
	//   'date': 'name' //No ending comma for the last holiday
	// }
	// The format of the date may be one of the following:
	// # For a holiday recurring every year in the same day: 'dd-mm' (dd is the day of the month, mm is the month). For example: '25-12'.
	// # For a holiday that exists only in one specific year: 'dd-mm-yyyy' (dd is the day of the month, mm is the month, yyyy is the year). For example: '31-01-2013'
	// # For Easter: use simply 'easter'
	// # For holidays that are based on the Easter date: 'easter+offset in days'.
	//   Some examples:
	//   - 'easter-2' is Good Friday (2 days before Easter)
	//   - 'easter+1' is Easter Monday (1 day after Easter)
	//   - 'easter+39' is the Ascension Day
	//   - 'easter+49' is Pentecost
	// # For holidays that are on a specific weekday after the beginning of a month: 'mm+n*w', where 'mm' is the month, 'n' is the ordinal position, 'w' is the weekday being 0: Sunday, 1: Monday, ..., 6: Saturnday
	//   For example:
	//   - Second (2) Monday (1) in October (10): '10+2*1'
	// # For holidays that are on a specific weekday before the ending of a month: 'mm-n*w', where 'mm' is the month, 'n' is the ordinal position, 'w' is the weekday being 0: Sunday, 1: Monday, ..., 6: Saturnday
	//   For example:
	//   - Last (1) Saturnday (6) in Match (03): '03-1*6'
	//   - Last (1) Monday (1) in May (05): '05-1*1'
	// # You can also specify a holiday that lasts more than one day. To do that use the format 'start>end' where 'start' and 'end' are specified as above.
	//   For example:
	//   - From 1 January to 6 January: '01-01>06-01'
	//   - Easter and the day after Easter: 'easter>easter+1'
	//   Limitations: currently the multi-day holydays can't cross an year. So, for example, you can't specify a range as '30-12>01-01'; as a workaround you can specify two distinct holidays (for instance '30-12>31-12' and '01-01'). 
	holidays: {
	}
};
