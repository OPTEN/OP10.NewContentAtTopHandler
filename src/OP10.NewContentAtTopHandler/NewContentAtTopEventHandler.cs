using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace OP10.NewContentAtTopHandler
{
	/// <summary>
	/// The Umbraco Event Handler to subscribe to the ContentService Saving event
	/// </summary>
	/// <seealso cref="Umbraco.Core.ApplicationEventHandler" />
	public class NewContentAtTopEventHandler : ApplicationEventHandler
	{
		private string[] newNodeToTopAliases;
		private const string contentRootAlias = "#ContentRoot";
		private const string mediaRootAlias = "#MediaRoot";
		private const char aliasDelimiter = ';';
		private const string appSettingsKey = "OP10.NewContentAtTopHandler";

		protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
		{
			string config = ConfigurationManager.AppSettings[appSettingsKey];

			if (!string.IsNullOrWhiteSpace(config))
			{
				newNodeToTopAliases = config.Split(aliasDelimiter);
				ContentService.Saved += ContentService_Saved;
				MediaService.Saved += MediaService_Saved;
			}
		}

		private void MediaService_Saved(IMediaService sender, SaveEventArgs<IMedia> e)
		{
			foreach (IMedia saved in e.SavedEntities)
			{
				if (saved.IsNewEntity())
				{
					IMedia parent = saved.Parent();
					bool hasParent = parent != null;
					string parentContentTypeAlias = hasParent ? parent.ContentType.Alias : mediaRootAlias;

					if (newNodeToTopAliases.Contains(parentContentTypeAlias))
					{
						IEnumerable<IMedia> children = hasParent ? parent.Children() : sender.GetRootMedia();

						// sort only if there are more than 1 child.
						// Umbraco crashes when there is only one child.
						if (children != null && children.Any() && children.Count() > 1)
						{
							IList<IMedia> siblings = children.OrderBy(o => o.SortOrder).ToList();

							// Remove the newly created media from the list and add it again at the first position
							siblings.Remove(saved);
							siblings.Insert(0, saved);

							sender.Sort(siblings);
						}
					}
				}
			}
		}

		private void ContentService_Saved(IContentService sender, SaveEventArgs<IContent> e)
		{
			foreach (IContent saved in e.SavedEntities)
			{
				if (saved.IsNewEntity())
				{
					IContent parent = saved.Parent();
					bool hasParent = parent != null;
					string parentContentTypeAlias = hasParent ? parent.ContentType.Alias : contentRootAlias;

					if (newNodeToTopAliases.Contains(parentContentTypeAlias))
					{
						IEnumerable<IContent> children = hasParent ? parent.Children() : sender.GetRootContent();

						// sort only if there are more than 1 child.
						// Umbraco crashes when there is only one child.
						if (children != null && children.Any() && children.Count() > 1)
						{
							IList<IContent> siblings = children.OrderBy(o => o.SortOrder).ToList();

							// Remove the newly created content from the list and add it again at the first position
							siblings.Remove(saved);
							siblings.Insert(0, saved);

							sender.Sort(siblings);
						}
					}
				}
			}
		}
	}
}
