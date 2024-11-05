namespace Sample.Views


open System
open System.Diagnostics

open Avalonia.Controls.Templates
open Avalonia.Media
open IcedTasks
open IcedTasks.Polyfill.Async

open FSharp.Data.Adaptive

open Avalonia
open Avalonia.Controls
open Avalonia.Data

open NXUI.Desktop
open NXUI.FSharp.Extensions

open Navs
open Navs.Avalonia

open Threads.Lib

open Sample
open Sample.Services


module Metrics =

  let page (metricsStore: MetricsStore) ctx _ : Async<Control> = async {
    let! token = Async.CancellationToken

    Async.StartImmediate(metricsStore.loadMetrics(), token)

    metricsStore.metrics.AddCallback(fun value -> printfn $"Metrics: %A{value}")
    |> ignore

    return UserControl().content(TextBlock().text("Metrics"))
  }
