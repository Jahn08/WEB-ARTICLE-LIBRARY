(function () {
	'use strict';

	angular.module('ArticleLibraryApp').controller('ArticleViewCtrl', ['$scope', '$state', '$stateParams', 'ArticleReqFactory', 'ErrorService', 'AuthService', '$sce', 'ConverterService', '$timeout', '$location', '$anchorScroll', 'UserReqFactory', '$uibModal', 'NO_PHOTO_FILE_URL', 'MODAL_CLICK_MSG',
		function ($scope, $state, $stateParams, ArticleReqFactory, ErrorService, AuthService, $sce, ConverterService, $timeout, $location, $anchorScroll, UserReqFactory, $uibModal, NO_PHOTO_FILE_URL, MODAL_CLICK_MSG) {
			$scope.sending = true;
			$scope.msg = null;

			var setInfo = function (userInfo) {
				ArticleReqFactory.viewArticle($stateParams.id, userInfo.id).then(function (data) {
					$scope.art = data.article;
					$scope.updatedDate = data.updatedDate;
					$scope.ui = userInfo;

					$scope.comments = data.comments;
					$scope.userPhotos = data.userPhotos;
					$scope.userNames = data.userNames;
					$scope.authorName = $scope.userNames[$scope.art.authorId];

					$scope.searchForUserArticles = function () {
						$state.go('app.articlesearch', { author: $scope.authorName });
					};

					$scope.estimate = data.estimate;
					$scope.estimateState = data.curEstimate == ArticleReqFactory.EST_POSITIVE ? 1 :
						(data.curEstimate == ArticleReqFactory.EST_NEGATIVE ? -1 : 0);

					$scope.hasAdminStatus = function () {
						return $scope.ui && $scope.ui.status == UserReqFactory.ST_ADMIN;
					};

					if (!userInfo && $scope.art.status != ArticleReqFactory.ST_APPROVED) {
						onError('The article has not been published yet');
					}
					else if (($scope.art.status != ArticleReqFactory.ST_DRAFT && $scope.art.status != ArticleReqFactory.ST_EDIT) ||
						(userInfo && $scope.art.authorId == userInfo.id)) {
						$scope.sending = false;
						$scope.msg = null;

						var htmlContent = $sce.trustAsHtml(ConverterService.bytesToStr($scope.art.content));
						$scope.art.tempContent = htmlContent;
						$scope.art.hashTags = $scope.art.tags.split(' ');

						$timeout(function () {
							$('#artContent').find('.amendment').remove();
						});

						$scope.getUserPhoto = function (authorId) {
							var contents = $scope.userPhotos[authorId];
							return contents ? "data:image/jpeg;base64," + contents : NO_PHOTO_FILE_URL;
						};

						// Only for a logged-in user
						if ($scope.ui) {
							$scope.artHistories = [];
							$scope.showHistory = function (id) {
								var art = $scope.art;

								var openHistoryModal = function (data) {
									$uibModal.open({
										templateUrl: "Views/ModalDialogs/ArticleHistoryModal.html",
										controller: "ArticleHistoryModalCtrl",
										resolve: {
											data: {
												id: id,
												data: data,
												articleName: art.name,
												articleApproved: art.status == ArticleReqFactory.ST_APPROVED,
											}
										}
									}).result.then(null, onError);
								};

								var history = $scope.artHistories[art.id];

								if (history) {
									openHistoryModal(history);
								}
								else {
									ArticleReqFactory.getArticleHistory(art.id).then(function (data) {
										$scope.artHistories[art.id] = data;
										openHistoryModal(data);
									}, onError);
								}
							};

							if ($stateParams.historyId) {
								$scope.showHistory($stateParams.historyId);
							}

							if ($scope.art.status == ArticleReqFactory.ST_APPROVED) {
								const commentContentId = "#comment";
								$(commentContentId).wysiwyg();
								$scope.comment = {};

								$scope.onTextInput = function () {
									$scope.comment.content = $(commentContentId).html();
								};

								$scope.comments.forEach(function (val) {
									var htmlContent = $sce.trustAsHtml(ConverterService.bytesToStr(val.content));
									val.tempContent = htmlContent;
									val.author = $scope.userNames[val.authorId];
								});

								$scope.respondTo = function (id) {
									$scope.comment.responseToId = id;
								};

								$scope.cancelResponse = function (id) {
									$scope.comment.responseToId = null;
								};

								$scope.toComment = function (id) {
									$scope.respondTo(id);
									$location.hash(id);
									$anchorScroll();
								};

								var complaining = function (commentId) {
									onRequest();

									$uibModal.open({
										templateUrl: "Views/ModalDialogs/ComplaintModal.html",
										controller: "ComplaintModalCtrl",
										resolve: {
											data: {
												commentId: commentId,
												articleId: $scope.art.id
											}
										}
									}).result.then(function (complaint) {
										ArticleReqFactory.createComplaint(complaint).then(function () {
											onRequest(true);
											alert("Your complaint is waiting its turn to be looked into");
										}, onError);
									}, onError);
								};

								$scope.showBanned = false;
								$scope.commentIsBlocked = function (status) {
									return ArticleReqFactory.ST_BLOCKED == status;
								};
								$scope.commentIsDeleted = function (status) {
									return ArticleReqFactory.ST_DELETED == status;
								};
								$scope.commentIsDeletedById = function (id) {
									var comment = $scope.comments.find(function (val) {
										return val.id == id;
									});
									return !comment || $scope.commentIsDeleted(comment.status);
								};

								$scope.commentComplaint = function (id) {
									if (confirm('You are going to make a complaint about the selected comment. Continue?')) {
										complaining(id);
									}
								};

								$scope.commentBan = function (cmnt, recover) {
									var process, status;

									if (recover) {
										process = "recover";
										status = ArticleReqFactory.ST_CREATED;
									}
									else {
										process = "ban";
										status = ArticleReqFactory.ST_BLOCKED;
									}

									if (confirm('You are going to ' + process + ' the comment. Continue?')) {
										ArticleReqFactory.updateCommentStatus(cmnt.id, status).then(function () {
											cmnt.status = status;
											onRequest(true);
										}, onError);
									}
								};

								$scope.commentRemove = function (cmnt) {
									if (confirm('You are going to remove the comment. Continue?')) {
										onRequest();
										ArticleReqFactory.updateCommentStatus(cmnt.id, ArticleReqFactory.ST_DELETED).then(function () {
											$state.transitionTo($state.current, { id: $stateParams.id }, {
												reload: true, inherit: false, notify: true
											});
										}, onError);
									}
								};

								$scope.articleComplaint = function () {
									if (confirm('You are going to make a complaint about the article. Continue?')) {
										complaining();
									}
								};

								$scope.articleBan = function () {
									if (confirm('You are going to return the article onto the review state. Continue?')) {
										onRequest();

										$uibModal.open({
											templateUrl: "Views/ModalDialogs/AmendmentModal.html",
											controller: "AmendmentModalCtrl",
											resolve: {
												data: {
													global: true,
													readonly: false
												}
											}
										}).result.then(function (data) {
											ArticleReqFactory.assignArticle($scope.art.id).then(function () {
												ArticleReqFactory.createAmendment($scope.art.id, data).then(function () {
													$state.go('app.articleedit', { id: $scope.art.id });
												}, onError);
											}, onError);
										}, onError);
									}
								};

								$scope.sendComment = function () {
									onRequest();

									$scope.comment.content = $(commentContentId).html();
									ArticleReqFactory.createComment($scope.art.id, $scope.comment).then(function () {
										$state.transitionTo($state.current, { id: $stateParams.id }, {
											reload: true, inherit: false, notify: true
										});
									}, onError);
								};

								// Go to a chosen comment
								if ($stateParams.commentId) {
									var value = $scope.comments.find(function (val) {
										return val.id == $stateParams.commentId;
									});

									if (value) {
										$scope.showComments = true;
										$timeout(function () {
											$scope.toComment($stateParams.commentId);
										});
									}
									else {
										$timeout(function () {
											alert('The searched comment was probably removed or banned');
										});
									}
								}
							}
						}
					}
					else
						onError('You cannot see the article while it is being edited');
				}, onError);
			};

			var onError = function (data) {
				if (data && data != MODAL_CLICK_MSG) {
					$scope.sending = false;
					$scope.isError = true;
					$scope.msg = ErrorService.formatMsg('Error', data);
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

			if (!$stateParams.id)
				$state.go('app');

			AuthService.getCurrentUser().then(function (outcome) {
				setInfo(outcome);
			}, onError);

			$scope.assess = function (positive) {
				onRequest();

				ArticleReqFactory.assessArticle($scope.art.id, positive).then(function (data) {
					$scope.estimate = data.estimate;
					$scope.estimateState = positive ? 1 : -1;
					onRequest(true);
				}, onError);
			}

			$scope.close = function () {
				$state.go('app.articleinfo');
			};
		}]);
})();