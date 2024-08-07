namespace Threads.Lib.API

open System.Threading
open System.Threading.Tasks

open FsToolkit.ErrorHandling

type ThreadsClient =
  // post endpoints
  abstract postContainer:
    postParams: Threads.Lib.Posts.PostParam seq *
    ?cancellationToken: CancellationToken ->
      TaskResult<
        Threads.Lib.Posts.PostId,
        Threads.Lib.Posts.SingleContainerError
       >

  abstract postCarousel:
    children: Threads.Lib.Posts.PostId seq *
    ?textContent: string *
    ?cancellationToken: CancellationToken ->
      TaskResult<
        Threads.Lib.Posts.PostId,
        Threads.Lib.Posts.CarouselContainerError
       >

  abstract publishPost:
    containerId: string * ?cancellationToken: CancellationToken ->
      Task<Threads.Lib.Posts.PostId>

  // media endpoints
  abstract fetchThreads:
    ?fields: Threads.Lib.Media.ThreadField seq *
    ?pagination: Threads.Lib.PaginationKind *
    ?cancellationToken: CancellationToken ->
      TaskResult<Threads.Lib.Media.ThreadListResponse, string>

  abstract fetchThread:
    threadId: string *
    ?fields: Threads.Lib.Media.ThreadField seq *
    ?cancellationToken: CancellationToken ->
      TaskResult<Threads.Lib.Media.ThreadValue seq, string>
  // profile endpoints
  abstract fetchProfile:
    profileId: string *
    ?profileFields: Threads.Lib.Profiles.ProfileField seq *
    ?cancelaltionToken: CancellationToken ->
      TaskResult<Threads.Lib.Profiles.ProfileValue seq, string>

  abstract fetchMe:
    ?profileFields: Threads.Lib.Profiles.ProfileField seq *
    ?cancelaltionToken: CancellationToken ->
      TaskResult<Threads.Lib.Profiles.ProfileValue seq, string>

type ThreadClient =
  static member create
    (
      accessToken: string,
      ?baseUrl: string,
      ?cancellationToken: CancellationToken
    ) : Task<ThreadsClient> =
    task {
      let baseUrl = defaultArg baseUrl "https://graph.threads.net/v1.0/"
      let token = defaultArg cancellationToken CancellationToken.None

      let! profileId =
        Threads.Lib.Profiles.getProfile baseUrl accessToken None [] token
        |> TaskResult.map(fun value ->
          match value |> Seq.head with
          | Threads.Lib.Profiles.ProfileValue.Id value -> value
          | _ -> failwith "no id present")
        |> TaskResult.defaultWith(fun result -> failwith result)

      let fetchProfile = Threads.Lib.Profiles.getProfile baseUrl accessToken
      let fetchThreads = Threads.Lib.Media.getThreads baseUrl accessToken
      let fetchThread = Threads.Lib.Media.getThread baseUrl accessToken

      let postCarousel =
        Threads.Lib.Posts.createCarouselContainer baseUrl accessToken profileId

      let postSingle =
        Threads.Lib.Posts.createSingleContainer baseUrl accessToken profileId

      let publishPost =
        Threads.Lib.Posts.publishContainer baseUrl accessToken profileId

      return
        { new ThreadsClient with
            member this.fetchMe
              (
                ?profileFields: Threads.Lib.Profiles.ProfileField seq,
                ?cancellationToken: CancellationToken
              ) : TaskResult<Threads.Lib.Profiles.ProfileValue seq, string> =
              let profileFields = defaultArg profileFields []
              let token = defaultArg cancellationToken CancellationToken.None
              fetchProfile None profileFields token

            member _.fetchProfile
              (
                profileId: string,
                ?profileFields: Threads.Lib.Profiles.ProfileField seq,
                ?cancellationToken: CancellationToken
              ) : TaskResult<Threads.Lib.Profiles.ProfileValue seq, string> =
              let profileFields = defaultArg profileFields []
              let token = defaultArg cancellationToken CancellationToken.None
              fetchProfile (Some profileId) profileFields token

            member _.fetchThread
              (
                threadId: string,
                ?fields: Threads.Lib.Media.ThreadField seq,
                ?cancellationToken: CancellationToken
              ) : TaskResult<Threads.Lib.Media.ThreadValue seq, string> =
              let profileFields = defaultArg fields []
              let token = defaultArg cancellationToken CancellationToken.None
              fetchThread threadId profileFields token

            member _.fetchThreads
              (
                ?fields: Threads.Lib.Media.ThreadField seq,
                ?pagination: Threads.Lib.PaginationKind,
                ?cancellationToken: CancellationToken
              ) : TaskResult<Threads.Lib.Media.ThreadListResponse, string> =
              let profileFields = defaultArg fields []
              let token = defaultArg cancellationToken CancellationToken.None
              fetchThreads pagination profileFields token

            member _.postCarousel
              (
                children: Threads.Lib.Posts.PostId seq,
                ?textContent: string,
                ?cancellationToken: CancellationToken
              ) : TaskResult<
                    Threads.Lib.Posts.PostId,
                    Threads.Lib.Posts.CarouselContainerError
                   >
              =
              let token = defaultArg cancellationToken CancellationToken.None
              postCarousel children textContent token

            member _.postContainer
              (
                postParams: Threads.Lib.Posts.PostParam seq,
                ?cancellationToken: CancellationToken
              ) : TaskResult<
                    Threads.Lib.Posts.PostId,
                    Threads.Lib.Posts.SingleContainerError
                   >
              =
              let token = defaultArg cancellationToken CancellationToken.None
              postSingle postParams token

            member _.publishPost
              (containerId: string, ?cancellationToken: CancellationToken)
              : Task<Threads.Lib.Posts.PostId> =
              let token = defaultArg cancellationToken CancellationToken.None
              publishPost { id = containerId } token
        }
    }
