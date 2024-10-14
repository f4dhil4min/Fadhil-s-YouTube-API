$(document).ready(function () {
    const toggleButton = $('#modeToggle');
    const body = $('body');

    // Check local storage for saved theme
    const currentTheme = localStorage.getItem('theme');
    if (currentTheme === 'dark') {
        body.removeClass('light-mode').addClass('dark-mode');
    }

    // Toggle dark/light mode
    toggleButton.click(function () {
        body.toggleClass('dark-mode');

        // Save the theme in local storage
        if (body.hasClass('dark-mode')) {
            localStorage.setItem('theme', 'dark');
            toggleButton.text('Toggle Light Mode'); // Update button text
        } else {
            localStorage.setItem('theme', 'light');
            toggleButton.text('Toggle Dark Mode'); // Update button text
        }
    });

    // Instant search preview with thumbnails
    var $searchInput = $('#search-input');
    var $suggestionsList = $('#suggestions-list');

    $searchInput.on('input', function () {
        var query = $(this).val();

        if (query.length >= 2) {
            $.ajax({
                url: '/YouTube/GetSearchSuggestions', // Adjust the URL as needed
                data: { query: query },
                success: function (response) {
                    var suggestions = response.suggestions;
                    $suggestionsList.empty().show();

                    if (suggestions.length > 0) {
                        suggestions.forEach(function (suggestion) {
                            var listItem = `
                                <li class="list-group-item suggestion-item">
                                    <img src="${suggestion.thumbnail}" alt="${suggestion.title}" style="width: 50px; height: auto; margin-right: 10px;">
                                    <span>${suggestion.title}</span>
                                </li>`;
                            $suggestionsList.append(listItem);
                        });
                    } else {
                        $suggestionsList.hide();
                    }
                },
                error: function () {
                    $suggestionsList.hide();
                }
            });
        } else {
            $suggestionsList.hide();
        }
    });

    // Set the clicked suggestion to the input box
    $suggestionsList.on('click', '.suggestion-item', function () {
        var selectedSuggestion = $(this).find('span').text();
        $searchInput.val(selectedSuggestion);
        $suggestionsList.hide();
    });

    // Hide suggestions when clicking outside
    $(document).on('click', function (e) {
        if (!$(e.target).closest('#search-input, #suggestions-list').length) {
            $suggestionsList.hide();
        }
    });

    // Voice Search
    $('#voice-search').on('click', function () {
        var recognition = new webkitSpeechRecognition();
        recognition.lang = 'en-US';
        recognition.start();

        recognition.onresult = function (event) {
            var query = event.results[0][0].transcript;
            $('#search-input').val(query).trigger('input'); // Trigger input to show suggestions
        };
    });

    // Load recent searches
    var recentSearches = JSON.parse(localStorage.getItem('recentSearches')) || [];
    var $recentSearchesList = $('#recent-searches-list');

    if (recentSearches.length > 0) {
        recentSearches.forEach(function (search) {
            $recentSearchesList.append(`<li class="list-group-item">${search}</li>`);
        });
    }

    // Save recent searches
    $searchInput.on('input', function () {
        var query = $(this).val();
        if (query.length > 2 && !recentSearches.includes(query)) {
            recentSearches.push(query);
            localStorage.setItem('recentSearches', JSON.stringify(recentSearches));
            $recentSearchesList.append(`<li class="list-group-item">${query}</li>`);
        }
    });

    // Load trending searches
    $.ajax({
        url: '/YouTube/GetTrendingSearches', // Adjust the URL as needed
        success: function (response) {
            var $trendingSearches = $('#trending-searches');
            response.trending.forEach(function (trend) {
                $trendingSearches.append(`<li class="list-group-item">${trend}</li>`);
            });
        }
    });
});
