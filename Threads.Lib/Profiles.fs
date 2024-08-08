namespace Threads.Lib

open System

open FsHttp
open Thoth.Json.Net

module Profiles =

  [<Struct>]
  type ProfileField =
    | Id
    | Username
    | ThreadsProfilePictureUrl
    | ThreadsBiography

  module ProfileField =
    let asString =
      function
      | Id -> "id"
      | Username -> "username"
      | ThreadsProfilePictureUrl -> "threads_profile_picture_url"
      | ThreadsBiography -> "threads_biography"

  type ProfileValue =
    | Id of string
    | Username of string
    | ThreadsProfilePictureUrl of Uri
    | ThreadsBiography of string

  module ProfileValue =

    let decodeId(get: Decode.IGetters, fields: _ ResizeArray) =
      get.Required.Field "id" Decode.string |> Id |> fields.Add

      get, fields

    let decodeUsername(get: Decode.IGetters, fields: _ ResizeArray) =

      match get.Optional.Field "username" Decode.string with
      | Some username -> Username username |> fields.Add
      | None -> ()

      get, fields

    let decodeThreadsProfilePicture
      (get: Decode.IGetters, fields: _ ResizeArray)
      =

      match get.Optional.Field "threads_profile_picture_url" Decode.string with
      | Some profilePictureUrl ->
        Uri profilePictureUrl |> ThreadsProfilePictureUrl |> fields.Add
      | None -> ()

      get, fields

    let decodeThreadsBiography(get: Decode.IGetters, fields: _ ResizeArray) =

      match get.Optional.Field "threads_biography" Decode.string with
      | Some bio -> bio |> ThreadsBiography |> fields.Add
      | None -> ()

      get, fields

    let inline finish(_, fields: _ seq) = fields

    let Decode: Decoder<ProfileValue seq> =
      Decode.object(fun get ->
        (get, ResizeArray())
        |> decodeId
        |> decodeUsername
        |> decodeThreadsProfilePicture
        |> decodeThreadsBiography
        |> finish)

  let getProfile baseUrl accessToken profileId profileFields = async {
    let fields =
      profileFields
      |> Seq.map ProfileField.asString
      |> (fun f -> String.Join(",", f))

    let profileId =
      match profileId with
      | Some id -> id
      | None -> "me"

    let! req =
      http {
        GET $"%s{baseUrl}/%s{profileId}"

        query [
          if String.IsNullOrEmpty fields then () else "fields", fields
          "access_token", accessToken
        ]
      }
      |> Request.sendAsync

    let! res = Response.toTextAsync req

    return Decode.fromString ProfileValue.Decode res
  }
