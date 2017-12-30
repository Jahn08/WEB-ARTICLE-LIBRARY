using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using WebArticleLibrary.Helpers;
using WebArticleLibrary.Models;
using WebArticleLibrary.Model;
using WebArticleLibrary.Hubs;

namespace WebArticleLibrary.Controllers
{
	public class ArticleController: ApiController
	{
		private ArticleLibraryContext _db;

		public ArticleController()
		{
			_db = new ArticleLibraryContext();
			_db.OnNotificationAddedEvent += OnNotificationAddedEvent;
		}

		private void OnNotificationAddedEvent(USER_NOTIFICATION[] etities)
		{
			NotificationHub.AddNotifications(etities);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && _db != null)
			{
				_db.OnNotificationAddedEvent -= OnNotificationAddedEvent;
				_db.Dispose();
			}

			base.Dispose(disposing);
		}

		const Int32 pageLength = 10;
		String[] defaultCategories = { "science", "politics", "literature", "travelling", "financies" };

		[HttpGet]
		[AllowAnonymous]
		public IHttpActionResult GetDefaultCategories()
		{
			return Ok(defaultCategories);
		}

		#region ARTICLE

		[HttpPost]
		public IHttpActionResult UpdateArticle(ARTICLE article)
		{
			ARTICLE curArt;
			var curUser = new UserStore(_db).GetCurrentUserInfo();
			Int32 curUserId = curUser.id;

			DateTime now = DateTime.Now;
			Boolean newArticle = article.ID == 0;

			if (newArticle)
			{				
				curArt = _db.ARTICLE.Add(_db.ARTICLE.Create());
				// Creating a new article, hence a current user is its author
				curArt.AUTHOR_ID = curUserId;
				curArt.INSERT_DATE = now;
			}
			else
			{
				curArt = _db.ARTICLE.FirstOrDefault(a => a.ID == article.ID);

				if (curArt == null)
					return BadRequest("The requested article does not exist in the data base");
			}

			ARTICLE_HISTORY history = null;

			if (!newArticle && curArt.NAME != article.NAME)
				history = AddHistory(curArt.ID, curUserId, "NAME", article.NAME, curArt.NAME, now);

			curArt.NAME = article.NAME;

			if (!newArticle && curArt.TAGS != article.TAGS)
				history = AddHistory(curArt.ID, curUserId, "TAGS", article.TAGS, curArt.TAGS, now);

			curArt.TAGS = article.TAGS;

			if (curArt.STATUS != ARTICLE_STATUS.ON_REVIEW && curArt.STATUS != ARTICLE_STATUS.ON_AMENDING)
			{
				if (!newArticle && !Enumerable.SequenceEqual(article.CONTENT, curArt.CONTENT))
					history = AddHistory(curArt.ID, curUserId, "CONTENT", null, null, now);

				curArt.CONTENT = article.CONTENT;
			}

			if (article.REVIEWED_CONTENT != null)
				curArt.REVIEWED_CONTENT = article.REVIEWED_CONTENT;

			if (!newArticle && curArt.DESCRIPTION != article.DESCRIPTION)
				history = AddHistory(curArt.ID, curUserId, "DESCRIPTION", article.DESCRIPTION, curArt.DESCRIPTION, now);

			curArt.DESCRIPTION = article.DESCRIPTION;

			Int32? assignedToId = null;

			if (!newArticle && curArt.STATUS != article.STATUS)
			{
				history = AddHistory(curArt.ID, curUserId, "STATUS", article.STATUS.ToString(),
						curArt.STATUS.ToString(), now);
				curArt.STATUS = article.STATUS;

				if (article.STATUS == ARTICLE_STATUS.APPROVED || article.STATUS == ARTICLE_STATUS.DRAFT)
				{
					if (curArt.AMENDMENTS.Any())
					{
						foreach (var am in curArt.AMENDMENTS)
						{
							am.ARCHIVED = true;
						}

						AddHistory(curArt.ID, curUserId, "AMENDMENTS.ARCHIVED", null, null, now);
					}

					curArt.REVIEWED_CONTENT = null;

					if (curArt.ASSIGNED_TO_ID.HasValue)
					{
						AddHistory(curArt.ID, curUserId, "ASSIGNED_TO", null,
							curArt.ASSIGNED_TO.LOGIN, now);

						assignedToId = curArt.ASSIGNED_TO_ID;
						curArt.ASSIGNED_TO_ID = null;
					}

					if (article.STATUS == ARTICLE_STATUS.APPROVED)
					{
						AddNotification($"An article '{curArt.NAME}' has been approved by {curUser.name} and published",
							history, true, curArt.AUTHOR_ID);
					}
					else
					{
						var recipients = new List<Int32>();
						recipients.Add(curArt.AUTHOR_ID);

						if (assignedToId.HasValue)
							recipients.Add(assignedToId.Value);

						AddNotification($"All ammendments related to an article '{curArt.NAME}' have been archived as it has been returned to the draft state",
							history, recipients.ToArray());
					}
				}
				else if (article.STATUS == ARTICLE_STATUS.ON_EDIT && curArt.ASSIGNED_TO_ID == curUserId)
					AddNotification($"An article '{curArt.NAME}' has been sent on edit by {curUser.name}", history, curArt.AUTHOR_ID);
				else if (article.STATUS == ARTICLE_STATUS.ON_REVIEW && curArt.AUTHOR_ID == curUserId)
				{
					var text = $"An article '{curArt.NAME}' has been sent on review by its author {curUser.name}";

					if (curArt.ASSIGNED_TO_ID.HasValue)
						AddNotification(text, history, curArt.ASSIGNED_TO_ID.Value);
					else
						AddNotification(text, history, true);
				}
				// It's considered to be a new article if it has just been sent - needs a notification
				else if (article.STATUS == ARTICLE_STATUS.CREATED)
					AddNotification($"{curUser.name} has added a new article '{curArt.NAME}'", history, true);
			}

			if (history != null && curArt.ASSIGNED_TO_ID != null)
			{
				AddNotification($"An article '{curArt.NAME}' has been changed by its author {curUser.name}",
					history, curArt.ASSIGNED_TO_ID.Value);
			}

			_db.SaveChanges();

			if (newArticle && article.STATUS != ARTICLE_STATUS.DRAFT)
			{
				history = AddHistory(curArt.ID, curUserId, "CREATED", null, null, null);
				curArt.STATUS = article.STATUS;

				AddNotification($"{curUser.name} has added a new article '{curArt.NAME}'", history, true);
				_db.SaveChanges();
			}

			return Ok();
		}

		[HttpGet]
		[CustomAuthorize(Roles = CustomAuthorizeAttribute.adminRole)]
		public IHttpActionResult SetArticleAssignment(Int32 id, Boolean assign)
		{
			ARTICLE curArt = _db.ARTICLE.FirstOrDefault(a => a.ID == id);

			if (curArt == null)
				return BadRequest("The requested article does not exist in the data base");

			var curUser = new UserStore(_db).GetCurrentUserInfo();
			var curUserId = curUser.id;

			const String eventName = "ASSIGNED_TO";

			if (assign)
			{
				curArt.ASSIGNED_TO_ID = curUserId;
				curArt.STATUS = ARTICLE_STATUS.ON_REVIEW;

				var history = AddHistory(id, curUserId, eventName, curUser.name, null);
				AddNotification($"{curUser.name} has signed up for reviewing an article '{curArt.NAME}'",
					history, true, curArt.AUTHOR_ID);
			}
			else
			{
				curArt.STATUS = ARTICLE_STATUS.CREATED;

				foreach (var am in curArt.AMENDMENTS.Where(a => a.AUTHOR_ID == curArt.ASSIGNED_TO_ID && !a.RESOLVED).ToArray())
					curArt.AMENDMENTS.Remove(am);

				var history = AddHistory(id, curUserId, eventName, null, curArt.ASSIGNED_TO.LOGIN);
				AddNotification($"{curUser.name} has backed out of reviewing an article '{curArt.NAME}'",
					history, true, curArt.AUTHOR_ID);

				curArt.ASSIGNED_TO_ID = null;
			}

			_db.SaveChanges();

			return Ok();
		}

