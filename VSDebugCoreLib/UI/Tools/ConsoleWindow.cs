using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudio.Utilities;
using VSDebugCoreLib.Console;
using VSDebugCoreLib.Utils;
using DefGuidList = Microsoft.VisualStudio.Editor.DefGuidList;
//scomponentmodel
//editor
//text
//Text
//IVsTextView
// for content type
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
using UserControl = System.Windows.Controls.UserControl;

namespace VSDebugCoreLib.UI.Tools
{
    [Guid("4EC19E3D-EFB0-41D7-B08D-79B00BECF072")]
    public class ConsoleWindow : ToolWindowPane
    {
        private ConsoleEngine _engine;
        private VSDebugContext _context;

        private IWpfTextView _textView;
        private IWpfTextViewHost _textViewHost;

        // This command service is used to hide the one created in the base class because
        // there is no way to add a parent command target to it, so we will have to create
        // a new one and return it in our version of GetService.
        private OleMenuCommandService commandService;

        // commands history
        private HistoryBuffer history;
        private ITextBuffer mefTextBuffer;

        // The text view used to visualize the text inside the console
        private IVsTextView textView;

        private UserControl uc;

        public ConsoleWindow()
            : base(null)
        {
            ToolBar = new CommandID(GuidList.GuidVSDebugProConsoleMenu, (int) PkgCmdIDList.VSDConsoleTbar);
            Caption = "VSD Console";
            // Set the image that will appear on the tab of the window frame
            // when docked with an other window
            // The resource ID correspond to the one defined in the resx file
            // while the Index is the offset in the bitmap strip. Each image in
            // the strip being 16x16.
            BitmapResourceID = 301;
            BitmapIndex = 1;
            //
        }

        public ConsoleEngine Engine
        {
            get => _engine;
            set
            {
                _engine = value;
                _engine.StdOut = mefTextBuffer;
            }
        }

        public VSDebugContext Context
        {
            get => _context;
            set => _context = value;
        }

        public override object Content
        {
            get
            {
                if (uc == null)
                {
                    uc = new UserControl();
                    uc.Content = _textViewHost.HostControl;
                }

                return uc;
            }
        }

        private IContentType GetContentType(IContentTypeRegistryService registryService)
        {
            var contentType = registryService.GetContentType(VSDContentTypeDefinition.ContentType);
            if (contentType == null)
            {
                // If the content type is not available, we can try to create it
                contentType = registryService.AddContentType(VSDContentTypeDefinition.ContentType, new[] { "code" });
            }
            return contentType;
        }

        protected override void Initialize()
        {
            base.Initialize();

            var compMod = GetService(typeof(SComponentModel)) as IComponentModel;
            if (compMod != null)
            {
                var adapterFactory = compMod.GetService<IVsEditorAdaptersFactoryService>();
                var registryService = compMod.GetService<IContentTypeRegistryService>();

                textView = adapterFactory.CreateVsTextViewAdapter(
                    GetService(typeof(IOleServiceProvider)) as IOleServiceProvider);
                var textBuffer =
                    adapterFactory.CreateVsTextBufferAdapter(
                        GetService(typeof(IOleServiceProvider)) as IOleServiceProvider);
                var textViewInitFlags = (uint) TextViewInitFlags.VIF_DEFAULT |
                                        (uint) TextViewInitFlags.VIF_HSCROLL |
                                        (uint) TextViewInitFlags.VIF_VSCROLL;
                textBuffer.InitializeContent("", 0);
                textView.Initialize(textBuffer as IVsTextLines, IntPtr.Zero, textViewInitFlags, null);

                // Create Dev10 objects
                _textView = adapterFactory.GetWpfTextView(textView);
                mefTextBuffer = adapterFactory.GetDataBuffer(textBuffer);

                var userData = textView as IVsUserData;
                if (userData != null)
                {
                    var g = DefGuidList.guidIWpfTextViewHost;
                    object obj;
                    var hr = userData.GetData(ref g, out obj);
                    if (hr == VSConstants.S_OK) _textViewHost = obj as IWpfTextViewHost;
                }

                // disable some view properties
                _textView.Options.SetOptionValue(DefaultTextViewHostOptions.ZoomControlId, false);
                _textView.Options.SetOptionValue(DefaultTextViewHostOptions.GlyphMarginId, false);
                _textView.Options.SetOptionValue(DefaultTextViewHostOptions.SuggestionMarginId, false);
                _textView.Options.SetOptionValue(DefaultTextViewHostOptions.ChangeTrackingId, false);
                _textView.Options.SetOptionValue(DefaultTextViewHostOptions.LineNumberMarginId, false);
                _textView.Options.SetOptionValue(DefaultTextViewHostOptions.ShowErrorsOptionId, false);
                _textView.Options.SetOptionValue(DefaultTextViewHostOptions.LineEndingMarginOptionId, false);
                _textView.Options.SetOptionValue(DefaultTextViewHostOptions.ShowChangeTrackingMarginOptionId, false);
                _textView.Options.SetOptionValue(DefaultTextViewHostOptions.IndentationCharacterMarginOptionId, false);
                _textView.Options.SetOptionValue(DefaultTextViewHostOptions.EnableFileHealthIndicatorOptionId, false);
                _textView.Options.SetOptionValue(DefaultTextViewHostOptions.RowColMarginOptionId, false);
                

                //Initialize the history
                if (null == history)
                    history = new HistoryBuffer();

                adapterFactory.GetWpfTextView(textView).Caret.MoveTo(new SnapshotPoint(mefTextBuffer.CurrentSnapshot,
                    mefTextBuffer.CurrentSnapshot.Length));

                var ivsdContentType = GetContentType(registryService);

                mefTextBuffer.ChangeContentType(ivsdContentType, null);
            }

            // Fetch motd
            var jtf = new JoinableTaskFactory(ThreadHelper.JoinableTaskContext);
            async System.Threading.Tasks.Task UpdateMotdAsync()
            {
                string txtRecord = await MiscHelpers.GetDnsTxtRecordAsync(Resources.MotdUrl);

                await jtf.SwitchToMainThreadAsync();
                if (txtRecord != null && txtRecord != "*")
                {
                    // insert message and add new line
                    mefTextBuffer.Insert(mefTextBuffer.CurrentSnapshot.Length, txtRecord);
                    mefTextBuffer.Insert(mefTextBuffer.CurrentSnapshot.Length, "\n");

                    // make the buffer readonly
                    ExtendReadOnlyRegion();
                }
            }
            jtf.Run(async () => await UpdateMotdAsync());

            // init console input
            WriteConsoleInputSymbol();
        }

