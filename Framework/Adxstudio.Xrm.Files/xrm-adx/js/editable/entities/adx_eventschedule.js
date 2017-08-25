/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/


XRM.onActivate(function () {

	var ns = XRM.namespace('editable.Entity.handlers');
	var Entity = XRM.editable.Entity;
	var Handler = Entity.Handler;
	var $ = XRM.jQuery;

	XRM.localizations['entity.create.adx_eventschedule.label'] = window.ResourceManager['Entity_Create_ADX_EventSchedule_Label'];
	XRM.localizations['entity.create.adx_eventschedule.tooltip'] = window.ResourceManager['Entity_Create_ADX_EventSchedule_Tooltip'];
	XRM.localizations['adx_eventschedule.recurrence'] = window.ResourceManager['ADX_EventSchedule_Recurrence'];
	XRM.localizations['adx_eventschedule.acceptabledays'] = window.ResourceManager['ADX_EventSchedule_AcceptableDays'];
	XRM.localizations['adx_eventschedule.shortname'] = window.ResourceManager['ADX_EventSchedule_ShortName'];
	XRM.localizations['adx_eventschedule.update.tooltip'] = window.ResourceManager['ADX_EventSchedule_Update_Tooltip'];
	XRM.localizations['entity.delete.adx_eventschedule.tooltip'] = window.ResourceManager['Entity_Delete_ADX_EventSchedule_Tooltip'];

	var self = ns.adx_eventschedule = function (entityContainer, toolbar) {
	};

	self.formPrototype = {
		uri: null,
		urlServiceUri: null,
		urlServiceUriTemplate: null,
		title: null,
		entityName: 'adx_eventschedule',
		reload: true,
		fields: [
		  { name: 'adx_name', label: window.ResourceManager['Name_Label'], type: 'text', required: true	  },
		  { name: 'adx_starttime', label: window.ResourceManager['Start_Time_Label'], type: 'datetime', required: true  },
		  { name: 'adx_endtime', label: window.ResourceManager['End_Time_Label'], type: 'datetime', required: true	  },
		  { name: 'adx_publishingstateid', label: window.ResourceManager['Publishing_State_Label'], type: 'select', required: true, excludeEmptyData: true, uri: null, optionEntityName: 'adx_publishingstate', optionText: 'adx_name', optionValue: 'adx_publishingstateid'  },
		  { name: 'adx_alldayevent', label: window.ResourceManager['All_Day_Event_Label'], type: 'checkbox'	  },
		  { name: 'adx_recurrence', label: window.ResourceManager['Recurrence_Label'], type: 'picklist', required: true	  },
		  { name: 'adx_week', label: window.ResourceManager['Week_Label'], type: 'picklist'  },
		  { name: 'adx_recurrenceenddate', label: window.ResourceManager['Recurrence_End_Time_Label'], type: 'datetime', excludeEmptyData: true	  },
		  { name: 'adx_maxrecurrences', label: window.ResourceManager['Max_Recurrences_Label'], type: 'integer'  },
		  { name: 'adx_interval', label: window.ResourceManager['Interval_Label'], type: 'integer'  },
		  { name: 'acceptabledays', type: 'instructions' },
		  { name: 'adx_sunday', label: window.ResourceManager['Sunday_Label'], type: 'checkbox', checkedByDefault: true	  },
		  { name: 'adx_monday', label: window.ResourceManager['Monday_Label'], type: 'checkbox', checkedByDefault: true	  },
		  { name: 'adx_tuesday', label: window.ResourceManager['Tuesday_Label'], type: 'checkbox', checkedByDefault: true  },
		  { name: 'adx_wednesday', label: window.ResourceManager['Wednesday_Label'], type: 'checkbox', checkedByDefault: true  },
		  { name: 'adx_thursday', label: window.ResourceManager['Thursday_Label'], type: 'checkbox', checkedByDefault: true	  },
		  { name: 'adx_friday', label: window.ResourceManager['Friday_Label'], type: 'checkbox', checkedByDefault: true	  },
		  { name: 'adx_saturday', label: window.ResourceManager['Saturday_Label'], type: 'checkbox', checkedByDefault: true	  }
		],
		layout: {
			cssClass: 'xrm-dialog-3column',
			columns: [
			  { cssClass: 'xrm-dialog-column1', fields: ['adx_name', 'adx_starttime', 'adx_endtime', 'adx_publishingstateid', 'adx_alldayevent'] },
			  { cssClass: 'xrm-dialog-column2', fields: ['adx_recurrence', 'adx_week', 'adx_recurrenceenddate', 'adx_maxrecurrences', 'adx_interval'] },
			  { cssClass: 'xrm-dialog-column3', fields: ['acceptabledays', 'adx_sunday', 'adx_monday', 'adx_tuesday', 'adx_wednesday', 'adx_thursday', 'adx_friday', 'adx_saturday'] }
			]
		}
	};

	self.getForm = function (entityContainer, options) {
		return Handler.getForm(self.formPrototype, entityContainer, options);
	};

});
