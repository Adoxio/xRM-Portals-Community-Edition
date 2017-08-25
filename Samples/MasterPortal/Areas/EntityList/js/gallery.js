/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/



(function($) {

  if (!$) {
    return;
  }

  $(function() {
    if (navigator.registerProtocolHandler) {
      $('.package-installer').show();
    } else {
      $('.package-installer').hide();
    }

    $('.zeroclipboard').each(function () {
      var $this = $(this);
      var client = new ZeroClipboard(this);

      client.on('load', function (client, args) {
        $this.parent().show().parent().addClass('input-group');
      });
    });
  });

}(window.jQuery));

(function($, moment, Handlebars, ZeroClipboard) {

  if (!$ || !moment || !Handlebars) {
    return;
  }

  Handlebars.registerHelper('ifeq', function(a, b, options) {
    if (a === b) {
      return options.fn(this);
    } else {
      return options.inverse(this);
    }
  });

  Handlebars.registerHelper('iffeatured', function (a, options) {
    if (a === 'Featured') {
      return options.fn(this);
    } else {
      return options.inverse(this);
    }
  });

  function take(n, items, options) {
    return _.chain(items || [])
     .first(n)
     .reduce(function (memo, e) { return memo + options.fn(e); }, '')
     .value();
  }

  Handlebars.registerHelper('take1', function (items, options) {
    return take(1, items, options);
  });

  Handlebars.registerHelper('take2', function (items, options) {
    return take(2, items, options);
  });

  Handlebars.registerHelper('take3', function (items, options) {
    return take(3, items, options);
  });

  Handlebars.registerHelper('take4', function (items, options) {
    return take(4, items, options);
  });

  function buildUrl(baseUrl, data) {
    var separator, key, url;
    url = baseUrl;
    separator = (baseUrl.indexOf('?') < 0) ? '?' : '&';
    for (key in data) {
      var value = data[key];
      if (value !== null) {
        url += separator + key + '=' + encodeURIComponent(value);
        separator = '&';
      }
    }
    return url;
  }

  function GalleryView(element) {
    this._element = $(element);
    this._url = this._element.data('gallery-url');
    this._galleryTemplate = Handlebars.compile($('#gallery-template').html());
    this._errorTemplate = Handlebars.compile($('#gallery-error-template').html());

    this._addEvents(this._element.find('.grid-actions'));
  }

  GalleryView.prototype.block = function() {
    this._element.block({
      message: '<div class="gallery-loading"></div>',
      centerX: false,
      centerY: false,
      fadeIn: 0,
      css: { border: 'none', background: 'transparent', width: '100%', left: '0', top: '0' },
      overlayCSS: { opacity: 0.3 }
    });
  };

  GalleryView.prototype.unblock = function() {
    this._element.unblock();
  };

  GalleryView.prototype.render = function() {
    var $this = this;

    if (!$this._url) {
      return;
    }

    var url = buildUrl(this._url, {
      category: $this._category || null,
      filter: $this._filter || null,
      search: $this._search || null
    });

    $this.block();

    $.ajax({
      url: url,
      dataType: 'json',
      type: 'GET',
      async: false,
      success: function (json) {
        json.ActiveCategory = $this._category;

        json.VisiblePackages = _.reject(json.Packages || [], function (e) {
          return e.HideFromPackageListing;
        });

        json.NonFeaturedCategories = _.reject(json.Categories || [], function(e) {
          return e === 'Featured';
        });

        _.each(json.VisiblePackages, function(e) {
          e.IsFeatured = _.contains(e.Categories || [], 'Featured');
          e.NonFeaturedCategories = _.reject(e.Categories || [], function(c) {
            return c === 'Featured';
          });

          if (e.IsFeatured) {
            json.HasFeatured = true;
          }
        });

        json.FeaturedPackage = _.chain(json.VisiblePackages)
          .filter(function(e) { return e.IsFeatured; })
          .sample(1)
          .first()
          .value();

        json.NonFeaturedPackages = json.FeaturedPackage
          ? _.chain(json.VisiblePackages)
            .reject(function (e) { return e.URI === json.FeaturedPackage.URI; })
            .value()
          : json.VisiblePackages;

        $this._renderView(json);
        $this.unblock();
      },
      error: function() {
        $this._renderError();
        $this.unblock();
      }
    });
  };

  GalleryView.prototype._renderError = function() {
    this._element.find('.view').html(this._errorTemplate({}));
  };

  GalleryView.prototype._renderView = function(json) {
    var $this = this;
    var element = $this._element;

    element.find('.view').html(this._galleryTemplate(json));

    $this._addEvents(element.find('.view'));
  };

  GalleryView.prototype._addEvents = function(scope) {
    var $this = this;

    scope.find('[data-gallery-nav]').each(function() {
      var e = $(this);

      e.removeAttr('href').removeAttr('onclick').prop('onclick', null);

      function setDropdownTitle(title) {
        e.parents('.dropdown').find('.dropdown-toggle > .title').html(title);
      }

      function clearSearch() {
        $this._search = '';
        $this._element.find('.entitylist-search .query').val('');
      }

      e.click(function() {
        var command = e.data('gallery-nav');

        switch (command) {
        case "view":
          $this._url = e.data('gallery-nav-value');
          setDropdownTitle(e.html());
          clearSearch();
          $this.render();
          return true;

        case "filter":
          $this._filter = e.data('gallery-nav-value');
          setDropdownTitle(e.html());
          clearSearch();
          $this.render();
          return true;

        case "search":
          $this._search = $this._element.find('.entitylist-search .query').val();
          $this.render();
          return false;

        case "category":
          $this._category = e.data('gallery-nav-value');
          clearSearch();
          $this.render();
          return false;
        }
      });
    });

    scope.find('.zeroclipboard').each(function () {
      var $this = $(this);
      var client = new ZeroClipboard(this);

      client.on('load', function (client, args) {
        $this.parent().show().parent().addClass('input-group');
      });
    });
  };

  $(function() {
    $('[data-view="gallery"]').each(function() {
      new GalleryView($(this)).render();
    });
  });

}(window.jQuery, window.moment, window.Handlebars, window.ZeroClipboard));
