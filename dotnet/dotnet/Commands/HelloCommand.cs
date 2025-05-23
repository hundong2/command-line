using Spectre.Console;
using System.Threading.Tasks;

namespace DotnetCli.Commands
{
    public class HelloCommand : ICommand
    {
        public string Name => "hello";
        public string Description => "Displays a greeting message.";

        public Task ExecuteAsync(string[] args)
        {
            AnsiConsole.MarkupLine("[green]Hello there, from the refactored command![/]");
            return Task.CompletedTask;
        }
    }
}
