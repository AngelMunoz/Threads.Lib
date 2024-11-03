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

## A word on types and flexibility

Usually most of the API responses would be a record for strong types. However, the Threads API allows you to specify which fields you want in your responses. we could have crafted the API calls to return a record with all the fields but given how they can be optiona, most of the fields would be of the option type making it a bit tedious to work with as you would need to unwrap the option type every time you want to access a single or multiple fields.

Our recomendation is to define a type that represents the response you're interested in and then define a mapping function to convert the response to your type.

An example would be:

```fsharp
// Define a type to represent the user profile
// with all of the fields you're interested in
type UserBio = {
    id: string
    bio: string
  }

  module UserBio =
    let defaultBioAndId = {
      id = ""
      bio = ""
    }

    // In a module or static method, you can define a mapping function to convert
    // the ProfileValue seq to a your type instance
    let ofValues (values: ProfileValue seq) : UserBio =

      let foldToProfile (current: UserBio) (nextValue: ProfileValue) =
        match nextValue with
        // skip the fields we're not interested in
        | Username _
        | ThreadsProfilePictureUrl _-> current
        // assign the fields we're interested in
        | Id id -> { current with id = id }
        | ThreadsBiography bio -> { current with bio = bio }

      Seq.fold foldToProfile defaultBioAndId values

// somewhere else in your code
async {
  let! (response: ProfileValue seq) =
    threads.Profile.FetchProfile(
      profileId = "me",
      profileFields = [
        ProfileField.Id
        ProfileField.ThreadsBiography
      ]
    )

  let profile = UserProfile.ofValues response
  printfn $"%A{profile}"
  // { id = "1234"; bio = "I'm a bio" }
}
```

That being said, you could add type extensions or extension methods to the `ProfileValue` type to make it easier to convert to your type.

[Threads]: https://developers.facebook.com/docs/threads
[obtain the access token]: https://developers.facebook.com/docs/threads/get-started/get-access-tokens-and-permissions
