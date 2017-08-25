/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

(function ($) {

	function entityNotes(element) {
		this._element = $(element);
		this._target = this._element.data("target") || {};
		this._attachmentSettings = this._element.data("attachmentsettings");
		this._serviceUrlGet = this._element.attr("data-url-get");
		this._serviceUrlAdd = this._element.attr("data-url-add");
		this._serviceUrlEdit = this._element.attr("data-url-edit");
		this._serviceUrlDelete = this._element.attr("data-url-delete");
		this._serviceUrlGetAttachments = this._element.attr("data-url-get-attachments");
		this._hideFieldLabel = this._element.attr("data-hide-field-label");
		this._attachmentAcceptTypes = this._element.attr("data-add-accept-types");
		this._addEnabled = this._element.data("add-enabled");
		this._editEnabled = this._element.data("edit-enabled");
		this._deleteEnabled = this._element.data("delete-enabled");
		this._pageSize = this._element.attr("data-pagesize");
		this._orders = this._element.data("orders");
		this._addSuccess = false;
		this._editSuccess = false;
		this._deleteSuccess = false;
		this._useScrollingPagination = this._element.attr("data-use-scrolling-pagination");
		this._$editModal = this._element.children(".modal-editnote").appendTo("body");
		this._$deleteModal = this._element.children(".modal-deletenote").appendTo("body");
		var that = this;
		this._pageNumber = 1;

		$(element).on("refresh", function (e, page) {
			if (that._useScrollingPagination == "True") {
				page = -1;
			}

			that.load(page);
		});
	}

	$(document).ready(function () {
		var container = $(".entity-notes");
		if (container.length == 0) {
			container = $(".entity-timeline");
		}

		container.each(function () {
			Handlebars.registerHelper('if_eq', function (a, b, opts) {
				if (a == b) 
					return opts.fn(this);
				else
					return opts.inverse(this);
			});

			Handlebars.registerHelper('if_not_eq', function (a, b, opts) {
				if (a != b)
					return opts.fn(this);
				else
					return opts.inverse(this);
			});

			Handlebars.registerHelper('commaSeparatedList', function (items, opts) {
			    var commaSeparatedList = '';

			    for (var i = 0; i < items.length; i++) {
			        if (items[i].Name) {
			            commaSeparatedList += items[i].Name + (i !== (items.length - 1) ? ', ' : '');
			        }
			    }

			    return commaSeparatedList;
			});

			Handlebars.registerHelper('dateTimeFormatter', function (date) {
			    var moment = window.moment;
			    if (moment) {
			        var dateFormat = dateFormatConverter.convert("M/d/yyyy", dateFormatConverter.dotNet, dateFormatConverter.momentJs);
			        var timeFormat = dateFormatConverter.convert("h:mm tt", dateFormatConverter.dotNet, dateFormatConverter.momentJs);
			        var datetimeFormat = dateFormat + ' ' + timeFormat;
			        return moment(date).format(datetimeFormat);
			    }
			});

			new entityNotes($(this)).render();
		});
	});

	entityNotes.prototype.render = function () {
		var $this = this;
		var $element = $this._element;
		var $addNoteButton = ($this._hideFieldLabel === "True") ? $element.children(".timelineheader").find("a.addnote") : $element.children(".note-actions").find("a.addnote");
		var $modalAddNote = $element.children(".modal-addnote").appendTo("body");
		var $modalAddNoteButton = $modalAddNote.find(".modal-footer .btn-primary");
		var $moreButton = $element.children(".note-actions").find(".loadmore");

        if (this._hideFieldLabel === "False") {
        	$(".notes-cell .info").show();
        }

		$this.load();

		if ($this._addEnabled == "True") {
			$addNoteButton.show();

			$addNoteButton.on("click", function () {
				$modalAddNote.modal("show");
			});

			$modalAddNoteButton.on("click", function () {
				$this.addNote($modalAddNote);
			});

			$modalAddNote.on('hidden.bs.modal', function () {
				$modalAddNote.find("textarea").val('');
				$modalAddNote.find("input[type='file']").val('');
				$modalAddNote.find(".alert-danger.error").remove();
			});
		}

		if (this._useScrollingPagination == "True") {
			$moreButton.on("click", function () {
				$this._pageNumber += 1;
				$moreButton.hide();
				$this.load($this._pageNumber);
			});
		}
	}

	entityNotes.prototype.load = function (page) {
		var $this = this;
		var $element = $this._element;
		var $notes = $element.children(".notes");
		var $errorMessage = $element.children(".notes-error");
		var $emptyMessage = $element.children(".notes-empty");
		var $accessDeniedMessage = $element.children(".notes-access-denied");
		var $loadingMessage = $element.children(".notes-loading");
		var $loadingMessageMore = $element.children(".notes-loading-more");
		var $pagination = $element.find(".notes-pagination");
		var $moreButton = $element.children(".note-actions").find(".loadmore");
		var serviceUrlGet = $this._serviceUrlGet;
		var serviceUrlGetAttachments = $this._serviceUrlGetAttachments;
		var regarding = $this._target;
		var orders = $this._orders;
		var defaultPageSize = $this._pageSize;
		var useScrollingPagination = $this._useScrollingPagination;

		$errorMessage.hide();
		$emptyMessage.hide();
		$accessDeniedMessage.hide();
		
		if (useScrollingPagination == "True") {
			$loadingMessageMore.show();
		} else {
			$notes.hide().empty();
			$loadingMessage.show();
		}
		
		var pageNumber = $pagination.data("current-page");
		if (pageNumber == null || pageNumber == '') {
			pageNumber = 1;
		}
		page = page || pageNumber;

		var pageSize = $pagination.data("pagesize");
		if (pageSize == null || pageSize == '') {
			pageSize = defaultPageSize;
		}

		var isSingleActivity = false;
		if (useScrollingPagination == "True" && page == -1) {
			isSingleActivity = true;
		}

		$this.getData(serviceUrlGet, regarding, orders, (isSingleActivity) ? 1 : page, (isSingleActivity) ? 1 : pageSize,
			function (data) {
				// done
				if (typeof data === typeof undefined || data === false || data == null) {
					$emptyMessage.fadeIn();
					return;
				}
				if (typeof data.Records !== typeof undefined && data.Records !== false && (data.Records == null || (data.Records.length == 0 && page == 1))) {
					$emptyMessage.fadeIn();
					return;
				}
				if (typeof data.AccessDenied !== typeof undefined && data.AccessDenied !== false && data.AccessDenied) {
					$accessDeniedMessage.fadeIn();
					return;
				}

				var removeLastRecord = false;
				if (useScrollingPagination == "True") {
					if (isSingleActivity) {
						if ($this._pageNumber * pageSize < data.ItemCount) {
							$moreButton.show();
							removeLastRecord = true;
						} 
					} else if (data.PageCount > $this._pageNumber) {
						$moreButton.show();
					}
				}

				var source = $("#notes-template").html();

				Handlebars.registerHelper('AttachmentUrlWithTimeStamp', function () {
					return this.AttachmentUrl + "?t=" + new Date().getTime(); //unique cache-busting query parameter
				});

				// This helper acts returns a placeholder for attachments in the timeline template,
				// then asynchronously grabs the attachment data, compiles the attachment template and injects
				// it into the placeholder
				Handlebars.registerHelper('attachments', function (activityId, activityLogicalName) {
					// a unique id for this attachment placeholder
					var id = new Date().getTime();
					var attachmentSource = $("#notes-attachment-template").html();
					var attachmentTemplate = Handlebars.compile(attachmentSource);

					regardingActivity = {
						"Id": activityId,
						"LogicalName": activityLogicalName
					};

					$this.getAttachments(serviceUrlGetAttachments, regardingActivity, function (data) {
						var attachment = $(".attachmentasync-" + id);
						attachment.html(attachmentTemplate(data));
					});

					return $("<div>").html($("<div>").attr("class", "attachmentasync-" + id)).html();
				});

				var template = Handlebars.compile(source);

				if (useScrollingPagination == "True") {
					if (isSingleActivity) {
						$notes.prepend(template(data));
						if (removeLastRecord) {
							$notes.children(".note").last().remove();
						}
					} else {
						$notes.append(template(data));
					}
				} else {
					$notes.html(template(data));
				}

				$notes.find(".timeago").each(function () {
					var date = $(this).attr("title");
					var moment = window.moment;
					if (moment) {
						var dateFormat = dateFormatConverter.convert($element.closest("[data-dateformat]").data("dateformat") || "M/d/yyyy", dateFormatConverter.dotNet, dateFormatConverter.momentJs);
						var timeFormat = dateFormatConverter.convert($element.closest("[data-timeformat]").data("timeformat") || "h:mm tt", dateFormatConverter.dotNet, dateFormatConverter.momentJs);
						var datetimeFormat = dateFormat + ' ' + timeFormat;
						$(this).text(moment(date).format(datetimeFormat));
					}
				});

				$notes.find(".timeago").timeago();
				$notes.fadeIn();

				if (useScrollingPagination == "False") {
					$this.initializePagination(data);
				}
				if ($this._editEnabled && $this._editEnabled != "False") {
					$this.addEditClickEventHandlers();
				}
				if ($this._deleteEnabled && $this._deleteEnabled != "False") {
					$this.addDeleteClickEventHandlers();
				}
			},
			function (jqXhr, textStatus, errorThrown) {
				// fail
				$errorMessage.find(".details").append(errorThrown);
				$errorMessage.show();
			},
			function () {
				// always
				$loadingMessage.hide();
				$loadingMessageMore.hide();
			});
	}

	entityNotes.prototype.getData = function (url, regarding, orders, page, pageSize, done, fail, always) {
		done = $.isFunction(done) ? done : function () { };
		fail = $.isFunction(fail) ? fail : function () { };
		always = $.isFunction(always) ? always : function () { };
		if (!url || url == '') {
			always.call(this);
			fail.call(this, null, "error", window.ResourceManager['Service_URL_Not_Provided']);
			return;
		}
		if (!regarding) {
			always.call(this);
			fail.call(this, null, "error", window.ResourceManager['Required_Regarding_EntityReference_Parameter_Was_Not_Provided']);
			return;
		}
		pageSize = pageSize || -1;
		var data = {};
		data.regarding = regarding;
		data.orders = orders;
		data.page = page;
		data.pageSize = pageSize;
		var jsonData = JSON.stringify(data);
	    shell.ajaxSafePost({
			type: 'POST',
			dataType: "json",
			contentType: 'application/json',
			url: url,
			data: jsonData,
			global: false
		}).done(done).fail(fail).always(always);
	}

	entityNotes.prototype.getAttachments = function (url, regarding, done, fail, always) {
		done = $.isFunction(done) ? done : function () { };
		fail = $.isFunction(fail) ? fail : function () { };
		always = $.isFunction(always) ? always : function () { };
		if (!url || url == '') {
			always.call(this);
			fail.call(this, null, "error", window.ResourceManager['Service_URL_Not_Provided']);
			return;
		}
		if (!regarding) {
			always.call(this);
			fail.call(this, null, "error", window.ResourceManager['Required_Regarding_EntityReference_Parameter_Was_Not_Provided']);
			return;
		}
		var data = {};
		data.regarding = regarding;
		var jsonData = JSON.stringify(data);
		shell.ajaxSafePost({
			type: 'POST',
			dataType: "json",
			contentType: 'application/json',
			url: url,
			data: jsonData,
			success: done,
			error: fail,
			global: false
		});
	}

	entityNotes.prototype.addEditClickEventHandlers = function () {
		var $this = this;
		var $element = $this._element;
		var $modal = $this._$editModal;

		if (!$modal || $modal.length == 0) {
			return;
		}

		var $file = $modal.find("input[type='file']");
		var $button = $modal.find("button.primary");

		if ($file.length > 0) {
			$file.on('change', function () {
				$modal.find(".attachment").remove();
			});
		}

		$button.unbind("click");
		$button.on("click", function (e) {
			e.preventDefault();
			$this.updateNote($modal);
		});

		$modal.on('hidden.bs.modal', function () {
			$modal.find(".alert-danger.error").remove();
			$modal.find("textarea").val('');
			$modal.find("input[type='file']").val('');
			$modal.find(".alert-danger.error").remove();
			$modal.data("id", "");
			$modal.find(".attachment").empty();
		});

		$element.find(".edit-link").on("click", function (e) {
			e.preventDefault();
			var $note = $(this).closest(".note");
			var id = $note.data("id");
			var text = $note.data("unformattedtext") || "";
			var subject = $note.data("subject") || "";
			var isPrivate = $note.data("isprivate");
			var hasAttachment = $note.data("hasattachment");
			var attachmentFileName = $note.data("attachmentfilename");
			var attachmentFileSize = $note.data("attachmentfilesize");
			var attachmentUrl = $note.data("attachmenturl");
			var attachmentIsImage = $note.data("attachmentisimage");

			if (!id || id == '') {
				if (typeof console != 'undefined' && console) {
					console.log("Failed to launch edit note dialog. Data parameter 'id' is null.");
				}
				return;
			}

			$modal.data("id", id);
			$modal.data("subject", subject);
			$modal.find("textarea").val(text);
			if (isPrivate) {
				$modal.find("input[type='checkbox']").prop("checked", true);
			}
			var $fileContainer = $file.parent();
			if (hasAttachment) {
				attachmentUrl += "?t=" + new Date().getTime(); //unique cache-busting query parameter
				var $attachment = $modal.find(".attachment");
				if ($attachment.length == 0) {
					$attachment = $("<div class='attachment clearfix'></div>");
				}
				var $linkContainer = $("<div class='link'></div>");
				var $link = $("<a target='_blank'></a>").attr("href", attachmentUrl).html("<span class='fa fa-file' aria-hidden='true'></span> " + attachmentFileName + " (" + attachmentFileSize + ")");
				$linkContainer.html($link);
				if (attachmentIsImage) {
					var $imageLink = $("<a target='_blank' class='thumbnail'></a>").attr("href", attachmentUrl);
					var $image = $("<img />").attr("src", attachmentUrl);
					var $thumbnail = $("<div class='img col-md-4'></div>");
					$thumbnail.append($imageLink.html($image));
					$attachment.html($thumbnail).append($linkContainer);
					$fileContainer.prepend($attachment);
				} else {
					$attachment.html($linkContainer);
					$fileContainer.prepend($attachment);
				}
			}
			$modal.modal();
		});
	}

	entityNotes.prototype.addDeleteClickEventHandlers = function () {
		var $this = this;
		var $element = $this._element;
		var url = $this._serviceUrlDelete;
		var $modal = $this._$deleteModal;

		if (!$modal || $modal.length == 0) {
			return;
		}

		$modal.on('hidden.bs.modal', function () {
			$modal.find(".alert-danger.error").remove();
		});

		$element.find(".delete-link").on("click", function (e) {
			e.preventDefault();
			var $note = $(this).closest(".note");
			var id = $note.data("id");
			if (!id || id == '') {
				console.log("Failed to launch delete note dialog. Data parameter 'id' is null.");
				return;
			}
			var $button = $modal.find(".modal-footer button.primary");
			$button.unbind("click");
			$button.on("click", function () {
				$(this).attr("disabled", "disabled").prepend("<span class='fa fa-spinner fa-spin' aria-hidden='true'></span>");
				var data = {};
				data.id = id;
				var jsonData = JSON.stringify(data);
			    shell.ajaxSafePost({
				    type: "POST",
					contentType: "application/json",
					url: url,
					data: jsonData
				}).done(function () {
					$this._deleteSuccess = true;
					$element.trigger("refresh");
					$modal.modal("hide");
				}).fail(function (jqXhr) {
					onFail(getError(jqXhr), $modal);
				}).always(function () {
					$button.removeAttr("disabled", "disabled").find(".fa-spin").remove();
				});
			});
			$modal.modal();
		});
	}

	entityNotes.prototype.addNote = function ($modal) {
		var $this = this;
		var $element = $this._element;
		var target = $this._target;
		var url = $this._serviceUrlAdd;
		var attachmentAcceptTypes = this._attachmentAcceptTypes;
		var $button = $modal.find(".modal-footer button.primary");
		var noteText = $modal.find("textarea").val();
		var isPrivate = false;
		
		if (url == null || url == '') {
			var urlError = { Message: "System Error", InnerError: { Message: window.ResourceManager['URL_Service_For_Add_Note_Request_Could_Not_Determined'] } };
			onFail(urlError, $modal);
			return;
		}

		if (noteText == null || !/\S+/gm.test(noteText)) {
			var labelText = $modal.find("#note_label");
			if (labelText) {
			    var noteLengthError = { Message: window.ResourceManager['Required_Field_Error'].replace('{0}', labelText.text()) };
				onFail(noteLengthError, $modal);
			}
			return;
		}

		var $isPrivate = $modal.find("input[type='checkbox']");

		if ($isPrivate.length > 0) {
			isPrivate = $isPrivate.prop('checked');
		}

		var $file = $modal.find("input[type='file']");

		$button.attr("disabled", "disabled").prepend("<span class='fa fa-spinner fa-spin' aria-hidden='true'></span>");

		var $form = $('<form>').attr('id', 'add-note-' + new Date().getTime()).attr('method', 'POST').attr('action', url).hide().appendTo($('body'));
		$('<input>').attr('name', 'regardingEntityLogicalName').attr('type', 'hidden').appendTo($form).val(target.LogicalName);
		$('<input>').attr('name', 'regardingEntityId').attr('type', 'hidden').appendTo($form).val(target.Id);
		$('<input>').attr('name', 'text').attr('type', 'hidden').appendTo($form).val(noteText);
		$('<input>').attr('name', 'isPrivate').attr('type', 'hidden').appendTo($form).val(isPrivate);
		$('<input>').attr('name', 'attachmentSettings').attr('type', 'hidden').appendTo($form).val($this._attachmentSettings);

		var $newFile = $('<input>').attr('type', 'file').attr('name', 'file').attr("accept", attachmentAcceptTypes).insertAfter($file);

		$file.appendTo($form);

		$form.submit(function () {
		    shell.ajaxSafePost({
				success: function () {
					$this._addSuccess = true;
					$element.trigger("refresh");
					$modal.modal("hide");
					$form.remove();
					$button.removeAttr("disabled", "disabled").find(".fa-spin").remove();
				},
				error: function (jqXhr) {
					onFail({ Message: jqXhr.statusText }, $modal);
					$file.insertAfter($newFile);
					$newFile.remove();
					$form.remove();
					$button.removeAttr("disabled", "disabled").find(".fa-spin").remove();
				}
		    }, $(this));

			return false;
		});

		$form.submit();
	}

	entityNotes.prototype.updateNote = function ($modal) {
		var $this = this;
		var $element = $this._element;
		var url = $this._serviceUrlEdit;
		var $button = $modal.find(".modal-footer button.primary");
		var noteText = $modal.find("textarea").val();
		var subject = $modal.data("subject") || "";
		var isPrivate = false;

		if (url == null || url == '') {
			var urlError = { Message: "System Error", InnerError: { Message: window.ResourceManager['URL_Service_For_Update_Note_Request_Could_Not_Determined'] } };
			onFail(urlError, $modal);
			return;
		}

		var id = $modal.data("id");

		if (!id || id == '') {
			var idError = { Message: "System Error", InnerError: { Message: window.ResourceManager['Failed_Determine_RecordID'] } };
			onFail(idError, $modal);
			return;
		}

		if (noteText == null || !/\S+/gm.test(noteText)) {
			var labelText = $modal.find("#note_label");
			if (labelText) {
			    var noteLengthError = { Message: window.ResourceManager['Required_Field_Error'].replace('{0}', labelText.text()) };
			onFail(noteLengthError, $modal);
		}
		return;
		}

		var $isPrivate = $modal.find("input[type='checkbox']");

		if ($isPrivate.length > 0) {
			isPrivate = $isPrivate.prop('checked');
		}

		var $file = $modal.find("input[type='file']");

		$button.attr("disabled", "disabled").prepend("<span class='fa fa-spinner fa-spin' aria-hidden='true'></span>");

		var $form = $('<form>').attr('id', 'update-note-' + new Date().getTime()).attr('method', 'POST').attr('action', url).hide().appendTo($('body'));

		$('<input>').attr('name', 'id').attr('type', 'hidden').appendTo($form).val(id);
		$('<input>').attr('name', 'subject').attr('type', 'hidden').appendTo($form).val(subject);
		$('<input>').attr('name', 'text').attr('type', 'hidden').appendTo($form).val(noteText);
		$('<input>').attr('name', 'isPrivate').attr('type', 'hidden').appendTo($form).val(isPrivate);
		$('<input>').attr('name', 'attachmentSettings').attr('type', 'hidden').appendTo($form).val($this._attachmentSettings);

		var $newFile = $('<input>').attr('type', 'file').attr('name', 'file').insertAfter($file);

		$file.appendTo($form);

		$form.submit(function () {
		    shell.ajaxSafePost({
				success: function () {
					$this._editSuccess = true;
					$element.trigger("refresh");
					$modal.modal("hide");
					$form.remove();
					$button.removeAttr("disabled", "disabled").find(".fa-spin").remove();
				},
				error: function (jqXhr) {
					onFail({ Message: jqXhr.statusText }, $modal);
					$file.insertAfter($newFile);
					$newFile.remove();
					$form.remove();
					$button.removeAttr("disabled", "disabled").find(".fa-spin").remove();
				}
		    }, $(this));

			return false;
		});

		$form.submit();
	}

	entityNotes.prototype.initializePagination = function (data) {
		// requires ~/js/jquery.bootstrap-pagination.js
		if (typeof data === typeof undefined || data === false || data == null) {
			return;
		}

		if ((typeof data.PageSize === typeof undefined || data.PageSize === false || data.PageSize == null) ||
		(typeof data.PageCount === typeof undefined || data.PageCount === false || data.PageCount == null) ||
		(typeof data.PageNumber === typeof undefined || data.PageNumber === false || data.PageNumber == null) ||
		(typeof data.ItemCount === typeof undefined || data.ItemCount === false || data.ItemCount == null)) {
			return;
		}

		var $this = this;
		var $element = $this._element;
		var $pagination = $element.find(".notes-pagination");

		if (data.PageCount <= 1) {
			$pagination.hide();
			return;
		}

		$pagination
			.data("pagesize", data.PageSize)
			.data("pages", data.PageCount)
			.data("current-page", data.PageNumber)
			.data("count", data.ItemCount)
			.unbind("click")
			.pagination({
				total_pages: $pagination.data("pages"),
				current_page: $pagination.data("current-page"),
				callback: function (event, pg) {
					event.preventDefault();
					var $li = $(event.target).closest("li");
					if ($li.not(".disabled").length > 0 && $li.not(".active").length > 0) {
						$this.load(pg);
					}
				}
			})
			.show();
	}

	function onFail(error, $modal) {
		if (typeof error !== typeof undefined && error !== false && error != null) {
			if (typeof console != 'undefined' && console) {
				console.log(error);
			}

			var $body = $modal.find(".modal-body");

			var $error = $modal.find(".alert-danger.error");

			if ($error.length == 0) {
				$error = $("<div></div>").addClass("alert alert-block alert-danger error clearfix");
			} else {
				$error.empty();
			}

			var $appendToError = $("<p><span class='fa fa-exclamation-triangle' aria-hidden='true'></span></p>");
			if (typeof error.InnerError !== typeof undefined &&
				typeof error.InnerError.Message !== typeof undefined &&
				error.InnerError.Message !== false &&
				error.InnerError.Message != null)
			{
				$error.append($appendToError).text(error.InnerError.Message + ((typeof error.InnerError.Message === "number") ? " Error" : ""));
			} else if (typeof error.Message !== typeof undefined &&
						error.Message !== false &&
						error.Message != null)
			{
				$error.append($appendToError).text(error.Message + ((typeof error.Message === "number") ? " Error" : ""));
			}

			$body.prepend($error);
		}
	}

	function getError(jqXhr) {
		var error = { Message: window.ResourceManager['UnKnown_Error_Occurred'] };
		if (jqXhr == null) return error;
		try {
			var contentType = jqXhr.getResponseHeader("content-type");
			if (contentType != null) {
				error = contentType.indexOf("json") > -1 ? $.parseJSON(jqXhr.responseText) : { Message: jqXhr.status, InnerError: { Message: jqXhr.statusText } };
			} else {
				error = { Message: jqXhr.statusText };
			}
		} catch (e) {
			error = { Message: e.message }
		}
		return error;
	}
}(jQuery));
