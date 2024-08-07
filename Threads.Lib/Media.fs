namespace Threads.Lib

open System

open Thoth.Json.Net
open FsHttp


module Media =

  [<Struct>]
  type MediaProductType = Threads

  [<Struct>]
  type MediaType =
    | TextPost
    | Image
    | Video
    | CarouselAlbum
    | Audio
    | RepostFacade

  [<Struct>]
  type Owner = { id: string }

  module Owner =
    let Decode: Decoder<Owner> =
      Decode.object(fun get -> {
        id = get.Required.Field "id" Decode.string
      })

  [<Struct>]
  type ThreadId = { id: string }

  module ThreadId =
    let Decode: Decoder<ThreadId> =
      Decode.object(fun get -> {
        id = get.Required.Field "id" Decode.string
      })

  type ThreadChildren = { data: ThreadId seq }

  module ThreadChildren =
    let Decode: Decoder<ThreadChildren> =
      Decode.object(fun get -> {
        data = get.Required.Field "data" (Decode.list ThreadId.Decode)
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

  type ThreadValue =
    | Id of string
    | MediaProductType of MediaProductType
    | MediaType of MediaType
    | MediaUrl of Uri
    | Permalink of Uri
    | Owner of Owner
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
      | Some "TEXT_POST" -> MediaType TextPost |> fields.Add
      | Some "IMAGE" -> MediaType Image |> fields.Add
      | Some "VIDEO" -> MediaType Video |> fields.Add
      | Some "CAROUSEL_ALBUM" -> MediaType CarouselAlbum |> fields.Add
      | Some "AUDIO" -> MediaType Audio |> fields.Add
      | Some "REPOST_FACADE" -> MediaType RepostFacade |> fields.Add
      | Some value -> () // new value added?
      | None -> ()

      get, fields

    let decodeMediaProductType(get: Decode.IGetters, fields: _ ResizeArray) =
      match get.Optional.Field "media_product_type" Decode.string with
      | Some "THREADS" -> MediaProductType Threads |> fields.Add
      | Some value -> () // new value added?
      | None -> ()

      get, fields

    let decodeMediaUrl(get: Decode.IGetters, fields: _ ResizeArray) =
      match get.Optional.Field "media_url" Decode.string with
      | Some value -> Uri value |> MediaUrl |> fields.Add
      | None -> ()

      get, fields

    let decodePermaLink(get: Decode.IGetters, fields: _ ResizeArray) =
      match get.Optional.Field "permalink" Decode.string with
      | Some value -> Uri value |> Permalink |> fields.Add
      | None -> ()

      get, fields

    let decodeOwner(get: Decode.IGetters, fields: _ ResizeArray) =
      match get.Optional.Field "owner" Owner.Decode with
      | Some owner -> Owner owner |> fields.Add
      | None -> ()

      get, fields

    let decodeUsername(get: Decode.IGetters, fields: _ ResizeArray) =
      match get.Optional.Field "username" Decode.string with
      | Some username -> Username username |> fields.Add
      | None -> ()

      get, fields

    let decodeText(get: Decode.IGetters, fields: _ ResizeArray) =
      match get.Optional.Field "text" Decode.string with
      | Some text -> Text text |> fields.Add
      | None -> ()

      get, fields

    let decodeTimestamp(get: Decode.IGetters, fields: _ ResizeArray) =

      match get.Optional.Field "timestamp" Decode.datetimeOffset with
      | Some timestamp -> Timestamp timestamp |> fields.Add
      | None -> ()

      get, fields

    let decodeShortcode(get: Decode.IGetters, fields: _ ResizeArray) =

      match get.Optional.Field "shortcode" Decode.string with
      | Some shortcode -> ShortCode shortcode |> fields.Add
      | None -> ()

      get, fields

    let decodeThumbnailUrl(get: Decode.IGetters, fields: _ ResizeArray) =

      match get.Optional.Field "thumbnail_url" Decode.string with
      | Some value -> Uri value |> ThumbnailUrl |> fields.Add
      | None -> ()

      get, fields

    let decodeChildren(get: Decode.IGetters, fields: _ ResizeArray) =

      match get.Optional.Field "children" ThreadChildren.Decode with
      | Some value -> value |> Children |> fields.Add
      | None -> ()

      get, fields

    let decodeIsQuotePost(get: Decode.IGetters, fields: _ ResizeArray) =

      match get.Optional.Field "is_quote_post" Decode.bool with
      | Some value -> value |> IsQuotePost |> fields.Add
      | None -> ()

      get, fields

    let inline finish(get: Decode.IGetters, fields: _ seq) = fields


    let Decode: Decoder<ThreadValue seq> =
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
    data: ThreadValue seq seq
    paging: Pagination
  }

  module ThreadListResponse =
    let Decode: Decoder<ThreadListResponse> =
      Decode.object(fun get -> {
        data = get.Required.Field "data" (Decode.list(ThreadValue.Decode))
        paging = get.Required.Field "paging" Pagination.Decode
      })

  let getThreads baseUrl accessToken pagination threadFields cancellationToken = task {
    let fields =
      threadFields
      |> Seq.map ThreadField.asString
      |> (fun f -> String.Join(",", f))

    let! req =
      http {
        GET $"{baseUrl}/threads"

        query [
          if String.IsNullOrEmpty fields then () else "field", fields
          match pagination with
          | Some pagination -> yield! PaginationKind.toStringTuple pagination
          | None -> ()
          "access_token", accessToken
        ]
      }
      |> Request.sendTAsync

    let! res = Response.toTextTAsync cancellationToken req

    return Decode.fromString ThreadListResponse.Decode res
  }

  let getThread baseUrl accessToken threadId threadFields cancellationToken = task {
    let fields =
      threadFields
      |> Seq.map ThreadField.asString
      |> (fun f -> String.Join(",", f))

    let! req =
      http {
        GET $"{baseUrl}/threads/{threadId}"

        query [
          if String.IsNullOrEmpty fields then () else "field", fields
          "access_token", accessToken
        ]
      }
      |> Request.sendTAsync

    let! res = Response.toTextTAsync cancellationToken req

    return Decode.fromString ThreadValue.Decode res
  }
