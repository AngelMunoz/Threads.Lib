open System


open IcedTasks
open IcedTasks.Polyfill.Async
open IcedTasks.Polyfill.Task

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
open FsToolkit.ErrorHandling

let onPostThread (store: UserThreadsStore) (postParams: PostParameters) = async {
  try
    do! store.postThread postParams
    return Ok()
  with _ ->
    return Error "Failed to post"
}

let topToolbar(navigate, onToggleNewPost) =
  StackPanel()
    .OrientationHorizontal()
    .margin(8)
    .spacing(8)
    .children(
      Button()
        .content("My Profile")
        .OnClickHandler(fun _ _ -> navigate "/profile"),
      Button()
        .content("My Metrics")
        .OnClickHandler(fun _ _ -> navigate $"/metrics"),
      ToggleButton()
        .content("New Post")
        .OnIsCheckedChangedHandler(fun sender args ->
          sender.IsChecked
          |> Option.ofNullable
          |> Option.defaultValue false
          |> onToggleNewPost)
    )

let postComposer(showComposer, onPostThread) =
  UserControl()
    .isVisible(showComposer |> AVal.toBinding)
    .content(Views.Composer.view(onPostThread))


// App orchestration
let app accessToken () =

  // services
  let threads = Threads.Create(accessToken)
  let threadsService = ThreadsService.create(threads)

  // stores/viewmodels you name it.
  let profileStore = ProfileStore.create(threadsService)
  let threadsStore = UserThreadsStore.create(threadsService)
  let metricsStore = MetricsStore.create(threadsService)

  let navigateTo = ref ValueNone

  let appStore = AppStore.create(navigateTo)

  let router: IRouter<_> =
    AvaloniaRouter(
      Routes.getRoutes(profileStore, threadsStore, metricsStore),
      splash = (fun _ -> TextBlock().text("Loading..."))
    )

  navigateTo.Value <- ValueSome(fun route -> router.Navigate(route))
  // locally injected dependencies
  let navigate url =
    Async.StartImmediate(
      router.Navigate(url) |> TaskResult.ignoreError |> Async.AwaitTask
    )

  let onPostThread = onPostThread threadsStore

  let window =
    Window()
      .minWidth(420)
      .minHeight(250)
      .content(
        DockPanel()
          .children(
            topToolbar(navigate, appStore.showComposer).DockTop(),
            postComposer(appStore.isPostComposerVisible, onPostThread)
              .DockBottom(),
            RouterOutlet().router(router).padding(12, 24)
          )
      )
      .OnLoadedHandler(fun _ _ -> navigate "/profile")
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
