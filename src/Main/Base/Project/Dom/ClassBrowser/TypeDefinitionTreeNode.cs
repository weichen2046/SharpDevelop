﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using System.Security.Policy;
using ICSharpCode.Core.Presentation;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.Utils;
using ICSharpCode.TreeView;
using ICSharpCode.SharpDevelop.Dom;

namespace ICSharpCode.SharpDevelop.Dom.ClassBrowser
{
	public class TypeDefinitionTreeNode : ModelCollectionTreeNode
	{
		static readonly IComparer<SharpTreeNode> TypeMemberNodeComparer =  new TypeDefinitionMemberNodeComparer();
		ITypeDefinitionModel definition;
		
		public TypeDefinitionTreeNode(ITypeDefinitionModel definition)
		{
			if (definition == null)
				throw new ArgumentNullException("definition");
			this.definition = definition;
		}
		
		protected override object GetModel()
		{
			return definition;
		}
		
		public override object Icon {
			// TODO why do I have to resolve this?
			get {
				return ClassBrowserIconService.GetIcon(definition.Resolve()).ImageSource;
			}
		}
		
		public override object Text {
			get {
				return definition.Name;
			}
		}
		
		protected override IComparer<SharpTreeNode> NodeComparer {
			get {
				return TypeMemberNodeComparer;
			}
		}
		
		protected override IModelCollection<object> ModelChildren {
			get {
				return definition.NestedTypes.Concat<object>(definition.Members);
			}
		}
		
		protected override void LoadChildren()
		{
			base.LoadChildren();
			var baseTypesTreeNode = new BaseTypesTreeNode(definition);
			Children.Insert(0, baseTypesTreeNode);
		}
		
		public override void ActivateItem(System.Windows.RoutedEventArgs e)
		{
			var target = definition.Resolve();
			if (target != null)
				NavigationService.NavigateTo(target);
		}
		
		public override void ShowContextMenu()
		{
			var entityModel = this.Model as IEntityModel;
			if ((entityModel != null) && (entityModel.ParentProject != null)) {
				var ctx = MenuService.ShowContextMenu(null, entityModel, "/SharpDevelop/EntityContextMenu");
			}
		}
		
		class TypeDefinitionMemberNodeComparer : IComparer<SharpTreeNode>
		{
			public int Compare(SharpTreeNode x, SharpTreeNode y)
			{
				var a = x.Model as IMemberModel;
				var b = y.Model as IMemberModel;
				
				if (a == null && b == null)
					return NodeTextComparer.Compare(x, y);
				if (a == null)
					return -1;
				if (b == null)
					return 1;
				
				if (a.SymbolKind < b.SymbolKind)
					return -1;
				if (a.SymbolKind > b.SymbolKind)
					return 1;
				
				return NodeTextComparer.Compare(x, y);
			}
		}
	}
}

