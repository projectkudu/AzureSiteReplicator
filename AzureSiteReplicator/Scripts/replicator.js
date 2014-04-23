(function () {
    var refreshTimeout = 5000;
    var spinner = '<i class="fa fa-spinner fa-spin"></i>'
    var spinnerBadge = '<span class="badgePlain">' + spinner + '</span>';
    var syncSiteBadge = '<a href="#" class="syncSiteBadge" title="Sync"><span class="badgePlain glyphicon glyphicon-repeat"></span></a>';
    var siteStatusFormatString =
            '<li class="list-group-item" data-siteName="{0}" data-siteState="{1}">\n' +
            '  <span class="siteStatusText">{0} - {1}</span>\n' +
            '  <a href="#" title="Remove Site" class="removeSiteBadge"><span class="badgePlain glyphicon glyphicon-remove"></span></a>\n' +
            '  <a href="{2}?siteName={0}" title="Download logs"><span class="badgePlain glyphicon glyphicon-list"></span></a>\n' +
            '  <a href="#" class="syncSiteBadge" title="Sync"><span class="badgePlain glyphicon glyphicon-repeat"></span></a>\n' +
            '</li>';

    var noSitesMessage = "There doesn't appear to be any sites configured yet.  Upload a publish settings file to get started.";
    // Global string replacement function
    String.format = function () {
        var s = arguments[0];
        for (var i = 0; i < arguments.length - 1; i++) {
            var reg = new RegExp("\\{" + i + "\\}", "gm");
            s = s.replace(reg, arguments[i + 1]);
        }

        return s;
    }

    function removeSkipFile() {
        var $this = $(this);
        $this.parent().parent().remove();
        enableSaveButton();
        event.preventDefault();
    }

    function enableSaveButton() {
        var $saveSkipButton = $('#saveSkipButton');
        $saveSkipButton.prop('disabled', false);
        $saveSkipButton.addClass('btn-danger');
    }

    function getSkipRulesFromTable() {
        var skipRules = [];
        $('.skipRuleRow').each(function () {
            var $this = $(this);
            
            var skipRule = {};
            skipRule["expression"] = $this.find('td:nth-child(1)').text();
            skipRule["isDirectory"] = $this.find('td:nth-child(2) > input').is(':checked');
            skipRules.push(skipRule);
        });

        return skipRules;
    }

    // Wires up event handlers for each button in a site item
    function setupSiteStatusButtons() {
        $('.syncSiteBadge').each(function () {
            var $this = $(this);
            var $siteItem = $this.parent();
            var siteName = $siteItem.attr('data-siteName');
            var siteState = $siteItem.attr('data-siteState');

            // Replace sync button with spinner if currently deploying
            if (siteState == 'Deploying' || siteState == 'NotStarted') {
                $this.remove();
                $siteItem.append($(spinnerBadge));
            }

            $this.click(function () {
                var syncSiteUrl = BasePath + '/home/syncSite?name=' + siteName;

                $this.remove();
                $siteItem.append($(spinnerBadge));
                $siteItem.find('.siteStatusText').text(siteName + ' - ' + 'Deploying');

                $.ajax({
                    type: 'POST',
                    url: syncSiteUrl,
                    contentType: 'application/json',
                    error: function (jqXhr, textStatus, errorThrown) {
                        alert('Sync failed to site ' + siteName + ': ' + textStatus + ': ' + errorThrown);
                    }
                });
            });
        });

        $('.removeSiteBadge').click(function () {
            var $this = $(this);
            var siteName = $this.parent().attr("data-siteName");
            event.preventDefault();

            if (!confirm('Are you sure you would like to remove the site "' + siteName + '"?')) {
                return;
            }

            var deleteSiteUrl = BasePath + '/home/site?name=' + siteName;

            $.ajax({
                type: "Delete",
                url: deleteSiteUrl,
                contentType: "application/json",
                error: function (jqXhr, textStatus, errorThrown) {
                    alert(textStatus + ": " + errorThrown);
                }
            });

            $this.parent().remove();

            var $siteList = $('#siteList');
            if ($siteList.find('li').length == 0) {
                $siteList.text(noSitesMessage);
            }
        });
    }

    function getSiteStatus() {
        var statusUrl = BasePath + '/home/siteStatuses';
        var logUrl = BasePath + '/home/logFile';

        $.ajax({
            type: "GET",
            url: statusUrl,
            contentType: "application/json",
            success: function (data, textStatus, jqXHR) {
                var $siteList = $('#siteList');
                
                if (data.length == 0) {
                    $siteList.text(noSitesMessage);
                }
                else {
                    $siteList.text('');
                }

                data.forEach(function (entry) {
                    var $site = $(String.format(siteStatusFormatString, entry.Name, entry.State, logUrl));
                    $siteList.append($site);
                });
            },
            complete: function () {
                setupSiteStatusButtons();
                setTimeout(getSiteStatus, refreshTimeout);
            }
        });
    };

    setTimeout(getSiteStatus, refreshTimeout);
    setupSiteStatusButtons();

    $('#addSkipButton').click(function () {
        var $expr = $('#skipExpressionText');
        var exprValue = $expr.val();

        var row =
            '<tr class="skipRuleRow">\n' +
            '   <td>' + exprValue + '</td>\n' +
            '   <td class="rowCentered"><input type="checkbox" /></td>\n' +
            '   <td class="rowCentered"><button type="button" class="close" aria-hidden="true">×</button></td>\n' +
            '</tr>';

        var $row = $(row);
        $row.find('button').click(removeSkipFile);
        $row.find('input[type="checkbox"]').change(enableSaveButton);

        $('#skipRulesTableBody').append($row);
        enableSaveButton();

        $(this).prop('disabled', true);
        $expr.val('');

        event.preventDefault()
    });

    $('.skipRuleRow input[type="checkbox"]').change(enableSaveButton);

    $('#skipExpressionText').keyup(function () {
        var $this = $(this);
        var $addButton = $('#addSkipButton');
        if ($this.val().length == 0) {
            $addButton.prop('disabled', true);
        }
        else {
            $addButton.prop('disabled', false);
        }

        event.preventDefault()
    });

    $('#saveSkipButton').click(function () {
        var skipRules = getSkipRulesFromTable();
        var saveUrl = BasePath + '/home/skiprules';
        var $this = $(this);
        $this.text('');
        $this.html(spinner);

        $.ajax({
            type: "POST",
            url: saveUrl,
            contentType: "application/json",
            data: JSON.stringify(skipRules),
            error: function (jqXhr, textStatus, errorThrown) {
                alert("Failed to save skip rules: " + textStatus + ", " + errorThrown);
            },
            complete: function () {
                $this.text('Save');
            }
        });

        var $this = $(this);
        $this.prop('disabled', true);
        $this.removeClass('btn-danger');
        event.preventDefault()
    });

    $('tr.skipRuleRow button').click(removeSkipFile);

    $('#testSkipButton').click(function () {
        var testUrl = BasePath + '/home/testSkipRules';
        var skipRules = getSkipRulesFromTable();
        var $this = $(this);
        $this.text('');
        $this.html(spinner);

        $.ajax({
            type: "POST",
            url: testUrl,
            contentType: "application/json",
            data: JSON.stringify(skipRules),
            error: function (jqXhr, textStatus, errorThrown) {
                alert("Failed to test skip rules: " + textStatus + ", " + errorThrown);
            },
            success: function (data, textStatus, jqXHR) {
                var $textArea = $('#testSkipTextArea');
                var content = '';
                data.forEach(function (entry) {
                    content += entry + '\n';
                });

                if (content.length == 0) {
                    content = "0 files were skipped";
                }

                $textArea.val(content);
            },
            complete: function () {
                $this.text('Test');
            }
        });
    });
})();