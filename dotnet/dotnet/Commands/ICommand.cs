using System.Threading.Tasks;

namespace DotnetCli.Commands
{
    public interface ICommand
    {
        string Name { get; }
        string Description { get; } // Added for better help/listing later
        Task ExecuteAsync(string[] args);
    }
}
