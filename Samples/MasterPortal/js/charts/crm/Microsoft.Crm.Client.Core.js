/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

Type.registerNamespace('Microsoft.Crm.Client.Core');

Microsoft.Crm.Client.Core.SharedScript = function Microsoft_Crm_Client_Core_SharedScript() {
}
Microsoft.Crm.Client.Core.SharedScript.load = function Microsoft_Crm_Client_Core_SharedScript$load(target, scriptUrl, importer) {
    if (IsNull(scriptUrl)) {
        throw Error.argumentNull('scriptUrl');
    }
    if (IsNull(importer)) {
        throw Error.argumentNull('importer');
    }
    var scriptId = scriptUrl.toLowerCase();
    var hostsToProbe = [ target.top, target.parent ];
    for (var i = 0; i < hostsToProbe.length; i++) {
        var hostWindow = hostsToProbe[i];
        if (!IsNull(hostWindow) && !IsNull(hostWindow.document) && hostWindow !== target.self) {
            var sharedScriptElement = hostWindow.document.getElementById(scriptId);
            if (!IsNull(sharedScriptElement)) {
                importer(hostWindow);
                return;
            }
        }
    }
    target.document.writeln('<script src=' + CrmEncodeDecode.CrmHtmlAttributeEncode(scriptUrl) + ' id=' + CrmEncodeDecode.CrmHtmlAttributeEncode(scriptId) + ' ></script>');
}


Type.registerNamespace('Microsoft.Crm.Client.Core.Framework');

Microsoft.Crm.Client.Core.Framework.ICustomControlAttribute = function() {}
Microsoft.Crm.Client.Core.Framework.ICustomControlAttribute.registerInterface('Microsoft.Crm.Client.Core.Framework.ICustomControlAttribute');


Microsoft.Crm.Client.Core.Framework.IOptionMetadata = function() {}
Microsoft.Crm.Client.Core.Framework.IOptionMetadata.registerInterface('Microsoft.Crm.Client.Core.Framework.IOptionMetadata');


Microsoft.Crm.Client.Core.Framework.IOptionSetMetadata = function() {}
Microsoft.Crm.Client.Core.Framework.IOptionSetMetadata.registerInterface('Microsoft.Crm.Client.Core.Framework.IOptionSetMetadata');


Microsoft.Crm.Client.Core.Framework.DayOfWeek = function() {}
Microsoft.Crm.Client.Core.Framework.DayOfWeek.prototype = {
    Sunday: 0, 
    Monday: 1, 
    Tuesday: 2, 
    Wednesday: 3, 
    Thursday: 4, 
    Friday: 5, 
    Saturday: 6
}
Microsoft.Crm.Client.Core.Framework.DayOfWeek.registerEnum('Microsoft.Crm.Client.Core.Framework.DayOfWeek', false);


Microsoft.Crm.Client.Core.Framework.ErrorSource = function() {}
Microsoft.Crm.Client.Core.Framework.ErrorSource.prototype = {
    unknown: 0, 
    authentication: 1, 
    localStore: 2, 
    metadataSync: 3
}
Microsoft.Crm.Client.Core.Framework.ErrorSource.registerEnum('Microsoft.Crm.Client.Core.Framework.ErrorSource', false);


Microsoft.Crm.Client.Core.Framework.PerformanceMarkerType = function() {}
Microsoft.Crm.Client.Core.Framework.PerformanceMarkerType.prototype = {
    undefined: 0, 
    majorEvent: 1, 
    localStore: 2
}
Microsoft.Crm.Client.Core.Framework.PerformanceMarkerType.registerEnum('Microsoft.Crm.Client.Core.Framework.PerformanceMarkerType', false);


Microsoft.Crm.Client.Core.Framework.IAuthenticationManager = function() {}
Microsoft.Crm.Client.Core.Framework.IAuthenticationManager.registerInterface('Microsoft.Crm.Client.Core.Framework.IAuthenticationManager');


Microsoft.Crm.Client.Core.Framework.AuthenticationState = function() {}
Microsoft.Crm.Client.Core.Framework.AuthenticationState.prototype = {
    initializing: 0, 
    ready: 1, 
    error: 2
}
Microsoft.Crm.Client.Core.Framework.AuthenticationState.registerEnum('Microsoft.Crm.Client.Core.Framework.AuthenticationState', false);


Microsoft.Crm.Client.Core.Framework.FormFactor = function() {}
Microsoft.Crm.Client.Core.Framework.FormFactor.prototype = {
    none: 0, 
    slate: 1, 
    phone: 2, 
    desktop: 3, 
    mailApp: 4
}
Microsoft.Crm.Client.Core.Framework.FormFactor.registerEnum('Microsoft.Crm.Client.Core.Framework.FormFactor', false);


Microsoft.Crm.Client.Core.Framework.Orientation = function() {}
Microsoft.Crm.Client.Core.Framework.Orientation.prototype = {
    none: 0, 
    portrait: 1, 
    landscape: 2
}
Microsoft.Crm.Client.Core.Framework.Orientation.registerEnum('Microsoft.Crm.Client.Core.Framework.Orientation', false);


Microsoft.Crm.Client.Core.Framework.ViewType = function() {}
Microsoft.Crm.Client.Core.Framework.ViewType.prototype = {
    none: 0, 
    multipleItem: 2, 
    singleItem: 3
}
Microsoft.Crm.Client.Core.Framework.ViewType.registerEnum('Microsoft.Crm.Client.Core.Framework.ViewType', false);


Microsoft.Crm.Client.Core.Framework.MailAppDisplayMode = function() {}
Microsoft.Crm.Client.Core.Framework.MailAppDisplayMode.prototype = {
    unknown: 0, 
    readDesktop: 1, 
    readTablet: 2, 
    readPhone: 3, 
    composeDesktop: 4, 
    composeTablet: 5, 
    composePhone: 6
}
Microsoft.Crm.Client.Core.Framework.MailAppDisplayMode.registerEnum('Microsoft.Crm.Client.Core.Framework.MailAppDisplayMode', false);


Microsoft.Crm.Client.Core.Framework.DateTimeFieldBehavior = function() {}
Microsoft.Crm.Client.Core.Framework.DateTimeFieldBehavior.prototype = {
    none: 0, 
    userLocal: 1, 
    dateOnly: 2, 
    timeZoneIndependent: 3
}
Microsoft.Crm.Client.Core.Framework.DateTimeFieldBehavior.registerEnum('Microsoft.Crm.Client.Core.Framework.DateTimeFieldBehavior', false);


Microsoft.Crm.Client.Core.Framework.MetadataType = function() {}
Microsoft.Crm.Client.Core.Framework.MetadataType.prototype = {
    none: 0, 
    workspace: 1, 
    form: 2, 
    component: 3, 
    entityCard: 4, 
    quickCreateForm: 5, 
    customization: 6, 
    listQuery: 7, 
    grid: 8, 
    command: 9, 
    siteMap: 10, 
    list: 11, 
    dialog: 12, 
    gridForm: 13, 
    inlineCreateForm: 14, 
    processControl: 15, 
    fetchXml: 16, 
    chartDrilldownForm: 17, 
    entity: 18, 
    attribute: 19, 
    webResource: 20, 
    applicationMetadata: 21, 
    quickEditForm: 22, 
    chart: 23, 
    uiStrings: 24, 
    businessLogic: 25, 
    searchPage: 26, 
    globalApplicationMetadata: 27, 
    applicationMetadataUserContext: 28, 
    staticJSFile: 29, 
    layoutXml: 30, 
    draftsPage: 31, 
    duplicateRecordsPage: 32, 
    applicationMetadataSettings: 33, 
    senderFormPage: 34, 
    setRegarding: 35, 
    senderCreateFormPage: 36, 
    interactionCentricWorkspace: 37, 
    taskBasedFlowGlobalMenu: 38, 
    taskBasedFlow: 39, 
    interactionCentricForm: 40, 
    cardForm: 41, 
    controlConfiguration: 42, 
    commandXaml: 43, 
    documentTemplate: 44, 
    processAction: 45, 
    recommendationModel: 46, 
    relevanceSearchPage: 47, 
    customControlTemplate: 48, 
    globalApplicationMetadataState: 49, 
    powerBIFullScreenPage: 50, 
    relationship: 52, 
    followPage: 53, 
    emailTemplatesPage: 54, 
    salesLiteraturePage: 55, 
    knowledgeBaseArticlesPage: 56
}
Microsoft.Crm.Client.Core.Framework.MetadataType.registerEnum('Microsoft.Crm.Client.Core.Framework.MetadataType', false);


Microsoft.Crm.Client.Core.Framework.ApplicationMetadataType = function() {}
Microsoft.Crm.Client.Core.Framework.ApplicationMetadataType.prototype = {
    all: 0, 
    systemApplicationMetadata: 1, 
    userApplicationMetadata: 2
}
Microsoft.Crm.Client.Core.Framework.ApplicationMetadataType.registerEnum('Microsoft.Crm.Client.Core.Framework.ApplicationMetadataType', false);


Microsoft.Crm.Client.Core.Framework.MetadataSubtype = function() {}
Microsoft.Crm.Client.Core.Framework.MetadataSubtype.prototype = {
    none: 0, 
    main: 1, 
    lookup: 2, 
    advancedFind: 3, 
    subGrid: 4, 
    jScript: 5, 
    css: 6, 
    png: 7, 
    jpg: 8, 
    gif: 9, 
    html: 10
}
Microsoft.Crm.Client.Core.Framework.MetadataSubtype.registerEnum('Microsoft.Crm.Client.Core.Framework.MetadataSubtype', false);


Microsoft.Crm.Client.Core.Framework.IAlias = function() {}
Microsoft.Crm.Client.Core.Framework.IAlias.registerInterface('Microsoft.Crm.Client.Core.Framework.IAlias');


Microsoft.Crm.Client.Core.Framework.IPicklistItem = function() {}
Microsoft.Crm.Client.Core.Framework.IPicklistItem.registerInterface('Microsoft.Crm.Client.Core.Framework.IPicklistItem');


Microsoft.Crm.Client.Core.Framework.IUserContext = function() {}
Microsoft.Crm.Client.Core.Framework.IUserContext.registerInterface('Microsoft.Crm.Client.Core.Framework.IUserContext');


Microsoft.Crm.Client.Core.Framework.IReference = function() {}
Microsoft.Crm.Client.Core.Framework.IReference.registerInterface('Microsoft.Crm.Client.Core.Framework.IReference');


Microsoft.Crm.Client.Core.Framework.ISerializable = function() {}
Microsoft.Crm.Client.Core.Framework.ISerializable.registerInterface('Microsoft.Crm.Client.Core.Framework.ISerializable');


Microsoft.Crm.Client.Core.Framework.IList$1 = function() {}
Microsoft.Crm.Client.Core.Framework.IList$1.$$ = function Microsoft_Crm_Client_Core_Framework_IList$1$$$(T) {
    var $$cn = 'IList$1' + '$' + T.getName().replace(/\./g, '_');
    if (!Microsoft.Crm.Client.Core.Framework[$$cn]) {
        var $$ccr = Microsoft.Crm.Client.Core.Framework[$$cn] = function() {
        };
        $$ccr.registerInterface('Microsoft.Crm.Client.Core.Framework.' + $$cn);
    }
    return Microsoft.Crm.Client.Core.Framework[$$cn];
}
Microsoft.Crm.Client.Core.Framework.IList$1.registerInterface('Microsoft.Crm.Client.Core.Framework.IList$1');


Microsoft.Crm.Client.Core.Framework.TraceComponent = function() {}
Microsoft.Crm.Client.Core.Framework.TraceComponent.prototype = {
    undefined: -1, 
    actionQueue: 0, 
    actions: 1, 
    addTrustedSenderResponseProcessor: 2, 
    animations: 3, 
    app: 4, 
    appCache: 5, 
    applyConversationAction: 6, 
    applyConversationActionResponseProcessor: 7, 
    attachmentsCleanupManager: 8, 
    attachmentViewModel: 9, 
    autodiscover: 10, 
    baseJsonResponseAction: 11, 
    binding: 12, 
    calendar: 13, 
    calendarActionsErrorHandling: 14, 
    calendarItems: 15, 
    calendarServiceCommandHelper: 16, 
    calendarShareMessageViewModel: 17, 
    calendarSharingInfoProviderViewModel: 18, 
    chat: 19, 
    chromeWebApp: 20, 
    clearConversationNextPredictedActionResponseProcessor: 21, 
    clearNextPredictedActionResponseProcessor: 22, 
    clientStore: 23, 
    conductor: 24, 
    connectionManager: 25, 
    controls: 26, 
    conversationItems: 27, 
    conversationListVM: 28, 
    conversations: 29, 
    core: 30, 
    createAttachmentAction: 31, 
    createAttachmentResponseProcessor: 32, 
    createAttachmentServiceCommand: 33, 
    createItemResponseProcessor: 34, 
    createItemServiceCommand: 35, 
    createPersonaResponseProcessor: 36, 
    deleteAttachmentResponseProcessor: 37, 
    deleteAttachmentServiceCommand: 38, 
    deleteFolderResponseProcessor: 39, 
    deleteItemResponseProcessor: 40, 
    deleteItemServiceCommand: 41, 
    deletePersonaResponseProcessor: 42, 
    discovery: 43, 
    droppableControl: 44, 
    emptyFolderResponseProcessor: 45, 
    errorHandler: 46, 
    extensibility: 47, 
    findConversationServiceCommand: 48, 
    findFolderServiceCommand: 49, 
    folders: 50, 
    framework: 51, 
    getCalendarFoldersServiceCommand: 52, 
    getConversationItemsServiceCommand: 53, 
    getFavoriteFolders: 54, 
    getFolderServiceCommand: 55, 
    getItemServiceCommand: 56, 
    grouping: 57, 
    hiddenAttachmentUpload: 58, 
    identityCorrelationTable: 59, 
    indexedDb: 60, 
    instrumentation: 61, 
    itemSynchronizer: 62, 
    listView: 63, 
    logDatapointResponseProcessor: 64, 
    mailBaseLVM: 65, 
    mailboxDataContext: 66, 
    mailCompose: 67, 
    mailComposeUpgrade: 68, 
    mailFolderItems: 69, 
    markAsJunkResponseProcessor: 70, 
    media: 71, 
    multiSelectListView: 72, 
    notifications: 73, 
    offlineMailboxDataContext: 74, 
    offlineManager: 75, 
    offlineNotifications: 76, 
    onlineProxy: 77, 
    openPALAttachment: 78, 
    owaResponseProcessors: 79, 
    pageListVM: 80, 
    PAL: 81, 
    palAttachmentDownloadManager: 82, 
    palAttachmentRenderer: 83, 
    performance: 84, 
    performReminderActionResponseProcessor: 85, 
    personaItems: 86, 
    placeItems: 87, 
    popOut: 88, 
    popOutMailboxDataContext: 89, 
    pushNotification: 90, 
    readingPane: 91, 
    reminders: 92, 
    requestQueueProcessor: 93, 
    responseProcessors: 94, 
    responseQueueProcessor: 95, 
    scheduling: 96, 
    securityPolicy: 97, 
    serviceCommand_CreatePersona: 98, 
    serviceCommand_FindItem: 99, 
    serviceCommand_GetOwaUserConfiguration: 100, 
    serviceCommand_GetReminders: 101, 
    serviceCommand_PerformReminderAction: 102, 
    serviceCommand_UpdateViewStateConfiguration: 103, 
    serviceCommands: 104, 
    shell: 105, 
    simpleVLV: 106, 
    singleDoc: 107, 
    speech: 108, 
    sql: 109, 
    sqlBatch: 110, 
    sqlDbTransactionAdapter: 111, 
    stackPanel: 112, 
    storage_CalendarItem: 113, 
    storage_Item: 114, 
    syncChangeUpdater: 115, 
    syncFolderSettingProcessor: 116, 
    syncManager: 117, 
    taskItems: 118, 
    taskRunner: 119, 
    timeZoneConverter: 120, 
    unitTest: 121, 
    updateCalendarItemServiceCommand: 122, 
    updateFolderResponseProcessor: 123, 
    updateItemResponseProcessor: 124, 
    updateItemServiceCommand: 125, 
    updatePersonaResponseProcessor: 126, 
    updateUserConfigurationResponseProcessor: 127, 
    views: 128, 
    viewStateConfiguration: 129, 
    virtualScrollbar: 130, 
    virtualScrollRegion: 131, 
    VLV: 132, 
    watson: 133, 
    webpart: 134, 
    webServices: 135, 
    mailModule: 136, 
    peopleModule: 137, 
    tasksModule: 138, 
    applicationBar: 139, 
    diagnosticsModule: 140, 
    location: 141, 
    mailTips: 142, 
    retentionPolicy: 143, 
    search: 144, 
    explicitLogon: 145, 
    optionsModule: 146, 
    findRecipient: 147, 
    linkPersona: 148, 
    personaCard: 149, 
    meCard: 150, 
    recipientWell: 151, 
    peoplePicker: 152, 
    playOnPhone: 153, 
    datePicker: 154, 
    timePicker: 155, 
    infoBar: 156, 
    dateTimePicker: 157, 
    managePassword: 158, 
    authentication: 159, 
    scriptErrorHandlerDialog: 160, 
    rootViewModel: 161, 
    draftsView: 162, 
    navigationCommand: 163, 
    telemetry: 164, 
    duplicateRecordsView: 165, 
    xrmInternals: 166, 
    performanceReport: 167, 
    unknown: 1000, 
    section: 1001, 
    workspace: 1002, 
    list: 1003, 
    application: 1004, 
    storage: 1005, 
    xmlNodeFactory: 1006, 
    dataSource: 1007, 
    localDataSource: 1008, 
    crmServerDataSource: 1009, 
    viewModelFactory: 1010, 
    listComponentViewModel: 1011, 
    openRecordCommand: 1012, 
    quickCreateForm: 1013, 
    pinnedTiles: 1014, 
    userPersonalization: 1015, 
    clientApi: 1016, 
    crmTileViewModel: 1017, 
    gridViewModel: 1018, 
    messageDialog: 1019, 
    userInput: 1020, 
    recordCollectionModel: 1021, 
    crmChartDrilldown: 1022, 
    basicMessageBar: 1023, 
    crmChartViewModel: 1024, 
    scheduler: 1025, 
    mruCache: 1026, 
    mashup: 1027, 
    dialog: 1028, 
    activeItemContainerViewModel: 1029, 
    offlineSyncErrorLog: 1030, 
    offlineDataStore: 1031, 
    offlineMetadataSync: 1032, 
    customControls: 1033, 
    taskBasedFlow: 1034, 
    offlineUpSync: 1035, 
    metadataSync: 1036, 
    webService: 1037, 
    formPreview: 1038, 
    userValidation: 1039, 
    interactionCentricDashboard: 2001, 
    interactionWall: 2002, 
    interactionWallEvent: 2003, 
    activityEntityInteractionWallSource: 2004, 
    noteEntityInteractionWallSource: 2005, 
    postEntityInteractionWallSource: 2006, 
    interactionCentricNavigationBar: 2007, 
    interactionCentricTimerControl: 2008, 
    lookupRelationshipFiltering: 2009
}
Microsoft.Crm.Client.Core.Framework.TraceComponent.registerEnum('Microsoft.Crm.Client.Core.Framework.TraceComponent', false);


