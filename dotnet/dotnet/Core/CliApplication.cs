using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq; // Required for FirstOrDefault, Skip, ToArray
using System.Threading;
using System.Threading.Tasks;
using DotnetCli.Commands; // Required for ICommand and concrete command classes
using DotnetCli.Services; // Required for BackgroundTaskService

namespace DotnetCli.Core
{
    public class CliApplication
    {
        private readonly CancellationTokenSource _cts;
        private readonly Dictionary<string, ICommand> _commands;
        private BackgroundTaskService _taskService; // New field for the service

        public CliApplication()
        {
            _cts = new CancellationTokenSource();

            _commands = new Dictionary<string, ICommand>(StringComparer.OrdinalIgnoreCase)
            {
                { new HelloCommand().Name, new HelloCommand() },
                { new StatusCommand(this).Name, new StatusCommand(this) },
                { new ExitCommand(this).Name, new ExitCommand(this) }
            };

            Console.CancelKeyPress += (sender, e) =>
            {
                AnsiConsole.MarkupLine("[yellow]Ctrl+C detected. Initiating graceful shutdown...[/]");
                e.Cancel = true; 
                RequestShutdown(); 
            };
        }

        public void RequestShutdown()
        {
            if (!_cts.IsCancellationRequested)
            {
                _cts.Cancel();
            }
        }

        // Modified to use BackgroundTaskService
        public string GetBackgroundTaskStatus()
        {
            return _taskService?.GetStatus() ?? "Service not initialized";
        }

        public async Task RunAsync()
        {
            AnsiConsole.MarkupLine("[bold blue]Welcome to the .NET Interactive Agent (Service-based Task)![/]");

            // Instantiate and start the BackgroundTaskService
            _taskService = new BackgroundTaskService(_cts.Token);
            _taskService.Start();

            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    if (_cts.IsCancellationRequested)
                    {
                        AnsiConsole.MarkupLine("[dim]Cancellation detected before asking for input. Breaking loop.[/]");
                        break;
                    }

                    var input = AnsiConsole.Ask<string>("[grey]>[/] ");
                    var parts = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    
                    var commandName = parts.FirstOrDefault()?.ToLowerInvariant();
                    var commandArgs = parts.Skip(1).ToArray();

                    if (string.IsNullOrEmpty(commandName))
                    {
                        continue; 
                    }

                    if (_commands.TryGetValue(commandName, out var commandToExecute))
                    {
                        await commandToExecute.ExecuteAsync(commandArgs);
                    }
                    else
                    {
                        AnsiConsole.MarkupLine($"[red]Unknown command: {commandName}[/]");
                    }
                }
                catch (InvalidOperationException ex) when (ex.Message.Contains("non-interactive mode"))
                {
                    AnsiConsole.MarkupLine("[red]Cannot read input in non-interactive mode. Exiting loop.[/]");
                    RequestShutdown();
                    break;
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]An error occurred in the input loop: {ex.Message}[/]");
                    AnsiConsole.MarkupLine("[red]Initiating shutdown due to error.[/]");
                    RequestShutdown();
                    break;
                }
            }

            // Cleanup logic
            AnsiConsole.MarkupLine("[dim]Application loop finished. Proceeding with shutdown...[/]");
            RequestShutdown(); 

            // Get the task from the service and wait for it
            var backgroundTask = _taskService.GetExecutingTask();
            if (backgroundTask != null)
            {
                AnsiConsole.MarkupLine("[dim]Waiting for background task service to complete...[/]");
                try
                {
                    // Using Wait with a timeout for cleanup
                    if (!backgroundTask.Wait(TimeSpan.FromSeconds(7))) 
                    {
                        AnsiConsole.MarkupLine("[red]Background task service did not complete in time.[/]");
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[green]Background task service completed successfully.[/]");
                    }
                }
                catch (OperationCanceledException)
                {
                    // This might occur if the task itself was directly cancelled by its own token,
                    // though here it's more likely due to the main CTS.
                    AnsiConsole.MarkupLine("[green]Background task service was cancelled and completed.[/]");
                }
                catch (AggregateException ae) when (ae.InnerExceptions.Any(e => e is TaskCanceledException || e is OperationCanceledException))
                {
                    AnsiConsole.MarkupLine("[green]Background task service was cancelled (via AggregateException) and completed.[/]");
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Error waiting for background task service: {ex.Message}[/]");
                }
            }
            AnsiConsole.MarkupLine("[bold blue]Application shut down (Service-based Task).[/]");
        }

        // RunBackgroundTaskAsync method is now removed from CliApplication
    }
}
