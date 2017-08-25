/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/


XRM.onActivate(function() {

  var $ = XRM.jQuery;
  
  function createPreviewButton(container) {
    if (!YAHOO.util.Cookie) {
      XRM.log('YAHOO.util.Cookie not loaded. Cannot enable Preview functionality.', 'warn');
      return null;
    }

    // No preview button if the DOM does not flag it as enabled.
    if ($('[data-xrm-preview-permitted], .xrm-preview-permitted').length < 1) {
      return null;
    }
    
    var previewCookieValue = YAHOO.util.Cookie.get('adxPreviewUnpublishedEntities', function(stringValue) { return stringValue === 'true'; });

    var module = $('<div />').addClass('xrm-editable-toolbar-module').prependTo(container).get(0);

    return new YAHOO.widget.Button({
      container: module,
      type: 'checkbox',
      label: (previewCookieValue ?  window.ResourceManager['Preview_On_Label'] :  window.ResourceManager['Preview_Off_Label']),
      title: window.ResourceManager['Published_UnPublished_Entities_Title'],
      onclick: {
        fn: function() {
          YAHOO.util.Cookie.set('adxPreviewUnpublishedEntities', !previewCookieValue, { path: '/' });
          location.reload();
        }
      },
      checked: previewCookieValue
    });
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
  
  $(document).ready(function () {
      $(document).on('focus', '.first-child', function () {
          $('span.yui-push-button>span.first-child button').last().on("focus", function () {
              $('span.yui-button.yui-menu-button button').attr('aria-expanded', 'false');
          });
      });

      $(document).on('hover', '.yuimenuitem', function () {
          $('.yuimenuitemlabel').each(function () {
              $(this).parent().attr('title', $(this).text());
          });
          $(this).unbind('hover');
      });

      $(document).on('hover', '.yui-button > .first-child > button', function () {
          $(this).attr('title', $(this).text());
          $(this).unbind('hover');
      });

      $('.container-close').click(function () {
          $('.yuimenu').removeClass('visible');
          $('.yuimenu').addClass('yui-overlay-hidden');
          $('.yuimenu').attr('style', 'z-index: 1; position: absolute; visibility: hidden;');
      });

      if (!XRM.editable.toolbar) {
      XRM.log('Unable to extend XRM.editable.toolbar.', 'warn');
      return;
    }

    createPreviewButton(XRM.editable.toolbar.body);

    var positionToolbar = debounce(function () {
      XRM.editable.toolbar.align('tr', 'tr');
    }, 100);

    positionToolbar();

    $(window).resize(positionToolbar).scroll(positionToolbar);

    $(XRM.editable.toolbar.element).addClass('xrm-editable-toolbar-pinned');

    var dragTimeout;

    XRM.editable.toolbar.dragEvent.subscribe(function () {
      if (dragTimeout) {
        clearTimeout(dragTimeout);
      }

      $(XRM.editable.toolbar.element).removeClass('xrm-editable-toolbar-pinned');

      dragTimeout = setTimeout(function () {
        $(XRM.editable.toolbar.element).addClass('xrm-editable-toolbar-pinned');
      }, 100);
    });
    $(".container-close").attr('title', window.ResourceManager["Close_DefaultText"]);   
  });

  XRM.namespace('ui').getEditTooltip = function (containerElement) {
    var container = $(containerElement);

    var label = container.data('label') || container.children('.xrm-entity-ref').attr('title');

    return label
      ? XRM.localize('editable.label.prefix') + label
      : XRM.localize('editable.tooltip');
  };

  $.fn.extend({
    editable: function (serviceUri, options) {
      options = options || {};
      var container = this;
      var containerElement = container.get(0);

      var yuiContainer = $('<div />').addClass('xrm-editable-overlay').appendTo($('body'));
      var panelContainer = $('<div />').addClass('xrm-editable-panel xrm-editable-controls').appendTo(yuiContainer);

      var editPanel = new YAHOO.widget.Panel(panelContainer.get(0), {
        close: false,
        draggable: false,
        constraintoviewport: true,
        visible: false,
        zindex: XRM.zindex,
        context: [containerElement, 'tl', 'tl'],
        underlay: 'none'
      });

      var label = container.data('label') || container.children('.xrm-entity-ref').attr('title');

      editPanel.setBody('<a class="xrm-editable-edit"></a><span class="xrm-editable-label"></span>');
      editPanel.render();

      if (label) {
        $('.xrm-editable-label', editPanel.body).text(label).attr('title', label);
      } else {
        $('.xrm-editable-label', editPanel.body).remove();
      }

      function hide() {
        editPanel.hide();
        container.removeClass('xrm-editable-hover');
      }

      $('.xrm-editable-edit', editPanel.body)
        .attr('title', XRM.ui.getEditTooltip(containerElement))
        .text(XRM.ui.getEditLabel(containerElement))
        .click(function () {
          if (!serviceUri) {
            hide();

            if ($.isFunction(options.loadSuccess)) {
              options.loadSuccess();
            }

            return;
          }

          var hideProgressPanel = XRM.ui.showProgressPanelAtXY(XRM.localize('editable.loading'), YAHOO.util.Dom.getXY(containerElement));

          hide();

          // Retrieve the latest value data for the attribute, as JSON, and enter edit mode
          // when (if) it returns successfully.
          XRM.data.getJSON(serviceUri, {
            success: function (data) {
              hideProgressPanel();

              if ($.isFunction(options.loadSuccess)) {
                options.loadSuccess(data);
              }
            },
            error: function (xhr) {
              hideProgressPanel();

              if ($.isFunction(options.loadError)) {
                options.loadError(xhr);
              }
              else {
                XRM.ui.showDataServiceError(xhr);
              }
            }
          });
        });

      var positionPanel = debounce(function () {
        var containerPosition = containerElement.getBoundingClientRect();
        // Allow 30px for the height of the edit controls, hence the -29px offset.
        if (containerPosition && containerPosition.top < 30) {
          editPanel.align('tl', 'bl', [0, 0]);
        } else {
          editPanel.align('tl', 'tl', [0, -29]);
        }
      }, 100);

      positionPanel();

      $(window).load(positionPanel).resize(positionPanel).scroll(positionPanel);
      $(document).ajaxComplete(positionPanel);

      var timeout;

      var hoverIn = function () {
        if (timeout) {
          clearTimeout(timeout);
        }

        container.addClass('xrm-editable-hover');
        editPanel.show();
      };

      var hoverOut = function () {
        timeout = setTimeout(hide, 100);
      };

      panelContainer.hover(hoverIn, hoverOut);
      container.hover(hoverIn, hoverOut);
    },

    noneditable: function () {
      this.off('mouseenter mouseleave');
    }
  });
    
});
