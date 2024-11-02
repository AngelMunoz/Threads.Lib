namespace Threads.Lib

open System

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
    | Shares
    | FollowerCount
    | FollowerDemographics

  [<Struct>]
  type Period =
    | Lifetime
    | Day

  [<Struct>]
  type MetricValue = {
    value: uint
    endTime: DateTimeOffset voption
  }

  type MediaMetric =
    | Name of Metric
    | Period of Period
    | Values of MetricValue array
    | Title of string
    | Description of string
    | Id of string
    | TotalValue of uint

  type MetricResponse = { data: MediaMetric array array }

  [<Struct>]
  type InsightParam =
    | Since of since: DateTimeOffset
    | Until of until: DateTimeOffset
    | Breakdown of demographicBreakdown: DemographicBreakdown

  val internal getMediaInsights:
    baseUrl: string ->
    accessToken: string ->
    mediaId: string ->
    metrics: Metric array ->
      Async<Result<MetricResponse, string>>

  type InsightError =
    | DateTooEarly
    | SerializationError of string
    | FollowerDemographicsMustIncludeBreakdown

  val internal getUserInsights:
    baseUrl: string ->
    accessToken: string ->
    userId: string ->
    metrics: Metric array ->
    insightParams: InsightParam seq ->
      Async<Result<MetricResponse, InsightError>>
