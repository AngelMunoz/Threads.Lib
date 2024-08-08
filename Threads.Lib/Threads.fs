namespace Threads.Lib.API

open System.Threading
open System.Threading.Tasks


type PostService =
  abstract PostContainer:
    profileId: string *
    postParams: Threads.Lib.Posts.PostParam seq *
    ?cancellationToken: CancellationToken ->
      Task<Threads.Lib.Posts.PostId>

  abstract PostCarouselItemContainer:
    profileId: string *
    postParams: Threads.Lib.Posts.PostParam seq *
    ?cancellationToken: CancellationToken ->
      Task<Threads.Lib.Posts.PostId>

  abstract PostCarousel:
    profileId: string *
    children: Threads.Lib.Posts.PostId seq *
    ?textContent: string *
    ?cancellationToken: CancellationToken ->
      Task<Threads.Lib.Posts.PostId>

  abstract PublishPost:
    profileId: string *
    containerId: Threads.Lib.Posts.PostId *
    ?cancellationToken: CancellationToken ->
      Task<Threads.Lib.Posts.PostId>

type MediaService =
  abstract FetchThreads:
    profileId: string *
    ?fields: Threads.Lib.Media.ThreadField seq *
    ?pagination: Threads.Lib.PaginationKind *
    ?cancellationToken: CancellationToken ->
      Task<Threads.Lib.Media.ThreadListResponse>

  abstract FetchThread:
    threadId: string *
    ?fields: Threads.Lib.Media.ThreadField seq *
    ?cancellationToken: CancellationToken ->
      Task<Threads.Lib.Media.ThreadValue seq>

type ProfileService =
  abstract FetchProfile:
    profileId: string *
    ?profileFields: Threads.Lib.Profiles.ProfileField seq *
    ?cancelaltionToken: CancellationToken ->
      Task<Threads.Lib.Profiles.ProfileValue seq>


type ThreadsClient =
  inherit PostService
  inherit MediaService
  inherit ProfileService

type ThreadClient =

  static member Create(accessToken: string, ?baseUrl: string) : ThreadsClient =
    let baseUrl = defaultArg baseUrl "https://graph.threads.net/v1.0/"

    let fetchProfile = Threads.Lib.Profiles.getProfile baseUrl accessToken
    let fetchThreads = Threads.Lib.Media.getThreads baseUrl accessToken
    let fetchThread = Threads.Lib.Media.getThread baseUrl accessToken

    let postCarousel =
      Threads.Lib.Posts.createCarouselContainer baseUrl accessToken

    let postSingle = Threads.Lib.Posts.createSingleContainer baseUrl accessToken

    let postCarouselItem =
      Threads.Lib.Posts.createCarouselItemContainer baseUrl accessToken

    let publishPost = Threads.Lib.Posts.publishContainer baseUrl accessToken

    { new ThreadsClient with

        member _.FetchProfile(profileId, ?profileFields, ?cancellationToken) =
          let token = defaultArg cancellationToken CancellationToken.None

          let work = async {
            let profileFields = defaultArg profileFields []

            match! fetchProfile (Some profileId) profileFields with
            | Ok result -> return result
            | Error err -> return raise(exn err)
          }

          Async.StartAsTask(work, cancellationToken = token)

        member _.FetchThread(threadId, ?fields, ?cancellationToken) =
          let token = defaultArg cancellationToken CancellationToken.None

          let work = async {
            let profileFields = defaultArg fields []

            match! fetchThread threadId profileFields with
            | Ok result -> return result
            | Error err -> return raise(exn err)
          }

          Async.StartAsTask(work, cancellationToken = token)

        member _.FetchThreads
          (profileId, ?fields, ?pagination, ?cancellationToken)
          =
          let token = defaultArg cancellationToken CancellationToken.None

          let work = async {
            let profileFields = defaultArg fields []

            match! fetchThreads profileId pagination profileFields with
            | Ok result -> return result
            | Error err -> return raise(exn err)
          }

          Async.StartAsTask(work, cancellationToken = token)

        member _.PostCarousel
          (profileId, children, ?textContent, ?cancellationToken)
          =
          let token = defaultArg cancellationToken CancellationToken.None

          let work = async {

            match! postCarousel profileId children textContent with
            | Ok value -> return value
            | Error err ->
              match err with
              | Threads.Lib.Posts.CarouselContainerError.CarouselPostIsEmpty ->
                return
                  nameof(
                    Threads.Lib.Posts.CarouselContainerError.CarouselPostIsEmpty
                  )
                  |> exn
                  |> raise
              | Threads.Lib.Posts.CarouselContainerError.MoreThan10Children ->
                return
                  nameof(
                    Threads.Lib.Posts.CarouselContainerError.CarouselPostIsEmpty
                  )
                  |> exn
                  |> raise
          }

          Async.StartAsTask(work, cancellationToken = token)

        member _.PostContainer(profileId, postParams, ?cancellationToken) =
          let token = defaultArg cancellationToken CancellationToken.None

          let work = async {
            match! postSingle profileId postParams with
            | Ok value -> return value
            | Error err ->
              match err with
              | Threads.Lib.Posts.SingleContainerError.IsCarouselInSingleContainer ->
                return
                  nameof(
                    Threads.Lib.Posts.SingleContainerError.IsCarouselInSingleContainer
                  )
                  |> exn
                  |> raise
              | Threads.Lib.Posts.SingleContainerError.IsImageButImageNotProvided ->
                return
                  nameof(
                    Threads.Lib.Posts.SingleContainerError.IsImageButImageNotProvided
                  )
                  |> exn
                  |> raise
              | Threads.Lib.Posts.SingleContainerError.IsTextButNoTextProvided ->
                return
                  nameof(
                    Threads.Lib.Posts.SingleContainerError.IsTextButNoTextProvided
                  )
                  |> exn
                  |> raise
              | Threads.Lib.Posts.SingleContainerError.IsVideoButNoVideoProvided ->
                return
                  nameof(
                    Threads.Lib.Posts.SingleContainerError.IsVideoButNoVideoProvided
                  )
                  |> exn
                  |> raise
          }

          Async.StartAsTask(work, cancellationToken = token)

        member _.PostCarouselItemContainer
          (profileId, postParams, ?cancellationToken)
          =
          let token = defaultArg cancellationToken CancellationToken.None

          let work = async {
            match! postCarouselItem profileId postParams with
            | Ok value -> return value
            | Error err ->
              match err with
              | Threads.Lib.Posts.CarouselItemContainerError.IsImageButImageNotProvided ->
                return
                  nameof
                    Threads.Lib.Posts.CarouselItemContainerError.IsImageButImageNotProvided
                  |> exn
                  |> raise
              | Threads.Lib.Posts.CarouselItemContainerError.IsVideoButNoVideoProvided ->
                return
                  nameof
                    Threads.Lib.Posts.CarouselItemContainerError.IsVideoButNoVideoProvided
                  |> exn
                  |> raise
              | Threads.Lib.Posts.CarouselItemContainerError.MediaTypeMustbeVideoOrImage ->
                return
                  nameof
                    Threads.Lib.Posts.CarouselItemContainerError.MediaTypeMustbeVideoOrImage
                  |> exn
                  |> raise
          }

          Async.StartAsTask(work, cancellationToken = token)

        member _.PublishPost
          (
            profileId: string,
            containerId: Threads.Lib.Posts.PostId,
            ?cancellationToken: CancellationToken
          ) : Task<Threads.Lib.Posts.PostId> =
          let token = defaultArg cancellationToken CancellationToken.None

          Async.StartAsTask(
            publishPost profileId containerId,
            cancellationToken = token
          )
    }
