# EditInVSCode

A Visual Studio extension that lets you open any file directly in VS Code for editing.

This is mainly useful when working with AI tools in Visual Studio: some AI integrations require a default editor to be set for a file type, but you may still want to actually edit that file in VS Code (a common example being Razor files).

## Requirements

- Visual Studio 2022 (or later) with the **Visual Studio extension development** workload installed.
- [Visual Studio Code](https://code.visualstudio.com/) installed and available on the system `PATH` (as the `code` command).

## Building

1. Clone the repository:
   ```powershell
   git clone https://github.com/leedavi/EditInVSCode
   ```
2. Open `EditInVSCode.slnx` (or `EditInVSCode.csproj`) in Visual Studio.
3. Build the solution (**Build > Build Solution**, or `Ctrl+Shift+B`).
   - This produces an `EditInVSCode.vsix` file in the project's `bin\Debug` (or `bin\Release`) folder.

You can also build from the command line using a Developer PowerShell/Command Prompt for Visual Studio:
```powershell
msbuild EditInVSCode.csproj /p:Configuration=Release
```

## Running / Debugging

To try the extension without installing it permanently:

1. Open the solution in Visual Studio.
2. Press `F5` (or select **Debug > Start Debugging**).
3. This launches an **Experimental Instance** of Visual Studio with the extension already loaded, so you can test your changes immediately.

## Installing

To install the extension into your regular Visual Studio instance:

1. Build the project in `Release` mode (see [Building](#building)) to produce the `.vsix` file, or download it from the [Releases](https://github.com/leedavi/EditInVSCode/releases) page.
2. Close Visual Studio.
3. Double-click the `.vsix` file to launch the VSIX Installer and follow the prompts.
4. Restart Visual Studio when prompted.

Alternatively, once published, you can install it directly from within Visual Studio via **Extensions > Manage Extensions** by searching for "EditInVSCode".

## Uninstalling

1. In Visual Studio, go to **Extensions > Manage Extensions**.
2. Select the **Installed** tab and locate **EditInVSCode**.
3. Click **Uninstall**, then restart Visual Studio when prompted to complete the removal.