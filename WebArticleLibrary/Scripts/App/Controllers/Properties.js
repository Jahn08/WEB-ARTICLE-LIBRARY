(function () {
	'use strict';

	angular.module('ArticleLibraryApp').controller('PropertiesCtrl', ['$scope', '$state', '$stateParams', 'UserReqFactory', 'AuthService', 'ErrorService', 'NO_PHOTO_FILE_URL',
		function ($scope, $state, $stateParams, UserReqFactory, AuthService, ErrorService, NO_PHOTO_FILE_URL) {
			$scope.hasAdminStatus = function () {
				return $scope.ui && $scope.ui.status == UserReqFactory.ST_ADMIN;
			};

			var onError = function (data) {
				$scope.msg = ErrorService.formatMsg('Error', data);
				$scope.isError = true;
				$scope.sending = false;
			};

			if (!$scope.ui) {
				$scope.isError = false;
				$scope.msg = "Please, wait...";
				$scope.sending = true;

				var oldEmail;

				if (window.FileReader) {

					$('#filePicture').on('change', function () {
						var file = this.files[0];

						var fr = new FileReader();
						fr.onload = function () {
							var photo = fr.result.split(',')[1];
							$scope.ui.photo = photo;

							$('#imgPicture').attr('src', fr.result);
						};

						fr.readAsDataURL(file);
					});
				}

				$scope.removePhoto = function () {
					$('#imgPicture').attr('src', NO_PHOTO_FILE_URL);
					$scope.ui.photo = '';
				};

				var setUserInfo = function (userInfo) {
					$scope.isError = false;
					$scope.msg = $stateParams.confirmEmail == "true" ? 'A message with a confirmation link was sent to your new email address.' +
						'Please, confirm your new email address through the link until it gets expired' : null;

					$scope.ui = userInfo;
					$scope.sending = false;

					$scope.getUserPhoto = function () {
						var contents = $scope.ui.photo;
						return contents ? "data:image/jpeg;base64," + contents : NO_PHOTO_FILE_URL;
					};

					oldEmail = userInfo.email;

					$scope.getStatusCaption = UserReqFactory.getStatusCaption;
				};

				AuthService.getCurrentUser(true).then(function (outcome) {
					if (outcome == 0)
						$state.go('app');
					else if (outcome)
						setUserInfo(outcome);
				}, onError);
			}

			$scope.save = function () {
				$scope.msg = "Please, wait...";
				$scope.sending = true;

				UserReqFactory.saveUserInfo($scope.ui).then(function () {
					AuthService.logIn($scope.ui, true);
					$state.transitionTo($state.current, { confirmEmail: oldEmail != $scope.ui.email }, {
						reload: true, inherit: false, notify: true
					});
				}, onError);
			};
		}]);
})();