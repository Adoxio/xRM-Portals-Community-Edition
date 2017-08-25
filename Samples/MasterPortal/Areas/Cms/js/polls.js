/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

(function(adx, $) {
	"use strict";

	adx.Poll = (function() {
		var Poll = function(element) {
			this._element = $(element);
			this._content = this._element.find(".poll-content").length ? this._element.find(".poll-content") : this._element;
			this._url = this._element.data("url");
			this._submitUrl = this._element.data("submit-url");
		};

		Poll.prototype.init = function() {
			var $this = this;
			retrieveAsync.call(this).then(function() {
				process.call($this);
				handlers.call($this);
				$this._element.trigger({ type: "poll_ready", ad: $this });
			}, function(e) {
				fail.call($this, e);
			});
		};

		Poll.prototype.viewPoll = function() {
			toggleView.call(this, true);
		};

		Poll.prototype.viewResults = function() {
			toggleView.call(this, false);
			if (this._submitted) {
				this._element.find(".poll-return").remove();
			}
		};

		var fail = function(e) {
			if (console && console.error) {
				console.error({ error: e, poll: this });
			}
		}

		var retrieveAsync = function() {
			var $this = this;
			var d = $.Deferred();

			if (this._url) {
				$.ajax({
					url: this._url,
					type: 'GET'
				}).then(function(html) {
					$this.html = $(html.trim());
					d.resolve();
				}, d.fail);
			} else {
				this.html = $(this._content.html().trim());
				d.resolve();
			}

			return d.promise();
		};

		var process = function() {
			render.call(this);

			this._id = this._element.find(".poll-questionpanel").data("id");
			this._name = this._element.find(".poll-questionpanel").data("name");
			this._submitted = !this._element.find(".poll-questionpanel").length;

			activateView.call(this);
		};

		var render = function() {
			this._content.html(this.html).show();
		}

		var activateView = function() {
			if ((this._submitted)) {
				this.viewResults();
			} else {
				this.viewPoll();
			}
		};

		var toggleView = function(question) {
			this._element.find(".poll-questionpanel").toggle(question);
			this._element.find(".poll-resultspanel").toggle(!question);
		}

		var submitPoll = function() {
			var $this = this;
			var checked = this._element.find("input[id^='poll_option_']:checked").val();
			if (checked && this._submitUrl) {
			    var jqXhr = shell.ajaxSafePost({
					url: this._submitUrl,
					async: false,
					type: "POST",
					data: JSON.stringify({
						pollId: this._id,
						optionId: checked
					}),
					contentType: "application/json; charset=utf-8"
				});

				jqXhr.then(function(html) {
					$this.html = $(html.trim());
					process.call($this);
				});

				jqXhr.fail(function() {
					console.log({ m: "Post Failed", d: arguments });
				});
			}
		};

		var handlers = function() {
			var $this = this;
			this._element.on("click.poll", ".poll-submit", function() {
				console.log("submit");
				submitPoll.call($this);
			});

			this._element.on("click.poll", ".poll-viewresults", function() {
				console.log("view results");
				$this.viewResults();
			});

			this._element.on("click.poll", ".poll-return", function() {
				console.log("return to poll");
				$this.viewPoll();
			});
		}

		return Poll;
	}());

	adx.PollPlacement = (function() {
		var PollPlacement = function(element) {
			this._element = $(element).hide();
			this._url = this._element.data("url");

			this._element.data("pollplacement", this);
		};

		PollPlacement.prototype.init = function() {
			var $this = this;
			retrieveAsync.call(this).then(function() {
				render.call($this);
				polls.call($this);
				$this._element.trigger({ type: "pollplacement_ready", pollplacement: $this });
			}, function(e) {
				fail.call($this, e);
			});
		};

		var fail = function(e) {
			if (console && console.error) {
				console.error({ error: e, ad: this });
			}
		}

		var retrieveAsync = function() {
			var $this = this;
			var d = $.Deferred();

			if (this._url) {
				$.ajax({
					url: this._url
				}).then(function(html) {
					$this.html = $(html.trim());
					d.resolve();
				}, d.fail);
			} else {
				this.html = $(this._element.html().trim());
				d.resolve();
			}

			return d.promise();
		};

		var render = function() {
			this._element.html(this.html).show();
		}

		var polls = function() {
			initPolls(this._element.find(".poll"));
		};

		return PollPlacement;
	}());

	function initPolls(elements) {
		elements.each(function() {
			var poll = new adx.Poll(this);
			poll.init();
		});
	}

	function initPollPlacements(elements) {
		elements.each(function() {
			var placement = new adx.PollPlacement(this);
			placement.init();
		});
	}

	$(document).ready(function() {
		initPolls($(".poll"));
		initPollPlacements($(".pollplacement"));
	});
}(window.adx || (window.adx = {}), window.jQuery));
