namespace Threads.Lib

open System
open Threads.Lib.Common

module Posts =
  [<Struct>]
  type MediaType =
    | Text
    | Image
    | Video
    | Carousel

  [<Struct>]
  type ReplyAudience =
    | Everyone
    | AccountsYouFollow
    | MentionedOnly

  type PostParam =
    | CarouselItem
    | ImageUrl of Uri
    | MediaType of MediaType
    | VideoUrl of Uri
    | Text of string
    | ReplyTo of string
    | ReplyControl of ReplyAudience

  [<Struct>]
  type SingleContainerError =
    | IsCarouselInSingleContainer
    | IsImageButImageNotProvided
    | IsVideoButNoVideoProvided
    | IsTextButNoTextProvided

  val internal createSingleContainer:
    baseUrl: string ->
    accessToken: string ->
    userId: string ->
    postParams: PostParam seq ->
      Async<Result<IdLike, SingleContainerError>>

  [<Struct>]
  type CarouselItemContainerError =
    | MediaTypeMustbeVideoOrImage
    | IsImageButImageNotProvided
    | IsVideoButNoVideoProvided

  val internal createCarouselItemContainer:
    baseUrl: string ->
    accessToken: string ->
    userId: string ->
    postParams: PostParam seq ->
      Async<Result<IdLike, CarouselItemContainerError>>

  [<Struct>]
  type CarouselContainerError =
    | MoreThan10Children
    | CarouselPostIsEmpty

  val internal createCarouselContainer:
    baseUrl: string ->
    accessToken: string ->
    userId: string ->
    children: IdLike seq ->
    textContent: string option ->
      Async<Result<IdLike, CarouselContainerError>>

  val internal publishContainer:
    baseUrl: string ->
    accessToken: string ->
    userId: string ->
    containerId: IdLike ->
      Async<IdLike>
