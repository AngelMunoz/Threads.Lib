namespace Threads.Lib.Common

open Thoth.Json.Net

[<Struct>]
type IdLike = { id: string }

module internal IdLike =

  val Decode: Decoder<IdLike>

[<Struct>]
type MediaProductType = | Threads

module internal MediaProductType =
  val asString: MediaProductType -> string