        /// <summary>
        ///     Performs the clean-up operations for this object.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    // Close the text view.
                    if (null != textView)
                    {
                        // Remove the command filter.
                        textView.RemoveCommandFilter(this);
                        // Release the text view.
                        textView.CloseView();
                        textView = null;
                    }

                    // Dispose the command service.
                    if (null != commandService)
                    {
                        commandService.Dispose();
                        commandService = null;
                    }
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        /// <summary>
        ///     Set the cursor at the end of the current buffer and, if needed, scrolls the text
        ///     view so that the cursor is visible.
        /// </summary>
        private void SetCursorAtEndOfBuffer()
        {
            // If the text view is not created, then there is no reason to set the cursor.
            if (null != _textView)
            {
                _textView.Caret.MoveTo(new SnapshotPoint(mefTextBuffer.CurrentSnapshot,
                    mefTextBuffer.CurrentSnapshot.Length));

                _textView.Caret.EnsureVisible();
            }
        }

        /// <summary>
        ///     Return the service of the given type.
        ///     This override is needed to be able to use a different command service from the one
        ///     implemented in the base class.
        /// </summary>
        protected override object GetService(Type serviceType)
        {
            if (typeof(IOleCommandTarget) == serviceType ||
                typeof(IMenuCommandService) == serviceType)
                if (null != commandService)
                    return commandService;
            return base.GetService(serviceType);
        }

