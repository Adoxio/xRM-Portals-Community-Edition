(function (CKEDITOR) {
	var pluginName = "select2";

	CKEDITOR.plugins.add(pluginName, {
		onLoad: function () {
			var initPrivateObject = function (elementDefinition) {
				this._ || (this._ = {});
				this._["default"] = this._.initValue = elementDefinition["default"] || "";
				this._.required = elementDefinition.required || false;
				var args = [this._];
				for (var i = 1; i < arguments.length; i++)
					args.push(arguments[i]);
				args.push(true);
				CKEDITOR.tools.extend.apply(CKEDITOR.tools, args);
				return this._;
			}

			CKEDITOR.tools.extend(CKEDITOR.ui.dialog, {
				select2: function (dialog, elementDefinition, htmlList) {
					var self = this;

					initPrivateObject.call(this, elementDefinition);

					var domId = this._.inputId = CKEDITOR.tools.getNextId() + "_select2",
						attributes = { 'class': "cke_dialog_ui_input_select2", id: domId, type: "hidden" };

					var myDefinition = CKEDITOR.tools.extend({}, {
						items: []
					}, elementDefinition, true);

					var innerHTML = function () {
						var html = ["<div class='cke_dialog_ui_input_", elementDefinition.type, "' role='presentation'"];

						if (elementDefinition.width) {
							html.push("style='width:" + elementDefinition.width + "' ");
						}

						html.push("><input ");

						attributes["aria-labelledby"] = this._.labelId;

						if (this._.required) {
							attributes["aria-required"] = this._.required;
						};

						for (var i in attributes) {
							if (attributes.hasOwnProperty(i)) {
								html.push(i + "='" + attributes[i] + "' ");
							}
						}

						html.push(" /></div>");
						return html.join("");
					};
					
					dialog.on("load", function () {
						var element = this.getElement();
						var $element = $(element.$).find("input[type=hidden]");

						var select2 = CKEDITOR.tools.extend({}, {
							width: "100%",
							initSelection : function($item, callback) {
								var data = { id: $item.val(), text: $item.val() };
								callback(data);
							},
							query: function (options) {
								var queryOptions = $.map(myDefinition.items, function (item, i) {
									return {
										selected: !i,
										id: item[0],
										text: item.length === 1 ? item[0] : item[1]
									};
								});
								options.callback({
									results: queryOptions
								});
							},
							minimumResultsForSearch: 6
						}, elementDefinition.select2, true);

						$element.select2(select2).on("change", function(e) {
							self.fire("change", { value: e.val });
						});
					}, this);

					CKEDITOR.ui.dialog.labeledElement.call(this, dialog, myDefinition, htmlList, innerHTML);
				}
			});

			CKEDITOR.ui.dialog.select2.prototype = CKEDITOR.tools.extend(new CKEDITOR.ui.dialog.labeledElement(), {
				getInputElement: function () {
					return CKEDITOR.document.getById(this._.inputId);
				},
				setValue: function (value, noChangeEvent) {
					var element = this.getElement();
					var $element = $(element.$).find("input[type=hidden]");
					$element.select2("val", value);
					$element.val(value);
					!noChangeEvent && this.fire("change", { value: value });
					return this;
				},
				getValue: function () {
					var element = this.getElement();
					var $element = $(element.$).find("input[type=hidden]");
					return $element.select2("data") || "";
				},
				keyboardFocusable: true,
				isChanged: function () {
					return (this.getValue() && this.getValue().id) !== this.getInitValue();
				},
				reset: function (noChangeEvent) {
					this.setValue(this.getInitValue(), noChangeEvent);
				},
				setInitValue: function () {
					this._.initValue = (this.getValue() && this.getValue().id) || "";
				},
				resetInitValue: function () {
					this._.initValue = this._["default"];
				},
				getInitValue: function () {
					return this._.initValue || "";
				}
			}, true);

			CKEDITOR.dialog.addUIElement("select2", {
				build: function (dialog, elementDefinition, output) {
					return new CKEDITOR.ui.dialog.select2(dialog, elementDefinition, output);
				}
			});

			$("head").append("<link href='" + this.path + "styles/plugin.css' rel='stylesheet' />");
		}
	});
}(window.CKEDITOR));
