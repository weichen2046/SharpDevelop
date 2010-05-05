// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Siegfried Pammer" email="sie_pam@gmx.at"/>
//     <version>$Revision$</version>
// </file>

using ICSharpCode.Core.Presentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Editor;
using ICSharpCode.SharpDevelop.Editor.AvalonEdit;

namespace ICSharpCode.SharpDevelop.Gui
{
	public abstract class AbstractConsolePad : AbstractPadContent, IEditable, IPositionable, ITextEditorProvider, IToolsHost
	{
		const string toolBarTreePath = "/SharpDevelop/Pads/CommonConsole/ToolBar";
		
		DockPanel panel;
		ConsoleControl console;
		ToolBar toolbar;
		
		bool cleared;
		IList<string> history;
		int historyPointer;
		
		protected AbstractConsolePad()
		{
			this.panel = new DockPanel();
			
			this.console = new ConsoleControl();
			
			// creating the toolbar accesses the WordWrap property, so we must do this after creating the console
			this.toolbar = BuildToolBar();
			this.toolbar.SetValue(DockPanel.DockProperty, Dock.Top);
			
			this.panel.Children.Add(toolbar);
			this.panel.Children.Add(console);
			
			this.history = new List<string>();
			
			this.console.editor.TextArea.PreviewKeyDown += (sender, e) => {
				e.Handled = HandleInput(e.Key);
			};
			
			this.InitializeConsole();
		}
		
		protected virtual ToolBar BuildToolBar()
		{
			return ToolBarService.CreateToolBar(this, toolBarTreePath);
		}
		
		public virtual ITextEditor TextEditor {
			get {
				return console.TextEditor;
			}
		}
		
		public override object Control {
			get { return panel; }
		}
		
		public override object InitiallyFocusedControl {
			get { return console.editor; }
		}
		
		string GetText()
		{
			return this.TextEditor.Document.Text;
		}
		
		/// <summary>
		/// Creates a snapshot of the editor content.
		/// This method is thread-safe.
		/// </summary>
		public ITextBuffer CreateSnapshot()
		{
			return new StringTextBuffer(GetText());
		}
		
		string IEditable.Text {
			get {
				return GetText();
			}
		}
		
		public virtual ICSharpCode.SharpDevelop.Editor.IDocument GetDocumentForFile(OpenedFile file)
		{
			return null;
		}
		
		#region IPositionable implementation
		void IPositionable.JumpTo(int line, int column)
		{
			this.TextEditor.JumpTo(line, column);
		}
		
		int IPositionable.Line {
			get {
				return this.TextEditor.Caret.Line;
			}
		}
		
		int IPositionable.Column {
			get {
				return this.TextEditor.Caret.Column;
			}
		}
		#endregion
		
		object IToolsHost.ToolsContent {
			get { return TextEditorSideBar.Instance; }
		}
		
		protected virtual bool HandleInput(Key key) {
			switch (key) {
				case Key.Back:
				case Key.Delete:
					if (console.editor.SelectionStart == 0 &&
					    console.editor.SelectionLength == console.editor.Document.TextLength) {
						ClearConsole();
						return true;
					}
					return false;
				case Key.Down:
					if (console.CommandText.Contains("\n"))
						return false;
					this.historyPointer = Math.Min(this.historyPointer + 1, this.history.Count);
					if (this.historyPointer == this.history.Count)
						console.CommandText = "";
					else
						console.CommandText = this.history[this.historyPointer];
					console.editor.ScrollToEnd();
					return true;
				case Key.Up:
					if (console.CommandText.Contains("\n"))
						return false;
					this.historyPointer = Math.Max(this.historyPointer - 1, 0);
					if (this.historyPointer == this.history.Count)
						console.CommandText = "";
					else
						console.CommandText = this.history[this.historyPointer];
					console.editor.ScrollToEnd();
					return true;
				case Key.Return:
					if (Keyboard.Modifiers == ModifierKeys.Shift)
						return false;
					int caretOffset = this.console.TextEditor.Caret.Offset;
					string commandText = console.CommandText;
					cleared = false;
					if (AcceptCommand(commandText)) {
						IDocument document = console.TextEditor.Document;
						if (!cleared) {
							if (document.GetCharAt(document.TextLength - 1) != '\n')
								document.Insert(document.TextLength, Environment.NewLine);
							AppendPrompt();
							console.TextEditor.Select(document.TextLength, 0);
						} else {
							console.CommandText = "";
						}
						cleared = false;
						this.history.Add(commandText);
						this.historyPointer = this.history.Count;
						console.editor.ScrollToEnd();
						return true;
					}
					return false;
				default:
					return false;
			}
		}
		
		/// <summary>
		/// Deletes the content of the console and prints a new prompt.
		/// </summary>
		public void ClearConsole()
		{
			this.console.editor.Document.Text = "";
			cleared = true;
			AppendPrompt();
		}
		
