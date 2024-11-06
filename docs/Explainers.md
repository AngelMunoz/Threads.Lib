---
title: FAQ
category: Explainers
categoryindex: 3
index: 1
description: Learn how to use Threads.Lib
keywords: faq
---



## Why a list of values rather than records?

Meta's document graphs can be very big and bring data that is irrelevant to your purposes, so they decided to parameterize which fields you can request, while flexible and powerful for dynamic languages like javascript or python, it creates friction for strongly typed languages where types usually need to be known at compile time.


The most flexible option without having to make every record's field optional is to return a list of values which are tied to a strong type. this way you can have the benefits of the F# type system while stil allowing the flexibility of the API.
An example would be:

```fsharp
// Define a type to represent the user profile
// with all of the fields you're interested in
type UserBio = {
    id: string
    bio: string
  }

  module UserBio =
    let empty = {
      id = ""
      bio = ""
    }

    // In a module or static method, you can define a mapping function to convert
    // the ProfileValue seq to a your type instance
    let ofValues (values: ProfileValue seq) : UserBio =

      let foldToProfile (current: UserBio) (nextValue: ProfileValue) =
        match nextValue with
        // skip the fields we're not interested in because we didn't request them
        | Username _
        | ThreadsProfilePictureUrl _-> current
        // assign the fields we're interested in and were requested
        | Id id -> { current with id = id }
        | ThreadsBiography bio -> { current with bio = bio }

      Seq.fold foldToProfile empty values

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

This leaves the mapping to strong record types up to the user, but it's a small price to pay for the flexibility of the API.



## Why Tasks instead of Async, and F# lists instead of IEnumerable\`T?

While this library is likely to be only consumed from F#, I believe in the idea of bulding libraries with interoperability in mind.
Tasks are a more common type in the dotnet ecosystem and are more likely to be used in other languages like C#, unfortunately F# async can't be awaited from C# code, so in this case Async would be a bit hostile outside F#.

while Seq\`T (IEnumerable\`T) is also a more commn type in the dotnet ecosystem however, F# lists are Linq compatible and don't have hard edges against other dotnet languages, so dotnet users will be able to perform the usual operations they would do on "list" and "IEnumerable" types.

There might be missing a few extension functions to work with some Discriminated unions that are exposed in some of the modules but if that becomes an issue, feel free to report it and we'll figure out how to make it ergonomic if possible.
