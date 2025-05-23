// See https://aka.ms/new-console-template for more information
using Spectre.Console;
using System; // Required for ConsoleCancelEventArgs
using System.Threading;
using System.Threading.Tasks;

AnsiConsole.MarkupLine("[bold blue]Welcome to the .NET Interactive Agent![/]");

var cts = new CancellationTokenSource();

// Subscribe to the Ctrl+C event
Console.CancelKeyPress += (sender, e) =>
{
    AnsiConsole.MarkupLine("[yellow]Ctrl+C detected. Initiating graceful shutdown...[/]");
    e.Cancel = true; // Prevent immediate termination
    if (!cts.IsCancellationRequested)
    {
        cts.Cancel();
    }
};

Task backgroundTask = RunBackgroundTaskAsync(cts.Token);

// Main application loop
while (!cts.IsCancellationRequested)
{
    try
    {
        if (cts.IsCancellationRequested)
        {
            AnsiConsole.MarkupLine("[dim]Cancellation detected before asking for input. Breaking loop.[/]");
            break;
        }

        var input = AnsiConsole.Ask<string>("[grey]>[/] ");
        var command = input.ToLowerInvariant();

        if (command == "exit")
        {
            AnsiConsole.MarkupLine("[yellow]\"exit\" command received. Initiating shutdown...[/]");
            if (!cts.IsCancellationRequested)
            {
                cts.Cancel(); // Signal cancellation
            }
            // The loop condition (!cts.IsCancellationRequested) will handle the break
        }
        else if (command == "hello")
        {
            AnsiConsole.MarkupLine("[green]Hello there![/]");
        }
        else if (command == "status")
        {
            // Check if background task is still running (not faulted, not cancelled, not completed)
            string taskStatusMessage = backgroundTask.IsCompletedSuccessfully ? "completed" : 
                                       backgroundTask.IsCanceled ? "cancelled" :
                                       backgroundTask.IsFaulted ? "faulted" : 
                                       "running";
            AnsiConsole.MarkupLine($"[blue]Application status: OK. Background task is {taskStatusMessage}.[/]");
        }
        else
        {
            AnsiConsole.MarkupLine($"[red]Unknown command: {input}[/]");
        }
    }
    catch (InvalidOperationException ex) when (ex.Message.Contains("non-interactive mode"))
    {
        AnsiConsole.MarkupLine("[red]Cannot read input in non-interactive mode. Exiting loop.[/]");
        if (!cts.IsCancellationRequested)
        {
            cts.Cancel(); 
        }
        break; 
    }
    catch (Exception ex) 
    {
        AnsiConsole.MarkupLine($"[red]An error occurred in the input loop: {ex.Message}[/]");
        AnsiConsole.MarkupLine("[red]Initiating shutdown due to error.[/]");
        if (!cts.IsCancellationRequested)
        {
            cts.Cancel();
        }
        break; 
    }
}

// Cleanup logic
AnsiConsole.MarkupLine("[dim]Application loop finished. Proceeding with shutdown...[/]");
if (!cts.IsCancellationRequested)
{
    cts.Cancel();
}

AnsiConsole.MarkupLine("[dim]Waiting for background task to complete...[/]");
try
{
    if (!backgroundTask.Wait(TimeSpan.FromSeconds(7))) 
    {
        AnsiConsole.MarkupLine("[red]Background task did not complete in time.[/]");
    }
    else
    {
        AnsiConsole.MarkupLine("[green]Background task completed successfully.[/]");
    }
}
catch (OperationCanceledException)
{
    AnsiConsole.MarkupLine("[green]Background task was cancelled and completed.[/]");
}
catch (AggregateException ae) when (ae.InnerExceptions.Any(e => e is TaskCanceledException || e is OperationCanceledException))
{
    AnsiConsole.MarkupLine("[green]Background task was cancelled (via AggregateException) and completed.[/]");
}
catch (Exception ex)
{
    AnsiConsole.MarkupLine($"[red]Error waiting for background task: {ex.Message}[/]");
}

AnsiConsole.MarkupLine("[bold blue]Application shut down.[/]");


static async Task RunBackgroundTaskAsync(CancellationToken cancellationToken)
{
    AnsiConsole.MarkupLineInterpolated($"[dim]Background task started. Will tick every 5 seconds. Press Ctrl+C or type 'exit' to quit.[/]");
    int tickCount = 0;
    while (!cancellationToken.IsCancellationRequested)
    {
        tickCount++;
        try
        {
            AnsiConsole.MarkupLineInterpolated($"[dim]Background task tick #{tickCount}: {DateTime.Now}[/]");
            await Task.Delay(5000, cancellationToken);
        }
        catch (TaskCanceledException)
        {
            AnsiConsole.MarkupLineInterpolated($"[dim]Background task cancellation requested during delay (tick #{tickCount}). Exiting loop.[/]");
            break; 
        }
        catch (OperationCanceledException)
        {
            AnsiConsole.MarkupLineInterpolated($"[dim]Background task operation cancelled (tick #{tickCount}). Exiting loop.[/]");
            break;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLineInterpolated($"[red]Error in background task (tick #{tickCount}): {ex.Message}[/]");
        }
    }
    AnsiConsole.MarkupLineInterpolated($"[dim]Background task finished after {tickCount} ticks.[/]");
}
