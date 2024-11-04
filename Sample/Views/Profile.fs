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

  type PageStatus =
    | Loading
    | Idle

  let loadingScreen() =
    TextBlock().text("Loading...") :> Control

  let profilePicture(source: IBinding) =
    Border()
      .cornerRadius(75)
      .width(64)
      .height(64)
      .clipToBounds(true)
      .child(Image().source(source).height(64).width(64))

  let postCard(post: Post) : Control =
    let date = post.timestamp.LocalDateTime.ToString("yyyy-MM-dd HH:mm:ss")

    Border()
      .cornerRadius(5)
      .margin(8.)
      .borderBrush(Brushes.Gray)
      .borderThickness(Thickness(0, 0, 0, 1))
      .padding(2.)
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

  let loadProfilePicture(pfpUri, onLoaded: Imaging.Bitmap -> unit) =
    async {
      let! image =
        match pfpUri |> Option.map(fun uri -> uri) with
        | Some uri -> uri
        | None -> Uri("https://via.placeholder.com/64")
        |> Image.getBitmapFromUri

      return onLoaded image
    }
    |> Async.StartImmediate

  let profileSection(profile: UserProfile) =

    let source = cval None

    loadProfilePicture(profile.profilePicture, Some >> source.setValue)

    let pfpSrc =
      source
      |> AVal.map (function
        | Some v -> v
        | None -> null)
      |> AVal.toBinding


    DockPanel()
      .lastChildFill(true)
      .maxHeight(150)
      .VerticalAlignmentTop()
      .children(
        profilePicture(pfpSrc).DockLeft().VerticalAlignmentTop().margin(8.),
        StackPanel()
          .DockTop()
          .spacing(4.)
          .children(
            TextBlock()
              .text($"@%s{profile.username}")
              .OnTappedEvent(fun _ obs ->
                obs.Add(fun _ ->
                  Debug.WriteLine(
                    $"Clicked on %s{profile.username}, Let's visit!"
                  ))),
            ScrollViewer().content(TextBlock().text(profile.bio))
          )
      )


  let page
    (threads: ThreadsClient, profile: ProfileStore, userThreads: UserThreads)
    ctx
    _
    =
    async {
      let! token = Async.CancellationToken

      let loadProfile = ProfileService.loadProfile threads
      let loadUserThreads = PostService.loadUserThreads threads

      let status = cval(Loading)
      let pagination = cval(None)



      Async.StartImmediate(
        async {
          let! profileData = loadProfile()
          profile.setProfile profileData
          status.setValue Idle
        },
        token
      )

      Async.StartImmediate(
        async {
          let! (posts, page) = loadUserThreads()
          userThreads.setThreads posts
          pagination.setValue(Some page)
        },
        token
      )

      let content =
        adaptive {
          match! status with
          | Loading -> return loadingScreen()
          | Idle ->
            let! profile = profile.profile

            return
              DockPanel()
                .lastChildFill(true)
                .children(
                  (profileSection profile).DockTop().margin(0., 0., 0., 8.),
                  ScrollViewer()
                    .DockTop()
                    .content(
                      ItemsControl()
                        .itemsSource(userThreads.threads |> AVal.toBinding)
                        .itemTemplate(
                          FuncDataTemplate<Post>(fun post _ -> postCard post)
                        )
                    )
                )
        }
        |> AVal.toBinding

      return UserControl().content(content)
    }
