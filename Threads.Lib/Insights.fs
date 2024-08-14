namespace Threads.Lib

open System

open FsToolkit.ErrorHandling
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

  type DemographicBreakdown =
    | Country of string
    | City of string
    | Age of uint
    | Gender of string

  [<Struct>]
  type Metric =
    | Views
    | Likes
    | Replies
    | Reposts
    | Quotes
    | FollowerCount
    | FollowerDemographics

  module Metric =
    let asString =
      function
      | Views -> "views"
      | Likes -> "likes"
      | Replies -> "replies"
      | Reposts -> "reposts"
      | Quotes -> "quotes"
      | FollowerCount -> "follower_count"
      | FollowerDemographics -> "follower_demographics"

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

  type MediaMetric =
    | Name of Metric
    | Period of Period
    | Values of MetricValue array
    | Title of string
    | Description of string
    | Id of string
    | TotalValue of uint


  module MediaMetric =

    let decodeTotalValue: Decoder<uint> =
      Decode.object(fun get -> get.Required.Field "total_value" Decode.uint32)

    let Decode: Decoder<MediaMetric array> =
      Decode.object(fun get -> [|
        get.Required.Field "name" Metric.Decode |> Name
        get.Required.Field "period" Period.Decode |> Period
        get.Required.Field "title" Decode.string |> Title
        get.Required.Field "description" Decode.string |> Description
        get.Required.Field "id" Decode.string |> Id
        match
          get.Optional.Field "values" (Decode.array MetricValue.Decode)
        with
        | Some v -> Values v
        | None -> ()

        match get.Optional.Field "total_value" decodeTotalValue with
        | Some v -> TotalValue v
        | None -> ()
      |])

  type MetricResponse = { data: MediaMetric array array }

  module MediaMetricResponse =

    let Decode: Decoder<MetricResponse> =
      Decode.object(fun get -> {
        data = get.Required.Field "data" (Decode.array MediaMetric.Decode)
      })

  [<Struct>]
  type InsightParam =
    | Since of since: DateTimeOffset
    | Until of until: DateTimeOffset
    | Breakdown of demographicBreakdown: DemographicBreakdown

  let getMediaInsights
    (baseHttp: HeaderContext)
    accessToken
    mediaId
    (metrics: Metric array)
    =
    async {
      let! req =
        baseHttp {
          GET $"%s{mediaId}/threads_insights"

          query [
            if metrics.Length > 0 then
              "metric", String.Join(",", Array.map Metric.asString metrics)
            "access_token", accessToken
          ]
        }
        |> Request.sendAsync

      let! res = Response.toTextAsync req

      return Decode.fromString MediaMetricResponse.Decode res
    }

  type InsightError =
    | DateTooEarly
    | SerializationError of string
    | FollowerDemographicsMustIncludeBreakdown

  let getUserInsights
    (baseHttp: HeaderContext)
    accessToken
    userId
    (metrics: Metric array)
    insightParams
    =
    asyncResult {
      let insightParams = Array.ofSeq insightParams

      let extractSince insightParams =
        insightParams
        |> Array.tryPick (function
          | Since since -> Some since
          | _ -> None)

      let extractUntil insightParams =
        insightParams
        |> Array.tryPick (function
          | Until until -> Some until
          | _ -> None)

      let extractBreakdown insightParams =
        insightParams
        |> Array.tryPick (function
          | Breakdown breakdown -> Some breakdown
          | _ -> None)

      let extractFollowerDemographics metrics =
        metrics
        |> Array.tryPick (function
          | FollowerDemographics -> Some FollowerDemographics
          | _ -> None)

      let mustBeHigherThan(currentDate: DateTimeOffset) =
        if currentDate.ToUnixTimeSeconds() < 1712991600 then
          None
        else
          Some()

      let since = extractSince insightParams
      let until = extractUntil insightParams

      match since with
      | Some since ->
        do! mustBeHigherThan since |> Result.requireSome DateTooEarly
      | None -> ()

      match until with
      | Some until ->
        do! mustBeHigherThan until |> Result.requireSome DateTooEarly
      | None -> ()

      match extractFollowerDemographics metrics with
      | Some FollowerDemographics ->
        do!
          extractBreakdown insightParams
          |> Result.requireSome FollowerDemographicsMustIncludeBreakdown
          |> Result.ignore
      | _ -> ()

      let! req =
        baseHttp {
          GET $"%s{userId}/threads_insights"

          query [
            if metrics.Length > 0 then
              "metric", String.Join(",", Array.map Metric.asString metrics)
            yield!
              insightParams
              |> Array.map (function
                | Since since -> "since", $"%i{since.ToUnixTimeSeconds()}"
                | Until until -> "until", $"%i{until.ToUnixTimeSeconds()}"
                | Breakdown(Country value) -> "breakdown", value
                | Breakdown(City value) -> "breakdown", value
                | Breakdown(Age value) -> "breakdown", $"%i{value}"
                | Breakdown(Gender value) -> "breakdown", value)
            "access_token", accessToken
          ]
        }
        |> Request.sendAsync

      let! res = Response.toTextAsync req

      return!
        Decode.fromString MediaMetricResponse.Decode res
        |> Result.mapError SerializationError
    }