		[HttpDelete]
		public IHttpActionResult RemoveArticle([ModelBinder]Int32[] ids)
		{
			var curUser = new UserStore(_db).GetCurrentUserInfo();
			var arts = _db.ARTICLE.Where(a => ids.Contains(a.ID));

			foreach (var curArt in arts)
			{
				if (curArt.AUTHOR_ID != curUser.id)
					return BadRequest("Only authors can delete their own articles");

				_db.ARTICLE.Remove(curArt);
			}

			_db.SaveChanges();

			return GetArticles();
		}


		[HttpGet]
		public IHttpActionResult GetArticles(ARTICLE_STATUS? status = null,
			String assignedTo = null, String author = null, String tags = null, String text = null,
			DateTime? dateStart = null, DateTime? dateEnd = null, Int32 page = 1,
			ColumnIndex colIndex = ColumnIndex.DATE, Boolean asc = false, Boolean includeComments = false)
		{
			var curUser = new UserStore(_db).GetCurrentUserInfo();
			var curUserId = curUser?.id;

			IQueryable<ARTICLE> articles = _db.ARTICLE.AsQueryable();

			if (dateStart != null)
			{
				var date = dateStart.Value;
				articles = articles.Where(a => a.INSERT_DATE >= date);
			}

			if (dateEnd != null)
			{
				var date = dateEnd.Value.Date.AddDays(1);
				articles = articles.Where(a => a.INSERT_DATE < date);
			}

			if (status != null)
			{
				articles = articles.Where(a => a.STATUS == status);
			}

			if (!String.IsNullOrEmpty(tags))
			{
				tags = tags.ToUpper();
				articles = articles.Where(a => a.TAGS.ToUpper().Contains(tags));
			}

			if (!String.IsNullOrEmpty(text))
			{
				text = text.ToUpper();
				articles = articles.Where(a => (a.NAME + a.DESCRIPTION).ToUpper().Contains(text));
			}

			if (assignedTo != null)
			{
				assignedTo = assignedTo.ToUpper();
				articles = articles.Where(a => a.ASSIGNED_TO_ID != null &&
					(a.ASSIGNED_TO.FIRST_NAME + a.ASSIGNED_TO.LAST_NAME + a.ASSIGNED_TO.PATRONYMIC_NAME)
						.ToUpper().Contains(assignedTo));
			}

			if (author != null)
			{
				author = author.ToUpper();
				var ids = _db.USER.Where(u => u.LOGIN.ToUpper().Contains(author)).Select(u => u.ID);
				articles = articles.Where(a => ids.Contains(a.AUTHOR_ID));
			}

			var privateArticles = articles.Where(a => a.AUTHOR_ID == curUserId);
			var privateData = OrderArticles(privateArticles, colIndex, asc).Select(a => new
			{
				id = a.ID,
				name = a.NAME,
				tags = a.TAGS,
				insertDate = a.INSERT_DATE,
				assignedToId = a.ASSIGNED_TO_ID,
				status = a.STATUS,
				description = a.DESCRIPTION
			}).ToArray();
			var privateDataCount = privateData.Count();
			var skip = (page - 1) * pageLength;
			privateData = privateData.Skip(skip).Take(pageLength).ToArray();

			var users = from u in _db.USER.Select(iu => new { iu.ID, iu.LOGIN })
						join a in privateData.Select(p => p.assignedToId).Distinct() on u.ID equals a
						select u;

			IEnumerable<dynamic> publicData = null;
			Int32 publicDataCount = 0;

			if (curUser.status == USER_STATUS.ADMINISTRATOR)
			{
				var publicArticles = articles.Where(a => a.AUTHOR_ID != curUserId &&
					a.STATUS != ARTICLE_STATUS.DRAFT &&
					a.STATUS != ARTICLE_STATUS.ON_EDIT);
				publicData = OrderArticles(publicArticles, colIndex, asc).Select(a => new
				{
					id = a.ID,
					name = a.NAME,
					tags = a.TAGS,
					insertDate = a.INSERT_DATE,
					assignedToId = a.ASSIGNED_TO_ID,
					authorId = a.AUTHOR_ID,
					status = a.STATUS,
					description = a.DESCRIPTION
				});
				publicDataCount = publicData.Count();
				publicData = publicData.Skip(skip).Take(pageLength).ToArray();

				users = users.Union(from u in _db.USER.Select(iu => new { iu.ID, iu.LOGIN })
									join a in publicData.SelectMany(p => new Int32[] { p.assignedToId ?? 0, p.authorId }).Distinct() on u.ID equals a
									select u);
			}

			var unionData = publicData == null ? privateData : privateData.Concat(publicData);
			var estimates = (from Int32 a in unionData.Select(d => d.id)
							 join est in _db.USER_ESTIMATE.Select(e => new { e.ARTICLE_ID, e.ESTIMATE }) on a equals est.ARTICLE_ID
							 select est).GroupBy(a => a.ARTICLE_ID);
			var commentNums = (from Int32 a in unionData.Select(d => d.id)
							   join est in _db.USER_COMMENT.Select(e => new { e.ARTICLE_ID }) on a equals est.ARTICLE_ID
							   select est).GroupBy(a => a.ARTICLE_ID);

			return Ok(new
			{
				privateData = privateData,
				privateDataCount = privateDataCount,
				publicData = publicData,
				publicDataCount = publicDataCount,
				pageLength = pageLength,
				userNames = users.ToDictionary(k => k.ID, v => v.LOGIN),
				estimates = estimates.ToDictionary(k => k.Key,
					v => v.Count(iv => iv.ESTIMATE == ESTIMATE_TYPE.POSITIVE) - v.Count(iv => iv.ESTIMATE == ESTIMATE_TYPE.NEGATIVE)),
				cmntNumber = commentNums.ToDictionary(k => k.Key, v => v.Count())
			});
		}

