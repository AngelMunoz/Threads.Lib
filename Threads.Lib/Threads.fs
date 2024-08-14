namespace Threads.Lib

open System.Threading
open System.Threading.Tasks
open FsHttp
open System.Runtime.InteropServices

open Threads.Lib

type InsightsService =
  abstract FetchMediaInsights:
    mediaId: string *
    metrics: Insights.Metric seq *
    [<OptionalAttribute>] ?cancellationToken: CancellationToken ->
      Task<Insights.MetricResponse>

  abstract FetchUserInsights:
    userId: string *
    metrics: Insights.Metric seq *
    insightParams: Insights.InsightParam seq *
    [<OptionalAttribute>] ?cancellationToken: CancellationToken ->
      Task<Insights.MetricResponse>

type ReplyManagementService =
  abstract FetchRateLimits:
    userId: string *
    [<OptionalAttribute>] ?fields: ReplyManagement.RateLimitField seq *
    [<OptionalAttribute>] ?cancellationToken: CancellationToken ->
      Task<ReplyManagement.RateLimitResponse>

  abstract FetchReplies:
    mediaId: string *
    [<OptionalAttribute>] ?fields: ReplyManagement.ReplyField seq *
    [<OptionalAttribute>] ?reverse: bool *
    [<OptionalAttribute>] ?cancellationToken: CancellationToken ->
      Task<ReplyManagement.ConversationResponse>

  abstract FetchConversation:
    mediaId: string *
    [<OptionalAttribute>] ?fields: ReplyManagement.ReplyField seq *
    [<OptionalAttribute>] ?reverse: bool *
    [<OptionalAttribute>] ?cancellationToken: CancellationToken ->
      Task<ReplyManagement.ConversationResponse>

  abstract FetchUserReplies:
    userId: string *
    [<OptionalAttribute>] ?fields: ReplyManagement.ReplyField seq *
    [<OptionalAttribute>] ?cancellationToken: CancellationToken ->
      Task<ReplyManagement.ConversationResponse>

  abstract ManageReply:
    replyId: string *
    shouldHide: bool *
    [<OptionalAttribute>] ?cancellationToken: CancellationToken ->
      Task<bool>

type PostService =
  abstract PostContainer:
    profileId: string *
    postParams: Posts.PostParam seq *
    [<OptionalAttribute>] ?cancellationToken: CancellationToken ->
      Task<Posts.PostId>

  abstract PostCarouselItemContainer:
    profileId: string *
    postParams: Posts.PostParam seq *
    [<OptionalAttribute>] ?cancellationToken: CancellationToken ->
      Task<Posts.PostId>

  abstract PostCarousel:
    profileId: string *
    children: Posts.PostId seq *
    [<OptionalAttribute>] ?textContent: string *
    [<OptionalAttribute>] ?cancellationToken: CancellationToken ->
      Task<Posts.PostId>

  abstract PublishPost:
    profileId: string *
    containerId: Posts.PostId *
    [<OptionalAttribute>] ?cancellationToken: CancellationToken ->
      Task<Posts.PostId>

type MediaService =
  abstract FetchThreads:
    profileId: string *
    [<OptionalAttribute>] ?fields: Media.ThreadField seq *
    [<OptionalAttribute>] ?pagination: PaginationKind *
    [<OptionalAttribute>] ?cancellationToken: CancellationToken ->
      Task<Media.ThreadListResponse>

  abstract FetchThread:
    threadId: string *
    [<OptionalAttribute>] ?fields: Media.ThreadField seq *
    [<OptionalAttribute>] ?cancellationToken: CancellationToken ->
      Task<Media.ThreadValue seq>

