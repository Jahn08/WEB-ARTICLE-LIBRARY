(function () {
	'use strict';

	angular.module('ArticleLibraryApp').controller('UserInfoCtrl', ['$scope', '$state', '$uibModal', 'UserReqFactory', 'AuthService', 'ErrorService', 'ArticleReqFactory', 'MODAL_CLICK_MSG',
		function ($scope, $state, $uibModal, UserReqFactory, AuthService, ErrorService, ArticleReqFactory, MODAL_CLICK_MSG) {

			$scope.filter = {};

			$scope.usersPage = 1;
			$scope.usersCol = 0;
			$scope.usersColAsc = true;

			$scope.statuses = UserReqFactory.getAvailableStatuses();

			$scope.hasAdminStatus = function (curUser) {
				return curUser ? $scope.ui && $scope.ui.status == UserReqFactory.ST_ADMIN : $scope.selectedUser && $scope.selectedUser.status == UserReqFactory.ST_ADMIN;
			};

			var getPageNumber = function (data, pageLength) {
				return new Array(Math.ceil(data / pageLength));
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

			var onError = function (data) {
				if (data && data != MODAL_CLICK_MSG) {
					$scope.msg = ErrorService.formatMsg('Error', data);
					$scope.isError = true;
					$scope.sending = false;
				}
				else if (!$scope.isError)
					onRequest(true);
			};

			$scope.sortUsers = function (col) {
				if (!$scope.usersPages.length)
					return;

				var asc = col == $scope.usersCol ? !$scope.usersColAsc : true;

				$scope.getFilteredUsers(null, col, asc);
			};

			$scope.goToUserPage = function (index) {
				if ($scope.usersPage == index)
					return;

				$scope.getFilteredUsers(index, null, null);
			};

			$scope.getFilteredUsers = function (page, col, asc) {
				page = page || $scope.usersPage;
				col = col == null ? $scope.usersCol : col;
				asc = asc == null ? $scope.usersColAsc : asc;

				onRequest();

				UserReqFactory.getUsers(page, col, asc, $scope.filter)
					.then(function (data) {
						$scope.users = data.data;
						$scope.cmntNumber = data.cmntNumber;
						$scope.artNumber = data.artNumber;
						
						$scope.usersPages = getPageNumber(data.dataCount, data.pageLength);
						$scope.usersPage = page;
						$scope.usersCol = col;
						$scope.usersColAsc = asc;

						onRequest(true);
					}, onError);
			};

			if (!$scope.ui) {
				onRequest();

				var setUserInfo = function (userInfo) {
					$scope.ui = userInfo;

					if (!$scope.hasAdminStatus(1))
						$state.go('app');

					onRequest(true);

					if (!$scope.users)
						$scope.getFilteredUsers();
				};

				AuthService.getCurrentUser().then(function (outcome) {
					if (outcome == 0)
						$state.go('app');
					else if (outcome)
						setUserInfo(outcome);
				}, onError);
			}

			var setStatus = function (_st) {
				onRequest();

				if ($scope.selectedUser) {
					UserReqFactory.setUserStatus($scope.selectedUser.id, _st).then(function () {
						$scope.selectedUser.status = _st;

						onRequest(true);
					}, onError);
				}
			};
			$scope.setAdminStatus = function () {
				setStatus(UserReqFactory.ST_ADMIN);
			};
			$scope.setBannedStatus = function () {
				setStatus(UserReqFactory.ST_BANNED);
			};
			$scope.setApprovedStatus = function () {
				setStatus(UserReqFactory.ST_APPROVED);
			};

			$scope.hasBannedStatus = function () {
				return $scope.selectedUser && $scope.selectedUser.status == UserReqFactory.ST_BANNED;
			};
			$scope.hasApprovedStatus = function () {
				return $scope.selectedUser && $scope.selectedUser.status == UserReqFactory.ST_APPROVED;
			};

			$scope.getStatusCaption = UserReqFactory.getStatusCaption;

			$scope.selectRow = function (u) {
				$scope.selectedUser = $scope.selectedUser && $scope.selectedUser.id == u.id ? null : u;
			};

			$scope.getCommentCount = function () {
				var user = $scope.selectedUser;
				return (user ? $scope.cmntNumber[user.id] : null) || 0;
			};
			$scope.getArticleCount = function () {
				var user = $scope.selectedUser;
				return (user ? $scope.artNumber[user.id] : null) || 0;
			};

			$scope.goToCommentsList = function () {
				var user = $scope.selectedUser;
				onRequest();

				ArticleReqFactory.getComments(null, null, true, user.id, true).then(function (data) {
					$uibModal.open({
						templateUrl: "Views/ModalDialogs/CommentModal.html",
						controller: "CommentModalCtrl",
						resolve: {
							data: {
								parentId: null,
								comments: data.data,
								userNames: data.userNames
							}
						}
					}).result.then(null, onError);
				}, onError);
			};
		}]);
})();