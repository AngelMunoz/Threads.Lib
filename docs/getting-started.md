---
title: Getting Started
category: Usage
categoryindex: 2
index: 1
description: Learn how to use Threads.Lib
keywords: usage, getting started, guide
---

# Getting Started

In order to use Threads.Lib, you will need to obtain an access token from the Threads API.
The Threads API use OAuth2 for authentication, so you will need to follow their documentation on this area.

> For more information please visit https://developers.facebook.com/docs/threads/get-started/get-access-tokens-and-permissions
>
> For development purposes, you can generate an access token from the apps dashboard in the facebook developers website.
> https://developers.facebook.com/apps/YOUR_APP_ID/use_cases/customize/?use_case_enum=THREADS_API&selected_tab=settings&product_route=threads-api

Once you have obtained an access token, you can start using Threads.Lib.

Please note that all the examples through this documentation will use F# scripts so you can run this code locally yourself, keep in mind that the version referenced in the scripts may be outdated in the documentation.

Please make sure of the latest version in the [Nuget Package](https://www.nuget.org/packages/Threads.Lib/).

## Usage

Threads.Lib is a dotnet library which is currently not compatible with the Fable Compiler so it can only be used in dotnet projects at the moment.

```fsharp
#r "nuget: Threads.Lib"

open System
open Threads.Lib
open Threads.Lib.Profile

let accessToken = "YOUR_ACCESS_TOKEN"
let client = Threads.Create(accessToken)

task {
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
|> Async.AwaitTask
// Note that this is just for the F# scripts environment. Projects usually don't have to call this.
|> Async.RunSynchronously
```

> **_NOTE_**: The profileId "me" is a special keyword that represents the current user's profile. In order to
> fetch another user's profile, you will need to replace "me" with the user's id.

And just like that you now have the token owner's profile information.

In general the Library contains 5 major services that correspond to the majority of the Threads Web API.

- MediaService - `client.Media.*` - https://developers.facebook.com/docs/threads/threads-media

  This service is used to pull down what is knon as "media" in the API language, however for users "post" would be a more familiar term.

- ProfileService - `client.Profile.*` - https://developers.facebook.com/docs/threads/threads-profiles

  This service is used to pull down profile information for a user.

- PostService - `client.Post.*` - https://developers.facebook.com/docs/threads/posts

  This service is used to generate "containers" which are objects that envelop the contents of a "post" or "media" object for futher publication to the threads web api.

- ReplyManagementService - `client.ReplyManagement.*` - https://developers.facebook.com/docs/threads/reply-management

  This service is used to manage replies and conversations (top level and nested replies connected to a post)

- InsightsService - `client.Insights.*` - https://developers.facebook.com/docs/threads/insights

  This service is used to pull down insights for a user or a post, it contains metrics like the number of views, likes, comments, etc.
