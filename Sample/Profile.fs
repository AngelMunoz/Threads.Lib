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

  type UserProfile = {
    id: string
    username: string
    bio: string
    profilePicture: Uri option
  } with

    static member Default() = {
      id = ""
      username = ""
      bio = ""
      profilePicture = None
    }



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
        threads.FetchProfile(
          "me",
          [
            ProfileField.Id
            ProfileField.Username
            ProfileField.ThreadsBiography
            ProfileField.ThreadsProfilePictureUrl
          ],
          token
        )

      let record =
        response
        |> Seq.fold
          (fun current next ->
            match next with
            | Id id -> { current with id = id }
            | Username username -> { current with username = username }
            | ThreadsBiography bio -> { current with bio = bio }
            | ThreadsProfilePictureUrl profilePicture ->
                {
                  current with
                      profilePicture = Some profilePicture
                })
          (UserProfile.Default())

      profile.setValue(record)
      status.setValue(Idle)
    }

  let loadingScreen() =
    TextBlock().text("Loading...") :> Control

  let profileSection(profile: cval<UserProfile>) = adaptive {
    let! profile = profile

    return
      StackPanel()
        .spacing(4.)
        .children(
          StackPanel()
            .spacing(2.)
            .OrientationHorizontal()
            .children(
              TextBlock().text("Username: "),
              TextBlock().text(profile.username)
            ),
          StackPanel()
            .spacing(2.)
            .OrientationHorizontal()
            .children(TextBlock().text("Bio: "), TextBlock().text(profile.bio))
        )
      :> Control
  }

  let page (threads: ThreadsClient) ctx _ =
    let loadProfile = loadProfile threads

    async {
      let status = cval(Loading)

      let profile = cval(UserProfile.Default())

      let! token = Async.CancellationToken

      Async.StartImmediate(loadProfile status profile, token)

      return
        UserControl()
          .content(
            adaptive {
              match! status with
              | Loading -> return loadingScreen()
              | Idle -> return! profileSection profile
            }
            |> AVal.toBinding
          )

    }
