/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Cms
{
	using System.Collections.Generic;
	using Adxstudio.Xrm.Services.Query;
	using Adxstudio.Xrm.Cms.SolutionVersions;
	using Adxstudio.Xrm.AspNet.Cms;

	public static class Solutions
	{
		public static readonly SolutionDefinition Commerce = SolutionDefinition.Create(PortalSolutions.SolutionNames.CommerceSolutionName, null, null);
		public static readonly SolutionDefinition Events = SolutionDefinition.Create(PortalSolutions.SolutionNames.EventsSolutionName, null, null);
		public static readonly SolutionDefinition Ideas = SolutionDefinition.Create(PortalSolutions.SolutionNames.IdeasSolutionName, new Dictionary<string, EntityDefinition>
			{
				{
					"adx_ideaforum",
					EntityDefinition.Create<IdeaForumNode>(
						PortalSolutions.SolutionNames.IdeasSolutionName, "adx_ideaforum", "adx_ideaforumid", IdeaForumNode.ColumnSet, 0,
						RetrieveByLinksQueryBuilder.Create(),
						IdeaSolutionVersions.NaosAndOlderVersions,
						RelationshipDefinition.Create<IdeaForumNode, WebsiteNode>(PortalSolutions.SolutionNames.IdeasSolutionName, "adx_website", "adx_websiteid", IdeaSolutionVersions.NaosAndOlderVersions,
							idea => idea.Website,
							site => site.Ideas,
							(site, idea) => { idea.Website = site; site.Ideas.Add(idea); },
							(site, idea, id) => { idea.Website = id; site.Ideas.Remove(idea); }))
				},
				{
					"adx_webrole_ideaforum_write",
					EntityDefinition.Create<IdeaForumWriteToWebRoleNode>(
						PortalSolutions.SolutionNames.IdeasSolutionName, "adx_webrole_ideaforum_write", "adx_webrole_ideaforum_writeid", IdeaForumWriteToWebRoleNode.ColumnSet, null,
						RetrieveByLinksQueryBuilder.Create(
							new Link { Name = "adx_ideaforum", FromAttribute = "adx_ideaforumid", ToAttribute = "adx_ideaforumid" },
							new Link { Name = "adx_webrole", FromAttribute = "adx_webroleid", ToAttribute = "adx_webroleid" }),
						IdeaSolutionVersions.NaosAndOlderVersions,
						RelationshipDefinition.Create<IdeaForumWriteToWebRoleNode, IdeaForumNode>(PortalSolutions.SolutionNames.IdeasSolutionName, "adx_webrole_ideaforum_write", "adx_webrole_ideaforum_writeid",
							IdeaSolutionVersions.NaosAndOlderVersions,
							idearole => idearole.Idea,
							rule => rule.WebRoleWriteIntersects,
							(rule, idearole) => { idearole.Idea = rule; rule.WebRoleWriteIntersects.Add(idearole); },
							(rule, idearole, id) => { idearole.Idea = id; rule.WebRoleWriteIntersects.Remove(idearole); }),
						RelationshipDefinition.Create<IdeaForumWriteToWebRoleNode, WebRoleNode>(PortalSolutions.SolutionNames.IdeasSolutionName, "adx_webrole", "adx_webroleid",
							IdeaSolutionVersions.NaosAndOlderVersions,
							idearole => idearole.WebRole,
							role => role.IdeaForumWriteIntersects,
							(role, idearole) => { idearole.WebRole = role; role.IdeaForumWriteIntersects.Add(idearole); },
							(role, idearole, id) => { idearole.WebRole = id; role.IdeaForumWriteIntersects.Remove(idearole); }))
				},
				{
					"adx_webrole_ideaforum_read",
					EntityDefinition.Create<IdeaForumReadToWebRoleNode>(
						PortalSolutions.SolutionNames.IdeasSolutionName, "adx_webrole_ideaforum_read", "adx_webrole_ideaforum_readid", IdeaForumReadToWebRoleNode.ColumnSet, null,
						RetrieveByLinksQueryBuilder.Create(
							new Link { Name = "adx_ideaforum", FromAttribute = "adx_ideaforumid", ToAttribute = "adx_ideaforumid" },
							new Link { Name = "adx_webrole", FromAttribute = "adx_webroleid", ToAttribute = "adx_webroleid" }),
						IdeaSolutionVersions.NaosAndOlderVersions,
						RelationshipDefinition.Create<IdeaForumReadToWebRoleNode, IdeaForumNode>(PortalSolutions.SolutionNames.IdeasSolutionName, "adx_webrole_ideaforum_read", "adx_webrole_ideaforum_readid",
							IdeaSolutionVersions.NaosAndOlderVersions,
							idearole => idearole.Idea,
							rule => rule.WebRoleReadIntersects,
							(rule, idearole) => { idearole.Idea = rule; rule.WebRoleReadIntersects.Add(idearole); },
							(rule, idearole, id) => { idearole.Idea = id; rule.WebRoleReadIntersects.Remove(idearole); }),
						RelationshipDefinition.Create<IdeaForumReadToWebRoleNode, WebRoleNode>(PortalSolutions.SolutionNames.IdeasSolutionName, "adx_webrole", "adx_webroleid",
							IdeaSolutionVersions.NaosAndOlderVersions,
							idearole => idearole.WebRole,
							role => role.IdeaForumReadIntersects,
							(role, idearole) => { idearole.WebRole = role; role.IdeaForumReadIntersects.Add(idearole); },
							(role, idearole, id) => { idearole.WebRole = id; role.IdeaForumReadIntersects.Remove(idearole); }))
				}
			}, new Dictionary<string, ManyRelationshipDefinition>
			{
				{
					"adx_webrole_ideaforum_read",
					new ManyRelationshipDefinition(PortalSolutions.SolutionNames.IdeasSolutionName, "adx_webrole_ideaforum_read", "adx_webrole_ideaforum_read", IdeaSolutionVersions.NaosAndOlderVersions)
				},
				{
					"adx_webrole_ideaforum_write",
					new ManyRelationshipDefinition(PortalSolutions.SolutionNames.IdeasSolutionName, "adx_webrole_ideaforum_write", "adx_webrole_ideaforum_write", IdeaSolutionVersions.NaosAndOlderVersions)
				}
			});

		public static readonly SolutionDefinition Forums = SolutionDefinition.Create(PortalSolutions.SolutionNames.ForumsSolutionName, new Dictionary<string, EntityDefinition>
			{
				{
					"adx_communityforum",
					EntityDefinition.Create<ForumNode>(
						PortalSolutions.SolutionNames.ForumsSolutionName, "adx_communityforum", "adx_communityforumid", ForumNode.ColumnSet, 0,
						RetrieveByLinksQueryBuilder.Create(),
						ForumSolutionVersions.NaosAndOlderVersions,
						RelationshipDefinition.Create<ForumNode, WebsiteNode>(PortalSolutions.SolutionNames.BaseSolutionName, "adx_website", "adx_websiteid",
							ForumSolutionVersions.NaosAndOlderVersions,
							forum => forum.Website,
							site => site.Forums,
							(site, forum) => { forum.Website = site; site.Forums.Add(forum); },
							(site, forum, id) => { forum.Website = id; site.Forums.Remove(forum); }),
						RelationshipDefinition.Create<ForumNode, WebPageNode>(PortalSolutions.SolutionNames.BaseSolutionName, "adx_webpage", "adx_parentpageid",
							ForumSolutionVersions.NaosAndOlderVersions,
							forum => forum.ParentPage,
							page => page.Forums,
							(page, forum) => { forum.ParentPage = page; page.Forums.Add(forum); },
							(page, forum, id) => { forum.ParentPage = id; page.Forums.Remove(forum); }),
						RelationshipDefinition.Create<ForumNode, WebsiteLanguageNode>(PortalSolutions.SolutionNames.BaseSolutionName, "adx_websitelanguage", "adx_websitelanguageid",
							BaseSolutionVersions.CentaurusVersion,
							forum => forum.WebsiteLanguage,
							language => language.Forums,
							(language, forum) => { forum.WebsiteLanguage = language; language.Forums.Add(forum); },
							(language, forum, id) => { forum.WebsiteLanguage = id; language.Forums.Remove(forum); }))

				},
				{
					"adx_communityforumaccesspermission",
					EntityDefinition.Create<ForumAccessPermissionNode>(
						PortalSolutions.SolutionNames.ForumsSolutionName, "adx_communityforumaccesspermission", "adx_communityforumaccesspermissionid", ForumAccessPermissionNode.ColumnSet, 0,
						RetrieveByLinksQueryBuilder.Create(
							new Link { Name = "adx_communityforum", FromAttribute = "adx_communityforumid", ToAttribute = "adx_forumid" }),
						ForumSolutionVersions.NaosAndOlderVersions,
						RelationshipDefinition.Create<ForumAccessPermissionNode, ForumNode>(PortalSolutions.SolutionNames.ForumsSolutionName, "adx_communityforum", "adx_forumid",
							ForumSolutionVersions.NaosAndOlderVersions,
							forumAccess => forumAccess.Forum,
							forum => forum.ForumAccessPermissions,
							(forum, forumAccess) => { forumAccess.Forum = forum; forum.ForumAccessPermissions.Add(forumAccess); },
							(forum, forumAccess, id) => { forumAccess.Forum = id; forum.ForumAccessPermissions.Remove(forumAccess); }))
				},
				 {
				 	"adx_communityforumaccesspermission_webrole",
				 	EntityDefinition.Create<ForumAccessPermissionsToWebRoleNode>(
				 		PortalSolutions.SolutionNames.ForumsSolutionName, "adx_communityforumaccesspermission_webrole", "adx_communityforumaccesspermission_webroleid", ForumAccessPermissionsToWebRoleNode.ColumnSet, null,
				 		RetrieveByLinksQueryBuilder.Create(
				 			new Link { Name = "adx_webrole", FromAttribute = "adx_webroleid", ToAttribute = "adx_webroleid" }),
				 		ForumSolutionVersions.NaosAndOlderVersions,
				 		 RelationshipDefinition.Create<ForumAccessPermissionsToWebRoleNode, ForumAccessPermissionNode>(PortalSolutions.SolutionNames.ForumsSolutionName, "adx_communityforumaccesspermission", "adx_communityforumaccesspermissionid",
				 			ForumSolutionVersions.NaosAndOlderVersions,
				 			forumAccess => forumAccess.ForumAccessPermission,
				 			role => role.WebRoleIntersect,
				 			(role, forumAccess) => { forumAccess.ForumAccessPermission = role; role.WebRoleIntersect.Add(forumAccess); },
				 			(role, forumAccess, id) => { forumAccess.ForumAccessPermission = id; role.WebRoleIntersect.Remove(forumAccess); }),
				 		RelationshipDefinition.Create<ForumAccessPermissionsToWebRoleNode, WebRoleNode>(PortalSolutions.SolutionNames.ForumsSolutionName, "adx_webrole", "adx_webroleid",
				 			ForumSolutionVersions.NaosAndOlderVersions,
				 			forumAccess => forumAccess.WebRole,
				 			role => role.ForumAccessPermissionsIntersects,
				 			(role, forumAccess) => { forumAccess.WebRole = role; role.ForumAccessPermissionsIntersects.Add(forumAccess); },
				 			(role, forumAccess, id) => { forumAccess.WebRole = id; role.ForumAccessPermissionsIntersects.Remove(forumAccess); }))
				 }

			}, new Dictionary<string, ManyRelationshipDefinition>
			{
				{
					"adx_communityforumaccesspermission_webrole",
					new ManyRelationshipDefinition(PortalSolutions.SolutionNames.ForumsSolutionName, "adx_communityforumaccesspermission_webrole", "adx_communityforumaccesspermission_webrole", ForumSolutionVersions.NaosAndOlderVersions)
				}
			});

		public static readonly SolutionDefinition Blogs = SolutionDefinition.Create(
			PortalSolutions.SolutionNames.BlogsSolutionName,
			new Dictionary<string, EntityDefinition>
			{
				{
					"adx_blog",
					EntityDefinition.Create<BlogNode>(
						PortalSolutions.SolutionNames.BlogsSolutionName, "adx_blog", "adx_blogid", BlogNode.ColumnSet, 0,
						RetrieveByLinksQueryBuilder.Create(),
						BlogSolutionVersions.NaosAndOlderVersions,
						RelationshipDefinition.Create<BlogNode, WebsiteNode>(PortalSolutions.SolutionNames.BaseSolutionName, "adx_website", "adx_websiteid",
							BlogSolutionVersions.NaosAndOlderVersions,
							blog => blog.Website,
							site => site.Blogs,
							(site, blog) => { blog.Website = site; site.Blogs.Add(blog); },
							(site, blog, id) => { blog.Website = id; site.Blogs.Remove(blog); }))
				},
				{
					"adx_webfile",
					EntityDefinition.Extend<WebFileNode>(
						PortalSolutions.SolutionNames.BaseSolutionName,
						"adx_webfile",
						new SolutionColumnSet(PortalSolutions.SolutionNames.BlogsSolutionName, new EntityNodeColumn("adx_blogpostid", BlogSolutionVersions.NaosAndOlderVersions)),
						BlogSolutionVersions.NaosAndOlderVersions,
						null)
				},
				{
					"adx_blog_webrole",
					EntityDefinition.Create<BlogToWebRoleNode>(
						PortalSolutions.SolutionNames.BlogsSolutionName, "adx_blog_webrole", "adx_blog_webroleid", BlogToWebRoleNode.ColumnSet, null,
						RetrieveByLinksQueryBuilder.Create(
							new Link { Name = "adx_blog", FromAttribute = "adx_blogid", ToAttribute = "adx_blogid" },
							new Link { Name = "adx_webrole", FromAttribute = "adx_webroleid", ToAttribute = "adx_webroleid" }),
						BlogSolutionVersions.NaosAndOlderVersions,
						RelationshipDefinition.Create<BlogToWebRoleNode, BlogNode>(PortalSolutions.SolutionNames.BlogsSolutionName, "adx_blog", "adx_blogid",
							BlogSolutionVersions.NaosAndOlderVersions,
							blog => blog.Blog,
							role => role.WebRoleIntersect,
							(role, blog) => { blog.Blog = role; role.WebRoleIntersect.Add(blog); },
							(role, blog, id) => { blog.Blog = id; role.WebRoleIntersect.Remove(blog); }),
						RelationshipDefinition.Create<BlogToWebRoleNode, WebRoleNode>(PortalSolutions.SolutionNames.BlogsSolutionName, "adx_webrole", "adx_webroleid",
							BlogSolutionVersions.NaosAndOlderVersions,
							blog => blog.WebRole,
							role => role.BlogPermissionIntersects,
							(role, blog) => { blog.WebRole = role; role.BlogPermissionIntersects.Add(blog); },
							(role, blog, id) => { blog.WebRole = id; role.BlogPermissionIntersects.Remove(blog); }))
				}

			}, new Dictionary<string, ManyRelationshipDefinition>
			{
				{
					"adx_blog_webrole",
					new ManyRelationshipDefinition(PortalSolutions.SolutionNames.BlogsSolutionName, "adx_blog_webrole", "adx_blog_webrole", BlogSolutionVersions.NaosAndOlderVersions)
				}
			});

		public static readonly SolutionDefinition WebForms = SolutionDefinition.Create(
			PortalSolutions.SolutionNames.WebFormsSolutionName,
			new Dictionary<string, EntityDefinition>
			{
				{
					"adx_webpage",
					EntityDefinition.Extend<WebPageNode>(
						PortalSolutions.SolutionNames.BaseSolutionName,
						"adx_webpage",
						new SolutionColumnSet(PortalSolutions.SolutionNames.WebFormsSolutionName, new EntityNodeColumn("adx_entityform", BaseSolutionVersions.NaosAndOlderVersions), new EntityNodeColumn("adx_entitylist", BaseSolutionVersions.NaosAndOlderVersions), new EntityNodeColumn("adx_webform", BaseSolutionVersions.NaosAndOlderVersions)),
						BaseSolutionVersions.NaosAndOlderVersions,
						null)
				}
			},
			null);

		public static readonly SolutionDefinition Base = SolutionDefinition.Create(
			PortalSolutions.SolutionNames.BaseSolutionName,
			new Dictionary<string, EntityDefinition>
			{
				{
					"annotation",
					EntityDefinition.Create<AnnotationNode>(
						PortalSolutions.SolutionNames.BaseSolutionName, "annotation", "annotationid", AnnotationNode.ColumnSet, null,
						RetrieveByLinksQueryBuilder.Create(new Link { Name = "adx_webfile", FromAttribute = "adx_webfileid", ToAttribute = "objectid" }),
						BaseSolutionVersions.NaosAndOlderVersions,
						true,
						RelationshipDefinition.Create<AnnotationNode, WebFileNode>(PortalSolutions.SolutionNames.BaseSolutionName, "adx_webfile", "objectid",
							BaseSolutionVersions.NaosAndOlderVersions,
							note => note.WebFile,
							file => file.Annotations,
							(file, note) => { note.WebFile = file; file.Annotations.Add(note); },
							(file, note, id) => { note.WebFile = id; file.Annotations.Remove(note); }))
				},
				{
					"adx_contentsnippet",
					EntityDefinition.Create<ContentSnippetNode>(
						PortalSolutions.SolutionNames.BaseSolutionName, "adx_contentsnippet", "adx_contentsnippetid", ContentSnippetNode.ColumnSet, 0,
						RetrieveByLinksQueryBuilder.Create(),
						BaseSolutionVersions.NaosAndOlderVersions,
						RelationshipDefinition.Create<ContentSnippetNode, WebsiteNode>(PortalSolutions.SolutionNames.BaseSolutionName, "adx_website", "adx_websiteid",
							BaseSolutionVersions.NaosAndOlderVersions,
							snippet => snippet.Website,
							site => site.ContentSnippets,
							(site, snippet) => { snippet.Website = site; site.ContentSnippets.Add(snippet); },
							(site, snippet, id) => { snippet.Website = id; site.ContentSnippets.Remove(snippet); }),

						RelationshipDefinition.Create<ContentSnippetNode, WebsiteLanguageNode>(PortalSolutions.SolutionNames.BaseSolutionName, "adx_websitelanguage", "adx_contentsnippetlanguageid",
							BaseSolutionVersions.CentaurusVersion,
							snippet => snippet.Language,
							language => language.Snippets,
							(language, snippet) => { snippet.Language = language; language.Snippets.Add(snippet); },
							(language, snippet, id) => { snippet.Language = id; language.Snippets.Remove(snippet); }))
				},
				{
					"adx_pagetemplate",
					EntityDefinition.Create<PageTemplateNode>(
						PortalSolutions.SolutionNames.BaseSolutionName, "adx_pagetemplate", "adx_pagetemplateid", PageTemplateNode.ColumnSet, null,
						RetrieveByLinksQueryBuilder.Create(),
						BaseSolutionVersions.NaosAndOlderVersions,
						RelationshipDefinition.Create<PageTemplateNode, WebsiteNode>(PortalSolutions.SolutionNames.BaseSolutionName, "adx_website", "adx_websiteid",
							BaseSolutionVersions.NaosAndOlderVersions,
							template => template.Website,
							site => site.PageTemplates,
							(site, template) => { template.Website = site; site.PageTemplates.Add(template); },
							(site, template, id) => { template.Website = id; site.PageTemplates.Remove(template); }))
				},
				{
					"adx_publishingstate",
					EntityDefinition.Create<PublishingStateNode>(
						PortalSolutions.SolutionNames.BaseSolutionName, "adx_publishingstate", "adx_publishingstateid", PublishingStateNode.ColumnSet, null,
						RetrieveByLinksQueryBuilder.Create(),
						BaseSolutionVersions.NaosAndOlderVersions,
						RelationshipDefinition.Create<PublishingStateNode, WebsiteNode>(PortalSolutions.SolutionNames.BaseSolutionName, "adx_website", "adx_websiteid",
							BaseSolutionVersions.NaosAndOlderVersions,
							state => state.Website,
							site => site.PublishingStates,
							(site, state) => { state.Website = site; site.PublishingStates.Add(state); },
							(site, state, id) => { state.Website = id; site.PublishingStates.Remove(state); }))
				},
				{
					"adx_shortcut",
					EntityDefinition.Create<ShortcutNode>(
						PortalSolutions.SolutionNames.BaseSolutionName, "adx_shortcut", "adx_shortcutid", ShortcutNode.ColumnSet, null,
						RetrieveByLinksQueryBuilder.Create(),
						BaseSolutionVersions.NaosAndOlderVersions,
						RelationshipDefinition.Create<ShortcutNode, WebPageNode>(PortalSolutions.SolutionNames.BaseSolutionName, "adx_webpage", "adx_parentpage_webpageid",
							BaseSolutionVersions.NaosAndOlderVersions,
							shortcut => shortcut.Parent,
							page => page.Shortcuts,
							(page, shortcut) => { shortcut.Parent = page; page.Shortcuts.Add(shortcut); },
							(page, shortcut, id) => { shortcut.Parent = id; page.Shortcuts.Remove(shortcut); }),
						RelationshipDefinition.Create<ShortcutNode, WebFileNode>(PortalSolutions.SolutionNames.BaseSolutionName, "adx_webfile", "adx_webfileid",
							BaseSolutionVersions.NaosAndOlderVersions,
							shortcut => shortcut.WebFile,
							null,
							(file, shortcut) => { shortcut.WebFile = file; },
							(file, shortcut, id) => { shortcut.WebFile = id; }),
						RelationshipDefinition.Create<ShortcutNode, WebPageNode>(PortalSolutions.SolutionNames.BaseSolutionName, "adx_webpage", "adx_webpageid",
							BaseSolutionVersions.NaosAndOlderVersions,
							shortcut => shortcut.WebPage,
							null,
							(page, shortcut) => { shortcut.WebPage = page; },
							(page, shortcut, id) => { shortcut.WebPage = id; }),
						RelationshipDefinition.Create<ShortcutNode, WebsiteNode>(PortalSolutions.SolutionNames.BaseSolutionName, "adx_website", "adx_websiteid",
							BaseSolutionVersions.NaosAndOlderVersions,
							shortcut => shortcut.Website,
							site => site.Shortcuts,
							(site, shortcut) => { shortcut.Website = site; site.Shortcuts.Add(shortcut); },
							(site, shortcut, id) => { shortcut.Website = id; site.Shortcuts.Remove(shortcut); }))
				},
				{
					"adx_sitemarker",
					EntityDefinition.Create<SiteMarkerNode>(
						PortalSolutions.SolutionNames.BaseSolutionName, "adx_sitemarker", "adx_sitemarkerid", SiteMarkerNode.ColumnSet, null,
						RetrieveByLinksQueryBuilder.Create(),
						BaseSolutionVersions.NaosAndOlderVersions,
						RelationshipDefinition.Create<SiteMarkerNode, WebPageNode>(PortalSolutions.SolutionNames.BaseSolutionName, "adx_webpage", "adx_pageid",
							BaseSolutionVersions.NaosAndOlderVersions,
							marker => marker.WebPage,
							page => page.SiteMarkers,
							(page, marker) => { marker.WebPage = page; page.SiteMarkers.Add(marker); },
							(page, marker, id) => { marker.WebPage = id; page.SiteMarkers.Remove(marker); }),
						RelationshipDefinition.Create<SiteMarkerNode, WebsiteNode>(PortalSolutions.SolutionNames.BaseSolutionName, "adx_website", "adx_websiteid",
							BaseSolutionVersions.NaosAndOlderVersions,
							marker => marker.Website,
							site => site.SiteMarkers,
							(site, marker) => { marker.Website = site; site.SiteMarkers.Add(marker); },
							(site, marker, id) => { marker.Website = id; site.SiteMarkers.Remove(marker); }))
				},
				{
					"subject",
					EntityDefinition.Create<SubjectNode>(
						PortalSolutions.SolutionNames.BaseSolutionName, "subject", "subjectid", SubjectNode.ColumnSet, null, null,
						BaseSolutionVersions.NaosAndOlderVersions)
				},
				{
					"adx_webfile",
					EntityDefinition.Create<WebFileNode>(
						PortalSolutions.SolutionNames.BaseSolutionName, "adx_webfile", "adx_webfileid", WebFileNode.ColumnSet, 0,
						RetrieveByLinksQueryBuilder.Create(),
						BaseSolutionVersions.NaosAndOlderVersions,
						RelationshipDefinition.Create<WebFileNode, WebFileNode>(PortalSolutions.SolutionNames.BaseSolutionName, "adx_webpage", "adx_masterwebfileid",
							BaseSolutionVersions.NaosAndOlderVersions,
							file => file.Master,
							/*file => Subscribers*/ null,
							(master, file) => { file.Master = master; /*master.Subscribers.Add(file);*/ },
							(master, file, id) => { file.Master = id; /*master.Subscribers.Remove(file);*/ }),
						RelationshipDefinition.Create<WebFileNode, WebPageNode>(PortalSolutions.SolutionNames.BaseSolutionName, "adx_webpage", "adx_parentpageid",
							BaseSolutionVersions.NaosAndOlderVersions,
							file => file.Parent,
							page => page.WebFiles,
							(page, file) => { file.Parent = page; page.WebFiles.Add(file); },
							(page, file, id) => { file.Parent = id; page.WebFiles.Remove(file); }),
						RelationshipDefinition.Create<WebFileNode, PublishingStateNode>(PortalSolutions.SolutionNames.BaseSolutionName, "adx_publishingstate", "adx_publishingstateid",
							BaseSolutionVersions.NaosAndOlderVersions,
							file => file.PublishingState,
							/*state => state.WebFiles*/ null,
							(state, file) => { file.PublishingState = state; /*state.WebFiles.Add(file);*/ },
							(state, file, id) => { file.PublishingState = id; /*state.WebFiles.Remove(file);*/ }),
						RelationshipDefinition.Create<WebFileNode, SubjectNode>(PortalSolutions.SolutionNames.BaseSolutionName, "subject", "adx_subjectid",
							BaseSolutionVersions.NaosAndOlderVersions,
							file => file.Subject,
							/*subject => subject.WebFiles*/ null,
							(subject, file) => { file.Subject = subject; /*subject.WebFiles.Add(file);*/ },
							(subject, file, id) => { file.Subject = id; /*subject.WebFiles.Remove(file);*/ }),
						RelationshipDefinition.Create<WebFileNode, WebsiteNode>(PortalSolutions.SolutionNames.BaseSolutionName, "adx_website", "adx_websiteid",
							BaseSolutionVersions.NaosAndOlderVersions,
							file => file.Website,
							site => site.WebFiles,
							(site, file) => { file.Website = site; site.WebFiles.Add(file); },
							(site, file, id) => { file.Website = id; site.WebFiles.Remove(file); }),
						RelationshipDefinition.Create<WebFileNode, WebFileNode>(PortalSolutions.SolutionNames.BaseSolutionName, "adx_webfile", "adx_masterwebfileid",
							BaseSolutionVersions.NaosAndOlderVersions,
							file => file.Master,
							master => master.Subscribers,
							(master, file) => { file.Master = master; master.Subscribers.Add(file); },
							(master, file, id) => { file.Master = id; master.Subscribers.Remove(file); }))
				},
				{
					"adx_weblink",
					EntityDefinition.Create<WebLinkNode>(
						PortalSolutions.SolutionNames.BaseSolutionName, "adx_weblink", "adx_weblinkid", WebLinkNode.ColumnSet, 0,
						RetrieveByLinksQueryBuilder.Create(new Link { Name = "adx_weblinkset", FromAttribute = "adx_weblinksetid", ToAttribute = "adx_weblinksetid" }),
						BaseSolutionVersions.NaosAndOlderVersions,
						RelationshipDefinition.Create<WebLinkNode, WebPageNode>(PortalSolutions.SolutionNames.BaseSolutionName, "adx_webpage", "adx_pageid",
							BaseSolutionVersions.NaosAndOlderVersions,
							link => link.WebPage,
							null,
							(page, link) => { link.WebPage = page; },
							(page, link, id) => { link.WebPage = id; }),
						RelationshipDefinition.Create<WebLinkNode, PublishingStateNode>(PortalSolutions.SolutionNames.BaseSolutionName, "adx_publishingstate", "adx_publishingstateid",
							BaseSolutionVersions.NaosAndOlderVersions,
							link => link.PublishingState,
							null,
							(state, link) => { link.PublishingState = state; },
							(state, link, id) => { link.PublishingState = id; }),
						RelationshipDefinition.Create<WebLinkNode, WebLinkSetNode>(PortalSolutions.SolutionNames.BaseSolutionName, "adx_weblinkset", "adx_weblinksetid",
							BaseSolutionVersions.NaosAndOlderVersions,
							link => link.WebLinkSet,
							wls => wls.WebLinks,
							(wls, link) => { link.WebLinkSet = wls; wls.WebLinks.Add(link); },
							(wls, link, id) => { link.WebLinkSet = id; wls.WebLinks.Remove(link); }))
				},
				{
					"adx_weblinkset",
					EntityDefinition.Create<WebLinkSetNode>(
						PortalSolutions.SolutionNames.BaseSolutionName, "adx_weblinkset", "adx_weblinksetid", WebLinkSetNode.ColumnSet, null,
						RetrieveByLinksQueryBuilder.Create(),
						BaseSolutionVersions.NaosAndOlderVersions,
						RelationshipDefinition.Create<WebLinkSetNode, PublishingStateNode>(PortalSolutions.SolutionNames.BaseSolutionName, "adx_publishingstate", "adx_publishingstateid",
							BaseSolutionVersions.NaosAndOlderVersions,
							wls => wls.PublishingState,
							/*state => state.WebLinkSets*/ null,
							(state, wls) => { wls.PublishingState = state; /*state.WebLinkSets.Add(wls);*/ },
							(state, wls, id) => { wls.PublishingState = id; /*state.WebLinkSets.Remove(wls);*/ }),
						RelationshipDefinition.Create<WebLinkSetNode, WebsiteLanguageNode>(PortalSolutions.SolutionNames.BaseSolutionName, "adx_websitelanguage", "adx_websitelanguageid",
							BaseSolutionVersions.CentaurusVersion,
							wls => wls.WebsiteLanguage,
							null,
							(websiteLang, wls) => { wls.WebsiteLanguage = websiteLang; },
							(websiteLang, wls, id) => { wls.WebsiteLanguage = id; }),
						RelationshipDefinition.Create<WebLinkSetNode, WebsiteNode>(PortalSolutions.SolutionNames.BaseSolutionName, "adx_website", "adx_websiteid",
							BaseSolutionVersions.NaosAndOlderVersions,
							wls => wls.Website,
							site => site.WebLinkSets,
							(site, wls) => { wls.Website = site; site.WebLinkSets.Add(wls); },
							(site, wls, id) => { wls.Website = id; site.WebLinkSets.Remove(wls); }))
				},
				{
					"adx_webpageaccesscontrolrule",
					EntityDefinition.Create<WebPageAccessControlRuleNode>(
						PortalSolutions.SolutionNames.BaseSolutionName, "adx_webpageaccesscontrolrule", "adx_webpageaccesscontrolruleid", WebPageAccessControlRuleNode.ColumnSet, 0,
						RetrieveByLinksQueryBuilder.Create(),
						BaseSolutionVersions.NaosAndOlderVersions,
						RelationshipDefinition.Create<WebPageAccessControlRuleNode, WebPageNode>(PortalSolutions.SolutionNames.BaseSolutionName, "adx_webpage", "adx_webpageid",
							BaseSolutionVersions.NaosAndOlderVersions,
							rule => rule.WebPage,
							page => page.WebPageAccessControlRules,
							(page, rule) => { rule.WebPage = page; page.WebPageAccessControlRules.Add(rule); },
							(page, rule, id) => { rule.WebPage = id; page.WebPageAccessControlRules.Remove(rule); }),
						RelationshipDefinition.Create<WebPageAccessControlRuleNode, WebsiteNode>(PortalSolutions.SolutionNames.BaseSolutionName, "adx_website", "adx_websiteid",
							BaseSolutionVersions.NaosAndOlderVersions,
							rule => rule.Website,
							site => site.WebPageAccessControlRules,
							(site, rule) => { rule.Website = site; site.WebPageAccessControlRules.Add(rule); },
							(site, rule, id) => { rule.Website = id; site.WebPageAccessControlRules.Remove(rule); }))
				},
				{
					"adx_webpage",
					EntityDefinition.Create<WebPageNode>(
						PortalSolutions.SolutionNames.BaseSolutionName, "adx_webpage", "adx_webpageid", WebPageNode.ColumnSet, 0,
						RetrieveByLinksQueryBuilder.Create(),
						BaseSolutionVersions.NaosAndOlderVersions,
						RelationshipDefinition.Create<WebPageNode, PageTemplateNode>(PortalSolutions.SolutionNames.BaseSolutionName, "adx_pagetemplate", "adx_pagetemplateid",
							BaseSolutionVersions.NaosAndOlderVersions,
							page => page.PageTemplate,
							template => template.WebPages,
							(template, page) => { page.PageTemplate = template; template.WebPages.Add(page); },
							(template, page, id) => { page.PageTemplate = id; template.WebPages.Remove(page); }),
						RelationshipDefinition.Create<WebPageNode, WebPageNode>(PortalSolutions.SolutionNames.BaseSolutionName, "adx_webpage", "adx_parentpageid",
							BaseSolutionVersions.NaosAndOlderVersions,
							page => page.Parent,
							page => page.WebPages,
							(parent, page) => { page.Parent = parent; parent.WebPages.Add(page); },
							(parent, page, id) => { page.Parent = id; parent.WebPages.Remove(page); }),
						RelationshipDefinition.Create<WebPageNode, PublishingStateNode>(PortalSolutions.SolutionNames.BaseSolutionName, "adx_publishingstate", "adx_publishingstateid",
							BaseSolutionVersions.NaosAndOlderVersions,
							page => page.PublishingState,
							/*state => state.WebPages*/ null,
							(state, page) => { page.PublishingState = state; /*state.WebPages.Add(page);*/ },
							(state, page, id) => { page.PublishingState = id; /*state.WebPages.Remove(page);*/ }),
						RelationshipDefinition.Create<WebPageNode, SubjectNode>(PortalSolutions.SolutionNames.BaseSolutionName, "subject", "adx_subjectid",
							BaseSolutionVersions.NaosAndOlderVersions,
							page => page.Subject,
							null,
							(subject, page) => { page.Subject = subject; },
							(subject, page, id) => { page.Subject = id; }),
						RelationshipDefinition.Create<WebPageNode, WebsiteNode>(PortalSolutions.SolutionNames.BaseSolutionName, "adx_website", "adx_websiteid",
							BaseSolutionVersions.NaosAndOlderVersions,
							page => page.Website,
							site => site.WebPages,
							(site, page) => { page.Website = site; site.WebPages.Add(page); },
							(site, page, id) => { page.Website = id; site.WebPages.Remove(page); }),
						RelationshipDefinition.Create<WebPageNode, WebPageNode>(PortalSolutions.SolutionNames.BaseSolutionName, "adx_webpage", "adx_masterwebpageid",
							BaseSolutionVersions.NaosAndOlderVersions,
							page => page.Master,
							master => master.Subscribers,
							(master, page) => { page.Master = master; master.Subscribers.Add(page); },
							(master, page, id) => { page.Master = id; master.Subscribers.Remove(page); }),
						RelationshipDefinition.Create<WebPageNode, WebPageNode>(PortalSolutions.SolutionNames.BaseSolutionName, "adx_webpage", "adx_rootwebpageid",
							BaseSolutionVersions.CentaurusVersion,
							page => page.RootWebPage,
							page => page.LanguageContentPages,
							(root, page) => { page.RootWebPage = root; root.LanguageContentPages.Add(page); },
							(root, page, id) => { page.RootWebPage = id; root.LanguageContentPages.Remove(page); }),
						RelationshipDefinition.Create<WebPageNode, WebsiteLanguageNode>(PortalSolutions.SolutionNames.BaseSolutionName, "adx_websitelanguage", "adx_webpagelanguageid",
							BaseSolutionVersions.CentaurusVersion,
							page => page.WebPageLanguage,
							null,
							(language, page) => { page.WebPageLanguage = language; },
							(language, page, id) => { page.WebPageLanguage = id; }))
				},
				{
					"adx_webrole",
					EntityDefinition.Create<WebRoleNode>(
						PortalSolutions.SolutionNames.BaseSolutionName, "adx_webrole", "adx_webroleid", WebRoleNode.ColumnSet, null,
						RetrieveByLinksQueryBuilder.Create(),
						BaseSolutionVersions.NaosAndOlderVersions,
						RelationshipDefinition.Create<WebRoleNode, WebsiteNode>(PortalSolutions.SolutionNames.BaseSolutionName, "adx_website", "adx_websiteid",
							BaseSolutionVersions.NaosAndOlderVersions,
							role => role.Website,
							site => site.WebRoles,
							(site, role) => { role.Website = site; site.WebRoles.Add(role); },
							(site, role, id) => { role.Website = id; site.WebRoles.Remove(role); }))
				},
				{
					"adx_websiteaccess",
					EntityDefinition.Create<WebsiteAccessNode>(
						PortalSolutions.SolutionNames.BaseSolutionName, "adx_websiteaccess", "adx_websiteaccessid", WebsiteAccessNode.ColumnSet, null,
						RetrieveByLinksQueryBuilder.Create(),
						BaseSolutionVersions.NaosAndOlderVersions,
						RelationshipDefinition.Create<WebsiteAccessNode, WebsiteNode>(PortalSolutions.SolutionNames.BaseSolutionName, "adx_website", "adx_websiteid",
							BaseSolutionVersions.NaosAndOlderVersions,
							access => access.Website,
							site => site.WebsiteAccesses,
							(site, access) => { access.Website = site; site.WebsiteAccesses.Add(access); },
							(site, access, id) => { access.Website = id; site.WebsiteAccesses.Remove(access); }))
				},
				{
					"adx_website",
					EntityDefinition.Create<WebsiteNode>(
						PortalSolutions.SolutionNames.BaseSolutionName, "adx_website", "adx_websiteid", WebsiteNode.ColumnSet, 0,
						RetrieveByLinksQueryBuilder.Create(),
						BaseSolutionVersions.NaosAndOlderVersions,
						RelationshipDefinition.Create<WebsiteNode, WebsiteNode>(PortalSolutions.SolutionNames.BaseSolutionName, "adx_website", "adx_parentwebsiteid",
							BaseSolutionVersions.NaosAndOlderVersions,
							site => site.Master,
							master => master.Subscribers,
							(master, site) => { site.Master = master; master.Subscribers.Add(site); },
							(master, site, id) => { site.Master = id; master.Subscribers.Remove(site); }),
						RelationshipDefinition.Create<WebsiteNode, WebsiteLanguageNode>(PortalSolutions.SolutionNames.BaseSolutionName, "adx_websitelanguage", "adx_defaultlanguage",
							BaseSolutionVersions.CentaurusVersion,
							site => site.DefaultLanguage,
							null,
							(language, site) => { site.DefaultLanguage = language; },
							(language, site, id) => { site.DefaultLanguage = id; }))
				},
				{
					"adx_accesscontrolrule_publishingstate",
					EntityDefinition.Create<WebPageAccessControlRuleToPublishingStateNode>(
						PortalSolutions.SolutionNames.BaseSolutionName, "adx_accesscontrolrule_publishingstate", "adx_accesscontrolrule_publishingstateid", WebPageAccessControlRuleToPublishingStateNode.ColumnSet, null,
						RetrieveByLinksQueryBuilder.Create(
							new Link { Name = "adx_webpageaccesscontrolrule", FromAttribute = "adx_webpageaccesscontrolruleid", ToAttribute = "adx_webpageaccesscontrolruleid" },
							new Link { Name = "adx_publishingstate", FromAttribute = "adx_publishingstateid", ToAttribute = "adx_publishingstateid" }),
						BaseSolutionVersions.NaosAndOlderVersions,
						RelationshipDefinition.Create<WebPageAccessControlRuleToPublishingStateNode, WebPageAccessControlRuleNode>(PortalSolutions.SolutionNames.BaseSolutionName, "adx_webpageaccesscontrolrule", "adx_webpageaccesscontrolruleid",
							BaseSolutionVersions.NaosAndOlderVersions,
							rulestate => rulestate.WebPageAccessControlRule,
							rule => rule.PublishingStateIntersects,
							(rule, rulestate) => { rulestate.WebPageAccessControlRule = rule; rule.PublishingStateIntersects.Add(rulestate); },
							(rule, rulestate, id) => { rulestate.WebPageAccessControlRule = id; rule.PublishingStateIntersects.Remove(rulestate); }),
						RelationshipDefinition.Create<WebPageAccessControlRuleToPublishingStateNode, PublishingStateNode>(PortalSolutions.SolutionNames.BaseSolutionName, "adx_publishingstate", "adx_publishingstateid",
							BaseSolutionVersions.NaosAndOlderVersions,
							rulestate => rulestate.PublishingState,
							state => state.WebPageAccessControlRuleIntersects,
							(state, rulestate) => { rulestate.PublishingState = state; state.WebPageAccessControlRuleIntersects.Add(rulestate); },
							(state, rulestate, id) => { rulestate.PublishingState = id; state.WebPageAccessControlRuleIntersects.Remove(rulestate); }))
				},
				{
					"adx_webpageaccesscontrolrule_webrole",
					EntityDefinition.Create<WebPageAccessControlRuleToWebRoleNode>(
						PortalSolutions.SolutionNames.BaseSolutionName, "adx_webpageaccesscontrolrule_webrole", "adx_webpageaccesscontrolrule_webroleid", WebPageAccessControlRuleToWebRoleNode.ColumnSet, null,
						RetrieveByLinksQueryBuilder.Create(
							new Link { Name = "adx_webpageaccesscontrolrule", FromAttribute = "adx_webpageaccesscontrolruleid", ToAttribute = "adx_webpageaccesscontrolruleid" },
							new Link { Name = "adx_webrole", FromAttribute = "adx_webroleid", ToAttribute = "adx_webroleid" }),
						BaseSolutionVersions.NaosAndOlderVersions,
						RelationshipDefinition.Create<WebPageAccessControlRuleToWebRoleNode, WebPageAccessControlRuleNode>(PortalSolutions.SolutionNames.BaseSolutionName, "adx_webpageaccesscontrolrule", "adx_webpageaccesscontrolruleid",
							BaseSolutionVersions.NaosAndOlderVersions,
							rulerole => rulerole.WebPageAccessControlRule,
							rule => rule.WebRoleIntersects,
							(rule, rulerole) => { rulerole.WebPageAccessControlRule = rule; rule.WebRoleIntersects.Add(rulerole); },
							(rule, rulerole, id) => { rulerole.WebPageAccessControlRule = id; rule.WebRoleIntersects.Remove(rulerole); }),
						RelationshipDefinition.Create<WebPageAccessControlRuleToWebRoleNode, WebRoleNode>(PortalSolutions.SolutionNames.BaseSolutionName, "adx_webrole", "adx_webroleid",
							BaseSolutionVersions.NaosAndOlderVersions,
							rulerole => rulerole.WebRole,
							role => role.WebPageAccessControlRuleIntersects,
							(role, rulerole) => { rulerole.WebRole = role; role.WebPageAccessControlRuleIntersects.Add(rulerole); },
							(role, rulerole, id) => { rulerole.WebRole = id; role.WebPageAccessControlRuleIntersects.Remove(rulerole); }))
				},
				{
					"adx_websiteaccess_webrole",
					EntityDefinition.Create<WebsiteAccessToWebRoleNode>(
						PortalSolutions.SolutionNames.BaseSolutionName, "adx_websiteaccess_webrole", "adx_websiteaccess_webroleid", WebsiteAccessToWebRoleNode.ColumnSet, null,
						RetrieveByLinksQueryBuilder.Create(
							new Link { Name = "adx_websiteaccess", FromAttribute = "adx_websiteaccessid", ToAttribute = "adx_websiteaccessid" },
							new Link { Name = "adx_webrole", FromAttribute = "adx_webroleid", ToAttribute = "adx_webroleid" }),
						BaseSolutionVersions.NaosAndOlderVersions,
						RelationshipDefinition.Create<WebsiteAccessToWebRoleNode, WebsiteAccessNode>(PortalSolutions.SolutionNames.BaseSolutionName, "adx_websiteaccess", "adx_websiteaccessid",
							BaseSolutionVersions.NaosAndOlderVersions,
							accessrole => accessrole.WebsiteAccess,
							access => access.WebRoleIntersects,
							(access, accessrole) => { accessrole.WebsiteAccess = access; access.WebRoleIntersects.Add(accessrole); },
							(access, accessrole, id) => { accessrole.WebsiteAccess = id; access.WebRoleIntersects.Remove(accessrole); }),
						RelationshipDefinition.Create<WebsiteAccessToWebRoleNode, WebRoleNode>(PortalSolutions.SolutionNames.BaseSolutionName, "adx_webrole", "adx_webroleid",
							BaseSolutionVersions.NaosAndOlderVersions,
							accessrole => accessrole.WebRole,
							role => role.WebsiteAccessIntersects,
							(role, accessrole) => { accessrole.WebRole = role; role.WebsiteAccessIntersects.Add(accessrole); },
							(role, accessrole, id) => { accessrole.WebRole = id; role.WebsiteAccessIntersects.Remove(accessrole); }))
				},

				{
					"adx_portallanguage",
					EntityDefinition.Create<PortalLanguageNode>(
						PortalSolutions.SolutionNames.BaseSolutionName, "adx_portallanguage", "adx_portallanguageid", PortalLanguageNode.ColumnSet, null,
						new RetrieveAllQueryBuilder(),
						BaseSolutionVersions.CentaurusVersion)
				},

				{
					"adx_websitelanguage",
					EntityDefinition.Create<WebsiteLanguageNode>(
						PortalSolutions.SolutionNames.BaseSolutionName, "adx_websitelanguage", "adx_websitelanguageid", WebsiteLanguageNode.ColumnSet, null,
						RetrieveByLinksQueryBuilder.Create(),
						BaseSolutionVersions.CentaurusVersion,
						RelationshipDefinition.Create<WebsiteLanguageNode, WebsiteNode>(PortalSolutions.SolutionNames.BaseSolutionName, "adx_website", "adx_websiteid",
							BaseSolutionVersions.CentaurusVersion,
							language => language.Website,
							site => site.WebsiteLanguages,
							(site, language) => { language.Website = site; site.WebsiteLanguages.Add(language); },
							(site, language, id) => { language.Website = id; site.WebsiteLanguages.Remove(language); }),
						RelationshipDefinition.Create<WebsiteLanguageNode, PublishingStateNode>(PortalSolutions.SolutionNames.BaseSolutionName, "adx_publishingstate", "adx_publishingstate",
							BaseSolutionVersions.CentaurusVersion,
							language => language.PublishingState,
							null,
							(state, language) => { language.PublishingState = state; },
							(state, language, id) => { language.PublishingState = id; }),
						RelationshipDefinition.Create<WebsiteLanguageNode, PortalLanguageNode>(PortalSolutions.SolutionNames.BaseSolutionName, "adx_portallanguage", "adx_portallanguageid",
							BaseSolutionVersions.CentaurusVersion,
							language => language.PortalLanguage,
							null,
							(portalLanguage, language) => { language.PortalLanguage = portalLanguage; },
							(portalLanguage, language, id) => { language.PortalLanguage = id; }))
				}
			},
			new Dictionary<string, ManyRelationshipDefinition>
			{
				{
					"adx_accesscontrolrule_publishingstate",
					new ManyRelationshipDefinition(PortalSolutions.SolutionNames.BaseSolutionName, "adx_accesscontrolrule_publishingstate", "adx_accesscontrolrule_publishingstate", BaseSolutionVersions.NaosAndOlderVersions)
				},
				{
					"adx_webpageaccesscontrolrule_webrole",
					new ManyRelationshipDefinition(PortalSolutions.SolutionNames.BaseSolutionName, "adx_webpageaccesscontrolrule_webrole", "adx_webpageaccesscontrolrule_webrole", BaseSolutionVersions.NaosAndOlderVersions)
				},
				{
					"adx_websiteaccess_webrole",
					new ManyRelationshipDefinition(PortalSolutions.SolutionNames.BaseSolutionName, "adx_websiteaccess_webrole", "adx_websiteaccess_webrole", BaseSolutionVersions.NaosAndOlderVersions)
				}
			});

		public static readonly IDictionary<string, SolutionDefinition> Definitions = new Dictionary<string, SolutionDefinition>
		{
			{ PortalSolutions.SolutionNames.BlogsSolutionName, Blogs },
			{ PortalSolutions.SolutionNames.CommerceSolutionName, Commerce },
			{ PortalSolutions.SolutionNames.EventsSolutionName, Events },
			{ PortalSolutions.SolutionNames.ForumsSolutionName, Forums },
			{ PortalSolutions.SolutionNames.IdeasSolutionName, Ideas },
			{ PortalSolutions.SolutionNames.WebFormsSolutionName, WebForms },
		};
	}
}
