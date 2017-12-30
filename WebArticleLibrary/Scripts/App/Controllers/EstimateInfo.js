(function () {
	'use strict';

	angular.module('ArticleLibraryApp').controller('EstimateInfoCtrl', ['$scope', '$state', '$sce', 'ConverterService', 'UserReqFactory', 'ArticleReqFactory', 'AuthService', 'ErrorService', '$uibModal', 'MODAL_CLICK_MSG',
		function ($scope, $state, $sce, ConverterService, UserReqFactory, ArticleReqFactory, AuthService, ErrorService, $uibModal, MODAL_CLICK_MSG) {
			$scope.filter = {};

			$scope.page = 1;
			$scope.col = 7;
			$scope.colAsc = false;

			$scope.statuses = ArticleReqFactory.getAvailableEstimateStatuses();

			$scope.hasAdminStatus = function (curUser) {
				return curUser ? $scope.ui && $scope.ui.status == UserReqFactory.ST_ADMIN : $scope.selectedUser && $scope.selectedUser.status == UserReqFactory.ST_ADMIN;
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
				else if (!$scope.isError)
					onRequest(true);
			};

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

			$scope.sortItems = function (col) {
				if (!$scope.pages.length)
					return;

				var asc = col == $scope.col ? !$scope.colAsc : true;

				$scope.getFilteredItems(null, col, asc);
			};

			$scope.goToPage = function (index) {
				if ($scope.page == index)
					return;

				$scope.getFilteredItems(index, null, null);
			};

			$scope.getFilteredItems = function (page, col, asc) {
				var search = $scope.search;
				page = page || $scope.page;
				col = col == null ? $scope.col : col;
				asc = asc == null ? $scope.colAsc : asc;

				onRequest();

				ArticleReqFactory.getEstimates(page, col, asc, $scope.ui.id, $scope.filter).then(function (data) {
					$scope.ests = data.data;
					$scope.articleNames = data.articleNames;

					$scope.pages = getPageNumber(data.dataCount, data.pageLength);
					$scope.page = page;
					$scope.col = col;
					$scope.colAsc = asc;

					$scope.selectedEst = null;

					onRequest(true);
				}, onError);
			};

			if (!$scope.ui) {
				onRequest();

				var setUserInfo = function (userInfo) {
					$scope.ui = userInfo;

					$scope.getFilteredItems();
				};

				AuthService.getCurrentUser().then(function (outcome) {
					if (outcome == 0)
						$state.go('app');
					else if (outcome)
						setUserInfo(outcome);
				}, onError);
			}

			$scope.selectEstRow = function (e) {
				$scope.selectedEst = $scope.selectedEst && $scope.selectedEst.id == e.id ? null : e;
			};

			$scope.getEstStatusCaption = ArticleReqFactory.getEstimateStatusCaption;

			$scope.getArticleName = function (cmnt) {
				return cmnt ? $scope.articleNames[cmnt.articleId] : null;
			};
		}]);
})();