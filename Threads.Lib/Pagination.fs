namespace Threads.Lib

open System
open Thoth.Json.Net

type Cursor = { before: string; after: string }

module Cursor =
  let Decode: Decoder<Cursor> =
    Decode.object(fun get -> {
      before = get.Required.Field "before" Decode.string
      after = get.Required.Field "after" Decode.string
    })

type Pagination = {
  cursors: Cursor
  next: string option
  previous: string option
}

module Pagination =
  let Decode: Decoder<Pagination> =
    Decode.object(fun get -> {
      cursors = get.Required.Field "cursors" Cursor.Decode
      next = get.Optional.Field "next" Decode.string
      previous = get.Optional.Field "previous" Decode.string
    })


type CursorParam =
  | Before of string
  | After of string
  | Limit of uint

type TimeParam =
  | Until of DateTimeOffset
  | Since of DateTimeOffset
  | Limit of uint

type OffsetParam =
  | Offset of uint
  | Limit of uint

type PaginationKind =
  | Cursor of CursorParam seq
  | Time of TimeParam seq
  | Offset of OffsetParam seq

module PaginationKind =
  let toStringTuple =
    function
    | Cursor cursor ->
      cursor
      |> set
      |> Set.fold
        (fun current next ->
          match next with
          | Before value -> "before", value
          | After value -> "after", value
          | CursorParam.Limit limit -> "limit", $"%i{limit}"
          :: current)
        []
    | Time time ->
      time
      |> set
      |> Set.fold
        (fun current next ->
          match next with
          | Until value -> "until", $"%i{value.ToUnixTimeMilliseconds()}"
          | Since value -> "since", $"%i{value.ToUnixTimeMilliseconds()}"
          | TimeParam.Limit limit -> "limit", $"%i{limit}"
          :: current)
        []
    | Offset offset ->
      offset
      |> set
      |> Set.fold
        (fun current next ->
          match next with
          | OffsetParam.Offset value -> "offset", $"%i{value}"
          | OffsetParam.Limit limit -> "limit", $"%i{limit}"
          :: current)
        []
