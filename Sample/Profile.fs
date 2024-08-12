namespace Sample

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
open Threads.Lib.API

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

  let loadingScreen() =
    TextBlock().text("Loading...") :> Control

  let profilePicture(source: IBinding) =
    Border()
      .cornerRadius(75)
      .width(64)
      .height(64)
      .clipToBounds(true)
      .child(Image().source(source).height(64).width(64))

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

  let profileSection(profile: cval<UserProfile>) = adaptive {
    let! profile = profile

    let source = cval ValueNone

    loadProfilePicture(profile.profilePicture, ValueSome >> source.setValue)

    let pfpSrc =
      source
      |> AVal.map (function
        | ValueSome v -> v
        | ValueNone -> null)
      |> AVal.toBinding

    return
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
      :> Control
  }

  let page (threads: ThreadsClient) ctx _ = async {
    let loadProfile = loadProfile threads
    let status = cval(Loading)

    let profile = cval(UserProfile.Default())

    let! token = Async.CancellationToken

    Async.StartImmediate(loadProfile status profile, token)

    let content =
      adaptive {
        match! status with
        | Loading -> return loadingScreen()
        | Idle -> return! profileSection profile
      }
      |> AVal.toBinding

    return UserControl().content(content)

  }
