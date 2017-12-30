(function () {
	'use strict';

	angular.module('ArticleLibraryApp').controller('ComplaintInfoCtrl', ['$scope', '$state', '$sce', 'ConverterService', 'UserReqFactory', 'ArticleReqFactory', 'AuthService', 'ErrorService', '$uibModal', 'MODAL_CLICK_MSG', 
		function ($scope, $state, $sce, ConverterService, UserReqFactory, ArticleReqFactory, AuthService, ErrorService, $uibModal, MODAL_CLICK_MSG) {
			$scope.filter = {};

			$scope.statuses = ArticleReqFactory.getAvailableComplaintStatuses();
			$scope.entityTypes = ArticleReqFactory.getComplaintEntityTypeEnum();

			$scope.page = 1;
			$scope.col = 7;
			$scope.colAsc = false;

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

				ArticleReqFactory.getComplaints(page, col, asc, $scope.filter)
					.then(function (data) {
						$scope.cmpls = data.data;
						$scope.userNames = data.userNames;

						if ($scope.userNames)
							$scope.userNames[$scope.ui.id] = $scope.ui.name;

						$scope.articleNames = data.articleNames;
						$scope.comments = data.comments;

						$scope.pages = getPageNumber(data.dataCount, data.pageLength);
						$scope.page = page;
						$scope.col = col;
						$scope.colAsc = asc;

						$scope.selectedCmpl = null;

						onRequest(true);
					}, onError);
			};

			if (!$scope.ui) {
				onRequest();

				var setUserInfo = function (userInfo) {
					$scope.ui = userInfo;

					if (!$scope.hasAdminStatus(1))
						$state.go('app');

					$scope.getFilteredItems();
				};

				AuthService.getCurrentUser().then(function (outcome) {
					if (outcome == 0)
						$state.go('app');
					else if (outcome)
						setUserInfo(outcome);
				}, onError);
			}

			var setAssignment = function (complaint, assign) {
				onRequest();

				var promise = assign ? ArticleReqFactory.assignCmpl(complaint.id) :
					ArticleReqFactory.unassignCmpl(complaint.id);

				promise.then(function () {
					complaint.assignedToId = assign ? $scope.ui.id : null;
					onRequest(true);
				}, onError);
			};

			$scope.userIsRelatedToIssue = function () {
				var userId = $scope.ui.id;
				var cmpl = $scope.selectedCmpl;
				return cmpl.authorId == userId || cmpl.cmntAuthorId == userId || cmpl.articleAuthorId == userId;
			};

			$scope.assignCmpl = function () {
				setAssignment($scope.selectedCmpl, true);
			};

			$scope.unassignCmpl = function () {
				setAssignment($scope.selectedCmpl);
			};

			$scope.selectCmplRow = function (c) {
				$scope.selectedCmpl = $scope.selectedCmpl && $scope.selectedCmpl.id == c.id ? null : c;
			};

			var setStatus = function (_st) {
				if ($scope.selectedCmpl) {
					$uibModal.open({
						templateUrl: "Views/ModalDialogs/ResponseModal.html",
						controller: "ResponseModalCtrl"
					}).result.then(function (data) {
						if (!data.content)
							alert('The state will not be changed as the comment is empty');
						else {
							onRequest();

							ArticleReqFactory.setComplaintStatus($scope.selectedCmpl.id, _st, data.content).then(function () {
								// An approved complaint for an article means probable influence on others
								if (!$scope.selectedCmpl.commentId && _st == ArticleReqFactory.ST_APPROVED) {
									$state.transitionTo($state.current, null, {
										reload: true, inherit: false, notify: true
									});
								}

								$scope.selectedCmpl.status = _st;
								$scope.selectedCmpl.assignedToId = null;
								onRequest(true);
							}, onError);

						}
					}, onError);
				}
			};

			$scope.approveComplaint = function () {
				setStatus(ArticleReqFactory.ST_APPROVED);
			};

			$scope.refuseComplaint = function () {
				setStatus(ArticleReqFactory.ST_REFUSED);
			};

			$scope.cmplIsCreated = function () {
				return $scope.selectedCmpl.status == ArticleReqFactory.ST_CREATED;
			};

			$scope.getCmplStatusCaption = ArticleReqFactory.getComplaintStatusCaption;

			$scope.getUserName = function (userId) {
				return $scope.userNames[userId];
			};

			$scope.getArticleName = function (id) {
				if (id)
					return $scope.articleNames[id];

				var cmpl = $scope.selectedCmpl;
				return cmpl && cmpl.articleId ? $scope.articleNames[cmpl.articleId] : null;
			};

			$scope.getCommentContent = function (commentId) {
				var cmpl = $scope.selectedCmpl;
				return cmpl && cmpl.userCommentId ? $sce.trustAsHtml(ConverterService.bytesToStr($scope.comments[cmpl.userCommentId])) : null;
			};
		}]);
})();