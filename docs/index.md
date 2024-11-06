# Threads.Lib

This is a .NET library for the [Threads] API.

## Usage

The library itself is a thin wrapper over the API, so you don't have to craft everything yourself. However we don't provide any authentication dances, so you have to [obtain the access token] required to interact with the API yourself.

Once you have your access token ready to go you can obtain a client instance like this:

```fsharp
open Threads.Lib
open Threads.Lib.Profile

let threads = Threads.Create("access-token")

async {
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
```
[Threads]: https://developers.facebook.com/docs/threads
[obtain the access token]: https://developers.facebook.com/docs/threads/get-started/get-access-tokens-and-permissions
