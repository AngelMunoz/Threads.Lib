namespace Threads.Lib

open System.Threading
open System.Threading.Tasks
open System.Runtime.InteropServices


type InsightsService =
  abstract FetchMediaInsights:
    mediaId: string *
    metrics: Insights.Metric seq *
    [<Optional>] ?cancellationToken: CancellationToken ->
      Task<Insights.MetricResponse>

  abstract FetchUserInsights:
    userId: string *
    metrics: Insights.Metric seq *
    insightParams: Insights.InsightParam seq *
    [<Optional>] ?cancellationToken: CancellationToken ->
      Task<Insights.MetricResponse>

type ReplyManagementService =
  abstract FetchRateLimits:
    userId: string *
    [<Optional>] ?fields: ReplyManagement.RateLimitField seq *
    [<Optional>] ?cancellationToken: CancellationToken ->
      Task<ReplyManagement.RateLimitResponse>

  abstract FetchReplies:
    mediaId: string *
    [<Optional>] ?fields: ReplyManagement.ReplyField seq *
    [<Optional>] ?pagination: PaginationKind *
    [<Optional>] ?reverse: bool *
    [<Optional>] ?cancellationToken: CancellationToken ->
      Task<ReplyManagement.ConversationResponse>

  abstract FetchConversation:
    mediaId: string *
    [<Optional>] ?fields: ReplyManagement.ReplyField seq *
    [<Optional>] ?pagination: PaginationKind *
    [<Optional>] ?reverse: bool *
    [<Optional>] ?cancellationToken: CancellationToken ->
      Task<ReplyManagement.ConversationResponse>

  abstract FetchUserReplies:
    userId: string *
    [<Optional>] ?fields: ReplyManagement.ReplyField seq *
    [<Optional>] ?pagination: PaginationKind *
    [<Optional>] ?cancellationToken: CancellationToken ->
      Task<ReplyManagement.ConversationResponse>

  abstract ManageReply:
    replyId: string *
    shouldHide: bool *
    [<Optional>] ?cancellationToken: CancellationToken ->
      Task<bool>

type PostService =
  abstract PostContainer:
    profileId: string *
    postParams: Posts.PostParam seq *
    [<Optional>] ?cancellationToken: CancellationToken ->
      Task<IdLike>

  abstract PostCarouselItemContainer:
    profileId: string *
    postParams: Posts.PostParam seq *
    [<Optional>] ?cancellationToken: CancellationToken ->
      Task<IdLike>

  abstract PostCarousel:
    profileId: string *
    children: IdLike seq *
    [<Optional>] ?textContent: string *
    [<Optional>] ?cancellationToken: CancellationToken ->
      Task<IdLike>

  abstract PublishPost:
    profileId: string *
    containerId: IdLike *
    [<Optional>] ?cancellationToken: CancellationToken ->
      Task<IdLike>

  abstract Repost:
    mediaId: string * [<Optional>] ?cancellationToken: CancellationToken ->
      Task<IdLike>

type MediaService =
  abstract FetchThreads:
    profileId: string *
    [<Optional>] ?fields: Media.ThreadField seq *
    [<Optional>] ?pagination: PaginationKind *
    [<Optional>] ?cancellationToken: CancellationToken ->
      Task<Media.ThreadListResponse>

  abstract FetchThread:
    threadId: string *
    [<Optional>] ?fields: Media.ThreadField seq *
    [<Optional>] ?cancellationToken: CancellationToken ->
      Task<Media.ThreadValue list>

type ProfileService =
  abstract FetchProfile:
    profileId: string *
    [<Optional>] ?profileFields: Profiles.ProfileField seq *
    [<Optional>] ?cancellationToken: CancellationToken ->
      Task<Profiles.ProfileValue list>

[<Interface>]
type ThreadsClient =
  abstract Media: MediaService
  abstract Posts: PostService
  abstract Profile: ProfileService
  abstract Replies: ReplyManagementService
  abstract Insights: InsightsService


