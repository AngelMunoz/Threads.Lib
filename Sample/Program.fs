open System

open Avalonia
open Avalonia.Controls
open Avalonia.Controls.Primitives
open Avalonia.Data

open NXUI.Desktop
open NXUI.FSharp.Extensions

open FSharp.Data.Adaptive

open Navs
open Navs.Avalonia

open Threads.Lib

open Sample
open Sample.Services
open Sample.Services.Threads

let navigate url (router: IRouter<Control>) _ _ =
  task {
    let! result = router.Navigate(url)

    match result with
    | Ok _ -> ()
    | Error e -> printfn $"%A{e}"
  }
  |> ignore

let onPostThread (store: UserThreadsStore) (postParams: PostParameters) = async {
  try
    do! store.postThread postParams
    return Ok()
  with _ ->
    return Error "Failed to post"
}

let app accessToken () =

  let threads = Threads.Create(accessToken)

  let threadsService = ThreadsService.create(threads)

  let profileStore = ProfileStore.create(threadsService)
  let threadsStore = UserThreadsStore.create(threadsService)
  let metricsStore = MetricsStore.create(threadsService)

  let router: IRouter<_> =
    AvaloniaRouter(
      Routes.getRoutes(profileStore, threadsStore, metricsStore),
      splash = (fun _ -> TextBlock().text("Loading..."))
    )

  let showComposer = cval false

  let window =
    Window()
      .minWidth(420)
      .minHeight(250)
      .content(
        DockPanel()
          .children(
            StackPanel()
              .DockTop()
              .OrientationHorizontal()
              .margin(8)
              .spacing(8)
              .children(
                Button()
                  .content("My Profile")
                  .OnClickHandler(navigate "/profile" router),
                Button()
                  .content("My Metrics")
                  .OnClickHandler(navigate $"/metrics" router),
                ToggleButton()
                  .content("New Post")
                  .OnIsCheckedChangedHandler(fun sender args ->
                    sender.IsChecked
                    |> Option.ofNullable
                    |> Option.defaultValue false
                    |> showComposer.setValue)
              ),
            UserControl()
              .DockBottom()
              .isVisible(showComposer |> AVal.toBinding)
              .content(Views.Composer.view(onPostThread threadsStore)),
            RouterOutlet().router(router).padding(12, 24)
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
  |> Option.defaultWith(fun _ -> failwith "access token was not set")

NXUI.Run(app accessToken, "Navs.Avalonia!", Environment.GetCommandLineArgs())
|> ignore
