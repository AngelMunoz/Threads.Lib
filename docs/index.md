# Threads.Lib

This is a .NET library for the [Threads] API.

## Installation

WIP

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
}
```

The Threads API is quite dynamic and you can choose which fields you want to obtain in the response, this is at odds with fsharp's type system which requires types to be statically known at compile time. To work around this we provide a `ProfileValue` type which is a discriminated union of all possible fields you can obtain from the API. This way you can pattern match on the response and extract the fields you need.

An example would be:

```fsharp
// Define a type to represent the user profile
// with all of the fields you're interested in
type UserProfile = {
    id: string
    username: string
    bio: string
  }

  module UserProfile =
    let emptyProfile = {
      id = ""
      username = ""
      bio = ""
    }

    // In a module or static method, you can define a mapping function to convert
    // the ProfileValue seq to a your type instance
    let ofValues (values: ProfileValue seq) : UserProfile =

      let foldToProfile (current: UserProfile) (nextValue: ProfileValue) =
        match nextValue with
        | Id id -> { current with id = id }
        | Username username -> { current with username = username }
        | ThreadsBiography bio -> { current with bio = bio }
        | ThreadsProfilePictureUrl profilePicture -> current

      Seq.fold foldToProfile emptyProfile values

// somewhere else in your code
async {
  let! (response: ProfileValue seq) =
    threads.Profile.FetchProfile(
      profileId = "me",
      profileFields = [
        ProfileField.Id
        ProfileField.Username
        ProfileField.ThreadsBiography
      ]
    )

  let profile = UserProfile.ofValues response
  printfn $"%A{profile}"
  // { id = "1234"; username = "johndoe"; bio = "I'm a bio" }
}
```

While there are too few fields in the profile response. This will come more handy in other responses like the thread posts themselves. Overall the library is meant to be used like this and tries to be as flexible as possible without compromising the type safety of F# or at least that's what we're trying.

[Threads]: https://developers.facebook.com/docs/threads
[obtain the access token]: https://developers.facebook.com/docs/threads/get-started/get-access-tokens-and-permissions