Microsoft.Crm.Client.Core.Framework.CustomControlAttributeProperty = function Microsoft_Crm_Client_Core_Framework_CustomControlAttributeProperty() {
}
Microsoft.Crm.Client.Core.Framework.CustomControlAttributeProperty.getAttributeListByType = function Microsoft_Crm_Client_Core_Framework_CustomControlAttributeProperty$getAttributeListByType(type) {
    var property = new Microsoft.Crm.Client.Core.Framework.CustomControlAttributeProperty();
    return property.getListByType(type);
}
Microsoft.Crm.Client.Core.Framework.CustomControlAttributeProperty.prototype = {
    
    getListByType: function Microsoft_Crm_Client_Core_Framework_CustomControlAttributeProperty$getListByType(type) {
        switch (type.toLowerCase()) {
            case 'boolean':
            case 'picklist':
            case 'optionset':
            case 'twooptions':
                return this._getOptionSetProps$p$0();
            case 'string':
            case 'singleline':
                return this._getStringProps$p$0();
            case 'memo':
            case 'multiple':
                return this._getMemoProps$p$0();
            case 'decimal':
            case 'double':
            case 'money':
            case 'currency':
            case 'fp':
            case 'float':
                return this._getNumberProps$p$0();
            case 'integer':
            case 'bigint':
            case 'whole':
                return this._getWholeProps$p$0();
            case 'lookup':
            case 'owner':
            case 'partylist':
            case 'customer':
                return this._getLookupProps$p$0();
            case 'datetime':
            case 'dateandtime':
                return this._getDateTimeProps$p$0();
            default:
                return this._getBaseFieldProps$p$0();
        }
    },
    
    _getBaseFieldProps$p$0: function Microsoft_Crm_Client_Core_Framework_CustomControlAttributeProperty$_getBaseFieldProps$p$0() {
        return [ 'displayName', 'name', 'requiredLevel', 'isSecured', 'type', 'sourcetype' ];
    },
    
    _getBaseNumberProps$p$0: function Microsoft_Crm_Client_Core_Framework_CustomControlAttributeProperty$_getBaseNumberProps$p$0() {
        var $$t_0;
        return ($$t_0 = this._getBaseFieldProps$p$0()).concat.call($$t_0, 'minValue', 'maxValue', 'imeMode', Microsoft.Crm.Client.Core.Framework.CustomControlConstants.lastUpdatedField, Microsoft.Crm.Client.Core.Framework.CustomControlConstants.lastUpdatedValue, Microsoft.Crm.Client.Core.Framework.CustomControlConstants.rollupStateField, Microsoft.Crm.Client.Core.Framework.CustomControlConstants.rollupStateValue, Microsoft.Crm.Client.Core.Framework.CustomControlConstants.recalculate, Microsoft.Crm.Client.Core.Framework.CustomControlConstants.rollupValid, Microsoft.Crm.Client.Core.Framework.CustomControlConstants.calculatedFieldValid);
    },
    
    _getDateTimeProps$p$0: function Microsoft_Crm_Client_Core_Framework_CustomControlAttributeProperty$_getDateTimeProps$p$0() {
        var $$t_0;
        return ($$t_0 = this._getBaseFieldProps$p$0()).concat.call($$t_0, 'behavior', 'format', 'imeMode', Microsoft.Crm.Client.Core.Framework.CustomControlConstants.lastUpdatedField, Microsoft.Crm.Client.Core.Framework.CustomControlConstants.lastUpdatedValue, Microsoft.Crm.Client.Core.Framework.CustomControlConstants.rollupStateField, Microsoft.Crm.Client.Core.Framework.CustomControlConstants.rollupStateValue, Microsoft.Crm.Client.Core.Framework.CustomControlConstants.recalculate, Microsoft.Crm.Client.Core.Framework.CustomControlConstants.rollupValid, Microsoft.Crm.Client.Core.Framework.CustomControlConstants.calculatedFieldValid);
    },
    
    _getLookupProps$p$0: function Microsoft_Crm_Client_Core_Framework_CustomControlAttributeProperty$_getLookupProps$p$0() {
        var $$t_0;
        return ($$t_0 = this._getBaseFieldProps$p$0()).concat.call($$t_0, 'targets');
    },
    
    _getWholeProps$p$0: function Microsoft_Crm_Client_Core_Framework_CustomControlAttributeProperty$_getWholeProps$p$0() {
        var $$t_0;
        return ($$t_0 = this._getBaseNumberProps$p$0()).concat.call($$t_0, 'format');
    },
    
    _getNumberProps$p$0: function Microsoft_Crm_Client_Core_Framework_CustomControlAttributeProperty$_getNumberProps$p$0() {
        var $$t_0;
        return ($$t_0 = this._getBaseNumberProps$p$0()).concat.call($$t_0, 'precision');
    },
    
    _getStringProps$p$0: function Microsoft_Crm_Client_Core_Framework_CustomControlAttributeProperty$_getStringProps$p$0() {
        var $$t_0;
        return ($$t_0 = this._getBaseFieldProps$p$0()).concat.call($$t_0, 'maxLength', 'format', 'imeMode');
    },
    
    _getMemoProps$p$0: function Microsoft_Crm_Client_Core_Framework_CustomControlAttributeProperty$_getMemoProps$p$0() {
        var $$t_0;
        return ($$t_0 = this._getBaseFieldProps$p$0()).concat.call($$t_0, 'maxLength', 'imeMode');
    },
    
    _getOptionSetProps$p$0: function Microsoft_Crm_Client_Core_Framework_CustomControlAttributeProperty$_getOptionSetProps$p$0() {
        var $$t_0;
        return ($$t_0 = this._getBaseFieldProps$p$0()).concat.call($$t_0, 'options', 'defaultValue');
    }
}


Microsoft.Crm.Client.Core.Framework.CustomControlConstants = function Microsoft_Crm_Client_Core_Framework_CustomControlConstants() {
}


Microsoft.Crm.Client.Core.Framework.CustomControlUtils = function Microsoft_Crm_Client_Core_Framework_CustomControlUtils() {
}
Microsoft.Crm.Client.Core.Framework.CustomControlUtils.retrieveCorrespondingManifestType = function Microsoft_Crm_Client_Core_Framework_CustomControlUtils$retrieveCorrespondingManifestType(sDataType, sDataTypeFormat) {
    if (!Microsoft.Crm.Client.Core.Framework._String.isNullOrEmpty(sDataType)) {
        switch (sDataType) {
            case 'boolean':
                return Microsoft.Crm.Client.Core.Framework.ManifestType.twoOptions;
            case 'customer':
                return Microsoft.Crm.Client.Core.Framework.ManifestType.lookupCustomer;
            case 'datetime':
                switch (sDataTypeFormat) {
                    case 'date':
                        return Microsoft.Crm.Client.Core.Framework.ManifestType.dateAndTimeDateOnly;
                    case 'datetime':
                        return Microsoft.Crm.Client.Core.Framework.ManifestType.dateAndTimeDateAndTime;
                    default:
                        return Microsoft.Crm.Client.Core.Framework.ManifestType.dateAndTimeDateOnly;
                }
            case 'decimal':
                return Microsoft.Crm.Client.Core.Framework.ManifestType.decimal;
            case 'float':
                return Microsoft.Crm.Client.Core.Framework.ManifestType.FP;
            case 'integer':
                switch (sDataTypeFormat) {
                    case 'duration':
                        return Microsoft.Crm.Client.Core.Framework.ManifestType.wholeDuration;
                    case 'language':
                        return Microsoft.Crm.Client.Core.Framework.ManifestType.wholeLanguage;
                    case 'timezone':
                        return Microsoft.Crm.Client.Core.Framework.ManifestType.wholeTimeZone;
                    default:
                        return Microsoft.Crm.Client.Core.Framework.ManifestType.wholeNone;
                }
            case 'lookup':
                switch (sDataTypeFormat) {
                    case 'regarding':
                        return Microsoft.Crm.Client.Core.Framework.ManifestType.lookupRegarding;
                    default:
                        return Microsoft.Crm.Client.Core.Framework.ManifestType.lookupSimple;
                }
            case 'memo':
                return Microsoft.Crm.Client.Core.Framework.ManifestType.multiple;
            case 'money':
                return Microsoft.Crm.Client.Core.Framework.ManifestType.currency;
            case 'owner':
                return Microsoft.Crm.Client.Core.Framework.ManifestType.lookupOwner;
            case 'partylist':
                return Microsoft.Crm.Client.Core.Framework.ManifestType.lookupPartyList;
            case 'picklist':
                return Microsoft.Crm.Client.Core.Framework.ManifestType.optionSet;
            case 'text':
            case 'string':
                switch (sDataTypeFormat) {
                    case 'email':
                        return Microsoft.Crm.Client.Core.Framework.ManifestType.singleLineEmail;
                    case 'phone':
                        return Microsoft.Crm.Client.Core.Framework.ManifestType.singleLinePhone;
                    case 'text':
                        return Microsoft.Crm.Client.Core.Framework.ManifestType.singleLineText;
                    case 'textarea':
                        return Microsoft.Crm.Client.Core.Framework.ManifestType.singleLineTextArea;
                    case 'tickersymbol':
                        return Microsoft.Crm.Client.Core.Framework.ManifestType.singleLineTickerSymbol;
                    case 'url':
                        return Microsoft.Crm.Client.Core.Framework.ManifestType.singleLineURL;
                    default:
                        return Microsoft.Crm.Client.Core.Framework.ManifestType.singleLineText;
                }
        }
    }
    return Microsoft.Crm.Client.Core.Framework._String.empty;
}
Microsoft.Crm.Client.Core.Framework.CustomControlUtils.isLinkedValueOptionMetadata = function Microsoft_Crm_Client_Core_Framework_CustomControlUtils$isLinkedValueOptionMetadata(column) {
    var lowerDataType = column.get_dataType().toLowerCase();
    return lowerDataType === 'optionset' || lowerDataType === 'twooptions' || lowerDataType === 'picklist' || lowerDataType === 'boolean';
}
Microsoft.Crm.Client.Core.Framework.CustomControlUtils.getInternalDataValue = function Microsoft_Crm_Client_Core_Framework_CustomControlUtils$getInternalDataValue(publicValue, column) {
    if (!_Script.isNullOrUndefined(publicValue) && !(Microsoft.Crm.Client.Core.Framework.IPicklistItem.isInstanceOfType(publicValue)) && Microsoft.Crm.Client.Core.Framework.CustomControlUtils.isLinkedValueOptionMetadata(column)) {
        var optionsKey;
        if (Boolean.isInstanceOfType(publicValue)) {
            optionsKey = (publicValue) ? '1' : '0';
        }
        else {
            optionsKey = publicValue.toString();
        }
        return (column.get_unformattedAttributeData()['options']).get_options()[optionsKey];
    }
    return publicValue;
}
Microsoft.Crm.Client.Core.Framework.CustomControlUtils.getPublicDataValue = function Microsoft_Crm_Client_Core_Framework_CustomControlUtils$getPublicDataValue(internalRawValue, column) {
    var publicValue;
    if (Microsoft.Crm.Client.Core.Framework.IOptionMetadata.isInstanceOfType(internalRawValue)) {
        var value = internalRawValue;
        if (!_Script.isNullOrUndefined(value)) {
            publicValue = value.get_value();
        }
        else {
            publicValue = value;
        }
    }
    else {
        publicValue = internalRawValue;
    }
    return publicValue;
}
Microsoft.Crm.Client.Core.Framework.CustomControlUtils.isLinkedEntityColumn = function Microsoft_Crm_Client_Core_Framework_CustomControlUtils$isLinkedEntityColumn(columnName) {
    return columnName.indexOf('.') !== -1;
}
Microsoft.Crm.Client.Core.Framework.CustomControlUtils.formatProperties = function Microsoft_Crm_Client_Core_Framework_CustomControlUtils$formatProperties(data) {
    var formattedData = {};
    var $$dict_3 = data;
    for (var $$key_4 in $$dict_3) {
        var entry = { key: $$key_4, value: $$dict_3[$$key_4] };
        if (Microsoft.Crm.Client.Core.Framework.IOptionSetMetadata.isInstanceOfType(entry.value)) {
            formattedData[entry.key] = (entry.value).createSimpleForm();
        }
        else {
            formattedData[entry.key] = entry.value;
        }
    }
    return formattedData;
}
Microsoft.Crm.Client.Core.Framework.CustomControlUtils.extractUnformattedAttributeData = function Microsoft_Crm_Client_Core_Framework_CustomControlUtils$extractUnformattedAttributeData(original, formatted) {
    var raw = {};
    var $$dict_4 = original;
    for (var $$key_5 in $$dict_4) {
        var entry = { key: $$key_5, value: $$dict_4[$$key_5] };
        if (original[entry.key] !== formatted[entry.key]) {
            raw[entry.key] = entry.value;
        }
    }
    return raw;
}


Microsoft.Crm.Client.Core.Framework.DefaultContext = function Microsoft_Crm_Client_Core_Framework_DefaultContext(callerId, callerName) {
    Microsoft.Crm.Client.Core.Framework.DefaultContext.initializeBase(this, [ callerId, callerName ]);
}
Microsoft.Crm.Client.Core.Framework.DefaultContext.tryCreate = function Microsoft_Crm_Client_Core_Framework_DefaultContext$tryCreate(callerId, callerName) {
    if (Microsoft.Crm.Client.Core.Framework.CallingContext.get_isEnabled()) {
        return new Microsoft.Crm.Client.Core.Framework.DefaultContext(callerId, callerName);
    }
    return null;
}
Microsoft.Crm.Client.Core.Framework.DefaultContext.prototype = {
    
    toString: function Microsoft_Crm_Client_Core_Framework_DefaultContext$toString() {
        return '[' + this.get_callerName() + ']';
    }
}


Microsoft.Crm.Client.Core.Framework.ErrorData = function Microsoft_Crm_Client_Core_Framework_ErrorData(errorCode, message) {
    this.set_errorCode(errorCode);
    this.set_diagnosticMessage((!_Script.isNullOrUndefined(message)) ? message : Microsoft.Crm.Client.Core.Framework._String.empty);
}
Microsoft.Crm.Client.Core.Framework.ErrorData.prototype = {
    _message$p$0: null,
    _title$p$0: null,
    _okButtonText$p$0: null,
    _cancelButtonText$p$0: null,
    _$$pf_IsWarning$p$0: false,
    
    get_isWarning: function Microsoft_Crm_Client_Core_Framework_ErrorData$get_isWarning() {
        return this._$$pf_IsWarning$p$0;
    },
    
    set_isWarning: function Microsoft_Crm_Client_Core_Framework_ErrorData$set_isWarning(value) {
        this._$$pf_IsWarning$p$0 = value;
        return value;
    },
    
    _$$pf_ErrorCode$p$0: 0,
    
    get_errorCode: function Microsoft_Crm_Client_Core_Framework_ErrorData$get_errorCode() {
        return this._$$pf_ErrorCode$p$0;
    },
    
    set_errorCode: function Microsoft_Crm_Client_Core_Framework_ErrorData$set_errorCode(value) {
        this._$$pf_ErrorCode$p$0 = value;
        return value;
    },
    
    get_message: function Microsoft_Crm_Client_Core_Framework_ErrorData$get_message() {
        return this._message$p$0;
    },
    
    set_message: function Microsoft_Crm_Client_Core_Framework_ErrorData$set_message(value) {
        this._message$p$0 = value;
        return value;
    },
    
    get_title: function Microsoft_Crm_Client_Core_Framework_ErrorData$get_title() {
        return this._title$p$0;
    },
    
    set_title: function Microsoft_Crm_Client_Core_Framework_ErrorData$set_title(value) {
        this._title$p$0 = value;
        return value;
    },
    
    _$$pf_DiagnosticMessage$p$0: null,
    
    get_diagnosticMessage: function Microsoft_Crm_Client_Core_Framework_ErrorData$get_diagnosticMessage() {
        return this._$$pf_DiagnosticMessage$p$0;
    },
    
    set_diagnosticMessage: function Microsoft_Crm_Client_Core_Framework_ErrorData$set_diagnosticMessage(value) {
        this._$$pf_DiagnosticMessage$p$0 = value;
        return value;
    },
    
    _$$pf_StackTrace$p$0: null,
    
    get_stackTrace: function Microsoft_Crm_Client_Core_Framework_ErrorData$get_stackTrace() {
        return this._$$pf_StackTrace$p$0;
    },
    
    set_stackTrace: function Microsoft_Crm_Client_Core_Framework_ErrorData$set_stackTrace(value) {
        this._$$pf_StackTrace$p$0 = value;
        return value;
    },
    
    _$$pf_OkButtonText$p$0: null,
    
    get_okButtonText: function Microsoft_Crm_Client_Core_Framework_ErrorData$get_okButtonText() {
        return this._$$pf_OkButtonText$p$0;
    },
    
    set_okButtonText: function Microsoft_Crm_Client_Core_Framework_ErrorData$set_okButtonText(value) {
        this._$$pf_OkButtonText$p$0 = value;
        return value;
    },
    
    _$$pf_CancelButtonText$p$0: null,
    
    get_cancelButtonText: function Microsoft_Crm_Client_Core_Framework_ErrorData$get_cancelButtonText() {
        return this._$$pf_CancelButtonText$p$0;
    },
    
    set_cancelButtonText: function Microsoft_Crm_Client_Core_Framework_ErrorData$set_cancelButtonText(value) {
        this._$$pf_CancelButtonText$p$0 = value;
        return value;
    }
}


