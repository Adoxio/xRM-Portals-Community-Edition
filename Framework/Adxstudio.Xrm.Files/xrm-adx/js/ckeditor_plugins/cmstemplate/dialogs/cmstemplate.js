(function(CKEDITOR) {
	var pluginName = "cmstemplate";

	CKEDITOR.dialog.add(pluginName, function (editor) {
		var templates = null, setting = false;

		function getTemplateList() {
			var deferred = $.Deferred();
			var promise = deferred.promise();
			var templateList = templates || editor.config.cmstemplates;

			if (typeof templateList === "string" && templates === null) {
				$.get(templateList, function (data) {
					templates = data;
					deferred.resolve(templates || []);
				});
			} else {
				deferred.resolve(templates || []);
			}

			return promise;
		}

		function debounce(func, wait, immediate) {
			var timeout;
			return function () {
				var context = this, args = arguments;
				var later = function () {
					timeout = null;
					if (!immediate) func.apply(context, args);
				};
				var callNow = immediate && !timeout;
				clearTimeout(timeout);
				timeout = setTimeout(later, wait);
				if (callNow) func.apply(context, args);
			};
		};

		function usingSourceApi(callback, attempts) {
			var dialog = CKEDITOR.dialog.getCurrent(),
				iframeWindow = $(dialog.getContentElement("tab", "source").getElement().$).find("iframe")[0].contentWindow,
				api = iframeWindow.cmstemplate;

			if (!api && ((attempts || 0) < 100)) {
				setTimeout(function () {
					usingSourceApi(callback, (attempts || 0) + 1);
				}, 0);

				return;
			}

			callback(api);
		}

		function setSource(source) {
			usingSourceApi(function (api) {
				setting = true;
				api.setSource(source);
			});
		}

		function insertPreviewHtml(html) {
			var dialog = CKEDITOR.dialog.getCurrent();

			if (html.indexOf("<html>") === -1) {
				var contentCssLinks = "";

				$.each(editor.config.contentsCss, function (i, href) {
					contentCssLinks += "<link type=\"text/css\" rel=\"stylesheet\" href=\"" + href + "\">";
				});

				html = (
				  "<!DOCTYPE html>" +
				  "<html>" +
					"<head>" +
					  contentCssLinks +
					"</head>" +
					"<body>" +
					  html +
					"</body>" +
				  "</html>"
				);
			}

			var doc = $(dialog.getContentElement("tab", "preview").getElement().$).find("iframe")[0].contentWindow.document;
			doc.open();
			doc.write(html);
			doc.close();
		}

		function onSelected() {
			var dialog = CKEDITOR.dialog.getCurrent(),
				templateData = dialog.getContentElement("tab", "template").getValue(),
				includeData = dialog.getContentElement("tab", "include").getValue(),
				templateUrlCache = {},
				templatePreviewCache = {},
				template,
				templateHtml,
				cachedSource,
				cachedPreview;

			if (!templateData.id) {
				return;
			}

			template = templateData.value;

			if (includeData.id === window.ResourceManager['CKEditor_CmsTemplate_Include']) {
				templateHtml = template.include || "{% include '" + template.name + "' %}";
				setSource(templateHtml);
			} else {
				if (template.url) {
					cachedSource = templateUrlCache[template.url];
					if (cachedSource) {
						setSource(cachedSource);
					} else {
						$.get(template.url, function(source) {
							templateHtml = templateUrlCache[template.url] = source;
							setSource(templateHtml);
						});
					}
				} else {
					templateHtml = template.content;
					setSource(templateHtml);
				}
			}

			if (template.preview_url) {
				cachedPreview = templatePreviewCache[template.preview_url];
				if (cachedPreview) {
					insertPreviewHtml(cachedPreview);
				} else {
					$.get(template.preview_url, function(html) {
						templatePreviewCache[template.preview_url] = html;
						insertPreviewHtml(html);
					});
				}
			}
		}

		var viewportSize = CKEDITOR.document.getWindow().getViewPaneSize();

		var plugin = editor.plugins.cmstemplate;

		return {
			title: window.ResourceManager['CKEditor_CmsTemplate_Toolbar'],
			width: viewportSize.width - 100,
			resizable: CKEDITOR.DIALOG_RESIZE_NONE,
			contents: [
				{
					id: "tab",
					label: "Basic Settings",
					elements: [
						{
							type: "select2",
							id: "template",
							width: "100%",
							label: window.ResourceManager['CKEditor_CmsTemplate_Templatelabel'],
							select2: {
								dropdownCssClass: "select2-ckeditor xrm-editable-select2-with-descriptions",
								formatResult: function(item) {
									var $result = $("<div>").append($("<div>").addClass("name").text(item.text));

									if (item.value && item.value.description) {
										$result.append($("<small>").addClass("description").text(item.value.description));
									}

									return $result;
								},
								query: function(options) {
									getTemplateList().then(function(templateList) {
										var templateOptions = $.map(templateList, function(template, i) {
											return {
												selected: !i,
												id: template.name,
												text: template.title,
												value: template
											}
										});
										options.callback({ results: templateOptions });
									});
								}
							},
							onChange: onSelected,
							onLoad: function (e) {
								e.sender.addFocusable(new CKEDITOR.dom.element($(this.getElement().$).find(".select2-focusser")[0]));
							}
						},
						{
							type: "select2",
							id: "include",
							width: "100%",
							label: window.ResourceManager['CKEditor_CmsTemplate_Insertlabel'],
							items: [[window.ResourceManager['CKEditor_CmsTemplate_Include']], [window.ResourceManager['CKEditor_CmsTemplate_Fullsource']]],
							"default": window.ResourceManager['CKEditor_CmsTemplate_Include'],
							onChange: onSelected,
							onLoad: function (e) {
								e.sender.addFocusable(new CKEDITOR.dom.element($(this.getElement().$).find(".select2-focusser")[0]));
							}
						},
						{
							type: "html",
							id: "source",
							width: "100%",
							html: "<div class='col-xs-12'><iframe src='" + plugin.path + "dialogs/source.html' class='panel panel-default' style='width:100%;height:150px;'></iframe></div>",
							onHide: function() {
								$(this.getElement().$).find("iframe")[0].contentWindow.location.reload();
							}
						},
						{
							type: "html",
							id: "preview",
							width: "100%",
							html: "<div><label class='col-xs-12'>" + window.ResourceManager['CKEditor_CmsTemplate_Previewlabel'] + "</label><div class='col-xs-12'><iframe src='about:blank' class='panel panel-default' style='width:100%;height:250px;'></iframe></div></div>",
							onHide: function () {
								$(this.getElement().$).find("iframe")[0].contentWindow.location.reload();
							}
						}
					]
				}
			],
			onShow: function () {
				var $el = $(this.getElement().$);
				$el.removeClass("cke_reset_all");
				$el.find(".cke_dialog_contents").addClass("container-fluid").css("margin-bottom", 0);
				$el.find(".cke_dialog_ui_vbox_child").addClass("row");
				$el.find("label").addClass("col-xs-3 col-md-2 col-lg-1");
				$el.find(".cke_dialog_ui_labeled_content").addClass("col-xs-9 col-md-10 col-lg-11");

				usingSourceApi(function (api) {
					api.on("change", debounce(function () {
						if (!setting) {
							var source = api.getSource();

							var dialog = CKEDITOR.dialog.getCurrent(),
								templateData = dialog.getContentElement("tab", "template").getValue(),
								template;

							if (!templateData.id) {
								return;
							}

							template = templateData.value;

							if (template.live_preview_url && source) {
							    shell.ajaxSafePost({
									type: "POST",
									url: template.live_preview_url,
									data: { source: source }
								}).then(function(html) {
									insertPreviewHtml(html);
								});
							}
						}
						setting = false;
					}, 1000));
				});

				this.layout();
			},
			onOk: function() {
				usingSourceApi(function (api) {
					var source = api.getSource();
					if (source) {
						editor.insertHtml(source);
					}
				});
			}
		};
	});
}(window.CKEDITOR));