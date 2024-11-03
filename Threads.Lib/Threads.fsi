namespace Threads.Lib

open System.Threading
open System.Threading.Tasks
open System.Runtime.InteropServices



/// <summary>
/// The Threads Insights API allows you to read the insights from users' own Threads.
/// The Threads Insights API requires an appropriate access token and permissions based on the node you are targeting.
/// </summary>
/// <remarks>
/// In the Threads API language, "Media" is the term used to describe what is commonly refered as "Post" in the threads app.
/// </remarks>
type InsightsService =

  /// <summary>
  /// Retrieve insights for a specific post.
  /// </summary>
  /// <remarks>
  /// Returned metrics do not capture nested replies' metrics.
  /// </remarks>
  /// <remarks>
  /// An empty array will be returned for <see cref="T:Threads.Lib.Media.MediaType.RepostFacade">RepostFacade</see> posts because they are posts made by other users.
  /// </remarks>
  /// <param name="mediaId">The posts' id for which to retrieve insights.</param>
  /// <param name="metrics">The metrics to retrieve insights for.</param>
  /// <param name="cancellationToken">A token to cancel the operation.</param>
  abstract FetchMediaInsights:
    mediaId: string *
    metrics: Insights.Metric seq *
    [<Optional>] ?cancellationToken: CancellationToken ->
      Task<Insights.MetricResponse>

  /// <summary>
  /// Retrieve insights for a specific user.
  /// </summary>
  /// <remarks>
  /// User insights are not guaranteed to work before June 1, 2024.
  /// </remarks>
  /// <param name="userId"></param>
  /// <param name="metrics"></param>
  /// <param name="insightParams"></param>
  /// <param name="cancellationToken"></param>
  abstract FetchUserInsights:
    userId: string *
    metrics: Insights.Metric seq *
    insightParams: Insights.InsightParam seq *
    [<Optional>] ?cancellationToken: CancellationToken ->
      Task<Insights.MetricResponse>

/// <summary>
/// The Threads Reply Management API allows you to read and manage replies to users' own Threads.
///
/// Replying to a post is a two-step process: first you need to create a media object with the <see cref="T:Threads.Lib.Posts.PostParam.ReplyTo">ReplyTo</see> parameter to obtain its id, then you have to publish that media object.
/// For more information please see the <seealso cref="T:Threads.Lib.PostService">PostService</seealso> on how to create media objects and publish them.
/// </summary>
type ReplyManagementService =

  /// <summary>
  /// Fetch the rate limits for the current user.
  /// </summary>
  /// <remarks>
  /// Threads profiles are limited to 1,000 API-published replies within a 24-hour moving period.
  /// </remarks>
  /// <remarks>
  /// This method requires the threads_basic, threads_content_publish, and threads_manage_replies permissions.
  /// in your access token.
  /// </remarks>
  /// <param name="userId">The user's id for which to retrieve rate limits.</param>
  /// <param name="fields">The fields to retrieve rate limits for.</param>
  /// <param name="cancellationToken">A token to cancel the operation.</param>
  /// <returns>A response containing the rate limits for the user.</returns>
  abstract FetchRateLimits:
    userId: string *
    [<Optional>] ?fields: ReplyManagement.RateLimitField seq *
    [<Optional>] ?cancellationToken: CancellationToken ->
      Task<ReplyManagement.RateLimitResponse>

  /// <summary>
  /// This method is applicable to the use cases that focus on the depth level of the replies.
  /// The method returns the immediate replies of the requested Threads ID.
  /// HasReplies indicates whether a Thread has nested replies or not and the field can be used to decide to chain further subsequent calls to retrieve replies located in the deeper levels.
  /// </summary>
  /// <param name="mediaId">The post's id for which to retrieve replies.</param>
  /// <param name="fields">The fields to retrieve replies for.</param>
  /// <param name="pagination">The pagination to retrieve replies for.</param>
  /// <param name="reverse">Whether to reverse the order of the replies.</param>
  /// <param name="cancellationToken">A token to cancel the operation.</param>
  /// <returns>A response containing the top level replies for the post.</returns>
  abstract FetchReplies:
    mediaId: string *
    [<Optional>] ?fields: ReplyManagement.ReplyField seq *
    [<Optional>] ?pagination: PaginationKind *
    [<Optional>] ?reverse: bool *
    [<Optional>] ?cancellationToken: CancellationToken ->
      Task<ReplyManagement.ConversationResponse>

  /// <summary>
  /// This method is applicable to specific use cases that do not focus on the knowledge of the depthness of the replies.
  /// </summary>
  /// <param name="mediaId">The post's id for which to retrieve replies.</param>
  /// <param name="fields">The fields to retrieve replies for.</param>
  /// <param name="pagination">The pagination to retrieve replies for.</param>
  /// <param name="reverse">Whether to reverse the order of the replies.</param>
  /// <param name="cancellationToken">A token to cancel the operation.</param>
  /// <returns>A response containing the replies for the post.</returns>
  /// <remarks>
  /// This method is only intended to be used on the root-level threads with replies.
  /// </remarks>
  abstract FetchConversation:
    mediaId: string *
    [<Optional>] ?fields: ReplyManagement.ReplyField seq *
    [<Optional>] ?pagination: PaginationKind *
    [<Optional>] ?reverse: bool *
    [<Optional>] ?cancellationToken: CancellationToken ->
      Task<ReplyManagement.ConversationResponse>

  /// <summary>
  /// Fetch the replies of a specific user.
  /// </summary>
  /// <param name="userId">The user's id for which to retrieve replies.</param>
  /// <param name="fields">The fields to retrieve replies for.</param>
  /// <param name="pagination">The pagination to retrieve replies for.</param>
  /// <param name="cancellationToken">A token to cancel the operation.</param>
  /// <returns>A response containing the replies for the user.</returns>
  abstract FetchUserReplies:
    userId: string *
    [<Optional>] ?fields: ReplyManagement.ReplyField seq *
    [<Optional>] ?pagination: PaginationKind *
    [<Optional>] ?cancellationToken: CancellationToken ->
      Task<ReplyManagement.ConversationResponse>

  /// <summary>
  /// Controls whether to show or hide a reply and its nested replies.
  /// </summary>
  /// <param name="replyId">The reply's id to manage.</param>
  /// <param name="shouldHide">Whether to hide the reply and its nested replies.</param>
  /// <param name="cancellationToken">A token to cancel the operation.</param>
  /// <returns>True if the operation was successful, false otherwise.</returns>
  /// <remarks>
  /// Replies nested deeper than the top-level reply cannot be targeted in isolation to be hidden/unhidden.
  /// </remarks>
  abstract ManageReply:
    replyId: string *
    shouldHide: bool *
    [<Optional>] ?cancellationToken: CancellationToken ->
      Task<bool>

