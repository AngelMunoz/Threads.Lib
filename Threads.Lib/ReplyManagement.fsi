namespace Threads.Lib

open Thoth.Json.Net
open FsHttp

module ReplyManagement =
  open System

  [<Struct>]
  type RateLimitField =
    | ReplyQuotaUsage
    | ReplyConfig

  [<Struct>]
  type ReplyConfig = {
    quotaTotal: int64
    quotaDuration: int64
  }

  [<Struct>]
  type RateLimitFieldValue =
    | ReplyQuotaUsage of rqu: uint
    | ReplyConfig of rc: ReplyConfig

  type RateLimitResponse = { data: RateLimitFieldValue seq seq }

  [<Struct>]
  type MediaProductType = | Threads

  [<Struct>]
  type MediaType =
    | TextPost
    | Image
    | Video
    | CarouselAlbum
    | Audio

  [<Struct>]
  type HideStatus =
    | NotHushed
    | Unhushed
    | Hidden
    | Covered
    | Blocked
    | Restricted

  [<Struct>]
  type ReplyAudience =
    | Everyone
    | AccountsYouFollow
    | MentionedOnly

  [<Struct>]
  type ReplyField =
    | Id
    | Text
    | Username
    | Permalink
    | Timestamp
    | MediaProductType
    | MediaType
    | MediaUrl
    | Shortcode
    | ThumbnailUrl
    | Children
    | IsQuotePost
    | HasReplies
    | RootPost
    | RepliedTo
    | IsReply
    | IsReplyOwnedByMe
    | HideStatus
    | ReplyAudience

  [<Struct>]
  type PostId = { id: string }

  type ReplyFieldValue =
    | Id of string
    | Text of string
    | Username of string
    | Permalink of Uri
    | Timestamp of DateTimeOffset
    | MediaProductType of MediaProductType
    | MediaType of MediaType
    | MediaUrl of Uri
    | Shortcode of string
    | ThumbnailUrl of Uri
    | Children of PostId array
    | IsQuotePost of bool
    | HasReplies of bool
    | RootPost of PostId
    | RepliedTo of PostId
    | IsReply of bool
    | IsReplyOwnedByMe of bool
    | HideStatus of HideStatus
    | ReplyAudience of ReplyAudience

  type ConversationResponse = {
    data: ReplyFieldValue seq seq
    paging: Pagination
  }

  val internal getRateLimits:
    baseUrl: string ->
    accessToken: string ->
    userId: string ->
    fields: RateLimitField seq ->
      Async<Result<RateLimitResponse, string>>

  val internal getReplies:
    baseUrl: string ->
    accessToken: string ->
    mediaId: string ->
    fields: ReplyField seq ->
    reverse: bool ->
      Async<Result<ConversationResponse, string>>

  val internal getConversations:
    baseUrl: string ->
    accessToken: string ->
    mediaId: string ->
    fields: ReplyField seq ->
    reverse: bool ->
      Async<Result<ConversationResponse, string>>

  val internal getUserReplies:
    baseUrl: string ->
    accessToken: string ->
    userId: string ->
    fields: ReplyField seq ->
      Async<Result<ConversationResponse, string>>

  val internal manageReply:
    baseUrl: string ->
    accessToken: string ->
    replyId: string ->
    shouldHide: bool ->
      Async<bool>
