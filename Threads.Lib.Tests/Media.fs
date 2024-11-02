namespace Threads.Lib.Tests

#nowarn "25"

open System
open System.Threading.Tasks
open Microsoft.VisualStudio.TestTools.UnitTesting

open FsToolkit.ErrorHandling

open Flurl
open Flurl.Http
open Flurl.Http.Testing

open Threads.Lib
open Threads.Lib.Common
open Threads.Lib.Media


[<TestClass>]
type MediaTests() =

  [<TestMethod>]
  member _.``Fetch Thread encodes the request correctly``() : Task = task {
    use test = new HttpTest()

    test.RespondWithJson({| id = "1234567890" |}) |> ignore

    let threads = Threads.Lib.Threads.Create("fake_token")

    let sorted =
      [
        "id"
        "media_product_type"
        "media_type"
        "media_url"
        "permalink"
        "owner"
        "username"
        "text"
        "timestamp"
        "shortcode"
        "thumbnail_url"
        "children"
        "is_quote_post"
      ]
      |> set

    let! _ =
      threads.Media.FetchThread(
        "fake_thread_id",
        [
          Id
          MediaProductType
          MediaType
          MediaUrl
          Permalink
          Owner
          Username
          Text
          Timestamp
          ShortCode
          ThumbnailUrl
          Children
          IsQuotePost
        ]
      )

    let url, method =
      test.CallLog
      |> Seq.head
      |> fun call -> (call.Request.Url, call.Request.Verb.Method)

    Assert.AreEqual("GET", method)
    Assert.AreEqual("/v1.0/fake_thread_id/threads", url.Path)

    match url.QueryParams.TryGetFirst("fields") with
    | true, fields ->
      let actual = (unbox<string> fields).Split(',') |> set
      Assert.AreEqual(sorted, actual)
    | false, _ -> Assert.Fail("Fields not found")

    match url.QueryParams.TryGetFirst("access_token") with
    | true, token -> Assert.AreEqual("fake_token", unbox<string> token)
    | false, _ -> Assert.Fail("Token not found")

  }

  [<TestMethod>]
  member _.``Fetch Thread decodes the response correctly``() : Task = task {
    use test = new HttpTest()

    let expected = {|
      id = "1234567890"
      media_product_type = "THREADS"
      media_type = "TEXT_POST"
      media_url = "https://media_url"
      permalink = "https://permalink"
      owner = { id = "owner_id" }
      username = "username"
      text = "text"
      timestamp = "2021-01-01T00:00:00Z"
      shortcode = "shortcode"
      thumbnail_url = "https://thumbnail_url"
      children = { data = [ { id = "children" } ] }
      is_quote_post = false
    |}

    test.RespondWithJson(expected) |> ignore

    let threads = Threads.Lib.Threads.Create("fake_token")

    let! response = threads.Media.FetchThread("fake_thread_id", [ Id ])

    let url, method =
      test.CallLog
      |> Seq.head
      |> fun call -> (call.Request.Url, call.Request.Verb.Method)

    Assert.AreEqual("GET", method)
    Assert.AreEqual("/v1.0/fake_thread_id/threads", url.Path)

    match url.QueryParams.TryGetFirst("fields") with
    | true, fields ->
      let actual = (unbox<string> fields).Split(',') |> set
      Assert.AreEqual(set [ "id" ], actual)
    | false, _ -> Assert.Fail("Fields not found")

    match url.QueryParams.TryGetFirst("access_token") with
    | true, token -> Assert.AreEqual("fake_token", unbox<string> token)
    | false, _ -> Assert.Fail("Token not found")

    Assert.AreEqual(13, response |> Seq.length)

    for value in response do
      match value with
      | ThreadValue.Id id -> Assert.AreEqual(expected.id, id)
      | ThreadValue.MediaProductType productType ->
        Assert.AreEqual(MediaProductType.Threads, productType)
      | ThreadValue.MediaType mediaType ->
        Assert.AreEqual(MediaType.TextPost, mediaType)
      | ThreadValue.MediaUrl mediaUrl ->
        Assert.AreEqual(Uri expected.media_url, mediaUrl)
      | ThreadValue.Permalink permalink ->
        Assert.AreEqual(Uri expected.permalink, permalink)
      | ThreadValue.Owner owner -> Assert.AreEqual(expected.owner.id, owner.id)
      | ThreadValue.Username username ->
        Assert.AreEqual(expected.username, username)
      | ThreadValue.Text text -> Assert.AreEqual(expected.text, text)
      | ThreadValue.Timestamp timestamp ->
        Assert.AreEqual(DateTimeOffset.Parse(expected.timestamp), timestamp)
      | ThreadValue.ShortCode shortcode ->
        Assert.AreEqual(expected.shortcode, shortcode)
      | ThreadValue.ThumbnailUrl thumbnailUrl ->
        Assert.AreEqual(Uri expected.thumbnail_url, thumbnailUrl)
      | ThreadValue.Children children ->
        Assert.AreEqual(1, children.data |> Seq.length)
      | ThreadValue.IsQuotePost isQuotePost ->
        Assert.AreEqual(expected.is_quote_post, isQuotePost)
  }

  [<TestMethod>]
  member _.``Fetch Threads encodes fields and pagination correctly and decodes response correctly``
    ()
    : Task =
    task {
      use test = new HttpTest()

      let response1 = {|
        id = "res1"
        media_product_type = "THREADS"
        media_type = "TEXT_POST"
        media_url = "https://media_url"
        permalink = "https://permalink"
        owner = { id = "owner_1" }
        username = "username"
        text = "text"
        timestamp = "2021-01-01T00:00:00Z"
        shortcode = "shortcode"
        thumbnail_url = "https://thumbnail_url"
        children = { data = [ { id = "children" } ] }
        is_quote_post = false
      |}

      let response2 = {|
        id = "res2"
        media_product_type = "THREADS"
        media_type = "TEXT_POST"
        media_url = "https://media_url"
        permalink = "https://permalink"
        owner = { id = "owner_2" }
        username = "username"
        text = "text"
        timestamp = "2021-01-01T00:00:00Z"
        shortcode = "shortcode"
        thumbnail_url = "https://thumbnail_url"
        children = { data = [ { id = "children" } ] }
        is_quote_post = false
      |}

      let response = {|
        data = [ response1; response2 ]
        paging = {|
          cursors = {| before = "before"; after = "after" |}
          next = Some "next"
          previous = Some "previous"
        |}
      |}

      test.RespondWithJson(response) |> ignore

      let threads = Threads.Lib.Threads.Create("fake_token")

      let! response =
        threads.Media.FetchThreads(
          "fake_thread_id",
          [
            Id
            MediaProductType
            MediaType
            MediaUrl
            Permalink
            Owner
            Username
            Text
            Timestamp
            ShortCode
            ThumbnailUrl
            Children
            IsQuotePost
          ],
          PaginationKind.Cursor [
            After "after"
            Before "before"
            CursorParam.Limit 10u
          ]
        )

      let url, method =
        test.CallLog
        |> Seq.head
        |> fun call -> (call.Request.Url, call.Request.Verb.Method)

      Assert.AreEqual("GET", method)
      Assert.AreEqual("/v1.0/fake_thread_id/threads", url.Path)

      match url.QueryParams.TryGetFirst("fields") with
      | true, fields ->
        let actual = (unbox<string> fields).Split(',') |> set

        Assert.AreEqual(
          set [
            "id"
            "media_product_type"
            "media_type"
            "media_url"
            "permalink"
            "owner"
            "username"
            "text"
            "timestamp"
            "shortcode"
            "thumbnail_url"
            "children"
            "is_quote_post"
          ],
          actual
        )
      | false, _ -> Assert.Fail("Fields not found")

      match url.QueryParams.TryGetFirst("access_token") with
      | true, token -> Assert.AreEqual("fake_token", unbox<string> token)
      | false, _ -> Assert.Fail("Token not found")

      match url.QueryParams.TryGetFirst("after") with
      | true, after -> Assert.AreEqual("after", unbox<string> after)
      | false, _ -> Assert.Fail("After not found")

      match url.QueryParams.TryGetFirst("before") with
      | true, before -> Assert.AreEqual("before", unbox<string> before)
      | false, _ -> Assert.Fail("Before not found")

      match url.QueryParams.TryGetFirst("limit") with
      | true, limit -> Assert.AreEqual("10", unbox<string> limit)
      | false, _ -> Assert.Fail("Limit not found")

      Assert.AreEqual("next", response.paging.next.Value)
      Assert.AreEqual("previous", response.paging.previous.Value)
      Assert.AreEqual("before", response.paging.cursors.before)
      Assert.AreEqual("after", response.paging.cursors.after)

      let [ first; second ] = response.data |> Seq.toList

      for value in first do
        match value with
        | ThreadValue.Id id -> Assert.AreEqual(response1.id, id)
        | ThreadValue.MediaProductType productType ->
          Assert.AreEqual(MediaProductType.Threads, productType)
        | ThreadValue.MediaType mediaType ->
          Assert.AreEqual(MediaType.TextPost, mediaType)
        | ThreadValue.MediaUrl mediaUrl ->
          Assert.AreEqual(Uri response1.media_url, mediaUrl)
        | ThreadValue.Permalink permalink ->
          Assert.AreEqual(Uri response1.permalink, permalink)
        | ThreadValue.Owner owner ->
          Assert.AreEqual(response1.owner.id, owner.id)
        | ThreadValue.Username username ->
          Assert.AreEqual(response1.username, username)
        | ThreadValue.Text text -> Assert.AreEqual(response1.text, text)
        | ThreadValue.Timestamp timestamp ->
          Assert.AreEqual(DateTimeOffset.Parse(response1.timestamp), timestamp)
        | ThreadValue.ShortCode shortcode ->
          Assert.AreEqual(response1.shortcode, shortcode)
        | ThreadValue.ThumbnailUrl thumbnailUrl ->
          Assert.AreEqual(Uri response1.thumbnail_url, thumbnailUrl)
        | ThreadValue.Children children ->
          Assert.AreEqual(1, children.data |> Seq.length)
        | ThreadValue.IsQuotePost isQuotePost ->
          Assert.AreEqual(response1.is_quote_post, isQuotePost)

      for value in second do
        match value with
        | ThreadValue.Id id -> Assert.AreEqual(response2.id, id)
        | ThreadValue.MediaProductType productType ->
          Assert.AreEqual(MediaProductType.Threads, productType)
        | ThreadValue.MediaType mediaType ->
          Assert.AreEqual(MediaType.TextPost, mediaType)
        | ThreadValue.MediaUrl mediaUrl ->
          Assert.AreEqual(Uri response2.media_url, mediaUrl)
        | ThreadValue.Permalink permalink ->
          Assert.AreEqual(Uri response2.permalink, permalink)
        | ThreadValue.Owner owner ->
          Assert.AreEqual(response2.owner.id, owner.id)
        | ThreadValue.Username username ->
          Assert.AreEqual(response2.username, username)
        | ThreadValue.Text text -> Assert.AreEqual(response2.text, text)
        | ThreadValue.Timestamp timestamp ->
          Assert.AreEqual(DateTimeOffset.Parse(response2.timestamp), timestamp)
        | ThreadValue.ShortCode shortcode ->
          Assert.AreEqual(response2.shortcode, shortcode)
        | ThreadValue.ThumbnailUrl thumbnailUrl ->
          Assert.AreEqual(Uri response2.thumbnail_url, thumbnailUrl)
        | ThreadValue.Children children ->
          Assert.AreEqual(1, children.data |> Seq.length)
        | ThreadValue.IsQuotePost isQuotePost ->
          Assert.AreEqual(response2.is_quote_post, isQuotePost)
    }