Microsoft.Crm.Client.Core.Framework.ErrorStatus = function Microsoft_Crm_Client_Core_Framework_ErrorStatus(message) {
    this.set_message(message);
    this.set_errorSource(0);
    this.set_errors(new Array(0));
}
Microsoft.Crm.Client.Core.Framework.ErrorStatus.fromMessage = function Microsoft_Crm_Client_Core_Framework_ErrorStatus$fromMessage(message) {
    var args = [];
    for (var $$pai_3 = 1; $$pai_3 < arguments.length; ++$$pai_3) {
        args[$$pai_3 - 1] = arguments[$$pai_3];
    }
    var errorMessage = (Microsoft.Crm.Client.Core.Framework._String.isNullOrEmpty(message)) ? Microsoft.Crm.Client.Core.Framework._String.empty : String.format.apply(null, [ message ].concat(args));
    return new Microsoft.Crm.Client.Core.Framework.ErrorStatus(errorMessage);
}
Microsoft.Crm.Client.Core.Framework.ErrorStatus.fromException = function Microsoft_Crm_Client_Core_Framework_ErrorStatus$fromException(exception, message) {
    var args = [];
    for (var $$pai_4 = 2; $$pai_4 < arguments.length; ++$$pai_4) {
        args[$$pai_4 - 2] = arguments[$$pai_4];
    }
    var errorStatus;
    if (!Microsoft.Crm.Client.Core.Framework._String.isNullOrEmpty(message)) {
        message = String.format.apply(null, [ message ].concat(args));
        errorStatus = new Microsoft.Crm.Client.Core.Framework.ErrorStatus(message);
    }
    else {
        errorStatus = new Microsoft.Crm.Client.Core.Framework.ErrorStatus(exception.message);
    }
    errorStatus.set_exception(exception);
    return errorStatus;
}
Microsoft.Crm.Client.Core.Framework.ErrorStatus.fromErrorCodeOnly = function Microsoft_Crm_Client_Core_Framework_ErrorStatus$fromErrorCodeOnly(errorCode) {
    var errorStatus = new Microsoft.Crm.Client.Core.Framework.ErrorStatus(Microsoft.Crm.Client.Core.Framework._String.empty);
    errorStatus.set_errorCode(errorCode);
    return errorStatus;
}
Microsoft.Crm.Client.Core.Framework.ErrorStatus.fromErrorData = function Microsoft_Crm_Client_Core_Framework_ErrorStatus$fromErrorData(errors) {
    var error = new Microsoft.Crm.Client.Core.Framework.ErrorData(0);
    if (Microsoft.Crm.Client.Core.Framework.ErrorData.isInstanceOfType(errors) && !(errors).get_isWarning()) {
        error = errors;
    }
    else if (!_Script.isNullOrUndefined(errors)) {
        for (var $$arr_2 = errors, $$len_3 = $$arr_2.length, $$idx_4 = 0; $$idx_4 < $$len_3; ++$$idx_4) {
            var errorData = $$arr_2[$$idx_4];
            if (!errorData.get_isWarning()) {
                error = errorData;
                break;
            }
        }
    }
    var errorStatus = new Microsoft.Crm.Client.Core.Framework.ErrorStatus(error.get_message());
    errorStatus.set_title(error.get_title());
    errorStatus.set_okButtonText(error.get_okButtonText());
    errorStatus.set_cancelButtonText(error.get_cancelButtonText());
    errorStatus.set_errorCode(error.get_errorCode());
    errorStatus.set_errors((Microsoft.Crm.Client.Core.Framework.ErrorData.isInstanceOfType(errors)) ? [ error ] : errors);
    return errorStatus;
}
Microsoft.Crm.Client.Core.Framework.ErrorStatus.fromLocalStoreError = function Microsoft_Crm_Client_Core_Framework_ErrorStatus$fromLocalStoreError(errorCode, message) {
    var args = [];
    for (var $$pai_4 = 2; $$pai_4 < arguments.length; ++$$pai_4) {
        args[$$pai_4 - 2] = arguments[$$pai_4];
    }
    var errorStatus = new Microsoft.Crm.Client.Core.Framework.ErrorStatus(String.format.apply(null, [ message ].concat(args)));
    errorStatus.set_errorSource(Microsoft.Crm.Client.Core.Framework.ErrorSource.localStore);
    errorStatus.set_errorCode(errorCode);
    return errorStatus;
}
Microsoft.Crm.Client.Core.Framework.ErrorStatus.prototype = {
    _$$pf_ErrorCode$p$0: 0,
    
    get_errorCode: function Microsoft_Crm_Client_Core_Framework_ErrorStatus$get_errorCode() {
        return this._$$pf_ErrorCode$p$0;
    },
    
    set_errorCode: function Microsoft_Crm_Client_Core_Framework_ErrorStatus$set_errorCode(value) {
        this._$$pf_ErrorCode$p$0 = value;
        return value;
    },
    
    _$$pf_Message$p$0: null,
    
    get_message: function Microsoft_Crm_Client_Core_Framework_ErrorStatus$get_message() {
        return this._$$pf_Message$p$0;
    },
    
    set_message: function Microsoft_Crm_Client_Core_Framework_ErrorStatus$set_message(value) {
        this._$$pf_Message$p$0 = value;
        return value;
    },
    
    _$$pf_Title$p$0: null,
    
    get_title: function Microsoft_Crm_Client_Core_Framework_ErrorStatus$get_title() {
        return this._$$pf_Title$p$0;
    },
    
    set_title: function Microsoft_Crm_Client_Core_Framework_ErrorStatus$set_title(value) {
        this._$$pf_Title$p$0 = value;
        return value;
    },
    
    _$$pf_OkButtonText$p$0: null,
    
    get_okButtonText: function Microsoft_Crm_Client_Core_Framework_ErrorStatus$get_okButtonText() {
        return this._$$pf_OkButtonText$p$0;
    },
    
    set_okButtonText: function Microsoft_Crm_Client_Core_Framework_ErrorStatus$set_okButtonText(value) {
        this._$$pf_OkButtonText$p$0 = value;
        return value;
    },
    
    _$$pf_CancelButtonText$p$0: null,
    
    get_cancelButtonText: function Microsoft_Crm_Client_Core_Framework_ErrorStatus$get_cancelButtonText() {
        return this._$$pf_CancelButtonText$p$0;
    },
    
    set_cancelButtonText: function Microsoft_Crm_Client_Core_Framework_ErrorStatus$set_cancelButtonText(value) {
        this._$$pf_CancelButtonText$p$0 = value;
        return value;
    },
    
    _$$pf_Exception$p$0: null,
    
    get_exception: function Microsoft_Crm_Client_Core_Framework_ErrorStatus$get_exception() {
        return this._$$pf_Exception$p$0;
    },
    
    set_exception: function Microsoft_Crm_Client_Core_Framework_ErrorStatus$set_exception(value) {
        this._$$pf_Exception$p$0 = value;
        return value;
    },
    
    _$$pf_InnerError$p$0: null,
    
    get_innerError: function Microsoft_Crm_Client_Core_Framework_ErrorStatus$get_innerError() {
        return this._$$pf_InnerError$p$0;
    },
    
    set_innerError: function Microsoft_Crm_Client_Core_Framework_ErrorStatus$set_innerError(value) {
        this._$$pf_InnerError$p$0 = value;
        return value;
    },
    
    _$$pf_ErrorFault$p$0: null,
    
    get_errorFault: function Microsoft_Crm_Client_Core_Framework_ErrorStatus$get_errorFault() {
        return this._$$pf_ErrorFault$p$0;
    },
    
    set_errorFault: function Microsoft_Crm_Client_Core_Framework_ErrorStatus$set_errorFault(value) {
        this._$$pf_ErrorFault$p$0 = value;
        return value;
    },
    
    _$$pf_Errors$p$0: null,
    
    get_errors: function Microsoft_Crm_Client_Core_Framework_ErrorStatus$get_errors() {
        return this._$$pf_Errors$p$0;
    },
    
    set_errors: function Microsoft_Crm_Client_Core_Framework_ErrorStatus$set_errors(value) {
        this._$$pf_Errors$p$0 = value;
        return value;
    },
    
    _$$pf_ErrorSource$p$0: 0,
    
    get_errorSource: function Microsoft_Crm_Client_Core_Framework_ErrorStatus$get_errorSource() {
        return this._$$pf_ErrorSource$p$0;
    },
    
    set_errorSource: function Microsoft_Crm_Client_Core_Framework_ErrorStatus$set_errorSource(value) {
        this._$$pf_ErrorSource$p$0 = value;
        return value;
    },
    
    chainError: function Microsoft_Crm_Client_Core_Framework_ErrorStatus$chainError(previousError) {
        this.set_message(this.get_message() + '\nInner Error Message:\n' + previousError.get_message());
        this.set_innerError(previousError);
    },
    
    getDiagnostics: function Microsoft_Crm_Client_Core_Framework_ErrorStatus$getDiagnostics() {
        var errorMessage = Microsoft.Crm.Client.Core.Framework._String.empty;
        if (!_Script.isNullOrUndefined(this.get_errors())) {
            for (var $$arr_1 = this.get_errors(), $$len_2 = $$arr_1.length, $$idx_3 = 0; $$idx_3 < $$len_2; ++$$idx_3) {
                var error = $$arr_1[$$idx_3];
                if (!Microsoft.Crm.Client.Core.Framework._String.isNullOrEmpty(error.get_diagnosticMessage())) {
                    errorMessage += error.get_diagnosticMessage();
                    if (!Microsoft.Crm.Client.Core.Framework._String.isNullOrEmpty(error.get_stackTrace())) {
                        errorMessage += '\nStackTrace:\n' + error.get_stackTrace();
                    }
                    errorMessage += '\n\n';
                }
            }
        }
        if (Microsoft.Crm.Client.Core.Framework._String.isNullOrWhiteSpace(errorMessage)) {
            errorMessage = 'Diagnostic message not found for error ' + this.get_errorCode() + ' with message: ' + this.get_message();
        }
        return errorMessage;
    },
    
    getObjectData: function Microsoft_Crm_Client_Core_Framework_ErrorStatus$getObjectData() {
        var data = {};
        data['errorcode'] = this.get_errorCode();
        data['message'] = this.get_message();
        if (!_Script.isNullOrUndefined(this.get_innerError())) {
            data['innererror'] = this.get_innerError().getObjectData();
        }
        return data;
    }
}


function _XMLNode() {
}
_XMLNode.getLocalName = function _XMLNode$getLocalName(node) {
    Microsoft.Crm.Client.Core.Framework.Utils.ExceptionHelpers.throwOnNullArgument(node, 'node');
    var prefixLength = 0;
    if (!Microsoft.Crm.Client.Core.Framework._String.isNullOrEmpty(node.prefix)) {
        prefixLength = node.prefix.length;
    }
    if (!prefixLength) {
        return node.nodeName;
    }
    return node.nodeName.substr(prefixLength + 1);
}
_XMLNode.getInnerText = function _XMLNode$getInnerText(node) {
    Microsoft.Crm.Client.Core.Framework.Utils.ExceptionHelpers.throwOnNullArgument(node, 'node');
    var builder = new Sys.StringBuilder();
    if (!Microsoft.Crm.Client.Core.Framework._String.isNullOrWhiteSpace(node.nodeValue)) {
        builder.append(node.nodeValue);
    }
    for (var index = 0; index < node.childNodes.length; index++) {
        var childNode = node.childNodes[index];
        builder.append(_XMLNode.getInnerText(childNode));
    }
    return builder.toString();
}
_XMLNode.getAttributeValue = function _XMLNode$getAttributeValue(node, localName) {
    var attribute = _XMLNode._getAttribute$p(node, localName);
    return (null === attribute) ? null : attribute.value;
}
_XMLNode._getAttribute$p = function _XMLNode$_getAttribute$p(node, localName) {
    Microsoft.Crm.Client.Core.Framework.Utils.ExceptionHelpers.throwOnNullArgument(node, 'node');
    Microsoft.Crm.Client.Core.Framework.Utils.ExceptionHelpers.throwOnNullOrEmptyArgument(localName, 'localName');
    localName = localName.toLowerCase();
    for (var index = 0; index < node.attributes.length; index++) {
        var currentAttribute = node.attributes.item(index);
        var attributeLocalName = _XMLNode.getLocalName(currentAttribute).toLowerCase();
        if (attributeLocalName === localName) {
            return currentAttribute;
        }
    }
    return null;
}


Microsoft.Crm.Client.Core.Framework._Dictionary = function Microsoft_Crm_Client_Core_Framework__Dictionary() {
}
Microsoft.Crm.Client.Core.Framework._Dictionary.count = function Microsoft_Crm_Client_Core_Framework__Dictionary$count(obj) {
    var count = 0;
    var $$dict_3 = obj;
    for (var $$key_4 in $$dict_3) {
        var entry = { key: $$key_4, value: $$dict_3[$$key_4] };
        count++;
    }
    return count;
}


Microsoft.Crm.Client.Core.Framework._Enum = function Microsoft_Crm_Client_Core_Framework__Enum() {
}
Microsoft.Crm.Client.Core.Framework._Enum.parse = function Microsoft_Crm_Client_Core_Framework__Enum$parse(T, enumKey) {
    return Microsoft.Crm.Client.Core.Framework._Enum.parseType(T, enumKey);
}
Microsoft.Crm.Client.Core.Framework._Enum.parseType = function Microsoft_Crm_Client_Core_Framework__Enum$parseType(enumType, enumKey) {
    var firstCharCode = enumKey.charCodeAt(0);
    if (firstCharCode <= 57 && firstCharCode >= 0) {
        var intValue = parseInt(enumKey);
        if (isFinite(intValue) && intValue >= 0) {
            return intValue;
        }
    }
    try {
        return enumType.parse(enumKey, true);
    }
    catch (ex) {
        var intValue = parseInt(enumKey);
        if (isFinite(intValue) && intValue >= 0) {
            return intValue;
        }
        throw ex;
    }
}
Microsoft.Crm.Client.Core.Framework._Enum.toString = function Microsoft_Crm_Client_Core_Framework__Enum$toString(enumType, value) {
    return enumType.toString(value);
}


Microsoft.Crm.Client.Core.Framework.DynamicsTrace = function Microsoft_Crm_Client_Core_Framework_DynamicsTrace() {
}
Microsoft.Crm.Client.Core.Framework.DynamicsTrace.logInfo = function Microsoft_Crm_Client_Core_Framework_DynamicsTrace$logInfo(key, area, parameter) {
}


Microsoft.Crm.Client.Core.Framework.PerformanceMarker = function Microsoft_Crm_Client_Core_Framework_PerformanceMarker() {
    this.parameters = new Array(0);
}
Microsoft.Crm.Client.Core.Framework.PerformanceMarker.prototype = {
    name: null,
    timestamp: 0,
    id: null,
    type: 0,
    data: null
}


Microsoft.Crm.Client.Core.Framework.PerformanceStopwatch = function Microsoft_Crm_Client_Core_Framework_PerformanceStopwatch(name) {
}
Microsoft.Crm.Client.Core.Framework.PerformanceStopwatch.prototype = {
    name: null,
    startMarker: null,
    stopMarker: null,
    
    start: function Microsoft_Crm_Client_Core_Framework_PerformanceStopwatch$start() {
    },
    
    stop: function Microsoft_Crm_Client_Core_Framework_PerformanceStopwatch$stop() {
    },
    
    addParameter: function Microsoft_Crm_Client_Core_Framework_PerformanceStopwatch$addParameter(parameter) {
    }
}


Microsoft.Crm.Client.Core.Framework.PageLoadPerformanceMarkerNames = function Microsoft_Crm_Client_Core_Framework_PageLoadPerformanceMarkerNames() {
}


Microsoft.Crm.Client.Core.Framework.AppBootPerformanceMarkerNames = function Microsoft_Crm_Client_Core_Framework_AppBootPerformanceMarkerNames() {
}


Microsoft.Crm.Client.Core.Framework.DataLayerPerformanceMarkerNames = function Microsoft_Crm_Client_Core_Framework_DataLayerPerformanceMarkerNames() {
}


Microsoft.Crm.Client.Core.Framework.OfflineDataStoreCRUDPerformanceMarkerNames = function Microsoft_Crm_Client_Core_Framework_OfflineDataStoreCRUDPerformanceMarkerNames() {
}


Microsoft.Crm.Client.Core.Framework.FieldFormat = function Microsoft_Crm_Client_Core_Framework_FieldFormat() {
}
Microsoft.Crm.Client.Core.Framework.FieldFormat._addDelimiter$p = function Microsoft_Crm_Client_Core_Framework_FieldFormat$_addDelimiter$p(format) {
    return Microsoft.Crm.Client.Core.Framework.FieldFormat.delimiter + Microsoft.Crm.Client.Core.Framework.FieldFormat._fieldFormatValue.toString(format);
}


Microsoft.Crm.Client.Core.Framework.FieldFormat._fieldFormatValue = function() {}
Microsoft.Crm.Client.Core.Framework.FieldFormat._fieldFormatValue.prototype = {
    Raw: 0, 
    Numeric: 1, 
    Label: 2, 
    Value: 3, 
    State: 4, 
    Id: 5, 
    Name: 6, 
    LogicalName: 7, 
    AllowedStatusTransitions: 8, 
    DefaultStatus: 9, 
    Color: 10
}
Microsoft.Crm.Client.Core.Framework.FieldFormat._fieldFormatValue.registerEnum('Microsoft.Crm.Client.Core.Framework.FieldFormat._fieldFormatValue', false);


Microsoft.Crm.Client.Core.Framework.MetadataTypeName = function Microsoft_Crm_Client_Core_Framework_MetadataTypeName() {
}


Microsoft.Crm.Client.Core.Framework.TypedDictionary$1 = function Microsoft_Crm_Client_Core_Framework_TypedDictionary$1(items) {
    if (!_Script.isNullOrUndefined(items)) {
        this._items$p$0 = items;
    }
    else {
        this._items$p$0 = {};
    }
}
Microsoft.Crm.Client.Core.Framework.TypedDictionary$1.$$ = function Microsoft_Crm_Client_Core_Framework_TypedDictionary$1$$$(T) {
    var $$cn = 'TypedDictionary$1' + '$' + T.getName().replace(/\./g, '_');
    if (!Microsoft.Crm.Client.Core.Framework[$$cn]) {
        var $$ccr = Microsoft.Crm.Client.Core.Framework[$$cn] = function() {
            (this.$$gta = this.$$gta || {})['Microsoft.Crm.Client.Core.Framework.TypedDictionary$1'] = {'T': T};
            var newArgs = [];
            for (var i = 0; i < arguments.length; ++i) {
                newArgs[i] = arguments[i];
            }
            Microsoft.Crm.Client.Core.Framework.TypedDictionary$1.apply(this, newArgs);
        };
        $$ccr.registerClass('Microsoft.Crm.Client.Core.Framework.' + $$cn);
        var $$dict_5 = Microsoft.Crm.Client.Core.Framework.TypedDictionary$1.prototype;
        for (var $$key_6 in $$dict_5) {
            var $$entry_7 = { key: $$key_6, value: $$dict_5[$$key_6] };
            if ('constructor' !== $$entry_7.key) {
                $$ccr.prototype[$$entry_7.key] = $$entry_7.value;
            }
        }
    }
    return Microsoft.Crm.Client.Core.Framework[$$cn];
}
Microsoft.Crm.Client.Core.Framework.TypedDictionary$1.prototype = {
    _items$p$0: null,
    
    get_item: function Microsoft_Crm_Client_Core_Framework_TypedDictionary$1$get_item(index) {
        return this._items$p$0[index];
    },
    
    set_item: function Microsoft_Crm_Client_Core_Framework_TypedDictionary$1$set_item(index, value) {
        this._items$p$0[index] = value;
        return value;
    },
    
    remove: function Microsoft_Crm_Client_Core_Framework_TypedDictionary$1$remove(index) {
        delete this._items$p$0[index];
    },
    
    clear: function Microsoft_Crm_Client_Core_Framework_TypedDictionary$1$clear() {
        this._items$p$0 = {};
    },
    
    contains: function Microsoft_Crm_Client_Core_Framework_TypedDictionary$1$contains(item) {
        var $$dict_2 = this._items$p$0;
        for (var $$key_3 in $$dict_2) {
            var entry = { key: $$key_3, value: $$dict_2[$$key_3] };
            if ((entry.value == item)) {
                return true;
            }
        }
        return false;
    },
    
    containsKey: function Microsoft_Crm_Client_Core_Framework_TypedDictionary$1$containsKey(key) {
        return ((key) in this._items$p$0);
    },
    
    indexOf: function Microsoft_Crm_Client_Core_Framework_TypedDictionary$1$indexOf(item) {
        var $$dict_2 = this._items$p$0;
        for (var $$key_3 in $$dict_2) {
            var entry = { key: $$key_3, value: $$dict_2[$$key_3] };
            if ((entry.value == item)) {
                return entry.key;
            }
        }
        return null;
    },
    
    count: function Microsoft_Crm_Client_Core_Framework_TypedDictionary$1$count() {
        return Microsoft.Crm.Client.Core.Framework._Dictionary.count(this._items$p$0);
    },
    
    toArray: function Microsoft_Crm_Client_Core_Framework_TypedDictionary$1$toArray() {
        var result = new Array(this.count());
        var i = 0;
        var $$dict_3 = this._items$p$0;
        for (var $$key_4 in $$dict_3) {
            var entry = { key: $$key_4, value: $$dict_3[$$key_4] };
            result[i] = entry.value;
            i++;
        }
        return result;
    },
    
    toDictionary: function Microsoft_Crm_Client_Core_Framework_TypedDictionary$1$toDictionary() {
        return this._items$p$0;
    },
    
    get_keys: function Microsoft_Crm_Client_Core_Framework_TypedDictionary$1$get_keys() {
        var result = new Array(this.count());
        var i = 0;
        var $$dict_3 = this._items$p$0;
        for (var $$key_4 in $$dict_3) {
            var entry = { key: $$key_4, value: $$dict_3[$$key_4] };
            result[i] = entry.key;
            i++;
        }
        return result;
    }
}


