﻿// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Matthew Ward" email="mrward@users.sourceforge.net"/>
//     <version>$Revision$</version>
// </file>

using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Threading;
using Microsoft.Scripting.Hosting.Shell;
using ICSharpCode.RubyBinding;
using NUnit.Framework;
using RubyBinding.Tests.Utils;

namespace RubyBinding.Tests.Console
{
	/// <summary>
	/// The Home Key should return the user to the start of the line after the prompt.
	/// </summary>
	[TestFixture]
	public class RubyConsoleHomeKeyTestFixture : RubyConsoleTestsBase
	{
		string prompt = ">>> ";

		[SetUp]
		public void Init()
		{
			base.CreateRubyConsole();
			TestableRubyConsole.Write(prompt, Style.Prompt);
		}
		
		[Test]
		public void HomeKeyPressedWhenNoUserTextInConsole()
		{
			MockConsoleTextEditor.RaisePreviewKeyDownEventForDialogKey(Key.Home);
		
			int expectedColumn = prompt.Length;
			Assert.AreEqual(expectedColumn, MockConsoleTextEditor.Column);
		}
		
		[Test]
		public void HomeKeyPressedWhenTextInConsole()
		{
			MockConsoleTextEditor.RaisePreviewKeyDownEvent(Key.A);
			MockConsoleTextEditor.RaisePreviewKeyDownEventForDialogKey(Key.Home);

			int expectedColumn = prompt.Length;
			Assert.AreEqual(expectedColumn, MockConsoleTextEditor.Column);
		}
	}
}
