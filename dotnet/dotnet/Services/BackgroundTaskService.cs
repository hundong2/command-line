using Spectre.Console;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DotnetCli.Services
{
    public class BackgroundTaskService
    {
        private readonly CancellationToken _cancellationToken;
        private Task? _executingTask;

        public BackgroundTaskService(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
        }

        public void Start()
        {
            _executingTask = RunBackgroundTaskAsync();
        }

        private async Task RunBackgroundTaskAsync()
        {
            AnsiConsole.MarkupLine("[grey]Background task service started.[/]");
            try
            {
                while (!_cancellationToken.IsCancellationRequested)
                {
                    AnsiConsole.MarkupLineInterpolated($"[dim]Background service tick: {DateTime.Now} (Thread: {Thread.CurrentThread.ManagedThreadId})[/]");
                    await Task.Delay(5000, _cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                // This is the expected exception when cancellation is requested.
                AnsiConsole.MarkupLine("[yellow]Background task service gracefully cancelled.[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Background task service error: {ex.Message}[/]");
            }
            finally
            {
                AnsiConsole.MarkupLine("[grey]Background task service stopped.[/]");
            }
        }

        public Task? GetExecutingTask() => _executingTask;
        
        public string GetStatus()
        {
            if (_executingTask == null) return "Not Started";
            if (_executingTask.IsFaulted) return $"Faulted ({_executingTask.Exception?.InnerExceptions.FirstOrDefault()?.Message})";
            if (_executingTask.IsCanceled) return "Cancelled";
            if (_executingTask.IsCompletedSuccessfully) return "Completed Successfully";
            return _executingTask.Status.ToString(); // e.g., Running, WaitingToRun
        }
    }
}
