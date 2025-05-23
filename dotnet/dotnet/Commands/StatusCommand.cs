using Spectre.Console;
using System; 
using System.Threading.Tasks;
using DotnetCli.Core; 

namespace DotnetCli.Commands
{
    public class StatusCommand : ICommand
    {
        private readonly CliApplication _app; 

        public StatusCommand(CliApplication app) 
        {
            _app = app;
        }

        public string Name => "status";
        public string Description => "Shows the current application and background task status.";

        public Task ExecuteAsync(string[] args)
        {
            var bgTaskStatus = _app.GetBackgroundTaskStatus(); 
            AnsiConsole.MarkupLine($"[blue]Application status: OK. Background task is {bgTaskStatus}.[/]");
            return Task.CompletedTask;
        }
    }
}