		[AllowAnonymous]
		[HttpGet]
		public IHttpActionResult SearchArticles(String author = null, String tags = null, String text = null,
			DateTime? dateStart = null, DateTime? dateEnd = null,
			Int32 page = 1, ColumnIndex colIndex = ColumnIndex.DATE, Boolean asc = false)
		{
			var curUser = new UserStore(_db).GetCurrentUserInfo();
			var curUserId = curUser?.id;

			IQueryable<ARTICLE> articles = _db.ARTICLE.Where(a => a.STATUS == ARTICLE_STATUS.APPROVED);

			if (dateStart != null)
			{
				var date = dateStart.Value;
				articles = articles.Where(a => a.INSERT_DATE >= date);
			}

			if (dateEnd != null)
			{
				var date = dateEnd.Value.AddDays(1);
				articles = articles.Where(a => a.INSERT_DATE < date);
			}

			if (!String.IsNullOrEmpty(tags))
			{
				tags = tags.ToUpper();
				articles = articles.Where(a => a.TAGS.ToUpper().Contains(tags));
			}

			if (!String.IsNullOrEmpty(text))
			{
				text = text.ToUpper();
				articles = articles.Where(a => (a.NAME + a.DESCRIPTION).ToUpper().Contains(text));
			}

			if (author != null)
			{
				author = author.ToUpper();
				var ids = _db.USER.Where(u => u.LOGIN.ToUpper().Contains(author)).Select(u => u.ID);
				articles = articles.Where(a => ids.Contains(a.AUTHOR_ID));
			}

			var data = OrderArticles(articles, colIndex, asc).Select(a => new
			{
				id = a.ID,
				name = a.NAME,
				tags = a.TAGS,
				insertDate = a.INSERT_DATE,
				status = a.STATUS,
				description = a.DESCRIPTION,
				authorId = a.AUTHOR_ID
			}).ToArray();
			var dataCount = data.Count();
			var skip = (page - 1) * pageLength;
			data = data.Skip(skip).Take(pageLength).ToArray();

			var users = from u in _db.USER.Select(iu => new { iu.ID, iu.LOGIN })
						join a in data.Select(p => p.authorId).Distinct() on u.ID equals a
						select u;

			var estimates = (from Int32 a in data.Select(d => d.id)
							 join est in _db.USER_ESTIMATE.Select(e => new { e.ARTICLE_ID, e.ESTIMATE }) on a equals est.ARTICLE_ID
							 select est).GroupBy(a => a.ARTICLE_ID);

			return Ok(new
			{
				articles = data,
				articleCount = dataCount,
				pageLength = pageLength,
				userNames = users.ToDictionary(k => k.ID, v => v.LOGIN),
				estimates = estimates.ToDictionary(k => k.Key,
					v => v.Count(iv => iv.ESTIMATE == ESTIMATE_TYPE.POSITIVE) - v.Count(iv => iv.ESTIMATE == ESTIMATE_TYPE.NEGATIVE))
			});
		}

		private IQueryable<ARTICLE> OrderArticles(IQueryable<ARTICLE> source, ColumnIndex colIndex, Boolean asc)
		{
			switch (colIndex)
			{
				case ColumnIndex.NAME:
					source = asc ? source.OrderBy(s => s.NAME) : source.OrderByDescending(s => s.NAME);
					break;
				case ColumnIndex.DATE:
					source = asc ? source.OrderBy(s => s.INSERT_DATE) : source.OrderByDescending(s => s.INSERT_DATE);
					break;
				case ColumnIndex.TAGS:
					source = asc ? source.OrderBy(s => s.TAGS) : source.OrderByDescending(s => s.TAGS);
					break;
				case ColumnIndex.AUTHOR:
					source = asc ? source.OrderBy(s => s.AUTHOR.LOGIN) : source.OrderByDescending(s => s.AUTHOR.LOGIN);
					break;
				case ColumnIndex.STATUS:
					source = asc ? source.OrderBy(s => s.STATUS) : source.OrderByDescending(s => s.STATUS);
					break;
				case ColumnIndex.ASSIGNED_TO:
					source = asc ? source.OrderBy(s => s.ASSIGNED_TO == null ? null : s.ASSIGNED_TO.LOGIN) :
						source.OrderByDescending(s => s.ASSIGNED_TO == null ? null : s.ASSIGNED_TO.LOGIN);
					break;
				case ColumnIndex.ID:
					source = asc ? source.OrderBy(s => s.ID) : source.OrderByDescending(s => s.ID);
					break;
				case ColumnIndex.TEXT:
					source = asc ? source.OrderBy(s => s.DESCRIPTION) : source.OrderByDescending(s => s.DESCRIPTION);
					break;
			}

			return source;
		}

		[HttpGet]
		public IHttpActionResult CreateArticleVersion(Int32 id)
		{
			var curArt = _db.ARTICLE.FirstOrDefault(a => a.ID == id);

			if (curArt == null)
				return BadRequest("The requested article does not exist in the data base");

			Int32 curUserId = new UserStore(_db).GetCurrentUserInfo().id;

			if (curArt.AUTHOR_ID != curUserId)
				return BadRequest("Only authors can create new versions of their articles");

			var newArt = _db.ARTICLE.Add(_db.ARTICLE.Create());

			DateTime now = DateTime.Now;
			newArt.INSERT_DATE = now;

			var name = curArt.NAME.Length > 150 ? curArt.NAME.Substring(0, 150) : curArt.NAME;
			newArt.NAME = name + "_" + Guid.NewGuid().ToString();
			newArt.CONTENT = curArt.CONTENT;
			newArt.AUTHOR_ID = curArt.AUTHOR_ID;
			newArt.TAGS = curArt.TAGS;
			newArt.DESCRIPTION = curArt.DESCRIPTION;

			_db.SaveChanges();

			AddHistory(curArt.ID, curUserId, "CHILD VERSION", null, null, now, newArt.ID);
			AddHistory(newArt.ID, curUserId, "PARENT VERSION", null, null, now, curArt.ID);

			_db.SaveChanges();

			return Ok(new { id = newArt.ID });
		}

		[HttpGet]
		[AllowAnonymous]
		public IHttpActionResult GetArticleTitles()
		{
			var articles = _db.ARTICLE.Where(art => art.STATUS == ARTICLE_STATUS.APPROVED &&
					defaultCategories.Any(c => art.TAGS.Contains(c)))
				.Select(art => new
				{
					id = art.ID,
					authorId = art.AUTHOR_ID,
					name = art.NAME,
					tags = art.TAGS,
					insertDate = art.INSERT_DATE,
					estimate = art.USER_ESTIMATES.Count(e => e.ESTIMATE == ESTIMATE_TYPE.POSITIVE) -
						art.USER_ESTIMATES.Count(e => e.ESTIMATE == ESTIMATE_TYPE.NEGATIVE)
				})
				.OrderByDescending(a => a.estimate)
				.ThenByDescending(a => a.insertDate)
				.Take(15).ToArray();

			var userNames = (from a in articles
							 join user in _db.USER.Where(u => u.STATUS != USER_STATUS.BANNED)
								 .Select(u => new { id = u.ID, login = u.LOGIN })
							 on a.authorId equals user.id
							 select user).Distinct().ToDictionary(k => k.id, v => v.login);

			return Ok(new
			{
				articles = articles,
				userNames = userNames
			});
		}

