namespace Threads.Lib

open System

open FsHttp

module Profiles =
  [<Struct>]
  type ProfileField =
    | Id
    | Username
    | ThreadsProfilePictureUrl
    | ThreadsBiography

  type ProfileValue =
    | Id of string
    | Username of string
    | ThreadsProfilePictureUrl of Uri
    | ThreadsBiography of string

  val internal getProfile:
    baseHttp: HeaderContext ->
    accessToken: string ->
    profileId: string option ->
    profileFields: ProfileField seq ->
      Async<Result<ProfileValue seq, string>>
