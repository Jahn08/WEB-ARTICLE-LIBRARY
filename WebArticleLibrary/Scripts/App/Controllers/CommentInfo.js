(function () {
	'use strict';

	angular.module('ArticleLibraryApp').controller('CommentInfoCtrl', ['$scope', '$state', '$sce', 'ConverterService', 'UserReqFactory', 'ArticleReqFactory', 'AuthService', 'ErrorService', '$uibModal', 'MODAL_CLICK_MSG',
		function ($scope, $state, $sce, ConverterService, UserReqFactory, ArticleReqFactory, AuthService, ErrorService, $uibModal, MODAL_CLICK_MSG) {
			$scope.filter = {};
			$scope.page = 1;
			$scope.col = 7;
			$scope.colAsc = false;
			$scope.statuses = ArticleReqFactory.getAvailableCommentStatuses();

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

				ArticleReqFactory.getComments(page, col, asc, $scope.ui.id, false, null, $scope.filter)
					.then(function (data) {
						$scope.cmnts = data.data;
						$scope.articleNames = data.articleNames;
						$scope.complaintNumber = data.complaintNumber;
						$scope.relatedCmntNumber = data.relatedCmntNumber;

						$scope.pages = getPageNumber(data.dataCount, data.pageLength);
						$scope.page = page;
						$scope.col = col;
						$scope.colAsc = asc;

						$scope.selectedCmnt = null;

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

			$scope.selectCmntRow = function (c) {
				$scope.selectedCmnt = $scope.selectedCmnt && $scope.selectedCmnt.id == c.id ? null : c;
			};

			$scope.removeComment = function () {
				if (confirm('You are going to remove the comment. Continue?')) {
					onRequest();
					var cmnt = $scope.selectedCmnt;

					ArticleReqFactory.updateCommentStatus(cmnt.id, ArticleReqFactory.ST_DELETED).then(function () {
						$state.transitionTo($state.current, null, {
							reload: true, inherit: false, notify: true
						});
					}, onError);
				}
			};

			$scope.getCmntStatusCaption = ArticleReqFactory.getCommentStatusCaption;

			$scope.getRelatedCommentCount = function () {
				var cmnt = $scope.selectedCmnt;
				return cmnt ? $scope.relatedCmntNumber[cmnt.id] : null;
			};
			$scope.getComplaintCount = function () {
				var cmnt = $scope.selectedCmnt;
				return cmnt ? $scope.complaintNumber[cmnt.id] : null;
			};
			$scope.getArticleName = function (cmnt) {
				return cmnt ? $scope.articleNames[cmnt.articleId] : null;
			};
			$scope.getCommentContent = function () {
				var cmnt = $scope.selectedCmnt;
				return cmnt ? $sce.trustAsHtml(ConverterService.bytesToStr(cmnt.content)) : null;
			};
			$scope.goToCommentsList = function () {
				var cmnt = $scope.selectedCmnt;
				onRequest();

				ArticleReqFactory.getComments(null, null, true, null, true, cmnt.id).then(function (data) {
					$uibModal.open({
						templateUrl: "Views/ModalDialogs/CommentModal.html",
						controller: "CommentModalCtrl",
						resolve: {
							data: {
								parentId: cmnt.id,
								comments: data.data,
								userNames: data.userNames
							}
						}
					}).result.then(null, onError);
				}, onError);
			};
		}]);
})();