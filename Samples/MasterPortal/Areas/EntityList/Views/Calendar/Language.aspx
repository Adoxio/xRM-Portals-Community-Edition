<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<System.Globalization.CultureInfo>" %>
<%@ Import Namespace="System.Globalization" %>
<%@ Import Namespace="Adxstudio.Xrm.Resources" %>
<% Response.ContentType = "text/javascript"; %>
<% Func<string, string> jsString = value => HttpUtility.JavaScriptStringEncode(value ?? string.Empty, true); %>
<% Func<string, string> jsStringResource = key => jsString(ResourceManager.GetString(key)); %>
if(!window.calendar_languages) {
  window.calendar_languages = {};
}

window.calendar_languages[<%= jsString(Model.ToString()) %>] = {
  error_noview: <%= jsStringResource("EntityList_Calendar_NoView") %>,
  error_dateformat: <%= jsStringResource("EntityList_Calendar_DateFormat") %>,
  error_loadurl: <%= jsStringResource("EntityList_Calendar_LoadUrl") %>,
  error_where: <%= jsStringResource("EntityList_Calendar_Where") %>,
  error_timedevide: <%= jsStringResource("EntityList_Calendar_TimeDivide") %>,
  no_events_in_day: <%= jsStringResource("EntityList_Calendar_No_Events_In_Day") %>,
  title_year: <%= jsStringResource("EntityList_Calendar_Title_Year") %>,
  title_month: <%= jsStringResource("EntityList_Calendar_Title_Month") %>,
  title_week: <%= jsStringResource("EntityList_Calendar_Title_Week") %>,
  title_day: <%= jsStringResource("EntityList_Calendar_Title_Day") %>,
  week: <%= jsStringResource("EntityList_Calendar_Week") %>,
  all_day: <%= jsStringResource("EntityList_Calendar_All_Day") %>,
  time: <%= jsStringResource("EntityList_Calendar_Time") %>,
  events: <%= jsStringResource("EntityList_Calendar_Events") %>,
  before_time: <%= jsStringResource("EntityList_Calendar_Before_Time") %>,
  after_time: <%= jsStringResource("EntityList_Calendar_After_Time") %>,

<% for (var i = 0; i < Model.DateTimeFormat.MonthNames.Length; i++) { %>
  m<%= i.ToString(CultureInfo.InvariantCulture) %>: <%= jsString(Model.DateTimeFormat.MonthNames[i]) %>,
<% } %>

<% for (var i = 0; i < Model.DateTimeFormat.AbbreviatedMonthNames.Length; i++) { %>
  ms<%= i.ToString(CultureInfo.InvariantCulture) %>: <%= jsString(Model.DateTimeFormat.AbbreviatedMonthNames[i]) %>,
<% } %>

<% for (var i = 0; i < Model.DateTimeFormat.DayNames.Length; i++) { %>
  d<%= i.ToString(CultureInfo.InvariantCulture) %>: <%= jsString(Model.DateTimeFormat.DayNames[i]) %>,
<% } %>

  first_day: <%= ((int)Model.DateTimeFormat.FirstDayOfWeek).ToString(CultureInfo.InvariantCulture) %>
};
