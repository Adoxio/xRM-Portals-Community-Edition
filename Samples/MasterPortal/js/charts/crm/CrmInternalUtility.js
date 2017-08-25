/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

Type.registerNamespace('Mscrm');

Mscrm.IBrowserStorage = function() {}
Mscrm.IBrowserStorage.registerInterface('Mscrm.IBrowserStorage');


Mscrm.CrmCrossBrowser = function() {
}
Mscrm.CrmCrossBrowser.get_hash = function() {
    return Mscrm.CrmCrossBrowser.getHash(window.self.location);
}
Mscrm.CrmCrossBrowser.getHash = function(location) {
    if (!location) {
        throw Error.argumentNull('location');
    }
    var $v_0 = location.href.split('#');
    return ($v_0.length > 1) ? $v_0[1] : '';
}
Mscrm.CrmCrossBrowser.getLocalStorage = function($sn_window) {
    if (!$sn_window) {
        throw Error.argumentNull('window');
    }
    return $sn_window.localStorage;
}
Mscrm.CrmCrossBrowser.getSessionStorage = function($sn_window) {
    if (!$sn_window) {
        throw Error.argumentNull('window');
    }
    return $sn_window.sessionStorage;
}


Mscrm.CrmDisposeHelper = function() {
    this.$3_0 = [];
}
Mscrm.CrmDisposeHelper.prototype = {
    
    schedule: function(disposeAction) {
        if (!this.$3_0) {
            throw Error.invalidOperation('Object is already disposed');
        }
        if (!disposeAction) {
            throw Error.argumentNull('disposeAction');
        }
        Array.add(this.$3_0, disposeAction);
    },
    
    dispose: function() {
        if (!this.$3_0) {
            return;
        }
        for (var $v_0 = this.$3_0.length - 1; $v_0 >= 0; $v_0--) {
            var $v_1 = this.$3_0[$v_0];
            $v_1();
        }
        this.$3_0 = null;
    }
}


Type.registerNamespace('Mscrm.Imported');

Type.registerNamespace('jQueryApi');

jQueryApi.IActiveDeferred$2 = function() {}
jQueryApi.IActiveDeferred$2.$$ = function(TData, TError) {
    var $$cn = 'IActiveDeferred$2' + '$' + TData.getName().replace(/\./g, '_') + '$' + TError.getName().replace(/\./g, '_');
    if (!jQueryApi[$$cn]) {
        var $$ccr = jQueryApi[$$cn] = function() {
        };
        $$ccr.registerInterface('jQueryApi.' + $$cn);
    }
    return jQueryApi[$$cn];
}
jQueryApi.IActiveDeferred$2.registerInterface('jQueryApi.IActiveDeferred$2');


jQueryApi.jQueryDeferredFactory = function() {
}
jQueryApi.jQueryDeferredFactory.Deferred = function(TData, TError, initializer) {
    if (initializer === undefined || !initializer) {
        return $P_CRM.Deferred();
    }
    else {
        return $P_CRM.Deferred(initializer);
    }
}
jQueryApi.jQueryDeferredFactory.fromResult = function(TData, TError, value) {
    var $v_0 = jQueryApi.jQueryDeferredFactory.Deferred(TData, TError);
    $v_0.resolve(value);
    return $v_0.promise();
}


jQueryApi.JSTreeOptions = function() {
    this.$2_0 = [];
}
jQueryApi.JSTreeOptions.prototype = {
    $2_0: null,
    
    get_plugins: function() {
        return this.$2_0;
    },
    
    set_plugins: function(value) {
        this.$2_0 = value;
        return value;
    },
    
    getJSTreeConfig: function() {
        var $v_0 = '{ \"plugins\" : [ ';
        for (var $v_2 = 0; $v_2 < this.$2_0.length; $v_2++) {
            var $v_3 = this.$2_0[$v_2];
            if ($v_2 !== this.$2_0.length - 1) {
                $v_0 += $v_3.$1_0 + ',';
            }
            else {
                $v_0 += $v_3.$1_0 + '],';
            }
        }
        for (var $v_4 = 0; $v_4 < this.$2_0.length; $v_4++) {
            var $v_5 = this.$2_0[$v_4];
            $v_0 += $v_5.$1_0 + ' : { ';
            var $$dict_6 = $v_5.$0_0;
            for (var $$key_7 in $$dict_6) {
                var $v_6 = { key: $$key_7, value: $$dict_6[$$key_7] };
                $v_0 += $v_6.key + ' : ' + $v_6.value + ',';
            }
            $v_0 = $v_0.substring(0, $v_0.length - 1);
            $v_0 += '},';
        }
        $v_0 = $v_0.substring(0, $v_0.length - 1);
        $v_0 += '}';
        var $v_1 = $P_CRM.parseJSON($v_0);
        return $v_1;
    }
}


