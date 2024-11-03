namespace Threads.Lib

open System.Collections.Generic
open Thoth.Json.Net
open Flurl
open Flurl.Http


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
        quotaDuration = get.Required.Field "quota_duration" Decode.int64
      })

  [<Struct; RequireQualifiedAccess>]
  type RateLimitFieldValue =
    | ReplyQuotaUsage of rqu: uint
    | ReplyConfig of rc: ReplyConfig

  module RateLimitFieldValue =
    let Decode: Decoder<RateLimitFieldValue list> =
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

  type RateLimitResponse = { data: RateLimitFieldValue list list }

  module RateLimitResponse =
    let Decode: Decoder<RateLimitResponse> =
      Decode.object(fun get -> {
        data =
          get.Required.Field "data" (Decode.list RateLimitFieldValue.Decode)
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
      | ThumbnailUrl -> "thumbnail_url"
      | Children -> "children"
      | IsQuotePost -> "is_quote_post"
      | HasReplies -> "has_replies"
      | RootPost -> "root_post"
      | RepliedTo -> "replied_to"
      | IsReply -> "is_reply"
      | IsReplyOwnedByMe -> "is_reply_owned_by_me"
      | HideStatus -> "hide_status"
      | ReplyAudience -> "reply_audience"

  [<RequireQualifiedAccess>]
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
      get.Required.Field "id" Decode.string |> ReplyFieldValue.Id |> values.Add

      get, values

    let decodeText(get: Decode.IGetters, values: ReplyFieldValue ResizeArray) =
      get.Optional.Field "text" Decode.string
      |> Option.map(ReplyFieldValue.Text >> values.Add)
      |> Option.defaultValue()

      get, values

    let decodeUsername
      (get: Decode.IGetters, values: ReplyFieldValue ResizeArray)
      =
      get.Optional.Field "username" Decode.string
      |> Option.map(ReplyFieldValue.Username >> values.Add)
      |> Option.defaultValue()

      get, values

    let decodePermalink
      (get: Decode.IGetters, values: ReplyFieldValue ResizeArray)
      =
      get.Optional.Field "permalink" Decode.string
      |> Option.map(Uri >> ReplyFieldValue.Permalink >> values.Add)
      |> Option.defaultValue()

      get, values

    let decodeTimestamp
      (get: Decode.IGetters, values: ReplyFieldValue ResizeArray)
      =
      get.Optional.Field "timestamp" Decode.datetimeOffset
      |> Option.map(ReplyFieldValue.Timestamp >> values.Add)
      |> Option.defaultValue()

      get, values

    let decodeMediaProductType
      (get: Decode.IGetters, values: ReplyFieldValue ResizeArray)
      =
      get.Optional.Field "media_product_type" Decode.string
      |> Option.map(fun value ->
        match value with
        | "THREADS" -> values.Add(ReplyFieldValue.MediaProductType Threads)
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
        | "TEXT_POST" -> values.Add(ReplyFieldValue.MediaType TextPost)
        | "IMAGE" -> values.Add(ReplyFieldValue.MediaType Image)
        | "VIDEO" -> values.Add(ReplyFieldValue.MediaType Video)
        | "CAROUSEL_ALBUM" ->
          values.Add(ReplyFieldValue.MediaType CarouselAlbum)
        | "AUDIO" -> values.Add(ReplyFieldValue.MediaType Audio)
        | "THREADS" -> values.Add(ReplyFieldValue.MediaProductType Threads)
        | other -> () // new value added?
      )
      |> Option.defaultValue()

      get, values

    let decodeMediaUrl
      (get: Decode.IGetters, values: ReplyFieldValue ResizeArray)
      =
      get.Optional.Field "media_url" Decode.string
      |> Option.map(Uri >> ReplyFieldValue.MediaUrl >> values.Add)
      |> Option.defaultValue()

      get, values

    let decodeShortcode
      (get: Decode.IGetters, values: ReplyFieldValue ResizeArray)
      =
      get.Optional.Field "shortcode" Decode.string
      |> Option.map(ReplyFieldValue.Shortcode >> values.Add)
      |> Option.defaultValue()

      get, values

    let decodeThumbnailUrl
      (get: Decode.IGetters, values: ReplyFieldValue ResizeArray)
      =
      get.Optional.Field "thumbnail_url" Decode.string
      |> Option.map(Uri >> ReplyFieldValue.ThumbnailUrl >> values.Add)
      |> Option.defaultValue()

      get, values

    let decodeChildren
      (get: Decode.IGetters, values: ReplyFieldValue ResizeArray)
      =
      get.Optional.Field "children" (Decode.array IdLike.Decode)
      |> Option.map(ReplyFieldValue.Children >> values.Add)
      |> Option.defaultValue()

      get, values


    let decodeIsQuotePost
      (get: Decode.IGetters, values: ReplyFieldValue ResizeArray)
      =
      get.Optional.Field "is_quote_post" Decode.bool
      |> Option.map(ReplyFieldValue.IsQuotePost >> values.Add)
      |> Option.defaultValue()

      get, values

    let decodeHasReplies
      (get: Decode.IGetters, values: ReplyFieldValue ResizeArray)
      =
      get.Optional.Field "has_replies" Decode.bool
      |> Option.map(ReplyFieldValue.HasReplies >> values.Add)
      |> Option.defaultValue()

      get, values

    let decodeRootPost
      (get: Decode.IGetters, values: ReplyFieldValue ResizeArray)
      =
      get.Optional.Field "root_post" IdLike.Decode
      |> Option.map(ReplyFieldValue.RootPost >> values.Add)
      |> Option.defaultValue()

      get, values

    let decodeRepliedTo
      (get: Decode.IGetters, values: ReplyFieldValue ResizeArray)
      =
      get.Optional.Field "replied_to" IdLike.Decode
      |> Option.map(ReplyFieldValue.RepliedTo >> values.Add)
      |> Option.defaultValue()

      get, values

    let decodeIsReply
      (get: Decode.IGetters, values: ReplyFieldValue ResizeArray)
      =
      get.Optional.Field "is_reply" Decode.bool
      |> Option.map(ReplyFieldValue.IsReply >> values.Add)
      |> Option.defaultValue()

      get, values

    let decodeIsReplyOwnedByMe
      (get: Decode.IGetters, values: ReplyFieldValue ResizeArray)
      =
      get.Optional.Field "is_reply_owned_by_me" Decode.bool
      |> Option.map(ReplyFieldValue.IsReplyOwnedByMe >> values.Add)
      |> Option.defaultValue()

      get, values

    let decodeHideStatus
      (get: Decode.IGetters, values: ReplyFieldValue ResizeArray)
      =
      get.Optional.Field "hide_status" Decode.string
      |> Option.map (function
        | "NOT_HUSHED" -> values.Add(ReplyFieldValue.HideStatus NotHushed)
        | "UNHUSHED" -> values.Add(ReplyFieldValue.HideStatus Unhushed)
        | "HIDDEN" -> values.Add(ReplyFieldValue.HideStatus Hidden)
        | "COVERED" -> values.Add(ReplyFieldValue.HideStatus Covered)
        | "BLOCKED" -> values.Add(ReplyFieldValue.HideStatus Blocked)
        | "RESTRICTED" -> values.Add(ReplyFieldValue.HideStatus Restricted)
        | other -> () // new value?
      )
      |> Option.defaultValue()

      get, values

    let decodeReplyAudience
      (get: Decode.IGetters, values: ReplyFieldValue ResizeArray)
      =
      get.Optional.Field "reply_audience" Decode.string
      |> Option.map (function
        | "EVERYONE" -> values.Add(ReplyFieldValue.ReplyAudience Everyone)
        | "ACCOUNTS_YOU_FOLLOW" ->
          values.Add(ReplyFieldValue.ReplyAudience AccountsYouFollow)
        | "MENTIONED_ONLY" ->
          values.Add(ReplyFieldValue.ReplyAudience MentionedOnly)
        | other -> () // new value?
      )
      |> Option.defaultValue()

      get, values

    let inline finish(_, values: ReplyFieldValue seq) = values |> Seq.toList

    let Decode: Decoder<ReplyFieldValue list> =
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
    data: ReplyFieldValue list list
    paging: Pagination
  }

  module ReplyResponse =
    let Decode: Decoder<ConversationResponse> =
      Decode.object(fun get -> {
        data = get.Required.Field "data" (Decode.list ReplyFieldValue.Decode)
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
    pagination
    reverse
    =
    async {
      let fields = fields |> Seq.toList

      let pagination = pagination |> Option.map PaginationKind.toStringTuple

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
              match pagination with
              | Some values -> yield! values
              | None -> ()
              "access_token", accessToken
            ]
          )
          .GetAsync()
        |> Async.AwaitTask

      let! res = req.GetStringAsync() |> Async.AwaitTask

      return Decode.fromString ReplyResponse.Decode res
    }

  let getConversation
    (baseUrl: string)
    accessToken
    (mediaId: string)
    fields
    pagination
    reverse
    =
    async {
      let fields = fields |> Seq.toList

      let pagination = pagination |> Option.map PaginationKind.toStringTuple

      let! req =

        baseUrl
          .AppendPathSegments(mediaId, "conversation")
          .SetQueryParams(
            [
              if fields.Length > 0 then
                let values = fields |> List.map ReplyField.asString
                "fields", String.Join(",", values)
              if reverse then
                "reverse", "true"
              match pagination with
              | Some values -> yield! values
              | None -> ()
              "access_token", accessToken
            ]
          )
          .GetAsync()
        |> Async.AwaitTask

      let! res = req.GetStringAsync() |> Async.AwaitTask

      return Decode.fromString ReplyResponse.Decode res
    }

  let getUserReplies
    (baseUrl: string)
    accessToken
    (userId: string)
    fields
    pagination
    =
    async {
      let fields = fields |> Seq.toList

      let pagination = pagination |> Option.map PaginationKind.toStringTuple

      let! req =

        baseUrl
          .AppendPathSegments(userId, "replies")
          .SetQueryParams(
            [
              if fields.Length > 0 then
                let values = fields |> List.map ReplyField.asString
                "fields", String.Join(",", values)

              match pagination with
              | Some values -> yield! values
              | None -> ()

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

    let! res = req.GetJsonAsync<Text.Json.JsonDocument>() |> Async.AwaitTask

    return
      match res.RootElement.TryGetProperty("success") with
      | true, value -> value.GetBoolean()
      | _ -> false
  }
