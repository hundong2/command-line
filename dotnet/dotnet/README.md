# .NET Interactive CLI Application

This is a .NET 8 command-line application that provides an interactive shell.
It runs a background task concurrently and processes user commands until exited.

## Features

- Interactive command loop.
- Background task running concurrently.
- Graceful shutdown via 'exit' command or Ctrl+C.
- Uses Spectre.Console for enhanced console output.

## How to Build and Run

1.  Navigate to the `dotnet/dotnet` directory.
2.  Build the application:
    ```bash
    dotnet build
    ```
3.  Run the application:
    ```bash
    dotnet run
    ```

## Available Commands

- `hello`: Displays a greeting message.
- `status`: Shows the current application and background task status.
- `exit`: Exits the application gracefully.

Press `Ctrl+C` at any time to gracefully shut down the application.
