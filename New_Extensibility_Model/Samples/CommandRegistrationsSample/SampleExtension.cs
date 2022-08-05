﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace SampleCommandExtension;

using System.Threading;
using System.Threading.Tasks;
using Microsoft;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;
using Microsoft.VisualStudio.Extensibility.Definitions;

/// <summary>
/// Sample extension entry point for registering a custom command handler.
/// </summary>
[CommandsPackage("Microsoft.VisualStudio.MyExtension.SampleCommandSetService", "1.0")]
[CommandsPackageLoad("IsCSharp", new string[] { "IsCSharp" }, new string[] { "SolutionHasProjectCapability:CSharp" })]
public class SampleExtension : Extension
{
	private const string SampleCommandOutputChannelIdentifier = nameof(SampleExtension);

	/// <inheritdoc />
	public override Task InitializeCommandsAsync(CommandSetBase commandSet)
	{
		Requires.NotNull(commandSet, nameof(commandSet));

		// We can register a command via an execution delegate.
#pragma warning disable VSTHRD103 // Call async methods when in an async method, async alternative is for command instances.
		var command = commandSet.RegisterCommand("Microsoft.VisualStudio.MyExtension.WriteToOutputWindow", this.WriteToOutputWindowAsync);
#pragma warning restore VSTHRD103 // Call async methods when in an async method

		// or register through the CommandAttribute (see SampleCommand2.cs).
		return base.InitializeCommandsAsync(commandSet);
	}

	private async Task WriteToOutputWindowAsync(Command command, IClientContext context, CancellationToken cancellationToken)
	{
		var existingName = command.DisplayName;
		command.DisplayName = "Executing Command...";

		try
		{
			// While this is showing intended usage, code doesn't work because service hub is not using multiplexed streams as of now.
			using var channel = await command.Extensibility.Views().Output.GetChannelAsync(SampleCommandOutputChannelIdentifier, nameof(Resources.OutputWindowName), cancellationToken);
			Assumes.NotNull(channel);
			await channel.Writer.WriteLineAsync("Executing command from sample command set...");
		}
		finally
		{
			command.DisplayName = existingName;
		}
	}
}
