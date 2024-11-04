open System

open Avalonia
open Avalonia.Controls
open Avalonia.Data

open NXUI.Desktop
open NXUI.FSharp.Extensions

open FSharp.Data.Adaptive

open Navs
open Navs.Avalonia
open Threads.Lib
open Avalonia.Controls.Primitives

let navigate url (router: IRouter<Control>) _ _ =
  task {
    let! result = router.Navigate(url)

    match result with
    | Ok _ -> ()
    | Error e -> printfn $"%A{e}"
  }
  |> ignore

let app accessToken () =

  let threads = Threads.Create(accessToken)

  let router: IRouter<_> =
    AvaloniaRouter(
      Samples.Routes.getRoutes threads,
      splash = (fun _ -> TextBlock().text("Loading..."))
    )

  let showComposer = cval false
  let opacity = showComposer |> AVal.map(fun x -> if x then 1.0 else 0.0)
  let tr = Animation.Transitions()

  tr.Add(
    Animation.DoubleTransition(
      Property = Control.OpacityProperty,
      Duration = TimeSpan.FromMilliseconds(250),
      Easing = Animation.Easings.ElasticEaseInOut()
    )
  )

  let window =
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
                  .OnClickHandler(navigate $"/threads" router),
                ToggleButton()
                  .content("New Post")
                  .OnClickHandler(fun _ _ -> showComposer.setValue true)
              ),
            UserControl()
              .DockTop()
              .isVisible(showComposer |> AVal.toBinding)
              .opacity(opacity |> AVal.toBinding)
              .transitions(tr)
              .content(
                Sample.Views.Composer.view(fun newPost ->
                  printfn $"%A{newPost}"
                  showComposer.setValue false)
              ),
            RouterOutlet()
              .router(router)
              .transitions(tr)
              .opacity(
                opacity
                |> AVal.map(fun v -> if v = 1. then 0. else 1.)
                |> AVal.toBinding
              )
          )
      )
      .OnLoadedHandler(navigate "/profile" router)
#if DEBUG
  window.AttachDevTools()
#endif
  window

let accessToken =
  // we'd usually do the authentication dance
  // but for sample purposes we can use a dev access token provided in the
  // meda apps dashboard
  Environment.GetEnvironmentVariable("THREADS_ACCESS_TOKEN")
  |> Option.ofObj
  |> Option.defaultValue ""

NXUI.Run(app accessToken, "Navs.Avalonia!", Environment.GetCommandLineArgs())
|> ignore