        /// <summary>
        ///     Function called when the window frame is set on this tool window.
        /// </summary>
        public override void OnToolWindowCreated()
        {
            // Call the base class's implementation.
            base.OnToolWindowCreated();

            // Register this object as command filter for the text view so that it will
            // be possible to intercept some command.
            IOleCommandTarget originalFilter;
            ErrorHandler.ThrowOnFailure(
                textView.AddCommandFilter(this, out originalFilter));
            // Create a command service that will use the previous command target
            // as parent target and will route to it the commands that it can not handle.
            if (null == originalFilter)
                commandService = new OleMenuCommandService(this);
            else
                commandService = new OleMenuCommandService(this, originalFilter);

            // Add the command handler for RETURN.
            var id = new CommandID(
                typeof(VSConstants.VSStd2KCmdID).GUID,
                (int) VSConstants.VSStd2KCmdID.RETURN);
            var cmd = new OleMenuCommand(OnReturn, id);
            cmd.BeforeQueryStatus += UnsupportedOnCompletion;
            commandService.AddCommand(cmd);

            // Command handler for UP and DOWN arrows. These commands are needed to implement
            // the history in the console, but at the moment the implementation is empty.
            id = new CommandID(typeof(VSConstants.VSStd2KCmdID).GUID,
                (int) VSConstants.VSStd2KCmdID.UP);
            cmd = new OleMenuCommand(OnHistory, id);
            cmd.BeforeQueryStatus += SupportCommandOnInputPosition;
            commandService.AddCommand(cmd);
            id = new CommandID(typeof(VSConstants.VSStd2KCmdID).GUID,
                (int) VSConstants.VSStd2KCmdID.DOWN);
            cmd = new OleMenuCommand(OnHistory, id);
            cmd.BeforeQueryStatus += SupportCommandOnInputPosition;
            commandService.AddCommand(cmd);

            // Command handler for the LEFT arrow. This command handler is needed in order to
            // avoid that the user uses the left arrow to move to the previous line or over the
            // command prompt.
            id = new CommandID(typeof(VSConstants.VSStd2KCmdID).GUID,
                (int) VSConstants.VSStd2KCmdID.LEFT);
            cmd = new OleMenuCommand(OnNoAction, id);
            cmd.BeforeQueryStatus += OnBeforeMoveLeft;
            commandService.AddCommand(cmd);

            // Handle also the HOME command.
            id = new CommandID(typeof(VSConstants.VSStd2KCmdID).GUID,
                (int) VSConstants.VSStd2KCmdID.BOL);
            cmd = new OleMenuCommand(OnHome, id);
            commandService.AddCommand(cmd);

            id = new CommandID(typeof(VSConstants.VSStd2KCmdID).GUID,
                (int) VSConstants.VSStd2KCmdID.BOL_EXT);
            cmd = new OleMenuCommand(OnShiftHome, id);
            cmd.BeforeQueryStatus += SupportCommandOnInputPosition;
            commandService.AddCommand(cmd);

            // Adding support for "Clear Pane" command.
            id = new CommandID(typeof(VSConstants.VSStd97CmdID).GUID,
                (int) VSConstants.VSStd97CmdID.ClearPane);
            cmd = new OleMenuCommand(OnClearPane, id);
            commandService.AddCommand(cmd);

            // Add a command handler for the context menu.
            id = new CommandID(typeof(VSConstants.VSStd2KCmdID).GUID,
                (int) VSConstants.VSStd2KCmdID.SHOWCONTEXTMENU);
            cmd = new OleMenuCommand(ShowContextMenu, id);
            commandService.AddCommand(cmd);

            // Now we set the key binding for this frame to the same value as the text editor
            // so that there will be the same mapping for the commands.
            var commandUiGuid = VSConstants.GUID_TextEditorFactory;
            ((IVsWindowFrame) Frame).SetGuidProperty((int) __VSFPROPID.VSFPROPID_InheritKeyBindings, ref commandUiGuid);
        }

        /// <summary>
        ///     Return true if the user is currently on the input line.
        ///     Here we assume that the input line is always the last one.
        /// </summary>
        private bool IsCurrentLineInputLine()
        {
            return mefTextBuffer.CurrentSnapshot.GetLineFromLineNumber(mefTextBuffer.CurrentSnapshot.LineCount - 1)
                .ExtentIncludingLineBreak.Contains(_textView.Caret.Position.BufferPosition - 1);
        }

        /// <summary>
        ///     Returns true if the current position is inside the writable section of the buffer.
        /// </summary>
        private bool IsCurrentPositionInputPosition()
        {
            return !mefTextBuffer.IsReadOnly(_textView.Caret.Position.BufferPosition.Position);
        }

        public string TextOfLine(int line, int endColumn)
        {
            var mefLine = mefTextBuffer.CurrentSnapshot.GetLineFromLineNumber(line);
            var start = 0;
            if (mefTextBuffer.IsReadOnly(mefLine.Extent.Span))
                start = GetReadOnlyLength(mefTextBuffer.CurrentSnapshot) - mefLine.Start;
            return mefLine.GetText().Substring(start, endColumn);
        }

        public string GetInputLine()
        {
            var readOnlyLength = GetReadOnlyLength(mefTextBuffer.CurrentSnapshot);
            return mefTextBuffer.CurrentSnapshot.GetText(readOnlyLength,
                mefTextBuffer.CurrentSnapshot.Length - readOnlyLength);
        }

        public void SaveHistory(CCmdHistory cmdHistory)
        {
            try
            {
                cmdHistory.Values = history.Cmds;
            }
            catch (Exception)
            {
            }
        }

        public void LoadHistory(CCmdHistory cmdHistory)
        {
            try
            {
                foreach (var item in cmdHistory.Values) history.AddEntry(item);
            }
            catch (Exception)
            {
            }
        }

