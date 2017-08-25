/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

Type.registerNamespace('Microsoft.Crm.Client.Core.Framework');

Microsoft.Crm.Client.Core.Framework.Trace = function Microsoft_Crm_Client_Core_Framework_Trace() {
}
Microsoft.Crm.Client.Core.Framework.Trace.logWarning = function Microsoft_Crm_Client_Core_Framework_Trace$logWarning(component, format) {
}
Microsoft.Crm.Client.Core.Framework.Trace.prototype = {
    
    executeLogWarning: function Microsoft_Crm_Client_Core_Framework_Trace$executeLogWarning(component, message) {
        Microsoft.Crm.Client.Core.Framework.Trace.logWarning(component, message);
    }
}


Microsoft.Crm.Client.Core.Framework.UserDateTimeUtils = function Microsoft_Crm_Client_Core_Framework_UserDateTimeUtils() {
}
Microsoft.Crm.Client.Core.Framework.UserDateTimeUtils.getDSTAdjustment = function Microsoft_Crm_Client_Core_Framework_UserDateTimeUtils$getDSTAdjustment(time, adjusters) {
    if (_Script.isNullOrUndefined(adjusters)) {
        return 0;
    }
    var timeAdjuster = null;
    var timeDateOnly = new Date(time.getTime());
    timeDateOnly.setHours(0);
    timeDateOnly.setMinutes(0);
    timeDateOnly.setSeconds(0);
    timeDateOnly.setMilliseconds(0);
    for (var i = 0; i < adjusters.length; i++) {
        var start = adjusters[i].get_dateStart();
        var end = adjusters[i].get_dateEnd();
        if (timeDateOnly >= start && timeDateOnly <= end) {
            timeAdjuster = adjusters[i];
            break;
        }
    }
    return 0;
}
Microsoft.Crm.Client.Core.Framework.UserDateTimeUtils.convertDateTimeFromUtcToUserDisplay = function Microsoft_Crm_Client_Core_Framework_UserDateTimeUtils$convertDateTimeFromUtcToUserDisplay(time, userUtcOffsetMinutes, adjusters) {
    if (_Script.isNullOrUndefined(time)) {
        return time;
    }
    var userUtcOffsetMilliseconds = userUtcOffsetMinutes * Microsoft.Crm.Client.Core.Framework.UserDateTimeUtils._millisecondsInMinute$p;
    userUtcOffsetMilliseconds += Microsoft.Crm.Client.Core.Framework.UserDateTimeUtils.getDSTAdjustment(time, adjusters);
    var systemUtcOffsetMilliseconds = time.getTimezoneOffset() * Microsoft.Crm.Client.Core.Framework.UserDateTimeUtils._millisecondsInMinute$p;
    return new Date(time.getTime() + userUtcOffsetMilliseconds + systemUtcOffsetMilliseconds);
}
Microsoft.Crm.Client.Core.Framework.UserDateTimeUtils.getUtcEquivalentFromLocal = function Microsoft_Crm_Client_Core_Framework_UserDateTimeUtils$getUtcEquivalentFromLocal(time) {
    return new Date(time.getTime() + (time.getTimezoneOffset() * Microsoft.Crm.Client.Core.Framework.UserDateTimeUtils._millisecondsInMinute$p));
}
Microsoft.Crm.Client.Core.Framework.UserDateTimeUtils.convertTimeToBehaviorDisplay = function Microsoft_Crm_Client_Core_Framework_UserDateTimeUtils$convertTimeToBehaviorDisplay(time, userUtcOffsetMinutes, behavior, adjusters) {
    switch (behavior) {
        case Microsoft.Crm.Client.Core.Framework.DateTimeFieldBehavior.dateOnly:
        case Microsoft.Crm.Client.Core.Framework.DateTimeFieldBehavior.timeZoneIndependent:
            return Microsoft.Crm.Client.Core.Framework.UserDateTimeUtils.getUtcEquivalentFromLocal(time);
        case Microsoft.Crm.Client.Core.Framework.DateTimeFieldBehavior.userLocal:
            return Microsoft.Crm.Client.Core.Framework.UserDateTimeUtils.convertDateTimeFromUtcToUserDisplay(time, userUtcOffsetMinutes, adjusters);
        default:
            return new Date(time.getTime());
    }
}
Microsoft.Crm.Client.Core.Framework.UserDateTimeUtils.prototype = {
    
    formatTimeToBehaviorDisplay: function Microsoft_Crm_Client_Core_Framework_UserDateTimeUtils$formatTimeToBehaviorDisplay(time, userUtcOffsetMinutes, behavior, adjusters) {
        return Microsoft.Crm.Client.Core.Framework.UserDateTimeUtils.convertTimeToBehaviorDisplay(time, userUtcOffsetMinutes, behavior, adjusters);
    }
}


Type.registerNamespace('Microsoft.Crm.Client.Core.Storage.Common');

