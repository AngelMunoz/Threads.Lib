namespace Threads.Lib

open System
open Thoth.Json.Net

[<Struct>]
type IdLike = { id: string }

module internal IdLike =

  val Decode: Decoder<IdLike>

[<Struct; RequireQualifiedAccess>]
type MediaProductType = | Threads

module internal MediaProductType =
  val asString: MediaProductType -> string


type Cursor = { before: string; after: string }

module internal Cursor =
  val Decode: Decoder<Cursor>

type Pagination = {
  cursors: Cursor
  next: string option
  previous: string option
}

module internal Pagination =
  val Decode: Decoder<Pagination>

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

module internal PaginationKind =
  val toStringTuple: PaginationKind -> (string * string) list
