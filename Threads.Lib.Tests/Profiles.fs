namespace Threads.Lib.Tests


open System
open System.Threading.Tasks
open Microsoft.VisualStudio.TestTools.UnitTesting


open Flurl
open Flurl.Http
open Flurl.Http.Testing

open Threads.Lib

open Threads.Lib.Profiles

[<TestClass>]
type ProfileTests() =


  [<TestMethod>]
  member _.``Fetch Profile can encode the request correctly``() : Task = task {
    use test = new HttpTest()

    test.RespondWithJson(
      {|
        id = "123"
        username = "test"
        threads_profile_picture_url = "https://example.com"
        threads_biography = "test"
      |},
      200
    )
    |> ignore

    let threads = Threads.Create("fake_token")

    let! profile =
      threads.Profile.FetchProfile(
        "me",
        [ Id; Username; ThreadsProfilePictureUrl; ThreadsBiography ]
      )

    let expected = [
      ProfileValue.Id "123"
      ProfileValue.Username "test"
      ProfileValue.ThreadsProfilePictureUrl(Uri("https://example.com"))
      ProfileValue.ThreadsBiography "test"
    ]

    Assert.AreEqual(expected, profile |> Seq.toList)


    let url, method =
      test.CallLog
      |> Seq.head
      |> fun call -> (call.Request.Url, call.Request.Verb.Method)

    Assert.AreEqual("GET", method)
    Assert.AreEqual("/v1.0/me", url.Path)

    match url.QueryParams.TryGetFirst("fields") with
    | true, fields ->
      let fields = (unbox<string> fields).Split(',')
      Assert.AreEqual(4, fields.Length)

      Assert.AreEqual(
        set [
          "id"
          "username"
          "threads_profile_picture_url"
          "threads_biography"
        ],
        fields |> Set.ofArray
      )

    | false, _ -> Assert.Fail("Fields not found in query params")

    match url.QueryParams.TryGetFirst("access_token") with
    | true, value -> Assert.AreEqual("fake_token", unbox<string> value)
    | false, _ -> Assert.Fail("access token not found in query params")
  }
