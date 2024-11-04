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
  Route.define("profile", "/profile", Sample.Profile.page threads)
]
