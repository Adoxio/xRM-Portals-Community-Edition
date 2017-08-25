// If you want to suggest a new language you can use this file as a template.
// To reduce the file size you should remove the comment lines (the ones that start with // )
if(!window.calendar_languages) {
	window.calendar_languages = {};
}
// Here you define the language and Country code. Replace en-US with your own.
// First letters: the language code (lower case). See http://www.loc.gov/standards/iso639-2/php/code_list.php
// Last letters: the Country code (upper case). See http://www.iso.org/iso/home/standards/country_codes/country_names_and_code_elements.htm
window.calendar_languages['uk-UA'] = {
	error_noview: 'Календар: Перегляд {0} не знайдено',
	error_dateformat: 'Календар: Неправильний формат дати {0}. Повинно бути або «зараз», або «рррр-мм-дд»',
	error_loadurl: 'Календар: URL-адреса події не вказана',
	error_where: 'Календар: Неправильний напрямок навігації {0}. Допускається лише «далі», «назад» або «сьогодні»',
	error_timedevide: 'Календар: Параметри розподілу часу повинні ділити 60 на цілі числа. Наприклад: 10, 15, 30 і т. д.',

	no_events_in_day: 'На цей день не заплановано подій.',

	// {0} will be replaced with the year (example: 2013)
	title_year: '{0}',
	// {0} will be replaced with the month name (example: September)
	// {1} will be replaced with the year (example: 2013)
	title_month: '{0} {1}',
	// {0} will be replaced with the week number (example: 37)
	// {1} will be replaced with the year (example: 2013)
	title_week: 'тиждень {0} з {1}',
	// {0} will be replaced with the weekday name (example: Thursday)
	// {1} will be replaced with the day of the month (example: 12)
	// {2} will be replaced with the month name (example: September)
	// {3} will be replaced with the year (example: 2013)
	title_day: '{0} {1} {2}, {3}',

	week:'Тиждень {0}',
	all_day:     'Цілий день',
	time:        'Час',
	events:      'Події',
	before_time: 'Закінчується до терміну реалізації',
	after_time:  'Починається після терміну реалізації',

	m0: 'Січень',
	m1: 'Лютий',
	m2: 'Березень',
	m3: 'Квітень',
	m4: 'Травень',
	m5: 'Червень',
	m6: 'Липень',
	m7: 'Серпень',
	m8: 'Вересень',
	m9: 'Жовтень',
	m10: 'Листопад',
	m11: 'Грудень',

	ms0: 'Січ',
	ms1: 'Лют',
	ms2: 'Бер',
	ms3: 'Квіт',
	ms4: 'Тра',
	ms5: 'Черв',
	ms6: 'Лип',
	ms7: 'Серп',
	ms8: 'Вер',
	ms9: 'Жовт',
	ms10: 'Лист',
	ms11: 'Груд',

	d0: 'Неділя',
	d1: 'Понеділок',
	d2: 'Вівторок',
	d3: 'Середа',
	d4: 'Четвер',
	d5: 'П\'ятниця',
	d6: 'Субота',

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