jQueryApi.JSTreePlugin = function() {
    this.$0_0 = {};
}
jQueryApi.JSTreePlugin.prototype = {
    $1_0: null,
    
    get_name: function() {
        return this.$1_0;
    },
    
    set_name: function(value) {
        this.$1_0 = value;
        return value;
    },
    
    get_configuration: function() {
        return this.$0_0;
    },
    
    set_configuration: function(value) {
        this.$0_0 = value;
        return value;
    }
}


jQueryApi.JSTreeCorePlugin = function() {
    jQueryApi.JSTreeCorePlugin.initializeBase(this);
    this.$1_0 = '\"core\"';
}
jQueryApi.JSTreeCorePlugin.prototype = {
    
    get_animation: function() {
        return this.$0_0['\"animation\"'].toString();
    },
    
    set_animation: function(value) {
        this.$0_0['\"animation\"'] = value;
        return value;
    },
    
    get_rightToLeft: function() {
        return this.$0_0['\"rtl\"'].toString();
    },
    
    set_rightToLeft: function(value) {
        this.$0_0['\"rtl\"'] = value;
        return value;
    },
    
    get_loadOpen: function() {
        return this.$0_0['\"load_open\"'].toString();
    },
    
    set_loadOpen: function(value) {
        this.$0_0['\"load_open\"'] = value;
        return value;
    },
    
    get_loadingString: function() {
        return this.$0_0['\"strings\"'].toString();
    },
    
    set_loadingString: function(value) {
        this.$0_0['\"strings\"'] = '{ \"loading\" :\"' + this.$4_1(value) + '\"}';
        return value;
    },
    
    $4_1: function($p0) {
        var $v_0 = new RegExp('\\\\', 'g');
        var $v_1 = new RegExp('\"', 'g');
        return ($p0.replace($v_0, '\\\\')).replace($v_1, '\\\"');
    }
}


jQueryApi.JSTreeUIPlugin = function() {
    jQueryApi.JSTreeUIPlugin.initializeBase(this);
    this.$1_0 = '\"ui\"';
}
jQueryApi.JSTreeUIPlugin.prototype = {
    
    get_selectLimit: function() {
        return this.$0_0['\"select_limit\"'].toString();
    },
    
    set_selectLimit: function(value) {
        this.$0_0['\"select_limit\"'] = value;
        return value;
    },
    
    get_selectMultipleModifier: function() {
        return this.$0_0['\"select_multiple_modifier\"'].toString();
    },
    
    set_selectMultipleModifier: function(value) {
        this.$0_0['\"select_multiple_modifier\"'] = '\"' + value + '\"';
        return value;
    },
    
    get_selectRangeModifier: function() {
        return this.$0_0['\"select_range_modifier\"'].toString();
    },
    
    set_selectRangeModifier: function(value) {
        this.$0_0['\"select_range_modifier\"'] = '\"' + value + '\"';
        return value;
    },
    
    get_selectedParentClose: function() {
        return this.$0_0['\"selected_parent_close\"'].toString();
    },
    
    set_selectedParentClose: function(value) {
        if (value === 'false') {
            this.$0_0['\"selected_parent_close\"'] = value;
        }
        else {
            this.$0_0['\"selected_parent_close\"'] = '\"' + value + '\"';
        }
        return value;
    },
    
    get_selectedParentOpen: function() {
        return this.$0_0['\"selected_parent_open\"'].toString();
    },
    
    set_selectedParentOpen: function(value) {
        this.$0_0['\"selected_parent_open\"'] = value;
        return value;
    },
    
    get_selectPreviousOnDelete: function() {
        return this.$0_0['\"select_prev_on_delete\"'].toString();
    },
    
    set_selectPreviousOnDelete: function(value) {
        this.$0_0['\"select_prev_on_delete\"'] = value;
        return value;
    },
    
    get_disableSelectingChildren: function() {
        return this.$0_0['\"disable_selecting_children\"'].toString();
    },
    
    set_disableSelectingChildren: function(value) {
        this.$0_0['\"disable_selecting_children\"'] = value;
        return value;
    },
    
    get_initiallySelectedList: function() {
        return this.$0_0['\"initially_select\"'].toString();
    },
    
    set_initiallySelectedList: function(value) {
        this.$0_0['\"initially_select\"'] = '[' + value + ']';
        return value;
    }
}