/// <summary>
/// You can use the Threads API to publish image, video, text, or carousel posts.
///
/// Publishing a single image, video, or text post is a two-step process:
/// 1. Use the <see cref="T:Threads.Lib.PostService.PostContainer">PostContainer</see> methid to ceate a media container using an image or video hosted on your public server and optional text. Alternatively, use this method to create a media container with text only.
/// 2. Use the <see cref="T:Threads.Lib.PostService.PublishPost">PublishPost</see> method to publish the media container.
///
/// For carousel posts, you may publish up to 20 images, videos, or a mix of the two in a carousel post. Publishing carousels is a three-step process:
///
/// 1. Use the <see cref="T:Threads.Lib.PostService.PostCarouselItemContainer">PostCarouselItemContainer</see> method to create a media container for each image or video in the carousel.
/// 2. Use the <see cref="T:Threads.Lib.PostService.PostCarousel">PostCarousel</see> method to create a carousel container with the media container ids and optional text.
/// 3. Use the <see cref="T:Threads.Lib.PostService.PublishPost">PublishPost</see> method with the id returned from the <see cref="T:Threads.Lib.PostService.PostCarousel">PostCarousel</see> method to publish the carousel.
///
/// Carousel posts count as a single post against the profile's
///
/// Limitations:
/// - Carousels are limited to 20 images, videos, or a mix of the two.
/// - Carousels require a minimum of two children.
/// </summary>
type PostService =

  /// <summary>
  /// Create a media container for a single image, video, or text post.
  /// </summary>
  /// <param name="profileId">The profile's id to create the media container for.</param>
  /// <param name="postParams">The post parameters to create the media container with.</param>
  /// <param name="cancellationToken">A token to cancel the operation.</param>
  /// <returns>The id of the created media container.</returns>
  /// <remarks>
  /// The following parameters are required.
  ///
  /// - <see cref="T:Threads.Lib.Posts.PostParam.ImageUrl">ImageUrl</see> for image posts.
  /// - <see cref="T:Threads.Lib.Posts.PostParam.VideoUrl">VideoUrl</see> for video posts.
  /// - <see cref="T:Threads.Lib.Posts.PostParam.MediaType">MediaType</see> for posts, note that Carousel is not supported for this method.
  /// - <see cref="T:Threads.Lib.Posts.PostParam.Text">Text</see> for text only posts.
  /// </remarks>
  /// <exception cref="T:Threads.Lib.Posts.SingleContainerArgumentException">Thrown when the post parameters are invalid.</exception>
  abstract PostContainer:
    profileId: string *
    postParams: Posts.PostParam seq *
    [<Optional>] ?cancellationToken: CancellationToken ->
      Task<IdLike>

  /// <summary>
  /// Create a media container for a single image, video, or text post that will be part of a carousel post.
  /// </summary>
  /// <param name="profileId">The profile's id to create the media container for.</param>
  /// <param name="postParams">The post parameters to create the media container with.</param>
  /// <param name="cancellationToken">A token to cancel the operation.</param>
  /// <returns>The id of the created media container.</returns>
  /// <remarks>
  /// The following parameters are required.
  ///
  /// - <see cref="T:Threads.Lib.Posts.PostParam.ImageUrl">ImageUrl</see> for image posts.
  /// - <see cref="T:Threads.Lib.Posts.PostParam.VideoUrl">VideoUrl</see> for video posts.
  /// - <see cref="T:Threads.Lib.Posts.PostParam.MediaType">MediaType</see> for posts, note that Carousel is not supported for this method.
  /// - <see cref="T:Threads.Lib.Posts.PostParam.Text">Text</see> for text only posts.
  /// </remarks>
  /// <exception cref="T:Threads.Lib.Posts.CarouselItemContainerArgumentException">Thrown when the post parameters are invalid.</exception>
  abstract PostCarouselItemContainer:
    profileId: string *
    postParams: Posts.PostParam seq *
    [<Optional>] ?cancellationToken: CancellationToken ->
      Task<IdLike>

  /// <summary>
  /// Creates a media container for a carousel post.
  /// </summary>
  /// <param name="profileId">The profile's id to create the media container for.</param>
  /// <param name="children">The media container ids to create the carousel container with.</param>
  /// <param name="textContent">The text content to create the carousel container with.</param>
  /// <param name="cancellationToken">A token to cancel the operation.</param>
  /// <returns>The id of the created media container.</returns>
  /// <exception cref="T:Threads.Lib.Posts.CarouselContainerArgumentException">Thrown when the post parameters are invalid.</exception>
  abstract PostCarousel:
    profileId: string *
    children: IdLike seq *
    [<Optional>] ?textContent: string *
    [<Optional>] ?cancellationToken: CancellationToken ->
      Task<IdLike>

  /// <summary>
  ///  Publish a media container for the given container id.
  /// </summary>
  /// <param name="profileId">The profile's id to publish the media container for.</param>
  /// <param name="containerId">The container's id to publish.</param>
  /// <param name="cancellationToken">A token to cancel the operation.</param>
  /// <returns>The id of the published media container.</returns>
  /// <remarks>
  ///  To publish the container ID returned in the previous step. It is recommended to wait on average 30 seconds before publishing a Threads media container to give threads' servers enough time to fully process the upload
  /// </remarks>
  abstract PublishPost:
    profileId: string *
    containerId: IdLike *
    [<Optional>] ?cancellationToken: CancellationToken ->
      Task<IdLike>

  /// <summary>
  /// Reposts an existing media object.
  /// </summary>
  /// <param name="mediaId">The media id to repost.</param>
  /// <param name="cancellationToken">A token to cancel the operation.</param>
  /// <returns>The id of the reposted media object.</returns>
  /// <remarks>
  /// The mediaId must be that of an already published media object.
  /// </remarks>
  abstract Repost:
    mediaId: string * [<Optional>] ?cancellationToken: CancellationToken ->
      Task<IdLike>