        public void ExecuteLast()
        {
            if (GetInputLine().Length == 0)
            {
                string strInput = _engine.LastValidInput;
                if (strInput != null)
                {
                    mefTextBuffer.Insert(mefTextBuffer.CurrentSnapshot.Length, strInput);
                    mefTextBuffer.Insert(mefTextBuffer.CurrentSnapshot.Length, "\n");
                    if (_engine != null)
                    {
                        _engine.ExecuteLast();

                        AfterConsoleExecute();
                    }
                }
            }
        }

        public void WriteInfo(string strInfo)
        {
            if (strInfo != null)
            {
                // write new line
                mefTextBuffer.Insert(mefTextBuffer.CurrentSnapshot.Length, "\n");
                mefTextBuffer.Insert(mefTextBuffer.CurrentSnapshot.Length, strInfo);
                mefTextBuffer.Insert(mefTextBuffer.CurrentSnapshot.Length, "\n");

                AfterConsoleExecute();
            }
        }

        #region Command Handlers

        /// <summary>
        ///     Set the Supported property on the sender command to true if and only if the
        ///     current position of the cursor is an input position.
        /// </summary>
        private void SupportCommandOnInputPosition(object sender, EventArgs args)
        {
            // Check if the sender is a MenuCommand.
            var command = sender as MenuCommand;
            if (null == command) return;
        }

        /// <summary>
        ///     Set the status of the command to Unsupported when the completion window is visible.
        /// </summary>
        private void UnsupportedOnCompletion(object sender, EventArgs args)
        {
            var command = sender as MenuCommand;
            if (null == command) return;
        }

        /// <summary>
        ///     Command handler for the history commands.
        ///     The standard implementation of a console has a history function implemented when
        ///     the user presses the UP or DOWN key.
        /// </summary>
        private void OnHistory(object sender, EventArgs e)
        {
            //if (!completionBroker.IsCompletionActive(_textView))
            // {
            // Get the command to figure out from the ID if we have to get the previous or the
            // next element in the history.
            var command = sender as OleMenuCommand;
            if (null == command ||
                command.CommandID.Guid != typeof(VSConstants.VSStd2KCmdID).GUID)
                return;
            string historyEntry = null;
            if (command.CommandID.ID == (int) VSConstants.VSStd2KCmdID.UP)
                historyEntry = history.PreviousEntry();
            else if (command.CommandID.ID == (int) VSConstants.VSStd2KCmdID.DOWN) historyEntry = history.NextEntry();
            if (string.IsNullOrEmpty(historyEntry)) return;

            var start = GetReadOnlyLength(mefTextBuffer.CurrentSnapshot);
            if (!mefTextBuffer.EditInProgress)
            {
                var edit = mefTextBuffer.CreateEdit();
                edit.Replace(new Span(start, mefTextBuffer.CurrentSnapshot.Length - start), historyEntry);
                edit.Apply();
            }

            // }
        }

        /// <summary>
        ///     Handles the HOME command in two different ways if the current line is the input
        ///     line or not.
        /// </summary>
        private void OnHome(object sender, EventArgs e)
        {
            if (IsCurrentLineInputLine())
                _textView.Caret.MoveTo(new SnapshotPoint(mefTextBuffer.CurrentSnapshot,
                    GetReadOnlyLength(mefTextBuffer.CurrentSnapshot)));
            else
                _textView.Caret.MoveTo(_textView.Caret.ContainingTextViewLine.Start);
        }

        /// <summary>
        ///     Overwrite the default 'Shift' + 'HOME' to limit the selection to the input section
        ///     of the buffer.
        /// </summary>
        private void OnShiftHome(object sender, EventArgs args)
        {
            SnapshotPoint start;
            if (IsCurrentLineInputLine())
                start = new SnapshotPoint(mefTextBuffer.CurrentSnapshot,
                    GetReadOnlyLength(mefTextBuffer.CurrentSnapshot));
            else
                start = _textView.Caret.ContainingTextViewLine.Start;
            _textView.Selection.Select(new SnapshotSpan(start, _textView.Caret.Position.BufferPosition), true);
        }

        /// <summary>
        ///     Determines whether it is possible to move left on the current line.
        ///     It is used to avoid a situation where the user moves over the console's prompt.
        /// </summary>
        private void OnBeforeMoveLeft(object sender, EventArgs e)
        {
            // Verify that the sender is of the expected type.
            var command = sender as OleMenuCommand;
            if (null == command) return;
            // As default we don't want to handle this command because it should be handled
            // by the dafault implementation of the text view.
            command.Supported = false;

            if (IsCurrentLineInputLine())
                if (_textView.Caret.Position.BufferPosition.Position <=
                    GetReadOnlyLength(mefTextBuffer.CurrentSnapshot))
                    command.Supported = true;
        }

