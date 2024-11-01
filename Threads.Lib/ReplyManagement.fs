namespace Threads.Lib

open System.Collections.Generic
open Thoth.Json.Net
open Flurl
open Flurl.Http
open Threads.Lib.Common

module ReplyManagement =
  open System

  [<Struct>]
  type RateLimitField =
    | ReplyQuotaUsage
    | ReplyConfig

  module RateLimitField =
    let asString =
      function
      | ReplyQuotaUsage -> "reply_quota_usage"
      | ReplyConfig -> "reply_config"

  [<Struct>]
  type ReplyConfig = {
    quotaTotal: int64
    quotaDuration: int64
  }

  module ReplyConfig =
    let Decode: Decoder<ReplyConfig> =
      Decode.object(fun get -> {
        quotaTotal = get.Required.Field "quota_total" Decode.int64
        quotaDuration = get.Required.Field "quota_dutarion" Decode.int64
      })

  [<Struct>]
  type RateLimitFieldValue =
    | ReplyQuotaUsage of rqu: uint
    | ReplyConfig of rc: ReplyConfig

  module RateLimitFieldValue =
    let Decode: Decoder<RateLimitFieldValue seq> =
      Decode.object(fun get ->
        let replyQuotaUsage =
          get.Optional.Field "reply_quota_usage" Decode.uint32

        let replyConfig = get.Optional.Field "reply_config" ReplyConfig.Decode

        [
          match replyQuotaUsage with
          | Some q -> RateLimitFieldValue.ReplyQuotaUsage q
          | None -> ()

          match replyConfig with
          | Some rc -> RateLimitFieldValue.ReplyConfig rc
          | None -> ()
        ])

  type RateLimitResponse = { data: RateLimitFieldValue seq seq }

  module RateLimitResponse =
    let Decode: Decoder<RateLimitResponse> =
      Decode.object(fun get -> {
        data =
          get.Required.Field "data" (Decode.array RateLimitFieldValue.Decode)
      })

  [<Struct>]
  type MediaType =
    | TextPost
    | Image
    | Video
    | CarouselAlbum
    | Audio

  module MediaType =
    let asString =
      function
      | TextPost -> "TEXT_POST"
      | Image -> "IMAGE"
      | Video -> "VIDEO"
      | CarouselAlbum -> "CAROUSEL_ALBUM"
      | Audio -> "AUDIO"

  [<Struct>]
  type HideStatus =
    | NotHushed
    | Unhushed
    | Hidden
    | Covered
    | Blocked
    | Restricted

  module HideStatus =
    let asString =
      function
      | NotHushed -> "NOT_HUSHED"
      | Unhushed -> "UNHUSHED"
      | Hidden -> "HIDDEN"
      | Covered -> "COVERED"
      | Blocked -> "BLOCKED"
      | Restricted -> "RESTRICTED"

  [<Struct>]
  type ReplyAudience =
    | Everyone
    | AccountsYouFollow
    | MentionedOnly

  module ReplyAudience =
    let asString =
      function
      | Everyone -> "EVERYONE"
      | AccountsYouFollow -> "ACCOUNTS_YOU_FOLLOW"
      | MentionedOnly -> "MENTIONED_ONLY"

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

  module ReplyField =
    let asString =
      function
      | Id -> "id"
      | Text -> "text"
      | Username -> "username"
      | Permalink -> "permalink"
      | Timestamp -> "timestamp"
      | MediaProductType -> "media_product_type"
      | MediaType -> "media_type"
      | MediaUrl -> "media_url"
      | Shortcode -> "shortcode"
      | ThumbnailUrl -> "thuimbnail_irl"
      | Children -> "children"
      | IsQuotePost -> "is_quote_post"
      | HasReplies -> "has_replies"
      | RootPost -> "root_post"
      | RepliedTo -> "replied_to"
      | IsReply -> "is_reply"
      | IsReplyOwnedByMe -> "is_reply_owned_by_me"
      | HideStatus -> "hide_status"
      | ReplyAudience -> "reply_audience"

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
    | Children of IdLike array
    | IsQuotePost of bool
    | HasReplies of bool
    | RootPost of IdLike
    | RepliedTo of IdLike
    | IsReply of bool
    | IsReplyOwnedByMe of bool
    | HideStatus of HideStatus
    | ReplyAudience of ReplyAudience

  module ReplyFieldValue =
    let decodeId (get: Decode.IGetters) (values: ReplyFieldValue ResizeArray) =
      get.Required.Field "id" Decode.string |> Id |> values.Add

      get, values

    let decodeText(get: Decode.IGetters, values: ReplyFieldValue ResizeArray) =
      get.Optional.Field "text" Decode.string
      |> Option.map(Text >> values.Add)
      |> Option.defaultValue()

      get, values

    let decodeUsername
      (get: Decode.IGetters, values: ReplyFieldValue ResizeArray)
      =
      get.Optional.Field "username" Decode.string
      |> Option.map(Username >> values.Add)
      |> Option.defaultValue()

      get, values

    let decodePermalink
      (get: Decode.IGetters, values: ReplyFieldValue ResizeArray)
      =
      get.Optional.Field "permalink" Decode.string
      |> Option.map(Uri >> Permalink >> values.Add)
      |> Option.defaultValue()

      get, values

    let decodeTimestamp
      (get: Decode.IGetters, values: ReplyFieldValue ResizeArray)
      =
      get.Optional.Field "timestamp" Decode.datetimeOffset
      |> Option.map(Timestamp >> values.Add)
      |> Option.defaultValue()

      get, values

    let decodeMediaProductType
      (get: Decode.IGetters, values: ReplyFieldValue ResizeArray)
      =
      get.Optional.Field "media_product_type" Decode.string
      |> Option.map(fun value ->
        match value with
        | "THREADS" -> values.Add(MediaProductType Threads)
        | other -> () // new value added?
      )
      |> Option.defaultValue()

      get, values

    let decodeMediaType
      (get: Decode.IGetters, values: ReplyFieldValue ResizeArray)
      =
      get.Optional.Field "media_type" Decode.string
      |> Option.map(fun value ->
        match value with
        | "TEXT_POST" -> values.Add(MediaType TextPost)
        | "IMAGE" -> values.Add(MediaType Image)
        | "VIDEO" -> values.Add(MediaType Video)
        | "CAROUSEL_ALBUM" -> values.Add(MediaType CarouselAlbum)
        | "AUDIO" -> values.Add(MediaType Audio)
        | "THREADS" -> values.Add(MediaProductType Threads)
        | other -> () // new value added?
      )
      |> Option.defaultValue()

      get, values

    let decodeMediaUrl
      (get: Decode.IGetters, values: ReplyFieldValue ResizeArray)
      =
      get.Optional.Field "media_url" Decode.string
      |> Option.map(Uri >> MediaUrl >> values.Add)
      |> Option.defaultValue()

      get, values

    let decodeShortcode
      (get: Decode.IGetters, values: ReplyFieldValue ResizeArray)
      =
      get.Optional.Field "shortcode" Decode.string
      |> Option.map(Shortcode >> values.Add)
      |> Option.defaultValue()

      get, values

    let decodeThumbnailUrl
      (get: Decode.IGetters, values: ReplyFieldValue ResizeArray)
      =
      get.Optional.Field "thumbnail_url" Decode.string
      |> Option.map(Uri >> ThumbnailUrl >> values.Add)
      |> Option.defaultValue()

      get, values

    let decodeChildren
      (get: Decode.IGetters, values: ReplyFieldValue ResizeArray)
      =
      get.Optional.Field "children" (Decode.array IdLike.Decode)
      |> Option.map(Children >> values.Add)
      |> Option.defaultValue()

      get, values


    let decodeIsQuotePost
      (get: Decode.IGetters, values: ReplyFieldValue ResizeArray)
      =
      get.Optional.Field "is_quote_post" Decode.bool
      |> Option.map(IsQuotePost >> values.Add)
      |> Option.defaultValue()

      get, values

    let decodeHasReplies
      (get: Decode.IGetters, values: ReplyFieldValue ResizeArray)
      =
      get.Optional.Field "has_replies" Decode.bool
      |> Option.map(HasReplies >> values.Add)
      |> Option.defaultValue()

      get, values

    let decodeRootPost
      (get: Decode.IGetters, values: ReplyFieldValue ResizeArray)
      =
      get.Optional.Field "root_post" IdLike.Decode
      |> Option.map(RootPost >> values.Add)
      |> Option.defaultValue()

      get, values

    let decodeRepliedTo
      (get: Decode.IGetters, values: ReplyFieldValue ResizeArray)
      =
      get.Optional.Field "replied_to" IdLike.Decode
      |> Option.map(RepliedTo >> values.Add)
      |> Option.defaultValue()

      get, values

    let decodeIsReply
      (get: Decode.IGetters, values: ReplyFieldValue ResizeArray)
      =
      get.Optional.Field "is_reply" Decode.bool
      |> Option.map(IsReply >> values.Add)
      |> Option.defaultValue()

      get, values

    let decodeIsReplyOwnedByMe
      (get: Decode.IGetters, values: ReplyFieldValue ResizeArray)
      =
      get.Optional.Field "is_reply_owned_by_me" Decode.bool
      |> Option.map(IsReplyOwnedByMe >> values.Add)
      |> Option.defaultValue()

      get, values

    let decodeHideStatus
      (get: Decode.IGetters, values: ReplyFieldValue ResizeArray)
      =
      get.Optional.Field "is_reply_owned_by_me" Decode.string
      |> Option.map (function
        | "NOT_HUSHED" -> values.Add(HideStatus NotHushed)
        | "UNHUSHED" -> values.Add(HideStatus Unhushed)
        | "HIDDEN" -> values.Add(HideStatus Hidden)
        | "COVERED" -> values.Add(HideStatus Covered)
        | "BLOCKED" -> values.Add(HideStatus Blocked)
        | "RESTRICTED" -> values.Add(HideStatus Restricted)
        | other -> () // new value?
      )
      |> Option.defaultValue()

      get, values

    let decodeReplyAudience
      (get: Decode.IGetters, values: ReplyFieldValue ResizeArray)
      =
      get.Optional.Field "is_reply_owned_by_me" Decode.string
      |> Option.map (function
        | "EVERYONE" -> values.Add(ReplyAudience Everyone)
        | "ACCOUNTS_YOU_FOLLOW" -> values.Add(ReplyAudience AccountsYouFollow)
        | "MENTIONED_ONLY" -> values.Add(ReplyAudience MentionedOnly)
        | other -> () // new value?
      )
      |> Option.defaultValue()

      get, values

    let inline finish(_, values: ReplyFieldValue seq) = values

    let Decode: Decoder<ReplyFieldValue seq> =
      Decode.object(fun get ->

        ResizeArray()
        |> decodeId get
        |> decodeText
        |> decodeUsername
        |> decodePermalink
        |> decodeTimestamp
        |> decodeMediaProductType
        |> decodeMediaType
        |> decodeMediaUrl
        |> decodeShortcode
        |> decodeThumbnailUrl
        |> decodeChildren
        |> decodeIsQuotePost
        |> decodeHasReplies
        |> decodeRootPost
        |> decodeRepliedTo
        |> decodeIsReply
        |> decodeIsReplyOwnedByMe
        |> decodeHideStatus
        |> decodeReplyAudience
        |> finish)

  type ConversationResponse = {
    data: ReplyFieldValue seq seq
    paging: Pagination
  }

  module ReplyResponse =
    let Decode: Decoder<ConversationResponse> =
      Decode.object(fun get -> {
        data = get.Required.Field "data" (Decode.array ReplyFieldValue.Decode)
        paging = get.Required.Field "paging" Pagination.Decode
      })


  let getRateLimits (baseUrl: string) accessToken (userId: string) fields = async {
    let fields = fields |> Seq.toList

    let! req =

      baseUrl
        .AppendPathSegments(userId, "threads_publishing_limit")
        .SetQueryParams(
          [
            if fields.Length > 0 then
              let values = fields |> List.map RateLimitField.asString
              "fields", String.Join(",", values)
            "access_token", accessToken
          ]
        )
        .GetAsync()
      |> Async.AwaitTask

    let! res = req.GetStringAsync() |> Async.AwaitTask

    return Decode.fromString RateLimitResponse.Decode res
  }


  let getReplies
    (baseUrl: string)
    accessToken
    (mediaId: string)
    fields
    reverse
    =
    async {
      let fields = fields |> Seq.toList

      let! req =

        baseUrl
          .AppendPathSegments(mediaId, "replies")
          .SetQueryParams(
            [
              if fields.Length > 0 then
                let values = fields |> List.map ReplyField.asString
                "fields", String.Join(",", values)
              if reverse then
                "reverse", "true"
              "access_token", accessToken
            ]
          )
          .GetAsync()
        |> Async.AwaitTask

      let! res = req.GetStringAsync() |> Async.AwaitTask

      return Decode.fromString ReplyResponse.Decode res
    }

  let getConversations
    (baseUrl: string)
    accessToken
    (mediaId: string)
    fields
    reverse
    =
    async {
      let fields = fields |> Seq.toList

      let! req =

        baseUrl
          .AppendPathSegments(mediaId, "conversations")
          .SetQueryParams(
            [
              if fields.Length > 0 then
                let values = fields |> List.map ReplyField.asString
                "fields", String.Join(",", values)
              if reverse then
                "reverse", "true"
              "access_token", accessToken
            ]
          )
          .GetAsync()
        |> Async.AwaitTask

      let! res = req.GetStringAsync() |> Async.AwaitTask

      return Decode.fromString ReplyResponse.Decode res
    }

  let getUserReplies (baseUrl: string) accessToken (userId: string) fields = async {
    let fields = fields |> Seq.toList

    let! req =

      baseUrl
        .AppendPathSegments(userId, "replies")
        .SetQueryParams(
          [
            if fields.Length > 0 then
              let values = fields |> List.map ReplyField.asString
              "fields", String.Join(",", values)
            "access_token", accessToken
          ]
        )
        .GetAsync()
      |> Async.AwaitTask

    let! res = req.GetStringAsync() |> Async.AwaitTask

    return Decode.fromString ReplyResponse.Decode res
  }

  let manageReply (baseUrl: string) accessToken (replyId: string) shouldHide = async {
    let! req =

      baseUrl
        .AppendPathSegments(replyId, "manage_reply")
        .SetQueryParams(
          [
            "hide", (if shouldHide then "true" else "false")
            "access_token", accessToken
          ]
        )
        .PostAsync()
      |> Async.AwaitTask

    let! res = req.GetJsonAsync<Map<string, obj>>() |> Async.AwaitTask

    return
      res
      |> Map.tryFind "success"
      |> Option.map(fun v -> (unbox<string> v) = "true")
      |> Option.defaultValue false

  }
