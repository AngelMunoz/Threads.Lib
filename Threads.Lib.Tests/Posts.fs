namespace Threads.Lib.Tests

open System
open System.Threading.Tasks
open Flurl.Util
open Microsoft.VisualStudio.TestTools.UnitTesting
open Flurl.Http.Testing

open Threads.Lib
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

  [<TestMethod>]
  member _.``Post container creates an image container``() : Task = task {
    use test = new HttpTest()

    let suppliedId = Guid.NewGuid().ToString()

    test.RespondWithJson({ id = suppliedId }) |> ignore

    let threads = Threads.Lib.Threads.Create("fake_token")

    let! response =
      threads.Posts.PostContainer(
        "me",
        [ ImageUrl(new Uri("http://example.com/image.jpg")); MediaType Image ]
      )

    let url, method =
      test.CallLog
      |> Seq.head
      |> fun call -> (call.Request.Url, call.Request.Verb.Method)

    Assert.AreEqual("POST", method)
    Assert.AreEqual("/v1.0/me/threads", url.Path)

    match url.QueryParams.TryGetFirst("image_url") with
    | true, value ->
      Assert.AreEqual("http://example.com/image.jpg", unbox value)
    | _ -> Assert.Fail("image_url parameter not found")

    match url.QueryParams.TryGetFirst("media_type") with
    | true, value -> Assert.AreEqual("IMAGE", unbox value)
    | _ -> Assert.Fail("media_type parameter not found")

    match url.QueryParams.TryGetFirst("is_carousel_item") with
    | true, value -> Assert.AreEqual("false", unbox value)
    | _ -> Assert.Fail("is_carousel_item parameter not found")

    match url.QueryParams.TryGetFirst("access_token") with
    | true, value -> Assert.AreEqual("fake_token", unbox value)
    | _ -> Assert.Fail("fake_token parameter not found")

    Assert.AreEqual(suppliedId, response.id)
  }

  [<TestMethod>]
  member _.``Post container creates a video container``() : Task = task {
    use test = new HttpTest()

    let suppliedId = Guid.NewGuid().ToString()

    test.RespondWithJson({ id = suppliedId }) |> ignore

    let threads = Threads.Lib.Threads.Create("fake_token")

    let! response =
      threads.Posts.PostContainer(
        "me",
        [ VideoUrl(new Uri("http://example.com/video.mp4")); MediaType Video ]
      )

    let url, method =
      test.CallLog
      |> Seq.head
      |> fun call -> (call.Request.Url, call.Request.Verb.Method)

    Assert.AreEqual("POST", method)
    Assert.AreEqual("/v1.0/me/threads", url.Path)

    match url.QueryParams.TryGetFirst("video_url") with
    | true, value ->
      Assert.AreEqual("http://example.com/video.mp4", unbox value)
    | _ -> Assert.Fail("video_url parameter not found")

    match url.QueryParams.TryGetFirst("media_type") with
    | true, value -> Assert.AreEqual("VIDEO", unbox value)
    | _ -> Assert.Fail("media_type parameter not found")

    match url.QueryParams.TryGetFirst("is_carousel_item") with
    | true, value -> Assert.AreEqual("false", unbox value)
    | _ -> Assert.Fail("is_carousel_item parameter not found")

    match url.QueryParams.TryGetFirst("access_token") with
    | true, value -> Assert.AreEqual("fake_token", unbox value)
    | _ -> Assert.Fail("fake_token parameter not found")

    Assert.AreEqual(suppliedId, response.id)
  }

  [<TestMethod>]
  member _.``Post container fails to create an image container if the image URL is missing``
    ()
    : Task =
    task {
      use test = new HttpTest()

      let suppliedId = Guid.NewGuid().ToString()

      test.RespondWithJson({ id = suppliedId }) |> ignore

      let threads = Threads.Lib.Threads.Create("fake_token")

      let! response =
        Assert.ThrowsExceptionAsync<SingleContainerArgumentException>(fun () ->
          threads.Posts.PostContainer("me", [ MediaType Image ]))

      Assert.AreEqual(
        typeof<SingleContainerArgumentException>,
        response.GetType()
      )

    }

  [<TestMethod>]
  member _.``Post container fails to create a video container if the video URL is missing``
    ()
    : Task =
    task {
      use test = new HttpTest()

      let suppliedId = Guid.NewGuid().ToString()

      test.RespondWithJson({ id = suppliedId }) |> ignore

      let threads = Threads.Lib.Threads.Create("fake_token")

      let! response =
        Assert.ThrowsExceptionAsync<SingleContainerArgumentException>(fun () ->
          threads.Posts.PostContainer("me", [ MediaType Video ]))

      Assert.AreEqual(
        typeof<SingleContainerArgumentException>,
        response.GetType()
      )
    }

  [<TestMethod>]
  member _.``Post container fails to create a text container if the text is missing``
    ()
    : Task =
    task {
      use test = new HttpTest()

      let suppliedId = Guid.NewGuid().ToString()

      test.RespondWithJson({ id = suppliedId }) |> ignore

      let threads = Threads.Lib.Threads.Create("fake_token")

      let! response =
        Assert.ThrowsExceptionAsync<SingleContainerArgumentException>(fun () ->
          threads.Posts.PostContainer("me", [ MediaType MediaType.Text ]))

      Assert.AreEqual(
        typeof<SingleContainerArgumentException>,
        response.GetType()
      )
    }

  [<TestMethod>]
  member _.``Post container fails to create a carousel item container because it is not allowed``
    ()
    : Task =
    task {
      use test = new HttpTest()

      let suppliedId = Guid.NewGuid().ToString()

      test.RespondWithJson({ id = suppliedId }) |> ignore

      let threads = Threads.Lib.Threads.Create("fake_token")

      let! response =
        Assert.ThrowsExceptionAsync<SingleContainerArgumentException>(fun () ->
          threads.Posts.PostContainer("me", [ MediaType Carousel ]))

      Assert.AreEqual(
        typeof<SingleContainerArgumentException>,
        response.GetType()
      )
    }

  [<TestMethod>]
  member _.``Carousel item container can create an image container``() : Task = task {
    use test = new HttpTest()

    let suppliedId = Guid.NewGuid().ToString()

    test.RespondWithJson({ id = suppliedId }) |> ignore

    let threads = Threads.Lib.Threads.Create("fake_token")

    let! response =
      threads.Posts.PostCarouselItemContainer(
        "me",
        [ ImageUrl(new Uri("http://example.com/image.jpg")); MediaType Image ]
      )

    let url, method =
      test.CallLog
      |> Seq.head
      |> fun call -> (call.Request.Url, call.Request.Verb.Method)

    Assert.AreEqual("POST", method)
    Assert.AreEqual("/v1.0/me/threads", url.Path)

    match url.QueryParams.TryGetFirst("image_url") with
    | true, value ->
      Assert.AreEqual("http://example.com/image.jpg", unbox value)
    | _ -> Assert.Fail("image_url parameter not found")

    match url.QueryParams.TryGetFirst("is_carousel_item") with
    | true, value -> Assert.AreEqual("true", unbox value)
    | _ -> Assert.Fail("is_carousel_item parameter not found")

    match url.QueryParams.TryGetFirst("access_token") with
    | true, value -> Assert.AreEqual("fake_token", unbox value)
    | _ -> Assert.Fail("fake_token parameter not found")

    Assert.AreEqual(suppliedId, response.id)
  }

  [<TestMethod>]
  member _.``Carousel item container can create a video container``() : Task = task {
    use test = new HttpTest()

    let suppliedId = Guid.NewGuid().ToString()

    test.RespondWithJson({ id = suppliedId }) |> ignore

    let threads = Threads.Lib.Threads.Create("fake_token")

    let! response =
      threads.Posts.PostCarouselItemContainer(
        "me",
        [ VideoUrl(new Uri("http://example.com/video.mp4")); MediaType Video ]
      )

    let url, method =
      test.CallLog
      |> Seq.head
      |> fun call -> (call.Request.Url, call.Request.Verb.Method)

    Assert.AreEqual("POST", method)
    Assert.AreEqual("/v1.0/me/threads", url.Path)

    match url.QueryParams.TryGetFirst("video_url") with
    | true, value ->
      Assert.AreEqual("http://example.com/video.mp4", unbox value)
    | _ -> Assert.Fail("video_url parameter not found")

    match url.QueryParams.TryGetFirst("is_carousel_item") with
    | true, value -> Assert.AreEqual("true", unbox value)
    | _ -> Assert.Fail("is_carousel_item parameter not found")

    match url.QueryParams.TryGetFirst("access_token") with
    | true, value -> Assert.AreEqual("fake_token", unbox value)
    | _ -> Assert.Fail("fake_token parameter not found")

    Assert.AreEqual(suppliedId, response.id)
  }

  [<TestMethod>]
  member _.``Carousel item container fails to create an image container if the image URL is missing``
    ()
    : Task =
    task {
      use test = new HttpTest()

      let suppliedId = Guid.NewGuid().ToString()

      test.RespondWithJson({ id = suppliedId }) |> ignore

      let threads = Threads.Lib.Threads.Create("fake_token")

      let! response =
        Assert.ThrowsExceptionAsync<CarouselItemContainerArgumentException>
          (fun () ->
            threads.Posts.PostCarouselItemContainer("me", [ MediaType Image ]))

      Assert.AreEqual(
        typeof<CarouselItemContainerArgumentException>,
        response.GetType()
      )
    }

  [<TestMethod>]
  member _.``Carousel item container fails to create a video container if the video URL is missing``
    ()
    : Task =
    task {
      use test = new HttpTest()

      let suppliedId = Guid.NewGuid().ToString()

      test.RespondWithJson({ id = suppliedId }) |> ignore

      let threads = Threads.Lib.Threads.Create("fake_token")

      let! response =
        Assert.ThrowsExceptionAsync<CarouselItemContainerArgumentException>
          (fun () ->
            threads.Posts.PostCarouselItemContainer("me", [ MediaType Video ]))

      Assert.AreEqual(
        typeof<CarouselItemContainerArgumentException>,
        response.GetType()
      )
    }

  [<TestMethod>]
  member _.``Carousel item container fails to create a text container if the text is provided``
    ()
    : Task =
    task {
      use test = new HttpTest()

      let suppliedId = Guid.NewGuid().ToString()

      test.RespondWithJson({ id = suppliedId }) |> ignore

      let threads = Threads.Lib.Threads.Create("fake_token")

      let! response =
        Assert.ThrowsExceptionAsync<CarouselItemContainerArgumentException>
          (fun () ->
            threads.Posts.PostCarouselItemContainer(
              "me",
              [ Text "Hello, World!" ]
            ))

      Assert.AreEqual(
        typeof<CarouselItemContainerArgumentException>,
        response.GetType()
      )
    }

  [<TestMethod>]
  member _.``Carouse container can be created``() : Task = task {
    use test = new HttpTest()

    let suppliedId = Guid.NewGuid().ToString()

    test.RespondWithJson({ id = suppliedId }) |> ignore

    let threads = Threads.Lib.Threads.Create("fake_token")

    let children = [
      { id = Guid.NewGuid().ToString() }
      { id = Guid.NewGuid().ToString() }
    ]

    let! response = threads.Posts.PostCarousel("me", children)

    let url, method =
      test.CallLog
      |> Seq.head
      |> fun call -> (call.Request.Url, call.Request.Verb.Method)

    Assert.AreEqual("POST", method)
    Assert.AreEqual("/v1.0/me/threads", url.Path)

    match url.QueryParams.TryGetFirst("media_type") with
    | true, value -> Assert.AreEqual("CAROUSEL", unbox value)
    | _ -> Assert.Fail("media_type parameter not found")

    match url.QueryParams.TryGetFirst("children") with
    | true, value ->
      let children = unbox<string> value |> _.Split(",")
      let childIds = children |> Seq.toArray

      Array.zip childIds children
      |> Seq.iter(fun (expected, actual) -> Assert.AreEqual(expected, actual))
    | _ -> Assert.Fail("children parameter not found")

    match url.QueryParams.TryGetFirst("access_token") with
    | true, value -> Assert.AreEqual("fake_token", unbox value)
    | _ -> Assert.Fail("fake_token parameter not found")

    Assert.AreEqual(suppliedId, response.id)
  }

  [<TestMethod>]
  member _.``Carousel container can be created with text and children``
    ()
    : Task =
    task {
      use test = new HttpTest()

      let suppliedId = Guid.NewGuid().ToString()

      test.RespondWithJson({ id = suppliedId }) |> ignore

      let threads = Threads.Lib.Threads.Create("fake_token")

      let children = [
        { id = Guid.NewGuid().ToString() }
        { id = Guid.NewGuid().ToString() }
      ]

      let! response =
        threads.Posts.PostCarousel("me", children, "Hello, World!")

      let url, method =
        test.CallLog
        |> Seq.head
        |> fun call -> (call.Request.Url, call.Request.Verb.Method)

      Assert.AreEqual("POST", method)
      Assert.AreEqual("/v1.0/me/threads", url.Path)

      match url.QueryParams.TryGetFirst("media_type") with
      | true, value -> Assert.AreEqual("CAROUSEL", unbox value)
      | _ -> Assert.Fail("media_type parameter not found")

      match url.QueryParams.TryGetFirst("children") with
      | true, value ->
        let children = unbox<string> value |> _.Split(",")
        let childIds = children |> Seq.toArray

        Array.zip childIds children
        |> Seq.iter(fun (expected, actual) -> Assert.AreEqual(expected, actual))
      | _ -> Assert.Fail("children parameter not found")

      match url.QueryParams.TryGetFirst("text") with
      | true, value -> Assert.AreEqual("Hello, World!", unbox value)
      | _ -> Assert.Fail("text parameter not found")

      match url.QueryParams.TryGetFirst("access_token") with
      | true, value -> Assert.AreEqual("fake_token", unbox value)
      | _ -> Assert.Fail("fake_token parameter not found")

      Assert.AreEqual(suppliedId, response.id)
    }

  [<TestMethod>]
  member _.``Carousel container fails to create if there are no children``
    ()
    : Task =
    task {
      use test = new HttpTest()

      let suppliedId = Guid.NewGuid().ToString()

      test.RespondWithJson({ id = suppliedId }) |> ignore

      let threads = Threads.Lib.Threads.Create("fake_token")

      let! response =
        Assert.ThrowsExceptionAsync<CarouselContainerArgumentException>
          (fun () -> threads.Posts.PostCarousel("me", []))

      Assert.AreEqual(
        typeof<CarouselContainerArgumentException>,
        response.GetType()
      )
    }

  [<TestMethod>]
  member _.``Carousel container fails to create if there are more than 20 children``
    ()
    : Task =
    task {
      use test = new HttpTest()

      let suppliedId = Guid.NewGuid().ToString()

      test.RespondWithJson({ id = suppliedId }) |> ignore

      let threads = Threads.Lib.Threads.Create("fake_token")

      let children = [ for _ in 0..21 -> { id = Guid.NewGuid().ToString() } ]

      let! response =
        Assert.ThrowsExceptionAsync<CarouselContainerArgumentException>
          (fun () -> threads.Posts.PostCarousel("me", children))

      Assert.AreEqual(
        typeof<CarouselContainerArgumentException>,
        response.GetType()
      )
    }

  [<TestMethod>]
  member _.``Containers can be published``() : Task = task {
    use test = new HttpTest()

    let suppliedId = Guid.NewGuid().ToString()

    test.RespondWithJson({ id = suppliedId }) |> ignore

    let threads = Threads.Lib.Threads.Create("fake_token")

    let containerId = { id = Guid.NewGuid().ToString() }
    let! response = threads.Posts.PublishPost("me", containerId)

    let url, method =
      test.CallLog
      |> Seq.head
      |> fun call -> (call.Request.Url, call.Request.Verb.Method)

    Assert.AreEqual("POST", method)
    Assert.AreEqual("/v1.0/me/threads_publish", url.Path)

    match url.QueryParams.TryGetFirst("creation_id") with
    | true, value -> Assert.AreEqual(containerId.id, unbox value)
    | _ -> Assert.Fail("creation_id parameter not found")

    match url.QueryParams.TryGetFirst("access_token") with
    | true, value -> Assert.AreEqual("fake_token", unbox value)
    | _ -> Assert.Fail("fake_token parameter not found")

    Assert.AreEqual(suppliedId, response.id)

  }
