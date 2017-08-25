/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

(function ($) {

	function entityForm(element) {
		this._element = $(element);
		this._layout = null;
		var $element = this._element;
		var $parent = $element.closest(".entity-form");

		if ($parent.length < 1) {
			var $grandparent = $element.closest(".crmEntityFormView");
			$parent = $grandparent.find(".entity-form");
		}

		if ($parent.length == 0) return;

		var $config = $parent.find("[id$='_EntityLayoutConfig']");

		if ($config.length == 0) return;

		this._layout = $config.data("form-layout");
	}
	
	$(document).ready(function () {
		$(".form-custom-actions").each(function () {
			new entityForm($(this)).render();
		});

		$(".form-custom-actions .dropdown-toggle").each(function () {
			var $menu = $(this).parent().find(".dropdown-menu");
			var count = $menu.find("a:not(.hidden)").length;
			if (count < 1) {
				$(this).parent().hide();
			}
		});
		$(".crmEntityFormView").attr("role", "main");
		var headerLabel = $(".page-header h1").text();
		var genericEntityLabel = window.ResourceManager["Entity_Form_Label"];
		$(".crmEntityFormView").attr("aria-label", headerLabel || genericEntityLabel);
	});
	

	function getFormActionLinks() {
		var formActionLinks = [];
		var layout = this._layout;
		if ((layout && layout.TopFormActionLinks && layout.TopFormActionLinks.length) && (layout.BottomFormActionLinks && layout.BottomFormActionLinks.length)) {
			formActionLinks = _.union(layout.TopFormActionLinks, layout.BottomFormActionLinks);
		} else if (layout && layout.TopFormActionLinks && layout.TopFormActionLinks.length) {
			formActionLinks = layout.TopFormActionLinks;
		} else if (layout) {
			formActionLinks = layout.BottomFormActionLinks;
		}
		return formActionLinks;
	}

	function getUrlWithRelatedReference($element, uri) {
		uri = URI(uri);
		var refEntityName = $element.data("ref-entity");
		var refEntityId = $element.data("ref-id");
		var refRelationship = $element.data("ref-rel");
		var refRelationshipRole = $element.data("ref-rel-role");

		if (refEntityName && refEntityId && refRelationship) {
			uri.addSearch('refentity', refEntityName);
			uri.addSearch('refid', refEntityId);
			uri.addSearch('refrel', refRelationship);

			if (refRelationshipRole) {
				uri.addSearch('refrelrole', refRelationshipRole);
			}
		}

		return uri.toString();
	}

	// Returns URL for Create Related Record action. 
	// Contains params to fill referencing entity.
	function getUrlForCreateRelatedRecordAction(uri, refEntityName, refEntityId, refRelationship) {
		uri = URI(uri);

		if (refEntityName && refEntityId && refRelationship) {
			uri.addSearch('refentity', refEntityName);
			uri.addSearch('refid', refEntityId);
			uri.addSearch('refrel', refRelationship);
		}

		return uri.toString();
	}

	entityForm.prototype.render = function () {
		var $this = this;
		var $element = $this._element;
		var layout = $this._layout;

		if (typeof layout === typeof undefined || layout == null || layout == '') {
			$element.hide();
		}

		var delLink = $this.addDeleteActionLinkClickEventHandlers();
		var worlink = $this.addWorkflowActionLinkClickEventHandlers();
		var qualink = $this.addQualifyLeadActionLinkClickEventHandlers();
		var cloLink = $this.addCloseCaseActionLinkClickEventHandlers();
		var resLink = $this.addResolveCaseActionLinkClickEventHandlers();
		var relink = $this.addReopenCaseActionLinkClickEventHandlers();
		var canlink = $this.addCancelCaseActionLinkClickEventHandlers();
		var conlink = $this.addConvertQuoteActionLinkClickEventHandlers();
		var conorderLink = $this.addConvertOrderActionLinkClickEventHandlers();
		var calLink = $this.addCalculateOpportunityActionLinkClickEventHandlers();
		var actLink = $this.addActivateActionLinkClickEventHandlers();
		var decLink = $this.addDeactivateActionLinkClickEventHandlers();
		var actQuoteLink = $this.addActivateQuoteActionLinkClickEventHandlers();
		var onholdLink = $this.addSetOpportunityOnHoldActionLinkClickEventHandlers();
		var reOppLink = $this.addReopenOpportunityActionLinkClickEventHandlers();
		var winlink = $this.addWinOpportunityActionLinkClickEventHandlers();
		var loselink = $this.addLoseOpportunityActionLinkClickEventHandlers();
		var genlink = $this.addGenerateQuoteFromOpportunityActionLinkClickEventHandlers();
		var upLink = $this.addUpdatePipelinePhaseActionLinkClickEventHandlers();
		var submitLink = $this.addSubmitActionLinkClickEventHandlers();
		var nextLink = $this.addNextActionLinkClickEventHandlers();
		var previousLink = $this.addPreviousActionLinkClickEventHandlers();
		var crRelRecordLink = $this.addCreateRelatedRecordClickEventHandlers();

		if ((delLink || worlink || qualink || cloLink || resLink || relink || canlink || conlink || conorderLink || calLink
			|| actLink || decLink || actQuoteLink || onholdLink || winlink || loselink || genlink || upLink || reOppLink || submitLink || nextLink || 
			previousLink) && layout != null && (layout.EnableActions || submitLink || nextLink || previousLink || crRelRecordLink))
		{
			$element.show();
		}
		else
		{
			$element.hide();
		}
	};
	
	entityForm.prototype.addDeleteActionLinkClickEventHandlers = function () {
		var $this = this;
		var $element = $this._element;
		var layout = $this._layout;

		if (layout == null || layout == '' || layout.DeleteActionLink == null || !layout.DeleteActionLink.Enabled || !layout.DeleteActionLink.URL) {
			var element = $element.find(".delete-link");
			element.hide();
			element.addClass("hidden");
			return false;
		}
		
		var url = layout.DeleteActionLink.URL.PathWithQueryString;
		
		var $modal = $element.children(".modal-delete");
		var $parent = $element.closest(".entity-form");

		if ($parent.length < 1) {
			var $grandparent = $element.closest(".crmEntityFormView");
			$parent = $grandparent.find(".entity-form");
		}

		var id = $parent.find("[id$='_EntityID']").val();
		var $button = $modal.find("button.primary");
		$button.off("click");
		$button.on("click", function (ev) {
			ev.preventDefault();
			$(this).attr("disabled", "disabled").prepend("<span class='fa fa-spinner fa-spin' aria-hidden='true'></span>");
			$parent.find(".navbar-collapse").find(".action-status").html("<span class='fa fa-spinner fa-spin fa-fw' aria-hidden='true'></span>");
			var entityReference = {};
			entityReference.LogicalName = layout.EntityName;
			entityReference.Id = id;
			var data = JSON.stringify(entityReference);
			shell.ajaxSafePost({
				type: "POST",
				contentType: "application/json",
				url: url,
				data: data
			}).done(function () {
				$modal.modal("hide");
				createNotificationCookie(layout.DeleteActionLink.SuccessMessage);
				onComplete(layout.DeleteActionLink, true);
			}).fail(function (jqXhr) {
				$modal.modal("hide");
				displayErrorAlert(getError(jqXhr), $parent);
			}).always(function () {
				$button.removeAttr("disabled", "disabled").find(".fa-spin").remove();
			});
		});

		$element.find(".delete-link").on("click", function (e) {
			e.preventDefault();
			$modal.modal();
		});

		return true;
	};

	entityForm.prototype.addSubmitActionLinkClickEventHandlers = function () {
		var $this = this;
		var $element = $this._element;
		var layout = $this._layout;

		if (layout == null || layout == '' || layout.SubmitActionLink == null || !layout.SubmitActionLink.Enabled) {
			var element = $element.find(".submit-btn");
			element.addClass("hidden");
			element.hide();
			return false;
		}
		return true;
	};

	entityForm.prototype.addNextActionLinkClickEventHandlers = function () {
		var $this = this;
		var $element = $this._element;
		var layout = $this._layout;

		if (layout == null || layout == '' || layout.NextActionLink == null || !layout.NextActionLink.Enabled) {
			var element = $element.find(".next-btn");
			element.addClass("hidden");
			element.hide();
			return false;
		}
		return true;
	};

	entityForm.prototype.addPreviousActionLinkClickEventHandlers = function () {
		var $this = this;
		var $element = $this._element;
		var layout = $this._layout;

		if (layout == null || layout == '' || layout.PreviousActionLink == null || !layout.PreviousActionLink.Enabled) {
			var element = $element.find(".previous-btn");
			element.addClass("hidden");
			element.hide();
			return false;
		}
		return true;
	};
	
	entityForm.prototype.addWorkflowActionLinkClickEventHandlers = function () {
		var $this = this;
		var $element = $this._element;
		var layout = $this._layout;
		var $parent = $element.closest(".entity-form");

		if ($parent.length < 1) {
			var $grandparent = $element.closest(".crmEntityFormView");
			$parent = $grandparent.find(".entity-form");
		}

		if (!layout) {
			return false;
		}

		var formActionLinks = getFormActionLinks.call($this);

		if (formActionLinks.length == 0) {
			$element.find(".workflow-link").hide();
			return false;
		}

		var actionLink = null;
		for (var i = 0; i < formActionLinks.length; i++) {
			var item = formActionLinks[i];
			if (item != null && typeof item.Type !== typeof undefined && item.Type == 7 && typeof item.Workflow !== typeof undefined && item.Workflow != null) {
				if (!item.Enabled || !item.URL) {
					$element.find(".workflow-link[data-workflowid='" + item.Workflow.Id + "']").hide();
					$element.find(".workflow-link[data-workflowid='" + item.Workflow.Id + "']").addClass("hidden");
				} else {
					actionLink = item;
				}
			}
		}

		if (!actionLink) {
			return false;
		}

		if (actionLink.ShowModal == 1) {
			var $modal = $element.children(".modal-run-workflow");

			$element.find(".workflow-link").on("click", function(e) {
				e.preventDefault();
				$modal.attr("aria-hidden", "false");
				var id = $parent.find("[id$='_EntityID']").val();
				var $button = $modal.find("button.primary");
				var $cancelbutton = $modal.find("button.cancel");
				var $closebutton = $modal.find("button.close");
				var $modalSpawningButton = $(this);
				var url = $(this).data("url");
				var workflowid = $(this).data("workflowid");
				if (!workflowid || !url) {
					return;
				}
				var modalTitle = $(this).data("modal-title");
				if (modalTitle) {
					$modal.find(".modal-title").html(modalTitle).attr("title", modalTitle);
				}
				$modal.parent().find('section').attr('aria-label', modalTitle);
				var modalConfirmation = $(this).data("modal-confirmation");
				if (modalConfirmation) {
					$modal.find(".modal-body").html(modalConfirmation);
				}
				var modalPrimaryAction = $(this).data("modal-primary-action");
				if (modalPrimaryAction) {
					$button.html(modalPrimaryAction);
					$button.attr('title', modalPrimaryAction);
					$button.attr('aria-label', modalPrimaryAction);
				}
				var modalCancelAction = $(this).data("modal-cancel-action");
				if (modalCancelAction) {
					$modal.find(".cancel").html(modalCancelAction);
				}
				$button.off("click");
				$button.on("click", function(ev) {
					ev.preventDefault();
					$modal.attr("aria-hidden", "true");
					$(this).attr("disabled", "disabled").prepend("<span class='fa fa-spinner fa-spin' aria-hidden='true'></span>");
					var workflowActionLink = null;
					for (var j = 0; j < formActionLinks.length; j++) {
						var action = formActionLinks[j];
						if (action != null && typeof action.Type !== typeof undefined && action.Type == 7 && typeof action.Workflow !== typeof undefined && action.Workflow != null && action.Workflow.Id == workflowid) {
							workflowActionLink = action;
							break;
						}
					}
					var data = {};
					var workflowReference = {};
					workflowReference.LogicalName = "workflow";
					workflowReference.Id = workflowid;
					data.workflow = workflowReference;
					var entityReference = {};
					entityReference.LogicalName = layout.EntityName;
					entityReference.Id = id;
					data.entity = entityReference;
					var json = JSON.stringify(data);
					shell.ajaxSafePost({
						type: "POST",
						contentType: "application/json",
						url: url,
						data: json
					}).done(function() {
						$modal.modal("hide");
						createNotificationCookie(workflowActionLink != null ? workflowActionLink.SuccessMessage : null);
						onComplete(workflowActionLink);
					}).fail(function(jqXhr) {
						$modal.modal("hide");
						displayErrorAlert(getError(jqXhr), $parent);
					}).always(function() {
						$button.removeAttr("disabled", "disabled").find(".fa-spin").remove();
					});
				});
				$cancelbutton.on("click", function (ev) {
					$modal.attr("aria-hidden", "true");
				});
				$closebutton.on("click", function (ev) {
					$modal.attr("aria-hidden", "true");
				});
				$modal.modal();
				$modal.on('shown.bs.modal',
					function() {
						$modal.find("button.primary").focus();
					});

				$modal.on('hidden.bs.modal',
					function () {
						if ($modalSpawningButton.is(':visible')) {
							$modalSpawningButton.trigger('focus');
						}
					});
			});
		} else {
			$element.find(".workflow-link").on("click", function(e) {
				e.preventDefault();
				var id = $parent.find("[id$='_EntityID']").val();
				var workflowid = $(this).data("workflowid");
				var url = $(this).data("url");
				if (!workflowid || !url) {
					return;
				}
				var workflowActionLink = null;
				for (var j = 0; j < formActionLinks.length; j++) {
					var action = formActionLinks[j];
					if (action != null && typeof action.Type !== typeof undefined && action.Type == 7 && typeof action.Workflow !== typeof undefined && action.Workflow != null && action.Workflow.Id == workflowid) {
						workflowActionLink = action;
						break;
					}
				}
				var data = {};
				var workflowReference = {};
				workflowReference.LogicalName = "workflow";
				workflowReference.Id = workflowid;
				data.workflow = workflowReference;
				var entityReference = {};
				entityReference.LogicalName = layout.EntityName;
				entityReference.Id = id;
				data.entity = entityReference;
				var json = JSON.stringify(data);
				shell.ajaxSafePost({
					type: "POST",
					contentType: "application/json",
					url: url,
					data: json
				}).done(function () {
					createNotificationCookie(workflowActionLink != null ? workflowActionLink.SuccessMessage : null);
					onComplete(workflowActionLink, true);
				}).fail(function (jqXhr) {
					displayErrorAlert(getError(jqXhr), $parent);
				});
			});
		}

		return true;
	};

	entityForm.prototype.addQualifyLeadActionLinkClickEventHandlers = function() {
		var $this = this;
		var $element = $this._element;
		var layout = $this._layout;
		var $parent = $element.closest(".entity-form");

		if ($parent.length < 1) {
			var $grandparent = $element.closest(".crmEntityFormView");
			$parent = $grandparent.find(".entity-form");
		}

		var statecode = $parent.find("[id$='_EntityState']").val();

		if (layout == null || layout == '' || layout.QualifyLeadActionLink == null || !layout.QualifyLeadActionLink.Enabled || !layout.QualifyLeadActionLink.URL || statecode != "0") {
			var element = $element.find(".qualify-lead-link");
			element.addClass("hidden");
			element.hide();
			return false;
		}

		if (layout.QualifyLeadActionLink.ShowModal == 1)
		{
			var $modal = $element.children(".modal-qualify");
			
			$element.find(".qualify-lead-link").on("click", function(e) {
				e.preventDefault();
				var id = $parent.find("[id$='_EntityID']").val();
				var $button = $modal.find("button.primary");
				var url = $(this).data("url");

				$button.off("click");
				$button.on("click", function(ev) {
					ev.preventDefault();
					$(this).attr("disabled", "disabled").prepend("<span class='fa fa-spinner fa-spin' aria-hidden='true'></span>");
					var data = {};
					data.createAccount = true;
					data.createContact = true;
					data.createOpportunity = true;
					var entityReference = {};
					entityReference.LogicalName = layout.EntityName;
					entityReference.Id = id;
					data.entityReference = entityReference;
					var json = JSON.stringify(data);
					shell.ajaxSafePost({
						type: "POST",
						contentType: "application/json",
						url: url,
						data: json
					}).done(function() {
						$modal.modal("hide");
						createNotificationCookie(layout.QualifyLeadActionLink.SuccessMessage);
						onComplete(layout.QualifyLeadActionLink);
					}).fail(function(jqXhr) {
						$modal.modal("hide");
						displayErrorAlert(getError(jqXhr), $parent);
					}).always(function() {
						$button.removeAttr("disabled", "disabled").find(".fa-spin").remove();
					});
				});
				$modal.modal();
			});
		} else {
			$element.find(".qualify-lead-link").on("click", function (e) {
				e.preventDefault();
				var id = $parent.find("[id$='_EntityID']").val();
				var url = $(this).data("url");
				var data = {};
				data.createAccount = true;
				data.createContact = true;
				data.createOpportunity = true;
				var entityReference = {};
				entityReference.LogicalName = layout.EntityName;
				entityReference.Id = id;
				data.entityReference = entityReference;
				var json = JSON.stringify(data);
				shell.ajaxSafePost({
					type: "POST",
					contentType: "application/json",
					url: url,
					data: json
				}).done(function () {
					createNotificationCookie(layout.QualifyLeadActionLink.SuccessMessage);
					onComplete(layout.QualifyLeadActionLink);
				}).fail(function (jqXhr) {
					displayErrorAlert(getError(jqXhr), $parent);
				});
			});
		}

		return true;
	};
	
	entityForm.prototype.addCloseCaseActionLinkClickEventHandlers = function () {
		var $this = this;
		var $element = $this._element;
		var layout = $this._layout;
		var $parent = $element.closest(".entity-form");

		if ($parent.length < 1) {
			var $grandparent = $element.closest(".crmEntityFormView");
			$parent = $grandparent.find(".entity-form");
		}

		var statecode = $parent.find("[id$='_EntityState']").val();

		if (layout == null || layout == '' || layout.CloseIncidentActionLink == null || !layout.CloseIncidentActionLink.Enabled || !layout.CloseIncidentActionLink.URL || statecode != "0") {
			var element = $element.find(".close-case-link");
			element.addClass("hidden");
			element.hide();
			return false;
		}

		if (layout.CloseIncidentActionLink.ShowModal == 1)
		{
			var $modal = $element.children(".modal-closecase");
			
			$element.find(".close-case-link").on("click", function(e) {
				e.preventDefault();
				var id = $parent.find("[id$='_EntityID']").val();
				var $button = $modal.find("button.primary");
				var url = $(this).data("url");

				$button.off("click");
				$button.on("click", function(ev) {
					ev.preventDefault();
					$(this).attr("disabled", "disabled").prepend("<span class='fa fa-spinner fa-spin' aria-hidden='true'></span>");
					var data = {};
					var entityReference = {};
					entityReference.LogicalName = layout.EntityName;
					entityReference.Id = id;
					data.entityReference = entityReference;
					var resolution = layout.CloseIncidentActionLink.DefaultResolution;
					var description = layout.CloseIncidentActionLink.DefaultResolutionDescription;
					data.resolutionSubject = resolution;
					data.resolutionDescription = description;
					var json = JSON.stringify(data);
					shell.ajaxSafePost({
						type: "POST",
						contentType: "application/json",
						url: url,
						data: json
					}).done(function() {
						$modal.modal("hide");
						createNotificationCookie(layout.CloseIncidentActionLink.SuccessMessage);
						onComplete(layout.CloseIncidentActionLink);
					}).fail(function(jqXhr) {
						$modal.modal("hide");
						displayErrorAlert(getError(jqXhr), $parent);
					}).always(function() {
						$button.removeAttr("disabled", "disabled").find(".fa-spin").remove();
					});
				});
				$modal.modal();
			});
			$modal.on('show.bs.modal', function () {
			    $modal.attr("aria-hidden", "false");
			});

			$modal.on('hidden.bs.modal', function () {
			    $modal.attr("aria-hidden", "true");
			});
		} else {
			$element.find(".close-case-link").on("click", function (e) {
				e.preventDefault();
				var id = $parent.find("[id$='_EntityID']").val();
				var url = $(this).data("url");
				var data = {};
				var entityReference = {};
				entityReference.LogicalName = layout.EntityName;
				entityReference.Id = id;
				data.entityReference = entityReference;
				var resolution = layout.CloseIncidentActionLink.DefaultResolution;
				var description = layout.CloseIncidentActionLink.DefaultResolutionDescription;
				data.resolutionSubject = resolution;
				data.resolutionDescription = description;
				var json = JSON.stringify(data);
				shell.ajaxSafePost({
					type: "POST",
					contentType: "application/json",
					url: url,
					data: json
				}).done(function () {
					createNotificationCookie(layout.CloseIncidentActionLink.SuccessMessage);
					onComplete(layout.CloseIncidentActionLink);
				}).fail(function (jqXhr) {
					displayErrorAlert(getError(jqXhr), $parent);
				});
			});
		}

		return true;
	};

	entityForm.prototype.addConvertQuoteActionLinkClickEventHandlers = function () {
		var $this = this;
		var $element = $this._element;
		var layout = $this._layout;
		var $parent = $element.closest(".entity-form");

		if ($parent.length < 1) {
			var $grandparent = $element.closest(".crmEntityFormView");
			$parent = $grandparent.find(".entity-form");
		}

		var statecode = $parent.find("[id$='_EntityState']").val();

		if (layout == null || layout == '' || layout.ConvertQuoteToOrderActionLink == null || !layout.ConvertQuoteToOrderActionLink.Enabled || !layout.ConvertQuoteToOrderActionLink.URL || statecode != "1") {
			var element = $element.find(".convert-quote-link");
			element.addClass("hidden");
			element.hide();
			return false;
		}

		if (layout.ConvertQuoteToOrderActionLink.ShowModal == 1) {
			var $modal = $element.children(".modal-convert-quote");
			$element.find(".convert-quote-link").on("click", function(e) {
				e.preventDefault();
				var id = $parent.find("[id$='_EntityID']").val();
				var $button = $modal.find("button.primary");
				var url = $(this).data("url");

				$button.off("click");
				$button.on("click", function(ev) {
					ev.preventDefault();
					$(this).attr("disabled", "disabled").prepend("<span class='fa fa-spinner fa-spin' aria-hidden='true'></span>");
					var data = {};
					var entityReference = {};
					entityReference.LogicalName = layout.EntityName;
					entityReference.Id = id;
					data.entityReference = entityReference;
					var json = JSON.stringify(data);
					shell.ajaxSafePost({
						type: "POST",
						contentType: "application/json",
						url: url,
						data: json
					}).done(function() {
						$modal.modal("hide");
						createNotificationCookie(layout.ConvertQuoteToOrderActionLink.SuccessMessage);
						onComplete(layout.ConvertQuoteToOrderActionLink);
					}).fail(function(jqXhr) {
						$modal.modal("hide");
						displayErrorAlert(getError(jqXhr), $parent);
					}).always(function() {
						$button.removeAttr("disabled", "disabled").find(".fa-spin").remove();
					});
				});
				$modal.modal();
			});
		} else {
			$element.find(".convert-quote-link").on("click", function (e) {
				e.preventDefault();
				var $button = $(this);
				$button.attr("disabled", "disabled");
				$button.closest("li.action").children(".dropdown-toggle").css("cursor", "not-allowed").dropdown('toggle').removeAttr('data-toggle').off("click");
				$button.closest(".navbar-collapse").find(".action-status").html("<span class='fa fa-spinner fa-spin fa-fw' aria-hidden='true'></span>");
				var id = $parent.find("[id$='_EntityID']").val();
				var url = $(this).data("url");
				var data = {};
				var entityReference = {};
				entityReference.LogicalName = layout.EntityName;
				entityReference.Id = id;
				data.entityReference = entityReference;
				var json = JSON.stringify(data);
				shell.ajaxSafePost({
					type: "POST",
					contentType: "application/json",
					url: url,
					data: json
				}).done(function () {
					createNotificationCookie(layout.ConvertQuoteToOrderActionLink.SuccessMessage);
					onComplete(layout.ConvertQuoteToOrderActionLink);
				}).fail(function(jqXhr) {
					displayErrorAlert(getError(jqXhr), $parent);
				}).always(function() {
					$button.removeAttr("disabled");
					$button.closest("li.action").children(".dropdown-toggle").css("cursor", "").attr("data-toggle", "dropdown").data("toggle", "dropdown").on("click", function() { $(this).dropdown(); });
				});
			});
		}

		return true;
	};

	entityForm.prototype.addConvertOrderActionLinkClickEventHandlers = function () {
		var $this = this;
		var $element = $this._element;
		var layout = $this._layout;
		var $parent = $element.closest(".entity-form");

		if ($parent.length < 1) {
			var $grandparent = $element.closest(".crmEntityFormView");
			$parent = $grandparent.find(".entity-form");
		}

		var statecode = $parent.find("[id$='_EntityState']").val();

		if (layout == null || layout == '' || layout.ConvertOrderToInvoiceActionLink == null || !layout.ConvertOrderToInvoiceActionLink.Enabled || !layout.ConvertOrderToInvoiceActionLink.URL || statecode == "2") {
			var element = $element.find(".convert-order-link");
			element.addClass("hidden");
			element.hide();
			return false;
		}

		if (layout.ConvertOrderToInvoiceActionLink.ShowModal == 1) {
			var $modal = $element.children(".modal-convert-order");
			$element.find(".convert-order-link").on("click", function(e) {
				e.preventDefault();
				var id = $parent.find("[id$='_EntityID']").val();
				var $button = $modal.find("button.primary");
				var url = $(this).data("url");

				$button.off("click");
				$button.on("click", function(ev) {
					ev.preventDefault();
					$(this).attr("disabled", "disabled").prepend("<span class='fa fa-spinner fa-spin' aria-hidden='true'></span>");
					var data = {};
					var entityReference = {};
					entityReference.LogicalName = layout.EntityName;
					entityReference.Id = id;
					data.entityReference = entityReference;
					var json = JSON.stringify(data);
					shell.ajaxSafePost({
						type: "POST",
						contentType: "application/json",
						url: url,
						data: json
					}).done(function() {
						$modal.modal("hide");
						createNotificationCookie(layout.ConvertOrderToInvoiceActionLink.SuccessMessage);
						onComplete(layout.ConvertOrderToInvoiceActionLink);
					}).fail(function(jqXhr) {
						$modal.modal("hide");
						displayErrorAlert(getError(jqXhr), $parent);
					}).always(function() {
						$button.removeAttr("disabled", "disabled").find(".fa-spin").remove();
					});
				});
				$modal.modal();
			});
		} else {
			$element.find(".convert-order-link").on("click", function (e) {
				e.preventDefault();
				var id = $parent.find("[id$='_EntityID']").val();
				var url = $(this).data("url");
				var data = {};
				var entityReference = {};
				entityReference.LogicalName = layout.EntityName;
				entityReference.Id = id;
				data.entityReference = entityReference;
				var json = JSON.stringify(data);
				shell.ajaxSafePost({
					type: "POST",
					contentType: "application/json",
					url: url,
					data: json
				}).done(function () {
					createNotificationCookie(layout.ConvertOrderToInvoiceActionLink.SuccessMessage);
					onComplete(layout.ConvertOrderToInvoiceActionLink);
				}).fail(function (jqXhr) {
					displayErrorAlert(getError(jqXhr), $parent);
				});
			});
		}

		return true;
	}

	entityForm.prototype.addCalculateOpportunityActionLinkClickEventHandlers = function() {
		var $this = this;
		var $element = $this._element;
		var layout = $this._layout;

		if (layout == null || layout == '' || layout.CalculateOpportunityActionLink == null || !layout.CalculateOpportunityActionLink.Enabled || !layout.CalculateOpportunityActionLink.URL) {
			var element = $element.find(".calculate-opportunity-link");
			element.addClass("hidden");
			element.hide();
			return false;
		}

		var $parent = $element.closest(".entity-form");

		if ($parent.length < 2) {
			var $grandparent = $element.closest(".crmEntityFormView");
			$parent = $grandparent.find(".entity-form");
		}

		if (layout.CalculateOpportunityActionLink.ShowModal == 1) {
			var $modal = $element.children(".modal-calculate");
			$element.find(".calculate-opportunity-link").on("click", function(e) {
				e.preventDefault();
				var id = $parent.find("[id$='_EntityID']").val();
				var $button = $modal.find("button.primary");
				var url = $(this).data("url");

				$button.off("click");
				$button.on("click", function(ev) {
					ev.preventDefault();
					$(this).attr("disabled", "disabled").prepend("<span class='fa fa-spinner fa-spin' aria-hidden='true'></span>");
					var data = {};
					var entityReference = {};
					entityReference.LogicalName = layout.EntityName;
					entityReference.Id = id;
					data.entityReference = entityReference;
					var json = JSON.stringify(data);
					shell.ajaxSafePost({
						type: "POST",
						contentType: "application/json",
						url: url,
						data: json
					}).done(function() {
						$modal.modal("hide");
						createNotificationCookie(layout.CalculateOpportunityActionLink.SuccessMessage);
						onComplete(layout.CalculateOpportunityActionLink);
					}).fail(function(jqXhr) {
						$modal.modal("hide");
						displayErrorAlert(getError(jqXhr), $parent);
					}).always(function() {
						$button.removeAttr("disabled", "disabled").find(".fa-spin").remove();
					});
				});
				$modal.modal();
			});

		} else {
			$element.find(".calculate-opportunity-link").on("click", function (e) {
				e.preventDefault();
				var id = $parent.find("[id$='_EntityID']").val();
				var url = $(this).data("url");
				var data = {};
				var entityReference = {};
				entityReference.LogicalName = layout.EntityName;
				entityReference.Id = id;
				data.entityReference = entityReference;
				var json = JSON.stringify(data);
				shell.ajaxSafePost({
					type: "POST",
					contentType: "application/json",
					url: url,
					data: json
				}).done(function () {
					createNotificationCookie(layout.CalculateOpportunityActionLink.SuccessMessage);
					onComplete(layout.CalculateOpportunityActionLink);
				}).fail(function (jqXhr) {
					displayErrorAlert(getError(jqXhr), $parent);
				});
			});
		}
		
		return true;
	};

	entityForm.prototype.addResolveCaseActionLinkClickEventHandlers = function () {
		var $this = this;
		var $element = $this._element;
		var layout = $this._layout;
		var $parent = $element.closest(".entity-form");

		if ($parent.length < 1) {
			var $grandparent = $element.closest(".crmEntityFormView");
			$parent = $grandparent.find(".entity-form");
		}

		var statecode = $parent.find("[id$='_EntityState']").val();

		if (layout == null || layout == '' || layout.ResolveCaseActionLink == null || !layout.ResolveCaseActionLink.Enabled || !layout.ResolveCaseActionLink.URL || statecode != "0") {
			var element = $element.find(".resolve-case-link");
			element.addClass("hidden");
			element.hide();
			return false;
		}
		
		var $modal = $element.children(".modal-resolvecase");

		$modal.on('hidden.bs.modal.entityform', function () {
			$modal.find(".resolution-input").val('');
			$modal.find(".resolution-description-input").val('');
			$modal.find(".alert-danger.error").remove();
		});

		$element.find(".resolve-case-link").on("click", function (e) {
			e.preventDefault();
			var id = $parent.find("[id$='_EntityID']").val();
			var $button = $modal.find("button.primary");
			var url = $(this).data("url");

			$button.off("click");
			$button.on("click", function (ev) {
				ev.preventDefault();
				$(this).attr("disabled", "disabled").prepend("<span class='fa fa-spinner fa-spin' aria-hidden='true'></span>");
				var data = {};
				var entityReference = {};
				entityReference.LogicalName = layout.EntityName;
				entityReference.Id = id;
				data.entityReference = entityReference;

				var $resolution = $modal.find(".resolution-input");
				data.resolutionSubject = $resolution.val();
				var $resolutionDescription = $modal.find(".resolution-description-input");
				data.resolutionDescription = $resolutionDescription.val();
				var json = JSON.stringify(data);

				shell.ajaxSafePost({
					type: "POST",
					contentType: "application/json",
					url: url,
					data: json
				}).done(function () {
					$modal.modal("hide");
					createNotificationCookie(layout.ResolveCaseActionLink.SuccessMessage);
					onComplete(layout.ResolveCaseActionLink);
				}).fail(function (jqXhr) {
					$modal.modal("show");
					onFail({ Message: jqXhr.statusText} ,$modal)
				}).always(function () {
					$button.removeAttr("disabled", "disabled").find(".fa-spin").remove();
				});
			});
			$modal.modal();
		});
		return true;
	};

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

			if (typeof error.InnerError !== typeof undefined && typeof error.InnerError.Message !== typeof undefined && error.InnerError.Message !== false && error.InnerError.Message != null) {
				if (typeof error.InnerError.Message === 'number') {
					$error.append("<p><span class='fa fa-exclamation-triangle' aria-hidden='true'></span> " + error.InnerError.Message + " Error</p>");
				} else {
					$error.append("<p><span class='fa fa-exclamation-triangle' aria-hidden='true'></span> " + error.InnerError.Message + "</p>");
				}
			} else if (typeof error.Message !== typeof undefined && error.Message !== false && error.Message != null) {
				if (typeof error.Message === 'number') {
					$error.append("<p><span class='fa fa-exclamation-triangle' aria-hidden='true'></span> " + error.Message + " Error</p>");
				} else {
					$error.append("<p><span class='fa fa-exclamation-triangle' aria-hidden='true'></span> " + error.Message + "</p>");
				}
			}

			$body.prepend($error);
		}
	}

	entityForm.prototype.addReopenCaseActionLinkClickEventHandlers = function () {
		var $this = this;
		var $element = $this._element;
		var layout = $this._layout;
		var $parent = $element.closest(".entity-form");

		if ($parent.length < 1) {
			var $grandparent = $element.closest(".crmEntityFormView");
			$parent = $grandparent.find(".entity-form");
		}

		var statecode = $parent.find("[id$='_EntityState']").val();

		if (layout == null || layout == '' || layout.ReopenCaseActionLink == null || !layout.ReopenCaseActionLink.Enabled || !layout.ReopenCaseActionLink.URL || statecode == "0") {
			var element = $element.find(".reopen-case-link");
			element.addClass("hidden");
			element.hide();
			return false;
		}

		if (layout.ReopenCaseActionLink.ShowModal == 1) {
			var $modal = $element.children(".modal-reopencase");
			$element.find(".reopen-case-link").on("click", function(e) {
				e.preventDefault();
				var id = $parent.find("[id$='_EntityID']").val();
				var $button = $modal.find("button.primary");
				var url = $(this).data("url");
				$button.off("click");
				$button.on("click", function(ev) {
					ev.preventDefault();
					$(this).attr("disabled", "disabled").prepend("<span class='fa fa-spinner fa-spin' aria-hidden='true'></span>");
					var data = {};
					var entityReference = {};
					entityReference.LogicalName = layout.EntityName;
					entityReference.Id = id;
					data.entityReference = entityReference;
					var json = JSON.stringify(data);
					shell.ajaxSafePost({
						type: "POST",
						contentType: "application/json",
						url: url,
						data: json
					}).done(function() {
						$modal.modal("hide");
						createNotificationCookie(layout.ReopenCaseActionLink.SuccessMessage);
						onComplete(layout.ReopenCaseActionLink);
					}).fail(function(jqXhr) {
						$modal.modal("hide");
						displayErrorAlert(getError(jqXhr), $parent);
					}).always(function() {
						$button.removeAttr("disabled", "disabled").find(".fa-spin").remove();
					});
				});
				$modal.modal();
			});
		} else {
			$element.find(".reopen-case-link").on("click", function (e) {
				e.preventDefault();
				var id = $parent.find("[id$='_EntityID']").val();
				var url = $(this).data("url");
				var data = {};
				var entityReference = {};
				entityReference.LogicalName = layout.EntityName;
				entityReference.Id = id;
				data.entityReference = entityReference;
				var json = JSON.stringify(data);
				shell.ajaxSafePost({
					type: "POST",
					contentType: "application/json",
					url: url,
					data: json
				}).done(function () {
					createNotificationCookie(layout.ReopenCaseActionLink.SuccessMessage);
					onComplete(layout.ReopenCaseActionLink);
				}).fail(function (jqXhr) {
					displayErrorAlert(getError(jqXhr), $parent);
				});
			});
		}

		return true;
	};

	entityForm.prototype.addCancelCaseActionLinkClickEventHandlers = function () {
		var $this = this;
		var $element = $this._element;
		var layout = $this._layout;
		var $parent = $element.closest(".entity-form");

		if ($parent.length < 1) {
			var $grandparent = $element.closest(".crmEntityFormView");
			$parent = $grandparent.find(".entity-form");
		}

		var statecode = $parent.find("[id$='_EntityState']").val();

		if (layout == null || layout == '' || layout.CancelCaseActionLink == null || !layout.CancelCaseActionLink.Enabled || !layout.CancelCaseActionLink.URL || statecode != "0") {
			var element = $element.find(".cancel-case-link");
			element.addClass("hidden");
			element.hide();
			return false;
		}

		if (layout.CancelCaseActionLink.ShowModal == 1) {
			var $modal = $element.children(".modal-cancelcase");
			$element.find(".cancel-case-link").on("click", function(e) {
				e.preventDefault();
				var id = $parent.find("[id$='_EntityID']").val();
				var $button = $modal.find("button.primary");
				var url = $(this).data("url");
				$button.off("click");
				$button.on("click", function(ev) {
					ev.preventDefault();
					$(this).attr("disabled", "disabled").prepend("<span class='fa fa-spinner fa-spin' aria-hidden='true'></span>");
					var data = {};
					var entityReference = {};
					entityReference.LogicalName = layout.EntityName;
					entityReference.Id = id;
					data.entityReference = entityReference;
					var json = JSON.stringify(data);
					shell.ajaxSafePost({
						type: "POST",
						contentType: "application/json",
						url: url,
						data: json
					}).done(function() {
						$modal.modal("hide");
						createNotificationCookie(layout.CancelCaseActionLink.SuccessMessage);
						onComplete(layout.CancelCaseActionLink);
					}).fail(function(jqXhr) {
						$modal.modal("hide");
						displayErrorAlert(getError(jqXhr), $parent);
					}).always(function() {
						$button.removeAttr("disabled", "disabled").find(".fa-spin").remove();
					});
				});
				$modal.modal();
			});
			$modal.on('show.bs.modal', function () {
			    $modal.attr("aria-hidden", "false");
			});

			$modal.on('hidden.bs.modal', function () {
			    $modal.attr("aria-hidden", "true");
			});
		} else {
			$element.find(".cancel-case-link").on("click", function (e) {
				e.preventDefault();
				var id = $parent.find("[id$='_EntityID']").val();
				var url = $(this).data("url");
				var data = {};
				var entityReference = {};
				entityReference.LogicalName = layout.EntityName;
				entityReference.Id = id;
				data.entityReference = entityReference;
				var json = JSON.stringify(data);
				shell.ajaxSafePost({
					type: "POST",
					contentType: "application/json",
					url: url,
					data: json
				}).done(function () {
					createNotificationCookie(layout.CancelCaseActionLink.SuccessMessage);
					onComplete(layout.CancelCaseActionLink);
				}).fail(function (jqXhr) {
					displayErrorAlert(getError(jqXhr), $parent);
				});
			});
		}

		return true;
	};

	entityForm.prototype.addActivateActionLinkClickEventHandlers = function () {
		var $this = this;
		var $element = $this._element;
		var layout = $this._layout;
		var $parent = $element.closest(".entity-form");

		if ($parent.length < 1) {
			var $grandparent = $element.closest(".crmEntityFormView");
			$parent = $grandparent.find(".entity-form");
		}

		var statecode = $parent.find("[id$='_EntityState']").val();

		if (layout == null || layout == '' || layout.ActivateActionLink == null || !layout.ActivateActionLink.Enabled || !layout.ActivateActionLink.URL || statecode == "0") {
			var element = $element.find(".activate-link");
			element.addClass("hidden");
			element.hide();
			return false;
		}

		if (layout.ActivateActionLink.ShowModal == 1) {
			var $modal = $element.children(".modal-activate");
			$element.find(".activate-link").on("click", function(e) {
				e.preventDefault();
				var id = $parent.find("[id$='_EntityID']").val();
				var $button = $modal.find("button.primary");
				var url = $(this).data("url");
				$button.off("click");
				$button.on("click", function(ev) {
					ev.preventDefault();
					$(this).attr("disabled", "disabled").prepend("<span class='fa fa-spinner fa-spin' aria-hidden='true'></span>");
					var data = {};
					var entityReference = {};
					entityReference.LogicalName = layout.EntityName;
					entityReference.Id = id;
					data.entityReference = entityReference;
					var json = JSON.stringify(data);
					shell.ajaxSafePost({
						type: "POST",
						contentType: "application/json",
						url: url,
						data: json
					}).done(function() {
						$modal.modal("hide");
						createNotificationCookie(layout.ActivateActionLink.SuccessMessage);
						onComplete(layout.ActivateActionLink);
					}).fail(function(jqXhr) {
						$modal.modal("hide");
						displayErrorAlert(getError(jqXhr), $parent);
					}).always(function() {
						$button.removeAttr("disabled", "disabled").find(".fa-spin").remove();
					});
				});
				$modal.modal();
			});
		} else {
			$element.find(".activate-link").on("click", function (e) {
				e.preventDefault();
				var id = $parent.find("[id$='_EntityID']").val();
				var url = $(this).data("url");
				var data = {};
				var entityReference = {};
				entityReference.LogicalName = layout.EntityName;
				entityReference.Id = id;
				data.entityReference = entityReference;
				var json = JSON.stringify(data);
				shell.ajaxSafePost({
					type: "POST",
					contentType: "application/json",
					url: url,
					data: json
				}).done(function () {
					createNotificationCookie(layout.ActivateActionLink.SuccessMessage);
					onComplete(layout.ActivateActionLink);
				}).fail(function (jqXhr) {
					displayErrorAlert(getError(jqXhr), $parent);
				});
			});
		}

		return true;
	};

	entityForm.prototype.addDeactivateActionLinkClickEventHandlers = function () {
		var $this = this;
		var $element = $this._element;
		var layout = $this._layout;
		var $parent = $element.closest(".entity-form");

		if ($parent.length < 1) {
			var $grandparent = $element.closest(".crmEntityFormView");
			$parent = $grandparent.find(".entity-form");
		}

		var statecode = $parent.find("[id$='_EntityState']").val();

		if (layout == null || layout == '' || layout.DeactivateActionLink == null || !layout.DeactivateActionLink.Enabled || !layout.DeactivateActionLink.URL || statecode == "1") {
			var element = $element.find(".deactivate-link");
			element.addClass("hidden");
			element.hide();
			return false;
		}

		if (layout.DeactivateActionLink.ShowModal == 1) {
			var $modal = $element.children(".modal-deactivate");
			$element.find(".deactivate-link").on("click", function(e) {
				e.preventDefault();
				var id = $parent.find("[id$='_EntityID']").val();
				var $button = $modal.find("button.primary");
				var url = $(this).data("url");
				$button.off("click");
				$button.on("click", function(ev) {
					ev.preventDefault();
					$(this).attr("disabled", "disabled").prepend("<span class='fa fa-spinner fa-spin' aria-hidden='true'></span>");
					var data = {};
					var entityReference = {};
					entityReference.LogicalName = layout.EntityName;
					entityReference.Id = id;
					data.entityReference = entityReference;
					var json = JSON.stringify(data);
					shell.ajaxSafePost({
						type: "POST",
						contentType: "application/json",
						url: url,
						data: json
					}).done(function() {
						$modal.modal("hide");
						createNotificationCookie(layout.DeactivateActionLink.SuccessMessage);
						onComplete(layout.DeactivateActionLink);
					}).fail(function(jqXhr) {
						$modal.modal("hide");
						displayErrorAlert(getError(jqXhr), $parent);
					}).always(function() {
						$button.removeAttr("disabled", "disabled").find(".fa-spin").remove();
					});
				});
				$modal.modal();
			});
		} else {
			$element.find(".deactivate-link").on("click", function (e) {
				e.preventDefault();
				var id = $parent.find("[id$='_EntityID']").val();
				var url = $(this).data("url");
				var data = {};
				var entityReference = {};
				entityReference.LogicalName = layout.EntityName;
				entityReference.Id = id;
				data.entityReference = entityReference;
				var json = JSON.stringify(data);
				shell.ajaxSafePost({
					type: "POST",
					contentType: "application/json",
					url: url,
					data: json
				}).done(function () {
					createNotificationCookie(layout.DeactivateActionLink.SuccessMessage);
					onComplete(layout.DeactivateActionLink);
				}).fail(function (jqXhr) {
					displayErrorAlert(getError(jqXhr), $parent);
				});
			});
		}

		return true;
	};

	entityForm.prototype.addActivateQuoteActionLinkClickEventHandlers = function () {
		var $this = this;
		var $element = $this._element;
		var layout = $this._layout;
		var $parent = $element.closest(".entity-form");

		if ($parent.length < 1) {
			var $grandparent = $element.closest(".crmEntityFormView");
			$parent = $grandparent.find(".entity-form");
		}

		var statecode = $parent.find("[id$='_EntityState']").val();

		if (layout == null || layout == '' || layout.ActivateQuoteActionLink == null || !layout.ActivateQuoteActionLink.Enabled || !layout.ActivateQuoteActionLink.URL || statecode != "0") {
			var element = $element.find(".activate-quote-link");
			element.addClass("hidden");
			element.hide();
			return false;
		}

		if (layout.ActivateQuoteActionLink.ShowModal == 1) {
			var $modal = $element.children(".modal-activate-quote");
			$element.find(".activate-quote-link").on("click", function(e) {
				e.preventDefault();
				var id = $parent.find("[id$='_EntityID']").val();
				var $button = $modal.find("button.primary");
				var url = $(this).data("url");
				$button.off("click");
				$button.on("click", function(ev) {
					ev.preventDefault();
					$(this).attr("disabled", "disabled").prepend("<span class='fa fa-spinner fa-spin' aria-hidden='true'></span>");
					var data = {};
					var entityReference = {};
					entityReference.LogicalName = layout.EntityName;
					entityReference.Id = id;
					data.entityReference = entityReference;
					var json = JSON.stringify(data);
					shell.ajaxSafePost({
						type: "POST",
						contentType: "application/json",
						url: url,
						data: json
					}).done(function() {
						$modal.modal("hide");
						createNotificationCookie(layout.ActivateQuoteActionLink.SuccessMessage);
						onComplete(layout.ActivateQuoteActionLink);
					}).fail(function(jqXhr) {
						$modal.modal("hide");
						displayErrorAlert(getError(jqXhr), $parent);
					}).always(function() {
						$button.removeAttr("disabled", "disabled").find(".fa-spin").remove();
					});
				});
				$modal.modal();
			});
		} else {
			$element.find(".activate-quote-link").on("click", function (e) {
				e.preventDefault();
				var id = $parent.find("[id$='_EntityID']").val();
				var url = $(this).data("url");
				var data = {};
				var entityReference = {};
				entityReference.LogicalName = layout.EntityName;
				entityReference.Id = id;
				data.entityReference = entityReference;
				var json = JSON.stringify(data);
				shell.ajaxSafePost({
					type: "POST",
					contentType: "application/json",
					url: url,
					data: json
				}).done(function () {
					createNotificationCookie(layout.ActivateQuoteActionLink.SuccessMessage);
					onComplete(layout.ActivateQuoteActionLink);
				}).fail(function (jqXhr) {
					displayErrorAlert(getError(jqXhr), $parent);
				});
			});
		}

		return true;
	};

	entityForm.prototype.addSetOpportunityOnHoldActionLinkClickEventHandlers = function () {
		var $this = this;
		var $element = $this._element;
		var layout = $this._layout;
		var $parent = $element.closest(".entity-form");

		if ($parent.length < 1) {
			var $grandparent = $element.closest(".crmEntityFormView");
			$parent = $grandparent.find(".entity-form");
		}

		var statecode = $parent.find("[id$='_EntityState']").val();
		var statuscode = $parent.find("[id$='_EntityStatus']").val();

		if (layout == null || layout == '' || layout.SetOpportunityOnHoldActionLink == null || !layout.SetOpportunityOnHoldActionLink.Enabled || !layout.SetOpportunityOnHoldActionLink.URL || statecode != "0" || statuscode == "2") {
			var element = $element.find(".set-opportunity-on-hold-link");
			element.addClass("hidden");
			element.hide();
			return false;
		}

		if (layout.SetOpportunityOnHoldActionLink.ShowModal == 1) {
			var $modal = $element.children(".modal-set-opportunity-on-hold");
			$element.find(".set-opportunity-on-hold-link").on("click", function(e) {
				e.preventDefault();
				var id = $parent.find("[id$='_EntityID']").val();
				var $button = $modal.find("button.primary");
				var url = $(this).data("url");
					$button.off("click");
				$button.on("click", function(ev) {
					ev.preventDefault();
					$(this).attr("disabled", "disabled").prepend("<span class='fa fa-spinner fa-spin' aria-hidden='true'></span>");
					var data = {};
					var entityReference = {};
					entityReference.LogicalName = layout.EntityName;
					entityReference.Id = id;
					data.entityReference = entityReference;
					var json = JSON.stringify(data);
					shell.ajaxSafePost({
						type: "POST",
						contentType: "application/json",
						url: url,
						data: json
					}).done(function() {
						$modal.modal("hide");
						createNotificationCookie(layout.SetOpportunityOnHoldActionLink.SuccessMessage);
						onComplete(layout.SetOpportunityOnHoldActionLink);
					}).fail(function(jqXhr) {
						$modal.modal("hide");
						displayErrorAlert(getError(jqXhr), $parent);
					}).always(function() {
						$button.removeAttr("disabled", "disabled").find(".fa-spin").remove();
					});
				});
				$modal.modal();
			});
		} else {
			$element.find(".set-opportunity-on-hold-link").on("click", function (e) {
				e.preventDefault();
				var id = $parent.find("[id$='_EntityID']").val();
				var url = $(this).data("url");
				var data = {};
				var entityReference = {};
				entityReference.LogicalName = layout.EntityName;
				entityReference.Id = id;
				data.entityReference = entityReference;
				var json = JSON.stringify(data);
				shell.ajaxSafePost({
					type: "POST",
					contentType: "application/json",
					url: url,
					data: json
				}).done(function () {
					createNotificationCookie(layout.SetOpportunityOnHoldActionLink.SuccessMessage);
					onComplete(layout.SetOpportunityOnHoldActionLink);
				}).fail(function (jqXhr) {
					displayErrorAlert(getError(jqXhr), $parent);
				}).always(function () { });
			});
		}

		return true;
	};

	entityForm.prototype.addReopenOpportunityActionLinkClickEventHandlers = function () {
		var $this = this;
		var $element = $this._element;
		var layout = $this._layout;
		var $parent = $element.closest(".entity-form");

		if ($parent.length < 1) {
			var $grandparent = $element.closest(".crmEntityFormView");
			$parent = $grandparent.find(".entity-form");
		}

		var statecode = $parent.find("[id$='_EntityState']").val();
		var statuscode = $parent.find("[id$='_EntityStatus']").val();

		if (layout == null || layout == '' || layout.ReopenOpportunityActionLink == null || !layout.ReopenOpportunityActionLink.Enabled || !layout.ReopenOpportunityActionLink.URL || statecode == "0") {
			var element = $element.find(".reopen-opportunity-link");
			element.addClass("hidden");
			element.hide();
			return false;
		}

		if (layout.ReopenOpportunityActionLink.ShowModal == 1) {
			var $modal = $element.children(".modal-reopen-opportunity");
			$element.find(".reopen-opportunity-link").on("click", function (e) {
				e.preventDefault();
				var id = $parent.find("[id$='_EntityID']").val();
				var $button = $modal.find("button.primary");
				var url = $(this).data("url");
				$button.off("click");
				$button.on("click", function (ev) {
					ev.preventDefault();
					$(this).attr("disabled", "disabled").prepend("<span class='fa fa-spinner fa-spin' aria-hidden='true'></span>");
					var data = {};
					var entityReference = {};
					entityReference.LogicalName = layout.EntityName;
					entityReference.Id = id;
					data.entityReference = entityReference;
					var json = JSON.stringify(data);
					shell.ajaxSafePost({
						type: "POST",
						contentType: "application/json",
						url: url,
						data: json
					}).done(function () {
						$modal.modal("hide");
						createNotificationCookie(layout.ReopenOpportunityActionLink.SuccessMessage);
						onComplete(layout.ReopenOpportunityActionLink);
					}).fail(function (jqXhr) {
						$modal.modal("hide");
						displayErrorAlert(getError(jqXhr), $parent);
					}).always(function () {
						$button.removeAttr("disabled", "disabled").find(".fa-spin").remove();
					});
				});
				$modal.modal();
			});
		} else {
			$element.find(".reopen-opportunity-link").on("click", function (e) {
				e.preventDefault();
				var id = $parent.find("[id$='_EntityID']").val();
				var url = $(this).data("url");
				var data = {};
				var entityReference = {};
				entityReference.LogicalName = layout.EntityName;
				entityReference.Id = id;
				data.entityReference = entityReference;
				var json = JSON.stringify(data);
				shell.ajaxSafePost({
					type: "POST",
					contentType: "application/json",
					url: url,
					data: json
				}).done(function () {
					createNotificationCookie(layout.ReopenOpportunityActionLink.SuccessMessage);
					onComplete(layout.ReopenOpportunityActionLink);
				}).fail(function (jqXhr) {
					displayErrorAlert(getError(jqXhr), $parent);
				}).always(function () { });
			});
		}

		return true;
	};

	entityForm.prototype.addWinOpportunityActionLinkClickEventHandlers = function () {
		var $this = this;
		var $element = $this._element;
		var layout = $this._layout;
		var $parent = $element.closest(".entity-form");

		if ($parent.length < 1) {
			var $grandparent = $element.closest(".crmEntityFormView");
			$parent = $grandparent.find(".entity-form");
		}

		var statecode = $parent.find("[id$='_EntityState']").val();

		if (layout == null || layout == '' || layout.WinOpportunityActionLink == null || !layout.WinOpportunityActionLink.Enabled || !layout.WinOpportunityActionLink.URL || statecode != "0") {
			var element = $element.find(".win-opportunity-link");
			element.addClass("hidden");
			element.hide();
			return false;
		}

		if (layout.WinOpportunityActionLink.ShowModal == 1) {
			var $modal = $element.children(".modal-win-opportunity");
			$element.find(".win-opportunity-link").on("click", function(e) {
			    e.preventDefault();
			    $modal.attr("aria-hidden", "false");
			    var ariaLabelText = $modal.attr("aria-label");
			    if (ariaLabelText) {
			        $modal.attr("aria-label", removeHtmlTags(ariaLabelText));
			    }
				var id = $parent.find("[id$='_EntityID']").val();
				var $button = $modal.find("button.primary");
				var url = $(this).data("url");
				$button.off("click");
				$button.on("click", function(ev) {
				    ev.preventDefault();
				    $modal.attr("aria-hidden", "true");
					$(this).attr("disabled", "disabled").prepend("<span class='fa fa-spinner fa-spin' aria-hidden='true'></span>");
					var data = {};
					var entityReference = {};
					entityReference.LogicalName = layout.EntityName;
					entityReference.Id = id;
					data.entityReference = entityReference;
					var json = JSON.stringify(data);
					shell.ajaxSafePost({
						type: "POST",
						contentType: "application/json",
						url: url,
						data: json
					}).done(function() {
						$modal.modal("hide");
						createNotificationCookie(layout.WinOpportunityActionLink.SuccessMessage);
						onComplete(layout.WinOpportunityActionLink);
					}).fail(function(jqXhr) {
						$modal.modal("hide");
						displayErrorAlert(getError(jqXhr), $parent);
					}).always(function() {
						$button.removeAttr("disabled", "disabled").find(".fa-spin").remove();
					});
				});
				$modal.on('hide.bs.modal', function (e) {
				    $(this).attr('aria-hidden', 'true');
				});
				$modal.modal();
			});
		} else {
			$element.find(".win-opportunity-link").on("click", function (e) {
				e.preventDefault();
				var id = $parent.find("[id$='_EntityID']").val();
				var url = $(this).data("url");
				var data = {};
				var entityReference = {};
				entityReference.LogicalName = layout.EntityName;
				entityReference.Id = id;
				data.entityReference = entityReference;
				var json = JSON.stringify(data);
				shell.ajaxSafePost({
					type: "POST",
					contentType: "application/json",
					url: url,
					data: json
				}).done(function () {
					createNotificationCookie(layout.WinOpportunityActionLink.SuccessMessage);
					onComplete(layout.WinOpportunityActionLink);
				}).fail(function (jqXhr) {
					displayErrorAlert(getError(jqXhr), $parent);
				}).always(function () { });
			});
		}

		return true;
	};

	entityForm.prototype.addLoseOpportunityActionLinkClickEventHandlers = function () {
		var $this = this;
		var $element = $this._element;
		var layout = $this._layout;
		var $parent = $element.closest(".entity-form");

		if ($parent.length < 1) {
			var $grandparent = $element.closest(".crmEntityFormView");
			$parent = $grandparent.find(".entity-form");
		}

		var statecode = $parent.find("[id$='_EntityState']").val();

		if (layout == null || layout == '' || layout.LoseOpportunityActionLink == null || !layout.LoseOpportunityActionLink.Enabled || !layout.LoseOpportunityActionLink.URL || statecode != "0") {
			var element = $element.find(".lose-opportunity-link");
			element.addClass("hidden");
			element.hide();
			return false;
		}
		if (layout.LoseOpportunityActionLink.ShowModal == 1) {
			var $modal = $element.children(".modal-lose-opportunity");
			$element.find(".lose-opportunity-link").on("click", function(e) {
			    e.preventDefault();
			    $modal.attr("aria-hidden", "false");
			    var ariaLabelText = $modal.attr("aria-label");
			    if (ariaLabelText) {
			        $modal.attr("aria-label", removeHtmlTags(ariaLabelText));
			    }
				var id = $parent.find("[id$='_EntityID']").val();
				var $button = $modal.find("button.primary");
				var url = $(this).data("url");
				$button.off("click");
				$button.on("click", function(ev) {
				    ev.preventDefault();
				    $modal.attr("aria-hidden", "true");
					$(this).attr("disabled", "disabled").prepend("<span class='fa fa-spinner fa-spin' aria-hidden='true'></span>");
					var data = {};
					var entityReference = {};
					entityReference.LogicalName = layout.EntityName;
					entityReference.Id = id;
					data.entityReference = entityReference;
					var json = JSON.stringify(data);
					shell.ajaxSafePost({
						type: "POST",
						contentType: "application/json",
						url: url,
						data: json
					}).done(function() {
						$modal.modal("hide");
						createNotificationCookie(layout.LoseOpportunityActionLink.SuccessMessage);
						onComplete(layout.LoseOpportunityActionLink);
					}).fail(function(jqXhr) {
						$modal.modal("hide");
						displayErrorAlert(getError(jqXhr), $parent);
					}).always(function() {
						$button.removeAttr("disabled", "disabled").find(".fa-spin").remove();
					});
				});
				$modal.on('hide.bs.modal', function (e) {
				    $(this).attr('aria-hidden', 'true');
				});
				$modal.modal();
			});
		} else {
			$element.find(".lose-opportunity-link").on("click", function (e) {
				e.preventDefault();
				var id = $parent.find("[id$='_EntityID']").val();
				var url = $(this).data("url");
				var data = {};
				var entityReference = {};
				entityReference.LogicalName = layout.EntityName;
				entityReference.Id = id;
				data.entityReference = entityReference;
				var json = JSON.stringify(data);
				shell.ajaxSafePost({
					type: "POST",
					contentType: "application/json",
					url: url,
					data: json
				}).done(function () {
					createNotificationCookie(layout.LoseOpportunityActionLink.SuccessMessage);
					onComplete(layout.LoseOpportunityActionLink);
				}).fail(function (jqXhr) {
					displayErrorAlert(getError(jqXhr), $parent);
				}).always(function () { });
			});
		}

		return true;
	};

	entityForm.prototype.addGenerateQuoteFromOpportunityActionLinkClickEventHandlers = function () {
		var $this = this;
		var $element = $this._element;
		var layout = $this._layout;
		var $parent = $element.closest(".entity-form");

		if ($parent.length < 1) {
			var $grandparent = $element.closest(".crmEntityFormView");
			$parent = $grandparent.find(".entity-form");
		}

		var statecode = $parent.find("[id$='_EntityState']").val();

		if (layout == null || layout == '' || layout.GenerateQuoteFromOpportunityActionLink == null || !layout.GenerateQuoteFromOpportunityActionLink.Enabled || !layout.GenerateQuoteFromOpportunityActionLink.URL || statecode != "0") {
			var element = $element.find(".generate-quote-from-opportunity-link");
			element.addClass("hidden");
			element.hide();
			return false;
		}

		if (layout.GenerateQuoteFromOpportunityActionLink.ShowModal == 1) {
			var $modal = $element.children(".modal-generate-quote-from-opportunity");
			$element.find(".generate-quote-from-opportunity-link").on("click", function(e) {
				e.preventDefault();
				var id = $parent.find("[id$='_EntityID']").val();
				var $button = $modal.find("button.primary");
				var url = $(this).data("url");
				$button.off("click");
				$button.on("click", function(ev) {
					ev.preventDefault();
					$(this).attr("disabled", "disabled").prepend("<span class='fa fa-spinner fa-spin' aria-hidden='true'></span>");
					var data = {};
					var entityReference = {};
					entityReference.LogicalName = layout.EntityName;
					entityReference.Id = id;
					data.entityReference = entityReference;
					var json = JSON.stringify(data);
					shell.ajaxSafePost({
						type: "POST",
						contentType: "application/json",
						url: url,
						data: json
					}).done(function() {
						$modal.modal("hide");
						createNotificationCookie(layout.GenerateQuoteFromOpportunityActionLink.SuccessMessage);
						onComplete(layout.GenerateQuoteFromOpportunityActionLink);
					}).fail(function(jqXhr) {
						$modal.modal("hide");
						displayErrorAlert(getError(jqXhr), $parent);
					}).always(function() {
						$button.removeAttr("disabled", "disabled").find(".fa-spin").remove();
					});
				});
				$modal.modal();
			});
		} else {
			$element.find(".generate-quote-from-opportunity-link").on("click", function (e) {
				e.preventDefault();
				var id = $parent.find("[id$='_EntityID']").val();
				var url = $(this).data("url");
				var data = {};
				var entityReference = {};
				entityReference.LogicalName = layout.EntityName;
				entityReference.Id = id;
				data.entityReference = entityReference;
				var json = JSON.stringify(data);
				shell.ajaxSafePost({
					type: "POST",
					contentType: "application/json",
					url: url,
					data: json
				}).done(function () {
					createNotificationCookie(layout.GenerateQuoteFromOpportunityActionLink.SuccessMessage);
					onComplete(layout.GenerateQuoteFromOpportunityActionLink);
				}).fail(function (jqXhr) {
					displayErrorAlert(getError(jqXhr), $parent);
				});
			});
		}

		return true;
	};

	entityForm.prototype.addUpdatePipelinePhaseActionLinkClickEventHandlers = function () {
		var $this = this;
		var $element = $this._element;
		var layout = $this._layout;
		var $parent = $element.closest(".entity-form");

		if ($parent.length < 1) {
			var $grandparent = $element.closest(".crmEntityFormView");
			$parent = $grandparent.find(".entity-form");
		}

		var statecode = $parent.find("[id$='_EntityState']").val();

		if (layout == null || layout == '' || layout.UpdatePipelinePhaseActionLink == null || !layout.UpdatePipelinePhaseActionLink.Enabled || !layout.UpdatePipelinePhaseActionLink.URL || statecode != "0") {
			var element = $element.find(".update-pipeline-phase-link");
			element.addClass("hidden");
			element.hide();
			return false;
		}

		var $modal = $element.children(".modal-updatepipelinephase");
		
		$element.find(".update-pipeline-phase-link").on("click", function (e) {
			e.preventDefault();
			var id = $parent.find("[id$='_EntityID']").val();
			var $button = $modal.find("button.primary");
			var url = $(this).data("url");
			$button.off("click");
			$button.on("click", function (ev) {
				ev.preventDefault();
				$(this).attr("disabled", "disabled").prepend("<span class='fa fa-spinner fa-spin' aria-hidden='true'></span>");
				var data = {};
				var entityReference = {};
				entityReference.LogicalName = layout.EntityName;
				entityReference.Id = id;
				data.entityReference = entityReference;

				var $phase = $modal.find(".pipelinephase-input");
				data.salesStage = $phase.val();

				var $phaseName = $phase.find("option:selected");
				data.stepName = $phaseName.text();

				var $resolutionDescription = $modal.find(".resolution-description-input");
				data.description = $resolutionDescription.val();

				var json = JSON.stringify(data);
				
				shell.ajaxSafePost({
					type: "POST",
					contentType: "application/json",
					url: url,
					data: json
				}).done(function () {
					$modal.modal("hide");
					createNotificationCookie(layout.UpdatePipelinePhaseActionLink.SuccessMessage);
					onComplete(layout.UpdatePipelinePhaseActionLink);
				}).fail(function (jqXhr) {
					$modal.modal("hide");
					displayErrorAlert(getError(jqXhr), $parent);
				}).always(function () {
					$button.removeAttr("disabled", "disabled").find(".fa-spin").remove();
				});
			});
			$modal.modal();
		});
		return true;
	};

	entityForm.prototype.addCreateRelatedRecordClickEventHandlers = function() {
		var $this = this;
		var $element = $this._element;
		var layout = $this._layout;
		var $parent = $element.closest(".entity-form");

		if ($parent.length < 1) {
			var $grandparent = $element.closest(".crmEntityFormView");
			$parent = $grandparent.find(".entity-form");
		}

		if (!layout || !layout.CreateRelatedRecordActionLink) {
			return false;
		}

		window.addEventListener('message', function (e) {
			if (e.data == 'Success') {
				$element.children(".modal-form").modal('hide');
				window.location.assign(location.href);
			}
		});

		var formActionLinks = getFormActionLinks.call($this);

		if (formActionLinks && formActionLinks.length) {
			$.each(formActionLinks, function (index, value) {
				if (value.Type === 29) {
					if (value.Enabled) {
						if (layout.CreateRelatedRecordActionLink.ShowModal === 1) {
							var $aCreate = $element.find(".create-related-record-link[data-filtercriteriaid = '"+ value.FilterCriteriaId+"']");
							$aCreate.on("click", function (e) {
								e.preventDefault();
								var $modal = $element.children(".modal-form-createrecord[data-filtercriteriaid = '" + value.FilterCriteriaId + "']");
								$modal.attr("aria-hidden", "false");
								var ariaLabelText = $modal.attr("aria-label");
								if (ariaLabelText) {
								    $modal.attr("aria-label", removeHtmlTags(ariaLabelText));
								}
								var id = $parent.find("[id$='_EntityID']").val();
								var iframe = $modal.find("iframe");
								var page = iframe.data("page");
								var relationship = value.Relationship;
								var entityformid = value.EntityForm.Id;

								var uri = page + "?id=" + id + "&entityformid=" + entityformid + "&languagecode=" + layout.LanguageCode;

								var src = getUrlForCreateRelatedRecordAction(uri, layout.EntityName, id, relationship);

								iframe.attr("src", src);
								iframe.on("load", function () {
									$modal.find(".form-loading").hide();
									$modal.find("iframe").contents().find("#EntityFormControl").show();
								});
								$modal.find(".form-loading").show();
								$modal.find("iframe").contents().find("#EntityFormControl").hide();
								$modal.on('hide.bs.modal', function (e) {
								    $(this).attr('aria-hidden', 'true');
								});
								$modal.modal();
							});
						} else {
							$element.find(".create-related-record-link[data-filtercriteriaid = '" + value.FilterCriteriaId + "']").on("click", function (e) {
								e.preventDefault();
								var id = $parent.find("[id$='_EntityID']").val();
								var url = $(this).data("url");
								var data = {};
								var entityReference = {};
								entityReference.LogicalName = layout.EntityName;
								entityReference.Id = id;
								data.entityReference = entityReference;
								var json = JSON.stringify(data);
								shell.ajaxSafePost({
									type: "POST",
									contentType: "application/json",
									url: url,
									data: json
								}).done(function () {
									createNotificationCookie(layout.CreateRelatedRecordActionLink.SuccessMessage);
									onComplete(layout.CreateRelatedRecordActionLink);
								}).fail(function (jqXhr) {
									displayErrorAlert(getError(jqXhr), $parent);
								}).always(function () { });
							});
						}
					} else {
						var $elemToHide = $element.find(".create-related-record-link[data-filtercriteriaid = '" + value.FilterCriteriaId + "']");
						$elemToHide.addClass("hidden");
						$elemToHide.hide();
					}
				}
			});
		} else {
			return false;
		}

		return true;
	};

	function createNotificationCookie(notification) {
		// Store an alert message in a cookie so when a redirect occurrs, the target page will have the notification displayed.
		$.cookie("adx-notification", notification, { expires: 1 });
	}

	function displayErrorAlert(error, $element) {
		if (typeof error !== typeof undefined && error !== false && error != null) {
			console.log(error);
			var message;
			if (typeof error.InnerError !== typeof undefined && error.InnerError !== false && error.InnerError != null) {
				message = error.InnerError.Message;
			} else {
				message = error.Message;
			}
			var $container = $(".notifications");
			if ($container.length == 0) {
				var $pageheader = $(".page-heading");
				if ($pageheader.length == 0) {
					$container = $("<div class='notifications'></div>").prependTo($("#content-container"));
				} else {
					$container = $("<div class='notifications'></div>").appendTo($pageheader);
				}
			}
			$container.find(".notification").slideUp().remove();
			var $status = $element.find(".navbar-collapse").find(".action-status");
			if ($status.length == 0) $status = $element.parent().find(".navbar-collapse").find(".action-status");
			$status.html("<span class='fa fa-fw fa-exclamation-circle text-danger' aria-hidden='true'></span>");
			var $alert = $("<div class='notification alert alert-danger error alert-dismissible' role='alert'><button type='button' class='close' data-dismiss='alert' aria-label='Close'><span aria-hidden='true'>&times;</span></button><span class='fa fa-exclamation-triangle' aria-hidden='true'></span> " + message + "</div>")
				.on('closed.bs.alert', function () {
					$status.html("<span class='fa fa-fw' aria-hidden='true'></span>");
					if ($container.find(".notification").length == 0) $container.hide();
				}).prependTo($container);
			$container.show();
			$('html, body').animate({
				scrollTop: ($alert.offset().top-20)
			}, 200);
		}
	}

	function displaySuccessAlert(success, $element, autohide) {
		// if a redirect follows, use createNotificationCookie instead, the target page will have the notification message displayed
		var $container = $(".notifications");
		if ($container.length == 0) {
			var $pageheader = $(".page-heading");
			if ($pageheader.length == 0) {
				$container = $("<div class='notifications'></div>").prependTo($("#content-container"));
			} else {
				$container = $("<div class='notifications'></div>").appendTo($pageheader);
			}
		}
		$container.find(".notification").slideUp().remove();
		if (typeof success !== typeof undefined && success !== false && success != null && success != '') {
			var $status = $element.find(".navbar-collapse").find(".action-status");
			if ($status.length == 0) $status = $element.parent().find(".navbar-collapse").find(".action-status");
			$status.html("<span class='fa fa-fw fa-check-circle text-success' aria-hidden='true'></span>");
			var $alert = $("<div class='notification alert alert-success success alert-dismissible' role='alert'><button type='button' class='close' data-dismiss='alert' aria-label='Close'><span aria-hidden='true'>&times;</span></button>" + success + "</div>")
				.on('closed.bs.alert', function () {
					$status.html("<span class='fa fa-fw' aria-hidden='true'></span>");
					if ($container.find(".notification").length == 0) $container.hide();
				}).prependTo($container);
			$container.show();
			$('html, body').animate({
				scrollTop: ($alert.offset().top - 20)
			}, 200);
			if (autohide) {
				setTimeout(function() {
					$alert.slideUp(100).remove();
					if ($container.find(".notification").length == 0) $container.hide();
				}, 5000);
			}
		}
	}
	
	function onComplete(action, reloadParent) {

		if (typeof action == typeof undefined || action == null) {
			reload(reloadParent);
			return;
		}

		var onCompleteRefresh = 0;
		var onCompleteRedirectToWebPage = 1;
		var onCompleteRedirectToUrl = 2;
		var onCompleteRedirect = typeof action.OnComplete != typeof undefined && action.OnComplete != null && action.OnComplete != onCompleteRefresh;

		if (onCompleteRedirect) {
			var redirectUrl = getRedirectUrl(action);

			if (redirectUrl != null && redirectUrl != '') {
				$.cookie("adx-notification", null);
				redirect(redirectUrl);
			} else {
				reload(reloadParent);
			}
		} else {
			reload(reloadParent);
		}
	}

	function redirect(redirectUrl) {
		if (typeof redirectUrl == 'undefined' || redirectUrl == null || redirectUrl == '') return;
		$.blockUI({
			css: {
				border: 'none',
				backgroundColor: 'transparent',
				opacity: .5,
				color: '#fff'
			},
			message: "<span class='fa fa-2x fa-spinner fa-pulse' aria-hidden='true'></span>"
		});
		if (parent) {
			parent.location.replace(redirectUrl);
		} else {
			window.location.replace(redirectUrl);
		} 
	}

	function reload(reloadParent) {
		$.blockUI({
			css: {
				border: 'none',
				backgroundColor: 'transparent',
				opacity: .5,
				color: '#fff'
			},
			message: "<span class='fa fa-2x fa-spinner fa-pulse' aria-hidden='true'></span>"
		});
		if (reloadParent && parent) {
		    //The following check is to verify whether URL contains # in the URL .
		    //As presense of '#' or '#someKeyword' , parent.location.href will fail to reload the page.
		    if (parent.location.href.indexOf("#") != -1) {
		        parent.location.reload(true);
		    } else {
		        parent.location = parent.location.href;
		    }
		} else {
		    //The following check is to verify whether URL contains # in the URL .
		    //As presense of '#' or '#someKeyword' , window.location.href will fail to reload the page.
		    if (location.href.indexOf("#") != -1) {
		        window.location.reload(true);
		    } else {
		        window.location = window.location.href;
		    }
		}
	}

	function getRedirectUrl(action) {
		if (!action || !action.RedirectUrl) return null;
		if (action.RedirectUrl.Host == window.location.hostname) {
			return URI(action.RedirectUrl.PathWithQueryString);
		} else {
			return URI(action.RedirectUrl.Uri);
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
	function removeHtmlTags(html) {
	    return html.replace(/<(?:.|\n)*?>/gm, '');
	}
}(jQuery));
