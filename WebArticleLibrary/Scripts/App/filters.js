(function () {
	'use strict';

	angular.module('ArticleLibraryApp')
		.filter('limitToFormat', function () {
			return function (input, length) {
				var input = input || '';
				return input.length > length ? input.substr(0, length) + '...' : input;
			};
		});
})();