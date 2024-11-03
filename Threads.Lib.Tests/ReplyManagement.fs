namespace Threads.Lib.Tests


open System
open System.Threading.Tasks
open Microsoft.VisualStudio.TestTools.UnitTesting

open Flurl
open Flurl.Http
open Flurl.Http.Testing

open Threads.Lib
open Threads.Lib.Common
open Threads.Lib.ReplyManagement


module Data =

  let rateLimitsResponse = {|
    data = [
      {|
        reply_quota_usage = 100
        reply_config = {|
          quota_total = 1000L
          quota_duration = 100000L
        |}
      |}
    ]
  |}

  let conversationResponse = {|
    data = [
      {|
        id = "1234567890"
        text = "First Reply"
        username = "test"
        permalink = "https://example.com"
        timestamp = "2024-01-01T18:20:00+0000"
        media_product_type = "THREADS"
        media_type = "TEXT_POST"
        media_url = "https://example.com"
        shortcode = "abcdefg"
        thumbnail_url = "https://example.com"
        is_quote_post = false
        has_replies = true
        root_post = {| id = "1234567890" |}
        replied_to = {| id = "1234567890" |}
        is_reply = true
        is_reply_owned_by_me = true
        hide_status = "NOT_HUSHED"
        reply_audience = "EVERYONE"
      |}
    ]
    paging = {|
      cursors = {|
        before = "BEFORE_CURSOR"
        after = "AFTER_CURSOR"
      |}
    |}
  |}

  let repliesResponse userId = {|
    data = [
      {|
        id = userId
        text = "First Reply"
        timestamp = "2024-01-01T18:20:00+0000"
        media_product_type = "THREADS"
        media_type = "TEXT_POST"
        shortcode = "abcdefg"
        has_replies = true
        root_post = { id = "1234567890" }
        replied_to = { id = "1234567890" }
        is_reply = true
        hide_status = "NOT_HUSHED"
      |}
    ]
    paging = {|
      cursors = {|
        before = "BEFORE_CURSOR"
        after = "AFTER_CURSOR"
      |}
    |}
  |}