Microsoft.Crm.Client.Core.Framework.CallingContext = function Microsoft_Crm_Client_Core_Framework_CallingContext(callerId, callerName, callerPriority) {
    this._callerId$p$0 = (!Microsoft.Crm.Client.Core.Framework._String.isNullOrEmpty(callerId)) ? callerId : Microsoft.Crm.Client.Core.Framework.CallingContext.unknown;
    this._callerName$p$0 = (!Microsoft.Crm.Client.Core.Framework._String.isNullOrEmpty(callerName)) ? callerName : Microsoft.Crm.Client.Core.Framework.CallingContext.unknown;
    this._callerPriority$p$0 = callerPriority;
}
Microsoft.Crm.Client.Core.Framework.CallingContext.get_isEnabled = function Microsoft_Crm_Client_Core_Framework_CallingContext$get_isEnabled() {
    return true;
}
Microsoft.Crm.Client.Core.Framework.CallingContext.prototype = {
    _callerId$p$0: null,
    _callerName$p$0: null,
    _callerPriority$p$0: 0,
    
    get_callerId: function Microsoft_Crm_Client_Core_Framework_CallingContext$get_callerId() {
        return this._callerId$p$0;
    },
    
    get_callerName: function Microsoft_Crm_Client_Core_Framework_CallingContext$get_callerName() {
        return this._callerName$p$0;
    },
    
    get_callerPriority: function Microsoft_Crm_Client_Core_Framework_CallingContext$get_callerPriority() {
        return this._callerPriority$p$0;
    }
}


Microsoft.Crm.Client.Core.Framework.CallbackSafeArray$1 = function Microsoft_Crm_Client_Core_Framework_CallbackSafeArray$1(argument) {
    if (_Script.isNullOrUndefined(argument)) {
        this._array$p$0 = new Array(0);
    }
    else if ($P_CRM.isArray(argument)) {
        this._array$p$0 = argument;
    }
    else {
        this._array$p$0 = new Array(argument);
    }
}
Microsoft.Crm.Client.Core.Framework.CallbackSafeArray$1.$$ = function Microsoft_Crm_Client_Core_Framework_CallbackSafeArray$1$$$(T) {
    var $$cn = 'CallbackSafeArray$1' + '$' + T.getName().replace(/\./g, '_');
    if (!Microsoft.Crm.Client.Core.Framework[$$cn]) {
        var $$ccr = Microsoft.Crm.Client.Core.Framework[$$cn] = function() {
            (this.$$gta = this.$$gta || {})['Microsoft.Crm.Client.Core.Framework.CallbackSafeArray$1'] = {'T': T};
            var newArgs = [];
            for (var i = 0; i < arguments.length; ++i) {
                newArgs[i] = arguments[i];
            }
            Microsoft.Crm.Client.Core.Framework.CallbackSafeArray$1.apply(this, newArgs);
        };
        $$ccr.registerClass('Microsoft.Crm.Client.Core.Framework.' + $$cn);
        var $$dict_5 = Microsoft.Crm.Client.Core.Framework.CallbackSafeArray$1.prototype;
        for (var $$key_6 in $$dict_5) {
            var $$entry_7 = { key: $$key_6, value: $$dict_5[$$key_6] };
            if ('constructor' !== $$entry_7.key) {
                $$ccr.prototype[$$entry_7.key] = $$entry_7.value;
            }
        }
    }
    return Microsoft.Crm.Client.Core.Framework[$$cn];
}
Microsoft.Crm.Client.Core.Framework.CallbackSafeArray$1.prototype = {
    _array$p$0: null,
    
    get_length: function Microsoft_Crm_Client_Core_Framework_CallbackSafeArray$1$get_length() {
        return this._array$p$0.length;
    },
    
    get_item: function Microsoft_Crm_Client_Core_Framework_CallbackSafeArray$1$get_item(index) {
        return this._array$p$0[index];
    },
    
    set_item: function Microsoft_Crm_Client_Core_Framework_CallbackSafeArray$1$set_item(index, value) {
        this._array$p$0[index] = value;
        return value;
    },
    
    add: function Microsoft_Crm_Client_Core_Framework_CallbackSafeArray$1$add(item) {
        Array.add(this._array$p$0, item);
    },
    
    addRange: function Microsoft_Crm_Client_Core_Framework_CallbackSafeArray$1$addRange(range) {
        Array.addRange(this._array$p$0, range);
    },
    
    toArray: function Microsoft_Crm_Client_Core_Framework_CallbackSafeArray$1$toArray() {
        return this._array$p$0;
    }
}


Microsoft.Crm.Client.Core.Framework.CrmErrorCodes = function Microsoft_Crm_Client_Core_Framework_CrmErrorCodes() {
}
Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.isOfflineError = function Microsoft_Crm_Client_Core_Framework_CrmErrorCodes$isOfflineError(errorCode) {
    return errorCode === Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.retrieveRecordOfflineErrorCode || errorCode === Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.dataSourceOfflineErrorCode;
}


Microsoft.Crm.Client.Core.Framework.Guid = function Microsoft_Crm_Client_Core_Framework_Guid(guidValue) {
    this._rawGuid$p$0 = Microsoft.Crm.Client.Core.Framework.Guid._getParsedString$p(guidValue);
    if (Microsoft.Crm.Client.Core.Framework._String.isNullOrEmpty(this._rawGuid$p$0)) {
        throw Error.argumentOutOfRange(String.format(Microsoft.Crm.Client.Core.Framework.Guid._invalidGuidException$p, guidValue));
    }
}
Microsoft.Crm.Client.Core.Framework.Guid._getParsedString$p = function Microsoft_Crm_Client_Core_Framework_Guid$_getParsedString$p(guidValue) {
    if (Microsoft.Crm.Client.Core.Framework._String.isNullOrEmpty(guidValue)) {
        return null;
    }
    guidValue = guidValue.toLowerCase();
    if (Microsoft.Crm.Client.Core.Framework.Guid.get__hyphenGuidVerifierPattern$p().test(guidValue) || Microsoft.Crm.Client.Core.Framework.Guid.get__braceAndHyphenGuidVerifierPattern$p().test(guidValue)) {
        return guidValue.replace(Microsoft.Crm.Client.Core.Framework.Guid.get__guidStripperPattern$p(), Microsoft.Crm.Client.Core.Framework._String.empty);
    }
    else if (Microsoft.Crm.Client.Core.Framework.Guid.get__contiguousGuidVerifierPattern$p().test(guidValue)) {
        return guidValue;
    }
    return null;
}
Microsoft.Crm.Client.Core.Framework.Guid.get_empty = function Microsoft_Crm_Client_Core_Framework_Guid$get_empty() {
    return Microsoft.Crm.Client.Core.Framework.Guid._empty$p || (Microsoft.Crm.Client.Core.Framework.Guid._empty$p = new Microsoft.Crm.Client.Core.Framework.Guid('00000000-0000-0000-0000-000000000000'));
}
Microsoft.Crm.Client.Core.Framework.Guid.get__guidStripperPattern$p = function Microsoft_Crm_Client_Core_Framework_Guid$get__guidStripperPattern$p() {
    return Microsoft.Crm.Client.Core.Framework.Guid._guidStripperPattern$p || (Microsoft.Crm.Client.Core.Framework.Guid._guidStripperPattern$p = new RegExp('{|}|-', 'g'));
}
Microsoft.Crm.Client.Core.Framework.Guid.get__hyphenGuidVerifierPattern$p = function Microsoft_Crm_Client_Core_Framework_Guid$get__hyphenGuidVerifierPattern$p() {
    return Microsoft.Crm.Client.Core.Framework.Guid._hyphenGuidVerifierPattern$p || (Microsoft.Crm.Client.Core.Framework.Guid._hyphenGuidVerifierPattern$p = new RegExp('^(\\d|[a-f]){8}-(\\d|[a-f]){4}-(\\d|[a-f]){4}-(\\d|[a-f]){4}-(\\d|[a-f]){12}$'));
}
Microsoft.Crm.Client.Core.Framework.Guid.get__braceAndHyphenGuidVerifierPattern$p = function Microsoft_Crm_Client_Core_Framework_Guid$get__braceAndHyphenGuidVerifierPattern$p() {
    return Microsoft.Crm.Client.Core.Framework.Guid._braceAndHyphenGuidVerifierPattern$p || (Microsoft.Crm.Client.Core.Framework.Guid._braceAndHyphenGuidVerifierPattern$p = new RegExp('^{(\\d|[a-f]){8}-(\\d|[a-f]){4}-(\\d|[a-f]){4}-(\\d|[a-f]){4}-(\\d|[a-f]){12}}$'));
}
Microsoft.Crm.Client.Core.Framework.Guid.get__contiguousGuidVerifierPattern$p = function Microsoft_Crm_Client_Core_Framework_Guid$get__contiguousGuidVerifierPattern$p() {
    return Microsoft.Crm.Client.Core.Framework.Guid._contiguousGuidVerifierPattern$p || (Microsoft.Crm.Client.Core.Framework.Guid._contiguousGuidVerifierPattern$p = new RegExp('^(\\d|[a-f]){32}$'));
}
Microsoft.Crm.Client.Core.Framework.Guid.createFromObjectData = function Microsoft_Crm_Client_Core_Framework_Guid$createFromObjectData(data) {
    var rawguid = data['rawguid'];
    return new Microsoft.Crm.Client.Core.Framework.Guid(rawguid);
}
Microsoft.Crm.Client.Core.Framework.Guid.tryCreate = function Microsoft_Crm_Client_Core_Framework_Guid$tryCreate(guidValue) {
    var rawGuid = Microsoft.Crm.Client.Core.Framework.Guid._getParsedString$p(guidValue);
    if (!Microsoft.Crm.Client.Core.Framework._String.isNullOrEmpty(rawGuid)) {
        return new Microsoft.Crm.Client.Core.Framework.Guid(rawGuid);
    }
    return Microsoft.Crm.Client.Core.Framework.Guid.get_empty();
}
Microsoft.Crm.Client.Core.Framework.Guid.formatToUpper = function Microsoft_Crm_Client_Core_Framework_Guid$formatToUpper(sourceGuid) {
    if (_Script.isNullOrUndefined(sourceGuid)) {
        return sourceGuid;
    }
    sourceGuid = sourceGuid.toLowerCase();
    if (Microsoft.Crm.Client.Core.Framework.Guid.get__braceAndHyphenGuidVerifierPattern$p().test(sourceGuid)) {
        return sourceGuid.toUpperCase();
    }
    else {
        return String.format('{{{0}}}', sourceGuid.toUpperCase());
    }
}
Microsoft.Crm.Client.Core.Framework.Guid.removeBrackets = function Microsoft_Crm_Client_Core_Framework_Guid$removeBrackets(sourceGuid) {
    if (_Script.isNullOrUndefined(sourceGuid)) {
        return sourceGuid;
    }
    return sourceGuid.replace('{', Microsoft.Crm.Client.Core.Framework._String.empty).replace('}', Microsoft.Crm.Client.Core.Framework._String.empty).trim();
}
Microsoft.Crm.Client.Core.Framework.Guid.isNullOrEmpty = function Microsoft_Crm_Client_Core_Framework_Guid$isNullOrEmpty(guid) {
    if (_Script.isNullOrUndefined(guid) || !guid.length) {
        return true;
    }
    if (new Microsoft.Crm.Client.Core.Framework.Guid(guid).equals(Microsoft.Crm.Client.Core.Framework.Guid._empty$p)) {
        return true;
    }
    return false;
}
Microsoft.Crm.Client.Core.Framework.Guid.newGuid = function Microsoft_Crm_Client_Core_Framework_Guid$newGuid() {
    var HexChars = '0123456789abcdef';
    var GuidSize = 36;
    var sGuid = new Sys.StringBuilder();
    for (var i = 0; i < GuidSize; i++) {
        if (i === 14) {
            sGuid.append('4');
            continue;
        }
        if (i === 8 || i === 13 || i === 18 || i === 23) {
            sGuid.append('-');
            continue;
        }
        if (i === 19) {
            var n = Math.floor(Math.random() * 16);
            HexChars.substr(n & 3 | 8, 1);
        }
        sGuid.append(HexChars.substr(Math.floor(Math.random() * 16), 1));
    }
    return new Microsoft.Crm.Client.Core.Framework.Guid(sGuid.toString());
}
Microsoft.Crm.Client.Core.Framework.Guid.prototype = {
    _rawGuid$p$0: null,
    _formattedGuid$p$0: null,
    
    getObjectData: function Microsoft_Crm_Client_Core_Framework_Guid$getObjectData() {
        var data = {};
        data['rawguid'] = this._rawGuid$p$0;
        return data;
    },
    
    equals: function Microsoft_Crm_Client_Core_Framework_Guid$equals(obj) {
        if (Microsoft.Crm.Client.Core.Framework.Guid.isInstanceOfType(obj)) {
            return (obj)._rawGuid$p$0 === this._rawGuid$p$0;
        }
        if (String.isInstanceOfType(obj)) {
            try {
                var otherGuid = new Microsoft.Crm.Client.Core.Framework.Guid(obj);
                return otherGuid._rawGuid$p$0 === this._rawGuid$p$0;
            }
            catch ($$e_2) {
                return false;
            }
        }
        return false;
    },
    
    getHashCode: function Microsoft_Crm_Client_Core_Framework_Guid$getHashCode() {
        return (!Microsoft.Crm.Client.Core.Framework._String.isNullOrEmpty(this._rawGuid$p$0)) ? Microsoft.Crm.Client.Core.Framework._String.hashCode(this._rawGuid$p$0) : 0;
    },
    
    toString: function Microsoft_Crm_Client_Core_Framework_Guid$toString() {
        if (!this._formattedGuid$p$0) {
            this._formattedGuid$p$0 = this._rawGuid$p$0.substring(0, 8) + '-' + this._rawGuid$p$0.substring(8, 12) + '-' + this._rawGuid$p$0.substring(12, 16) + '-' + this._rawGuid$p$0.substring(16, 20) + '-' + this._rawGuid$p$0.substring(20, 32);
        }
        return this._formattedGuid$p$0;
    }
}


Microsoft.Crm.Client.Core.Framework.KeyValuePair$2 = function Microsoft_Crm_Client_Core_Framework_KeyValuePair$2(key, value) {
    this._key$p$0 = ((this.$$gta['Microsoft.Crm.Client.Core.Framework.KeyValuePair$2']['TKey'] === Number || Type.isEnum(this.$$gta['Microsoft.Crm.Client.Core.Framework.KeyValuePair$2']['TKey'])) ? 0 : (this.$$gta['Microsoft.Crm.Client.Core.Framework.KeyValuePair$2']['TKey'] === Boolean) ? false : null);
    this._value$p$0 = ((this.$$gta['Microsoft.Crm.Client.Core.Framework.KeyValuePair$2']['TValue'] === Number || Type.isEnum(this.$$gta['Microsoft.Crm.Client.Core.Framework.KeyValuePair$2']['TValue'])) ? 0 : (this.$$gta['Microsoft.Crm.Client.Core.Framework.KeyValuePair$2']['TValue'] === Boolean) ? false : null);
    this._key$p$0 = key;
    this._value$p$0 = value;
}
Microsoft.Crm.Client.Core.Framework.KeyValuePair$2.$$ = function Microsoft_Crm_Client_Core_Framework_KeyValuePair$2$$$(TKey, TValue) {
    var $$cn = 'KeyValuePair$2' + '$' + TKey.getName().replace(/\./g, '_') + '$' + TValue.getName().replace(/\./g, '_');
    if (!Microsoft.Crm.Client.Core.Framework[$$cn]) {
        var $$ccr = Microsoft.Crm.Client.Core.Framework[$$cn] = function() {
            (this.$$gta = this.$$gta || {})['Microsoft.Crm.Client.Core.Framework.KeyValuePair$2'] = {'TKey': TKey, 'TValue': TValue};
            var newArgs = [];
            for (var i = 0; i < arguments.length; ++i) {
                newArgs[i] = arguments[i];
            }
            Microsoft.Crm.Client.Core.Framework.KeyValuePair$2.apply(this, newArgs);
        };
        $$ccr.registerClass('Microsoft.Crm.Client.Core.Framework.' + $$cn);
        var $$dict_6 = Microsoft.Crm.Client.Core.Framework.KeyValuePair$2.prototype;
        for (var $$key_7 in $$dict_6) {
            var $$entry_8 = { key: $$key_7, value: $$dict_6[$$key_7] };
            if ('constructor' !== $$entry_8.key) {
                $$ccr.prototype[$$entry_8.key] = $$entry_8.value;
            }
        }
    }
    return Microsoft.Crm.Client.Core.Framework[$$cn];
}
Microsoft.Crm.Client.Core.Framework.KeyValuePair$2.prototype = {
    
    get_key: function Microsoft_Crm_Client_Core_Framework_KeyValuePair$2$get_key() {
        return this._key$p$0;
    },
    
    get_value: function Microsoft_Crm_Client_Core_Framework_KeyValuePair$2$get_value() {
        return this._value$p$0;
    }
}


Microsoft.Crm.Client.Core.Framework.ManifestType = function Microsoft_Crm_Client_Core_Framework_ManifestType() {
}


Microsoft.Crm.Client.Core.Framework.TimeZoneAdjuster = function Microsoft_Crm_Client_Core_Framework_TimeZoneAdjuster(end, start, delta, daylightStart, daylightEnd) {
    this._dateEnd$p$0 = end;
    this._dateStart$p$0 = start;
    this._delta$p$0 = delta;
    this._daylightStart$p$0 = daylightStart;
    this._daylightEnd$p$0 = daylightEnd;
}
Microsoft.Crm.Client.Core.Framework.TimeZoneAdjuster.createFromObjectData = function Microsoft_Crm_Client_Core_Framework_TimeZoneAdjuster$createFromObjectData(data) {
    var dayStart = (Date.isInstanceOfType(data['datestart'])) ? data['datestart'] : new Date(Date.parse(data['datestart']));
    var dayEnd = (Date.isInstanceOfType(data['dateend'])) ? data['dateend'] : new Date(Date.parse(data['dateend']));
    var delta = data['delta'];
    var daylightStart = Microsoft.Crm.Client.Core.Framework.TransitionConstraint.createFromObjectData(data['daylightstart']);
    var daylightEnd = Microsoft.Crm.Client.Core.Framework.TransitionConstraint.createFromObjectData(data['daylightend']);
    return new Microsoft.Crm.Client.Core.Framework.TimeZoneAdjuster(dayEnd, dayStart, delta, daylightStart, daylightEnd);
}
Microsoft.Crm.Client.Core.Framework.TimeZoneAdjuster.prototype = {
    _dateEnd$p$0: null,
    _dateStart$p$0: null,
    _delta$p$0: 0,
    _daylightStart$p$0: null,
    _daylightEnd$p$0: null,
    
    get_dateStart: function Microsoft_Crm_Client_Core_Framework_TimeZoneAdjuster$get_dateStart() {
        return this._dateStart$p$0;
    },
    
    get_dateEnd: function Microsoft_Crm_Client_Core_Framework_TimeZoneAdjuster$get_dateEnd() {
        return this._dateEnd$p$0;
    },
    
    getObjectData: function Microsoft_Crm_Client_Core_Framework_TimeZoneAdjuster$getObjectData() {
        var context = {};
        context['dateend'] = this._dateEnd$p$0;
        context['datestart'] = this._dateStart$p$0;
        context['delta'] = this._delta$p$0;
        context['daylightstart'] = this._daylightStart$p$0.getObjectData();
        context['daylightend'] = this._daylightEnd$p$0.getObjectData();
        return context;
    }
}