Microsoft.Crm.Client.Core.Storage.Common.EntityMetadata = function Microsoft_Crm_Client_Core_Storage_Common_EntityMetadata() {
}
Microsoft.Crm.Client.Core.Storage.Common.EntityMetadata.prototype = {
    _$$pf_Id$p$0: null,
    
    get_id: function Microsoft_Crm_Client_Core_Storage_Common_EntityMetadata$get_id() {
        return this._$$pf_Id$p$0;
    },
    
    set_id: function Microsoft_Crm_Client_Core_Storage_Common_EntityMetadata$set_id(value) {
        this._$$pf_Id$p$0 = value;
        return value;
    },
    
    _$$pf_LogicalName$p$0: null,
    
    get_logicalName: function Microsoft_Crm_Client_Core_Storage_Common_EntityMetadata$get_logicalName() {
        return this._$$pf_LogicalName$p$0;
    },
    
    set_logicalName: function Microsoft_Crm_Client_Core_Storage_Common_EntityMetadata$set_logicalName(value) {
        this._$$pf_LogicalName$p$0 = value;
        return value;
    },
    
    _$$pf_DisplayName$p$0: null,
    
    get_displayName: function Microsoft_Crm_Client_Core_Storage_Common_EntityMetadata$get_displayName() {
        return this._$$pf_DisplayName$p$0;
    },
    
    set_displayName: function Microsoft_Crm_Client_Core_Storage_Common_EntityMetadata$set_displayName(value) {
        this._$$pf_DisplayName$p$0 = value;
        return value;
    },
    
    _$$pf_PluralName$p$0: null,
    
    get_pluralName: function Microsoft_Crm_Client_Core_Storage_Common_EntityMetadata$get_pluralName() {
        return this._$$pf_PluralName$p$0;
    },
    
    set_pluralName: function Microsoft_Crm_Client_Core_Storage_Common_EntityMetadata$set_pluralName(value) {
        this._$$pf_PluralName$p$0 = value;
        return value;
    },
    
    _$$pf_ObjectTypeCode$p$0: 0,
    
    get_objectTypeCode: function Microsoft_Crm_Client_Core_Storage_Common_EntityMetadata$get_objectTypeCode() {
        return this._$$pf_ObjectTypeCode$p$0;
    },
    
    set_objectTypeCode: function Microsoft_Crm_Client_Core_Storage_Common_EntityMetadata$set_objectTypeCode(value) {
        this._$$pf_ObjectTypeCode$p$0 = value;
        return value;
    },
    
    _$$pf_PrimaryIdAttribute$p$0: null,
    
    get_primaryIdAttribute: function Microsoft_Crm_Client_Core_Storage_Common_EntityMetadata$get_primaryIdAttribute() {
        return this._$$pf_PrimaryIdAttribute$p$0;
    },
    
    set_primaryIdAttribute: function Microsoft_Crm_Client_Core_Storage_Common_EntityMetadata$set_primaryIdAttribute(value) {
        this._$$pf_PrimaryIdAttribute$p$0 = value;
        return value;
    },
    
    _$$pf_PrimaryNameAttribute$p$0: null,
    
    get_primaryNameAttribute: function Microsoft_Crm_Client_Core_Storage_Common_EntityMetadata$get_primaryNameAttribute() {
        return this._$$pf_PrimaryNameAttribute$p$0;
    },
    
    set_primaryNameAttribute: function Microsoft_Crm_Client_Core_Storage_Common_EntityMetadata$set_primaryNameAttribute(value) {
        this._$$pf_PrimaryNameAttribute$p$0 = value;
        return value;
    },
    
    _$$pf_EntityColor$p$0: null,
    
    get_entityColor: function Microsoft_Crm_Client_Core_Storage_Common_EntityMetadata$get_entityColor() {
        return this._$$pf_EntityColor$p$0;
    },
    
    set_entityColor: function Microsoft_Crm_Client_Core_Storage_Common_EntityMetadata$set_entityColor(value) {
        this._$$pf_EntityColor$p$0 = value;
        return value;
    },
    
    getObjectData: function Microsoft_Crm_Client_Core_Storage_Common_EntityMetadata$getObjectData() {
        var data = {};
        data['Id'] = this.get_id();
        data['logicalName'] = this.get_logicalName();
        data['displayName'] = this.get_displayName();
        data['pluralName'] = this.get_pluralName();
        data['objectTypeCode'] = this.get_objectTypeCode();
        data['primaryIdAttribute'] = this.get_primaryIdAttribute();
        data['primrayNameAttribute'] = this.get_primaryNameAttribute();
        data['entityColor'] = this.get_entityColor();
        return data;
    }
}


