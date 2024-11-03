namespace Threads.Lib

open System
open Threads.Lib.Common

module Media =

  [<Struct>]
  type MediaType =
    | TextPost
    | Image
    | Video
    | CarouselAlbum
    | Audio
    | RepostFacade

  type ThreadChildren = { data: IdLike list }

  [<Struct>]
  type ThreadField =
    | Id
    | MediaProductType
    | MediaType
    | MediaUrl
    | Permalink
    | Owner
    | Username
    | Text
    | Timestamp
    | ShortCode
    | ThumbnailUrl
    | Children
    | IsQuotePost

  [<RequireQualifiedAccess>]
  type ThreadValue =
    | Id of string
    | MediaProductType of MediaProductType
    | MediaType of MediaType
    | MediaUrl of Uri
    | Permalink of Uri
    | Owner of IdLike
    | Username of string
    | Text of string
    | Timestamp of DateTimeOffset
    | ShortCode of string
    | ThumbnailUrl of Uri
    | Children of ThreadChildren
    | IsQuotePost of bool

  type ThreadListResponse = {
    data: ThreadValue list list
    paging: Pagination
  }

  val internal getThreads:
    baseUrl: string ->
    accessToken: string ->
    profileId: string ->
    pagination: PaginationKind option ->
    threadFields: ThreadField seq ->
      Async<Result<ThreadListResponse, string>>

  val internal getThread:
    baseUrl: string ->
    accessToken: string ->
    threadId: string ->
    threadFields: ThreadField seq ->
      Async<Result<ThreadValue list, string>>
