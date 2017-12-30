(function () {
	'use strict';

	angular.module('ArticleLibraryApp').controller('EmailConfirmationCtrl', ['$scope', 'UserReqFactory', '$stateParams', 'ErrorService',
		function ($scope, UserReqFactory, $stateParams, ErrorService) {
			$scope.ConfirmationStateMsg = 'Please wait for the end of the confirmation process...';

			UserReqFactory.confirmEmail($stateParams.id).then(function () {
				$scope.ConfirmationStateMsg = 'Congratulations! Your email address has been confirmed';
			}, function (data) {
				$scope.ConfirmationStateMsg = ErrorService.formatMsg('Unfortunately, your email address was not confirmed', data);
				$scope.error = true;
			});
		}]);
})();