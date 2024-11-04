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

let getRoutes
  (threads: ThreadsClient, profile: ProfileStore, userThreads: UserThreads)
  =
  [
    Route.define(
      "profile",
      "/profile",
      Profile.page(threads, profile, userThreads)
    )
  ]
