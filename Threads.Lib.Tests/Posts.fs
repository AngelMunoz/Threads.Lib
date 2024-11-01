namespace Threads.Lib.Tests

open System
open System.Threading.Tasks
open Flurl.Util
open Microsoft.VisualStudio.TestTools.UnitTesting
open Flurl.Http.Testing
open Threads.Lib.Common
open Threads.Lib.Posts


[<TestClass>]
type PostsTestsClass() =

  [<TestMethod>]
  member _.``Post container creates a text post container``() : Task = task {
    use test = new HttpTest()

    let suppliedId = Guid.NewGuid().ToString()

    test.RespondWithJson({ id = suppliedId }) |> ignore

    let threads = Threads.Lib.Threads.Create("fake_token")

    let! response = threads.Posts.PostContainer("me", [ Text "Hello, World!" ])


    let url, method =
      test.CallLog
      |> Seq.head
      |> fun call -> (call.Request.Url, call.Request.Verb.Method)

    Assert.AreEqual("POST", method)
    Assert.AreEqual("/v1.0/me/threads", url.Path)

    match url.QueryParams.TryGetFirst("text") with
    | true, value -> Assert.AreEqual("Hello, World!", unbox value)
    | _ -> Assert.Fail("text parameter not found")

    match url.QueryParams.TryGetFirst("is_carousel_item") with
    | true, value -> Assert.AreEqual("false", unbox value)
    | _ -> Assert.Fail("is_carousel_item parameter not found")

    match url.QueryParams.TryGetFirst("access_token") with
    | true, value -> Assert.AreEqual("fake_token", unbox value)
    | _ -> Assert.Fail("fake_token parameter not found")


    Assert.AreEqual(suppliedId, response.id)
  }
