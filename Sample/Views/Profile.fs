namespace Sample

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

module Profile =
  open Sample.Services

  type PageStatus =
    | Loading
    | Idle

  let loadingScreen() =
    TextBlock().text("Loading...") :> Control

  let profilePicture(source: aval<string>) =
    Border()
      .cornerRadius(75)
      .width(64)
      .height(64)
      .clipToBounds(true)
      .child(Image().asyncSource(source |> AVal.toBinding).height(64).width(64))

  let postCard(post: Post) : Control =
    let date = post.timestamp.LocalDateTime.ToString("yyyy-MM-dd HH:mm:ss")
    let opacity = cval 0
    let transitions = Animation.Transitions()

    transitions.Add(
      Animation.DoubleTransition(
        Property = Control.OpacityProperty,
        Duration = TimeSpan.FromMilliseconds(450),
        Easing = Animation.Easings.CubicEaseInOut()
      )
    )

    let border =
      Border()
        .cornerRadius(5)
        .margin(8.)
        .opacity(opacity |> AVal.toBinding)
        .borderBrush(Brushes.Gray)
        .borderThickness(Thickness(0, 0, 0, 1))
        .padding(2.)
        .transitions(transitions)
        .child(
          StackPanel()
            .spacing(2.5)
            .children(
              TextBlock().text(post.username).DockTop(),
              TextBlock()
                .text(post.text)
                .DockLeft()
                .textWrapping(TextWrapping.Wrap),
              TextBlock().text(date).DockBottom()
            )
        )

    border.AttachedToVisualTree.Add(fun _ -> opacity.setValue 1)
    border.DetachedFromVisualTree.Add(fun _ -> opacity.setValue 0)
    border

  let profileSection(profile: aval<UserProfile>, onNavigateToProfile) =
    let source =
      profile
      |> AVal.map(fun p ->
        defaultArg p.profilePicture (Uri("https://via.placeholder.com/64")))
      |> AVal.map(fun uri -> uri.ToString())

    let username = profile |> AVal.map(fun p -> $"@%s{p.username}")

    let bio = profile |> AVal.map(fun p -> p.bio)

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
              .text(username |> AVal.toBinding)
              .OnTappedEvent(fun sender obs ->
                obs.Add(fun args -> onNavigateToProfile())),
            ScrollViewer().content(TextBlock().text(bio |> AVal.toBinding))
          )
      )


  let page (profileStore: ProfileStore, userThreads: UserThreadsStore) ctx _ = async {
    let! token = Async.CancellationToken

    let status = cval(Loading)

    Async.StartImmediate(
      async {
        do!
          Async.Parallel(
            [ profileStore.loadProfile(); userThreads.loadUserThreads() ]
          )
          |> Async.Ignore

        status.setValue(Idle)
      },
      token
    )

    let profile =
      profileStore.profile |> AVal.map(fun p -> defaultArg p UserProfile.empty)

    let content =
      adaptive {
        match! status with
        | Loading -> return loadingScreen()
        | Idle ->
          return
            DockPanel()
              .lastChildFill(true)
              .children(
                profileSection(profile, profileStore.navigateToProfile)
                  .DockTop()
                  .margin(0., 0., 0., 8.),
                ScrollViewer()
                  .DockTop()
                  .content(
                    ItemsControl()
                      .itemsSource(userThreads.userThreads |> AVal.toBinding)
                      .itemTemplate(
                        FuncDataTemplate<Post>(fun post _ -> postCard post)
                      )
                  )
              )
      }
      |> AVal.toBinding

    return UserControl().content(content)
  }
