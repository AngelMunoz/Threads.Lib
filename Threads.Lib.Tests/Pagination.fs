namespace Threads.Lib.Tests

open System
open System.Threading.Tasks
open Flurl.Util
open Flurl.Http.Testing
open Microsoft.VisualStudio.TestTools.UnitTesting
open Threads.Lib
open Threads.Lib.Common

[<TestClass>]
type PaginationTests() =


  [<TestMethod>]
  member _.``PaginationKind.toStringTuple Cursor should return a list of tuples``
    ()
    =
    let cursor =
      Cursor [ Before "before"; After "after"; CursorParam.Limit 10u ]

    let result = PaginationKind.toStringTuple cursor

    let expected = [ "limit", "10"; "after", "after"; "before", "before" ]

    Assert.AreEqual(expected, result)

  [<TestMethod>]
  member _.``PaginationKind.toStringTuple Time should return a list of tuples``
    ()
    =
    let until = DateTimeOffset.Now
    let since = DateTimeOffset.Now.Subtract(TimeSpan.FromDays(1.0))
    let time = Time [ Until until; Since since; TimeParam.Limit 10u ]

    let result = PaginationKind.toStringTuple time

    let expected = [
      "limit", "10"
      "since", $"%i{since.ToUnixTimeMilliseconds()}"
      "until", $"%i{until.ToUnixTimeMilliseconds()}"
    ]

    Assert.AreEqual(expected, result)

  [<TestMethod>]
  member _.``PaginationKind.toStringTuple Offset should return a list of tuples``
    ()
    =
    let offset = Offset [ OffsetParam.Offset 10u; OffsetParam.Limit 10u ]
    let result = PaginationKind.toStringTuple offset

    let expected = [ "limit", "10"; "offset", "10" ]

    Assert.AreEqual(expected, result)
