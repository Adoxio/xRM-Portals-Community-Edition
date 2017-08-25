(function () {
  var pluginName = "ace";

  CKEDITOR.plugins.add(pluginName, {
    requires: 'iframedialog',
    init: function (editor) {
      if (editor.elementMode != CKEDITOR.ELEMENT_MODE_REPLACE) {
        return;
      }

      var viewportSize = CKEDITOR.document.getWindow().getViewPaneSize();

      editor.addCommand(pluginName, new CKEDITOR.dialogCommand(pluginName));

      editor.ui.addButton && editor.ui.addButton('Ace', {
        label: editor.lang.sourcearea.toolbar,
        command: pluginName,
        toolbar: 'mode,10',
        icon: 'source'
      });

      CKEDITOR.dialog.addIframe(
        pluginName,
        editor.lang.sourcearea.toolbar,
        this.path + 'dialog.html',
        viewportSize.width - 100,
        viewportSize.height - 180);
    }
  });
})();