Type.registerNamespace('Microsoft.Crm.Client.Core.Storage.Common.ObjectModel');

Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.ApplicationMetadata = function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_ApplicationMetadata() {
}
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.ApplicationMetadata.prototype = {
    _$$pf_DisplayName$p$0: null,
    
    get_displayName: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_ApplicationMetadata$get_displayName() {
        return this._$$pf_DisplayName$p$0;
    },
    
    set_displayName: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_ApplicationMetadata$set_displayName(value) {
        this._$$pf_DisplayName$p$0 = value;
        return value;
    },
    
    _$$pf_AssociatedEntityLogicalName$p$0: null,
    
    get_associatedEntityLogicalName: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_ApplicationMetadata$get_associatedEntityLogicalName() {
        return this._$$pf_AssociatedEntityLogicalName$p$0;
    },
    
    set_associatedEntityLogicalName: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_ApplicationMetadata$set_associatedEntityLogicalName(value) {
        this._$$pf_AssociatedEntityLogicalName$p$0 = value;
        return value;
    }
}


Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.ChartingAttributeMetaData = function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_ChartingAttributeMetaData() {
    this._attributeMetaDataDict$p$0 = {};
}
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.ChartingAttributeMetaData.prototype = {
    _attributeMetaDataDict$p$0: null,
    _$$pf_DisplayName$p$0: null,
    
    get_displayName: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_ChartingAttributeMetaData$get_displayName() {
        return this._$$pf_DisplayName$p$0;
    },
    
    set_displayName: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_ChartingAttributeMetaData$set_displayName(value) {
        this._$$pf_DisplayName$p$0 = value;
        return value;
    },
    
    _$$pf_Behavior$p$0: 0,
    
    get_behavior: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_ChartingAttributeMetaData$get_behavior() {
        return this._$$pf_Behavior$p$0;
    },
    
    set_behavior: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_ChartingAttributeMetaData$set_behavior(value) {
        this._$$pf_Behavior$p$0 = value;
        return value;
    },
    
    _$$pf_Type$p$0: 0,
    
    get_type: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_ChartingAttributeMetaData$get_type() {
        return this._$$pf_Type$p$0;
    },
    
    set_type: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_ChartingAttributeMetaData$set_type(value) {
        this._$$pf_Type$p$0 = value;
        return value;
    },
    
    getChartingMetaDataSet: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_ChartingAttributeMetaData$getChartingMetaDataSet() {
        return this._attributeMetaDataDict$p$0;
    },
    
    getChartingMetaDataInDisplayOrder: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_ChartingAttributeMetaData$getChartingMetaDataInDisplayOrder() {
        return this._attributeMetaDataDict$p$0;
    },
    
    addToDictionary: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_ChartingAttributeMetaData$addToDictionary(data) {
        var key = data.get_value().toString();
        this._attributeMetaDataDict$p$0[key] = data;
    }
}


Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.ChartingMetaData = function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_ChartingMetaData() {
}
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.ChartingMetaData.prototype = {
    _$$pf_Label$p$0: null,
    
    get_label: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_ChartingMetaData$get_label() {
        return this._$$pf_Label$p$0;
    },
    
    set_label: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_ChartingMetaData$set_label(value) {
        this._$$pf_Label$p$0 = value;
        return value;
    },
    
    _$$pf_Value$p$0: 0,
    
    get_value: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_ChartingMetaData$get_value() {
        return this._$$pf_Value$p$0;
    },
    
    set_value: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_ChartingMetaData$set_value(value) {
        this._$$pf_Value$p$0 = value;
        return value;
    },
    
    _$$pf_Color$p$0: null,
    
    get_color: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_ChartingMetaData$get_color() {
        return this._$$pf_Color$p$0;
    },
    
    set_color: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_ChartingMetaData$set_color(value) {
        this._$$pf_Color$p$0 = value;
        return value;
    }
}


Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.ChartingMetaDataAggregator = function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_ChartingMetaDataAggregator() {
    this._aggregateDict$p$0 = {};
}
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.ChartingMetaDataAggregator.prototype = {
    _aggregateDict$p$0: null,
    
    getChartingAttributeMetadata: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_ChartingMetaDataAggregator$getChartingAttributeMetadata(entityLogicalName, attributeName) {
        var attributeDict = this._aggregateDict$p$0[entityLogicalName];
        return attributeDict[attributeName];
    },
    
    addToDictionary: function Microsoft_Crm_Client_Core_Storage_Common_ObjectModel_ChartingMetaDataAggregator$addToDictionary(entityLogicalName, data) {
        if (((entityLogicalName) in this._aggregateDict$p$0)) {
            var attributeDict = this._aggregateDict$p$0[entityLogicalName];
            var displayName = data.get_displayName();
            attributeDict[displayName] = data;
        }
        else {
            var attributeDict = {};
            attributeDict[data.get_displayName()] = data;
            this._aggregateDict$p$0[entityLogicalName] = attributeDict;
        }
    }
}


Type.registerNamespace('Microsoft.Crm.Client.Core.Storage.DataApi');

Microsoft.Crm.Client.Core.Storage.DataApi.RetrieveEntityMetadataDataSource = function Microsoft_Crm_Client_Core_Storage_DataApi_RetrieveEntityMetadataDataSource() {
    this.retrieveEntityMetadata = this.RetrieveEntityMetadata;
}
Microsoft.Crm.Client.Core.Storage.DataApi.RetrieveEntityMetadataDataSource.prototype = {
    _entityMetadataArray$p$0: null,
    
    addEntityMetadata: function Microsoft_Crm_Client_Core_Storage_DataApi_RetrieveEntityMetadataDataSource$addEntityMetadata(entityMetadataDict) {
        this._entityMetadataArray$p$0 = new Array(entityMetadataDict.length);
        for (var i = 0; i < entityMetadataDict.length; i++) {
            this._entityMetadataArray$p$0[i] = new Microsoft.Crm.Client.Core.Storage.Common.EntityMetadata();
            this._entityMetadataArray$p$0[i].set_id(entityMetadataDict[i]['Id']);
            this._entityMetadataArray$p$0[i].set_logicalName(entityMetadataDict[i]['LogicalName']);
            this._entityMetadataArray$p$0[i].set_displayName(entityMetadataDict[i]['DisplayName']);
            this._entityMetadataArray$p$0[i].set_pluralName(entityMetadataDict[i]['PluralName']);
            this._entityMetadataArray$p$0[i].set_objectTypeCode(entityMetadataDict[i]['ObjectTypeCode']);
            this._entityMetadataArray$p$0[i].set_primaryIdAttribute(entityMetadataDict[i]['PrimaryIdAttribute']);
            this._entityMetadataArray$p$0[i].set_primaryNameAttribute(entityMetadataDict[i]['PrimaryNameAttribute']);
            this._entityMetadataArray$p$0[i].set_entityColor(entityMetadataDict[i]['EntityColor']);
        }
    },
    
    RetrieveEntityMetadata: function Microsoft_Crm_Client_Core_Storage_DataApi_RetrieveEntityMetadataDataSource$RetrieveEntityMetadata(logicalName, context) {
        var deferred = Microsoft.Crm.Client.Core.Imported.MscrmComponents.deferredPromiseFactory(Microsoft.Crm.Client.Core.Storage.Common.IEntityMetadata, Microsoft.Crm.Client.Core.Framework.ErrorStatus);
        var metaDataDict = this._entityMetadataArray$p$0;
        for (var i = 0; i < metaDataDict.length; i++) {
            var metaData = metaDataDict[i];
            if (metaData.get_logicalName() === logicalName) {
                deferred.resolve(metaData);
            }
            else if (i === metaDataDict.length - 1) {
                var incorrectMetaData = Microsoft.Crm.Client.Core.Framework.ErrorStatus.fromErrorCodeOnly(Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.entityMetadataSyncFailedWithContinue);
                deferred.reject(incorrectMetaData);
            }
        }
        return deferred.promise();
    },
    
    retrieveMultipleEntityMetadata: function Microsoft_Crm_Client_Core_Storage_DataApi_RetrieveEntityMetadataDataSource$retrieveMultipleEntityMetadata(context) {
        return null;
    },
    
    retrieveMultipleAttributeMetadata: function Microsoft_Crm_Client_Core_Storage_DataApi_RetrieveEntityMetadataDataSource$retrieveMultipleAttributeMetadata(query, context) {
        return null;
    },
    
    getAttributeTransactionCurrencySymbol: function Microsoft_Crm_Client_Core_Storage_DataApi_RetrieveEntityMetadataDataSource$getAttributeTransactionCurrencySymbol(attributeMetadata, groupAttributeHasAggregate) {
        return '$';
    },
    
    _getMockedMetadata$p$0: function Microsoft_Crm_Client_Core_Storage_DataApi_RetrieveEntityMetadataDataSource$_getMockedMetadata$p$0() {
        var entityMetadata = new Microsoft.Crm.Client.Core.Storage.Common.EntityMetadata();
        entityMetadata.set_id(null);
        entityMetadata.set_logicalName('opportunity');
        entityMetadata.set_displayName('Opportunity');
        entityMetadata.set_pluralName('Opportunities');
        entityMetadata.set_objectTypeCode(3);
        entityMetadata.set_primaryIdAttribute('opportunityid');
        entityMetadata.set_primaryNameAttribute('name');
        entityMetadata.set_entityColor('#3E7239');
        return entityMetadata;
    }
}


