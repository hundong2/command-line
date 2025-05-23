using System.Threading.Tasks;
using DotnetCli.Core; 

namespace DotnetCli.Commands
{
    public class ExitCommand : ICommand
    {
        private readonly CliApplication _app;

        public ExitCommand(CliApplication app)
        {
            _app = app;
        }

        public string Name => "exit";
        public string Description => "Exits the application gracefully.";

        public Task ExecuteAsync(string[] args)
        {
            _app.RequestShutdown(); 
            return Task.CompletedTask;
        }
    }
}