/// <summary>
/// This service provides methods to fetch media and profile information.
/// In the Threads API language, "Media" is the term used to describe what is commonly refered as "Post" in the threads app.
/// </summary>
type MediaService =

  /// <summary>
  /// Fetch the threads of the supplied profile.
  /// </summary>
  /// <param name="profileId">Profile to fetch threads from.</param>
  /// <param name="fields">The fields to fetch threads with.</param>
  /// <param name="pagination">The pagination to fetch threads with.</param>
  /// <param name="cancellationToken">A token to cancel the operation.</param>
  /// <returns>The fetched media object.</returns>
  abstract FetchThreads:
    profileId: string *
    [<Optional>] ?fields: Media.ThreadField seq *
    [<Optional>] ?pagination: PaginationKind *
    [<Optional>] ?cancellationToken: CancellationToken ->
      Task<Media.ThreadListResponse>

  /// <summary>
  /// Fetch the media object of the supplied media id.
  /// </summary>
  /// <param name="threadId">The media id to fetch.</param>
  /// <param name="fields">The fields to fetch the media object with.</param>
  /// <param name="cancellationToken">A token to cancel the operation.</param>
  /// <returns>The fetched media object.</returns>
  abstract FetchThread:
    threadId: string *
    [<Optional>] ?fields: Media.ThreadField seq *
    [<Optional>] ?cancellationToken: CancellationToken ->
      Task<Media.ThreadValue list>