Type.registerNamespace('Microsoft.Crm.Client.Core.Models');

Microsoft.Crm.Client.Core.Models.Model = function Microsoft_Crm_Client_Core_Models_Model() {
    this._fields$p$0 = {};
    this.getFormattedValue = this.GetFormattedValue;
    this.getValue = this.GetValue;
}
Microsoft.Crm.Client.Core.Models.Model.prototype = {
    _$$pf_Id$p$0: null,
    
    get_Id: function Microsoft_Crm_Client_Core_Models_Model$get_Id() {
        return this._$$pf_Id$p$0;
    },
    
    set_Id: function Microsoft_Crm_Client_Core_Models_Model$set_Id(value) {
        this._$$pf_Id$p$0 = value;
        return value;
    },
    
    _$$pf_ModelName$p$0: null,
    
    get_ModelName: function Microsoft_Crm_Client_Core_Models_Model$get_ModelName() {
        return this._$$pf_ModelName$p$0;
    },
    
    set_ModelName: function Microsoft_Crm_Client_Core_Models_Model$set_ModelName(value) {
        this._$$pf_ModelName$p$0 = value;
        return value;
    },
    
    _$$pf_ActionableModelName$p$0: null,
    
    get_actionableModelName: function Microsoft_Crm_Client_Core_Models_Model$get_actionableModelName() {
        return this._$$pf_ActionableModelName$p$0;
    },
    
    set_actionableModelName: function Microsoft_Crm_Client_Core_Models_Model$set_actionableModelName(value) {
        this._$$pf_ActionableModelName$p$0 = value;
        return value;
    },
    
    add_propertyChanged: function Microsoft_Crm_Client_Core_Models_Model$add_propertyChanged(value) {
    },
    
    remove_propertyChanged: function Microsoft_Crm_Client_Core_Models_Model$remove_propertyChanged(value) {
    },
    
    get_fieldNames: function Microsoft_Crm_Client_Core_Models_Model$get_fieldNames() {
        var fieldNames = new (Microsoft.Crm.Client.Core.Framework.List$1.$$(String))();
        var $$dict_2 = this._fields$p$0;
        for (var $$key_3 in $$dict_2) {
            var field = { key: $$key_3, value: $$dict_2[$$key_3] };
            fieldNames.add(field.key);
        }
        return fieldNames.toArray();
    },
    
    fillModel: function Microsoft_Crm_Client_Core_Models_Model$fillModel(fields) {
        this._fields$p$0 = fields;
    },
    
    getIEntityRecord: function Microsoft_Crm_Client_Core_Models_Model$getIEntityRecord() {
        return this;
    },
    
    GetValue: function Microsoft_Crm_Client_Core_Models_Model$GetValue(fieldName) {
        return this._fields$p$0[fieldName];
    },
    
    GetFormattedValue: function Microsoft_Crm_Client_Core_Models_Model$GetFormattedValue(fieldName) {
        return this._fields$p$0[fieldName];
    },
    
    SetValue: function Microsoft_Crm_Client_Core_Models_Model$SetValue(fieldName, value) {
        this._fields$p$0[fieldName] = value;
    },
    
    apcl: function Microsoft_Crm_Client_Core_Models_Model$apcl(propertyName, callback) {
    },
    
    rpcl: function Microsoft_Crm_Client_Core_Models_Model$rpcl(propertyName, callback) {
    }
}


Microsoft.Crm.Client.Core.Models.RecordCollectionModel = function Microsoft_Crm_Client_Core_Models_RecordCollectionModel() {
}
Microsoft.Crm.Client.Core.Models.RecordCollectionModel.prototype = {
    
    get_count: function Microsoft_Crm_Client_Core_Models_RecordCollectionModel$get_count() {
        return this.get_itemsAsList().get_Count();
    },
    
    _$$pf_ItemsAsList$p$0: null,
    
    get_itemsAsList: function Microsoft_Crm_Client_Core_Models_RecordCollectionModel$get_itemsAsList() {
        return this._$$pf_ItemsAsList$p$0;
    },
    
    set_itemsAsList: function Microsoft_Crm_Client_Core_Models_RecordCollectionModel$set_itemsAsList(value) {
        this._$$pf_ItemsAsList$p$0 = value;
        return value;
    },
    
    dispose: function Microsoft_Crm_Client_Core_Models_RecordCollectionModel$dispose() {
    }
}