module Impl =
  let getProfileService fetchProfile =
    { new ProfileService with
        member _.FetchProfile
          (
            profileId,
            [<Optional>] ?profileFields,
            [<Optional>] ?cancellationToken
          ) =

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

  let getMediaService fetchThread fetchThreads =
    { new MediaService with

        member _.FetchThread
          (threadId, [<Optional>] ?fields, [<Optional>] ?cancellationToken)
          =

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
          (
            profileId,
            [<Optional>] ?fields,
            [<Optional>] ?pagination,
            [<Optional>] ?cancellationToken
          ) =

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

  let getPostService
    createCarousel
    createSingle
    createCarouselItem
    publishPost
    repost
    =
    { new PostService with
        member _.PostCarousel
          (
            profileId,
            children,
            [<Optional>] ?textContent,
            [<Optional>] ?cancellationToken
          ) =

          let work = async {

            match! createCarousel profileId children textContent with
            | Ok value -> return value
            | Error err ->
              return Posts.CarouselContainerArgumentException err |> raise
          }

          Async.StartImmediateAsTask(
            work,
            ?cancellationToken = cancellationToken
          )

        member _.PostContainer
          (profileId, postParams, [<Optional>] ?cancellationToken)
          =

          let work = async {
            match! createSingle profileId postParams with
            | Ok value -> return value
            | Error err ->
              return Posts.SingleContainerArgumentException err |> raise
          }

          Async.StartImmediateAsTask(
            work,
            ?cancellationToken = cancellationToken
          )

        member _.PostCarouselItemContainer
          (profileId, postParams, [<Optional>] ?cancellationToken)
          =

          let work = async {
            match! createCarouselItem profileId postParams with
            | Ok value -> return value
            | Error err ->
              return Posts.CarouselItemContainerArgumentException err |> raise
          }

          Async.StartImmediateAsTask(
            work,
            ?cancellationToken = cancellationToken
          )

        member _.PublishPost
          (
            profileId: string,
            containerId: IdLike,
            [<Optional>] ?cancellationToken: CancellationToken
          ) : Task<IdLike> =

          Async.StartImmediateAsTask(
            publishPost profileId containerId,
            ?cancellationToken = cancellationToken
          )

        member _.Repost(mediaId, [<Optional>] ?cancellationToken) =
          Async.StartImmediateAsTask(
            repost mediaId,
            ?cancellationToken = cancellationToken
          )
    }

  let getReplyManagement
    manageReply
    fetchRateLimits
    fetchConvos
    fetchReplies
    allUserReplies
    =
    { new ReplyManagementService with
        member _.FetchConversation
          (
            mediaId,
            [<Optional>] ?fields,
            [<Optional>] ?pagination,
            [<Optional>] ?reverse,
            [<Optional>] ?cancellationToken
          ) =
          let reverse = defaultArg reverse false
          let fields = defaultArg fields Seq.empty

          let work = async {
            match! fetchConvos mediaId fields pagination reverse with
            | Ok result -> return result
            | Error err -> return err |> exn |> raise
          }

          Async.StartImmediateAsTask(
            work,
            ?cancellationToken = cancellationToken
          )


        member _.FetchRateLimits
          (userId, [<Optional>] ?fields, [<Optional>] ?cancellationToken)
          =
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
          (
            mediaId,
            [<Optional>] ?fields,
            [<Optional>] ?pagination,
            [<Optional>] ?reverse,
            [<Optional>] ?cancellationToken
          ) =
          let fields = defaultArg fields Seq.empty
          let reverse = defaultArg reverse false

          let work = async {
            match! fetchReplies mediaId fields pagination reverse with
            | Ok value -> return value
            | Error err -> return err |> exn |> raise
          }

          Async.StartImmediateAsTask(
            work,
            ?cancellationToken = cancellationToken
          )

        member _.FetchUserReplies
          (
            userId,
            [<Optional>] ?fields,
            [<Optional>] ?pagination,
            [<Optional>] ?cancellationToken
          ) =
          let fields = defaultArg fields Seq.empty

          let work = async {
            match! allUserReplies userId fields pagination with
            | Ok value -> return value
            | Error err -> return err |> exn |> raise
          }

          Async.StartImmediateAsTask(
            work,
            ?cancellationToken = cancellationToken
          )

        member _.ManageReply
          (replyId, shouldHide, [<Optional>] ?cancellationToken)
          =
          Async.StartImmediateAsTask(
            manageReply replyId shouldHide,
            ?cancellationToken = cancellationToken
          )
    }

  let getInsights fetchUserInsights fetchMediaInsights =
    { new InsightsService with
        member _.FetchMediaInsights
          (mediaId, metrics, [<Optional>] ?cancellationToken)
          =
          let metrics = metrics |> Seq.toArray
          let token = defaultArg cancellationToken CancellationToken.None

          let work = async {
            match! fetchMediaInsights mediaId metrics with
            | Ok value -> return value
            | Error err -> return err |> exn |> raise
          }

          Async.StartImmediateAsTask(work, cancellationToken = token)

        member _.FetchUserInsights
          (userId, metrics, insightParams, [<Optional>] ?cancellationToken)
          =
          let metrics = metrics |> Seq.toArray
          let insightParams = insightParams |> Seq.toArray
          let token = defaultArg cancellationToken CancellationToken.None

          let work = async {
            match! fetchUserInsights userId metrics insightParams with
            | Ok value -> return value
            | Error Insights.InsightError.DateTooEarly ->
              return nameof(Insights.InsightError.DateTooEarly) |> exn |> raise
            | Error(Insights.InsightError.SerializationError err) ->
              return err |> exn |> raise
            | Error Insights.InsightError.FollowerDemographicsMustIncludeBreakdown ->
              return
                nameof(
                  Insights.InsightError.FollowerDemographicsMustIncludeBreakdown
                )
                |> exn
                |> raise

          }

          Async.StartImmediateAsTask(work, cancellationToken = token)
    }


[<Class>]
type Threads =
  static member Create(accessToken, [<Optional>] ?baseUrl: string) =

    let baseUrl = defaultArg baseUrl "https://graph.threads.net/v1.0"

    let fetchProfile = Profiles.getProfile baseUrl accessToken

    let fetchThreads = Media.getThreads baseUrl accessToken
    let fetchThread = Media.getThread baseUrl accessToken

    let postSingle = Posts.createSingleContainer baseUrl accessToken

    let postCarouselItem = Posts.createCarouselItemContainer baseUrl accessToken

    let postCarousel = Posts.createCarouselContainer baseUrl accessToken
    let publishPost = Posts.publishContainer baseUrl accessToken
    let repost = Posts.repost baseUrl accessToken

    let fetchRateLimits = ReplyManagement.getRateLimits baseUrl accessToken
    let fetchReplies = ReplyManagement.getReplies baseUrl accessToken
    let fetchConvos = ReplyManagement.getConversation baseUrl accessToken
    let allUserReplies = ReplyManagement.getUserReplies baseUrl accessToken
    let manageReply = ReplyManagement.manageReply baseUrl accessToken

    let fetchUserInsights = Insights.getUserInsights baseUrl accessToken
    let fetchMediaInsights = Insights.getMediaInsights baseUrl accessToken

    { new ThreadsClient with
        member _.Media = Impl.getMediaService fetchThread fetchThreads

        member _.Posts =
          Impl.getPostService
            postCarousel
            postSingle
            postCarouselItem
            publishPost
            repost

        member _.Profile = Impl.getProfileService fetchProfile

        member _.Replies =
          Impl.getReplyManagement
            manageReply
            fetchRateLimits
            fetchConvos
            fetchReplies
            allUserReplies

        member _.Insights =
          Impl.getInsights fetchUserInsights fetchMediaInsights
    }
