using System.Threading.Tasks; // Required for async Main
using DotnetCli.Core;       // Required for CliApplication

// This is a top-level statements program.
// The Main method is implicitly defined.
// To make it async, we just use 'await' at the top level.

var app = new CliApplication();
await app.RunAsync();
