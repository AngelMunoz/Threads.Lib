namespace Threads.Lib

open System
open FsHttp
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


  type PostParam =
    | CarouselItem
    | ImageUrl of Uri
    | MediaType of MediaType
    | VideoUrl of Uri
    | Text of string

  module PostParam =
    let toStringTuple =
      function
      | CarouselItem -> "is_carousel_item", "true"
      | ImageUrl url -> "image_url", url.ToString()
      | MediaType media -> "media_type", MediaType.asString media
      | VideoUrl url -> "image_url", url.ToString()
      | Text text -> "text", text

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
  type PostId = { id: string }


  [<Struct>]
  type SingleContainerError =
    | IsCarouselInSingleContainer
    | IsImageButImageNotProvided
    | IsVideoButNoVideoProvided
    | IsTextButNoTextProvided


  let createSingleContainer
    baseUrl
    accessToken
    userId
    postParams
    cancellationToken
    =
    taskResult {
      let postParams =
        postParams
        |> Seq.filter(fun f ->
          match f with
          | CarouselItem -> false
          | _ -> true)
        |> Seq.toList

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

      let postParams = postParams |> List.map PostParam.toStringTuple

      let! req =
        http {
          POST $"{baseUrl}/{userId}/threads"

          query [
            yield! postParams
            "is_carousel_item", "false"
            "access_token", accessToken
          ]
        }
        |> Request.sendTAsync

      let! (response: PostId) =
        req |> Response.deserializeJsonTAsync cancellationToken

      return response
    }

  [<Struct>]
  type CarouselItemContainerError =
    | MediaTypeMustbeVideoOrImage
    | IsImageButImageNotProvided
    | IsVideoButNoVideoProvided

  let createCarouselItemContainer
    baseUrl
    accessToken
    userId
    postParams
    cancellationToken
    =
    taskResult {
      let postParams =
        postParams
        |> Seq.filter(fun f ->
          match f with
          | CarouselItem -> false
          | _ -> true)
        |> Seq.toList

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

      let postParams = postParams |> List.map PostParam.toStringTuple

      let! req =
        http {
          POST $"{baseUrl}/{userId}/threads"

          query [
            yield! postParams
            "is_carousel_item", "true"
            "access_token", accessToken
          ]
        }
        |> Request.sendTAsync

      let! (response: PostId) =
        req |> Response.deserializeJsonTAsync cancellationToken

      return response
    }

  [<Struct>]
  type CarouselContainerError =
    | MoreThan10Children
    | CarouselPostIsEmpty

  let createCarouselContainer
    baseUrl
    accessToken
    userId
    children
    textContent
    cancellationToken
    =
    taskResult {
      let children = children |> Seq.map _.id |> Seq.toList

      do! children.Length = 0 |> Result.requireFalse CarouselPostIsEmpty
      do! children.Length > 10 |> Result.requireFalse MoreThan10Children
      let children = String.Join(",", children)

      let! req =
        http {
          POST $"{baseUrl}/{userId}/threads"

          query [
            "media_type", "CAROUSEL"
            "children", children
            match textContent with
            | Some text -> "text", text
            | None -> ()
            "access_token", accessToken
          ]
        }
        |> Request.sendTAsync

      let! (response: PostId) =
        req |> Response.deserializeJsonTAsync cancellationToken

      return response
    }


  let publishContainer
    baseUrl
    accessToken
    userId
    containerId
    cancellationToken
    =
    task {
      let! req =
        http {
          POST $"{baseUrl}/{userId}/threads_publish"

          query [ "creation_id", containerId.id; "access_token", accessToken ]
        }
        |> Request.sendTAsync

      let! (response: PostId) =
        req |> Response.deserializeJsonTAsync cancellationToken

      return response
    }
