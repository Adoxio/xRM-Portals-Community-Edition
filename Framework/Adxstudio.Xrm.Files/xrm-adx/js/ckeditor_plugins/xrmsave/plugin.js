(function () {
  var pluginName = "xrmsave";

  var saveCmd = {
    readOnly: 1,
    exec: function (editor) {
      editor.fire('save');
    }
  };

  CKEDITOR.plugins.add(pluginName, {
    init: function (editor) {
      if (editor.elementMode != CKEDITOR.ELEMENT_MODE_REPLACE) {
        return;
      }

      var command = editor.addCommand(pluginName, saveCmd);
      command.modes = { wysiwyg: true };

      editor.ui.addButton && editor.ui.addButton('XrmSave', {
        label: editor.lang.save.toolbar,
        command: pluginName,
        toolbar: 'document,10',
        icon: 'save'
      });
    }
  });
})();
