(function () {
	'use strict';

	angular.module('ArticleLibraryApp').controller('UserConfirmationCtrl', ['$scope', 'AuthReqFactory', '$stateParams', '$state', 'ErrorService',
		function ($scope, AuthReqFactory, $stateParams, $state, ErrorService) {
			$scope.ConfirmationStateMsg = 'Please wait for the end of the confirmation process...';

			AuthReqFactory.confirm($stateParams.id).then(function () {
				alert('Congratulations! Your account has been confirmed. You can now use it to log in');
				$state.go('app');
			}, function (data) {
				$scope.ConfirmationStateMsg = ErrorService.formatMsg('Unfortunately, your account was not confirmed', data);
				$scope.error = true;
			});
		}]);
})();