		[AllowAnonymous]
		[HttpGet]
		public IHttpActionResult ViewArticle(Int32 id, Int32? userId = null)
		{
			var article = _db.ARTICLE.FirstOrDefault(a => a.ID == id);
			var comments = userId.HasValue ? _db.USER_COMMENT.Where(c => c.ARTICLE_ID == article.ID)
				.OrderBy(c => c.INSERT_DATE).ToArray() : null;

			Dictionary<Int32, Byte[]> photos = new Dictionary<Int32, Byte[]>();
			Dictionary<Int32, String> names = new Dictionary<Int32, String>();

			if (comments != null)
			{
				var userInfos = (from u in _db.USER.Select(u => new
				{
					id = u.ID,
					photo = u.PHOTO,
					login = u.LOGIN
				})
					join c in comments.Select(ct => ct.AUTHOR_ID).Union(new[] { article.AUTHOR_ID })
						on u.id equals c
					select u).ToDictionary(k => k.id, v => new { photo = v.photo, login = v.login });

				foreach (var c in userInfos)
				{
					photos.Add(c.Key, c.Value.photo);
					names.Add(c.Key, c.Value.login);
				}
			}
			else
			{
				var _author = _db.USER.Where(a => a.ID == article.AUTHOR_ID)
					.Select(u => new
					{
						id = u.ID,
						photo = u.PHOTO,
						login = u.LOGIN
					}).FirstOrDefault();
				photos.Add(_author.id, _author.photo);
				names.Add(_author.id, _author.login);
			}

			return Ok(new
			{
				article = article,
				updatedDate = _db.ARTICLE_HISTORY.Where(h => h.ARTICLE_ID == article.ID)
					.Select(h => h.INSERT_DATE)
					.OrderByDescending(h => h)
					.FirstOrDefault(),
				comments = comments,
				userNames = names,
				userPhotos = photos,
				estimate = article.GetFinalEstimate(),
				curEstimate = _db.USER_ESTIMATE.Where(e => e.AUTHOR_ID == userId && e.ARTICLE_ID == id)
					.Select(e => e.ESTIMATE)
					.FirstOrDefault()
			});
		}

		#endregion

		#region COMMENT

		[HttpPost]
		public IHttpActionResult CreateComment(USER_COMMENT comment)
		{
			var articleId = comment.ARTICLE_ID;
			var article = _db.ARTICLE.FirstOrDefault(a => a.ID == articleId);

			if (article == null)
				return BadRequest("The requested article does not exist in the data base");

			// If a user can create a comment according to a current status of an article
			if (article.STATUS != ARTICLE_STATUS.APPROVED)
				return BadRequest("The article ought to be approved to have comments");

			var curUser = new UserStore(_db).GetCurrentUserInfo();
			var curUserId = curUser.id;

			var entity = _db.USER_COMMENT.Add(_db.USER_COMMENT.Create());
			entity.INSERT_DATE = DateTime.Now;
			entity.ARTICLE_ID = article.ID;
			entity.AUTHOR_ID = curUserId;
			entity.CONTENT = comment.CONTENT;
			entity.STATUS = COMMENT_STATUS.CREATED;
			entity.RESPONSE_TO_ID = comment.RESPONSE_TO_ID;

			_db.SaveChanges();

			var history = AddHistory(articleId, curUserId, "COMMENT CREATED", null, null, null, entity.ID);

			if (curUserId != article.AUTHOR_ID)
				AddNotification($"{curUser.name} has added a comment #{entity.ID.ToString()} for an article '{article.NAME}'",
					history, article.AUTHOR_ID);

			_db.SaveChanges();
			return Ok(entity);
		}

		[HttpGet]
		public IHttpActionResult UpdateCommentStatus(Int32 id, COMMENT_STATUS status)
		{
			var entity = _db.USER_COMMENT.FirstOrDefault(c => c.ID == id);

			if (entity == null)
				return BadRequest("The requested comment does not exist in the data base");

			var curUser = new UserStore(_db).GetCurrentUserInfo();

			var now = DateTime.Now;
			var history = AddHistory(entity.ARTICLE_ID, curUser.id, "COMMENT.STATUS", status.ToString(),
				entity.STATUS.ToString(), now, entity.ID);

			if (status == COMMENT_STATUS.DELETED)
			{
				if (entity.AUTHOR_ID != curUser.id)
					return BadRequest("A comment can only be removed by its author");

				var artName = entity.ARTICLE.NAME;
				List<Int32> participants = new List<Int32>();

				foreach (var cmpln in entity.USER_COMPLAINTS)
				{
					participants.Add(cmpln.AUTHOR_ID);

					if (cmpln.ASSIGNED_TO_ID.HasValue)
						participants.Add(cmpln.ASSIGNED_TO_ID.Value);
				}

				AddNotification($"A comment #{entity.ID.ToString()} for an article '{artName}' was removed by his author",
					history, true, participants.ToArray());

				// Doesn't have a cascade removal
				_db.USER_COMPLAINT.RemoveRange(entity.USER_COMPLAINTS);

				if (entity.RELATED_COMMENTS.Any())
				{
					entity.STATUS = COMMENT_STATUS.DELETED;
				}
				else
				{
					RemoveComment(entity, now);
				}
			}
			else
			{
				if (curUser.status != USER_STATUS.ADMINISTRATOR)
					return BadRequest("You does not have rights to change statuses of comments");

				var art = entity.ARTICLE;

				var participants = new List<Int32>();
				participants.Add(entity.AUTHOR_ID);

				if (entity.AUTHOR_ID != art.AUTHOR_ID)
					participants.Add(art.AUTHOR_ID);

				AddNotification($"The administrator {curUser.name} has blocked a comment #{entity.ID.ToString()} for an article '{art.NAME}'",
					history, true, participants.ToArray());

				entity.STATUS = status;
			}

			_db.SaveChanges();

			return Ok();
		}

		[HttpGet]
		public IHttpActionResult GetComments(Int32 page = 1, ColumnIndex colIndex = ColumnIndex.DATE,
			Boolean asc = false, String id = null, String articleName = null,
			DateTime? dateStart = null, DateTime? dateEnd = null, COMMENT_STATUS? status = null,
			Int32? userId = null, Int32? parentId = null, Boolean all = false)
		{
			IQueryable<USER_COMMENT> cmnts = _db.USER_COMMENT
				.Where(c => c.ARTICLE.STATUS == ARTICLE_STATUS.APPROVED);

			if (userId != null)
			{
				cmnts = cmnts.Where(c => c.AUTHOR_ID == userId);
			}

			if (parentId != null)
			{
				cmnts = cmnts.Where(c => c.RESPONSE_TO_ID == parentId);
			}

			if (dateStart != null)
			{
				cmnts = cmnts.Where(c => c.INSERT_DATE >= dateStart);
			}

			if (dateEnd != null)
			{
				var filterDate = dateEnd.Value.Date.AddDays(1);
				cmnts = cmnts.Where(c => c.INSERT_DATE < filterDate);
			}

			if (status != null)
			{
				cmnts = cmnts.Where(c => c.STATUS == status);
			}

			if (id != null)
			{
				cmnts = cmnts.Where(c => c.ID.ToString().Contains(id));
			}

			if (articleName != null)
			{
				var filterVal = articleName.ToUpper();
				cmnts = cmnts.Where(c => c.ARTICLE.NAME.ToUpper().Contains(filterVal));
			}

			Int32 dataCount = cmnts.Count();

			cmnts = OrderComments(cmnts, colIndex, asc);
			USER_COMMENT[] cmntData = (all ? cmnts : cmnts.Skip((page - 1) * pageLength).Take(pageLength)).ToArray();

			return Ok(new
			{
				data = cmntData,
				dataCount = dataCount,
				articleNames = cmntData.Select(c => new
				{
					artId = c.ARTICLE.ID,
					artName = c.ARTICLE.NAME
				}).Distinct().ToDictionary(k => k.artId, v => v.artName),
				userNames = cmntData.Select(c => new
				{
					authorId = c.AUTHOR_ID,
					authorLogin = c.AUTHOR.LOGIN,
				}).Distinct().ToDictionary(k => k.authorId, v => v.authorLogin),
				complaintNumber = cmntData
					.Where(c => c.USER_COMPLAINTS.Any()).ToDictionary(k => k.ID, v => v.USER_COMPLAINTS.Count),
				relatedCmntNumber = cmntData
					.Where(c => c.RELATED_COMMENTS.Any()).ToDictionary(k => k.ID, v => v.RELATED_COMMENTS.Count),
				pageLength = pageLength
			});
		}

