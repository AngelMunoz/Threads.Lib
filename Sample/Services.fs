namespace Sample

open System
open System.Threading.Tasks
open IcedTasks
open IcedTasks.Polyfill.Async
open IcedTasks.Polyfill.Task
open Threads.Lib

open FSharp.Data.Adaptive

open Navs.Avalonia


type ProfileStore = {
  profile: aval<UserProfile>
  pagination: aval<Pagination option>
  setProfile: UserProfile -> unit
}

module ProfileStore =
  let create() =
    let profile = cval UserProfile.empty
    let pagination = cval None

    {
      profile = profile
      setProfile = profile.setValue
      pagination = pagination
    }

type UserThreads = {
  threads: aval<Post list>
  setThreads: Post list -> unit
  prependThread: Post -> unit
}

module UserThreads =
  let create() =
    let threads = cval []

    {
      threads = threads
      setThreads = threads.setValue
      prependThread =
        fun post -> AVal.mapSet threads (fun posts -> post :: posts)
    }


module PostService =

  let defaultFetchMediaParams =
    lazy
      ([
        Media.ThreadField.Id
        Media.ThreadField.Username
        Media.ThreadField.Text
        Media.ThreadField.Timestamp
        Media.ThreadField.MediaUrl
        Media.ThreadField.MediaType
        Media.ThreadField.Owner
        Media.ThreadField.Permalink
      ])

  let postThread (threads: ThreadsClient) (postParams: PostParameters) = async {
    let! token = Async.CancellationToken

    let! container =
      threads.Posts.PostContainer(
        "me",
        [
          Posts.PostParam.MediaType postParams.mediaType
          match postParams.mediaType with
          | Posts.Image -> Posts.PostParam.ImageUrl postParams.mediaUrl.Value
          | Posts.Video -> Posts.PostParam.VideoUrl postParams.mediaUrl.Value
          | _ -> ()
          Posts.PostParam.Text postParams.text
          Posts.PostParam.ReplyControl postParams.audience

        ],
        token
      )

    let! postId = threads.Posts.PublishPost("me", container, token)

    return!
      threads.Media.FetchThread(postId.id, defaultFetchMediaParams.Value, token)
      |> Post.ofPost
  }

  let loadUserThreads (threads: ThreadsClient) () = async {
    let! token = Async.CancellationToken

    return!
      threads.Media.FetchThreads(
        "me",
        defaultFetchMediaParams.Value,
        cancellationToken = token
      )
      |> Post.ofPosts
  }

module ProfileService =

  let defaultFetchProfileParams =
    lazy
      ([
        Profiles.ProfileField.Id
        Profiles.ProfileField.Username
        Profiles.ProfileField.ThreadsBiography
        Profiles.ProfileField.ThreadsProfilePictureUrl
      ])

  let loadProfile (threads: ThreadsClient) () = async {
    let! token = Async.CancellationToken

    let! response =
      threads.Profile.FetchProfile("me", defaultFetchProfileParams.Value, token)
      |> UserProfile.toProfile

    return response
  }