[<TestClass>]
type ReplyManagementTests() =

  [<TestMethod>]
  member _.``Fetch Rate Limits can encode the request correctly``() : Task = task {
    use test = new HttpTest()

    test.RespondWithJson(Data.rateLimitsResponse, 200) |> ignore

    let threads = Threads.Create("fake_token")

    let! rateLimits =
      threads.Replies.FetchRateLimits(
        "me",
        [ ReplyQuotaUsage; RateLimitField.ReplyConfig ]
      )

    let expected = [
      RateLimitFieldValue.ReplyQuotaUsage 100u
      RateLimitFieldValue.ReplyConfig {
        quotaTotal = 1000L
        quotaDuration = 100000L
      }
    ]

    Assert.AreEqual(expected, rateLimits.data |> Seq.head |> Seq.toList)

    let url, method =
      test.CallLog
      |> Seq.head
      |> fun call -> (call.Request.Url, call.Request.Verb.Method)

    Assert.AreEqual("GET", method)
    Assert.AreEqual("/v1.0/me/threads_publishing_limit", url.Path)

    match url.QueryParams.TryGetFirst("fields") with
    | true, fields ->
      let fields = (unbox<string> fields).Split(',')
      Assert.AreEqual(2, fields.Length)

      Assert.AreEqual(
        set [ "reply_quota_usage"; "reply_config" ],
        fields |> Set.ofArray
      )

    | _ -> Assert.Fail("Expected fields query parameter")

    match url.QueryParams.TryGetFirst("access_token") with

    | true, value -> Assert.AreEqual("fake_token", unbox<string> value)
    | _ -> Assert.Fail("Expected access token query parameter")

  }

  [<TestMethod>]
  member _.``Fetch conversation can encode and decode request correctly``
    ()
    : Task =
    task {
      use test = new HttpTest()

      test.RespondWithJson(Data.conversationResponse, 200) |> ignore

      let threads = Threads.Create("fake_token")

      let! conversation =
        threads.Replies.FetchConversation(
          "me",
          [
            Id
            Text
            Username
            Permalink
            Timestamp
            MediaProductType
            MediaType
            MediaUrl
            Shortcode
            ThumbnailUrl
            IsQuotePost
            HasReplies
            RootPost
            RepliedTo
            IsReply
            IsReplyOwnedByMe
            HideStatus
            ReplyAudience
          ],
          reverse = true
        )

      let expected = [
        ReplyFieldValue.Id "1234567890"
        ReplyFieldValue.Text "First Reply"
        ReplyFieldValue.Username "test"
        ReplyFieldValue.Permalink(Uri("https://example.com"))
        ReplyFieldValue.Timestamp(
          DateTimeOffset.Parse("2024-01-01T18:20:00+0000")
        )
        ReplyFieldValue.MediaProductType Threads
        ReplyFieldValue.MediaType TextPost
        ReplyFieldValue.MediaUrl(Uri("https://example.com"))
        ReplyFieldValue.Shortcode "abcdefg"
        ReplyFieldValue.ThumbnailUrl(Uri("https://example.com"))
        ReplyFieldValue.IsQuotePost false
        ReplyFieldValue.HasReplies true
        ReplyFieldValue.RootPost { id = "1234567890" }
        ReplyFieldValue.RepliedTo { id = "1234567890" }
        ReplyFieldValue.IsReply true
        ReplyFieldValue.IsReplyOwnedByMe true
        ReplyFieldValue.HideStatus NotHushed
        ReplyFieldValue.ReplyAudience Everyone
      ]

      Assert.AreEqual(expected, conversation.data |> Seq.head |> Seq.toList)


      let url, method =
        test.CallLog
        |> Seq.head
        |> fun call -> (call.Request.Url, call.Request.Verb.Method)

      Assert.AreEqual("GET", method)
      Assert.AreEqual("/v1.0/me/conversation", url.Path)

      match url.QueryParams.TryGetFirst("fields") with
      | true, fields ->
        let fields = (unbox<string> fields).Split(',')
        Assert.AreEqual(18, fields.Length)

        Assert.AreEqual(
          set [
            "id"
            "text"
            "username"
            "permalink"
            "timestamp"
            "media_product_type"
            "media_type"
            "media_url"
            "shortcode"
            "thumbnail_url"
            "is_quote_post"
            "has_replies"
            "root_post"
            "replied_to"
            "is_reply"
            "is_reply_owned_by_me"
            "hide_status"
            "reply_audience"
          ],
          fields |> Set.ofArray
        )

      | _ -> Assert.Fail("Expected fields query parameter")

      match url.QueryParams.TryGetFirst("access_token") with
      | true, value -> Assert.AreEqual("fake_token", unbox<string> value)
      | _ -> Assert.Fail("Expected access token query parameter")


      match url.QueryParams.TryGetFirst("reverse") with
      | true, value -> Assert.AreEqual("true", unbox<string> value)
      | _ -> Assert.Fail("Expected reverse query parameter")

    }

  [<TestMethod>]
  member _.``Fetch Replies can encode and decode request correctly``() : Task = task {
    use test = new HttpTest()

    let userId = Guid.NewGuid().ToString()

    test.RespondWithJson(Data.repliesResponse userId, 200) |> ignore

    let threads = Threads.Create("fake_token")

    let! replies =
      threads.Replies.FetchReplies(
        userId,
        [
          Id
          Text
          Timestamp
          MediaProductType
          MediaType
          Shortcode
          HasReplies
          RootPost
          RepliedTo
          IsReply
          HideStatus
        ],
        reverse = false
      )

    let expected = [
      ReplyFieldValue.Id userId
      ReplyFieldValue.Text "First Reply"
      ReplyFieldValue.Timestamp(
        DateTimeOffset.Parse("2024-01-01T18:20:00+0000")
      )
      ReplyFieldValue.MediaProductType Threads
      ReplyFieldValue.MediaType TextPost
      ReplyFieldValue.Shortcode "abcdefg"
      ReplyFieldValue.HasReplies true
      ReplyFieldValue.RootPost { id = "1234567890" }
      ReplyFieldValue.RepliedTo { id = "1234567890" }
      ReplyFieldValue.IsReply true
      ReplyFieldValue.HideStatus NotHushed
    ]

    Assert.AreEqual(expected, replies.data |> Seq.head |> Seq.toList)

    let url, method =
      test.CallLog
      |> Seq.head
      |> fun call -> (call.Request.Url, call.Request.Verb.Method)


    Assert.AreEqual("GET", method)
    Assert.AreEqual($"/v1.0/{userId}/replies", url.Path)

    match url.QueryParams.TryGetFirst("fields") with
    | true, fields ->
      let fields = (unbox<string> fields).Split(',')
      Assert.AreEqual(11, fields.Length)

      Assert.AreEqual(
        set [
          "id"
          "text"
          "timestamp"
          "media_product_type"
          "media_type"
          "shortcode"
          "has_replies"
          "root_post"
          "replied_to"
          "is_reply"
          "hide_status"
        ],
        fields |> Set.ofArray
      )

    | _ -> Assert.Fail("Expected fields query parameter")

    match url.QueryParams.TryGetFirst("access_token") with
    | true, value -> Assert.AreEqual("fake_token", unbox<string> value)
    | _ -> Assert.Fail("Expected access token query parameter")


    match url.QueryParams.TryGetFirst("reverse") with
    | true, _ -> Assert.Fail("Expected reverse query parameter to not be there")
    | _ -> ()
  }

  [<TestMethod>]
  member _.``Fetch User Replies can encode and decode the request correctly``
    ()
    : Task =
    task {
      use test = new HttpTest()

      let userId = Guid.NewGuid().ToString()

      test.RespondWithJson(Data.repliesResponse userId, 200) |> ignore

      let threads = Threads.Create("fake_token")

      let! userReplies =
        threads.Replies.FetchUserReplies(
          "me",
          [
            Id
            Text
            Timestamp
            MediaProductType
            MediaType
            Shortcode
            HasReplies
            RootPost
            RepliedTo
            IsReply
            HideStatus
          ]
        )

      let expected = [
        ReplyFieldValue.Id userId
        ReplyFieldValue.Text "First Reply"
        ReplyFieldValue.Timestamp(
          DateTimeOffset.Parse("2024-01-01T18:20:00+0000")
        )
        ReplyFieldValue.MediaProductType Threads
        ReplyFieldValue.MediaType TextPost
        ReplyFieldValue.Shortcode "abcdefg"
        ReplyFieldValue.HasReplies true
        ReplyFieldValue.RootPost { id = "1234567890" }
        ReplyFieldValue.RepliedTo { id = "1234567890" }
        ReplyFieldValue.IsReply true
        ReplyFieldValue.HideStatus NotHushed
      ]

      Assert.AreEqual(expected, userReplies.data |> Seq.head |> Seq.toList)


      let url, method =
        test.CallLog
        |> Seq.head
        |> fun call -> (call.Request.Url, call.Request.Verb.Method)


      Assert.AreEqual("GET", method)
      Assert.AreEqual("/v1.0/me/replies", url.Path)

      match url.QueryParams.TryGetFirst("fields") with

      | true, fields ->
        let fields = (unbox<string> fields).Split(',')
        Assert.AreEqual(11, fields.Length)

        Assert.AreEqual(
          set [
            "id"
            "text"
            "timestamp"
            "media_product_type"
            "media_type"
            "shortcode"
            "has_replies"
            "root_post"
            "replied_to"
            "is_reply"
            "hide_status"
          ],
          fields |> Set.ofArray
        )
      | _ -> Assert.Fail("Expected fields query parameter")

      match url.QueryParams.TryGetFirst("access_token") with
      | true, value -> Assert.AreEqual("fake_token", unbox<string> value)
      | _ -> Assert.Fail("Expected access token query parameter")

    }

  [<TestMethod>]
  member _.``Manage Reply can encode and decode hide = true reply request correctly``
    ()
    : Task =
    task {

      use test = new HttpTest()

      test.RespondWithJson({| success = true |}) |> ignore

      let threads = Threads.Create("fake_token")

      let! result = threads.Replies.ManageReply("1234567890", true)

      Assert.IsTrue(result)

      let url, method =
        test.CallLog
        |> Seq.head
        |> fun call -> (call.Request.Url, call.Request.Verb.Method)

      Assert.AreEqual("POST", method)
      Assert.AreEqual("/v1.0/1234567890/manage_reply", url.Path)

      match url.QueryParams.TryGetFirst("access_token") with
      | true, value -> Assert.AreEqual("fake_token", unbox<string> value)
      | _ -> Assert.Fail("Expected access token query parameter")

      match url.QueryParams.TryGetFirst("hide") with
      | true, value -> Assert.AreEqual("true", unbox<string> value)
      | _ -> Assert.Fail("Expected hide query parameter")
    }

  [<TestMethod>]
  member _.``Manage Reply can encode and decode hide = false reply request correctly``
    ()
    : Task =
    task {
      use test = new HttpTest()

      test.RespondWithJson({| success = true |}) |> ignore

      let threads = Threads.Create("fake_token")

      let! result = threads.Replies.ManageReply("1234567890", false)

      Assert.IsTrue(result)

      let url, method =
        test.CallLog
        |> Seq.head
        |> fun call -> (call.Request.Url, call.Request.Verb.Method)

      Assert.AreEqual("POST", method)
      Assert.AreEqual("/v1.0/1234567890/manage_reply", url.Path)

      match url.QueryParams.TryGetFirst("access_token") with
      | true, value -> Assert.AreEqual("fake_token", unbox<string> value)
      | _ -> Assert.Fail("Expected access token query parameter")

      match url.QueryParams.TryGetFirst("hide") with
      | true, value -> Assert.AreEqual("false", unbox<string> value)
      | _ -> Assert.Fail("Expected hide query parameter")
    }