/// <summary>
/// Provides means to obtain the profile information about a Threads user.
/// </summary>
type ProfileService =

  /// <summary>
  /// Fetch the profile information of the supplied profile id.
  /// </summary>
  /// <param name="profileId">The profile id to fetch.</param>
  /// <param name="profileFields">The fields to fetch the profile object with.</param>
  /// <param name="cancellationToken">A token to cancel the operation.</param>
  /// <returns>The fetched profile object.</returns>
  abstract FetchProfile:
    profileId: string *
    [<Optional>] ?profileFields: Profiles.ProfileField seq *
    [<Optional>] ?cancellationToken: CancellationToken ->
      Task<Profiles.ProfileValue list>

/// <summary>
/// A raw client that groups all the services together.
/// </summary>
[<Interface>]
type ThreadsClient =

  /// <summary>
  /// This service provides methods to fetch media and profile information.
  /// In the Threads API language, "Media" is the term used to describe what is commonly refered as "Post" in the threads app.
  /// </summary>
  abstract Media: MediaService
  /// <summary>
  /// You can use the Threads API to publish image, video, text, or carousel posts.
  ///
  /// Publishing a single image, video, or text post is a two-step process:
  /// 1. Use the <see cref="T:Threads.Lib.PostService.PostContainer">PostContainer</see> methid to ceate a media container using an image or video hosted on your public server and optional text. Alternatively, use this method to create a media container with text only.
  /// 2. Use the <see cref="T:Threads.Lib.PostService.PublishPost">PublishPost</see> method to publish the media container.
  ///
  /// For carousel posts, you may publish up to 20 images, videos, or a mix of the two in a carousel post. Publishing carousels is a three-step process:
  ///
  /// 1. Use the <see cref="T:Threads.Lib.PostService.PostCarouselItemContainer">PostCarouselItemContainer</see> method to create a media container for each image or video in the carousel.
  /// 2. Use the <see cref="T:Threads.Lib.PostService.PostCarousel">PostCarousel</see> method to create a carousel container with the media container ids and optional text.
  /// 3. Use the <see cref="T:Threads.Lib.PostService.PublishPost">PublishPost</see> method with the id returned from the <see cref="T:Threads.Lib.PostService.PostCarousel">PostCarousel</see> method to publish the carousel.
  ///
  /// Carousel posts count as a single post against the profile's
  ///
  /// Limitations:
  /// - Carousels are limited to 20 images, videos, or a mix of the two.
  /// - Carousels require a minimum of two children.
  /// </summary>
  abstract Posts: PostService

  /// <summary>
  /// Provides means to obtain the profile information about a Threads user.
  /// </summary>
  abstract Profile: ProfileService
  /// <summary>
  /// The Threads Reply Management API allows you to read and manage replies to users' own Threads.
  ///
  /// Replying to a post is a two-step process: first you need to create a media object with the <see cref="T:Threads.Lib.Posts.PostParam.ReplyTo">ReplyTo</see> parameter to obtain its id, then you have to publish that media object.
  /// For more information please see the <seealso cref="T:Threads.Lib.PostService">PostService</seealso> on how to create media objects and publish them.
  /// </summary>
  abstract Replies: ReplyManagementService
  /// <summary>
  /// The Threads Insights API allows you to read the insights from users' own Threads.
  /// The Threads Insights API requires an appropriate access token and permissions based on the node you are targeting.
  /// </summary>
  /// <remarks>
  /// In the Threads API language, "Media" is the term used to describe what is commonly refered as "Post" in the threads app.
  /// </remarks>
  abstract Insights: InsightsService

[<Class>]
type Threads =
  /// <summary>
  /// Orchestrates the Threads API services.
  /// </summary>
  /// <param name="accessToken">The access token to authenticate with.</param>
  /// <param name="baseUrl">The base URL to use for the API.</param>
  static member Create:
    accessToken: string * [<Optional>] ?baseUrl: string -> ThreadsClient
