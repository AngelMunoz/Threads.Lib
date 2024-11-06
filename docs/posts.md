---
title: Posts
category: Usage
categoryindex: 2
index: 2
description: Learn how to post new threads
keywords: usage, guide, posts
---

## Post a Thread

Posting a thread is a two step process, create the post and then publish it

```fsharp
#r "nuget: Threads.Lib"

open
open Threads.Lib
open Threads.Lib.Posts

let client = Threads.Create("acces_token")
task {

  let! containerId =
    // create a container with all of the requred parameters
    client.Posts.PostContainer(
      "me",
      [ MediaType Posts.Text; Text "This is the content of the new post!" ]
    )

  // publish the container's id to the threads web api.
  let! postId = client.Posts.PublishPost("me", containerId)

  // fetch the newly created post!
  let! results =
    client.Media.FetchThread(
      postId.id,
      [
        Media.ThreadField.Id
        Media.ThreadField.Username
        Media.ThreadField.Text
        Media.ThreadField.Timestamp
        Media.ThreadField.Permalink
      ]
    )

  // for single posts, the result is a list containing
  // the requested parameters
  for result in results do
    printfn $"%A{result}"
  (*
      Id "012345678901234567"
      Permalink https://www.threads.net/@user_handle/post/ShOrTcOdE
      Username "user_handle"
      Text "This is the content of the new post!"
      Timestamp 11/6/2024 3:30:46 AM +00:00
  *)
}
|> Async.AwaitTask
|> Async.RunSynchronously
```
