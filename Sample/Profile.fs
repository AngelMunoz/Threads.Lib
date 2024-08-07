namespace Sample

open Avalonia
open Avalonia.Controls
open Avalonia.Data

open NXUI.Desktop
open NXUI.FSharp.Extensions

open Navs
open Navs.Avalonia

open Threads.Lib

module Profile =

  let getMe baseUrl accessToken =
    Profiles.getProfile baseUrl accessToken None

  let route =
    Route.define(
      "profile",
      "/me",
      fun ctx _ -> async {

        return UserControl()
      }
    )
