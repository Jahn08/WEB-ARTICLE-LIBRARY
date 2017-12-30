(function () {
	'use strict';

	angular.module('ArticleLibraryApp').controller('ArticleInfoCtrl', ['$scope', '$state', '$uibModal', 'UserReqFactory', 'AuthService', 'ErrorService', 'ArticleReqFactory', 'MODAL_CLICK_MSG',
		function ($scope, $state, $uibModal, UserReqFactory, AuthService, ErrorService, ArticleReqFactory, MODAL_CLICK_MSG) {
			$scope.filterPrivate = {};
			$scope.filterPublic = {};

			$scope.statuses = ArticleReqFactory.getAvailableArticleStatuses();

			$scope.hasAdminStatus = function () {
				return $scope.ui && $scope.ui.status == UserReqFactory.ST_ADMIN;
			};

			var getPageNumber = function (data, pageLength) {
				return new Array(Math.ceil(data / pageLength));
			};

			var onError = function (data) {
				if (data && data != MODAL_CLICK_MSG) {
					$scope.msg = ErrorService.formatMsg('Error', data);
					$scope.isError = true;
					$scope.sending = false;
				}
			};

			var afterLoading = function () {
				$scope.msg = null;
				$scope.sending = false;
			};

			$scope.goToArticlePage = function (index, forPublic) {
				if ((!forPublic && $scope.curPrivateArtPage == index) ||
					(forPublic && $scope.curPublicArtPage == index))
					return;

				$scope.getFilteredArticles(forPublic, index, null, null);
			};

			$scope.sortArticles = function (col, forPublic) {
				var asc;

				if (forPublic) {
					if (!$scope.publicArtPages.length)
						return;

					asc = col == $scope.curPublicCol ? !$scope.curPublicColAsc : true;
				} else {
					if (!$scope.privateArtPages.length)
						return;

					asc = col == $scope.curPrivateCol ? !$scope.curPrivateColAsc : true;
				}

				$scope.getFilteredArticles(forPublic, null, col, asc);
			};

			$scope.sortPublicArticles = function (col) {
				$scope.sortArticles(col, 1);
			};

			$scope.getFilteredArticles = function (forPublic, page, col, asc) {
				var searchFilter;

				if (forPublic) {
					searchFilter = $scope.filterPublic;
					page = page || $scope.curPublicArtPage;
					col = col == null ? $scope.curPublicCol : col;
					asc = asc == null ? $scope.curPublicColAsc : asc;
				}
				else {
					searchFilter = $scope.filterPrivate;
					page = page || $scope.curPrivateArtPage;
					col = col == null ? $scope.curPrivateCol : col;
					asc = asc == null ? $scope.curPrivateColAsc : asc;
				}

				$scope.isError = false;
				$scope.msg = "Please, wait...";
				$scope.sending = true;

				var unionLists = function (target, source) {
					if (!target || !source)
						return;

					for (var prop in source) {
						if (!target[prop])
							target[prop] = source[prop];
					}
				};

				ArticleReqFactory.getAllArticles(page, col, asc, searchFilter)
					.then(function (artData) {
						$scope.selectedPArticle = [];

						unionLists($scope.userNames, artData.userNames);
						unionLists($scope.estimates, artData.estimates);
						unionLists($scope.cmntNumber, artData.cmntNumber);

						if (forPublic) {
							$scope.publicArticles = artData.publicData;
							$scope.publicArtPages = getPageNumber(artData.publicDataCount, artData.pageLength);
							$scope.curPublicArtPage = page;
							$scope.curPublicCol = col;
							$scope.curPublicColAsc = asc;
						}
						else {
							$scope.privateArticles = artData.privateData;
							$scope.privateArtPages = getPageNumber(artData.privateDataCount, artData.pageLength);
							$scope.curPrivateArtPage = page;
							$scope.curPrivateCol = col;
							$scope.curPrivateColAsc = asc;
						}

						afterLoading();
					}, onError);
			};
			if (!$scope.ui) {
				$scope.isError = false;
				$scope.msg = "Please, wait...";
				$scope.sending = true;

				var setUserInfo = function (userInfo) {
					$scope.isError = false;
					$scope.msg = "Please, wait...";
					$scope.sending = true;

					$scope.ui = userInfo;

					ArticleReqFactory.getAllArticles().then(function (artData) {
						$scope.userNames = artData.userNames;
						$scope.estimates = artData.estimates;
						$scope.cmntNumber = artData.cmntNumber;

						if ($scope.userNames)
							$scope.userNames[$scope.ui.id] = $scope.ui.name;

						$scope.privateArticles = artData.privateData;
						$scope.privateArtPages = getPageNumber(artData.privateDataCount, artData.pageLength);
						$scope.curPrivateArtPage = 1;
						$scope.curPrivateCol = 7;
						$scope.curPrivateColAsc = false;

						if ($scope.hasAdminStatus(1)) {
							$scope.publicArticles = artData.publicData;
							$scope.publicArtPages = getPageNumber(artData.publicDataCount, artData.pageLength);
							$scope.curPublicArtPage = 1;
							$scope.curPublicCol = 7;
							$scope.curPublicColAsc = false;
						}

						afterLoading();
					}, onError);

					$scope.getUserName = function (userId) {
						return $scope.userNames[userId];
					};
					$scope.getCommentCount = function (art) {
						return art ? $scope.cmntNumber[art.id] : 0;
					};
					$scope.getEstimate = function (art, _private) {
						var est = null;

						if ($scope.estimates) {
							est = art ? ($scope.estimates[art.id] || 0) : 0;
							if (_private)
								$scope.pEstimate = est;
							else
								$scope.estimate = est;
						}

						return est;
					};
					$scope.isApproved = function (art) {
						return art ? art.status == ArticleReqFactory.ST_APPROVED : false;
					};
				};

				AuthService.getCurrentUser().then(function (outcome) {
					if (outcome == 0)
						$state.go('app');
					else if (outcome)
						setUserInfo(outcome);
				}, onError);
			}

			$scope.selectedPArticle = [];
			$scope.hasSelectedPArticle = function (id) {
				return $scope.selectedPArticle.some(function (val) { return val.id == id; });
			};
			$scope.selectPArticleRow = function (a) {
				if ($scope.hasSelectedPArticle(a.id)) {
					$scope.selectedPArticle = $scope.selectedPArticle.filter(function (val) {
						return val.id != a.id;
					});
				}
				else
					$scope.selectedPArticle.push(a);
			};
			$scope.selectArticleRow = function (a) {
				$scope.selectedArticle = $scope.selectedArticle && $scope.selectedArticle.id == a.id ? null : a;
			};

			$scope.getArticleStatusCaption = ArticleReqFactory.getStatusCaption;
			$scope.assignArticle = function (article) {
				ArticleReqFactory.assignArticle(article.id).then(function () {
					article.assignedToId = $scope.ui.id;
					article.status = ArticleReqFactory.ST_REVIEW;
				}, onError);
			};
			$scope.unassignArticle = function (article) {
				ArticleReqFactory.unassignArticle(article.id).then(function () {
					article.assignedToId = null;
					article.status = ArticleReqFactory.ST_CREATED;
				}, onError);
			};
			$scope.isForAssign = function () {
				return $scope.selectedArticle && $scope.selectedArticle.status == ArticleReqFactory.ST_CREATED;
			};
			$scope.removeArticle = function () {

				if (confirm('You are going to remove these articles. Continue?')) {
					$scope.isError = false;
					$scope.msg = "Please, wait...";
					$scope.sending = true;

					var ids = $scope.selectedPArticle.map(function (val) {
						return val.id;
					});

					ArticleReqFactory.removeArticle(ids).then(function (artData) {
						$scope.selectedPArticle = [];
						$scope.privateArticles = artData.privateData;
						$scope.privateArtPages = getPageNumber(artData.privateDataCount, artData.pageLength);
						$scope.curPrivateArtPage = 1;
						$scope.curPrivateCol = 0;
						$scope.curPrivateColAsc = true;

						afterLoading();
					}, onError);
				}
			};

			$scope.artHistories = [];
			$scope.showHistory = function (art) {
				var openHistoryModal = function (data) {
					$uibModal.open({
						templateUrl: "Views/ModalDialogs/ArticleHistoryModal.html",
						controller: "ArticleHistoryModalCtrl",
						resolve: {
							data: {
								data: data,
								articleName: art.name,
								articleApproved: art.status == ArticleReqFactory.ST_APPROVED
							}
						}
					}).result.then(null, onError);
				};

				var history = $scope.artHistories[art.id];

				if (history) {
					openHistoryModal(history);
				}
				else {
					$scope.isError = false;
					$scope.msg = "Please, wait...";
					$scope.sending = true;

					ArticleReqFactory.getArticleHistory(art.id).then(function (data) {
						$scope.artHistories[art.id] = data;
						openHistoryModal(data);

						afterLoading();
					}, onError);
				}
			};
		}]);
})();