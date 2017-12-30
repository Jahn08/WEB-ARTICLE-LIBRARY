(function () {
	'use strict';

	angular.module('ArticleLibraryApp', ['ui.router', 'ui.bootstrap', 'ngResource', 'ngCookies', 'ngSanitize'])
		.constant('NO_PHOTO_FILE_URL', 'images/NoPhoto.png')
		.constant('MODAL_CLICK_MSG', 'backdrop click')	
		.config(function ($stateProvider, $urlRouterProvider, $locationProvider) {

			$locationProvider.hashPrefix('');

			$stateProvider.state('app', {
				url: '/',
				views: {
					'header': {
						templateUrl: 'Views/Header.html',
						controller: 'HeaderCtrl'
					},
					'content': {
						templateUrl: 'Views/Home.html',
						controller: "HomeCtrl"
					},
					'footer': {
						templateUrl: 'Views/Footer.html',
						controller: "FooterCtrl"
					}
				},
				resolve: {
					section: function () {
						return 'app';
					} 
				}
			})
			.state('app.confirmuser', {
				url: 'confirmuser/:id',
				views: {
					'content@': {
						template: '<span ng-class="{\'text-success\':!error, \'text-danger\':error}">{{ConfirmationStateMsg}}</span>',
						controller: 'UserConfirmationCtrl'
					}
				}
			})
			.state('app.confirmemail', {
				url: 'confirmemail/:id',
				views: {
					'content@': {
						template: '<span ng-class="{\'text-success\':!error, \'text-danger\':error}">{{ConfirmationStateMsg}}</span>',
						controller: 'EmailConfirmationCtrl'
					}
				}
			})
			.state('app.afterregistration', {
				url: 'afterregistration',
				views: {
					'content@': {
						template: '<span class="text-info">A message with a confirmation link was sent to your email.' +
							'Please, activate your account through the link until it gets expired</span>'
					}
				}
			})
			.state('app.aftermarkingpasswordforresetting', {
				url: 'aftermarkingpasswordforresetting',
				views: {
					'content@': {
						template: '<span class="text-info">A message with a confirmation link was sent to your email.' +
							'Please, reset your current password through the link until it gets expired</span>'
					}
				}
			})
			.state('app.userinfo', {
				url: 'userinfo',
				views: {
					'content@': {
						templateUrl: 'Views/UserInfo.html',
						controller: 'UserInfoCtrl'
					}
				},
				data: {
					secure: true
				}
			})
			.state('app.complaintinfo', {
				url: 'complaintinfo',
				views: {
					'content@': {
						templateUrl: 'Views/ComplaintInfo.html',
						controller: 'ComplaintInfoCtrl'
					}
				},
				data: {
					secure: true
				}
			})
			.state('app.articleinfo', {
				url: 'articleinfo',
				views: {
					'content@': {
						templateUrl: 'Views/ArticleInfo.html',
						controller: 'ArticleInfoCtrl'
					}
				},
				data: {
					secure: true
				}
			})
			.state('app.commentinfo', {
				url: 'commentinfo',
				views: {
					'content@': {
						templateUrl: 'Views/CommentInfo.html',
						controller: 'CommentInfoCtrl'
					}
				},
				data: {
					secure: true
				}
			})
			.state('app.estimateinfo', {
				url: 'estimateinfo',
				views: {
					'content@': {
						templateUrl: 'Views/EstimateInfo.html',
						controller: 'EstimateInfoCtrl'
					}
				},
				data: {
					secure: true
				}
			})
			.state('app.properties', {
				url: 'properties/:confirmEmail',
				views: {
					'content@': {
						templateUrl: 'Views/Properties.html',
						controller: 'PropertiesCtrl'
					}
				},
				data: {
					secure: true
				}
			})
			.state('app.resetpassword', {
				url: 'resetpassword/:id',
				views: {
					'content@': {
						templateUrl: 'Views/ResetPassword.html',
						controller: 'ResetPasswordCtrl'
					}
				}
			})
			.state('app.aboutus', {
				url: "aboutus",
				views: {
					'content@': {
						templateUrl: 'Views/AboutUs.html',
						controller: 'AboutUsCtrl'
					},
					'header@': {
						templateUrl: 'Views/Header.html',
						controller: 'HeaderCtrl',
					}
				},
				resolve: {
					section: function () {
						return "aboutus";
					} 
				}
			})
			.state('app.articleedit', {
				url: "editarticle/:id/:category",
				views: {
					'content@': {
						templateUrl: 'Views/ArticleEdit.html',
						controller: 'ArticleEditCtrl'
					},
					'header@': {
						templateUrl: 'Views/Header.html',
						controller: 'HeaderCtrl',
					}
				},
				data: {
					secure: true
				},
				resolve: {
					section: function ($stateParams) {
						return $stateParams.id ? 'app': 'createarticle';
					}
				}
			})
			.state('app.articleview', {
				url: "viewarticle/:id/:commentId/:historyId",
				views: {
					'content@': {
						templateUrl: 'Views/ArticleView.html',
						controller: 'ArticleViewCtrl'
					}
				}
			})
			.state('app.articlesearch', {
				url: "searcharticle/:category/:author",
				views: {
					'content@': {
						templateUrl: 'Views/ArticleSearch.html',
						controller: "ArticleSearchCtrl"
					},
					'header@': {
						templateUrl: 'Views/Header.html',
						controller: 'HeaderCtrl',
					}
				},
				resolve: {
					section: function ($stateParams) {
						return $stateParams.category || 'searcharticle';
					}
				}
			});

			$urlRouterProvider.otherwise('/');
		})
		.run(['$state', '$rootScope', 'AuthService', function ($state, $rootScope, AuthService) {
			$rootScope.$on('$stateChangeStart', function(event, toState, toParams, fromState, fromParams, options){ 

				if (toState.data && toState.data.secure) {

					AuthService.getCurrentUser().then(function (outcome) {
						if (outcome == 0) {
							event.preventDefault();
							$state.go('app');
						}
					});
				}
			});
		}]);
})();