# Threads.Lib

This is a .NET library for the [Threads] API.

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
