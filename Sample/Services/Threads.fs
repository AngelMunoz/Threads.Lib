namespace Sample.Services

open System
open System.Threading.Tasks
open IcedTasks
open IcedTasks.Polyfill.Async
open IcedTasks.Polyfill.Task

open Threads.Lib

open Sample

type ThreadsService =
  abstract member loadProfile: unit -> Async<UserProfile>

  abstract member fetchUserThreads:
    ?pagination: Pagination * ?limit: int -> Async<Post list * Pagination>

  abstract member postThread: postParams: PostParameters -> Async<Post>

  abstract member loadThread: id: string -> Async<Post>

  abstract member loadUserInsights: DataRange -> Async<Metric list>

module ThreadsService =

  let private defaultFetchMediaParams =
    lazy
      ([
        Media.ThreadField.Id
        Media.ThreadField.Username
        Media.ThreadField.Text
        Media.ThreadField.Timestamp
        Media.ThreadField.MediaUrl
        Media.ThreadField.MediaType
        Media.ThreadField.Owner
        Media.ThreadField.Permalink
      ])

  let private defaultFetchProfileParams =
    lazy
      ([
        Profiles.ProfileField.Id
        Profiles.ProfileField.Username
        Profiles.ProfileField.ThreadsBiography
        Profiles.ProfileField.ThreadsProfilePictureUrl
      ])

  let create(client: ThreadsClient) =
    { new ThreadsService with
        member _.loadProfile() = async {
          let! token = Async.CancellationToken

          let! response =
            client.Profile.FetchProfile(
              "me",
              defaultFetchProfileParams.Value,
              token
            )
            |> UserProfile.toProfile

          return response
        }

        member _.fetchUserThreads(?pagination, ?limit) = async {
          let! token = Async.CancellationToken

          return!
            client.Media.FetchThreads(
              "me",
              defaultFetchMediaParams.Value,
              pagination =
                Cursor [
                  match limit with
                  | None -> CursorParam.Limit 10u
                  | Some limit -> CursorParam.Limit(uint limit)

                  match pagination with
                  | None -> ()
                  | Some pagination ->
                    match pagination.next with
                    | Some next -> After next
                    | None -> ()

                    match pagination.previous with
                    | Some previous -> Before previous
                    | None -> ()
                ],
              cancellationToken = token
            )
            |> Post.ofPosts
        }

        member _.postThread postParams = async {
          let! token = Async.CancellationToken

          let! container =
            client.Posts.PostContainer(
              "me",
              [
                Posts.PostParam.MediaType postParams.mediaType
                match postParams.mediaType with
                | Posts.Image ->
                  Posts.PostParam.ImageUrl postParams.mediaUrl.Value
                | Posts.Video ->
                  Posts.PostParam.VideoUrl postParams.mediaUrl.Value
                | _ -> ()
                Posts.PostParam.Text postParams.text
                Posts.PostParam.ReplyControl postParams.audience
              ],
              token
            )

          let! postId = client.Posts.PublishPost("me", container, token)

          return!
            client.Media.FetchThread(
              postId.id,
              defaultFetchMediaParams.Value,
              token
            )
            |> Post.ofPost
        }

        member this.loadThread id = async {
          let! token = Async.CancellationToken

          return!
            client.Media.FetchThread(id, defaultFetchMediaParams.Value, token)
            |> Post.ofPost
        }

        member _.loadUserInsights range = async {
          let! token = Async.CancellationToken

          let insightParams = [
            match range with
            | Week -> Insights.Since(DateTimeOffset.Now.AddDays(-7.))
            | Month -> Insights.Since(DateTimeOffset.Now.AddDays(-30.))
            | Year ->
              let targetDate = DateTimeOffset.Now.AddDays(-365.)

              let targetDate =
                if
                  targetDate < DateTimeOffset(
                    DateOnly(2024, 6, 2),
                    TimeOnly(0, 0),
                    TimeSpan.Zero
                  )
                then
                  DateTimeOffset(
                    DateOnly(2024, 6, 2),
                    TimeOnly(0, 0),
                    TimeSpan.Zero
                  )
                else
                  targetDate

              Insights.Since(targetDate)

            Insights.Until(DateTimeOffset.Now)
          ]

          let! response =
            client.Insights.FetchUserInsights(
              "me",
              [
                Insights.Views
                Insights.Likes
                Insights.Replies
                Insights.Reposts
                Insights.Quotes
                Insights.FollowerCount
              ],
              insightParams,
              token
            )
            |> Metric.ofMetricResponse

          return response
        }
    }
