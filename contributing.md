# Contributing Guide

## Setup
Install the [.NET SDK](https://dotnet.microsoft.com/en-us/download) (latest LTS recommended).

## Issues & Discussions
Open an issue or discussion if something is broken, unclear, or missing.

- Issues: https://github.com/jinek/Consolonia/issues  
- Discussions: https://github.com/jinek/Consolonia/discussions  

Before reporting a bug, check if it belongs to AvaloniaUI:  
https://github.com/AvaloniaUI/Avalonia  

If yes — report it there instead.

## Code Style
- We use [dotnet analyzers](https://docs.microsoft.com/en-us/dotnet/framework/code-analyzers)  
- Warnings = errors in `RELEASE` (Check the `RELEASE` mode to compile fine before pushing changes) 

Formatting is defined in `.editorconfig` (supported by Rider/ReSharper).

No formatter? Open a PR — CI will validate style and reformat the code if necessary.

Suggestions for analyzers/style rules are welcome.
