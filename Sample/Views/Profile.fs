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
open Threads.Lib.Profiles

open Sample
open Sample.Services


[<AutoOpen>]
module private ProfileStyles =

  type Border with

    member this.StyleAsPfpBorder(pfpSize: int) =
      this.cornerRadius(75).width(pfpSize).height(pfpSize).clipToBounds(true)

    member this.StyleAsCardBorder() =
      this
        .cornerRadius(5)
        .margin(8.)
        .borderBrush(Brushes.Gray)
        .borderThickness(Thickness(0, 0, 0, 1))
        .padding(2.)

    member this.AddOpacityTransition() =
      let transitions = Animation.Transitions()

      transitions.Add(
        Animation.DoubleTransition(
          Property = Control.OpacityProperty,
          Duration = TimeSpan.FromMilliseconds(450),
          Easing = Animation.Easings.CubicEaseInOut()
        )
      )

      this.transitions(transitions)

  type TextBlock with
    member this.StyleAsLoadingScreen() =
      this
        .HorizontalAlignmentCenter()
        .VerticalAlignmentCenter()
        .padding(8.)
        .fontSize(24)

module Profile =

  type PageStatus =
    | Loading
    | Idle
    | Error of string

  let loadingScreen(error: string option) : Control =
    let content =
      match error with
      | Some e -> $"Failed to load profile and threads: {e} "
      | None -> "Loading..."

    TextBlock().StyleAsLoadingScreen().text(content)

  let profilePicture(source: aval<string>) =
    let pfpSize = 64

    Border()
      .StyleAsPfpBorder(pfpSize)
      .child(
        Image()
          .asyncSource(source |> AVal.toBinding)
          .height(pfpSize)
          .width(pfpSize)
      )

  let postCard(post: Post) : Control =
    let date = post.timestamp.LocalDateTime.ToString("yyyy-MM-dd HH:mm:ss")
    let opacity = cval 0

    let border =
      Border()
        .StyleAsCardBorder()
        .AddOpacityTransition()
        .opacity(opacity |> AVal.toBinding)
        .child(
          StackPanel()
            .spacing(2.5)
            .children(
              TextBlock().text(post.username),
              TextBlock().text(post.text).textWrapping(TextWrapping.Wrap),
              TextBlock().text(date)
            )
        )

    border.AttachedToVisualTree.Add(fun _ -> opacity.setValue 1)
    border.DetachedFromVisualTree.Add(fun _ -> opacity.setValue 0)
    border

  let profileSection(profile: aval<UserProfile>, onNavigateToProfile) =
    let bio = profile |> AVal.map(fun p -> p.bio) |> AVal.toBinding

    let username =
      profile |> AVal.map(fun p -> $"@%s{p.username}") |> AVal.toBinding

    let source = adaptive {
      let! profile = profile

      let uri =
        Uri("https://via.placeholder.com/64")
        |> defaultArg profile.profilePicture

      return uri.ToString()
    }

    DockPanel()
      .lastChildFill(true)
      .maxHeight(150)
      .VerticalAlignmentTop()
      .children(
        profilePicture(source).DockLeft().VerticalAlignmentTop().margin(8.),
        StackPanel()
          .DockTop()
          .spacing(4.)
          .children(
            TextBlock()
              .text(username)
              .OnTappedEvent(fun sender obs ->
                obs.Add(fun args -> onNavigateToProfile())),
            ScrollViewer().content(TextBlock().text(bio))
          )
      )

  let private postCardTpl = FuncDataTemplate<Post>(fun post _ -> postCard post)

  let private threadList(threadsList) =
    ScrollViewer()
      .content(
        ItemsControl()
          .itemsSource(threadsList |> AVal.toBinding)
          .itemTemplate(postCardTpl)
      )

  let private initStores(profileStore, userThreads, onSetUp, token) =
    Async.StartImmediate(
      async {
        try
          do!
            Async.Parallel(
              [ profileStore.loadProfile(); userThreads.loadUserThreads() ]
            )
            |> Async.Ignore

          onSetUp(Idle)
        with _ ->
          onSetUp(Error "Failed to load profile and threads")
      },
      token
    )

  let page (profileStore: ProfileStore, userThreads: UserThreadsStore) ctx _ = async {
    let! token = Async.CancellationToken
    let status = cval(Loading)

    initStores(profileStore, userThreads, status.setValue, token)

    let profile =
      profileStore.profile |> AVal.map(fun p -> defaultArg p UserProfile.empty)

    return
      UserControl()
        .content(
          adaptive {
            match! status with
            | Loading -> return loadingScreen(None)
            | Error e -> return loadingScreen(Some e)
            | Idle ->
              return
                DockPanel()
                  .children(
                    profileSection(profile, profileStore.navigateToProfile)
                      .DockTop()
                      .margin(0., 0., 0., 8.),
                    threadList(userThreads.userThreads).DockTop()
                  )
          }
          |> AVal.toBinding
        )
  }
