/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

(function ($) {

    var templateCache = {};

    function renderTemplate(url, container, data, done) {
        var tmpl = templateCache[url];

        if ($.isFunction(tmpl)) {
            $(container).html(tmpl(data));
            if ($.isFunction(done)) {
                done();
            }
            return;
        }

        $.ajax({
            url: url,
            method: 'GET',
            dataType: 'html',
            success: function (template) {
                tmpl = _.template(template);
                templateCache[url] = tmpl;
                $(container).html(tmpl(data));
                if ($.isFunction(done)) {
                    done();
                }
            }
        });
    }

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

    function formatDate(date) {
        return moment(date).utc().format('YYYY-MM-DDTHH:mm:ss') + 'Z';
    }

    function CalendarView(element) {
        var $this = this;
        this._element = $(element);
        this._templatePath = this._element.data('calendar-template-path');
        this._url = this._element.data('calendar-url');
        this._downloadUrl = this._element.data('calendar-download-url');
        this._initialView = this._element.data('calendar-initial-view');
        this._initialDate = this._element.data('calendar-initial-date');
        this._style = this._element.data('calendar-style');
        this._options = {
            events_source: function (from, to) {
                $this.block();
                return $this._getEvents(from, to);
            },
            tmpl_path: this._templatePath,
            onAfterViewLoad: function (view) {
                $this._onAfterViewLoad(this, view);
                $this.unblock();
            },
            onAfterEventsLoad: function (events) {
                $this._onAfterEventsLoad(this, events);
            },
            holidays: {},
            view: $this._initialView || 'month',
            day: $this._initialDate || 'now',
            language: this._element.closest('[lang]').attr('lang')
        };
    }

    CalendarView.prototype._getEvents = function (from, to) {
        var $this = this;

        $this._from = from;
        $this._to = to;

        var url = buildUrl(this._url, {
            from: formatDate(from),
            to: formatDate(to),
            filter: this._filter || null,
            search: $this._element.find('.entitylist-search .query').val() || null
        });

        var events = [];

        $.ajax({
            url: url,
            dataType: 'json',
            type: 'GET',
            async: false,
            success: function (json) {
                if (json.success) {
                    events = json.result;
                    $this._error = null;
                } else {
                    $this._error = json.error;
                }
            }
        });

        return events;
    };

    CalendarView.prototype._onAfterViewLoad = function (calendar, view) {
        var element = this._element;
        var error = this._error;

        this._view = view;

        element.find('.calendar-title').text(calendar.getTitle());
        element.find('[data-calendar-view]').removeClass('active');
        element.find('[data-calendar-view="' + view + '"]').addClass('active');

        if (this._downloadUrl) {
            element.find('.calendar-downloads a').attr('href', buildUrl(this._downloadUrl, {
                from: this._from ? formatDate(this._from) : null,
                to: this._to ? formatDate(this._to) : null,
                filter: this._filter || null,
                search: this._element.find('.entitylist-search .query').val() || null
            }));
        } else {
            element.find('.calendar-downloads').hide();
        }

        if (error) {
            element.find('.calendar-error').show().find('.message').text(error);
        } else {
            element.find('.calendar-error').hide();
        }
    };

    CalendarView.prototype._onAfterEventsLoad = function (calendar, events) {
        var $this = this;
        var element = $this._element;
        var eventListContainer = element.find('.event-list');

        calendar._small_ = $this._style === 'list';

        events = _.filter(events, function (e) {
            return (parseInt(e.start) < $this._to || e.start == null) && (parseInt(e.end) >= $this._from || e.end == null);
        });

        if (eventListContainer.length > 0) {
            renderTemplate($this._templatePath + 'view-event-list.html', eventListContainer, { cal: calendar, events: events });
        }
    };

    CalendarView.prototype._renderCalendar = function () {
        var $this = this;
        var element = $this._element;
        var calendarElement = element.find('.calendar');
        var calendar = calendarElement.calendar(this._options);

        if (this._style === 'list') {
            calendarElement.addClass('calendar-small');
        }

        element.find('[data-calendar-nav]').each(function () {
            var e = $(this);

            e.removeAttr('href').removeAttr('onclick').prop('onclick', null);

            function setDropdownTitle(title) {
                e.parents('.dropdown').find('.dropdown-toggle > .title').html(title);
            }

            e.click(function () {
                var command = e.data('calendar-nav');

                switch (command) {
                    case "view":
                        $this._url = e.data('calendar-nav-value');
                        $this._downloadUrl = e.data('calendar-nav-download');
                        setDropdownTitle(e.html());
                        calendar.view($this._view);
                        return true;

                    case "filter":
                        $this._filter = e.data('calendar-nav-value');
                        setDropdownTitle(e.html());
                        calendar.view($this._view);
                        return true;

                    case "search":
                        calendar.view($this._view);
                        return false;

                    default:
                        calendar.navigate(command);
                        return false;
                }
            });
        });

        element.find('[data-calendar-view]').each(function () {
            var e = $(this);
            e.click(function () {
                calendar.view(e.data('calendar-view'));
                return false;
            });
        });
    };

    CalendarView.prototype._showError = function (message) {
        this._error = message;
    };

    CalendarView.prototype.block = function () {
        this._element.block({
            message: '<div class="calendar-loading"></div>',
            centerX: false,
            centerY: false,
            fadeIn: 0,
            css: { border: 'none', background: 'transparent', width: '100%', left: '0', top: '0' },
            overlayCSS: { opacity: 0.3 }
        });
    };

    CalendarView.prototype.unblock = function () {
        this._element.unblock();
    };

    CalendarView.prototype.render = function () {
        var $this = this;

        if (!$this._templatePath) {
            return;
        }

        renderTemplate(this._templatePath + 'view.html', this._element.find('.view'), { _small_: $this._style === 'list' }, function () {
            $this._renderCalendar();
        });
    };

    $(function () {
        $('[data-view="calendar"]').each(function () {
            new CalendarView($(this)).render();
        });
    });
}(jQuery));
