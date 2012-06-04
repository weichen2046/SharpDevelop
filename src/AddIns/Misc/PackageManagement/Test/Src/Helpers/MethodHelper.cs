﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using ICSharpCode.SharpDevelop.Dom;
using Rhino.Mocks;

namespace PackageManagement.Tests.Helpers
{
	public class MethodHelper
	{
		public IMethod Method;
		public ProjectContentHelper ProjectContentHelper = new ProjectContentHelper();
		
		/// <summary>
		/// Method name should include class prefix (e.g. "Class1.MyMethod")
		/// </summary>
		public void CreateMethod(string fullyQualifiedName)
		{
			Method = MockRepository.GenerateMock<IMethod, IEntity>();
			Method.Stub(m => m.ProjectContent).Return(ProjectContentHelper.FakeProjectContent);
			Method.Stub(m => m.FullyQualifiedName).Return(fullyQualifiedName);
		}
		
		public void CreatePublicMethod(string name)
		{
			CreateMethod(name);
			Method.Stub(m => m.IsPublic).Return(true);
		}
		
		public void CreatePrivateMethod(string name)
		{
			CreateMethod(name);
			Method.Stub(m => m.IsPublic).Return(false);
			Method.Stub(m => m.IsPrivate).Return(true);
		}
		
		public void FunctionStartsAtColumn(int column)
		{
			var region = new DomRegion(1, column);
			FunctionStartsAt(region);
		}
		
		public void FunctionStartsAt(DomRegion region)
		{
			Method.Stub(m => m.Region).Return(region);
		}
		
		public void FunctionStartsAtLine(int line)
		{
			var region = new DomRegion(line, 1);
			FunctionStartsAt(region);
		}
		
		public void FunctionBodyEndsAtColumn(int column)
		{
			var region = new DomRegion(1, 1, 1, column);
			FunctionBodyEndsAt(region);
		}
		
		void FunctionBodyEndsAt(DomRegion region)
		{
			Method.Stub(m => m.BodyRegion).Return(region);
		}
		
		public void FunctionBodyEndsAtLine(int line)
		{
			var region = new DomRegion(1, 1, line, 1);
			FunctionBodyEndsAt(region);
		}
		
		public void SetRegion(DomRegion region)
		{
			Method.Stub(m => m.Region).Return(region);
		}
		
		public void SetBodyRegion(DomRegion region)
		{
			Method.Stub(m => m.BodyRegion).Return(region);
		}
		
		public void SetCompilationUnitFileName(string fileName)
		{
			ICompilationUnit unit = MockRepository.GenerateStub<ICompilationUnit>();
			unit.FileName = fileName;
			Method.Stub(m => m.CompilationUnit).Return(unit);
		}
	}
}