		private IQueryable<USER_COMMENT> OrderComments(IQueryable<USER_COMMENT> source, ColumnIndex colIndex, Boolean asc)
		{
			switch (colIndex)
			{
				case ColumnIndex.DATE:
					source = asc ? source.OrderBy(s => s.INSERT_DATE) : source.OrderByDescending(s => s.INSERT_DATE);
					break;
				case ColumnIndex.STATUS:
					source = asc ? source.OrderBy(s => s.STATUS) : source.OrderByDescending(s => s.STATUS);
					break;
				case ColumnIndex.ID:
					source = asc ? source.OrderBy(s => s.ID) : source.OrderByDescending(s => s.ID);
					break;
				case ColumnIndex.ARTICLE:
					source = asc ? source.OrderBy(s => s.ARTICLE.NAME) : source.OrderByDescending(s => s.ARTICLE.NAME);
					break;
			}

			return source;
		}

		private void RemoveComment(USER_COMMENT cmnt, DateTime now)
		{
			var parentId = cmnt.RESPONSE_TO_ID;

			AddHistory(cmnt.ARTICLE_ID, cmnt.AUTHOR_ID, "COMMENT REMOVED", null, null,
				now, cmnt.ID);
			_db.USER_COMMENT.Remove(cmnt);
				
			if (parentId != null)
			{
				// There might be other parental comments waited to be deleted
				USER_COMMENT parentalEntity = _db.USER_COMMENT.FirstOrDefault(c => c.ID == parentId);

				if (parentalEntity != null && parentalEntity.STATUS == COMMENT_STATUS.DELETED &&
					!parentalEntity.RELATED_COMMENTS.Any())
					RemoveComment(parentalEntity, now);
			}
		}

		#endregion

		#region COMPLAINT

		[HttpPost]
		public IHttpActionResult CreateComplaint(USER_COMPLAINT complaint)
		{
			var curUser = new UserStore(_db).GetCurrentUserInfo();
			var curUserId = curUser.id;

			var entity = _db.USER_COMPLAINT.Add(_db.USER_COMPLAINT.Create());
			Int32 articleId;
			String notificationStr = null;

			var recipients = new List<Int32>();

			if (complaint.USER_COMMENT_ID.HasValue)
			{
				var commentId = complaint.USER_COMMENT_ID;
				var comment = _db.USER_COMMENT.FirstOrDefault(a => a.ID == commentId);

				if (comment == null)
					return BadRequest("The requested comment does not exist in the data base");

				var article = comment.ARTICLE;

				// If a user can create a comment according to a current status of an article
				if (article.STATUS != ARTICLE_STATUS.APPROVED)
					return BadRequest("The article ought to be approved to have complaints");

				entity.USER_COMMENT_ID = commentId;
				entity.ARTICLE_ID = articleId = article.ID;

				recipients.Add(article.AUTHOR_ID);

				if (comment.AUTHOR_ID != article.AUTHOR_ID)
					recipients.Add(comment.AUTHOR_ID);

				notificationStr = $"{curUser.name} has complained about a comment #{commentId} for an article '{article.NAME}'";
			}
			else
			{
				articleId = complaint.ARTICLE_ID;
				var article = _db.ARTICLE.FirstOrDefault(a => a.ID == articleId);

				if (article == null)
					return BadRequest("The requested article does not exist in the data base");

				// If a user can create a comment according to a current status of an article
				if (article.STATUS != ARTICLE_STATUS.APPROVED)
					return BadRequest("The article ought to be approved to have complaints");

				entity.ARTICLE_ID = articleId;

				recipients.Add(article.AUTHOR_ID);
				notificationStr = $"{curUser.name} has complained about an article '{article.NAME}'";
			}

			entity.INSERT_DATE = DateTime.Now;
			entity.AUTHOR_ID = curUserId;
			entity.TEXT = complaint.TEXT;
			entity.STATUS = COMPLAINT_STATUS.CREATED;

			_db.SaveChanges();

			var history = AddHistory(articleId, curUserId, "COMPLAINT CREATED", null, null, null, entity.ID);
			AddHistory(articleId, curUserId, "COMPLAINT.CONTENT", entity.TEXT, null, null, entity.ID);

			AddNotification(notificationStr, history, true, recipients.ToArray());

			_db.SaveChanges();
			return Ok(entity);
		}

		[HttpGet]
		[CustomAuthorize(Roles = CustomAuthorizeAttribute.adminRole)]
		public IHttpActionResult SetComplaintStatus(Int32 id, COMPLAINT_STATUS status, String response)
		{
			var entity = _db.USER_COMPLAINT.FirstOrDefault(c => c.ID == id);

			if (entity == null)
				return BadRequest("The requested complaint does not exist in the data base");

			var curUser = new UserStore(_db).GetCurrentUserInfo();
			var now = DateTime.Now;

			String notificationStr = null;
			var recipients = new List<Int32>();

			var art = entity.ARTICLE;

			if (status == COMPLAINT_STATUS.APPROVED)
			{
				if (entity.USER_COMMENT_ID.HasValue)
				{
					var cmnt = entity.USER_COMMENT;
					cmnt.STATUS = COMMENT_STATUS.BLOCKED;

					notificationStr = $"A complaint about a comment #{entity.USER_COMMENT_ID.Value.ToString()}" +
						$" for an article '{art.NAME}' has been approved by {curUser.name} and the comment has been blocked" +
						$" with a response: '{response}'";

					recipients.Add(cmnt.AUTHOR_ID);
					recipients.Add(entity.AUTHOR_ID);

					if (entity.AUTHOR_ID != art.AUTHOR_ID && cmnt.AUTHOR_ID != art.AUTHOR_ID)
						recipients.Add(art.AUTHOR_ID);
				}
				else
				{
					art.STATUS = ARTICLE_STATUS.ON_EDIT;

					AMENDMENT amd;
					art.AMENDMENTS.Add(amd = _db.AMENDMENTS.Create());
					art.ASSIGNED_TO_ID = curUser.id;

					amd.AUTHOR_ID = curUser.id;
					amd.CONTENT = response;
					amd.INSERT_DATE = now;

					_db.SaveChanges();

					AddHistory(entity.ARTICLE_ID, curUser.id, "AMENDMENT CREATED", null, null, null, amd.ID);
					AddHistory(entity.ARTICLE_ID, curUser.id, "AMENDMENT.CONTENT", amd.CONTENT, null, null, amd.ID);

					notificationStr = $"A complaint about an article '{art.NAME}' has been approved" +
						$" by {curUser.name} and the article has been sent to be edited";

					recipients.Add(entity.AUTHOR_ID);
					recipients.Add(art.AUTHOR_ID);
				}
			}
			else
			{
				if (entity.USER_COMMENT_ID.HasValue)
				{
					var cmnt = entity.USER_COMMENT;

					recipients.Add(cmnt.AUTHOR_ID);
					recipients.Add(entity.AUTHOR_ID);

					if (entity.AUTHOR_ID != art.AUTHOR_ID && cmnt.AUTHOR_ID != art.AUTHOR_ID)
						recipients.Add(art.AUTHOR_ID);

					notificationStr = $"A complaint about a comment #{entity.USER_COMMENT_ID.Value.ToString()}" +
						$" for an article '{art.NAME}' has been declined by {curUser.name} with a response: '{response}'";
				}
				else
				{
					recipients.Add(entity.AUTHOR_ID);
					recipients.Add(art.AUTHOR_ID);

					notificationStr = $"A complaint about an article '{art.NAME}' has been declined" +
						$" by {curUser.name} with a response: '{response}'";
				}
			}

			var history = AddHistory(entity.ARTICLE_ID, curUser.id,
				"COMPLAINT.STATUS", status.ToString(), entity.STATUS.ToString(), now, entity.ID);
			AddHistory(entity.ARTICLE_ID, curUser.id, "COMPLAINT.RESPONSE", response,
				null, now, entity.ID);

			AddNotification(notificationStr, history, recipients.ToArray());

			entity.STATUS = status;

			AddHistory(entity.ARTICLE_ID, curUser.id, "COMPLAINT.ASSIGNED_TO", null,
				entity.ASSIGNED_TO.LOGIN, now, entity.ID);

			entity.ASSIGNED_TO_ID = null;
			_db.SaveChanges();

			return Ok();
		}

