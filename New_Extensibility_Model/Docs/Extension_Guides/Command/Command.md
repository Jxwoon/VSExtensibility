# Commands

Commands trigger actions in Visual Studio. They manifest as buttons parented to menus and/or toolbars in the IDE with their execution tied to user interaction. Commands in the new Extensibility Model run asynchronously and so the user can continue to interact with IDE while commands are executing.

## Creating new commands

To get started: 

* Add a reference to the [Microsoft.VisualStudio.Extensibility.Sdk](https://www.nuget.org/packages/Microsoft.VisualStudio.Extensibility.Sdk) and [Microsoft.VisualStudio.Extensibility.Build](https://www.nuget.org/packages/Microsoft.VisualStudio.Extensibility.Build) NuGet packages to your project.
* Change your project's `TargetFramework` from `net6.0` to `net6.0-windows`.

### Registering a command

Creating a command with the new Extensibility Model is as simple as extending the base class `Microsoft.VisualStudio.Extensibility.Commands.Command` and adorning your class with the attribute `Microsoft.VisualStudio.Extensibility.Commands.CommandAttribute`.

The attribute [Microsoft.VisualStudio.Extensibility.Commands.CommandAttribute](./../../APIs/Microsoft.VisualStudio.Extensibility.md/#commandattribute-type) has a few parameters that you should become familiar with:

| Parameter | Type | Required | Description |
| --------- |----- | -------- | ----------- |
| Name | String | Yes | A globally unique identifier for the command. It is recommended to use the full class name of your command here. |
| Id | ushort | Yes | A locally unique identifier for your command within your extension. Each command within your extension should use a different value. |
| DisplayName | String | Yes | The default display name of your command. Surround this string with the '%' character to enable localizing this string. See more on this at [Localizing a command](#localizing-a-command). |
| ContainerType | Type? | No | The type that is to act as the CommandSet for this command. Setting this parameter to null automatically generates a default CommandSet for your command. |
| Placement | KnownCommandPlacement | No | Indicates where within Visual Studio your command should be parented. If no placement is provided the command defaults to being parented to the Standard toolbar. |
| ClientContext | String | No | Client contexts requested by the command, separated by ','. By default the Shell and Editor contexts are returned. A client context is a snapshot of specific IDE states at the time a command was originally executed. Since these commands are executed asynchronously this state could change between the time the user executed the command and the command handler running. See more on this at [Client contexts](./../../Inside_the_SDK/Activation_Constraints.md/#client-contexts). |

```csharp
	[Command(CommandName, CommandId, "Sample Remote Command", placement: KnownCommandPlacement.ToolsMenu)]
	public class CommandHandler : Command
	{
		private const ushort CommandId = 1;
		private const string CommandName = "SimpleRemoteCommandSample.Command";

		public CommandHandler(VisualStudioExtensibility extensibility, TraceSource traceSource, ushort id)
			: base(extensibility, id)
		{
		}

		public override Task ExecuteCommandAsync(IClientContext context, CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}
	}
```

See the [InsertGuidSample](./../InsertGuidSample.md) sample to get started with creating an extension with a command.

### Adding an icon

Commands support adding icons to their menu item in addition to or instead of the display name of the command. To add an icon to your command, add the attribute [Microsoft.VisualStudio.Extensibility.Commands.CommandIconAttribute](./../../APIs/Microsoft.VisualStudio.Extensibility.md/#commandiconattribute-type) to your command class. Currently you can use any of the [KnownMonikers](https://docs.microsoft.com/en-us/dotnet/api/microsoft.visualstudio.imaging.knownmonikers?view=visualstudiosdk-2022) currently supported by Visual Studio. Custom monikers are not supported at this time.

| Parameter | Type | Required | Description |
| --------- |----- | -------- | ----------- |
| ImageMoniker | String | Yes | The name of any of the [KnownMonikers](https://docs.microsoft.com/en-us/dotnet/api/microsoft.visualstudio.imaging.knownmonikers?view=visualstudiosdk-2022) currently supported by Visual Studio, with or without the `KnownMonikers` type name. |
| IconSettings | IconSettings | Yes | Configures how the command will be displayed. For example `IconSettings.IconAndText` displays the icon alongside the command's display name, whereas `IconSettings.IconOnly` will only show the command's icon and not its DisplayName if parented to a toolbar. |

```csharp
	[CommandIcon("Extension", IconSettings.IconAndText)]
```

### Controlling command visibility

The visibility of a command can be controlled by adding the attribute [Microsoft.VisualStudio.Extensibility.Commands.CommandVisibleWhenAttribute](./../../APIs/Microsoft.VisualStudio.Extensibility.md/#commandvisiblewhenattribute-type) to your command class. This attribute supports specifying an expression, defining a set of terms used in the expression, and what values those terms should be replaced with upon evaluation. Note: Term names and values are mapped to their index in the array. i.e. the term name at index 0 corresponds with the term value at that same index. The command would be visible when the expression evaluates to true, and invisible when it is false.

If this attribute is omitted from your command, the default is for the command to always be visible.

An example of such an expression can be seen here:
```csharp
	// This command would become visible when an editor for a file with any file extension is active.
	[CommandVisibleWhen(
		expression: "AnyFile",
		termNames: new string[] { "AnyFile" },
		termValues: new string[] { "ClientContext:Shell.ActiveEditorContentType=.+" })]
```

To see more information on valid term values:
- [Using rule based activation constraints](./../../Inside_the_SDK/Activation_Constraints.md/#rule-based-activation-constraints)

### Controlling command Enabled/Disabled state

The visibility of a command can be controlled by adding the attribute [Microsoft.VisualStudio.Extensibility.Commands.CommandEnabledWhenAttribute](./../../APIs/Microsoft.VisualStudio.Extensibility.md/#commandenabledwhenattribute-type) to your command class. This attribute supports specifying an expression, defining a set of terms used in the expression, and what values those terms should be replaced with upon evaluation. Note: Term names and values are mapped to their index in the array. i.e. the term name at index 0 corresponds with the term value at that same index. The command would be enabled when the expression evaluates to true, and disabled when it is false. 

If this attribute is omitted from your command, the default is for the command to always be enabled. You can also automatically have your command be disabled if it is currently executing by setting `this.DisableDuringExecution = true;` in the constructor of your command class. Setting this property will override the enabled/disabled state defined by the `Microsoft.VisualStudio.Extensibility.Commands.CommandEnabledWhenAttribute` while the command is being executed.

An example of such an expression can be seen here:
```csharp
	// This command would become enabled when a solution is loaded in the IDE and a file with the file extension ".jpg", ".jpeg", or ".txt" is selected in the Solution Explorer.
	[CommandEnabledWhen(
		expression: "SolutionLoaded & IsValidFile",
		termNames: new string[] { "SolutionLoaded", "IsValidFile" },
		termValues: new string[] { "SolutionState:Exists", "ClientContext:Shell.ActiveSelectionFileName=(.jpg|.jpeg|.txt)$" })]
```

To see more information on valid term values:
- [Using rule based activation constraints](./../../Inside_the_SDK/Activation_Constraints.md/#rule-based-activation-constraints)

### Localizing a command

The text displayed on a command can be localized by including `string-resources.json` files with your extension and formatting the DisplayName parameter with the '%' character on either end in your [Microsoft.VisualStudio.Extensibility.Commands.CommandAttribute](./../../APIs/Microsoft.VisualStudio.Extensibility.md/#commandattribute-type).

Localized Command DisplayName
```csharp
	[Command(CommandName, CommandId, "%Microsoft.VisualStudio.MyExtension.SampleRemoteCommand.DisplayName%", placement: KnownCommandPlacement.ToolsMenu)]
```

#### string-resources.json

Your extension should provide a `string-resources.json` file for every language that your extension supports. This JSON file is a dictionary of key/value pairs where the key is a globally (all of Visual Studio) unique identifier for a string resource and the value is the localized string resource. These JSON files should be deployed with your extension under the ".vsextension" directory, with each language you support being shipped in a folder matching the name of the locale i.e. "de" for German, "it" for Italian, etc. The `string-resources.json` deployed at the root of the ".vsextension" directory is used as the default if your extension does not support the language that Visual Studio is currently set to. An example of what this directory structure would look like can be seen here:

![Localization directory structure](localizing-a-command.PNG "Localization directory structure")

string-resources.json sample:
```json
	{
		"Microsoft.VisualStudio.MyExtension.SampleRemoteCommand.DisplayName": "Sample Remote Command",
		"Microsoft.VisualStudio.MyExtension.OutputWindowTest.DisplayName": "Output Window Test"
	}
```