(function () {
	'use strict';

	angular.module('ArticleLibraryApp').controller('ArticleEditCtrl', ['$scope', '$state', '$timeout', '$stateParams', 'ArticleReqFactory', 'ErrorService', 'AuthService', '$location', '$anchorScroll', '$uibModal', 'ConverterService', 'MODAL_CLICK_MSG',
		function ($scope, $state, $timeout, $stateParams, ArticleReqFactory, ErrorService, AuthService, $location, $anchorScroll, $uibModal, ConverterService, MODAL_CLICK_MSG) {

			const artContentId = '#editor';
			const artReviewedContentId = '#reviewedContent';
			const amendmentClassName = 'amendment';

			$scope.amendments = [];
			$scope.selectedIndexes = [];
			$scope.allCategories = [];

			$('#categories').select2({
				tags: true,
				tokenSeparators: [',', '.', ' ', ':'],
				width: "100%"
			});

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

			var onError = function (data, msg) {
				if (data && data != MODAL_CLICK_MSG) {
					$scope.sending = false;
					$scope.isError = true;
					$scope.msg = ErrorService.formatMsg(msg, data);
				}
			};

			var _updateArticle = function (success, cleanMarks, excludeOriginalContent) {
				onRequest();

				if (cleanMarks) {
					$scope.amendments.forEach(function (val) {
						$(artReviewedContentId).find('#' + val.id).remove();
					});
				}

				var curAmClassSelector = '.underlined';
				$(curAmClassSelector).removeClass(curAmClassSelector);
				$('.' + amendmentClassName).removeClass('label-info');

				if (!excludeOriginalContent)
					$scope.art.content = $(artContentId).html();

				$scope.art.tempTags = getTags();

				ArticleReqFactory.updateArticle($scope.art).then(function () {
					if (success)
						success();

					onRequest(true);
				}, function (data) {
					onError(data, 'Unfortunately, the article was not saved');
				});
			};

			var setControls = function () {
				$scope.canBeDrafted = function () {
					return !$scope.curUser || $scope.art.authorId == $scope.curUser.id;
				};
				$scope.canBeSaved = function () {
					return !$scope.art.id || ($scope.art.authorId == $scope.curUser.id && $scope.art.status == ArticleReqFactory.ST_DRAFT);
				};
				$scope.canBeRemoved = function () {
					return $scope.art.id && $scope.art.authorId == $scope.curUser.id;
				};

				$scope.createVersion = function () {
					onRequest();

					ArticleReqFactory.createVersion($scope.art.id).then(function (data) {
						var url = $state.href('app.articleedit', { id: data.id });
						window.open(url);

						onRequest(true);
					}, onError);
				};

				$scope.removeArticle = function (id) {
					if (confirm('You are going to remove this article. Continue?')) {
						ArticleReqFactory.removeArticle([id]).then(function () {
							$state.go('app.articleinfo');
						}, function (data) {
							onError(data, 'Unfortunately, your article was not removed');
						});
					}
				};

				$scope.createArticle = function () {
					var status;
					var msg;

					if ($scope.draft)
					{
						status = ArticleReqFactory.ST_DRAFT;

						if ($scope.art.status != ArticleReqFactory.ST_DRAFT)
							msg = "You are going to make this article into a draft. Continue?";
					}
					else
					{
						status = ArticleReqFactory.ST_CREATED;
						msg = "You are going to make this article available for reviewing. Continue?";
					}

					if (!msg || confirm(msg))
					{
						$scope.art.status = status;
						_updateArticle(function () { $state.go('app.articleinfo'); }, $scope.draft);
					}
				};

				$scope.OnTextInput = function () {
					$scope.art.content = $(artContentId).html();
				};

				$scope.OnCategoryChange = function () {
					var tags = getTags();
					$scope.editArticleForm.$setValidity('tooManyCategories', !tags || tags.join(' ').length <= 50);
				};

				ArticleReqFactory.getDefaultCategories().then(function (defCategoryData) {
					$scope.allCategories = defCategoryData;

					if (!$stateParams.id && $stateParams.category) {
						$scope.art.tempTags = [$stateParams.category];
					}

					if ($scope.art.tempTags) {
                        $scope.art.tempTags.forEach(function (val) {
                            if ($scope.allCategories.indexOf(val) == -1)
								$scope.allCategories.push(val);
						})
					}

					if ($scope.art.reviewedContent) {
						$(artReviewedContentId).html($scope.art.reviewedContent);
					}

					$(artContentId).html($scope.art.content);
					$(artContentId).wysiwyg();

					$timeout(function () {
						$('#categories').trigger("change");
					}, 0);

					onRequest(true);
				}, onError);
			};

			// select2 doesn't consider custom tags through the angular model link
			var getTags = function () {
				var tags = $('#categories').val();

				if (tags) {
					tags = tags.map(function (v) {
						var mas = v.split(/[,. :]/);
						return mas[mas.length - 1];
					});
					return tags;
				};
			};

			if ($stateParams.id) {
				onRequest();

				var setInfo = function (userInfo) {
					$scope.curUser = userInfo;

					ArticleReqFactory.viewArticle($stateParams.id).then(function (data) {
						onRequest(true);

						var art = data.article;

						if (art.authorId != userInfo.id && (art.status == ArticleReqFactory.ST_DRAFT ||
							art.status == ArticleReqFactory.ST_EDIT)) {
							onError('You cannot see the article while it is being edited', 'Error');
						}
						else {
							$scope.art = art;
							$scope.readMode = $scope.art.authorId != userInfo.id || ($scope.art.status != ArticleReqFactory.ST_EDIT &&
								$scope.art.status != ArticleReqFactory.ST_DRAFT);

							$scope.art.content = ConverterService.bytesToStr($scope.art.content);
							if ($scope.art.reviewedContent)
								$scope.art.reviewedContent = ConverterService.bytesToStr($scope.art.reviewedContent);

							$scope.art.tempTags = $scope.art.tags.split(" ");
							$scope.content = $scope.art.content;

							setControls();
							setAmendingMode();
						}
					}, function (data) {
						onError(data, 'The article was not found');
					});
				};

				AuthService.getCurrentUser().then(function (outcome) {
					if (outcome == 0)
						$state.go('app');
					else if (outcome)
						setInfo(outcome);
				}, onError);
			}
			else {
				$scope.art = {};
				setControls();
			}

			// AMENDMENT SECTION FUNCTIONS
			var setAmendingMode = function () {
				const resolvedSymbol = '✔';
				const unresolvedSymbol = '✖';

				onRequest(true);

				$scope.startAmending = function () {
					$scope.art.reviewedContent = $(artContentId).html();
					$scope.art.status = ArticleReqFactory.ST_AMENDING;

					_updateArticle(function () {
						$state.transitionTo($state.current, { id: $stateParams.id }, {
							reload: true, inherit: false, notify: true
						});
					}, false, true);
				};

				var setStates = function () {
					$scope.amendingMode = $scope.art.status == ArticleReqFactory.ST_AMENDING || $scope.art.status == ArticleReqFactory.ST_EDIT ||
						($scope.art.status == ArticleReqFactory.ST_REVIEW && $scope.amendments.length > 0);
					$scope.onReview = $scope.art.assignedToId == $scope.curUser.id && $scope.art.status == ArticleReqFactory.ST_REVIEW;
					$scope.onAmending = $scope.art.assignedToId == $scope.curUser.id && $scope.art.status == ArticleReqFactory.ST_AMENDING;
					$scope.onEdit = $scope.art.authorId == $scope.curUser.id && $scope.art.status == ArticleReqFactory.ST_EDIT;
				};

				setStates();

				if ($scope.amendingMode || $scope.onReview) {
					ArticleReqFactory.getAmendments($stateParams.id).then(function (amendments) {
						$scope.amendments = amendments;
						setStates();
					}, onError);

					$scope.scrollTo = function (id) {
						$scope.scrolledAmendmentId = id;
						$location.hash(id);
						$anchorScroll();
						$('.' + amendmentClassName).removeClass('label-info');
						$('#' + id).addClass('label-info');
					};

					$timeout(function () {
						$('#amendmentPanel').affix({ offset: { top: 20 } });
					});

					$scope.getAmendmentComment = function (content) {
						var strs = content.split(' - ');
						content = strs.length > 1 ? strs[1] : content;
						return content.length > 25 ? content.substring(0, 25) + '...' : content;
					};

					$scope.openAmendment = function (amIndex) {
						var selIndex;
						var selection;
						var selText;

						if (amIndex == null) {
							selIndex = $scope.amendments.length;
							selection = $scope.selection;
							selText = selection.selectedText;
						}
						else
							selIndex = amIndex;

						var amendment = $scope.amendments[selIndex];

						$uibModal.open({
							templateUrl: "Views/ModalDialogs/AmendmentModal.html",
							controller: "AmendmentModalCtrl",
							resolve: {
								data: {
									selection: selText,
									amendment: amendment,
									readonly: !$scope.onAmending || (amendment && (amendment.resolved || amendment.archived))
								}
							}
						}).result.then(function (data) {
							amendment = data;
							onRequest();

							if (!data.id) {
								ArticleReqFactory.createAmendment($scope.art.id, amendment).then(function (entity) {
									var span = $("<span>").attr('id', entity.id)
										.attr('title', entity.content)
										.attr('contenteditable', false)
										.text(unresolvedSymbol)
										.addClass(amendmentClassName + ' text-danger');

									var parents = $(selection.focusNode).parentsUntil(artReviewedContentId);
									var focusNode = parents.length ? parents.last() : selection.focusNode;
									$(span).insertBefore(focusNode);

									$scope.art.reviewedContent = $(artReviewedContentId).html();
									_updateArticle(function () {
										$scope.amendments[selIndex] = entity;
										onRequest(true);
									}, false, true);
								}, onError);
							}
							else {
								ArticleReqFactory.updateAmendment([amendment]).then(function () {
									onRequest(true);
								}, onError);
							}
						}, onError);
					};
					$scope.removeAmendment = function (ids) {

						var forRemove = $scope.amendments.filter(function (val, index) {
							return ids.indexOf(index) != -1;
						}).map(function (val) {
							return val.id;
						});

						if (forRemove.length) {
							onRequest();

							ArticleReqFactory.removeAmendment(forRemove).then(function (entity) {
								onRequest(true);

								forRemove.forEach(function (val) {
									$('#' + val).remove();
								});

								$scope.art.reviewedContent = $(artReviewedContentId).html();
								_updateArticle(null, false, true);

								$scope.amendments = $scope.amendments.filter(function (val) {
									return forRemove.indexOf(val.id) == -1;
								});
								$scope.selectedIndexes = [];
							}, onError);
						}
					};
					$scope.selectAmendment = function (index) {
						if ($('.amdBox' + index + ':checked').length > 0) {
							$scope.selectedIndexes.push(index);
						}
						else {
							$scope.selectedIndexes = $scope.selectedIndexes.filter(function (val) {
								return val != index;
							});
						}
						$scope.selectedUnresolved = $scope.selectedIndexes.every(function (val) {
							var _am = $scope.amendments[val];
							return _am && !_am.archived && !_am.resolved;
						});
					};
					$scope.allResolved = function () {
						return !$scope.amendments.filter(function (val) {
							return !val.archived;
						}).some(function (val) {
							return !val.resolved;
						});
					};

					if ($scope.onAmending) {
						$(document).mousemove(function () {
							$scope.$apply(function () {
								var selection = window.getSelection();

								$scope.selection = {};
								$scope.selection.selectedText = selection.anchorNode &&
									$(selection.anchorNode).parents(artReviewedContentId).length > 0 &&
									$(selection.focusNode).parents(artReviewedContentId).length > 0 ? selection.toString() : null;

								if ($scope.selection.selectedText)
									$scope.selection.focusNode = selection.focusNode;
							});
						});
						$(artReviewedContentId).focusout(function () {
							$scope.$apply(function () {
								$scope.selection.selectedText = null;
							});
						});
					}

					if ($scope.onEdit) {
						$scope.resolveAmendment = function (amIndexes) {
							var amForResolve = $scope.amendments.filter(function (val, index) {
								return amIndexes.indexOf(index) != -1;
							});

							if (amForResolve.length) {
								onRequest();

								amForResolve.forEach(function (val) {
									val.resolved = true;
								});

								ArticleReqFactory.updateAmendment(amForResolve).then(function () {

									amForResolve.forEach(function (val) {
										var span = $("#" + val.id).text(resolvedSymbol)
											.removeClass('text-danger').addClass('text-success');
									});

									$scope.art.reviewedContent = $(artReviewedContentId).html();
									_updateArticle(function (entity) {
										onRequest(true);
									});
								}, onError);
							}
						};
					}
				}
			};

            $scope.close = function () {
                if (confirm('You are about to close the page without saving any progress you might have done. Continue?'))
                    $state.go('app.articleinfo');
			};
			$scope.changeArticleState = function (toEditing, approve) {				
				var status;
				var msg;

				if (approve)
				{
					status = ArticleReqFactory.ST_APPROVED;
					msg = "You are going to approve publishing this article. Continue?";
				}
				else
				{
					if (toEditing)
					{
						status = ArticleReqFactory.ST_EDIT;

						if ($scope.art.status != ArticleReqFactory.ST_EDIT)
							msg = "You are going to send this article back for editing. Continue?";
					}
					else {
						status = ArticleReqFactory.ST_REVIEW;
						msg = "You are going to send this article for reviewing. Continue?";
					}
				}

				if (!msg || confirm(msg))
				{
					onRequest();

					$scope.art.status = status;

					if ($scope.art.status != ArticleReqFactory.ST_APPROVED)
						$scope.art.reviewedContent = $(artReviewedContentId).html();

					_updateArticle(function () { $state.go('app.articleinfo'); }, approve);
				}
			};
		}]);
})();