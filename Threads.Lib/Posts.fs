namespace Threads.Lib

open System
open Flurl
open Flurl.Http
open FsToolkit.ErrorHandling


module Posts =

  [<Struct>]
  type MediaType =
    | Text
    | Image
    | Video
    | Carousel

  module MediaType =
    let asString =
      function
      | Text -> "TEXT"
      | Image -> "IMAGE"
      | Video -> "VIDEO"
      | Carousel -> "CAROUSEL"

  [<Struct>]
  type ReplyAudience =
    | Everyone
    | AccountsYouFollow
    | MentionedOnly

  module ReplyAudience =
    let asString =
      function
      | Everyone -> "everyone"
      | AccountsYouFollow -> "accounts_you_follow"
      | MentionedOnly -> "mentioned_only"

  type PostParam =
    | CarouselItem
    | ImageUrl of Uri
    | MediaType of MediaType
    | VideoUrl of Uri
    | Text of string
    | ReplyTo of string
    | ReplyControl of ReplyAudience

  module PostParam =
    let toStringTuple =
      function
      | CarouselItem -> "is_carousel_item", "true"
      | ImageUrl url -> "image_url", url.ToString()
      | MediaType media -> "media_type", MediaType.asString media
      | VideoUrl url -> "video_url", url.ToString()
      | Text text -> "text", text
      | ReplyTo value -> "reply_to", value
      | ReplyControl value -> "reply_control", ReplyAudience.asString value

    let extractCarousel values =
      values
      |> Seq.tryPick(fun v ->
        match v with
        | CarouselItem -> Some()
        | _ -> None)

    let extractMediaType values =
      values
      |> Seq.tryPick(fun v ->
        match v with
        | MediaType media -> Some media
        | _ -> None)

    let extractImageUrl values =
      values
      |> Seq.tryPick(fun v ->
        match v with
        | ImageUrl url -> Some url
        | _ -> None)

    let extractVideoUrl values =
      values
      |> Seq.tryPick(fun v ->
        match v with
        | VideoUrl url -> Some url
        | _ -> None)

    let extractText values =
      values
      |> Seq.tryPick(fun v ->
        match v with
        | Text url -> Some url
        | _ -> None)

  [<Struct>]
  type SingleContainerError =
    | IsCarouselInSingleContainer
    | IsImageButImageNotProvided
    | IsVideoButNoVideoProvided
    | IsTextButNoTextProvided

  let createSingleContainer
    (baseUrl: string)
    accessToken
    (userId: string)
    postParams
    =
    asyncResult {

      let mediaType = PostParam.extractMediaType postParams

      match mediaType with
      | Some Carousel -> do! Error IsCarouselInSingleContainer
      | Some Image ->
        do!
          postParams
          |> PostParam.extractImageUrl
          |> Result.requireSome IsImageButImageNotProvided
          |> Result.ignore
      | Some Video ->
        do!
          postParams
          |> PostParam.extractVideoUrl
          |> Result.requireSome IsVideoButNoVideoProvided
          |> Result.ignore
      | Some MediaType.Text
      | None ->
        do!
          postParams
          |> PostParam.extractText
          |> Result.requireSome IsTextButNoTextProvided
          |> Result.ignore

      let postParams = postParams |> Seq.map PostParam.toStringTuple

      let! response =
        baseUrl
          .AppendPathSegments(userId, "threads")
          .SetQueryParams(
            [
              yield! postParams
              "is_carousel_item", "false"
              "access_token", accessToken
            ]
          )
          .PostAsync()

      return! response.GetJsonAsync<IdLike>()
    }

  [<Struct>]
  type CarouselItemContainerError =
    | MediaTypeMustbeVideoOrImage
    | IsImageButImageNotProvided
    | IsVideoButNoVideoProvided

  let createCarouselItemContainer
    (baseUrl: string)
    accessToken
    (userId: string)
    postParams
    =
    asyncResult {

      let mediaType = PostParam.extractMediaType postParams

      match mediaType with
      | Some Carousel
      | Some MediaType.Text
      | None -> do! Error MediaTypeMustbeVideoOrImage
      | Some Image ->
        do!
          postParams
          |> PostParam.extractImageUrl
          |> Result.requireSome IsImageButImageNotProvided
          |> Result.ignore
      | Some Video ->
        do!
          postParams
          |> PostParam.extractVideoUrl
          |> Result.requireSome IsVideoButNoVideoProvided
          |> Result.ignore

      let postParams = postParams |> Seq.map PostParam.toStringTuple

      let! response =
        baseUrl
          .AppendPathSegments(userId, "threads")
          .SetQueryParams(
            [
              yield! postParams
              "is_carousel_item", "true"
              "access_token", accessToken
            ]
          )
          .PostAsync()

      return! response.GetJsonAsync<IdLike>()
    }

  [<Struct>]
  type CarouselContainerError =
    | ChildLimitExceeded
    | CarouselPostIsEmpty

  let createCarouselContainer
    (baseUrl: string)
    accessToken
    (userId: string)
    children
    textContent
    =
    asyncResult {
      let children = children |> Seq.map _.id |> Seq.toList

      do! children.Length = 0 |> Result.requireFalse CarouselPostIsEmpty
      do! children.Length > 20 |> Result.requireFalse ChildLimitExceeded
      let children = String.Join(",", children)

      let! response =
        baseUrl
          .AppendPathSegments(userId, "threads")
          .SetQueryParams(
            [
              "media_type", "CAROUSEL"
              "children", children

              match textContent with
              | Some text -> "text", text
              | None -> ()

              "access_token", accessToken
            ]
          )
          .PostAsync()


      return! response.GetJsonAsync<IdLike>()
    }

  let publishContainer
    (baseUrl: string)
    accessToken
    (userId: string)
    containerId
    =
    async {
      let! result =
        baseUrl
          .AppendPathSegments(userId, "threads_publish")
          .SetQueryParams(
            [ "creation_id", containerId.id; "access_token", accessToken ]
          )
          .PostAsync()
        |> Async.AwaitTask


      return! result.GetJsonAsync<IdLike>() |> Async.AwaitTask
    }


  exception SingleContainerArgumentException of SingleContainerError
  exception CarouselItemContainerArgumentException of CarouselItemContainerError
  exception CarouselContainerArgumentException of CarouselContainerError