        /// <summary>
        ///     Empty command handler used to overwrite some standard command with an empty action.
        /// </summary>
        private void OnNoAction(object sender, EventArgs e)
        {
            // Do Nothing.
        }

        /// <summary>
        ///     Command handler for the RETURN command.
        ///     It is called when the user presses the ENTER key inside the console window
        /// </summary>
        private void OnReturn(object sender, EventArgs e)
        {
            ExecuteUserInput();

            AfterConsoleExecute();
        }

        private void ExecuteUserInput()
        {
            var strInput = GetInputLine();

            // write new line
            mefTextBuffer.Insert(mefTextBuffer.CurrentSnapshot.Length, "\n");

            if (_engine != null) _engine.Execute(strInput);

            if (strInput != string.Empty)
            {
                history.AddEntry(strInput);
                SaveHistory(Context.Settings.CmdHistory);
                Context.SettingsManager.SaveSettings();
            }
        }

        private void WriteConsoleInputSymbol()
        {
            // write console input character
            mefTextBuffer.Insert(mefTextBuffer.CurrentSnapshot.Length, ">");

            // make the buffer readonly
            ExtendReadOnlyRegion();

            SetCursorAtEndOfBuffer();
        }

        private void AfterConsoleExecute()
        {
            // write the console input character
            WriteConsoleInputSymbol();

            // make the buffer readonly
            ExtendReadOnlyRegion();

            SetCursorAtEndOfBuffer();
        }

        private void ExtendReadOnlyRegion()
        {
            if (!mefTextBuffer.EditInProgress)
            {
                if (readOnlyRegion != null)
                {
                    var readOnlyRemove = mefTextBuffer.CreateReadOnlyRegionEdit();
                    readOnlyRemove.RemoveReadOnlyRegion(readOnlyRegion);
                    readOnlyRemove.Apply();
                }

                var readOnlyEdit = mefTextBuffer.CreateReadOnlyRegionEdit();
                readOnlyRegion = readOnlyEdit.CreateReadOnlyRegion(new Span(0, mefTextBuffer.CurrentSnapshot.Length));
                readOnlyEdit.Apply();
            }
        }

        private IReadOnlyRegion readOnlyRegion;

        internal void ClearReadOnlyRegion()
        {
            var readOnlyRemove = mefTextBuffer.CreateReadOnlyRegionEdit();
            readOnlyRemove.RemoveReadOnlyRegion(readOnlyRegion);
            readOnlyRemove.Apply();
            readOnlyRegion = null;

            var edit = mefTextBuffer.CreateEdit();
            edit.Delete(new Span(0, mefTextBuffer.CurrentSnapshot.Length)); //, string.Empty);
            edit.Insert(0, ">");
            edit.Apply();

            ExtendReadOnlyRegion();
        }

        private int GetReadOnlyLength(ITextSnapshot textSnapshot)
        {
            var max = 0;
            foreach (var region in textSnapshot.TextBuffer.GetReadOnlyExtents(new Span(0, textSnapshot.Length)))
                max = max < region.End ? region.End : max;
            return max;
        }

        /// <summary>
        ///     Function called when the user select the "Clear Pane" menu item from the context menu.
        ///     This will clear the content of the console window leaving only the console cursor and
        ///     resizing the read-only region.
        /// </summary>
        private void OnClearPane(object sender, EventArgs args)
        {
            if (!mefTextBuffer.EditInProgress)
            {
                ClearReadOnlyRegion();
                SetCursorAtEndOfBuffer();
            }
        }

        private void ShowContextMenu(object sender, EventArgs args)
        {
            // Get a reference to the UIShell.
            var uiShell = GetService(typeof(SVsUIShell)) as IVsUIShell;
            if (null == uiShell) return;

            // Get the position of the cursor.
            var pt = Cursor.Position;
            var pnts = new POINTS[1];
            pnts[0].x = (short) pt.X;
            pnts[0].y = (short) pt.Y;

            // Show the menu.
            var menuGuid = GuidList.GuidVSDebugProConsoleMenu;
            ErrorHandler.ThrowOnFailure(
                uiShell.ShowContextMenu(0, ref menuGuid, (int) PkgCmdIDList.VSDConsoleContext, pnts,
                    textView as IOleCommandTarget));
        }

        #endregion Command Handlers
    }
}