Microsoft.Crm.Client.Core.Framework.TransitionConstraint = function Microsoft_Crm_Client_Core_Framework_TransitionConstraint(day, dow, month, week, timeOfDay, isFixed) {
    this._day$p$0 = day;
    this._dayOfWeek$p$0 = dow;
    this._month$p$0 = month;
    this._week$p$0 = week;
    this._timeOfDay$p$0 = timeOfDay;
    this._isFixedDateRule$p$0 = isFixed;
}
Microsoft.Crm.Client.Core.Framework.TransitionConstraint.createFromObjectData = function Microsoft_Crm_Client_Core_Framework_TransitionConstraint$createFromObjectData(data) {
    var day = data['day'];
    var dayOfWeek = data['dayofweek'];
    var month = data['month'];
    var week = data['week'];
    var timeOfDay = (Date.isInstanceOfType(data['timeofday'])) ? data['timeofday'] : new Date(Date.parse(data['timeofday']));
    var isFixedDateRule = data['isfixeddaterule'];
    return new Microsoft.Crm.Client.Core.Framework.TransitionConstraint(day, dayOfWeek, month, week, timeOfDay, isFixedDateRule);
}
Microsoft.Crm.Client.Core.Framework.TransitionConstraint.prototype = {
    _day$p$0: 0,
    _dayOfWeek$p$0: 0,
    _month$p$0: 0,
    _week$p$0: 0,
    _timeOfDay$p$0: null,
    _isFixedDateRule$p$0: false,
    
    getObjectData: function Microsoft_Crm_Client_Core_Framework_TransitionConstraint$getObjectData() {
        var context = {};
        context['day'] = this._day$p$0;
        context['dayofweek'] = this._dayOfWeek$p$0;
        context['month'] = this._month$p$0;
        context['week'] = this._week$p$0;
        context['timeofday'] = this._timeOfDay$p$0;
        context['isfixeddaterule'] = this._isFixedDateRule$p$0;
        return context;
    }
}


Microsoft.Crm.Client.Core.Framework.Undefined = function Microsoft_Crm_Client_Core_Framework_Undefined() {
}


function _Math() {
}
_Math.modulo = function _Math$modulo(value, modulo) {
    return ((value % modulo) + modulo) % modulo;
}
_Math.randomBetween = function _Math$randomBetween(lo, hi) {
    return Math.floor((Math.random() * (hi - lo + 1)) + lo);
}


function _Script() {
}
_Script.isNull = function _Script$isNull(value) {
    return null === value;
}
_Script.isNullOrUndefined = function _Script$isNullOrUndefined(value) {
    return null === value || value === undefined;
}
_Script.supportsEquals = function _Script$supportsEquals(obj) {
    return !_Script.isNullOrUndefined(obj) && !_Script.isNullOrUndefined(obj.equals);
}
_Script.isUndefined = function _Script$isUndefined(value) {
    return value === undefined;
}
_Script.assertWireType = function _Script$assertWireType(dataType, data) {
    if (data) {
        var wireType = data.__type;
        if (wireType) {
            var expectedWireType = new (dataType)().__type;
            Microsoft.Crm.Client.Core.Framework.Debug.assert(!!expectedWireType, dataType.getName() + ' does not provide wire type information (__type)');
            Microsoft.Crm.Client.Core.Framework.Debug.assert(expectedWireType === wireType, wireType + ' data type received while expecting ' + expectedWireType);
        }
    }
}


Microsoft.Crm.Client.Core.Framework._String = function Microsoft_Crm_Client_Core_Framework__String() {
}
Microsoft.Crm.Client.Core.Framework._String.isNullOrEmpty = function Microsoft_Crm_Client_Core_Framework__String$isNullOrEmpty(value) {
    return _Script.isNullOrUndefined(value) || value === Microsoft.Crm.Client.Core.Framework._String.empty;
}
Microsoft.Crm.Client.Core.Framework._String.isNullOrWhiteSpace = function Microsoft_Crm_Client_Core_Framework__String$isNullOrWhiteSpace(value) {
    return Microsoft.Crm.Client.Core.Framework._String.isNullOrEmpty(value) || value.trim() === Microsoft.Crm.Client.Core.Framework._String.empty;
}
Microsoft.Crm.Client.Core.Framework._String.hashCode = function Microsoft_Crm_Client_Core_Framework__String$hashCode(value) {
    var hash = 0;
    for (var i = 0; i < value.length; ++i) {
        var ch = value.charCodeAt(i);
        hash = ((hash << 5) - hash) + ch;
        hash = hash & hash;
    }
    return hash;
}
Microsoft.Crm.Client.Core.Framework._String.format = function Microsoft_Crm_Client_Core_Framework__String$format(format, arg0, arg1, arg2, arg3, arg4, arg5) {
    if (_Script.isUndefined(arg0) && _Script.isUndefined(arg1) && _Script.isUndefined(arg2) && _Script.isUndefined(arg3) && _Script.isUndefined(arg4) && _Script.isUndefined(arg5)) {
        return format;
    }
    return String.format(format, arg0, arg1, arg2, arg3, arg4, arg5);
}
Microsoft.Crm.Client.Core.Framework._String.replaceNewlineWithEnding = function Microsoft_Crm_Client_Core_Framework__String$replaceNewlineWithEnding(text, ending) {
    if (Microsoft.Crm.Client.Core.Framework._String.isNullOrEmpty(text)) {
        return Microsoft.Crm.Client.Core.Framework._String.empty;
    }
    else {
        var builder = new Sys.StringBuilder();
        var lines = text.split(Microsoft.Crm.Client.Core.Framework._String._newLineExpression$p);
        for (var index = 0; index < lines.length; index++) {
            if (!Microsoft.Crm.Client.Core.Framework._String.isNullOrWhiteSpace(lines[index])) {
                builder.append(lines[index]);
                builder.append(ending);
            }
        }
        return builder.toString();
    }
}


Microsoft.Crm.Client.Core.Framework.List$1 = function Microsoft_Crm_Client_Core_Framework_List$1(items) {
    this._items$p$0 = items || new Array(0);
}
Microsoft.Crm.Client.Core.Framework.List$1.$$ = function Microsoft_Crm_Client_Core_Framework_List$1$$$(T) {
    var $$cn = 'List$1' + '$' + T.getName().replace(/\./g, '_');
    if (!Microsoft.Crm.Client.Core.Framework[$$cn]) {
        var $$ccr = Microsoft.Crm.Client.Core.Framework[$$cn] = function() {
            (this.$$gta = this.$$gta || {})['Microsoft.Crm.Client.Core.Framework.List$1'] = {'T': T};
            var newArgs = [];
            for (var i = 0; i < arguments.length; ++i) {
                newArgs[i] = arguments[i];
            }
            Microsoft.Crm.Client.Core.Framework.List$1.apply(this, newArgs);
        };
        $$ccr.registerClass('Microsoft.Crm.Client.Core.Framework.' + $$cn, null, Microsoft.Crm.Client.Core.Framework.IList$1.$$(T));
        var $$dict_5 = Microsoft.Crm.Client.Core.Framework.List$1.prototype;
        for (var $$key_6 in $$dict_5) {
            var $$entry_7 = { key: $$key_6, value: $$dict_5[$$key_6] };
            if ('constructor' !== $$entry_7.key) {
                $$ccr.prototype[$$entry_7.key] = $$entry_7.value;
            }
        }
    }
    return Microsoft.Crm.Client.Core.Framework[$$cn];
}
Microsoft.Crm.Client.Core.Framework.List$1.prototype = {
    _items$p$0: null,
    
    get_Items: function Microsoft_Crm_Client_Core_Framework_List$1$get_Items() {
        return this._items$p$0;
    },
    
    get_Count: function Microsoft_Crm_Client_Core_Framework_List$1$get_Count() {
        return this._items$p$0.length;
    },
    
    get_item: function Microsoft_Crm_Client_Core_Framework_List$1$get_item(index) {
        return this._items$p$0[index];
    },
    
    set_item: function Microsoft_Crm_Client_Core_Framework_List$1$set_item(index, value) {
        this._items$p$0[index] = value;
        return value;
    },
    
    add: function Microsoft_Crm_Client_Core_Framework_List$1$add(item) {
        Array.add(this._items$p$0, item);
    },
    
    addRange: function Microsoft_Crm_Client_Core_Framework_List$1$addRange(items) {
        Array.addRange(this._items$p$0, items);
    },
    
    clear: function Microsoft_Crm_Client_Core_Framework_List$1$clear() {
        Array.clear(this._items$p$0);
    },
    
    contains: function Microsoft_Crm_Client_Core_Framework_List$1$contains(item) {
        return Array.contains(this._items$p$0, item);
    },
    
    indexOf: function Microsoft_Crm_Client_Core_Framework_List$1$indexOf(item, startIndex) {
        startIndex = startIndex || 0;
        return Array.indexOf(this._items$p$0, item, startIndex);
    },
    
    insert: function Microsoft_Crm_Client_Core_Framework_List$1$insert(index, item) {
        Array.insert(this._items$p$0, index, item);
    },
    
    remove: function Microsoft_Crm_Client_Core_Framework_List$1$remove(item) {
        Array.remove(this._items$p$0, item);
    },
    
    removeAt: function Microsoft_Crm_Client_Core_Framework_List$1$removeAt(index) {
        Array.removeAt(this._items$p$0, index);
    },
    
    sort: function Microsoft_Crm_Client_Core_Framework_List$1$sort(compareCallback) {
        if (_Script.isNullOrUndefined(compareCallback)) {
            (this._items$p$0).sort();
        }
        else {
            (this._items$p$0).sort(compareCallback);
        }
    },
    
    toArray: function Microsoft_Crm_Client_Core_Framework_List$1$toArray() {
        var result = new Array(this.get_Count());
        for (var i = 0; i < this.get_Count(); i++) {
            result[i] = this.get_item(i);
        }
        return result;
    }
}


function HtmlEncoder() {
}
HtmlEncoder.encode = function HtmlEncoder$encode(rawHtml) {
    if (Microsoft.Crm.Client.Core.Framework._String.isNullOrEmpty(rawHtml)) {
        return rawHtml;
    }
    var encodedHtml = rawHtml.replace(HtmlEncoder._ampersand$p, '&amp;');
    encodedHtml = encodedHtml.replace(HtmlEncoder._lessThan$p, '&lt;');
    encodedHtml = encodedHtml.replace(HtmlEncoder._greaterThan$p, '&gt;');
    encodedHtml = encodedHtml.replace(HtmlEncoder._apostrophe$p, '&apos;');
    encodedHtml = encodedHtml.replace(HtmlEncoder._quotation$p, '&quot;');
    return encodedHtml;
}
HtmlEncoder.decode = function HtmlEncoder$decode(encodedHtml) {
    if (Microsoft.Crm.Client.Core.Framework._String.isNullOrEmpty(encodedHtml)) {
        return encodedHtml;
    }
    var decodedHtml = encodedHtml.replace(HtmlEncoder._encodedQuotation$p, '\"');
    decodedHtml = decodedHtml.replace(HtmlEncoder._encodedApostrophe$p, '\'');
    decodedHtml = decodedHtml.replace(HtmlEncoder._encodedGreaterThan$p, '>');
    decodedHtml = decodedHtml.replace(HtmlEncoder._encodedLessThan$p, '<');
    decodedHtml = decodedHtml.replace(HtmlEncoder._encodedAmpersand$p, '&');
    return decodedHtml;
}


Microsoft.Crm.Client.Core.Framework.Debug = function Microsoft_Crm_Client_Core_Framework_Debug() {
}
Microsoft.Crm.Client.Core.Framework.Debug.$$cctor = function Microsoft_Crm_Client_Core_Framework_Debug$$$cctor() {
    Sys.Browser.hasDebuggerStatement = true;
}
Microsoft.Crm.Client.Core.Framework.Debug.assert = function Microsoft_Crm_Client_Core_Framework_Debug$assert(condition, message) {
    if (!condition) {
        Sys.Debug.assert(false, message);
    }
}
Microsoft.Crm.Client.Core.Framework.Debug.fail = function Microsoft_Crm_Client_Core_Framework_Debug$fail(message) {
    Microsoft.Crm.Client.Core.Framework.Debug.assert(false, message);
}


function OptionalParameter() {
}
OptionalParameter.getValue = function OptionalParameter$getValue(T, value) {
    return OptionalParameter.getValueByType(T, value);
}
OptionalParameter.getValueByType = function OptionalParameter$getValueByType(type, value) {
    if (_Script.isNullOrUndefined(value)) {
        if (type === Number || Type.isEnum(type)) {
            return 0;
        }
        else if (type === Boolean) {
            return false;
        }
        else {
            return null;
        }
    }
    else {
        return value;
    }
}


Type.registerNamespace('Microsoft.Crm.Client.Core.Framework.Utils');

Microsoft.Crm.Client.Core.Framework.Utils.XmlParser = function Microsoft_Crm_Client_Core_Framework_Utils_XmlParser() {
}
Microsoft.Crm.Client.Core.Framework.Utils.XmlParser.createFromXml = function Microsoft_Crm_Client_Core_Framework_Utils_XmlParser$createFromXml(T, xml) {
    if (Microsoft.Crm.Client.Core.Framework._String.isNullOrWhiteSpace(xml)) {
        return ((T === Number || Type.isEnum(T)) ? 0 : (T === Boolean) ? false : null);
    }
    var doc = Sys.Net.XMLDOM(xml);
    return Microsoft.Crm.Client.Core.Framework.Utils.XmlParser._createFromXmlInternal$p(doc.documentElement);
}
Microsoft.Crm.Client.Core.Framework.Utils.XmlParser._createFromXmlInternal$p = function Microsoft_Crm_Client_Core_Framework_Utils_XmlParser$_createFromXmlInternal$p(node) {
    var result = {};
    for (var $$arr_2 = node.childNodes, $$len_3 = $$arr_2.length, $$idx_4 = 0; $$idx_4 < $$len_3; ++$$idx_4) {
        var childNode = $$arr_2[$$idx_4];
        var key = _XMLNode.getLocalName(childNode);
        if (key !== Microsoft.Crm.Client.Core.Framework.Utils.XmlParser._textNodeName$p) {
            var value;
            if (!childNode.hasChildNodes() || (childNode.childNodes.length === 1 && _XMLNode.getLocalName(childNode.childNodes[0]) === Microsoft.Crm.Client.Core.Framework.Utils.XmlParser._textNodeName$p)) {
                value = _XMLNode.getInnerText(childNode);
            }
            else {
                value = Microsoft.Crm.Client.Core.Framework.Utils.XmlParser._createFromXmlInternal$p(childNode);
            }
            result[key] = value;
        }
    }
    return result;
}
Microsoft.Crm.Client.Core.Framework.Utils.XmlParser.getInnerXml = function Microsoft_Crm_Client_Core_Framework_Utils_XmlParser$getInnerXml(node) {
    var innerNodeList = node.childNodes();
    var innerXml = Microsoft.Crm.Client.Core.Framework._String.empty;
    var tempNode = null;
    for (var i = 0; i < innerNodeList.get_count(); i++) {
        tempNode = innerNodeList.get_item(i);
        innerXml += tempNode.get_outerXml().toString();
    }
    return innerXml;
}


Microsoft.Crm.Client.Core.Framework.Utils.ExceptionHelpers = function Microsoft_Crm_Client_Core_Framework_Utils_ExceptionHelpers() {
}
Microsoft.Crm.Client.Core.Framework.Utils.ExceptionHelpers.throwOnNullArgument = function Microsoft_Crm_Client_Core_Framework_Utils_ExceptionHelpers$throwOnNullArgument(argument, argumentName) {
    if (_Script.isNull(argument)) {
        throw Error.argumentNull(argumentName, 'Argument can\'t be null');
    }
}
Microsoft.Crm.Client.Core.Framework.Utils.ExceptionHelpers.throwOnNullOrUndefinedArgument = function Microsoft_Crm_Client_Core_Framework_Utils_ExceptionHelpers$throwOnNullOrUndefinedArgument(argument, argumentName) {
    if (_Script.isNullOrUndefined(argument)) {
        throw Error.argumentNull(argumentName, 'Argument can\'t be null or undefined');
    }
}
Microsoft.Crm.Client.Core.Framework.Utils.ExceptionHelpers.throwOnUndefinedArgument = function Microsoft_Crm_Client_Core_Framework_Utils_ExceptionHelpers$throwOnUndefinedArgument(argument, argumentName) {
    if (_Script.isUndefined(argument)) {
        throw Error.argumentNull(argumentName, 'Argument can\'t be undefined');
    }
}
Microsoft.Crm.Client.Core.Framework.Utils.ExceptionHelpers.throwOnNullOrEmptyArgument = function Microsoft_Crm_Client_Core_Framework_Utils_ExceptionHelpers$throwOnNullOrEmptyArgument(str, argumentName) {
    if (!str || !str.length) {
        throw Error.argumentNull(argumentName, 'Argument can\'t be null or empty');
    }
}
Microsoft.Crm.Client.Core.Framework.Utils.ExceptionHelpers.throwOnNullOrEmptyArrayArgument = function Microsoft_Crm_Client_Core_Framework_Utils_ExceptionHelpers$throwOnNullOrEmptyArrayArgument(array, argumentName) {
    if (!array || !array.length) {
        throw Error.argumentNull(argumentName, 'Argument can\'t be null or empty');
    }
}
Microsoft.Crm.Client.Core.Framework.Utils.ExceptionHelpers.throwOnEquals = function Microsoft_Crm_Client_Core_Framework_Utils_ExceptionHelpers$throwOnEquals(argument, unexpectedValue, argumentName) {
    if (argument === unexpectedValue) {
        throw Error.argument(argumentName, 'Argument value should not be equal to ' + unexpectedValue.toString());
    }
}
Microsoft.Crm.Client.Core.Framework.Utils.ExceptionHelpers.throwOnNotEquals = function Microsoft_Crm_Client_Core_Framework_Utils_ExceptionHelpers$throwOnNotEquals(argument, expectedValue, argumentName) {
    if (argument !== expectedValue) {
        throw Error.argument(argumentName, 'Argument is ' + argument + 'but should be equal to ' + expectedValue);
    }
}
Microsoft.Crm.Client.Core.Framework.Utils.ExceptionHelpers.throwOnOutOfRange = function Microsoft_Crm_Client_Core_Framework_Utils_ExceptionHelpers$throwOnOutOfRange(value, minValue, maxValue, argumentName) {
    if (value < minValue || value > maxValue) {
        throw Error.argumentOutOfRange(argumentName);
    }
}
Microsoft.Crm.Client.Core.Framework.Utils.ExceptionHelpers.throwOnAssert = function Microsoft_Crm_Client_Core_Framework_Utils_ExceptionHelpers$throwOnAssert(condition, message) {
    if (!condition) {
        throw Error.create('ExceptionHelpers.ThrowOnAssert(' + message + ')');
    }
}


Type.registerNamespace('Microsoft.Crm.Client.Core.Storage.Common.Xml');

Microsoft.Crm.Client.Core.Storage.Common.Xml.IXmlNodeList = function() {}
Microsoft.Crm.Client.Core.Storage.Common.Xml.IXmlNodeList.registerInterface('Microsoft.Crm.Client.Core.Storage.Common.Xml.IXmlNodeList');


Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlEvaluatorType = function() {}
Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlEvaluatorType.prototype = {
    undefined: -1, 
    xPath: 0, 
    domParser: 1
}
Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlEvaluatorType.registerEnum('Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlEvaluatorType', false);