		[HttpGet]
		[CustomAuthorize(Roles = CustomAuthorizeAttribute.adminRole)]
		public IHttpActionResult SetComplaintAssignment(Int32 id, Boolean assign)
		{
			var entity = _db.USER_COMPLAINT.FirstOrDefault(a => a.ID == id);

			if (entity == null)
				return BadRequest("The requested complaint does not exist in the data base");

			var curUser = new UserStore(_db).GetCurrentUserInfo();
			var curUserId = curUser.id;

			const String eventName = "COMPLAINT.ASSIGNED_TO";
			var now = DateTime.Now;

			ARTICLE_HISTORY history;
			List<Int32> recipients = new List<Int32>();

			String notificationStr = "A complaint about ";

			var art = entity.ARTICLE;

			if (entity.USER_COMMENT_ID.HasValue)
				notificationStr += $"a comment #{entity.USER_COMMENT_ID.Value.ToString()} for ";

			notificationStr += $"an article '{art.NAME}' ";

			if (assign)
			{
				entity.ASSIGNED_TO_ID = curUserId;
				history = AddHistory(entity.ARTICLE_ID, curUserId, eventName, curUser.name, null,
					now, entity.ID);
				notificationStr += $"has been assigned to {curUser.name}";
			}
			else
			{
				history = AddHistory(entity.ARTICLE_ID, curUserId, eventName, null, entity.ASSIGNED_TO.LOGIN,
					now, entity.ID);
				entity.ASSIGNED_TO_ID = null;
				notificationStr += $"has been removed from the {curUser.name}'s assignments";
			}

			recipients.Add(entity.AUTHOR_ID);

			if (entity.USER_COMMENT_ID.HasValue)
			{
				Int32 cmntAuthorId = entity.USER_COMMENT.AUTHOR_ID;
				recipients.Add(cmntAuthorId);

				Int32 artAuthorId = art.AUTHOR_ID;
				if (entity.AUTHOR_ID != artAuthorId && cmntAuthorId != artAuthorId)
					recipients.Add(artAuthorId);
			}
			else
			{
				recipients.Add(entity.ARTICLE.AUTHOR_ID);
			}

			AddNotification(notificationStr, history, true, recipients.ToArray());
			_db.SaveChanges();

			return Ok();
		}

		[HttpGet]
		[CustomAuthorize(Roles = CustomAuthorizeAttribute.adminRole)]
		public IHttpActionResult GetComplaints(Int32 page = 1, ColumnIndex colIndex = ColumnIndex.DATE,
			Boolean asc = false, String text = null, COMPLAINT_STATUS? status = null,
			String author = null, String assignedTo = null,
			DateTime? dateStart = null, DateTime? dateEnd = null,
			ComplaintEntityType? entityType = null, String entity = null)
		{
			var curUser = new UserStore(_db).GetCurrentUserInfo();
			IQueryable<USER_COMPLAINT> data = _db.USER_COMPLAINT
				.Where(c => c.ARTICLE.STATUS == ARTICLE_STATUS.APPROVED);

			if (dateStart != null)
			{
				var filter = dateStart.Value.Date;
				data = data.Where(c => c.INSERT_DATE > filter);
			}

			if (dateEnd != null)
			{
				var filter = dateEnd.Value.Date.AddDays(1);
				data = data.Where(c => c.INSERT_DATE < filter);
			}

			if (status != null)
			{
				data = data.Where(c => c.STATUS == status);
			}

			if (entityType != null)
			{
				if (entityType == ComplaintEntityType.ARTICLE)
					data = data.Where(c => c.USER_COMMENT_ID == null);
				else
					data = data.Where(c => c.USER_COMMENT_ID != null);
			}

			if (entity != null)
			{
				var filter = entity.ToUpper();
				data = data.Where(c => (c.USER_COMMENT_ID == null ? c.ARTICLE.NAME.ToUpper() :
					c.USER_COMMENT_ID.ToString()).Contains(filter));
			}

			if (assignedTo != null)
			{
				var filter = assignedTo.ToUpper();
				data = data.Where(c => c.ASSIGNED_TO_ID != null && c.ASSIGNED_TO.LOGIN.ToUpper().Contains(filter));
			}

			if (author != null)
			{
				var filter = author.ToUpper();
				data = data.Where(c => c.AUTHOR.LOGIN.ToUpper().Contains(filter));
			}

			if (text != null)
			{
				var filter = text.ToUpper();
				data = data.Where(c => c.TEXT.ToUpper().Contains(filter));
			}

			Int32 dataCount = data.Count();
			var cmplns = OrderComplaints(data, colIndex, asc)
				.Skip((page - 1) * pageLength)
				.Take(pageLength);

			var users = from u in _db.USER.Select(iu => new { iu.ID, iu.LOGIN })
						join a in cmplns.SelectMany(p => new Int32[] { p.ASSIGNED_TO_ID ?? 0, p.AUTHOR_ID }).Distinct() on u.ID equals a
						select u;

			var articles = from a in _db.ARTICLE.Select(a => new { a.ID, a.NAME })
						   join c in cmplns.Select(p => p.ARTICLE_ID).Distinct() on a.ID equals c
						   select a;

			var comments = from e in _db.USER_COMMENT.Select(d => new { d.ID, d.CONTENT })
						   join c in cmplns.Where(p => p.USER_COMMENT_ID != null)
								.Select(p => p.USER_COMMENT_ID).Distinct() on e.ID equals c
						   select e;

			return Ok(new
			{
				data = cmplns.Select(c => new
				{
					authorId = c.AUTHOR_ID,
					assignedToId = c.ASSIGNED_TO_ID,
					articleId = c.ARTICLE_ID,
					insertDate = c.INSERT_DATE,
					id = c.ID,
					status = c.STATUS,
					text = c.TEXT,
					userCommentId = c.USER_COMMENT_ID,
					cmntAuthorId = c.USER_COMMENT == null ? null : (Int32?)c.USER_COMMENT.AUTHOR_ID,
					articleAuthorId = c.ARTICLE.AUTHOR_ID
				}).ToArray(),
				dataCount = dataCount,
				userNames = users.ToDictionary(k => k.ID, v => v.LOGIN),
				articleNames = articles.ToDictionary(k => k.ID, v => v.NAME),
				comments = comments.ToDictionary(k => k.ID, v => v.CONTENT),
				pageLength = pageLength
			});
		}

