(function () {
	'use strict';

	angular.module('ArticleLibraryApp').controller('HomeCtrl', ['$scope', '$timeout', 'AuthReqFactory', 'AuthService', 'ErrorService', 'ArticleReqFactory',
		function ($scope, $timeout, AuthReqFactory, AuthService, ErrorService, ArticleReqFactory) {

			var onRequest = function (end) {
				if (end) {
					$scope.loading = false;
					$scope.msg = null;
				}
				else {
					$scope.loading = true;
					$scope.msg = "Please, wait...";
				}
				$scope.isError = false;
			};

			var onError = function (data) {
				$scope.msg = ErrorService.formatMsg('Error', data);
				$scope.isError = true;
				$scope.loading = false;
			};

			onRequest();

			var setUserInfo = function (ui) {
				ArticleReqFactory.getArticleTitles().then(function (artData) {
					$scope.ui = ui;

					ArticleReqFactory.getDefaultCategories().then(function (data) {
						$scope.categories = data;
						$scope.userNames = artData.userNames;

						$scope.getUserName = function (userId) {
							return $scope.userNames ? $scope.userNames[userId] : null;
						};

						var randomInt = function (_min, _max) {
							return Math.floor(Math.random() * (_max - _min + 1)) + _min;
						};

						$scope.slides = [];

						var getCategory = function (_tags) {
							return $scope.categories.find(function (v) {
								return _tags.toLowerCase().split(' ').indexOf(v.toLowerCase()) != -1;
							})
						};

						if (artData.articles.length <= 3) {
							for (var i = 0; i < 3; ++i) {
								var item = artData.articles[i];

								var category = item ? getCategory(item.tags) : $scope.categories[randomInt(0, data.length - 1)];

								$scope.slides.push({
									article: item,
									index: randomInt(1, 3),
									category: category
								});
							}
						}
						else {
							var step = artData.articles.length / 3;

							for (var i = 0; i < 3; ++i) {
								var item = artData.articles[i];

								$scope.slides.push({
									article: item,
									index: randomInt(i+1, (i == 2 ? artData.articles.length : i + step) - 1),
									category: getCategory(item.tags)
								});
							}
						}

						onRequest(true);

						$timeout(function () { $('#slides').carousel(); }, 0);
					}, onError);
				}, onError);
			};

			AuthService.getCurrentUser().then(function (outcome) {
				setUserInfo(outcome);
			}, onError);
		}]);
})();