(function () {
	'use strict';

	angular.module('ArticleLibraryApp')
		.directive('clickDisabled', function () {
			return {
				restrict: 'A',
				scope: {
					disabled: '=clickDisabled'
				},
				link: function (scope, el, attrs) {
					el.on('click', function () {
						return !scope.disabled;
					});
				}
			};
		})
	.directive('limitedVal', function () {
		return {
			restrict: 'E',
			scope: {
				value: '=',
				length: '='
			},						
			template: '<div title="{{value}}">{{value|limitToFormat:length}}</div>'
		};
	})
	.directive('sortBtn', function () {
		return {
			restrict: 'E',
			scope: {
				name: '=',
				asc: '=',
				colIndex: '=',
				fnClick: '=',
				curColIndex: '='
			},						
			templateUrl: 'Views/Directives/SortBtn.html'
		};
	})
	.directive('pagination', function () {
		return {
			restrict: 'E',
			scope: {
				pages: '=',
				curPage: '=',
				fnNextPage: '=',
				addParam: '='
			},						
			templateUrl: 'Views/Directives/Pagination.html'
		};
	})
	.directive('breadcrumb', function () {
		return {
			restrict: 'E',
			scope: {
				links: '='
			},
			link: function (scope, el, attrs) {
				scope.states = scope.links.split('|').map(function (v) {
					var vals = v.split(':');
					return { name: vals[0], state: vals.length > 1 ? vals[1]: null };
				});
			},
			templateUrl: 'Views/Directives/Breadcrumb.html'
		};
	})
	.directive('loading', function () {
		return {
			restrict: 'E',
			scope: {
				msg: '=',
				isError: '=',
				sending: '='
			},
			templateUrl: 'Views/Directives/Loading.html'
		};
	});
})();