Microsoft.Crm.Client.Core.Storage.Common.Xml._domParserNodeListWrapper = function Microsoft_Crm_Client_Core_Storage_Common_Xml__domParserNodeListWrapper(elementList) {
    Microsoft.Crm.Client.Core.Framework.Debug.assert(!_Script.isNullOrUndefined(elementList), 'Node list obj should not be null');
    this._list$p$0 = new (Microsoft.Crm.Client.Core.Framework.List$1.$$(Microsoft.Crm.Client.Core.Storage.Common.Xml._domParserNodeWrapper))();
    for (var i = 0; i < elementList.length; i++) {
        this._list$p$0.add(elementList[i]);
    }
}
Microsoft.Crm.Client.Core.Storage.Common.Xml._domParserNodeListWrapper.prototype = {
    _list$p$0: null,
    
    get_count: function Microsoft_Crm_Client_Core_Storage_Common_Xml__domParserNodeListWrapper$get_count() {
        return this._list$p$0.get_Count();
    },
    
    get_item: function Microsoft_Crm_Client_Core_Storage_Common_Xml__domParserNodeListWrapper$get_item(index) {
        return this._list$p$0.get_Items()[index];
    }
}


Microsoft.Crm.Client.Core.Storage.Common.Xml._domParserNodeWrapper = function Microsoft_Crm_Client_Core_Storage_Common_Xml__domParserNodeWrapper(element, namespaces) {
    Microsoft.Crm.Client.Core.Storage.Common.Xml._domParserNodeWrapper.initializeBase(this);
    Microsoft.Crm.Client.Core.Framework.Debug.assert(!_Script.isNullOrUndefined(element), 'The element should not be null');
    this._element$p$1 = element;
    this._namespaces$p$1 = (_Script.isNullOrUndefined(namespaces)) ? {} : namespaces;
}
Microsoft.Crm.Client.Core.Storage.Common.Xml._domParserNodeWrapper.prototype = {
    _element$p$1: null,
    _namespaces$p$1: null,
    
    get_innerText: function Microsoft_Crm_Client_Core_Storage_Common_Xml__domParserNodeWrapper$get_innerText() {
        return (this._element$p$1.hasChildNodes()) ? this._element$p$1.firstChild.nodeValue : null;
    },
    
    get_outerXml: function Microsoft_Crm_Client_Core_Storage_Common_Xml__domParserNodeWrapper$get_outerXml() {
        return new XMLSerializer().serializeToString((this._element$p$1));
    },
    
    get_innerHtml: function Microsoft_Crm_Client_Core_Storage_Common_Xml__domParserNodeWrapper$get_innerHtml() {
        return Microsoft.Crm.Client.Core.Framework._String.empty;
    },
    
    get_tagName: function Microsoft_Crm_Client_Core_Storage_Common_Xml__domParserNodeWrapper$get_tagName() {
        return this._element$p$1.nodeName;
    },
    
    get_domParserElement: function Microsoft_Crm_Client_Core_Storage_Common_Xml__domParserNodeWrapper$get_domParserElement() {
        return this._element$p$1;
    },
    
    getAttribute: function Microsoft_Crm_Client_Core_Storage_Common_Xml__domParserNodeWrapper$getAttribute(name) {
        return this._element$p$1.getAttribute(name);
    },
    
    addNamespace: function Microsoft_Crm_Client_Core_Storage_Common_Xml__domParserNodeWrapper$addNamespace(prefix, uri) {
        this._namespaces$p$1[prefix] = uri;
    },
    
    selectSingleNode: function Microsoft_Crm_Client_Core_Storage_Common_Xml__domParserNodeWrapper$selectSingleNode(expression) {
        var list = this._selectDOMParserElements$p$1(expression, true);
        if (!list.get_count()) {
            return null;
        }
        return list.get_item(0);
    },
    
    selectNodes: function Microsoft_Crm_Client_Core_Storage_Common_Xml__domParserNodeWrapper$selectNodes(expression) {
        return this._selectDOMParserElements$p$1(expression, false);
    },
    
    getElementsByTagName: function Microsoft_Crm_Client_Core_Storage_Common_Xml__domParserNodeWrapper$getElementsByTagName(tagName) {
        var listTagName;
        if (_Script.isNullOrUndefined(this._element$p$1.ownerDocument)) {
            listTagName = this._element$p$1.getElementsByTagName(tagName);
        }
        else {
            listTagName = this._element$p$1.ownerDocument.getElementsByTagName(tagName);
        }
        var listByNodeName = [];
        for (var i = 0; i < listTagName.length; i++) {
            Array.add(listByNodeName, listTagName[i]);
        }
        var listTagLocalName = this._getElementsByTagNameWithDefaultNamespace$p$1(tagName);
        Array.addRange(listByNodeName, listTagLocalName);
        var parserNodeWrapperList = [];
        for (var i = 0; i < listByNodeName.length; i++) {
            Array.add(parserNodeWrapperList, new Microsoft.Crm.Client.Core.Storage.Common.Xml._domParserNodeWrapper(listByNodeName[i], this._namespaces$p$1));
        }
        return new Microsoft.Crm.Client.Core.Storage.Common.Xml._domParserNodeListWrapper(parserNodeWrapperList);
    },
    
    childNodes: function Microsoft_Crm_Client_Core_Storage_Common_Xml__domParserNodeWrapper$childNodes() {
        var parserNodeWrapperList = [];
        for (var i = 0, iLen = this._element$p$1.childNodes.length; i < iLen; i++) {
            Array.add(parserNodeWrapperList, new Microsoft.Crm.Client.Core.Storage.Common.Xml._domParserNodeWrapper(this._element$p$1.childNodes[i], this._namespaces$p$1));
        }
        return new Microsoft.Crm.Client.Core.Storage.Common.Xml._domParserNodeListWrapper(parserNodeWrapperList);
    },
    
    hasChildNodes: function Microsoft_Crm_Client_Core_Storage_Common_Xml__domParserNodeWrapper$hasChildNodes() {
        return this._element$p$1.hasChildNodes();
    },
    
    _selectDOMParserElements$p$1: function Microsoft_Crm_Client_Core_Storage_Common_Xml__domParserNodeWrapper$_selectDOMParserElements$p$1(expression, singleNode) {
        var list = [];
        if (expression.startsWith('//') || expression.startsWith('.//')) {
            var offset = 2;
            var elementContext = false;
            if (expression.startsWith('.')) {
                offset++;
                elementContext = true;
            }
            expression = expression.substr(offset);
            var indexOfSeparator = expression.indexOf('/');
            var compareCriteria, subExpression;
            if (indexOfSeparator < 0) {
                compareCriteria = expression;
                subExpression = null;
            }
            else {
                compareCriteria = expression.substr(0, indexOfSeparator);
                subExpression = expression.substr(indexOfSeparator + 1);
            }
            indexOfSeparator = compareCriteria.indexOf('[');
            var nodeName, filterPattern;
            if (indexOfSeparator < 0) {
                nodeName = compareCriteria;
                filterPattern = null;
            }
            else {
                nodeName = compareCriteria.substr(0, indexOfSeparator);
                filterPattern = compareCriteria.substr(indexOfSeparator + 1);
                indexOfSeparator = filterPattern.indexOf(']');
                filterPattern = filterPattern.substr(0, indexOfSeparator);
            }
            var listTagName;
            if (elementContext || _Script.isNullOrUndefined(this._element$p$1.ownerDocument)) {
                listTagName = this._element$p$1.getElementsByTagName(nodeName);
            }
            else {
                listTagName = this._element$p$1.ownerDocument.getElementsByTagName(nodeName);
            }
            var listByNodeName = [];
            for (var i = 0; i < listTagName.length; i++) {
                Array.add(listByNodeName, listTagName[i]);
            }
            var listTagLocalName = this._getElementsByTagNameWithDefaultNamespace$p$1(nodeName);
            Array.addRange(listByNodeName, listTagLocalName);
            var result = listByNodeName;
            if (listByNodeName.length > 0 && null !== filterPattern) {
                result = this._filterDOMParserElementList$p$1(listByNodeName, filterPattern);
            }
            if (!result.length || null === subExpression) {
                list = result;
            }
            else {
                list = this._searchDOMParserElementList$p$1(result, subExpression, singleNode);
            }
        }
        else if (expression.startsWith('/')) {
            if (_Script.isNullOrUndefined(this._element$p$1.ownerDocument)) {
                Array.add(list, this._element$p$1);
            }
            else {
                Array.add(list, this._element$p$1.ownerDocument);
            }
            list = this._searchDOMParserElementList$p$1(list, expression.substr(1), singleNode);
        }
        else {
            Array.add(list, this._element$p$1);
            list = this._searchDOMParserElementList$p$1(list, expression, singleNode);
        }
        var parserNodeWrapperList = [];
        for (var i = 0; i < list.length; i++) {
            Array.add(parserNodeWrapperList, new Microsoft.Crm.Client.Core.Storage.Common.Xml._domParserNodeWrapper(list[i], this._namespaces$p$1));
        }
        return new Microsoft.Crm.Client.Core.Storage.Common.Xml._domParserNodeListWrapper(parserNodeWrapperList);
    },
    
    _searchDOMParserElementList$p$1: function Microsoft_Crm_Client_Core_Storage_Common_Xml__domParserNodeWrapper$_searchDOMParserElementList$p$1(list, expression, singleNode) {
        if (singleNode) {
            return this._searchSingleNodeDOMParserElementList$p$1(list, expression);
        }
        return this._searchNodesDOMParserElementList$p$1(list, expression);
    },
    
    _searchSingleNodeDOMParserElementList$p$1: function Microsoft_Crm_Client_Core_Storage_Common_Xml__domParserNodeWrapper$_searchSingleNodeDOMParserElementList$p$1(list, expression) {
        if (_Script.isNullOrUndefined(expression) || expression.length < 1) {
            return list;
        }
        var resultList = [];
        for (var i = 0; i < list.length; i++) {
            var foundElement = this._searchSingleNodeDOMParserElement$p$1(list[i], expression);
            if (!_Script.isNullOrUndefined(foundElement)) {
                Array.add(resultList, foundElement);
                break;
            }
        }
        return resultList;
    },
    
    _searchSingleNodeDOMParserElement$p$1: function Microsoft_Crm_Client_Core_Storage_Common_Xml__domParserNodeWrapper$_searchSingleNodeDOMParserElement$p$1(parserElement, expression) {
        if (_Script.isNullOrUndefined(expression) || expression.length < 1) {
            return parserElement;
        }
        var indexOfSeparator = expression.indexOf('/');
        var compareCriteria, subExpression;
        if (indexOfSeparator < 0) {
            compareCriteria = expression;
            subExpression = null;
        }
        else {
            compareCriteria = expression.substr(0, indexOfSeparator);
            subExpression = expression.substr(indexOfSeparator + 1);
        }
        indexOfSeparator = compareCriteria.indexOf('[');
        var nodeName, filterPattern;
        if (indexOfSeparator < 0) {
            nodeName = compareCriteria;
            filterPattern = null;
        }
        else {
            nodeName = compareCriteria.substr(0, indexOfSeparator);
            filterPattern = compareCriteria.substr(indexOfSeparator + 1);
            indexOfSeparator = filterPattern.indexOf(']');
            filterPattern = filterPattern.substr(0, indexOfSeparator);
        }
        var foundElement = null;
        for (var i = 0, iLen = parserElement.childNodes.length; i < iLen; i++) {
            if (this._isNodeNameEqual$p$1(parserElement.childNodes[i], nodeName)) {
                var listByNodeName = [];
                Array.add(listByNodeName, parserElement.childNodes[i]);
                var result = listByNodeName;
                if (null !== filterPattern) {
                    result = this._filterDOMParserElementList$p$1(listByNodeName, filterPattern);
                }
                if (result.length > 0) {
                    if (null === subExpression) {
                        foundElement = result[0];
                        break;
                    }
                    else {
                        foundElement = this._searchSingleNodeDOMParserElement$p$1(result[0], subExpression);
                        if (foundElement) {
                            break;
                        }
                    }
                }
            }
        }
        return foundElement;
    },
    
    _searchNodesDOMParserElementList$p$1: function Microsoft_Crm_Client_Core_Storage_Common_Xml__domParserNodeWrapper$_searchNodesDOMParserElementList$p$1(list, expression) {
        if (_Script.isNullOrUndefined(expression) || expression.length < 1) {
            return list;
        }
        var indexOfSeparator = expression.indexOf('/');
        var compareCriteria, subExpression;
        if (indexOfSeparator < 0) {
            compareCriteria = expression;
            subExpression = null;
        }
        else {
            compareCriteria = expression.substr(0, indexOfSeparator);
            subExpression = expression.substr(indexOfSeparator + 1);
        }
        var updatedList = [];
        var expandedList;
        for (var i = 0; i < list.length; i++) {
            expandedList = this._searchDOMParserElement$p$1(list[i], compareCriteria);
            if (expandedList.length > 0) {
                Array.addRange(updatedList, expandedList);
            }
        }
        if (!updatedList.length || null === subExpression) {
            return updatedList;
        }
        return this._searchNodesDOMParserElementList$p$1(updatedList, subExpression);
    },
    
    _searchDOMParserElement$p$1: function Microsoft_Crm_Client_Core_Storage_Common_Xml__domParserNodeWrapper$_searchDOMParserElement$p$1(parserElement, compareCriteria) {
        var indexOfSeparator = compareCriteria.indexOf('[');
        var nodeName, filterPattern;
        if (indexOfSeparator < 0) {
            nodeName = compareCriteria;
            filterPattern = null;
        }
        else {
            nodeName = compareCriteria.substr(0, indexOfSeparator);
            filterPattern = compareCriteria.substr(indexOfSeparator + 1);
            indexOfSeparator = filterPattern.indexOf(']');
            filterPattern = filterPattern.substr(0, indexOfSeparator);
        }
        var listByNodeName = [];
        for (var i = 0, iLen = parserElement.childNodes.length; i < iLen; i++) {
            if (this._isNodeNameEqual$p$1(parserElement.childNodes[i], nodeName)) {
                Array.add(listByNodeName, parserElement.childNodes[i]);
            }
        }
        var result = listByNodeName;
        if (listByNodeName.length > 0 && null !== filterPattern) {
            result = this._filterDOMParserElementList$p$1(listByNodeName, filterPattern);
        }
        return result;
    },
    
    _filterDOMParserElementList$p$1: function Microsoft_Crm_Client_Core_Storage_Common_Xml__domParserNodeWrapper$_filterDOMParserElementList$p$1(list, filterPattern) {
        var filterByAttribute = filterPattern.startsWith('@');
        if (filterByAttribute) {
            filterPattern = filterPattern.substr(1);
        }
        var indexOfSeparator = filterPattern.indexOf('=');
        var filterKey, filterValue;
        if (indexOfSeparator < 0) {
            filterKey = filterPattern;
            filterValue = null;
        }
        else {
            filterKey = filterPattern.substr(0, indexOfSeparator);
            filterValue = filterPattern.substr(indexOfSeparator + 1);
            filterValue = filterValue.substr(1);
            filterValue = filterValue.substr(0, filterValue.length - 1);
        }
        var result = [];
        for (var i = 0; i < list.length; i++) {
            var parserElement = list[i];
            if (filterByAttribute && this._filterDOMParserElementAttribute$p$1(parserElement, filterKey, filterValue)) {
                Array.add(result, parserElement);
            }
            else if (this._filterDOMParserElementChildElement$p$1(parserElement, filterKey, filterValue)) {
                Array.add(result, parserElement);
            }
        }
        return result;
    },
    
    _filterDOMParserElementAttribute$p$1: function Microsoft_Crm_Client_Core_Storage_Common_Xml__domParserNodeWrapper$_filterDOMParserElementAttribute$p$1(parserElement, attributeName, attributeValue) {
        if (null === attributeValue) {
            return parserElement.hasAttribute(attributeName);
        }
        return attributeValue === parserElement.getAttribute(attributeName);
    },
    
    _filterDOMParserElementChildElement$p$1: function Microsoft_Crm_Client_Core_Storage_Common_Xml__domParserNodeWrapper$_filterDOMParserElementChildElement$p$1(parserElement, nodeName, nodeValue) {
        for (var i = 0, iLen = parserElement.childNodes.length; i < iLen; i++) {
            if (this._isNodeNameEqual$p$1(parserElement.childNodes[i], nodeName)) {
                if (null === nodeValue) {
                    return true;
                }
                else if (parserElement.childNodes[i].firstChild.nodeValue === nodeValue) {
                    return true;
                }
            }
        }
        return false;
    },
    
    _getElementsByTagNameWithDefaultNamespace$p$1: function Microsoft_Crm_Client_Core_Storage_Common_Xml__domParserNodeWrapper$_getElementsByTagNameWithDefaultNamespace$p$1(nodeNameWithDefaultNamespace) {
        var elementsList = [];
        var nodeNamespace = this._getNamespaceForFullName$p$1(nodeNameWithDefaultNamespace);
        if (_Script.isNullOrUndefined(nodeNamespace) || nodeNamespace.length < 1) {
            return elementsList;
        }
        var nodeName = this._getLocalNameForFullName$p$1(nodeNameWithDefaultNamespace);
        var listTagName;
        if (_Script.isNullOrUndefined(this._element$p$1.ownerDocument)) {
            listTagName = this._element$p$1.getElementsByTagName(nodeName);
        }
        else {
            listTagName = this._element$p$1.ownerDocument.getElementsByTagName(nodeName);
        }
        for (var i = 0; i < listTagName.length; i++) {
            if (listTagName[i].namespaceURI === nodeNamespace) {
                Array.add(elementsList, listTagName[i]);
            }
        }
        return elementsList;
    },
    
    _getNamespaceForFullName$p$1: function Microsoft_Crm_Client_Core_Storage_Common_Xml__domParserNodeWrapper$_getNamespaceForFullName$p$1(fullName) {
        var indexOfSeparator = fullName.indexOf(':');
        if (indexOfSeparator < 0) {
            return Microsoft.Crm.Client.Core.Framework._String.empty;
        }
        var prefix = fullName.substr(0, indexOfSeparator);
        var nodeNamespace = this._namespaces$p$1[prefix];
        return nodeNamespace;
    },
    
    _getLocalNameForFullName$p$1: function Microsoft_Crm_Client_Core_Storage_Common_Xml__domParserNodeWrapper$_getLocalNameForFullName$p$1(fullName) {
        var indexOfSeparator = fullName.indexOf(':');
        if (indexOfSeparator < 0) {
            return fullName;
        }
        return fullName.substr(indexOfSeparator + 1);
    },
    
    _isNodeNameEqual$p$1: function Microsoft_Crm_Client_Core_Storage_Common_Xml__domParserNodeWrapper$_isNodeNameEqual$p$1(parserElement, fullNodeName) {
        if (parserElement.nodeName === fullNodeName || '*' === fullNodeName) {
            return true;
        }
        var nodeNamespace = this._getNamespaceForFullName$p$1(fullNodeName);
        if (_Script.isNullOrUndefined(nodeNamespace) || nodeNamespace.length < 1) {
            return false;
        }
        var nodeName = this._getLocalNameForFullName$p$1(fullNodeName);
        return parserElement.namespaceURI === nodeNamespace && parserElement.localName === nodeName;
    }
}


