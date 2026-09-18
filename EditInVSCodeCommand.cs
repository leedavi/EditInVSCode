using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Linq;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace EditInVSCode
{
    /// <summary>
    /// Command handler that opens the currently selected file(s) in the Solution Explorer using Visual Studio Code.
    /// </summary>
    internal sealed class EditInVSCodeCommand
    {
        public const int CommandId = 0x0100;
        public const int CommandIdProject = 0x0101;

        public static readonly Guid CommandSet = new Guid("c1a89f0e-3d3b-4b2e-8f7e-6f2a1e3c9d21");

        private readonly AsyncPackage package;

        private EditInVSCodeCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new OleMenuCommand(this.Execute, menuCommandID);
            menuItem.BeforeQueryStatus += this.OnBeforeQueryStatus;
            commandService.AddCommand(menuItem);

            var projectCommandID = new CommandID(CommandSet, CommandIdProject);
            var projectMenuItem = new OleMenuCommand(this.Execute, projectCommandID);
            projectMenuItem.BeforeQueryStatus += this.OnBeforeQueryStatus;
            commandService.AddCommand(projectMenuItem);
        }

        public static EditInVSCodeCommand Instance
        {
            get;
            private set;
        }

        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider => this.package;

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new EditInVSCodeCommand(package, commandService);
        }

        private void OnBeforeQueryStatus(object sender, EventArgs e)
        {
            var command = sender as OleMenuCommand;
            if (command == null)
            {
                return;
            }

            ThreadHelper.ThrowIfNotOnUIThread();

            var paths = this.GetSelectedPaths();
            command.Visible = paths.Any();
            command.Enabled = command.Visible;
        }

        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var paths = this.GetSelectedPaths();
            foreach (var path in paths)
            {
                OpenInVSCode(path);
            }
        }

        private static void OpenInVSCode(string path)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "code",
                    Arguments = $"\"{path}\"",
                    UseShellExecute = true,
                    CreateNoWindow = true,
                };
                System.Diagnostics.Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                VsShellUtilities.ShowMessageBox(
                    ServiceProviderForDialog,
                    $"Failed to open '{path}' in Visual Studio Code. Make sure 'code' is available on your PATH.\n\n{ex.Message}",
                    "Edit in VS Code",
                    OLEMSGICON.OLEMSGICON_WARNING,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
            }
        }

        private static IServiceProvider ServiceProviderForDialog => (IServiceProvider)Instance?.package;

        private string[] GetSelectedPaths()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var dte = ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                return await this.package.GetServiceAsync(typeof(DTE)) as DTE;
            });

            if (dte?.SelectedItems == null)
            {
                return Array.Empty<string>();
            }

            var paths = new System.Collections.Generic.List<string>();

            foreach (SelectedItem item in dte.SelectedItems)
            {
                string path = null;

                if (item.ProjectItem != null)
                {
                    try
                    {
                        path = item.ProjectItem.FileNames[1];
                    }
                    catch
                    {
                        // ignore items without a file (e.g. folders without physical paths)
                    }
                }
                else if (item.Project != null)
                {
                    path = item.Project.FullName;
                }

                if (!string.IsNullOrEmpty(path) && (File.Exists(path) || Directory.Exists(path)))
                {
                    paths.Add(path);
                }
            }

            return paths.ToArray();
        }
    }
}
