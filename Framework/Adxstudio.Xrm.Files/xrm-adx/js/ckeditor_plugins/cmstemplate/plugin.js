(function (CKEDITOR) {
	var pluginName = "cmstemplate";

	CKEDITOR.plugins.add(pluginName, {
		onLoad: function () {
			CKEDITOR.dialog.add(pluginName, this.path + "dialogs/" + pluginName + ".js");
		},
		init: function(editor) {
			if (editor.elementMode !== CKEDITOR.ELEMENT_MODE_REPLACE) {
				return;
			}

			editor.addCommand(pluginName, new CKEDITOR.dialogCommand(pluginName));

			editor.ui.addButton && editor.ui.addButton("CmsTemplate", {
				label: window.ResourceManager['CKEditor_CmsTemplate_Toolbar'],
				command: pluginName,
				toolbar: "insert,100",
				icon: "codesnippet"
			});
		}
	});
}(window.CKEDITOR));