		private IQueryable<USER_COMPLAINT> OrderComplaints(IQueryable<USER_COMPLAINT> source, ColumnIndex colIndex, Boolean asc)
		{
			switch (colIndex)
			{
				case ColumnIndex.TEXT:
					source = asc ? source.OrderBy(s => s.TEXT) : source.OrderByDescending(s => s.TEXT);
					break;
				case ColumnIndex.DATE:
					source = asc ? source.OrderBy(s => s.INSERT_DATE) : source.OrderByDescending(s => s.INSERT_DATE);
					break;
				case ColumnIndex.AUTHOR:
					source = asc ? source.OrderBy(s => s.AUTHOR.LOGIN) : source.OrderByDescending(s => s.AUTHOR.LOGIN);
					break;
				case ColumnIndex.STATUS:
					source = asc ? source.OrderBy(s => s.STATUS) : source.OrderByDescending(s => s.STATUS);
					break;
				case ColumnIndex.ASSIGNED_TO:
					source = asc ? source.OrderBy(s => s.ASSIGNED_TO == null ? null : s.ASSIGNED_TO.LOGIN) :
						source.OrderByDescending(s => s.ASSIGNED_TO == null ? null : s.ASSIGNED_TO.LOGIN);
					break;
				case ColumnIndex.ID:
					source = asc ? source.OrderBy(s => s.USER_COMMENT_ID == null ? s.USER_COMMENT_ID : s.ARTICLE_ID) :
						source.OrderByDescending(s => s.USER_COMMENT_ID == null ? s.USER_COMMENT_ID : s.ARTICLE_ID);
					break;
			}

			return source;
		}

		#endregion

		#region ESTIMATE

		[HttpGet]
		public IHttpActionResult GetEstimates(Int32 page = 1, ColumnIndex colIndex = ColumnIndex.DATE,
			Boolean asc = false, Int32? userId = null,
			ESTIMATE_TYPE? estimate = null, String article = null,
			DateTime? dateStart = null, DateTime? dateEnd = null)
		{
			var curUser = new UserStore(_db).GetCurrentUserInfo();
			IQueryable<USER_ESTIMATE> data = _db.USER_ESTIMATE.Where(c => (userId == null || c.AUTHOR_ID == userId))
				.Where(e => e.ARTICLE.STATUS == ARTICLE_STATUS.APPROVED);

			if (dateStart != null)
			{
				var filter = dateStart.Value.Date;
				data = data.Where(c => c.INSERT_DATE > filter);
			}

			if (dateEnd != null)
			{
				var filter = dateEnd.Value.Date.AddDays(1);
				data = data.Where(c => c.INSERT_DATE < filter);
			}

			if (estimate != null)
			{
				data = data.Where(c => c.ESTIMATE == estimate);
			}

			if (article != null)
			{
				var filter = article.ToUpper();
				data = data.Where(c => c.ARTICLE.NAME.ToUpper().Contains(filter));
			}

			Int32 dataCount = data.Count();
			var ests = OrderEstimates(data, colIndex, asc)
				.Skip((page - 1) * pageLength).Take(pageLength).ToArray();

			return Ok(new
			{
				data = ests,
				dataCount = dataCount,
				articleNames = ests.Select(c => c.ARTICLE).Distinct().ToDictionary(k => k.ID, v => v.NAME),
				userNames = ests.Select(c => c.AUTHOR).Distinct().ToDictionary(k => k.ID, v => v.LOGIN),
				pageLength = pageLength
			});
		}
		
		private IQueryable<USER_ESTIMATE> OrderEstimates(IQueryable<USER_ESTIMATE> source, ColumnIndex colIndex, Boolean asc)
		{
			switch (colIndex)
			{
				case ColumnIndex.DATE:
					source = asc ? source.OrderBy(s => s.INSERT_DATE) : source.OrderByDescending(s => s.INSERT_DATE);
					break;
				case ColumnIndex.STATUS:
					source = asc ? source.OrderByDescending(s => s.ESTIMATE) : source.OrderBy(s => s.ESTIMATE);
					break;
				case ColumnIndex.ID:
					source = asc ? source.OrderBy(s => s.ID) : source.OrderByDescending(s => s.ID);
					break;
				case ColumnIndex.ARTICLE:
					source = asc ? source.OrderBy(s => s.ARTICLE.NAME) : source.OrderByDescending(s => s.ARTICLE.NAME);
					break;
			}

			return source;
		}

		[HttpGet]
		public IHttpActionResult AssessArticle(Int32 id, ESTIMATE_TYPE estimate)
		{
			var article = _db.ARTICLE.FirstOrDefault(a => a.ID == id);

			if (article == null)
				return BadRequest("The requested article does not exist in the data base");

			// If a user can create a comment according to a current status of an article
			if (article.STATUS != ARTICLE_STATUS.APPROVED)
				return BadRequest("The article ought to be approved to have comments");

			var curUser = new UserStore(_db).GetCurrentUserInfo();
			var curUserId = curUser.id;

			var entity = _db.USER_ESTIMATE.FirstOrDefault(e => e.ARTICLE_ID == id && e.AUTHOR_ID == curUserId);
			DateTime now = DateTime.Now;

			if (entity == null)
			{
				entity = _db.USER_ESTIMATE.Add(_db.USER_ESTIMATE.Create());
				entity.ARTICLE_ID = article.ID;
				entity.AUTHOR_ID = curUserId;
			}

			var history = AddHistory(id, curUserId, "ESTIMATE.ESTIMATE", estimate.ToString(), entity.ESTIMATE.ToString(),
				now, entity.ID);
			entity.ESTIMATE = estimate;
			entity.INSERT_DATE = DateTime.Now;

			AddNotification($"A user {curUser.name} has assessed an article '{article.NAME}'", history, article.AUTHOR_ID);

			_db.SaveChanges();

			return Ok(new { estimate = article.GetFinalEstimate() });
		}

		#endregion

		#region AMENDMENT

		[HttpGet]
		public IHttpActionResult GetAmendments(Int32 articleId)
		{
			var article = _db.ARTICLE.FirstOrDefault(a => a.ID == articleId);

			if (article == null)
				return BadRequest("The requested article does not exist in the data base");

			var curUser = new UserStore(_db).GetCurrentUserInfo();

			// If the user has the rights to get amendments
			if (article.AUTHOR_ID != curUser.id && article.ASSIGNED_TO_ID != curUser.id)
				return BadRequest("The user cannot see requested amendments");

			return Ok(article.AMENDMENTS);
		}

		[HttpPost]
		[CustomAuthorize(Roles = CustomAuthorizeAttribute.adminRole)]
		public IHttpActionResult CreateAmendment(AMENDMENT amendment)
		{
			var articleId = amendment.ARTICLE_ID;
			var article = _db.ARTICLE.FirstOrDefault(a => a.ID == articleId);

			if (article == null)
				return BadRequest("The requested article does not exist in the data base");

			var curUser = new UserStore(_db).GetCurrentUserInfo();

			// If the user has the rights to create amendments
			if (article.AUTHOR_ID != curUser.id && article.ASSIGNED_TO_ID != curUser.id)
				return BadRequest("The user cannot create amendments");

			var entity = _db.AMENDMENTS.Add(_db.AMENDMENTS.Create());
			entity.INSERT_DATE = DateTime.Now;
			entity.ARTICLE_ID = article.ID;
			entity.AUTHOR_ID = curUser.id;
			entity.CONTENT = amendment.CONTENT;
			entity.RESOLVED = amendment.RESOLVED;

			_db.SaveChanges();

			var history = AddHistory(amendment.ARTICLE_ID, curUser.id, "AMENDMENT CREATED", null, null, null, entity.ID);
			AddHistory(amendment.ARTICLE_ID, curUser.id, "AMENDMENT.CONTENT", entity.CONTENT, null, null, entity.ID);

			AddNotification($"An amendment has been added to an article '{article.NAME}' by {curUser.name}",
				history, article.AUTHOR_ID);

			_db.SaveChanges();

			return Ok(entity);
		}

