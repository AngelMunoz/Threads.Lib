namespace Threads.Lib.API

open System.Threading
open System.Threading.Tasks

open Threads.Lib

type ReplyManagementService =
  abstract FetchRateLimits:
    userId: string *
    ?fields: ReplyManagement.RateLimitField seq *
    ?cancellationToken: CancellationToken ->
      Task<ReplyManagement.RateLimitResponse>

  abstract FetchReplies:
    mediaId: string *
    ?fields: ReplyManagement.ReplyField seq *
    ?reverse: bool *
    ?cancellationToken: CancellationToken ->
      Task<ReplyManagement.ConversationResponse>

  abstract FetchConversation:
    mediaId: string *
    ?fields: ReplyManagement.ReplyField seq *
    ?reverse: bool *
    ?cancellationToken: CancellationToken ->
      Task<ReplyManagement.ConversationResponse>

  abstract FetchUserReplies:
    userId: string *
    ?fields: ReplyManagement.ReplyField seq *
    ?cancellationToken: CancellationToken ->
      Task<ReplyManagement.ConversationResponse>

  abstract ManageReply:
    replyId: string * shouldHide: bool * ?cancellationToken: CancellationToken ->
      Task<bool>

type PostService =
  abstract PostContainer:
    profileId: string *
    postParams: Posts.PostParam seq *
    ?cancellationToken: CancellationToken ->
      Task<Posts.PostId>

  abstract PostCarouselItemContainer:
    profileId: string *
    postParams: Posts.PostParam seq *
    ?cancellationToken: CancellationToken ->
      Task<Posts.PostId>

  abstract PostCarousel:
    profileId: string *
    children: Posts.PostId seq *
    ?textContent: string *
    ?cancellationToken: CancellationToken ->
      Task<Posts.PostId>

  abstract PublishPost:
    profileId: string *
    containerId: Posts.PostId *
    ?cancellationToken: CancellationToken ->
      Task<Posts.PostId>

type MediaService =
  abstract FetchThreads:
    profileId: string *
    ?fields: Media.ThreadField seq *
    ?pagination: PaginationKind *
    ?cancellationToken: CancellationToken ->
      Task<Media.ThreadListResponse>

  abstract FetchThread:
    threadId: string *
    ?fields: Media.ThreadField seq *
    ?cancellationToken: CancellationToken ->
      Task<Media.ThreadValue seq>

type ProfileService =
  abstract FetchProfile:
    profileId: string *
    ?profileFields: Profiles.ProfileField seq *
    ?cancelaltionToken: CancellationToken ->
      Task<Profiles.ProfileValue seq>


type ThreadsClient =
  abstract Media: MediaService
  abstract Posts: PostService
  abstract Profile: ProfileService
  abstract Replies: ReplyManagementService