Type.registerNamespace('Microsoft.Crm.Client.Core.ViewModels');

Microsoft.Crm.Client.Core.ViewModels.ChartConfigurableViewModel = function Microsoft_Crm_Client_Core_ViewModels_ChartConfigurableViewModel() {
}
Microsoft.Crm.Client.Core.ViewModels.ChartConfigurableViewModel.prototype = {
    _$$pf_IsInteractionCentricDashboard$p$0: false,
    
    get_isInteractionCentricDashboard: function Microsoft_Crm_Client_Core_ViewModels_ChartConfigurableViewModel$get_isInteractionCentricDashboard() {
        return this._$$pf_IsInteractionCentricDashboard$p$0;
    },
    
    set_isInteractionCentricDashboard: function Microsoft_Crm_Client_Core_ViewModels_ChartConfigurableViewModel$set_isInteractionCentricDashboard(value) {
        this._$$pf_IsInteractionCentricDashboard$p$0 = value;
        return value;
    },
    
    _$$pf_SecondaryGroupByAttributeName$p$0: null,
    
    get_secondaryGroupByAttributeName: function Microsoft_Crm_Client_Core_ViewModels_ChartConfigurableViewModel$get_secondaryGroupByAttributeName() {
        return this._$$pf_SecondaryGroupByAttributeName$p$0;
    },
    
    set_secondaryGroupByAttributeName: function Microsoft_Crm_Client_Core_ViewModels_ChartConfigurableViewModel$set_secondaryGroupByAttributeName(value) {
        this._$$pf_SecondaryGroupByAttributeName$p$0 = value;
        return value;
    },
    
    _$$pf_PrimaryModelName$p$0: null,
    
    get_primaryModelName: function Microsoft_Crm_Client_Core_ViewModels_ChartConfigurableViewModel$get_primaryModelName() {
        return this._$$pf_PrimaryModelName$p$0;
    },
    
    set_primaryModelName: function Microsoft_Crm_Client_Core_ViewModels_ChartConfigurableViewModel$set_primaryModelName(value) {
        this._$$pf_PrimaryModelName$p$0 = value;
        return value;
    },
    
    _$$pf_Title$p$0: null,
    
    get_title: function Microsoft_Crm_Client_Core_ViewModels_ChartConfigurableViewModel$get_title() {
        return this._$$pf_Title$p$0;
    },
    
    set_title: function Microsoft_Crm_Client_Core_ViewModels_ChartConfigurableViewModel$set_title(value) {
        this._$$pf_Title$p$0 = value;
        return value;
    },
    
    _$$pf_Colors$p$0: null,
    
    get_colors: function Microsoft_Crm_Client_Core_ViewModels_ChartConfigurableViewModel$get_colors() {
        return this._$$pf_Colors$p$0;
    },
    
    set_colors: function Microsoft_Crm_Client_Core_ViewModels_ChartConfigurableViewModel$set_colors(value) {
        this._$$pf_Colors$p$0 = value;
        return value;
    },
    
    _$$pf_XAxes$p$0: null,
    
    get_xAxes: function Microsoft_Crm_Client_Core_ViewModels_ChartConfigurableViewModel$get_xAxes() {
        return this._$$pf_XAxes$p$0;
    },
    
    set_xAxes: function Microsoft_Crm_Client_Core_ViewModels_ChartConfigurableViewModel$set_xAxes(value) {
        this._$$pf_XAxes$p$0 = value;
        return value;
    },
    
    _$$pf_YAxes$p$0: null,
    
    get_yAxes: function Microsoft_Crm_Client_Core_ViewModels_ChartConfigurableViewModel$get_yAxes() {
        return this._$$pf_YAxes$p$0;
    },
    
    set_yAxes: function Microsoft_Crm_Client_Core_ViewModels_ChartConfigurableViewModel$set_yAxes(value) {
        this._$$pf_YAxes$p$0 = value;
        return value;
    },
    
    _$$pf_SeriesList$p$0: null,
    
    get_seriesList: function Microsoft_Crm_Client_Core_ViewModels_ChartConfigurableViewModel$get_seriesList() {
        return this._$$pf_SeriesList$p$0;
    },
    
    set_seriesList: function Microsoft_Crm_Client_Core_ViewModels_ChartConfigurableViewModel$set_seriesList(value) {
        this._$$pf_SeriesList$p$0 = value;
        return value;
    },
    
    _$$pf_AllowPointSelect$p$0: false,
    
    get_allowPointSelect: function Microsoft_Crm_Client_Core_ViewModels_ChartConfigurableViewModel$get_allowPointSelect() {
        return this._$$pf_AllowPointSelect$p$0;
    },
    
    set_allowPointSelect: function Microsoft_Crm_Client_Core_ViewModels_ChartConfigurableViewModel$set_allowPointSelect(value) {
        this._$$pf_AllowPointSelect$p$0 = value;
        return value;
    },
    
    _$$pf_Legend$p$0: null,
    
    get_legend: function Microsoft_Crm_Client_Core_ViewModels_ChartConfigurableViewModel$get_legend() {
        return this._$$pf_Legend$p$0;
    },
    
    set_legend: function Microsoft_Crm_Client_Core_ViewModels_ChartConfigurableViewModel$set_legend(value) {
        this._$$pf_Legend$p$0 = value;
        return value;
    }
}


Type.registerNamespace('Portal.Charting');

