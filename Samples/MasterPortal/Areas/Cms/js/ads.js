/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

(function(adx, $) {
	"use strict";

	adx.Ad = (function() {
		var Ad = function(element) {
			this._element = $(element);
			this.url = this._element.data("url");

			this._element.data("ad", this);
		};

		Ad.prototype.init = function() {
			var $this = this;

			this._element.hide();
			retrieveAsync.call(this).then(function() {
				render.call($this);
				$this._element.trigger({ type: "adplacement_ready", adplacement: $this });
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

			if (this.url) {
				$.ajax({
					url: this.url,
					type: 'GET'
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

			this._element.trigger({ type: "ad_ready", ad: this });
		}

		return Ad;
	}());

	adx.AdPlacement = (function() {
		var AdPlacement = function(element) {
			this._element = $(element);
			this.url = this._element.data("url");

			this._element.data("adplacement", this);
		};

		AdPlacement.prototype.init = function() {
			var $this = this;

			$this._element.hide();
			retrieveAsync.call(this).then(function() {
				render.call($this);
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

			if (this.url) {
				$.ajax({
					url: this.url,
					type: 'GET',
					async: true
				}).then(function(html) {
					$this.html = html;
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

		return AdPlacement;
	}());

	$(document).ready(function() {
		$(".ad").each(function() {
			var ad = new adx.Ad(this);
			ad.init();
		});

		$(".adplacement").each(function() {
			$(this).on("adplacement_ready", function(e, args) {
				console.log(e);
				console.log(args);
			});

			var placement = new adx.AdPlacement(this);
			placement.init();
		});
	});
}(window.adx || (window.adx = {}), window.jQuery));
