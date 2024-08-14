namespace Threads.Lib

open System.Threading
open System.Threading.Tasks
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

module ThreadsClient =
  [<CompiledName "Create">]
  val create: accessToken: string -> ThreadsClient
