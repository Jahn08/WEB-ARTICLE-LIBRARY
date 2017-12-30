(function () {
	'use strict';

	angular.module('ArticleLibraryApp').controller('HeaderCtrl', ['$scope', '$uibModal', 'AuthReqFactory', 'AuthService', 'ErrorService', 'ArticleReqFactory', '$state', 'section', 'MODAL_CLICK_MSG',
		function ($scope, $uibModal, AuthReqFactory, AuthService, ErrorService, ArticleReqFactory, $state, section, MODAL_CLICK_MSG) {
			var hubServer;
			var userId;
			
			var onRequest = function (end) {
				if (end) {
					$scope.loading = false;
					$scope.msg = null;
				}
				else {
					$scope.loading = true;
				}
				$scope.isError = false;
			};

			var onError = function (data) {
				if (data && data != MODAL_CLICK_MSG)
				{
					$scope.msg = ErrorService.formatMsg('Error', data);
					$scope.isError = true;
					$scope.loading = false;
				}
			};

			$scope.section = section;

			onRequest();

			var setUserInfo = function (ui) {
				ArticleReqFactory.getDefaultCategories().then(function (data) {
					$scope.categories = data;

					if (ui) {
						$scope.userName = ui.name;
						$scope.showUser = true;
						userId = ui.id;

						ArticleReqFactory.getNotifications(userId).then(function (data) {
							$scope.notifications = data;

							//************* SIGNALR **************//
							var hub = $.connection.notificationHub;
							hub.client.notify = function (newEvents) {
								if ($scope.notifications)
								{
									$scope.notifications = $scope.notifications.concat(newEvents);
									$scope.notifications.sort(function (a, b) {
										if (a.date === b.date)
											return 0;
										return a.date < b.date;
									});
								}
								else
									$scope.notifications = newEvents;
							};
							hub.client.clear = function (removedEvents) {
								var active = $scope.notifications.filter(function (val) {
									return !removedEvents.find(function (v) { return v.id == val.id });
								});
								$scope.$apply(function () { $scope.notifications = active; });
							};

							hubServer = hub.server;
							
							$.connection.hub.disconnected(function () {
								hubServer = null;
							});

							hub.client.close = function () {
								$.connection.hub.stop();
							};

							$.connection.hub.start().done(function () {
								hubServer.signUp(userId)
							});
							//************* SIGNALR **************//
							
							$scope.openNotificationModal = function () {
								if ($scope.notifications) {
									$uibModal.open({
										templateUrl: "Views/ModalDialogs/NotificationModal.html",
										controller: "NotificationModalCtrl",
										resolve: {
											data: {
												notifications: $scope.notifications
											}
										}
									}).result.then(function (clear) {
										if (clear)
										{
											onRequest();

											var ids = $scope.notifications.map(function (val) {
												return val.id;
											});

											ArticleReqFactory.clearNotifications(ids).then(function () {
												$scope.notifications = null;

												onRequest(true);
											}, onError);
										}
									}, onError);
								}
							};

							onRequest(true);
						}, onError);
					}
					else
						onRequest(true);
				}, onError);
			};
			
			AuthService.getCurrentUser().then(function (outcome) {
				setUserInfo(outcome);
			}, onError);

			var reloadPage = function () {
				$state.transitionTo($state.current, null, {
					reload: true, inherit: false, notify: true
				});
			};

			$scope.logOut = function () {
				$scope.msg = null;

				AuthReqFactory.logOut().then(function () {
					AuthService.logOut();
					$scope.showUser = false;

					if (hubServer && userId)
						hubServer.signOut(userId);

					reloadPage();
				}, function (data) {
					$scope.msg = ErrorService.formatMsg('Authentication error', data);
				});
			};

			$scope.logIn = function () {
				$scope.msg = null;
				$scope.loading = true;

				AuthReqFactory.logIn({
					name: $scope.userName,
					password: $scope.userPassword
				}).then(function (data) {
					AuthService.logIn(data);
					reloadPage();
				}, function (data) {
					if (data.status == '401')
						$scope.msg = 'The usage of a wrong user name or password';
					else
						$scope.msg = ErrorService.formatMsg('Authentication error', data);

					AuthService.logOut();

					$scope.showUser = false
					$scope.loading = false;
				});
			};

			$scope.openModal = function () {
				$uibModal.open({
					templateUrl: "Views/ModalDialogs/RegisterModal.html",
					controller: "RegisterModalCtrl",
					resolve: {
						userData: {
							name: $scope.userName,
							password: $scope.userPassword
						}
					}
				}).result.then(null, onError);
			};
			
			$scope.resetPasswordModal = function () {
				$uibModal.open({
					templateUrl: "Views/ModalDialogs/MarkPasswordForResettingModal.html",
					controller: ['$scope', 'UserReqFactory', 'ErrorService', '$state', '$uibModalInstance',
						function ($scope, UserReqFactory, ErrorService, $state, $uibModalInstance) {
							$scope.closeModal = function () {
								$uibModalInstance.dismiss();
							};

							$scope.resetPassword = function () {
								$scope.loading = true;
								$scope.isError = false;

								UserReqFactory.resetPassword($scope.email).then(function (data) {
									$uibModalInstance.dismiss();
									$state.go('app.aftermarkingpasswordforresetting');
								}, function (data) {
									$scope.loading = false;
									$scope.isError = true;
									$scope.msg = ErrorService.formatMsg('Error', data);
								});
							};
						}]
				}).result.then(null, onError);
			};
		}]);
})();