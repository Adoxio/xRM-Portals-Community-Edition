// If you want to suggest a new language you can use this file as a template.
// To reduce the file size you should remove the comment lines (the ones that start with // )
if(!window.calendar_languages) {
	window.calendar_languages = {};
}
// Here you define the language and Country code. Replace en-US with your own.
// First letters: the language code (lower case). See http://www.loc.gov/standards/iso639-2/php/code_list.php
// Last letters: the Country code (upper case). See http://www.iso.org/iso/home/standards/country_codes/country_names_and_code_elements.htm
window.calendar_languages['kk-KZ'] = {
	error_noview: 'Күнтізбе: {0} көрінісі табылмады',
	error_dateformat: 'Күнтізбе: Қате {0} күн пішімі. Пішім \"қазір\" не \"жжжж-аа-кк\" түрінде болуы керек',
	error_loadurl: 'Күнтізбе: оқиғаның URL мекенжайы орнатылмаған',
	error_where: 'Күнтізбе: Қате {0} навигация бағыты. Тек \"келесі\" не \"алдыңғы\" немесе \"бүгін\" болуы керек',
	error_timedevide: 'Күнтізбе: уақытты бөлу параметрі 60 санын ондық сандарсыз бөлуі керек, мысалы. Мысалы: 10, 15, 30',

	no_events_in_day: 'Осы күні оқиғалар жоқ.',

	// {0} will be replaced with the year (example: 2013)
	title_year: '{0}',
	// {0} will be replaced with the month name (example: September)
	// {1} will be replaced with the year (example: 2013)
	title_month: '{0} {1}',
	// {0} will be replaced with the week number (example: 37)
	// {1} will be replaced with the year (example: 2013)
	title_week: '{1} ішінен {0} апта',
	// {0} will be replaced with the weekday name (example: Thursday)
	// {1} will be replaced with the day of the month (example: 12)
	// {2} will be replaced with the month name (example: September)
	// {3} will be replaced with the year (example: 2013)
	title_day: '{0} {1} {2}, {3}',

	week:'{0} апта',
	all_day:     'Күні бойы',
	time:        'Уақыт',
	events:      'Оқиғалар',
	before_time: 'Уақыт шкаласынан бұрын басталады',
	after_time:  'Уақыт шкаласынан кейін басталады',

	m0: 'Қаңтар',
	m1: 'Ақпан',
	m2: 'Наурыз',
	m3: 'Сәуір',
	m4: 'Мамыр',
	m5: 'Маусым',
	m6: 'Шілде',
	m7: 'Тамыз',
	m8: 'Қыркүйек',
	m9: 'Қазан',
	m10: 'Қараша',
	m11: 'Желтоқсан',

	ms0: 'Қаң',
	ms1: 'Ақп',
	ms2: 'Нау',
	ms3: 'Сәу',
	ms4: 'Мамыр',
	ms5: 'Мау',
	ms6: 'Шіл',
	ms7: 'Там',
	ms8: 'Қыр',
	ms9: 'Қаз',
	ms10: 'Қар',
	ms11: 'Жел',

	d0: 'Жексенбі',
	d1: 'Дүйсенбі',
	d2: 'Сейсенбі',
	d3: 'Сәрсенбі',
	d4: 'Бейсенбі',
	d5: 'Жұма',
	d6: 'Сенбі',

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