type ProfileService =
  abstract FetchProfile:
    profileId: string *
    [<OptionalAttribute>] ?profileFields: Profiles.ProfileField seq *
    [<OptionalAttribute>] ?cancelaltionToken: CancellationToken ->
      Task<Profiles.ProfileValue seq>


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
            [<OptionalAttribute>] ?profileFields,
            [<OptionalAttribute>] ?cancellationToken
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
          (
            threadId,
            [<OptionalAttribute>] ?fields,
            [<OptionalAttribute>] ?cancellationToken
          ) =

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
            [<OptionalAttribute>] ?fields,
            [<OptionalAttribute>] ?pagination,
            [<OptionalAttribute>] ?cancellationToken
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

  let getPostService postCarousel postSingle postCarouselItem publishPost =
    { new PostService with
        member _.PostCarousel
          (
            profileId,
            children,
            [<OptionalAttribute>] ?textContent,
            [<OptionalAttribute>] ?cancellationToken
          ) =

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

        member _.PostContainer
          (profileId, postParams, [<OptionalAttribute>] ?cancellationToken)
          =

          let work = async {
            match! postSingle profileId postParams with
            | Ok value -> return value
            | Error err ->
              match err with
              | Posts.SingleContainerError.IsCarouselInSingleContainer ->
                return
                  nameof(Posts.SingleContainerError.IsCarouselInSingleContainer)
                  |> exn
                  |> raise
              | Posts.SingleContainerError.IsImageButImageNotProvided ->
                return
                  nameof(Posts.SingleContainerError.IsImageButImageNotProvided)
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
          (profileId, postParams, [<OptionalAttribute>] ?cancellationToken)
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
            [<OptionalAttribute>] ?cancellationToken: CancellationToken
          ) : Task<Posts.PostId> =

          Async.StartImmediateAsTask(
            publishPost profileId containerId,
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
            [<OptionalAttribute>] ?fields,
            [<OptionalAttribute>] ?reverse,
            [<OptionalAttribute>] ?cancellationToken
          ) =
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


        member _.FetchRateLimits
          (
            userId,
            [<OptionalAttribute>] ?fields,
            [<OptionalAttribute>] ?cancellationToken
          ) =
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
            [<OptionalAttribute>] ?fields,
            [<OptionalAttribute>] ?reverse,
            [<OptionalAttribute>] ?cancellationToken
          ) =
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

        member _.FetchUserReplies
          (
            userId,
            [<OptionalAttribute>] ?fields,
            [<OptionalAttribute>] ?cancellationToken
          ) =
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

        member _.ManageReply
          (replyId, shouldHide, [<OptionalAttribute>] ?cancellationToken)
          =
          Async.StartImmediateAsTask(
            manageReply replyId shouldHide,
            ?cancellationToken = cancellationToken
          )
    }

  let getInsights fetchUserInsights fetchMediaInsights =
    { new InsightsService with
        member _.FetchMediaInsights
          (mediaId, metrics, [<OptionalAttribute>] ?cancellationToken)
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
          (
            userId,
            metrics,
            insightParams,
            [<OptionalAttribute>] ?cancellationToken
          ) =
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
  static member Create
    (accessToken, [<OptionalAttribute>] ?headerContext: HeaderContext) =

    let baseHttp =
      defaultArg
        headerContext
        (http { config_useBaseUrl "https://graph.threads.net/v1.0/" })

    let fetchProfile = Profiles.getProfile baseHttp accessToken

    let fetchThreads = Media.getThreads baseHttp accessToken
    let fetchThread = Media.getThread baseHttp accessToken

    let postSingle = Posts.createSingleContainer baseHttp accessToken

    let postCarouselItem =
      Posts.createCarouselItemContainer baseHttp accessToken

    let postCarousel = Posts.createCarouselContainer baseHttp accessToken
    let publishPost = Posts.publishContainer baseHttp accessToken

    let fetchRateLimits = ReplyManagement.getRateLimits baseHttp accessToken
    let fetchReplies = ReplyManagement.getReplies baseHttp accessToken
    let fetchConvos = ReplyManagement.getConversations baseHttp accessToken
    let allUserReplies = ReplyManagement.getUserReplies baseHttp accessToken
    let manageReply = ReplyManagement.manageReply baseHttp accessToken

    let fetchUserInsights = Insights.getUserInsights baseHttp accessToken
    let fetchMediaInsights = Insights.getMediaInsights baseHttp accessToken

    let profile = Impl.getProfileService fetchProfile
    let media = Impl.getMediaService fetchThread fetchThreads

    let posts =
      Impl.getPostService postCarousel postSingle postCarouselItem publishPost

    let replies =
      Impl.getReplyManagement
        manageReply
        fetchRateLimits
        fetchConvos
        fetchReplies
        allUserReplies

    let insights = Impl.getInsights fetchUserInsights fetchMediaInsights

    { new ThreadsClient with
        member _.Media = media
        member _.Posts = posts
        member _.Profile = profile
        member _.Replies = replies
        member _.Insights = insights
    }
