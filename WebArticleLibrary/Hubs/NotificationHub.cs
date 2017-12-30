using System;
using System.Collections.Concurrent;
using System.Linq;
using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;
using WebArticleLibrary.Model;

namespace WebArticleLibrary.Hubs
{
	public class NotificationHub : Hub
	{
		static ConcurrentDictionary<String, Int32> storage = 
			new ConcurrentDictionary<String, Int32>();

		public static void AddNotifications(USER_NOTIFICATION[] entities)
		{
			foreach (var g in entities.GroupBy(e => e.RECIPIENT_ID))
			{
				var keys = storage.Where(s => s.Value == g.Key)
					.Select(s => s.Key).ToArray();

				if (keys.Any())
				{
					var context = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
					context.Clients.Clients(keys).notify(g.Select(n => new
					{
						id = n.ID,
						date = n.INSERT_DATE,
						recipientID = n.RECIPIENT_ID,
						text = n.TEXT,
						historyId = n.ARTICLE_HISTORY_ID,
						articleId = n.ARTICLE_HISTORY.ARTICLE_ID
					}).OrderByDescending(n => n.date));
				}
			}			
		}

		public static void RemoveNotifications(USER_NOTIFICATION[] entities)
		{
			foreach (var g in entities.GroupBy(e => e.RECIPIENT_ID))
			{
				var keys = storage.Where(s => s.Value == g.Key)
					.Select(s => s.Key).ToArray();

				if (keys.Any())
				{
					var context = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
					context.Clients.Clients(keys).clear(g.Select(n => new
					{
						id = n.ID
					}));
				}
			}
		}

		public override Task OnDisconnected(bool stopCalled)
		{
			Int32 value;
			storage.TryRemove(this.Context.ConnectionId, out value);
			return base.OnDisconnected(stopCalled);
		}

		/// <summary>
		/// Removing all existing connections for the user when logging out
		/// </summary>
		/// <param name="userId"></param>
		public void SignOut(Int32 userId)
		{
			var keys = storage.Where(s => s.Value == userId)
				.Select(s => s.Key).ToArray();

			if (keys.Any())
				Clients.Clients(keys).close();
		}

		public void SignUp(Int32 userId)
		{
			storage.TryAdd(this.Context.ConnectionId, userId);
		}
	}
}