type ThreadClient =

  static member Create(accessToken: string) : ThreadsClient =
    let baseUrl = "https://graph.threads.net/v1.0/"

    let fetchProfile = Profiles.getProfile baseUrl accessToken

    let fetchThreads = Media.getThreads baseUrl accessToken
    let fetchThread = Media.getThread baseUrl accessToken

    let postSingle = Posts.createSingleContainer baseUrl accessToken
    let postCarouselItem = Posts.createCarouselItemContainer baseUrl accessToken
    let postCarousel = Posts.createCarouselContainer baseUrl accessToken
    let publishPost = Posts.publishContainer baseUrl accessToken

    let fetchRateLimits = ReplyManagement.getRateLimits baseUrl accessToken
    let fetchReplies = ReplyManagement.getReplies baseUrl accessToken
    let fetchConvos = ReplyManagement.getConversations baseUrl accessToken
    let allUserReplies = ReplyManagement.getUserReplies baseUrl accessToken
    let manageReply = ReplyManagement.manageReply baseUrl accessToken

    let profile =
      { new ProfileService with
          member _.FetchProfile(profileId, ?profileFields, ?cancellationToken) =

            let work = async {
              let profileFields = defaultArg profileFields []

              match! fetchProfile (Some profileId) profileFields with
              | Ok result -> return result
              | Error err -> return raise(exn err)
            }

            Async.StartImmediateAsTask(
              work,
              ?cancellationToken = cancellationToken
            )
      }

    let media =
      { new MediaService with

          member _.FetchThread(threadId, ?fields, ?cancellationToken) =

            let work = async {
              let profileFields = defaultArg fields []

              match! fetchThread threadId profileFields with
              | Ok result -> return result
              | Error err -> return raise(exn err)
            }

            Async.StartImmediateAsTask(
              work,
              ?cancellationToken = cancellationToken
            )

          member _.FetchThreads
            (profileId, ?fields, ?pagination, ?cancellationToken)
            =

            let work = async {
              let profileFields = defaultArg fields []

              match! fetchThreads profileId pagination profileFields with
              | Ok result -> return result
              | Error err -> return raise(exn err)
            }

            Async.StartImmediateAsTask(
              work,
              ?cancellationToken = cancellationToken
            )
      }

    let posts =
      { new PostService with
          member _.PostCarousel
            (profileId, children, ?textContent, ?cancellationToken)
            =

            let work = async {

              match! postCarousel profileId children textContent with
              | Ok value -> return value
              | Error err ->
                match err with
                | Posts.CarouselContainerError.CarouselPostIsEmpty ->
                  return
                    nameof(Posts.CarouselContainerError.CarouselPostIsEmpty)
                    |> exn
                    |> raise
                | Posts.CarouselContainerError.MoreThan10Children ->
                  return
                    nameof(Posts.CarouselContainerError.CarouselPostIsEmpty)
                    |> exn
                    |> raise
            }

            Async.StartImmediateAsTask(
              work,
              ?cancellationToken = cancellationToken
            )

          member _.PostContainer(profileId, postParams, ?cancellationToken) =

            let work = async {
              match! postSingle profileId postParams with
              | Ok value -> return value
              | Error err ->
                match err with
                | Posts.SingleContainerError.IsCarouselInSingleContainer ->
                  return
                    nameof(
                      Posts.SingleContainerError.IsCarouselInSingleContainer
                    )
                    |> exn
                    |> raise
                | Posts.SingleContainerError.IsImageButImageNotProvided ->
                  return
                    nameof(
                      Posts.SingleContainerError.IsImageButImageNotProvided
                    )
                    |> exn
                    |> raise
                | Posts.SingleContainerError.IsTextButNoTextProvided ->
                  return
                    nameof(Posts.SingleContainerError.IsTextButNoTextProvided)
                    |> exn
                    |> raise
                | Posts.SingleContainerError.IsVideoButNoVideoProvided ->
                  return
                    nameof(Posts.SingleContainerError.IsVideoButNoVideoProvided)
                    |> exn
                    |> raise
            }

            Async.StartImmediateAsTask(
              work,
              ?cancellationToken = cancellationToken
            )

          member _.PostCarouselItemContainer
            (profileId, postParams, ?cancellationToken)
            =

            let work = async {
              match! postCarouselItem profileId postParams with
              | Ok value -> return value
              | Error err ->
                match err with
                | Posts.CarouselItemContainerError.IsImageButImageNotProvided ->
                  return
                    nameof
                      Posts.CarouselItemContainerError.IsImageButImageNotProvided
                    |> exn
                    |> raise
                | Posts.CarouselItemContainerError.IsVideoButNoVideoProvided ->
                  return
                    nameof
                      Posts.CarouselItemContainerError.IsVideoButNoVideoProvided
                    |> exn
                    |> raise
                | Posts.CarouselItemContainerError.MediaTypeMustbeVideoOrImage ->
                  return
                    nameof
                      Posts.CarouselItemContainerError.MediaTypeMustbeVideoOrImage
                    |> exn
                    |> raise
            }

            Async.StartImmediateAsTask(
              work,
              ?cancellationToken = cancellationToken
            )

          member _.PublishPost
            (
              profileId: string,
              containerId: Posts.PostId,
              ?cancellationToken: CancellationToken
            ) : Task<Posts.PostId> =

            Async.StartImmediateAsTask(
              publishPost profileId containerId,
              ?cancellationToken = cancellationToken
            )
      }

    let replies =
      { new ReplyManagementService with
          member _.FetchConversation
            (mediaId, ?fields, ?reverse, ?cancellationToken)
            =
            let reverse = defaultArg reverse false
            let fields = defaultArg fields Seq.empty

            let work = async {
              match! fetchConvos mediaId fields reverse with
              | Ok result -> return result
              | Error err -> return err |> exn |> raise
            }

            Async.StartImmediateAsTask(
              work,
              ?cancellationToken = cancellationToken
            )


          member _.FetchRateLimits(userId, ?fields, ?cancellationToken) =
            let fields = defaultArg fields Seq.empty

            let work = async {
              match! fetchRateLimits userId fields with
              | Ok value -> return value
              | Error err -> return err |> exn |> raise
            }

            Async.StartImmediateAsTask(
              work,
              ?cancellationToken = cancellationToken
            )


          member _.FetchReplies
            (mediaId, ?fields, ?reverse, ?cancellationToken)
            =
            let fields = defaultArg fields Seq.empty
            let reverse = defaultArg reverse false

            let work = async {
              match! fetchReplies mediaId fields reverse with
              | Ok value -> return value
              | Error err -> return err |> exn |> raise
            }

            Async.StartImmediateAsTask(
              work,
              ?cancellationToken = cancellationToken
            )

          member _.FetchUserReplies(userId, ?fields, ?cancellationToken) =
            let fields = defaultArg fields Seq.empty

            let work = async {
              match! allUserReplies userId fields with
              | Ok value -> return value
              | Error err -> return err |> exn |> raise
            }

            Async.StartImmediateAsTask(
              work,
              ?cancellationToken = cancellationToken
            )

          member _.ManageReply(replyId, shouldHide, ?cancellationToken) =
            Async.StartImmediateAsTask(
              manageReply replyId shouldHide,
              ?cancellationToken = cancellationToken
            )
      }

    { new ThreadsClient with
        member _.Media = media
        member _.Posts = posts
        member _.Profile = profile
        member _.Replies = replies
    }
