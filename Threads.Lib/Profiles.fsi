namespace Threads.Lib

open System


module Profiles =

  [<Struct>]
  type ProfileField =
    | Id
    | Username
    | ThreadsProfilePictureUrl
    | ThreadsBiography

  [<RequireQualifiedAccess>]
  type ProfileValue =
    | Id of string
    | Username of string
    | ThreadsProfilePictureUrl of Uri
    | ThreadsBiography of string

  val internal getProfile:
    baseUrl: string ->
    accessToken: string ->
    profileId: string option ->
    profileFields: ProfileField seq ->
      Async<Result<ProfileValue seq, string>>