		[HttpPost]
		public IHttpActionResult UpdateAmendment(AMENDMENT[] amendments)
		{
			if (!amendments.Any())
				throw new Exception("The request is empty");

			var articleId = amendments.First().ARTICLE_ID;
			var article = _db.ARTICLE.FirstOrDefault(a => a.ID == articleId);

			if (article == null)
				return BadRequest("The requested article does not exist in the data base");

			var curUser = new UserStore(_db).GetCurrentUserInfo();

			// If the user has the rights to get amendments
			if (article.AUTHOR_ID != curUser.id && article.ASSIGNED_TO_ID != curUser.id)
				return BadRequest("The user cannot deal with these amendments");

			var ids = amendments.Select(am => am.ID).ToArray();
			DateTime now = DateTime.Now;

			ARTICLE_HISTORY history = null;

			foreach (var entity in _db.AMENDMENTS.Where(am => ids.Contains(am.ID)))
			{
				var amendment = amendments.First(am => am.ID == entity.ID);

				if (entity.CONTENT != amendment.CONTENT)
					history = AddHistory(amendment.ARTICLE_ID, curUser.id, "AMENDMENT.CONTENT",
						amendment.CONTENT, entity.CONTENT, now, amendment.ID);

				entity.CONTENT = amendment.CONTENT;

				if (entity.RESOLVED != amendment.RESOLVED)
					history = AddHistory(amendment.ARTICLE_ID, curUser.id, "AMENDMENT.RESOLVED",
						null, null, now, amendment.ID);

				entity.RESOLVED = amendment.RESOLVED;
			}

			if (history != null)
			{
				var recipient = curUser.id == article.AUTHOR_ID ?
					(article.ASSIGNED_TO_ID.HasValue ? article.ASSIGNED_TO_ID : null) : article.AUTHOR_ID;

				if (recipient.HasValue)
					AddNotification($"There have been changes to the amendments of an article '{article.NAME}' by {curUser.name}",
						history, recipient.Value);
			}

			_db.SaveChanges();
			return Ok();
		}

		[HttpDelete]
		[CustomAuthorize(Roles = CustomAuthorizeAttribute.adminRole)]
		public IHttpActionResult RemoveAmendment([ModelBinder]Int32[] ids)
		{
			var amendments = _db.AMENDMENTS.Where(a => ids.Contains(a.ID));
			DateTime now = DateTime.Now;

			ARTICLE_HISTORY history = null;
			ARTICLE art = null;

			var curUser = new UserStore(_db).GetCurrentUserInfo();

			foreach (var amendment in amendments)
			{
				if (art == null)
					art = amendment.ARTICLE;

				if (curUser.status == USER_STATUS.ADMINISTRATOR && amendment.AUTHOR_ID != curUser.id)
					return BadRequest("Only authors can delete their own amendments");

				history = AddHistory(amendment.ARTICLE_ID, curUser.id, "AMENDMENT REMOVED", null, null,
					now, amendment.ID);
				_db.AMENDMENTS.Remove(amendment);
			}

			if (history != null)
			{
				AddNotification($"Some amendments for an article '{art.NAME}' have been removed  by {curUser.name}",
					history, art.AUTHOR_ID);
			}

			_db.SaveChanges();
			return Ok();
		}

		#endregion

		#region HISTORY

		private void AddNotification(String text, ARTICLE_HISTORY history, 
			Boolean includeAdmins, params Int32[] recipientIds)
		{
			if (includeAdmins)
			{
				recipientIds = _db.USER.Where(u => u.STATUS == USER_STATUS.ADMINISTRATOR)
					.Select(u => u.ID).Union(recipientIds).ToArray();
			}

			var curUserId = new UserStore(_db).GetCurrentUserInfo().id;
			List<USER_NOTIFICATION> notifications = new List<USER_NOTIFICATION>();

			foreach (var recipientId in recipientIds.Distinct())
			{
				if (recipientId != curUserId)
				{
					var notification = _db.USER_NOTIFICATION.Create();

					notification.TEXT = text;
					notification.RECIPIENT_ID = recipientId;
					notification.INSERT_DATE = DateTime.Now;
					notification.ARTICLE_HISTORY = history;

					notifications.Add(notification);
				}
			}

			_db.USER_NOTIFICATION.AddRange(notifications);
		}

		private void AddNotification(String text, ARTICLE_HISTORY history, 
			params Int32[] recipientIds)
		{
			if (recipientIds.Any())
				AddNotification(text, history, false, recipientIds);
		}

		[HttpGet]
		public IHttpActionResult GetNotifications(Int32 userId)
		{
			var data = _db.USER_NOTIFICATION.Where(a => a.RECIPIENT_ID == userId);

			return Ok(data.OrderByDescending(h => h.INSERT_DATE).Select(n => new
			{
				id = n.ID,
				date = n.INSERT_DATE,
				recipientID = n.RECIPIENT_ID,
				text = n.TEXT,
				historyId = n.ARTICLE_HISTORY_ID,
				articleId = n.ARTICLE_HISTORY.ARTICLE_ID
			}).ToArray());
		}

		[HttpDelete]
		public IHttpActionResult ClearNotifications([ModelBinder]Int32[] ids)
		{
			var data = _db.USER_NOTIFICATION.Where(a => ids.Contains(a.ID));
			var removedData = _db.USER_NOTIFICATION.RemoveRange(data).ToArray();

			_db.SaveChanges();

			NotificationHub.RemoveNotifications(removedData.ToArray());
			return Ok();
		}

		[HttpGet]
		public IHttpActionResult GetArticleHistory(Int32 id)
		{
			var curArt = _db.ARTICLE.FirstOrDefault(a => a.ID == id);

			if (curArt == null)
				return BadRequest("The requested article does not exist in the data base");

			return Ok(curArt.ARTICLE_HISTORY
				.GroupBy(h => h.INSERT_DATE).OrderByDescending(h => h.Key)
				.Select(g => new
				{
					date = g.Key,
					author = g.First().AUTHOR.LOGIN,
					history = g
				}).ToArray());
		}
		
		private ARTICLE_HISTORY AddHistory(Int32 artId, Int32 authorId, String obj, 
			String newVal, String oldVal, DateTime? insDate = null, Int32? objId = null)
		{
			var history = _db.ARTICLE_HISTORY.Create();

			history.ARTICLE_ID = artId;
			history.AUTHOR_ID = authorId;
			history.OBJECT = obj;
			history.NEW_VALUE = newVal;
			history.OLD_VALUE = oldVal;
			history.OBJECT_ID = objId;

			var date = (insDate ?? DateTime.Now);
			history.INSERT_DATE = new DateTime(date.Year, date.Month, date.Day, 
				date.Hour, date.Minute, date.Second);

			return _db.ARTICLE_HISTORY.Add(history);
		}

		#endregion
	}
}