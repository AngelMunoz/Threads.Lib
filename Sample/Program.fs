open System

open Avalonia
open Avalonia.Controls
open Avalonia.Data

open NXUI.Desktop
open NXUI.FSharp.Extensions

open Navs
open Navs.Avalonia
open Threads.Lib.API

let navigate url (router: IRouter<Control>) _ _ =
  async {
    let! result = router.Navigate(url) |> Async.AwaitTask

    match result with
    | Ok _ -> ()
    | Error e -> printfn $"%A{e}"
  }
  |> Async.StartImmediate

let app accessToken () =

  let threads = ThreadClient.Create(accessToken)

  let router =
    AvaloniaRouter(
      Samples.Routes.getRoutes threads,
      splash = (fun _ -> TextBlock().text("Loading..."))
    )


  Window()
    .content(
      DockPanel()
        .lastChildFill(true)
        .children(
          StackPanel()
            .DockTop()
            .OrientationHorizontal()
            .spacing(8)
            .children(
              Button()
                .content("My Profile")
                .OnClickHandler(navigate "/profile" router),
              Button()
                .content("My Threads")
                .OnClickHandler(navigate $"/threads" router)
            ),
          RouterOutlet().router(router)
        )
    )

let accessToken =
  // we'd usually do the authentication dance
  // but for sample purposes we can use a dev access token provided in the
  // meda apps dashboard
  Environment.GetEnvironmentVariable("THREADS_ACCESS_TOKEN")
  |> Option.ofObj
  |> Option.defaultValue ""

NXUI.Run(app accessToken, "Navs.Avalonia!", Environment.GetCommandLineArgs())
|> ignore
