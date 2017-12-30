(function () {
	'use strict';

	angular.module('ArticleLibraryApp').controller('ResetPasswordCtrl', ['$scope', '$stateParams', 'UserReqFactory','$state', 'ErrorService',
		function ($scope, $stateParams, UserReqFactory, $state, ErrorService) {
			$scope.reset = function () {
				$scope.loading = true;
				$scope.msg = "Please, wait...";
				$scope.isError = false;

				UserReqFactory.replacePassword($scope.newPassword, $stateParams.id).then(function () {
					$scope.loading = false;
					alert('Congratulations! Your old password has been replaced');
					$state.go('app');
				}, function (data) {
					$scope.loading = false;
					$scope.isError = true;
					$scope.msg = ErrorService.formatMsg('Unfortunately, your password was not replaced', data);
				});
			};
		}]);
})();