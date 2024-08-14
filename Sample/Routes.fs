module Samples.Routes

open Avalonia
open Avalonia.Controls
open Avalonia.Data

open NXUI.Desktop
open NXUI.FSharp.Extensions

open Navs
open Navs.Avalonia

open Threads.Lib

let getRoutes(threads: ThreadsClient) = [
  Route.define(
    "guid",
    "/:id<guid>",
    fun context _ -> async {
      return
        match context.urlMatch.Params.TryGetValue "id" with
        | true, id -> TextBlock().text($"%O{id}")
        | false, _ -> TextBlock().text("Guid No GUID")
    }
  )
  Route.define("profile", "/profile", Sample.Profile.page threads)
]
