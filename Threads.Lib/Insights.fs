namespace Threads.Lib

open System

open Thoth.Json.Net
open FsHttp

module Decode =
  let timestamp: Decoder<DateTimeOffset> =
    fun path value ->
      if Decode.Helpers.isNumber value then
        let value: int64 = unbox value
        let datetime = DateTimeOffset.FromUnixTimeSeconds(value)
        Ok datetime
      else
        (path, BadPrimitive("a timestamp", value)) |> Error


module Insights =

  [<Struct>]
  type Metric =
    | Views
    | Likes
    | Replies
    | Reposts
    | Quotes

  module Metric =
    let asString =
      function
      | Views -> "views"
      | Likes -> "likes"
      | Replies -> "replies"
      | Reposts -> "reposts"
      | Quotes -> "quotes"

    let Decode: Decoder<Metric> =
      fun path jsonValue ->
        match unbox<string> jsonValue with
        | "views" -> Ok Views
        | "likes" -> Ok Likes
        | "replies" -> Ok Replies
        | "reposts" -> Ok Reposts
        | "quotes" -> Ok Quotes
        | _ -> (path, BadPrimitive("a valid metric", jsonValue)) |> Error

  [<Struct>]
  type Period =
    | Lifetime
    | Day

  module Period =
    let Decode: Decoder<Period> =
      fun path jsonValue ->
        match unbox<string> jsonValue with
        | "lifetime" -> Ok Lifetime
        | "day" -> Ok Day
        | _ -> (path, BadPrimitive("a valid period", jsonValue)) |> Error

  [<Struct>]
  type MetricValue = {
    value: uint
    endTime: DateTimeOffset voption
  }

  module MetricValue =
    let Decode: Decoder<MetricValue> =
      Decode.object(fun get -> {
        value = get.Required.Field "value" Decode.uint32
        endTime =
          match get.Optional.Field "end_time" Decode.timestamp with
          | Some v -> ValueSome v
          | None -> ValueNone
      })

  type MediaMetric = {
    name: Metric
    period: Period
    values: MetricValue array
    title: string
    description: string
    id: string
  }

  module MediaMetric =
    let Decode: Decoder<MediaMetric> =
      Decode.object(fun get -> {
        id = get.Required.Field "id" Decode.string
        description = get.Required.Field "id" Decode.string
        title = get.Required.Field "id" Decode.string
        values = get.Required.Field "values" (Decode.array MetricValue.Decode)
        period = get.Required.Field "period" Period.Decode
        name = get.Required.Field "name" Metric.Decode
      })

  type MetricResponse = { data: MediaMetric array }

  module MediaMetricResponse =

    let Decode: Decoder<MetricResponse> =
      Decode.object(fun get -> {
        data = get.Required.Field "data" (Decode.array MediaMetric.Decode)
      })