		/// <summary>
		/// Deletes the console history.
		/// </summary>
		public void DeleteHistory()
		{
			this.history.Clear();
			this.historyPointer = 0;
		}
		
		public void SetHighlighting(string language)
		{
			if (this.console != null)
				this.console.SetHighlighting(language);
		}
		
		public bool WordWrap {
			get { return this.console.editor.WordWrap; }
			set { this.console.editor.WordWrap = value; }
		}
		
		protected abstract string Prompt {
			get;
		}
		
		protected abstract bool AcceptCommand(string command);
		
		protected virtual void InitializeConsole()
		{
			AppendPrompt();
		}
		
		protected virtual void AppendPrompt()
		{
			console.Append(Prompt);
			console.SetReadonly();
			console.editor.Document.UndoStack.ClearAll();
		}
		
		protected void AppendLine(string text)
		{
			console.Append(text + Environment.NewLine);
		}
		
		protected void Append(string text)
		{
			console.Append(text);
		}
		
		protected void InsertBeforePrompt(string text)
		{
			int endOffset = this.console.readOnlyRegion.EndOffset;
			bool needScrollDown = this.console.editor.CaretOffset >= endOffset;
			this.console.editor.Document.Insert(endOffset - Prompt.Length, text);
			this.console.editor.ScrollToEnd();
			this.console.SetReadonly(endOffset + text.Length);
		}
		
		protected virtual void Clear()
		{
			this.ClearConsole();
		}
	}
	
	class ConsoleControl : Grid
	{
		internal AvalonEdit.TextEditor editor;
		internal BeginReadOnlySectionProvider readOnlyRegion;
		
		public ConsoleControl()
		{
			this.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
			this.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
			
			this.editor = Editor.AvalonEdit.AvalonEditTextEditorAdapter.CreateAvalonEditInstance();
			this.editor.SetValue(Grid.ColumnProperty, 0);
			this.editor.SetValue(Grid.RowProperty, 0);
			
			this.Children.Add(editor);
			
			editor.TextArea.ReadOnlySectionProvider = readOnlyRegion = new BeginReadOnlySectionProvider();
		}
		
		public ITextEditor TextEditor {
			get {
				return new AvalonEditTextEditorAdapter(editor);
			}
		}
		
		public void SetHighlighting(string language)
		{
			editor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition(language);
		}
		
		public void Append(string text)
		{
			editor.AppendText(text);
		}
		
		/// <summary>
		/// Sets the readonly region to a specified offset.
		/// </summary>
		public void SetReadonly(int offset)
		{
			readOnlyRegion.EndOffset = offset;
		}
		
		/// <summary>
		/// Sets the readonly region to the end of the document.
		/// </summary>
		public void SetReadonly()
		{
			readOnlyRegion.EndOffset = editor.Document.TextLength;
		}
		
		/// <summary>
		/// Gets/sets the command text displayed at the command prompt.
		/// </summary>
		public string CommandText {
			get {
				return editor.Document.GetText(new TextSegment() { StartOffset = readOnlyRegion.EndOffset, EndOffset = editor.Document.TextLength });
			}
			set {
				editor.Document.Replace(new TextSegment() { StartOffset = readOnlyRegion.EndOffset, EndOffset = editor.Document.TextLength }, value);
			}
		}
	}
	
	class BeginReadOnlySectionProvider : IReadOnlySectionProvider
	{
		public int EndOffset { get; set; }
		
		public bool CanInsert(int offset)
		{
			return offset >= EndOffset;
		}
		
		public IEnumerable<ISegment> GetDeletableSegments(ISegment segment)
		{
			if (segment.EndOffset < this.EndOffset)
				return Enumerable.Empty<ISegment>();
			
			return new[] {
				new TextSegment() {
					StartOffset = Math.Max(this.EndOffset, segment.Offset),
					EndOffset = segment.EndOffset
				}
			};
		}
	}
	
	class ClearConsoleCommand : AbstractMenuCommand
	{
		public override void Run()
		{
			var pad = this.Owner as AbstractConsolePad;
			if (pad != null)
				pad.ClearConsole();
		}
	}
	
	class DeleteHistoryCommand : AbstractMenuCommand
	{
		public override void Run()
		{
			var pad = this.Owner as AbstractConsolePad;
			if (pad != null)
				pad.DeleteHistory();
		}
	}
	
	class ToggleConsoleWordWrapCommand : AbstractCheckableMenuCommand
	{
		AbstractConsolePad pad;
		
		public override object Owner {
			get { return base.Owner; }
			set {
				if (!(value is AbstractConsolePad))
					throw new Exception("Owner has to be a AbstractConsolePad");
				pad = value as AbstractConsolePad;
				base.Owner = value;
			}
		}
		
		public override bool IsChecked {
			get { return pad.WordWrap; }
			set { pad.WordWrap = value; }
		}
		
		public override void Run()
		{
			IsChecked = !IsChecked;
		}
	}
}