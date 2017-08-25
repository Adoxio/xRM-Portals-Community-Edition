/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/


XRM.onActivate(function() {

	var ns = XRM.namespace('editable.Attribute.handlers');
	var $ = XRM.jQuery;
	var yuiSkinClass = XRM.yuiSkinClass;
	
	function renderRichUI(attributeContainer, attributeDisplayName, attributeName, attributeValue, entityServiceUri, editCompleteCallback) {
		var yuiContainer = $('<div />').addClass(yuiSkinClass).appendTo(document.body);
		var panelContainer = $('<div />').addClass('xrm-editable-panel xrm-editable-dialog xrm-editable-dialog-html').appendTo(yuiContainer);

		var editPanel = new YAHOO.widget.Panel(panelContainer.get(0), {
			close: false,
			draggable: true,
			constraintoviewport: true,
			visible: false,
			zindex: XRM.zindex,
			xy: YAHOO.util.Dom.getXY(attributeContainer.get(0))
		});

		editPanel.setHeader(XRM.localize('editable.label.prefix') + attributeDisplayName);
		editPanel.setBody(' ');
		editPanel.render();
		editPanel.show();

		XRM.ui.registerOverlay(editPanel);
		editPanel.focus();

		var id = tinymce.DOM.uniqueId();

		// Create the textarea that will host our content, and that tinymce will latch on to.
		$('<textarea />').attr('id', id).text(attributeValue || '').appendTo(editPanel.body);

		// Create our tinymce editor, and wire up the custom save/cancel handling.
		var editor = new tinymce.Editor(id, XRM.tinymceSettings);

		editor.onClick.add(function() { editPanel.focus(); });

		editor.addButton('save', { title: 'save.save_desc', cmd: 'xrmSave' });
		editor.addButton('cancel', { title: 'save.cancel_desc', cmd: 'xrmCancel' });

		editor.addShortcut('ctrl+s', editor.getLang('save.save_desc'), 'xrmSave');

		function removeEditor() {
			editPanel.hide();
			editor.remove();
			yuiContainer.remove();
		}

		editor.addCommand('xrmSave', function() {
			// Don't bother with the ajax call if nothing was changed.
			if (!editor.isDirty()) {
				removeEditor();
				return;
			}

			editor.setProgressState(1);
			var content = editor.getContent();

			XRM.data.services.putAttribute(entityServiceUri, attributeName, content, {
				success: function() {
					editor.setProgressState(0);
					// Set the original in-DOM content to the new content exported from tinymce.
					$('.xrm-attribute-value', attributeContainer).html(content);
					removeEditor();

					if ($.isFunction(editCompleteCallback)) {
						editCompleteCallback();
					}
				},
				error: function(xhr) {
					editor.setProgressState(0);
					XRM.ui.showDataServiceError(xhr);
				}
			});
		});

		editor.addCommand('xrmCancel', function() {
			if (editor.isDirty()) {
				if (!confirm(XRM.localize('confirm.unsavedchanges'))) {
					return;
				}
			}

			removeEditor();

			if ($.isFunction(editCompleteCallback)) {
				editCompleteCallback();
			}
		});

		editor.render();
	}
	
	function renderFallbackUI(attributeContainer, attributeDisplayName, attributeName, attributeValue, entityServiceUri, editCompleteCallback) {
		// Build the DOM necessary to support our UI.
		var yuiContainer = $('<div />').addClass(yuiSkinClass).appendTo(document.body);
		var dialogContainer = $('<div />').addClass('xrm-editable-panel xrm-editable-dialog xrm-editable-dialog-html-fallback').appendTo(yuiContainer);

		function completeEdit(dialog) {
			dialog.cancel();
			yuiContainer.remove();

			if ($.isFunction(editCompleteCallback)) {
				editCompleteCallback();
			}
		}

		function handleCancel(dialog) {
			completeEdit(dialog);
		}

		function handleSave(dialog) {
			var dialogInput = $('.xrm-text', dialog.body);
			var dialogInputValue = dialogInput.val();
			var dialogFooter = $(dialog.footer);

			// If the attribute value has been changed, persist the new value.
			if (dialogInputValue != attributeValue) {
				dialogFooter.hide();
				dialogInput.hide();
				dialogContainer.addClass('xrm-editable-wait');
				XRM.data.services.putAttribute(entityServiceUri, attributeName, dialogInputValue, {
					success: function() {
						$('.xrm-attribute-value', attributeContainer).html(dialogInputValue);
						completeEdit(dialog);
					},
					error: function(xhr) {
						dialogContainer.removeClass('xrm-editable-wait');
						dialogFooter.show();
						dialogInput.show();
						XRM.ui.showDataServiceError(xhr);
					}
				});
			}
			// Otherwise, just dismiss the edit dialog without doing anything.
			else {
				completeEdit(dialog);
			}
		}

		// Create our modal editing dialog.
		var dialog = new YAHOO.widget.Dialog(dialogContainer.get(0), {
			visible: false,
			constraintoviewport: true,
			zindex: XRM.zindex,
			xy: YAHOO.util.Dom.getXY(attributeContainer.get(0)),
			buttons: [
				{ text: XRM.localize('editable.save.label'), handler: function() { handleSave(this) }, isDefault: true },
				{ text: XRM.localize('editable.cancel.label'), handler: function() { handleCancel(this) } }]
		});

		dialog.setHeader('Edit ' + (attributeDisplayName || ''));
		dialog.setBody(' ');
		
		$('<textarea />').addClass('xrm-text').val(attributeValue || '').appendTo(dialog.body);

		// Add ctrl+s shortcut for saving content.
		$('.xrm-text', dialog.body).keypress(function(e) {
			if (!(e.which == ('s').charCodeAt(0) && e.ctrlKey)) {
				return true;
			}
			handleSave(dialog);
			return false;
		});

		dialog.render();
		dialog.show();

		XRM.ui.registerOverlay(dialog);
		dialog.focus();

		$('.xrm-text', dialog.body).focus();
	}
	
	ns.html = (typeof(tinymce) === 'undefined') ? renderFallbackUI : renderRichUI;
	
});
