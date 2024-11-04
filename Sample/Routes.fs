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

let getRoutes(profile: ProfileStore, userThreads: UserThreadsStore) = [
  Route.define("profile", "/profile", Profile.page(profile, userThreads))
]