Portal.Charting.ChartBuilder = function Portal_Charting_ChartBuilder() {}


Portal.Charting.ChartDefinition = function Portal_Charting_ChartDefinition() {}


Portal.Charting.PortalChartOrchestrator = function Portal_Charting_PortalChartOrchestrator() {
}
Portal.Charting.PortalChartOrchestrator.createChart = function Portal_Charting_PortalChartOrchestrator$createChart(chartBuilder) {
    var deferredWrapper = jQueryApi.jQueryDeferredFactory.Deferred(Object, Object);
    var attributeMetadataDict = Sys.Serialization.JavaScriptSerializer.deserialize(chartBuilder.AttributeMetadataSerialized);
    var entityMetadataDict = Sys.Serialization.JavaScriptSerializer.deserialize(chartBuilder.EntityMetadataSerialized);
    var resourceStrings = Sys.Serialization.JavaScriptSerializer.deserialize(chartBuilder.ResourceManagerStringOverridesSerialized);
    Microsoft.Crm.Client.Core.Framework.Common.ResourceManager.updateResources(resourceStrings);
    var length = attributeMetadataDict.length;
    var realAttributeMetadata = new Array(length);
    for (var i = 0; i < length; i++) {
        realAttributeMetadata[i] = Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.AttributeMetadata.createFromObjectData(attributeMetadataDict[i]);
    }
    var viewMetadata = new Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.ApplicationMetadata();
    viewMetadata.set_associatedEntityLogicalName(entityMetadataDict[0]['LogicalName']);
    var vizMetadata = new Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.ApplicationMetadata();
    vizMetadata.set_displayName(chartBuilder.ChartDefinition.Name);
    vizMetadata.set_associatedEntityLogicalName(entityMetadataDict[0]['LogicalName']);
    var chartQueryDecorator = new Microsoft.Crm.Client.Core.Models.Chart.ChartDataQueryDecorator(chartBuilder.ChartDefinition.DataDescriptionXml, chartBuilder.FetchXml);
    var airChartingDataSource = chartQueryDecorator.get_dataDefinition().get_chartingDataSource();
    airChartingDataSource.addEntityMetadata(entityMetadataDict);
    var entityMetadataDeferreds = chartQueryDecorator.get_dataDefinition().retrieveEntityMetadataDeferred();
    if (!entityMetadataDeferreds.length) {
        chartQueryDecorator.get_dataDefinition().setMetadata(null);
        var buildConfig = Portal.Charting.PortalChartOrchestrator._completeChartCreation$p(viewMetadata, vizMetadata, chartBuilder.DataJson, chartQueryDecorator, realAttributeMetadata, chartBuilder.ChartDefinition.PresentationDescriptionXml);
        deferredWrapper.resolve(buildConfig);
    }
    else {
        var deferredArray = new Array(entityMetadataDeferreds.length);
        for (var i = 0; i < entityMetadataDeferreds.length; i++) {
            deferredArray[i] = entityMetadataDeferreds[i];
        }
        var deferredObj = $.when.apply(null, deferredArray);
        deferredObj.then(function(results) {
            chartQueryDecorator.get_dataDefinition().setMetadata(arguments);
            var buildConfig = Portal.Charting.PortalChartOrchestrator._completeChartCreation$p(viewMetadata, vizMetadata, chartBuilder.DataJson, chartQueryDecorator, realAttributeMetadata, chartBuilder.ChartDefinition.PresentationDescriptionXml);
            deferredWrapper.resolve(buildConfig);
        }, function(status) {
            deferredWrapper.reject(status);
        });
    }
    return deferredWrapper;
}
Portal.Charting.PortalChartOrchestrator._completeChartCreation$p = function Portal_Charting_PortalChartOrchestrator$_completeChartCreation$p(viewMetadata, vizMetadata, dataJson, chartQueryDecorator, attributeMetadata, presentationXml) {
    chartQueryDecorator.setupAggregationQueries();
    var attributeQueryExpression = chartQueryDecorator.get_dataDefinition().get_entityExpression().get_attributes();
    var recordCollectionModel = Portal.Charting.PortalChartOrchestrator._buildRecordCollectionModel$p(dataJson, attributeQueryExpression);
    var chartBuilder = Microsoft.Crm.Client.Core.ViewModels.Controls.ChartBuilder.createChartBuilder(viewMetadata, vizMetadata, chartQueryDecorator.get_dataDefinition(), recordCollectionModel, presentationXml);
    var attributes = attributeMetadata;
    for (var i = 0; i < attributes.length; i++) {
        var metadataPair = chartQueryDecorator.get_dataDefinition().get_metadataPairCollection()[attributes[i].get_entityLogicalName()];
        if (attributes.length > 0 && !_Script.isNullOrUndefined(metadataPair)) {
            metadataPair.get_attributeMetadataCollection().mergeAttributeMetadata(attributes, false);
        }
    }
    var viewModelInfo = chartBuilder.buildChartModel();
    var chartConfigurableViewModel = Portal.Charting.PortalChartOrchestrator._populateChartConfigurableViewModel$p(viewModelInfo);
    var chartingMetaDataAggregator = Portal.Charting.PortalChartOrchestrator._createChartingMetaDataAggregator$p(chartQueryDecorator.get_dataDefinition());
    var trace = new Microsoft.Crm.Client.Core.Framework.Trace();
    var chartConfigObject = Microsoft.Crm.Client.Core.ViewModels.Controls.ChartConfigGenerator.generateConfigurationObject(chartConfigurableViewModel, chartingMetaDataAggregator, trace);
    Portal.Charting.PortalChartOrchestrator._modifyChartConfigObjectForAir$p(chartConfigObject);
    return chartConfigObject;
}
Portal.Charting.PortalChartOrchestrator._populateChartConfigurableViewModel$p = function Portal_Charting_PortalChartOrchestrator$_populateChartConfigurableViewModel$p(viewModelInfo) {
    var viewModel = new Microsoft.Crm.Client.Core.ViewModels.ChartConfigurableViewModel();
    viewModel.set_title(viewModelInfo.get_Title());
    viewModel.set_colors(viewModelInfo.get_Colors());
    viewModel.set_xAxes(viewModelInfo.get_XAxes());
    viewModel.set_yAxes(viewModelInfo.get_YAxes());
    viewModel.set_seriesList(viewModelInfo.get_SeriesList());
    viewModel.set_legend(viewModelInfo.get_Legend());
    viewModel.set_allowPointSelect(false);
    viewModel.set_primaryModelName(null);
    viewModel.set_secondaryGroupByAttributeName(null);
    viewModel.set_isInteractionCentricDashboard(false);
    return viewModel;
}
Portal.Charting.PortalChartOrchestrator._createChartingMetaDataAggregator$p = function Portal_Charting_PortalChartOrchestrator$_createChartingMetaDataAggregator$p(dataDefinition) {
    var metaDataAgg = new Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.ChartingMetaDataAggregator();
    for (var i = 0; i < dataDefinition.get_categoryColumns().get_Count(); i++) {
        var column = dataDefinition.get_categoryColumns().get_item(i);
        var attributes = column.get_entity().get_metadataPair().get_attributeMetadataCollection();
        for (var $$arr_5 = attributes.get_attributes(), $$len_6 = $$arr_5.length, $$idx_7 = 0; $$idx_7 < $$len_6; ++$$idx_7) {
            var attributeMetadata = $$arr_5[$$idx_7];
            var chartingAttrMetaData = new Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.ChartingAttributeMetaData();
            chartingAttrMetaData.set_behavior(attributeMetadata.get_behavior());
            chartingAttrMetaData.set_displayName(attributeMetadata.get_displayName());
            chartingAttrMetaData.set_type(attributeMetadata.get_type());
            if (attributeMetadata.get_optionSet()) {
                var $$enum_C = attributeMetadata.get_optionSet().get_options().getEnumerator();
                while ($$enum_C.moveNext()) {
                    var optionSet = $$enum_C.get_current();
                    var chartingMetaData = new Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.ChartingMetaData();
                    chartingMetaData.set_color(optionSet.get_color());
                    chartingMetaData.set_label(optionSet.get_label());
                    chartingMetaData.set_value(optionSet.get_value());
                    chartingAttrMetaData.addToDictionary(chartingMetaData);
                }
            }
            metaDataAgg.addToDictionary(attributes.get_associatedEntityLogicalName(), chartingAttrMetaData);
        }
    }
    return metaDataAgg;
}
Portal.Charting.PortalChartOrchestrator._buildRecordCollectionModel$p = function Portal_Charting_PortalChartOrchestrator$_buildRecordCollectionModel$p(dataJson, attributeQueryExpression) {
    var recordCollectionModel = new Microsoft.Crm.Client.Core.Models.RecordCollectionModel();
    recordCollectionModel.set_itemsAsList(new (Microsoft.Crm.Client.Core.Framework.List$1.$$(Microsoft.Crm.Client.Core.Models.IModel))());
    var unusedPrefixOfJson = 8;
    dataJson = dataJson.substring(unusedPrefixOfJson, dataJson.length);
    var parsedDataJson = JSON.parse(dataJson);
    for (var i = 0; i < parsedDataJson.length; i++) {
        var model = new Microsoft.Crm.Client.Core.Models.Model();
        var jsonObject = parsedDataJson[i];
        for (var $$arr_8 = attributeQueryExpression, $$len_9 = $$arr_8.length, $$idx_A = 0; $$idx_A < $$len_9; ++$$idx_A) {
            var attribute = $$arr_8[$$idx_A];
            if (attribute.get_aliasName() && !((attribute.get_aliasName()) in jsonObject) && jsonObject[attribute.get_name()]) {
                var value = jsonObject[attribute.get_name()].toString();
                jsonObject[attribute.get_aliasName()] = value;
            }
        }
        model.fillModel(jsonObject);
        var keys = null;
        keys = [];
        for (var key in jsonObject) { if (jsonObject.hasOwnProperty(key)) { keys.push(key) } };
        var extraValueLength = 6;
        for (var j = 0; j < keys.length; j++) {
            if (keys[j].endsWith('_Value')) {
                var newKey = keys[j].substring(0, keys[j].length - extraValueLength);
                jsonObject[newKey + 'undefinedRaw'] = jsonObject[keys[j]];
            }
        }
        recordCollectionModel.get_itemsAsList().add(model);
    }
    return recordCollectionModel;
}
Portal.Charting.PortalChartOrchestrator._modifyChartConfigObjectForAir$p = function Portal_Charting_PortalChartOrchestrator$_modifyChartConfigObjectForAir$p(chartConfigObject) {
    if (!_Script.isNullOrUndefined(chartConfigObject.series) && !_Script.isNullOrUndefined(chartConfigObject.series[0])) {
        if (chartConfigObject.series[0].type === 'column') {
            if (!_Script.isNullOrUndefined(chartConfigObject.xAxis) && !_Script.isNullOrUndefined(chartConfigObject.xAxis[0])) {
                var xAxis = chartConfigObject.xAxis[0];
                xAxis['tickAmount'] = 3;
                if (!_Script.isNullOrUndefined(xAxis['labels'])) {
                    var labels = xAxis['labels'];
                    labels['autoRotation'] = false;
                    labels['rotation'] = 0;
                    labels['align'] = 'center';
                }
            }
        }
        else {
            if (!_Script.isNullOrUndefined(chartConfigObject.yAxis) && !_Script.isNullOrUndefined(chartConfigObject.yAxis[0])) {
                var yAxis = chartConfigObject.yAxis[0];
                yAxis['tickAmount'] = 3;
                if (!_Script.isNullOrUndefined(yAxis['labels'])) {
                    var labels = yAxis['labels'];
                    labels['autoRotation'] = false;
                    labels['rotation'] = 0;
                    labels['align'] = 'center';
                }
            }
        }
        if (chartConfigObject.series[0].type === 'funnel') {
            var seriesDic = ((chartConfigObject.series[0]));
            seriesDic['neckWidth'] = '5%';
            seriesDic['neckHeight'] = '0%';
        }
        if (chartConfigObject.series[0].type === 'column' || chartConfigObject.series[0].type === 'bar') {
            if (!_Script.isNullOrUndefined(chartConfigObject.chart)) {
                chartConfigObject.chart.marginBottom -= 18;
            }
        }
    }
    if (!_Script.isNullOrUndefined(chartConfigObject.legend)) {
        chartConfigObject.legend['layout'] = 'horizontal';
        chartConfigObject.legend['align'] = 'center';
    }
    var exportingObj = {};
    exportingObj['enabled'] = false;
    (chartConfigObject)['exporting'] = exportingObj;
    var accessibilityObj = {};
    accessibilityObj['enabled'] = true;
    accessibilityObj['description'] = 'acc';
    (chartConfigObject)['accessibility'] = accessibilityObj;
}
Portal.Charting.PortalChartOrchestrator._changeColors$p = function Portal_Charting_PortalChartOrchestrator$_changeColors$p(chartConfigObject) {
    chartConfigObject.colors = ['#02B8AB', '#FD6260', '#384649', '#F1C80E', '#8ad9c4', '#f3c183', '#FF9655', '#FFF263', '#6AF9C4'];
}
Portal.Charting.PortalChartOrchestrator._removeBarColors$p = function Portal_Charting_PortalChartOrchestrator$_removeBarColors$p(chartConfigObject) {
    for (var $$arr_1 = chartConfigObject.series, $$len_2 = $$arr_1.length, $$idx_3 = 0; $$idx_3 < $$len_2; ++$$idx_3) {
        var hcs = $$arr_1[$$idx_3];
        hcs.color = null;
    }
}
Portal.Charting.PortalChartOrchestrator._changeChartTitleColor$p = function Portal_Charting_PortalChartOrchestrator$_changeChartTitleColor$p(chartConfigObject) {
    (chartConfigObject.title['style'])['color'] = '#00a3d9';
}


