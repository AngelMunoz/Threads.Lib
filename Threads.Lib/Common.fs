namespace Threads.Lib.Common


open Thoth.Json.Net

[<Struct>]
type IdLike = { id: string }

module IdLike =
  let Decode: Decoder<IdLike> =
    Decode.object(fun get -> {
      id = get.Required.Field "id" Decode.string
    })

[<Struct>]
type MediaProductType = | Threads

module MediaProductType =
  let asString =
    function
    | Threads -> "THREADS"
