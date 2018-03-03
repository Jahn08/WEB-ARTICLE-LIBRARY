(function () {
	'use strict';

	angular.module('ArticleLibraryApp').controller('RegisterModalCtrl', ['$scope', '$uibModalInstance', 'userData', '$window', '$state', 'AuthReqFactory', 'ErrorService',
		function ($scope, $uibModalInstance, userData, $window, $state, AuthReqFactory, ErrorService) {
			$scope.userName = userData.name;
			$scope.userPassword = userData.password;

			$scope.closeModal = function () {
				$uibModalInstance.dismiss();
			};

			$scope.register = function () {
				$scope.sending = false;
				$scope.isError = false;
				$scope.sending = true;
				$scope.msg = "Please, wait...";

				AuthReqFactory.register({
					name: $scope.userName,
					password: $scope.userPassword,
					firstName: $scope.userFirstName,
					lastName: $scope.userLastName,
					patronymicName: $scope.userPatronymicName,
					email: $scope.userEmail
				}).then(function () {
					$uibModalInstance.dismiss();
					$state.go('app.afterregistration');
				}, function (data) {
					$scope.sending = false;
					$scope.isError = true;
					$scope.msg = ErrorService.formatMsg('Registration error', data);
				});
			};
		}])
		.controller('CommentModalCtrl', ['$scope', '$sce', '$uibModalInstance', 'ConverterService', 'ArticleReqFactory', 'data',
			function ($scope, $sce, $uibModalInstance, ConverterService, ArticleReqFactory, data) {
				$scope.parentId = data.parentId;
				$scope.comments = data.comments;

				if (!$scope.parentId) {
					$scope.comments = $scope.comments.filter(function (val) {
						return val.status != ArticleReqFactory.ST_DELETED;
					});
				}

				var userNames = data.userNames;

				$scope.isBlocked = function (status) {
					return status == ArticleReqFactory.ST_BLOCKED;
				};

				$scope.getUserName = function (authorId) {
					return authorId && userNames ? userNames[authorId] : null;
				};

				$scope.getCommentContent = function (content) {
					return content ? $sce.trustAsHtml(ConverterService.bytesToStr(content)) : null;
				};

				$scope.closeModal = function () {
					$uibModalInstance.dismiss();
				};
			}])
		.controller('ResponseModalCtrl', ['$scope', '$uibModalInstance',
			function ($scope, $uibModalInstance) {
				var response = {};

				$scope.closeModal = function () {
					if (confirm('Without a comment you cannot change the state. Continue?'))
						$uibModalInstance.dismiss();
				};
				$scope.save = function () {
					response.content = $scope.content;
					$uibModalInstance.close(response);
				};
			}])
		.controller('ComplaintModalCtrl', ['$scope', '$uibModalInstance', 'data',
			function ($scope, $uibModalInstance, data) {
				var complaint = {};

				if (data.commentId) {
					complaint.userCommentId = data.commentId;
					$scope.typeAddition = "a comment";
				}
				else {
					$scope.typeAddition = "an article";
				}

				complaint.articleId = data.articleId;

				$scope.closeModal = function () {
					$uibModalInstance.dismiss();
				};
				$scope.save = function () {
					complaint.text = $scope.content;
					$uibModalInstance.close(complaint);
				};
			}])
		.controller('AmendmentModalCtrl', ['$scope', '$uibModalInstance', 'data', 'AuthReqFactory', 'ErrorService',
			function ($scope, $uibModalInstance, data, AuthReqFactory, ErrorService) {
				var amendment = {};

				if (data.amendment) {
					amendment = data.amendment;
					var strs = amendment.content.split(' - ');
					$scope.content = strs.length > 1 ? strs[1] : amendment.content;

					if (!data.selection)
						data.selection = strs[0].replace(/\"/g, '');
				}

				data.selection = data.selection || '';
				$scope.global = !data.selection.length ? true : data.global;
				$scope.selection = data.selection;
				$scope.cutSelection = data.selection.length > 100 ? data.selection.substr(0, 100) + "..." : data.selection;
				$scope.readonly = data.readonly;

				$scope.closeModal = function () {
					$uibModalInstance.dismiss();
				};
				$scope.save = function () {
					amendment.content = '"' + $scope.selection + '" - ' + $scope.content;
					$uibModalInstance.close(amendment);
				};
			}])
		.controller('ArticleHistoryModalCtrl', ['$scope', '$uibModalInstance', '$state', 'data',
			function ($scope, $uibModalInstance, $state, data) {
				$scope.filtered = [];

				$scope.items = data.data;
				$scope.artName = data.articleName;
				var articleApproved = data.articleApproved;

				$scope.items.forEach(function (val) {
					val.history.forEach(function (v) {
						if (v.object) {
							if (v.object.endsWith('VERSION'))
								v.toArticle = true;
							else if (articleApproved && v.object.startsWith('COMMENT'))
								v.toComment = true;
						}
					});
				});

				if (data.id) {
					$scope.selectedId = data.id;
				}

				$scope.goToObject = function (obj, id) {
					var objType = obj.split(/[. ]/g);

					$scope.filtered = $scope.items.filter(function (val) {
						var first = val.history[0];
						return first.object.startsWith(objType[0]) && first.objectId == id;
					});
				};
				$scope.exitObject = function () {
					$scope.filtered = [];
				};
				$scope.goToArticle = function (id, commentId) {
                    var url = $state.href("app.articleview", { id: id, commentId: commentId });
					window.open(url);
				};

				$scope.GetValueDescription = function (objName) {
					return objName.endsWith('VERSION') ? (objName.startsWith('CHILD') ?
						"ANOTHER VERSION WAS CREATED" : "WAS CREATED AS A NEW VERSION") : '';
				};
				$scope.closeModal = function () {
					$uibModalInstance.dismiss();
				};
			}])
		.controller('NotificationModalCtrl', ['$scope', '$uibModalInstance', '$state', 'data',
			function ($scope, $uibModalInstance, $state, data) {
				$scope.notifications = data.notifications;

				$scope.toHistory = function (articleId, historyId) {
                    var url = $state.href("app.articleview", { historyId: historyId, id: articleId });
					window.open(url);
				}

				$scope.closeModal = function (clear) {
					$uibModalInstance.close(clear);
				};
			}]);
})();