Microsoft.Crm.Client.Core.Framework.Trace.registerClass('Microsoft.Crm.Client.Core.Framework.Trace', null, Microsoft.Crm.Client.Core.Framework.ITrace);
Microsoft.Crm.Client.Core.Framework.UserDateTimeUtils.registerClass('Microsoft.Crm.Client.Core.Framework.UserDateTimeUtils', null, Microsoft.Crm.Client.Core.Framework.IUserDateTimeUtils);
Microsoft.Crm.Client.Core.Storage.Common.EntityMetadata.registerClass('Microsoft.Crm.Client.Core.Storage.Common.EntityMetadata', null, Microsoft.Crm.Client.Core.Storage.Common.IEntityMetadata, Microsoft.Crm.Client.Core.Framework.ISerializable);
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.ApplicationMetadata.registerClass('Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.ApplicationMetadata', null, Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.IApplicationMetadata);
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.ChartingAttributeMetaData.registerClass('Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.ChartingAttributeMetaData', null, Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.IChartingAttributeMetaData);
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.ChartingMetaData.registerClass('Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.ChartingMetaData', null, Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.IChartingMetaData);
Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.ChartingMetaDataAggregator.registerClass('Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.ChartingMetaDataAggregator', null, Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.IChartingMetaDataAggregator);
Microsoft.Crm.Client.Core.Storage.DataApi.RetrieveEntityMetadataDataSource.registerClass('Microsoft.Crm.Client.Core.Storage.DataApi.RetrieveEntityMetadataDataSource', null, Microsoft.Crm.Client.Core.Storage.DataApi.IRetrieveEntityMetadataDataSource, Microsoft.Crm.Client.Core.Storage.DataApi.IChartableUserContext);
Microsoft.Crm.Client.Core.Models.Model.registerClass('Microsoft.Crm.Client.Core.Models.Model', null, Microsoft.Crm.Client.Core.Models.IModel, Microsoft.Crm.Client.Core.Framework.IRootModel, Microsoft.Crm.Client.Core.Framework.INotifyPropertyChanged, Microsoft.Crm.Client.Core.Models.IEntityRecordContainer, Microsoft.Crm.Client.Core.Storage.Common.ObjectModel.IEntityRecord);
Microsoft.Crm.Client.Core.Models.RecordCollectionModel.registerClass('Microsoft.Crm.Client.Core.Models.RecordCollectionModel', null, Microsoft.Crm.Client.Core.Models.IRecordCollectionModel);
Microsoft.Crm.Client.Core.ViewModels.ChartConfigurableViewModel.registerClass('Microsoft.Crm.Client.Core.ViewModels.ChartConfigurableViewModel', null, Microsoft.Crm.Client.Core.ViewModels.IChartConfigurableViewModel);
Portal.Charting.ChartBuilder.registerClass('Portal.Charting.ChartBuilder');
Portal.Charting.ChartDefinition.registerClass('Portal.Charting.ChartDefinition');
Portal.Charting.PortalChartOrchestrator.registerClass('Portal.Charting.PortalChartOrchestrator');
Microsoft.Crm.Client.Core.Framework.UserDateTimeUtils._millisecondsInMinute$p = 60000;
//@ sourceMappingURL=.srcmap
