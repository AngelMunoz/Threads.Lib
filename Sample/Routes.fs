module Sample.Routes

open Avalonia
open Avalonia.Controls
open Avalonia.Data

open NXUI.Desktop
open NXUI.FSharp.Extensions

open FSharp.Data.Adaptive

open Navs
open Navs.Avalonia

open Threads.Lib

open Sample.Services
open Sample.Views

let getRoutes
  (
    profile: ProfileStore,
    userThreads: UserThreadsStore,
    metricsStore: MetricsStore
  ) =
  [
    Route.define("profile", "/profile", Profile.page(profile, userThreads))
    Route.define("metrics", "/metrics", Metrics.page(metricsStore))
  ]