Microsoft.Crm.Client.Core.Storage.Common.Xml._nodeSnapshotWrapper = function Microsoft_Crm_Client_Core_Storage_Common_Xml__nodeSnapshotWrapper(evaluator, obj, namespaces) {
    Microsoft.Crm.Client.Core.Framework.Debug.assert(!_Script.isNullOrUndefined(evaluator), 'XPathEvaluator should not be null');
    Microsoft.Crm.Client.Core.Framework.Debug.assert(!_Script.isNullOrUndefined(obj), 'Node list obj should not be null');
    this._list$p$0 = [];
    var collection = obj;
    if (!_Script.isNullOrUndefined(collection.length)) {
        for (var i = 0; i < collection.length; i++) {
            Array.add(this._list$p$0, new Microsoft.Crm.Client.Core.Storage.Common.Xml._xPathEvaluatorWrapper(evaluator, collection[i], namespaces));
        }
    }
    else {
        var result = obj;
        for (var i = 0; i < result.snapshotLength; i++) {
            Array.add(this._list$p$0, new Microsoft.Crm.Client.Core.Storage.Common.Xml._xPathEvaluatorWrapper(evaluator, result.snapshotItem(i), namespaces));
        }
    }
}
Microsoft.Crm.Client.Core.Storage.Common.Xml._nodeSnapshotWrapper.prototype = {
    _list$p$0: null,
    
    get_count: function Microsoft_Crm_Client_Core_Storage_Common_Xml__nodeSnapshotWrapper$get_count() {
        return this._list$p$0.length;
    },
    
    get_item: function Microsoft_Crm_Client_Core_Storage_Common_Xml__nodeSnapshotWrapper$get_item(index) {
        return this._list$p$0[index];
    }
}


Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlNode = function Microsoft_Crm_Client_Core_Storage_Common_Xml_XmlNode() {
}
Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlNode.createAttribute = function Microsoft_Crm_Client_Core_Storage_Common_Xml_XmlNode$createAttribute(doc, name, value) {
    var attrib = doc.createAttribute(name);
    attrib.value = value;
    return attrib;
}


Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlNodeFactory = function Microsoft_Crm_Client_Core_Storage_Common_Xml_XmlNodeFactory() {
}
Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlNodeFactory.get__preferredXmlEvaluator$p = function Microsoft_Crm_Client_Core_Storage_Common_Xml_XmlNodeFactory$get__preferredXmlEvaluator$p() {
    return Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlNodeFactory._preferredXmlEvaluator$p;
}
Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlNodeFactory.set__preferredXmlEvaluator$p = function Microsoft_Crm_Client_Core_Storage_Common_Xml_XmlNodeFactory$set__preferredXmlEvaluator$p(value) {
    Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlNodeFactory._preferredXmlEvaluator$p = value;
    return value;
}
Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlNodeFactory.create = function Microsoft_Crm_Client_Core_Storage_Common_Xml_XmlNodeFactory$create(node) {
    if (_Script.isNullOrUndefined(node)) {
        return null;
    }
    if (Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlNodeFactory.get__preferredXmlEvaluator$p() === Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlEvaluatorType.undefined) {
        Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlNodeFactory._determineXmlEvaluatorType$p(node);
    }
    if (!Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlNodeFactory.get__preferredXmlEvaluator$p()) {
        return new Microsoft.Crm.Client.Core.Storage.Common.Xml._xPathEvaluatorWrapper(node);
    }
    else {
        return new Microsoft.Crm.Client.Core.Storage.Common.Xml._domParserNodeWrapper(node);
    }
}
Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlNodeFactory.parseXmlDocument = function Microsoft_Crm_Client_Core_Storage_Common_Xml_XmlNodeFactory$parseXmlDocument(xml) {
    var xmlDocument = null;
    if (_Script.isNullOrUndefined(xmlDocument)) {
        xmlDocument = Sys.Net.XMLDOM(xml);
    }
    if (!_Script.isNullOrUndefined(xmlDocument)) {
        var errorNodes = xmlDocument.getElementsByTagName('parsererror');
        if (!_Script.isNullOrUndefined(errorNodes) && errorNodes.length > 0) {
            xmlDocument = null;
        }
    }
    return xmlDocument;
}
Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlNodeFactory._determineXmlEvaluatorType$p = function Microsoft_Crm_Client_Core_Storage_Common_Xml_XmlNodeFactory$_determineXmlEvaluatorType$p(node) {
    if (Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlNodeFactory._browserSupportsXPathEvaluate$p()) {
        var xmlNode = new Microsoft.Crm.Client.Core.Storage.Common.Xml._xPathEvaluatorWrapper(node);
        try {
            xmlNode.selectSingleNode('/root');
            Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlNodeFactory.set__preferredXmlEvaluator$p(0);
        }
        catch ($$e_2) {
        }
    }
    if (Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlNodeFactory.get__preferredXmlEvaluator$p() === Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlEvaluatorType.undefined) {
        if (Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlNodeFactory._browserSupportsDomParser$p()) {
            Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlNodeFactory.set__preferredXmlEvaluator$p(Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlEvaluatorType.domParser);
        }
    }
    if (Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlNodeFactory.get__preferredXmlEvaluator$p() === Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlEvaluatorType.undefined) {
        Microsoft.Crm.Client.Core.Framework.Utils.ExceptionHelpers.throwOnNullOrUndefinedArgument(Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlNodeFactory.get__preferredXmlEvaluator$p(), 'PreferredXmlEvaluator');
    }
}
Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlNodeFactory._browserSupportsXPathEvaluate$p = function Microsoft_Crm_Client_Core_Storage_Common_Xml_XmlNodeFactory$_browserSupportsXPathEvaluate$p() {
    var documentEvalute = document.evaluate;
    return !_Script.isNullOrUndefined(documentEvalute);
}
Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlNodeFactory._browserSupportsDomParser$p = function Microsoft_Crm_Client_Core_Storage_Common_Xml_XmlNodeFactory$_browserSupportsDomParser$p() {
    var windowDomParser = window.DOMParser;
    return !_Script.isNullOrUndefined(windowDomParser);
}


Microsoft.Crm.Client.Core.Storage.Common.Xml._xPathEvaluatorWrapper = function Microsoft_Crm_Client_Core_Storage_Common_Xml__xPathEvaluatorWrapper(evaluator, element, namespaces) {
    this.$$d__resolveNamespaces$p$1 = Function.createDelegate(this, this._resolveNamespaces$p$1);
    Microsoft.Crm.Client.Core.Storage.Common.Xml._xPathEvaluatorWrapper.initializeBase(this);
    Microsoft.Crm.Client.Core.Framework.Debug.assert(!_Script.isNullOrUndefined(evaluator), 'The XPathEvaluator should not be null');
    this._evaluator$p$1 = evaluator;
    this._element$p$1 = (_Script.isNullOrUndefined(element)) ? evaluator : element;
    this._namespaces$p$1 = (_Script.isNullOrUndefined(namespaces)) ? {} : namespaces;
}
Microsoft.Crm.Client.Core.Storage.Common.Xml._xPathEvaluatorWrapper.prototype = {
    _namespaces$p$1: null,
    _evaluator$p$1: null,
    _element$p$1: null,
    
    get_innerText: function Microsoft_Crm_Client_Core_Storage_Common_Xml__xPathEvaluatorWrapper$get_innerText() {
        return (this._element$p$1.hasChildNodes()) ? this._element$p$1.firstChild.nodeValue : null;
    },
    
    get_innerHtml: function Microsoft_Crm_Client_Core_Storage_Common_Xml__xPathEvaluatorWrapper$get_innerHtml() {
        return (this._element$p$1.hasChildNodes()) ? HtmlEncoder.decode(this._element$p$1.innerHTML) : null;
    },
    
    get_outerXml: function Microsoft_Crm_Client_Core_Storage_Common_Xml__xPathEvaluatorWrapper$get_outerXml() {
        return new XMLSerializer().serializeToString(this._element$p$1);
    },
    
    get_tagName: function Microsoft_Crm_Client_Core_Storage_Common_Xml__xPathEvaluatorWrapper$get_tagName() {
        return this._element$p$1.tagName;
    },
    
    get_domParserElement: function Microsoft_Crm_Client_Core_Storage_Common_Xml__xPathEvaluatorWrapper$get_domParserElement() {
        return this._element$p$1;
    },
    
    addNamespace: function Microsoft_Crm_Client_Core_Storage_Common_Xml__xPathEvaluatorWrapper$addNamespace(prefix, uri) {
        this._namespaces$p$1[prefix] = uri;
    },
    
    selectSingleNode: function Microsoft_Crm_Client_Core_Storage_Common_Xml__xPathEvaluatorWrapper$selectSingleNode(expression) {
        var result = this._evaluator$p$1.evaluate(expression, this._element$p$1, this.$$d__resolveNamespaces$p$1, Microsoft.Crm.Client.Core.Imported.XPathResultType.FIRST_ORDERED_NODE_TYPE, null);
        return (_Script.isNullOrUndefined(result.singleNodeValue)) ? null : new Microsoft.Crm.Client.Core.Storage.Common.Xml._xPathEvaluatorWrapper(this._evaluator$p$1, result.singleNodeValue, this._namespaces$p$1);
    },
    
    selectNodes: function Microsoft_Crm_Client_Core_Storage_Common_Xml__xPathEvaluatorWrapper$selectNodes(expression) {
        return new Microsoft.Crm.Client.Core.Storage.Common.Xml._nodeSnapshotWrapper(this._evaluator$p$1, this._evaluator$p$1.evaluate(expression, this._element$p$1, this.$$d__resolveNamespaces$p$1, Microsoft.Crm.Client.Core.Imported.XPathResultType.ORDERED_NODE_SNAPSHOT_TYPE, null), this._namespaces$p$1);
    },
    
    getAttribute: function Microsoft_Crm_Client_Core_Storage_Common_Xml__xPathEvaluatorWrapper$getAttribute(name) {
        return this._element$p$1.getAttribute(name);
    },
    
    getElementsByTagName: function Microsoft_Crm_Client_Core_Storage_Common_Xml__xPathEvaluatorWrapper$getElementsByTagName(tagName) {
        return new Microsoft.Crm.Client.Core.Storage.Common.Xml._nodeSnapshotWrapper(this._evaluator$p$1, this._element$p$1.getElementsByTagName(tagName), this._namespaces$p$1);
    },
    
    childNodes: function Microsoft_Crm_Client_Core_Storage_Common_Xml__xPathEvaluatorWrapper$childNodes() {
        return new Microsoft.Crm.Client.Core.Storage.Common.Xml._nodeSnapshotWrapper(this._evaluator$p$1, this._element$p$1.childNodes, this._namespaces$p$1);
    },
    
    hasChildNodes: function Microsoft_Crm_Client_Core_Storage_Common_Xml__xPathEvaluatorWrapper$hasChildNodes() {
        return this._element$p$1.hasChildNodes();
    },
    
    _resolveNamespaces$p$1: function Microsoft_Crm_Client_Core_Storage_Common_Xml__xPathEvaluatorWrapper$_resolveNamespaces$p$1(prefix) {
        if (((prefix) in this._namespaces$p$1)) {
            return this._namespaces$p$1[prefix];
        }
        else {
            return null;
        }
    }
}


Type.registerNamespace('Microsoft.Crm.Client.Core.Imported');

Microsoft.Crm.Client.Core.Imported.XPathResultType = function() {}
Microsoft.Crm.Client.Core.Imported.XPathResultType.prototype = {
    ANY_TYPE: 0, 
    NUMBER_TYPE: 1, 
    STRING_TYPE: 2, 
    BOOLEAN_TYPE: 3, 
    UNORDERED_NODE_ITERATOR_TYPE: 4, 
    ORDERED_NODE_ITERATOR_TYPE: 5, 
    UNORDERED_NODE_SNAPSHOT_TYPE: 6, 
    ORDERED_NODE_SNAPSHOT_TYPE: 7, 
    ANY_UNORDERED_NODE_TYPE: 8, 
    FIRST_ORDERED_NODE_TYPE: 9
}
Microsoft.Crm.Client.Core.Imported.XPathResultType.registerEnum('Microsoft.Crm.Client.Core.Imported.XPathResultType', false);


Microsoft.Crm.Client.Core.Imported.MscrmComponents = function Microsoft_Crm_Client_Core_Imported_MscrmComponents() {
}
Microsoft.Crm.Client.Core.Imported.MscrmComponents.deferredPromiseFactory = function Microsoft_Crm_Client_Core_Imported_MscrmComponents$deferredPromiseFactory(TData, TError) {
    return jQueryApi.jQueryDeferredFactory.Deferred(TData, TError);
}


Microsoft.Crm.Client.Core.Imported.DeferredPromiseHelper = function Microsoft_Crm_Client_Core_Imported_DeferredPromiseHelper() {
}
Microsoft.Crm.Client.Core.Imported.DeferredPromiseHelper.when = function Microsoft_Crm_Client_Core_Imported_DeferredPromiseHelper$when() {
    var deferreds = [];
    for (var $$pai_1 = 0; $$pai_1 < arguments.length; ++$$pai_1) {
        deferreds[$$pai_1] = arguments[$$pai_1];
    }
    return jQueryAjax.$P_CRM.when.apply.apply(null, [ null ].concat(deferreds));
}
Microsoft.Crm.Client.Core.Imported.DeferredPromiseHelper.whenArray = function Microsoft_Crm_Client_Core_Imported_DeferredPromiseHelper$whenArray(deferreds) {
    return $P_CRM.when.apply(null, deferreds);
}


function IsNull(value) {
    return typeof(value) === 'undefined' || typeof(value) === 'unknown' || value == null;
}


Type.registerNamespace('Microsoft.Crm.Client.Core.Attributes');

Microsoft.Crm.Client.Core.Attributes.ExportToTypescriptAttribute = function Microsoft_Crm_Client_Core_Attributes_ExportToTypescriptAttribute() {
    Microsoft.Crm.Client.Core.Attributes.ExportToTypescriptAttribute.initializeBase(this);
}


Type.registerNamespace('Microsoft.Crm.Client.Core.Framework.Common');

Microsoft.Crm.Client.Core.Framework.Common.ImeMode = function() {}
Microsoft.Crm.Client.Core.Framework.Common.ImeMode.prototype = {
    auto: 0, 
    inactive: 1, 
    active: 2, 
    disabled: 3
}
Microsoft.Crm.Client.Core.Framework.Common.ImeMode.registerEnum('Microsoft.Crm.Client.Core.Framework.Common.ImeMode', false);


