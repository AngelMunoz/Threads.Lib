namespace Sample

open System
open System.Threading.Tasks
open IcedTasks
open IcedTasks.Polyfill.Async
open IcedTasks.Polyfill.Task
open Threads.Lib


type UserProfile = {
  id: string
  username: string
  bio: string
  profilePicture: Uri option
}

module UserProfile =
  open Threads.Lib.Profiles

  let empty = {
    id = ""
    username = ""
    bio = ""
    profilePicture = None
  }

  let inline private foldProfile (current: UserProfile) (next: ProfileValue) =
    match next with
    | ProfileValue.Id id -> { current with id = id }
    | ProfileValue.Username username -> { current with username = username }
    | ProfileValue.ThreadsBiography bio -> { current with bio = bio }
    | ProfileValue.ThreadsProfilePictureUrl profilePicture ->
        {
          current with
              profilePicture = Some profilePicture
        }

  let toProfile(values: Task<Profiles.ProfileValue list>) = async {
    let! values = values
    return values |> Seq.fold foldProfile empty
  }

type Post = {
  id: string
  username: string
  text: string
  timestamp: DateTimeOffset
  mediaUrl: Uri option
  mediaType: Media.MediaType
  owner: IdLike option
  permalink: Uri
  children: IdLike list
  isQuotePost: bool
}

module Post =
  open Threads.Lib.Media

  let private empty = {
    id = ""
    username = ""
    text = ""
    timestamp = DateTimeOffset.MinValue
    mediaUrl = None
    mediaType = TextPost
    owner = None
    permalink = Unchecked.defaultof<Uri>
    children = List.empty
    isQuotePost = false
  }


  let inline private foldPost (current: Post) (next: ThreadValue) =
    match next with
    | ThreadValue.Id id -> { current with id = id }
    | ThreadValue.Username username -> { current with username = username }
    | ThreadValue.Text text -> { current with text = text }
    | ThreadValue.Timestamp timestamp -> { current with timestamp = timestamp }
    | ThreadValue.MediaUrl mediaUrl -> {
        current with
            mediaUrl = Some mediaUrl
      }
    | ThreadValue.MediaType mediaType -> { current with mediaType = mediaType }
    | ThreadValue.Owner owner -> { current with owner = Some owner }
    | ThreadValue.Permalink permalink -> { current with permalink = permalink }
    | ThreadValue.Children children -> {
        current with
            children = children.data
      }
    | ThreadValue.IsQuotePost isQuotePost -> {
        current with
            isQuotePost = isQuotePost
      }
    // we don't care about extra props for our case
    | _ -> current

  let ofPosts(values: Task<ThreadListResponse>) = async {
    let! values = values

    let mapped = values.data |> List.map(List.fold foldPost empty)

    return mapped, values.paging
  }

  let ofPost(values: Task<Media.ThreadValue list>) = async {
    let! values = values

    let mapped = values |> List.fold foldPost empty

    return mapped
  }


type PostParameters = {
  text: string
  mediaUrl: Uri option
  mediaType: Posts.MediaType
  audience: Posts.ReplyAudience
}
