# F# Library for the Threads API

For the Threads API docs check out https://developers.facebook.com/docs/threads where this is based from.

```fsharp
// script.fsx
#r "nuget: Threads.Lib"
open Threads.Lib
open Threads.Lib.Profile

async {
  let threads = Threads.Create("access-token")
  let! (response: ProfileValue seq) =
    threads.Profile.FetchProfile(
      profileId = "me",
      profileFields = [
        ProfileField.Id
        ProfileField.Username
        ProfileField.ThreadsBiography
        ProfileField.ThreadsProfilePictureUrl
      ]
    )

    printfn $"%A{response}"
    (*
      [ ProfileValue.Id "1234567890"
        ProfileValue.Username "johndoe"
        ProfileValue.ThreadsBiography "I'm a cool guy"
        ProfileValue.ThreadsProfilePictureUrl (Uri "https://example.com/picture.jpg")
      ]
    *)
}
|> Async.RunSynchronously
```

- `dotnet run script.fsx` should print the profile response.

For more information, check out the docs at https://angelmunoz.github.io/Threads.Lib/

# Build locally

`dotnet fsi build.fsx -- -p build
