namespace Sample

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
  open System
  open System.Diagnostics

  type UserProfile = {
    id: string
    username: string
    bio: string
    profilePicture: Uri voption
  }

  module UserProfile =
    let Default() = {
      id = ""
      username = ""
      bio = ""
      profilePicture = ValueNone
    }

    let FromValues values =
      values
      |> Seq.fold
        (fun current next ->
          match next with
          | Id id -> { current with id = id }
          | Username username -> { current with username = username }
          | ThreadsBiography bio -> { current with bio = bio }
          | ThreadsProfilePictureUrl profilePicture ->
              {
                current with
                    profilePicture = ValueSome profilePicture
              })
        (Default())

  type Post = {
    id: string
    username: string
    text: string
    timestamp: DateTimeOffset
    mediaUrl: string
    mediaType: Media.MediaType
    owner: Media.Owner
    permalink: string
    children: Media.ThreadId seq
  }

  module Post =

    let Default() = {
      id = ""
      username = ""
      text = ""
      timestamp = DateTimeOffset.MinValue
      mediaUrl = ""
      mediaType = Media.TextPost
      owner = { id = "" }
      permalink = ""
      children = []
    }

    let FromValues values =
      values
      |> Seq.fold
        (fun current next ->
          match next with
          | Media.ThreadValue.Id id -> { current with id = id }
          | Media.ThreadValue.Username username -> {
              current with
                  username = username
            }
          | Media.ThreadValue.Text text -> { current with text = text }
          | Media.ThreadValue.Timestamp timestamp -> {
              current with
                  timestamp = timestamp
            }
          | Media.ThreadValue.MediaUrl mediaUrl -> {
              current with
                  mediaUrl = mediaUrl.ToString()
            }
          | Media.ThreadValue.MediaType mediaType -> {
              current with
                  mediaType = mediaType
            }
          | Media.ThreadValue.Owner owner -> { current with owner = owner }
          | Media.ThreadValue.Permalink permalink -> {
              current with
                  permalink = permalink.ToString()
            }
          | Media.ThreadValue.Children children ->
              {
                current with
                    children = children.data
              })
        (Default())

  type PageStatus =
    | Loading
    | Idle


  let loadProfile
    (threads: ThreadsClient)
    (status: cval<PageStatus>)
    (profile: cval<UserProfile>)
    =
    async {
      let! token = Async.CancellationToken

      let! response =
        threads.Profile.FetchProfile(
          "me",
          [
            ProfileField.Id
            ProfileField.Username
            ProfileField.ThreadsBiography
            ProfileField.ThreadsProfilePictureUrl
          ],
          token
        )

      let record = UserProfile.FromValues response

      profile.setValue(record)
      status.setValue(Idle)
    }

  let loadUserThreads (threads: ThreadsClient) (mediaPosts: cval<Post list>) = async {
    let! token = Async.CancellationToken

    let! threads =
      threads.Media.FetchThreads(
        "me",
        [
          Media.ThreadField.Id
          Media.ThreadField.Username
          Media.ThreadField.Text
          Media.ThreadField.Timestamp
          Media.ThreadField.MediaUrl
          Media.ThreadField.MediaType
          Media.ThreadField.Owner
          Media.ThreadField.Permalink
        ],
        cancellationToken = token
      )

    threads.data |> Seq.map Post.FromValues |> Seq.toList |> mediaPosts.setValue

    return ()
  }

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

  let loadProfilePicture
    (pfpUri, onLoaded: Avalonia.Media.Imaging.Bitmap -> unit)
    =
    async {
      let! image =
        match pfpUri |> ValueOption.map(fun uri -> uri) with
        | ValueSome uri -> uri
        | ValueNone -> Uri("https://via.placeholder.com/64")
        |> Image.getBitmapFromUri

      return onLoaded image
    }
    |> Async.StartImmediate

  let profileSection(profile: UserProfile) =

    let source = cval ValueNone

    loadProfilePicture(profile.profilePicture, ValueSome >> source.setValue)

    let pfpSrc =
      source
      |> AVal.map (function
        | ValueSome v -> v
        | ValueNone -> null)
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


  let page (threads: ThreadsClient) ctx _ = async {
    let loadProfile = loadProfile threads
    let loadUserThreads = loadUserThreads threads
    let status = cval(Loading)

    let profile = cval(UserProfile.Default())
    let mediaPosts = cval([])

    let! token = Async.CancellationToken

    Async.StartImmediate(loadProfile status profile, token)
    Async.StartImmediate(loadUserThreads mediaPosts, token)

    let content =
      adaptive {
        match! status with
        | Loading -> return loadingScreen()
        | Idle ->
          let! profile = profile

          return
            DockPanel()
              .lastChildFill(true)
              .children(
                (profileSection profile).DockTop().margin(0., 0., 0., 8.),
                ScrollViewer()
                  .DockTop()
                  .content(
                    ItemsControl()
                      .itemsSource(mediaPosts |> AVal.toBinding)
                      .itemTemplate(
                        FuncDataTemplate<Post>(fun post _ -> postCard post)
                      )
                  )
              )
      }
      |> AVal.toBinding

    return UserControl().content(content)
  }
