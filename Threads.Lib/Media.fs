namespace Threads.Lib

open System

open Thoth.Json.Net
open Flurl
open Flurl.Http

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

  module ThreadChildren =
    let Decode: Decoder<ThreadChildren> =
      Decode.object(fun get -> {
        data = get.Required.Field "data" (Decode.list IdLike.Decode)
      })

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

  module ThreadField =
    let asString =
      function
      | Id -> "id"
      | MediaProductType -> "media_product_type"
      | MediaType -> "media_type"
      | MediaUrl -> "media_url"
      | Permalink -> "permalink"
      | Owner -> "owner"
      | Username -> "username"
      | Text -> "text"
      | Timestamp -> "timestamp"
      | ShortCode -> "shortcode"
      | ThumbnailUrl -> "thumbnail_url"
      | Children -> "children"
      | IsQuotePost -> "is_quote_post"

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

  module ThreadValue =

    let decodeMediaType(get: Decode.IGetters, fields: _ ResizeArray) =
      match get.Optional.Field "media_type" Decode.string with
      | Some "TEXT_POST" -> ThreadValue.MediaType TextPost |> fields.Add
      | Some "IMAGE" -> ThreadValue.MediaType Image |> fields.Add
      | Some "VIDEO" -> ThreadValue.MediaType Video |> fields.Add
      | Some "CAROUSEL_ALBUM" ->
        ThreadValue.MediaType CarouselAlbum |> fields.Add
      | Some "AUDIO" -> ThreadValue.MediaType Audio |> fields.Add
      | Some "REPOST_FACADE" -> ThreadValue.MediaType RepostFacade |> fields.Add
      | Some value -> () // new value added?
      | None -> ()

      get, fields

    let decodeMediaProductType(get: Decode.IGetters, fields: _ ResizeArray) =
      match get.Optional.Field "media_product_type" Decode.string with
      | Some "THREADS" ->
        ThreadValue.MediaProductType MediaProductType.Threads |> fields.Add
      | Some value -> () // new value added?
      | None -> ()

      get, fields

    let decodeMediaUrl(get: Decode.IGetters, fields: _ ResizeArray) =
      match get.Optional.Field "media_url" Decode.string with
      | Some value -> Uri value |> ThreadValue.MediaUrl |> fields.Add
      | None -> ()

      get, fields

    let decodePermaLink(get: Decode.IGetters, fields: _ ResizeArray) =
      match get.Optional.Field "permalink" Decode.string with
      | Some value -> Uri value |> ThreadValue.Permalink |> fields.Add
      | None -> ()

      get, fields

    let decodeOwner(get: Decode.IGetters, fields: _ ResizeArray) =
      match get.Optional.Field "owner" IdLike.Decode with
      | Some owner -> ThreadValue.Owner owner |> fields.Add
      | None -> ()

      get, fields

    let decodeUsername(get: Decode.IGetters, fields: _ ResizeArray) =
      match get.Optional.Field "username" Decode.string with
      | Some username -> ThreadValue.Username username |> fields.Add
      | None -> ()

      get, fields

    let decodeText(get: Decode.IGetters, fields: _ ResizeArray) =
      match get.Optional.Field "text" Decode.string with
      | Some text -> ThreadValue.Text text |> fields.Add
      | None -> ()

      get, fields

    let decodeTimestamp(get: Decode.IGetters, fields: _ ResizeArray) =

      match get.Optional.Field "timestamp" Decode.datetimeOffset with
      | Some timestamp -> ThreadValue.Timestamp timestamp |> fields.Add
      | None -> ()

      get, fields

    let decodeShortcode(get: Decode.IGetters, fields: _ ResizeArray) =

      match get.Optional.Field "shortcode" Decode.string with
      | Some shortcode -> ThreadValue.ShortCode shortcode |> fields.Add
      | None -> ()

      get, fields

    let decodeThumbnailUrl(get: Decode.IGetters, fields: _ ResizeArray) =

      match get.Optional.Field "thumbnail_url" Decode.string with
      | Some value -> Uri value |> ThreadValue.ThumbnailUrl |> fields.Add
      | None -> ()

      get, fields

    let decodeChildren(get: Decode.IGetters, fields: _ ResizeArray) =

      match get.Optional.Field "children" ThreadChildren.Decode with
      | Some value -> value |> ThreadValue.Children |> fields.Add
      | None -> ()

      get, fields

    let decodeIsQuotePost(get: Decode.IGetters, fields: _ ResizeArray) =

      match get.Optional.Field "is_quote_post" Decode.bool with
      | Some value -> value |> ThreadValue.IsQuotePost |> fields.Add
      | None -> ()

      get, fields

    let inline finish(_: Decode.IGetters, fields: _ seq) = fields |> Seq.toList


    let Decode: Decoder<ThreadValue list> =
      Decode.object(fun get ->
        let fields = ResizeArray()
        fields.Add(ThreadValue.Id(get.Required.Field "id" Decode.string))

        (get, fields)
        |> decodeMediaType
        |> decodeMediaProductType
        |> decodeMediaUrl
        |> decodePermaLink
        |> decodeOwner
        |> decodeUsername
        |> decodeText
        |> decodeTimestamp
        |> decodeShortcode
        |> decodeThumbnailUrl
        |> decodeChildren
        |> decodeIsQuotePost
        |> finish)

  type ThreadListResponse = {
    data: ThreadValue list list
    paging: Pagination
  }

  module ThreadListResponse =
    let Decode: Decoder<ThreadListResponse> =
      Decode.object(fun get -> {
        data = get.Required.Field "data" (Decode.list(ThreadValue.Decode))
        paging = get.Required.Field "paging" Pagination.Decode
      })

  let getThreads
    (baseUrl: string)
    accessToken
    (profileId: string)
    pagination
    threadFields
    =
    async {
      let fields =
        threadFields
        |> Seq.map ThreadField.asString
        |> (fun f -> String.Join(",", f))

      let! response =
        baseUrl
          .AppendPathSegments(profileId, "threads")
          .SetQueryParams(
            [
              match pagination with
              | Some pagination ->
                yield! PaginationKind.toStringTuple pagination
              | None -> ()
              "fields", fields
              "access_token", accessToken
            ]
          )
          .GetAsync()
        |> Async.AwaitTask

      let! content = response.GetStringAsync() |> Async.AwaitTask

      return Decode.fromString ThreadListResponse.Decode content
    }

  let getThread (baseUrl: string) accessToken (threadId: string) threadFields = async {
    let fields =
      threadFields
      |> Seq.map ThreadField.asString
      |> (fun f -> String.Join(",", f))

    let! response =

      baseUrl
        .AppendPathSegments(threadId, "threads")
        .SetQueryParams([ "fields", fields; "access_token", accessToken ])
        .GetAsync()
      |> Async.AwaitTask

    let! res = response.GetStringAsync() |> Async.AwaitTask
    return Decode.fromString ThreadValue.Decode res
  }
