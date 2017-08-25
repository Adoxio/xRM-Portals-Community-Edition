/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Cms
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Runtime.CompilerServices;
	using System.Threading;
	using Microsoft.Xrm.Sdk;
    using Adxstudio.Xrm.Diagnostics.Trace;
    using Adxstudio.Xrm.Resources;
	using AspNet.Cms;

	public enum ContentMapLockType
	{
		Read,
		Write,
	}

	[Serializable]
	public class ContentMap : Dictionary<string, IDictionary<EntityReference, EntityNode>>
	{
		private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

		public SolutionDefinition Solution { get; private set; }

		public TimeSpan LockTimeout { get; set; }

		public ContentMap(SolutionDefinition solution)
		{
			Solution = solution;
			LockTimeout = TimeSpan.FromMinutes(1);
		}

		public string GetSnippet(string name, ContextLanguageInfo language)
		{
			var snippet = this.GetSnippetNode(name, language);
			return snippet != null ? snippet.Value : null;
		}

		public ContentSnippetNode GetSnippetNode(string name, ContextLanguageInfo language)
		{
			IDictionary<EntityReference, EntityNode> snippets;
			// Note: Content snippets can have a language matching the current language, or they can have no language and apply for all languages
			var userLanguageEntity = language == null || !language.IsCrmMultiLanguageEnabled ? null : language.ContextLanguage.EntityReference;

			if (this.TryGetValue("adx_contentsnippet", out snippets))
			{
				var snippet = GetSnippetNode(snippets.Values.OfType<ContentSnippetNode>(), name, language);

				ADXTrace.Instance.TraceInfo(
					TraceCategory.Application,
					snippet == null
						? string.Format(
							"Does not exist: SnippetName={0}, UserLanguageId={1}, UserLanguageName={2}",
							name,
							userLanguageEntity == null ? null : userLanguageEntity.Id.ToString(),
							userLanguageEntity == null ? null : userLanguageEntity.Name)
						: string.Format(
							"Exist: SnippetName={0}, UserLanguageId={1}, UserLanguageName={2}, SnippetLanguageId={3}, SnippetLanguageName={4}",
							name,
							userLanguageEntity == null ? null : userLanguageEntity.Id.ToString(),
							userLanguageEntity == null ? null : userLanguageEntity.Name,
							snippet.Language == null ? null : snippet.Language.Id.ToString(),
							snippet.Language == null ? null : snippet.Language.Name));

				return snippet;
			}

			ADXTrace.Instance.TraceInfo(
				TraceCategory.Application,
				string.Format(
					"No content snippets found. SnippetName={0}, UserLanguageId={1}, UserLanguageName={2}",
					name,
					userLanguageEntity == null ? null : userLanguageEntity.Id.ToString(),
					userLanguageEntity == null ? null : userLanguageEntity.Name));

			return null;
		}

		private static ContentSnippetNode GetSnippetNode(IEnumerable<ContentSnippetNode> snippets, string name, ContextLanguageInfo language)
		{
			if (language != null && language.IsCrmMultiLanguageEnabled)
			{
				// first try to match on a language specific one. if that doesn't exist, try to match on language agnostic one
				return snippets.FirstOrDefault(s => string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase) && s.Language != null && s.Language.Id == language.ContextLanguage.EntityReference.Id)
					?? snippets.FirstOrDefault(s => string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase) && s.Language == null);
			}

			var snippet = snippets.FirstOrDefault(s => string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));

			return snippet;
		}

		public T Using<T>(ContentMapLockType lockType, Func<T> action,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			var result = default(T);
			Using(lockType, () => { result = action(); }, memberName, sourceFilePath, sourceLineNumber);
			return result;
		}

		public void Using(ContentMapLockType lockType, Action action,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0)
		{
			var timer = Stopwatch.StartNew();
			var contentMapCallId = Guid.NewGuid();

			CmsEventSource.Log.ContentMapLockStatus(lockType, "Requested", timer.ElapsedMilliseconds);
			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("ContentMap[{0}]: LockType - {1}, Requested - {2}", contentMapCallId, lockType, timer.ElapsedMilliseconds),
				memberName, sourceFilePath, sourceLineNumber);

			if (lockType == ContentMapLockType.Read && !_lock.TryEnterReadLock(LockTimeout))
			{
				TraceLock("Using", _lock);
				CmsEventSource.Log.ContentMapLockTimeout(lockType, _lock);
				ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("ContentMap[{0}]: Failed to acquire read lock on content map. LockType - {1}", contentMapCallId, lockType),
					memberName, sourceFilePath, sourceLineNumber);

				throw new TimeoutException("Failed to acquire read lock on content map.");
			}

			if (lockType == ContentMapLockType.Write && !_lock.TryEnterWriteLock(LockTimeout))
			{
				TraceLock("Using", _lock);
				CmsEventSource.Log.ContentMapLockTimeout(lockType, _lock);
				ADXTrace.Instance.TraceError(TraceCategory.Application, string.Format("ContentMap[{0}]: A write lock couldn't be acquired on the content map. LockType - {1}", contentMapCallId, lockType),
					memberName, sourceFilePath, sourceLineNumber);

				throw new TimeoutException("A write lock couldn't be acquired on the content map.");
			}

			try
			{
				CmsEventSource.Log.ContentMapLockStatus(lockType, "Acquired", timer.ElapsedMilliseconds);
				ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("ContentMap[{0}]: LockType - {1}, Acquired - {2}", contentMapCallId, lockType, timer.ElapsedMilliseconds),
					memberName, sourceFilePath, sourceLineNumber);
				action();
			}
			finally
			{
				if (lockType == ContentMapLockType.Read) _lock.ExitReadLock();
				if (lockType == ContentMapLockType.Write) _lock.ExitWriteLock();

				timer.Stop();

				CmsEventSource.Log.ContentMapLockStatus(lockType, "Released", timer.ElapsedMilliseconds);
				ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("ContentMap[{0}]: LockType - {1}, Released - {2}", contentMapCallId, lockType, timer.ElapsedMilliseconds),
					memberName, sourceFilePath, sourceLineNumber);
			}
		}

		public bool TryGetValue<T>(EntityNode reference, out T node)
			where T : EntityNode
		{
			if (reference != null)
			{
				return TryGetValue(reference.ToEntityReference(), out node);
			}

			node = null;
			return false;
		}

		public bool TryGetValue<T>(Entity entity, out T node)
			where T : EntityNode
		{
			if (entity != null)
			{
				return TryGetValue(entity.ToEntityReference(), out node);
			}

			node = null;
			return false;
		}

		public bool TryGetValue<T>(EntityReference reference, out T node)
			where T : EntityNode
		{
			EntityNode entityNode;

			if (TryGetValue(reference, out entityNode))
			{
				node = entityNode as T;

				return node != null;
			}

			node = null;
			return false;
		}

		private bool TryGetValue(EntityReference reference, out EntityNode node)
		{
			if (reference != null)
			{
				IDictionary<EntityReference, EntityNode> lookup;

				if (TryGetValue(reference.LogicalName, out lookup))
				{
					return lookup.TryGetValue(reference, out node);
				}
			}

			node = null;
			return false;
		}

		public void AddRange(IEnumerable<Entity> entities)
		{
			foreach (var entity in entities)
			{
				AddOrGetExisting(entity);
			}
		}

		public EntityNode AddOrGetExisting(Entity entity)
		{
			var reference = entity.ToEntityReference();

			// retrieve the global lookup for the entity

			EntityNode node;

			if (!TryGetValue(reference, out node))
			{
				EntityDefinition ed;

				if (Solution.Entities.TryGetValue(reference.LogicalName, out ed))
				{
					// create a new node for the entity

					node = ed.ToNode(entity);

					// associate the entity nodes

					Merge(ed, reference, node);
				}
			}
			else
			{
				ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Exists: LogicalName={0}, Id={1}", EntityNamePrivacy.GetEntityName(entity.LogicalName), entity.Id));
			}

			return node;
		}

		public EntityNode Replace(Entity entity)
		{
			entity.ThrowOnNull("entity");

			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("LogicalName={0}, Id={1}", EntityNamePrivacy.GetEntityName(entity.LogicalName), entity.Id));

			var reference = entity.ToEntityReference();

			// remove the existing node

			var removed = Unmerge(reference, ToEntityIdentifier);

			if (removed != null)
			{
				ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Removed: Id={0}", removed.Id));
			}

			// add the new node

			var added = AddOrGetExisting(entity);

			if (added != null)
			{
				ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Added: Id={0}", added.Id));
			}

			return added;
		}

		public void RemoveRange(IEnumerable<EntityReference> references)
		{
			foreach (var reference in references)
			{
				Remove(reference);
			}
		}

		public EntityNode Remove(EntityReference entityReference)
		{
			entityReference.ThrowOnNull("entityReference");

			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("LogicalName={0}, Id={1}", EntityNamePrivacy.GetEntityName(entityReference.LogicalName), entityReference.Id));

			var removed = Unmerge(entityReference, _ => null);

			if (removed != null)
			{
				ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Removed: Id={0}", removed.Id));
			}

			return removed;
		}

		public EntityNode Deactivate(EntityReference entityReference)
		{
			entityReference.ThrowOnNull("entityReference");

			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("LogicalName={0}, Id={1}", EntityNamePrivacy.GetEntityName(entityReference.LogicalName), entityReference.Id));

			var removed = Unmerge(entityReference, ToEntityIdentifier);

			if (removed != null)
			{
				ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Removed: Id={0}", removed.Id));
			}

			return removed;
		}

		private void AddToLookup(EntityReference reference, EntityNode node)
		{
			// add the node to the appropriate entity set lookup

			IDictionary<EntityReference, EntityNode> lookup;

			if (!TryGetValue(reference.LogicalName, out lookup))
			{
				// create a new entity set lookup

				lookup = new Dictionary<EntityReference, EntityNode>();
				Add(reference.LogicalName, lookup);
			}

			lookup.Add(reference, node);
		}

		private bool RemoveFromLookup(EntityReference reference)
		{
			IDictionary<EntityReference, EntityNode> lookup;

			if (TryGetValue(reference.LogicalName, out lookup))
			{
				var result = lookup.Remove(reference);

				if (result && !lookup.Any())
				{
					// cleanup the entity set

					Remove(reference.LogicalName);
				}

				return result;
			}

			return false;
		}

		private void Merge(EntityDefinition ed, EntityReference reference, EntityNode node)
		{
			AddToLookup(reference, node);

			if (ed.Relationships != null)
			{
				// process the N:1 relationships

				foreach (var relationship in ed.Relationships.Where(r => r.ToOne != null))
				{
					// try retrieve the foreign node from the given foreign reference

					var foreignReference = relationship.ToOne(node);

					EntityNode foreignNode;

					if (TryGetValue(foreignReference, out foreignNode) && foreignNode != null && !foreignNode.IsReference)
					{
						// associate the foreign node to the new node

						relationship.Associate(foreignNode, node);
					}
				}
			}

			if (!node.IsReference)
			{
				// scan over the existing (foreign) nodes for unassigned N:1 relationships that should point to the current node

				ForEachForeignRelationshipToNode(node, (relationship, otherNode) => relationship.Associate(node, otherNode));
			}
		}

		private EntityNode Unmerge(EntityReference entityReference, Func<EntityNode, EntityNode> getId)
		{
			EntityNode originalNode;

			if (TryGetValue(entityReference, out originalNode))
			{
				EntityDefinition ed;

				if (Solution.Entities.TryGetValue(entityReference.LogicalName, out ed))
				{
					// reset the N:1 relationships of the original node

					if (ed.Relationships != null)
					{
						foreach (var relationship in ed.Relationships.Where(r => r.ToOne != null && r.Disassociate != null))
						{
							// remove the original node from the 1:N collection of the foreign node

							var foreignNode = relationship.ToOne(originalNode);

							if (foreignNode != null && !foreignNode.IsReference)
							{
								// disassociate using the relationship definition

								relationship.Disassociate(foreignNode, originalNode, getId(foreignNode));
							}
						}
					}
				}
				else
				{
					ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Unknown: LogicalName={0}, Id={1}", EntityNamePrivacy.GetEntityName(entityReference.LogicalName), entityReference.Id));
				}

				if (!originalNode.IsReference)
				{
					// reset the 1:N relationships of the original node

					ForEachForeignRelationshipToNode(originalNode, (relationship, otherNode) => relationship.Disassociate(originalNode, otherNode, getId(originalNode)));
				}

				// remove the node from the map

				if (RemoveFromLookup(entityReference))
				{
					return originalNode;
				}
			}

			return null;
		}

		private void ForEachForeignRelationshipToNode(EntityNode node, Action<RelationshipDefinition, EntityNode> action)
		{
			var reference = node.ToEntityReference();

			// scan over the existing nodes of the map

			foreach (var lookup in this)
			{
				// find the entity descriptor for the current node type

				var logicalName = lookup.Key;
				EntityDefinition ed;

				if (Solution.Entities.TryGetValue(logicalName, out ed))
				{
					if (ed.Relationships != null)
					{
						// find the relationships that point to the current node type

						foreach (var relationship in ed.Relationships.Where(r => r.ForeignEntityLogicalname == reference.LogicalName && r.ToOne != null))
						{
							// scan over the nodes of the entity set

							foreach (var otherNode in lookup.Value.Values)
							{
								// determine if the foreign node points to the current node or not

								var foreignNode = relationship.ToOne(otherNode);

								if (foreignNode != null && Equals(foreignNode.ToEntityReference(), reference))
								{
									action(relationship, otherNode);
								}
							}
						}
					}
				}
				else
				{
                    ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format("Unknown: logicalName={0}", EntityNamePrivacy.GetEntityName(logicalName)));
				}
			}
		}

		private static EntityNode ToEntityIdentifier(EntityNode node)
		{
			var identifier = node != null
				? Activator.CreateInstance(node.GetType(), node.ToEntityReference()) as EntityNode
				: null;

			return identifier;
		}

		private static void TraceLock(string memberName, ReaderWriterLockSlim rwl)
		{
			ADXTrace.Instance.TraceInfo(TraceCategory.Application, string.Format(memberName, "ReaderWriterLockSlim: CurrentReadCount={0}, IsReadLockHeld={1}, IsWriteLockHeld={2}, WaitingReadCount={3}, WaitingUpgradeCount={4}, WaitingWriteCount={5}",
				rwl.CurrentReadCount, rwl.IsReadLockHeld, rwl.IsWriteLockHeld,
				rwl.WaitingReadCount, rwl.WaitingUpgradeCount, rwl.WaitingWriteCount));
		}
	}
}
