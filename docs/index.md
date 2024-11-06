# Threads.Lib

This is a dotnet library for the [Threads] Web API.

> This libary currently does not support fable. Feel free to chime in if this is in your interest!

Use this library in servers, F# scripts, CLI applications and even crossplatform GUI apps [check out the sample]!

## Micro example

This example shows the gist of how to use this library:

```fsharp
#r "nuget: Threads.Lib"

open Threads.Lib

let threads = Threads.Create("access token")
async {
  let! response = threads.Profile.FetchProfile("me", [ProfileField.Username])
  printfn $"%A{response[0]}"
  // ProfileValue.Username "example_username_handle"
}
|> Async.RunSynchronously
```

For more information check out the [Getting Started] guide.

[Threads]: https://developers.facebook.com/docs/threads
[Getting Started]: getting-started.md
[Check out the sample]: https://github.com/AngelMunoz/Threads.Lib/tree/main/Sample
