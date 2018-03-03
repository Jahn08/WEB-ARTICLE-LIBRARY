(function () {
	'use strict';

	angular.module('ArticleLibraryApp').controller('ArticleSearchCtrl', ['$scope', '$stateParams', '$timeout', 'UserReqFactory', 'ArticleReqFactory', 'AuthService', 'ErrorService',
		function ($scope, $stateParams, $timeout, UserReqFactory, ArticleReqFactory, AuthService, ErrorService) {
			$scope.filter = {};
			$scope.allCategories = [];

			$scope.columns = [{ val: 0, name: "Name" },
			{ val: 1, name: "Author" },
			{ val: 2, name: "Tags" },
			{ val: 5, name: "Description" },
			{ val: 7, name: "Date" }];

			$scope.page = 1;
			$scope.pages = [1];

			$scope.col = 7;
			$scope.colAsc = false;

			var onRequest = function (end) {
				if (end) {
					$scope.sending = false;
					$scope.msg = null;
				}
				else {
					$scope.sending = true;
					$scope.msg = "Please, wait...";
				}
				$scope.isError = false;
			};
			var onError = function (data, msg) {
				if (data) {
					$scope.sending = false;
					$scope.isError = true;
					$scope.msg = ErrorService.formatMsg(msg, data);
				}
			};

			onRequest();

			$('#categories').select2({
				tags: true,
				tokenSeparators: [',', '.', ' ', ':'],
				width: "100%"
			});

			var getPageNumber = function (data, pageLength) {
				return new Array(Math.ceil(data / pageLength));
			};

			$scope.search = function (page) {
				onRequest();

				page = page || $scope.page;

				ArticleReqFactory.searchArticles(page, $scope.col, $scope.colAsc, $scope.filter).then(function (data) {
					var pages = getPageNumber(data.articleCount, data.pageLength);
					$scope.pages = pages.length ? pages : [1];
					$scope.page = page;

					$scope.articles = data.articles;
					$scope.userNames = data.userNames;

					$scope.articles.forEach(function (val) {
						val.author = $scope.userNames ? $scope.userNames[val.authorId] : null;
						val.hashTags = val.tags.split(" ");
					});

					onRequest(true);
				}, onError);
			};

			$scope.goToPage = function (index) {
				if ($scope.page == index)
					return;

				$scope.search(index);
			};

			ArticleReqFactory.getDefaultCategories().then(function (defCategoryData) {
				$scope.allCategories = defCategoryData;

				var category;

				if ((category = $stateParams.category) && $scope.allCategories.indexOf(category) == -1)
					$scope.allCategories.push(category);

				$timeout(function () {
					$('#categories').trigger("change");
				}, 0);

				if (category || $stateParams.author) {
					$scope.filter = {
						tags: [category],
						author: $stateParams.author
					};
					$scope.search();
				}
				else
					onRequest(true);
			}, onError);
		}]);
})();