jQueryApi.JSTreeJsonDataPlugin = function() {
    jQueryApi.JSTreeJsonDataPlugin.initializeBase(this);
    this.$1_0 = '\"json_data\"';
    this.$0_0['\"data\"'] = 'true';
}
jQueryApi.JSTreeJsonDataPlugin.prototype = {
    
    get_progressiveRender: function() {
        return this.$0_0['\"progressive_render\"'].toString();
    },
    
    set_progressiveRender: function(value) {
        this.$0_0['\"progressive_render\"'] = value;
        return value;
    },
    
    get_progressiveUnload: function() {
        return this.$0_0['\"progressive_unload\"'].toString();
    },
    
    set_progressiveUnload: function(value) {
        this.$0_0['\"progressive_unload\"'] = value;
        return value;
    }
}


jQueryApi.JSTreeThemesPlugin = function() {
    jQueryApi.JSTreeThemesPlugin.initializeBase(this);
    this.$1_0 = '\"themes\"';
}
jQueryApi.JSTreeThemesPlugin.prototype = {
    
    get_theme: function() {
        return this.$0_0['\"theme\"'].toString();
    },
    
    set_theme: function(value) {
        this.$0_0['\"theme\"'] = '\"' + value + '\"';
        return value;
    },
    
    get_icons: function() {
        return this.$0_0['\"icons\"'].toString();
    },
    
    set_icons: function(value) {
        this.$0_0['\"icons\"'] = value;
        return value;
    },
    
    get_dots: function() {
        return this.$0_0['\"dots\"'].toString();
    },
    
    set_dots: function(value) {
        this.$0_0['\"dots\"'] = value;
        return value;
    },
    
    get_url: function() {
        return this.$0_0['\"url\"'].toString();
    },
    
    set_url: function(value) {
        this.$0_0['\"url\"'] = '\"' + value + '\"';
        return value;
    }
}


jQueryApi.JSTreeHotKeysPlugin = function() {
    jQueryApi.JSTreeHotKeysPlugin.initializeBase(this);
    this.$1_0 = '\"hotkeys\"';
    this.$0_0['\"return\"'] = 'true';
}


Type.registerNamespace('jQueryUIApi');

jQueryUIApi.WidgetOptions = function() {
}
jQueryUIApi.WidgetOptions.prototype = {
    disabled: false,
    hide: false,
    show: false,
    create: null,
    isRTL: false
}


jQueryUIApi.WidgetFactory = function() {
}
jQueryUIApi.WidgetFactory.register = function(TWidget) {
    var $v_0 = TWidget.getName().split('.');
    var $v_1 = $v_0[0], $v_2 = $v_0[1];
    var $v_3 = $v_1 + '-' + $v_2;
    var $v_4 = TWidget.prototype;
    $v_4.widgetName = $v_2;
    $v_4.widgetEventPrefix = $v_2;
    $v_4.widgetBaseClass = $v_3;
    $P_CRM.widget.bridge($v_2, TWidget);
}
jQueryUIApi.WidgetFactory.createInstance = function(TWidget, element, options) {
    var $v_0 = TWidget.getName().split('.');
    var $v_1 = $v_0[1];
    var $v_2 = $P_CRM(element)[$v_1](options);
    return $v_2.data($v_1);
}


Type.registerNamespace('Navigatorgeolocation');

Mscrm.CrmCrossBrowser.registerClass('Mscrm.CrmCrossBrowser');
Mscrm.CrmDisposeHelper.registerClass('Mscrm.CrmDisposeHelper', null, Sys.IDisposable);
jQueryApi.jQueryDeferredFactory.registerClass('jQueryApi.jQueryDeferredFactory');
jQueryApi.JSTreeOptions.registerClass('jQueryApi.JSTreeOptions');
jQueryApi.JSTreePlugin.registerClass('jQueryApi.JSTreePlugin');
jQueryApi.JSTreeCorePlugin.registerClass('jQueryApi.JSTreeCorePlugin', jQueryApi.JSTreePlugin);
jQueryApi.JSTreeUIPlugin.registerClass('jQueryApi.JSTreeUIPlugin', jQueryApi.JSTreePlugin);
jQueryApi.JSTreeJsonDataPlugin.registerClass('jQueryApi.JSTreeJsonDataPlugin', jQueryApi.JSTreePlugin);
jQueryApi.JSTreeThemesPlugin.registerClass('jQueryApi.JSTreeThemesPlugin', jQueryApi.JSTreePlugin);
jQueryApi.JSTreeHotKeysPlugin.registerClass('jQueryApi.JSTreeHotKeysPlugin', jQueryApi.JSTreePlugin);
jQueryUIApi.WidgetOptions.registerClass('jQueryUIApi.WidgetOptions');
jQueryUIApi.WidgetFactory.registerClass('jQueryUIApi.WidgetFactory');
//@ sourceMappingURL=.srcmap
