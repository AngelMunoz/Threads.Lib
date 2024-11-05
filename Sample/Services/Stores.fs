namespace Sample.Services

open System
open System.Diagnostics

open Threads.Lib
open FSharp.Data.Adaptive

open Navs.Avalonia

open Sample
open Sample.Services.Threads

type ProfileStore = {
  profile: aval<UserProfile option>
  setProfile: UserProfile option -> unit
  loadProfile: unit -> Async<Unit>
  navigateToProfile: unit -> unit
}

module ProfileStore =

  let create(threads: ThreadsService) =
    let profile = cval None

    {
      profile = profile
      setProfile = profile.setValue
      loadProfile =
        fun () -> async {
          let! loaded = threads.loadProfile()
          loaded |> Some |> profile.setValue
          return ()
        }
      navigateToProfile =
        fun () ->
          let username =
            profile
            |> AVal.map(fun p ->
              let username = p |> Option.map(fun p -> p.username)
              defaultArg username "")

          Process.Start(
            ProcessStartInfo(
              $"https://threads.net/@{username |> AVal.force}",
              UseShellExecute = true
            )
          )
          |> ignore
    }


type UserThreadsStore = {
  loadUserThreads: unit -> Async<unit>
  userThreads: aval<Post list>
  postThread: PostParameters -> Async<unit>
  nextPage: unit -> Async<Unit>
  previousPage: unit -> Async<Unit>
}

module UserThreadsStore =
  let create(threads: ThreadsService) =
    let userThreads = cval []
    let currentPagination: cval<Pagination option> = cval None

    {
      userThreads = userThreads
      loadUserThreads =
        fun () -> async {
          let! (posts, next) = threads.fetchUserThreads()
          userThreads.setValue posts
          currentPagination.setValue(Some next)
          return ()
        }
      postThread =
        fun postParams -> async {
          let! post = threads.postThread postParams
          userThreads.setValue(post :: userThreads.Value)
          return ()
        }

      nextPage =
        fun () -> async {
          let pagination = currentPagination |> AVal.force

          match pagination with
          | None -> return ()
          | Some pagination ->
            let! (posts, next) =
              threads.fetchUserThreads(
                pagination = {
                  pagination with
                      next = pagination.next
                }
              )

            userThreads.setValue posts
            currentPagination.setValue(Some next)
            return ()
        }

      previousPage =
        fun () -> async {
          let pagination = currentPagination |> AVal.force

          match pagination with
          | None -> return ()
          | Some pagination ->
            let! (posts, previous) =
              threads.fetchUserThreads(
                pagination = {
                  pagination with
                      previous = pagination.previous
                }
              )

            userThreads.setValue posts
            currentPagination.setValue(Some previous)
            return ()
        }
    }


type MetricsStore = {
  range: aval<DataRange>
  metrics: aval<Metric list>
  loadMetrics: unit -> Async<unit>
  updateRange: DataRange -> Async<Unit>
}


module MetricsStore =

  let create(threads: ThreadsService) =
    let metrics = cval []
    let range = cval Week

    {
      metrics = metrics
      range = range
      loadMetrics =
        fun () -> async {
          let range = range |> AVal.force
          let! foundMetrics = threads.loadUserInsights range
          foundMetrics |> metrics.setValue
          return ()
        }
      updateRange =
        fun newRange -> async {
          let! foundMetrics = threads.loadUserInsights newRange
          foundMetrics |> metrics.setValue
          range.setValue newRange
          return ()
        }
    }