Microsoft.Crm.Client.Core.SharedScript.registerClass('Microsoft.Crm.Client.Core.SharedScript');
Microsoft.Crm.Client.Core.Framework.CustomControlAttributeProperty.registerClass('Microsoft.Crm.Client.Core.Framework.CustomControlAttributeProperty');
Microsoft.Crm.Client.Core.Framework.CustomControlConstants.registerClass('Microsoft.Crm.Client.Core.Framework.CustomControlConstants');
Microsoft.Crm.Client.Core.Framework.CustomControlUtils.registerClass('Microsoft.Crm.Client.Core.Framework.CustomControlUtils');
Microsoft.Crm.Client.Core.Framework.CallingContext.registerClass('Microsoft.Crm.Client.Core.Framework.CallingContext');
Microsoft.Crm.Client.Core.Framework.DefaultContext.registerClass('Microsoft.Crm.Client.Core.Framework.DefaultContext', Microsoft.Crm.Client.Core.Framework.CallingContext);
Microsoft.Crm.Client.Core.Framework.ErrorData.registerClass('Microsoft.Crm.Client.Core.Framework.ErrorData');
Microsoft.Crm.Client.Core.Framework.ErrorStatus.registerClass('Microsoft.Crm.Client.Core.Framework.ErrorStatus', null, Microsoft.Crm.Client.Core.Framework.ISerializable);
_XMLNode.registerClass('_XMLNode');
Microsoft.Crm.Client.Core.Framework._Dictionary.registerClass('Microsoft.Crm.Client.Core.Framework._Dictionary');
Microsoft.Crm.Client.Core.Framework._Enum.registerClass('Microsoft.Crm.Client.Core.Framework._Enum');
Microsoft.Crm.Client.Core.Framework.DynamicsTrace.registerClass('Microsoft.Crm.Client.Core.Framework.DynamicsTrace');
Microsoft.Crm.Client.Core.Framework.PerformanceMarker.registerClass('Microsoft.Crm.Client.Core.Framework.PerformanceMarker');
Microsoft.Crm.Client.Core.Framework.PerformanceStopwatch.registerClass('Microsoft.Crm.Client.Core.Framework.PerformanceStopwatch');
Microsoft.Crm.Client.Core.Framework.PageLoadPerformanceMarkerNames.registerClass('Microsoft.Crm.Client.Core.Framework.PageLoadPerformanceMarkerNames');
Microsoft.Crm.Client.Core.Framework.AppBootPerformanceMarkerNames.registerClass('Microsoft.Crm.Client.Core.Framework.AppBootPerformanceMarkerNames');
Microsoft.Crm.Client.Core.Framework.DataLayerPerformanceMarkerNames.registerClass('Microsoft.Crm.Client.Core.Framework.DataLayerPerformanceMarkerNames');
Microsoft.Crm.Client.Core.Framework.OfflineDataStoreCRUDPerformanceMarkerNames.registerClass('Microsoft.Crm.Client.Core.Framework.OfflineDataStoreCRUDPerformanceMarkerNames');
Microsoft.Crm.Client.Core.Framework.FieldFormat.registerClass('Microsoft.Crm.Client.Core.Framework.FieldFormat');
Microsoft.Crm.Client.Core.Framework.MetadataTypeName.registerClass('Microsoft.Crm.Client.Core.Framework.MetadataTypeName');
Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.registerClass('Microsoft.Crm.Client.Core.Framework.CrmErrorCodes');
Microsoft.Crm.Client.Core.Framework.Guid.registerClass('Microsoft.Crm.Client.Core.Framework.Guid', null, Microsoft.Crm.Client.Core.Framework.ISerializable);
Microsoft.Crm.Client.Core.Framework.ManifestType.registerClass('Microsoft.Crm.Client.Core.Framework.ManifestType');
Microsoft.Crm.Client.Core.Framework.TimeZoneAdjuster.registerClass('Microsoft.Crm.Client.Core.Framework.TimeZoneAdjuster', null, Microsoft.Crm.Client.Core.Framework.ISerializable);
Microsoft.Crm.Client.Core.Framework.TransitionConstraint.registerClass('Microsoft.Crm.Client.Core.Framework.TransitionConstraint', null, Microsoft.Crm.Client.Core.Framework.ISerializable);
Microsoft.Crm.Client.Core.Framework.Undefined.registerClass('Microsoft.Crm.Client.Core.Framework.Undefined');
_Math.registerClass('_Math');
_Script.registerClass('_Script');
Microsoft.Crm.Client.Core.Framework._String.registerClass('Microsoft.Crm.Client.Core.Framework._String');
HtmlEncoder.registerClass('HtmlEncoder');
Microsoft.Crm.Client.Core.Framework.Debug.registerClass('Microsoft.Crm.Client.Core.Framework.Debug');
OptionalParameter.registerClass('OptionalParameter');
Microsoft.Crm.Client.Core.Framework.Utils.XmlParser.registerClass('Microsoft.Crm.Client.Core.Framework.Utils.XmlParser');
Microsoft.Crm.Client.Core.Framework.Utils.ExceptionHelpers.registerClass('Microsoft.Crm.Client.Core.Framework.Utils.ExceptionHelpers');
Microsoft.Crm.Client.Core.Storage.Common.Xml._domParserNodeListWrapper.registerClass('Microsoft.Crm.Client.Core.Storage.Common.Xml._domParserNodeListWrapper', null, Microsoft.Crm.Client.Core.Storage.Common.Xml.IXmlNodeList);
Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlNode.registerClass('Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlNode');
Microsoft.Crm.Client.Core.Storage.Common.Xml._domParserNodeWrapper.registerClass('Microsoft.Crm.Client.Core.Storage.Common.Xml._domParserNodeWrapper', Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlNode);
Microsoft.Crm.Client.Core.Storage.Common.Xml._nodeSnapshotWrapper.registerClass('Microsoft.Crm.Client.Core.Storage.Common.Xml._nodeSnapshotWrapper', null, Microsoft.Crm.Client.Core.Storage.Common.Xml.IXmlNodeList);
Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlNodeFactory.registerClass('Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlNodeFactory');
Microsoft.Crm.Client.Core.Storage.Common.Xml._xPathEvaluatorWrapper.registerClass('Microsoft.Crm.Client.Core.Storage.Common.Xml._xPathEvaluatorWrapper', Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlNode);
Microsoft.Crm.Client.Core.Imported.MscrmComponents.registerClass('Microsoft.Crm.Client.Core.Imported.MscrmComponents');
Microsoft.Crm.Client.Core.Imported.DeferredPromiseHelper.registerClass('Microsoft.Crm.Client.Core.Imported.DeferredPromiseHelper');
Microsoft.Crm.Client.Core.Attributes.ExportToTypescriptAttribute.registerClass('Microsoft.Crm.Client.Core.Attributes.ExportToTypescriptAttribute', Sys.Attribute);
Microsoft.Crm.Client.Core.Framework.CustomControlConstants.lastUpdatedField = 'lastUpdatedField';
Microsoft.Crm.Client.Core.Framework.CustomControlConstants.lastUpdatedValue = 'lastUpdatedValue';
Microsoft.Crm.Client.Core.Framework.CustomControlConstants.rollupStateField = 'rollupStateField';
Microsoft.Crm.Client.Core.Framework.CustomControlConstants.rollupStateValue = 'rollupStateValue';
Microsoft.Crm.Client.Core.Framework.CustomControlConstants.recalculate = 'recalculate';
Microsoft.Crm.Client.Core.Framework.CustomControlConstants.rollupValid = 'rollupValid';
Microsoft.Crm.Client.Core.Framework.CustomControlConstants.calculatedFieldValid = 'calculatedFieldValid';
Microsoft.Crm.Client.Core.Framework.CustomControlConstants.partyList = 'lookup.partylist';
Microsoft.Crm.Client.Core.Framework.CustomControlConstants.lookupCheck = 'lookup.';
Microsoft.Crm.Client.Core.Framework._Enum._separator$p = ',';
Microsoft.Crm.Client.Core.Framework.DynamicsTrace.storage = 1005;
Microsoft.Crm.Client.Core.Framework.PageLoadPerformanceMarkerNames.pageStart = 'BeforeInitializeStart';
Microsoft.Crm.Client.Core.Framework.PageLoadPerformanceMarkerNames.interactionReady = 'LoadedInteractionReady';
Microsoft.Crm.Client.Core.Framework.PageLoadPerformanceMarkerNames.loadedInitialUI = 'LoadedInitialUI';
Microsoft.Crm.Client.Core.Framework.PageLoadPerformanceMarkerNames.renderedInitialUI = 'RenderedInitialUI';
Microsoft.Crm.Client.Core.Framework.PageLoadPerformanceMarkerNames.loadedInitialData = 'LoadedInitialData';
Microsoft.Crm.Client.Core.Framework.PageLoadPerformanceMarkerNames.renderedInitialData = 'RenderedInitialData';
Microsoft.Crm.Client.Core.Framework.PageLoadPerformanceMarkerNames.loadedRefreshedData = 'RenderedRefreshedData';
Microsoft.Crm.Client.Core.Framework.PageLoadPerformanceMarkerNames.loadedReadReady = 'LoadedReadReady';
Microsoft.Crm.Client.Core.Framework.PageLoadPerformanceMarkerNames.renderedReadReady = 'RenderedReadReady';
Microsoft.Crm.Client.Core.Framework.PageLoadPerformanceMarkerNames.loadedEditReady = 'LoadedEditReady';
Microsoft.Crm.Client.Core.Framework.PageLoadPerformanceMarkerNames.renderedEditReady = 'RenderedEditReady';
Microsoft.Crm.Client.Core.Framework.PageLoadPerformanceMarkerNames.loadedViewportInitialUI = 'LoadedViewportInitialUI';
Microsoft.Crm.Client.Core.Framework.PageLoadPerformanceMarkerNames.renderedViewportInitialUI = 'RenderedViewportInitialUI';
Microsoft.Crm.Client.Core.Framework.PageLoadPerformanceMarkerNames.loadedViewportInitialData = 'LoadedViewportInitialData';
Microsoft.Crm.Client.Core.Framework.PageLoadPerformanceMarkerNames.renderedViewportInitialData = 'RenderedViewportInitialData';
Microsoft.Crm.Client.Core.Framework.PageLoadPerformanceMarkerNames.fullLoad = 'LoadedFull';
Microsoft.Crm.Client.Core.Framework.PageLoadPerformanceMarkerNames.firstQueueLoad = 'RenderedFirstQueueWithData';
Microsoft.Crm.Client.Core.Framework.AppBootPerformanceMarkerNames.bootStart = 'PageLoadStart';
Microsoft.Crm.Client.Core.Framework.AppBootPerformanceMarkerNames.shimInit = 'Shim Init';
Microsoft.Crm.Client.Core.Framework.AppBootPerformanceMarkerNames.cssInit = 'Style Init';
Microsoft.Crm.Client.Core.Framework.AppBootPerformanceMarkerNames.jsInit = 'JavaScript Init';
Microsoft.Crm.Client.Core.Framework.AppBootPerformanceMarkerNames.requireJsInit = 'RequireJs JavaScript Init';
Microsoft.Crm.Client.Core.Framework.AppBootPerformanceMarkerNames.delayedAssetsInit = 'Delayed Assets Init';
Microsoft.Crm.Client.Core.Framework.DataLayerPerformanceMarkerNames.crmSoapServiceCall = 'CrmSoapServiceCall';
Microsoft.Crm.Client.Core.Framework.DataLayerPerformanceMarkerNames.checkRemoteDataSourceUpdates = 'DataSource_CheckRemoteDataSourceUpdates';
Microsoft.Crm.Client.Core.Framework.DataLayerPerformanceMarkerNames.initializeAfterUpdatesCheck = 'DataSource_InitializeAfterUpdatesCheck';
Microsoft.Crm.Client.Core.Framework.DataLayerPerformanceMarkerNames.syncAllEntityAndAttributeMetadata = 'DataSource_SyncAllEntityAndAttributeMetadata';
Microsoft.Crm.Client.Core.Framework.DataLayerPerformanceMarkerNames.syncAllApplicationMetadata = 'DataSource_SyncAllApplicationMetadata';
Microsoft.Crm.Client.Core.Framework.DataLayerPerformanceMarkerNames.syncEntityAttributeBatch = 'DataSource_SyncEntityAttributeBatch';
Microsoft.Crm.Client.Core.Framework.DataLayerPerformanceMarkerNames.syncApplicationMetdataBatch = 'DataSource_SyncApplicationMetadataBatch';
Microsoft.Crm.Client.Core.Framework.DataLayerPerformanceMarkerNames.offlineSync = 'DataSource_OfflineSync';
Microsoft.Crm.Client.Core.Framework.OfflineDataStoreCRUDPerformanceMarkerNames.offlineCreateRecord = 'OfflineDataStore_CreateRecord';
Microsoft.Crm.Client.Core.Framework.OfflineDataStoreCRUDPerformanceMarkerNames.offlineRetrieveRecord = 'OfflineDataStore_RetrieveRecord';
Microsoft.Crm.Client.Core.Framework.OfflineDataStoreCRUDPerformanceMarkerNames.offlineUpdateRecord = 'OfflineDataStore_UpdateRecord';
Microsoft.Crm.Client.Core.Framework.OfflineDataStoreCRUDPerformanceMarkerNames.offlineDeleteRecord = 'OfflineDataStore_DeleteRecord';
Microsoft.Crm.Client.Core.Framework.OfflineDataStoreCRUDPerformanceMarkerNames.offlineRetrieveMultipleRecords = 'OfflineDataStore_RetrieveMultipleRecords';
Microsoft.Crm.Client.Core.Framework.FieldFormat.raw = Microsoft.Crm.Client.Core.Framework.FieldFormat._addDelimiter$p(0);
Microsoft.Crm.Client.Core.Framework.FieldFormat.numeric = Microsoft.Crm.Client.Core.Framework.FieldFormat._addDelimiter$p(Microsoft.Crm.Client.Core.Framework.FieldFormat._fieldFormatValue.Numeric);
Microsoft.Crm.Client.Core.Framework.FieldFormat.label = Microsoft.Crm.Client.Core.Framework.FieldFormat._addDelimiter$p(Microsoft.Crm.Client.Core.Framework.FieldFormat._fieldFormatValue.Label);
Microsoft.Crm.Client.Core.Framework.FieldFormat.value = Microsoft.Crm.Client.Core.Framework.FieldFormat._addDelimiter$p(Microsoft.Crm.Client.Core.Framework.FieldFormat._fieldFormatValue.Value);
Microsoft.Crm.Client.Core.Framework.FieldFormat.state = Microsoft.Crm.Client.Core.Framework.FieldFormat._addDelimiter$p(Microsoft.Crm.Client.Core.Framework.FieldFormat._fieldFormatValue.State);
Microsoft.Crm.Client.Core.Framework.FieldFormat.id = Microsoft.Crm.Client.Core.Framework.FieldFormat._addDelimiter$p(Microsoft.Crm.Client.Core.Framework.FieldFormat._fieldFormatValue.Id);
Microsoft.Crm.Client.Core.Framework.FieldFormat.name = Microsoft.Crm.Client.Core.Framework.FieldFormat._addDelimiter$p(Microsoft.Crm.Client.Core.Framework.FieldFormat._fieldFormatValue.Name);
Microsoft.Crm.Client.Core.Framework.FieldFormat.logicalName = Microsoft.Crm.Client.Core.Framework.FieldFormat._addDelimiter$p(Microsoft.Crm.Client.Core.Framework.FieldFormat._fieldFormatValue.LogicalName);
Microsoft.Crm.Client.Core.Framework.FieldFormat.defaultStatus = Microsoft.Crm.Client.Core.Framework.FieldFormat._addDelimiter$p(Microsoft.Crm.Client.Core.Framework.FieldFormat._fieldFormatValue.DefaultStatus);
Microsoft.Crm.Client.Core.Framework.FieldFormat.allowedStatusTransitions = Microsoft.Crm.Client.Core.Framework.FieldFormat._addDelimiter$p(Microsoft.Crm.Client.Core.Framework.FieldFormat._fieldFormatValue.AllowedStatusTransitions);
Microsoft.Crm.Client.Core.Framework.FieldFormat.color = Microsoft.Crm.Client.Core.Framework.FieldFormat._addDelimiter$p(Microsoft.Crm.Client.Core.Framework.FieldFormat._fieldFormatValue.Color);
Microsoft.Crm.Client.Core.Framework.FieldFormat.delimiter = '!';
Microsoft.Crm.Client.Core.Framework.MetadataTypeName.workspace = 'Workspace';
Microsoft.Crm.Client.Core.Framework.MetadataTypeName.interactionCentricWorkspace = 'InteractionCentricWorkspace';
Microsoft.Crm.Client.Core.Framework.CallingContext.unknown = 'unknown';
Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.emptyCommandOrEntity = -2146088111;
Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.commandNotSupported = -2146088110;
Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.operationFailedTryAgain = -2146088109;
Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.noUserPrivilege = -2146088112;
Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.xamlNotFound = -2146088113;
Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.genericError = 0;
Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.openCameraCaptureFailed = -2147094016;
Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.mobileClientLanguageNotSupported = -2147094015;
Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.mobileClientVersionNotSupported = -2147094014;
Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.roleNotEnabledForTabletApp = -2147094013;
Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.filePickerErrorFileSizeCannotBeZero = -2147094010;
Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.filePickerErrorUnableToOpenFile = -2147094009;
Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.filePickerErrorApplicationInSnapView = -2147094009;
Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.getPhotoFromGalleryFailed = -2147094008;
Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.saveDataFileErrorOutOfSpace = -2147094007;
Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.openDocumentErrorCodeUnableToFindTheDataId = -2147094005;
Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.openDocumentErrorCodeUnableToFindAnActivity = -2147094004;
Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.openDocumentErrorCodeGeneric = -2147094004;
Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.viewHasBeenDeleted = -2147094003;
Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.dashboardHasBeenDeleted = -2147094011;
Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.mobileClientNotConfiguredForCurrentUser = -2147094002;
Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.noMinimumRequiredPrivilegesForTabletApp = -2147094001;
Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.dataSourceInitializeFailedErrorCode = -2147094000;
Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.dataSourceOfflineErrorCode = -2147093999;
Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.pingFailureErrorCode = -2147093998;
Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.retrieveRecordOfflineErrorCode = -2147093997;
Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.cannotSaveRecordInOfflineErrorCode = -2147093996;
Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.generalAuthorizationErrorCode = -2147093995;
Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.cannotGoOfflineErrorCode = -2147093994;
Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.signOutFailed = -2147093993;
Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.invalidPreviewModeOperation = -2147093991;
Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.pageNotFound = -2147093990;
Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.viewNotAvailableForMobileOffline = -2147093989;
Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.notMobileWriteEnabled = -2147093988;
Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.dataStoreKeyNotFoundErrorCode = -2147093987;
Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.cantUpdateOnlineRecord = -2147093980;
Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.cannotDeleteOnlineRecord = -2147093944;
Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.entityMetadataSyncFailed = -2147093960;
Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.entityMetadataSyncFailedWithContinue = -2147093959;
Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.applicationMetadataSyncFailed = -2147093952;
Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.applicationMetadataSyncFailedWithContinue = -2147093951;
Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.applicationMetadataSyncTimeout = -2147093950;
Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.applicationMetadataSyncTimeoutWithContinue = -2147093949;
Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.applicationMetadataSyncAppLock = -2147093948;
Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.applicationMetadataSyncAppLockWithContinue = -2147093947;
Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.genericMetadataSyncFailed = -2147093946;
Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.genericMetadataSyncFailedWithContinue = -2147093945;
Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.concurrencyVersionMismatch = -2147088254;
Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.concurrencyVersionNotProvided = -2147088253;
Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.filePickerErrorFileSizeBreached = -2147205624;
Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.filePickerErrorAttachmentTypeBlocked = -2147205623;
Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.duplicateRecordDetected = -2147220685;
Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.objectDoesNotExist = -2147220969;
Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.privilegeDenied = -2147220960;
Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.userDisabled = -2147220955;
Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.duplicateRecord = -2147220937;
Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.isvAborted = -2147220891;
Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.businessUnitDisabled = -2147220692;
Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.metadataNoMapping = -2147217919;
Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.userWithoutRoles = -2147209463;
Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.userWithoutPrivileges = -2147209460;
Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.genericSqlError = -2147204784;
Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.notMobileEnabled = -2147093995;
Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.fieldIsReadOnly = -2147088625;
Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.quickCreateInvalidEntityName = -2147088112;
Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.quickCreateDisabledOnEntity = -2147088111;
Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.refreshCanceled = -2147088110;
Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.crmSqlGovernorDatabaseRequestDenied = -2147180543;
Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.trackingIsNotSupported = -2147881479;
Microsoft.Crm.Client.Core.Framework.CrmErrorCodes.emailAddressMismatch = -2147085807;
Microsoft.Crm.Client.Core.Framework.Guid._invalidGuidException$p = '\'{0}\' is not a valid Guid value.';
Microsoft.Crm.Client.Core.Framework.Guid._empty$p = null;
Microsoft.Crm.Client.Core.Framework.Guid._guidStripperPattern$p = null;
Microsoft.Crm.Client.Core.Framework.Guid._hyphenGuidVerifierPattern$p = null;
Microsoft.Crm.Client.Core.Framework.Guid._braceAndHyphenGuidVerifierPattern$p = null;
Microsoft.Crm.Client.Core.Framework.Guid._contiguousGuidVerifierPattern$p = null;
Microsoft.Crm.Client.Core.Framework.ManifestType.twoOptions = 'TwoOptions';
Microsoft.Crm.Client.Core.Framework.ManifestType.dateAndTimeDateOnly = 'DateAndTime.DateOnly';
Microsoft.Crm.Client.Core.Framework.ManifestType.dateAndTimeDateAndTime = 'DateAndTime.DateAndTime';
Microsoft.Crm.Client.Core.Framework.ManifestType.decimal = 'Decimal';
Microsoft.Crm.Client.Core.Framework.ManifestType.FP = 'FP';
Microsoft.Crm.Client.Core.Framework.ManifestType.wholeNone = 'Whole.None';
Microsoft.Crm.Client.Core.Framework.ManifestType.wholeDuration = 'Whole.Duration';
Microsoft.Crm.Client.Core.Framework.ManifestType.wholeTimeZone = 'Whole.TimeZone';
Microsoft.Crm.Client.Core.Framework.ManifestType.wholeLanguage = 'Whole.Language';
Microsoft.Crm.Client.Core.Framework.ManifestType.lookupSimple = 'Lookup.Simple';
Microsoft.Crm.Client.Core.Framework.ManifestType.lookupCustomer = 'Lookup.Customer';
Microsoft.Crm.Client.Core.Framework.ManifestType.lookupOwner = 'Lookup.Owner';
Microsoft.Crm.Client.Core.Framework.ManifestType.lookupPartyList = 'Lookup.PartyList';
Microsoft.Crm.Client.Core.Framework.ManifestType.lookupRegarding = 'Lookup.Regarding';
Microsoft.Crm.Client.Core.Framework.ManifestType.multiple = 'Multiple';
Microsoft.Crm.Client.Core.Framework.ManifestType.currency = 'Currency';
Microsoft.Crm.Client.Core.Framework.ManifestType.optionSet = 'OptionSet';
Microsoft.Crm.Client.Core.Framework.ManifestType.enumType = 'Enum';
Microsoft.Crm.Client.Core.Framework.ManifestType.singleLineEmail = 'SingleLine.Email';
Microsoft.Crm.Client.Core.Framework.ManifestType.singleLineText = 'SingleLine.Text';
Microsoft.Crm.Client.Core.Framework.ManifestType.singleLineTextArea = 'SingleLine.TextArea';
Microsoft.Crm.Client.Core.Framework.ManifestType.singleLineURL = 'SingleLine.URL';
Microsoft.Crm.Client.Core.Framework.ManifestType.singleLineTickerSymbol = 'SingleLine.Ticker';
Microsoft.Crm.Client.Core.Framework.ManifestType.singleLinePhone = 'SingleLine.Phone';
Microsoft.Crm.Client.Core.Framework.ManifestType.grid = 'Grid';
Microsoft.Crm.Client.Core.Framework.Undefined.undefinedKeyword = 'undefined';
Microsoft.Crm.Client.Core.Framework.Undefined.booleanValue = undefined;
Microsoft.Crm.Client.Core.Framework.Undefined.int32Value = undefined;
Microsoft.Crm.Client.Core.Framework.Undefined.doubleValue = undefined;
Microsoft.Crm.Client.Core.Framework.Undefined.stringValue = undefined;
Microsoft.Crm.Client.Core.Framework.Undefined.objectValue = undefined;
_Math.maxSignedInt32 = 2147483647;
_Math.minSignedInt32 = -2147483648;
Microsoft.Crm.Client.Core.Framework._String.empty = '';
Microsoft.Crm.Client.Core.Framework._String._newLineExpression$p = new RegExp('[\n\r]+');
HtmlEncoder._ampersand$p = new RegExp('&', 'g');
HtmlEncoder._lessThan$p = new RegExp('<', 'g');
HtmlEncoder._greaterThan$p = new RegExp('>', 'g');
HtmlEncoder._apostrophe$p = new RegExp('\'', 'g');
HtmlEncoder._quotation$p = new RegExp('\"', 'g');
HtmlEncoder._encodedAmpersand$p = new RegExp('&amp;', 'g');
HtmlEncoder._encodedLessThan$p = new RegExp('&lt;', 'g');
HtmlEncoder._encodedGreaterThan$p = new RegExp('&gt;', 'g');
HtmlEncoder._encodedApostrophe$p = new RegExp('&apos;', 'g');
HtmlEncoder._encodedQuotation$p = new RegExp('&quot;', 'g');
Microsoft.Crm.Client.Core.Framework.Debug.$$cctor();
Microsoft.Crm.Client.Core.Framework.Utils.XmlParser._textNodeName$p = '#text';
Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlNodeFactory._preferredXmlEvaluator$p = Microsoft.Crm.Client.Core.Storage.Common.Xml.XmlEvaluatorType.undefined;
//@ sourceMappingURL=.srcmap
