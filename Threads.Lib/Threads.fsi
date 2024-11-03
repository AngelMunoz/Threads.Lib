namespace Threads.Lib

open System.Threading
open System.Threading.Tasks
open System.Runtime.InteropServices

open Threads.Lib.Common

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
    [<Optional>] ?reverse: bool *
    [<Optional>] ?cancellationToken: CancellationToken ->
      Task<ReplyManagement.ConversationResponse>

  abstract FetchConversation:
    mediaId: string *
    [<Optional>] ?fields: ReplyManagement.ReplyField seq *
    [<Optional>] ?reverse: bool *
    [<Optional>] ?cancellationToken: CancellationToken ->
      Task<ReplyManagement.ConversationResponse>

  abstract FetchUserReplies:
    userId: string *
    [<Optional>] ?fields: ReplyManagement.ReplyField seq *
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
    [<Optional>] ?cancelaltionToken: CancellationToken ->
      Task<Profiles.ProfileValue list>

type ThreadsClient =
  abstract Media: MediaService
  abstract Posts: PostService
  abstract Profile: ProfileService
  abstract Replies: ReplyManagementService
  abstract Insights: InsightsService

[<Class>]
type Threads =
  static member Create:
    accessToken: string * [<Optional>] ?baseUrl: string -> ThreadsClient
