﻿module Specs

open System
open System.Linq
open Expecto
open Expect
open FsCheck
open NonEmptyEnumerable

let fromNonNullToObjs = Array.map (function NonNull x -> x)
let nonEmptyFromNonNullObjs = NonEmptyEnumerable.FromEnumerable << fromNonNullToObjs

[<Tests>]
let specs: Test =
  testList "NonEmptyEnumerable specs" [

    testList "constructing an NonEmptyEnumerable" [
      testCase "with both arguments as null" <| fun _ ->
        throwsT<ArgumentNullException> (fun () -> NonEmptyEnumerable(null, null) |> ignore) "throws an ArgumentNullException"

      testProperty "with only the first argument null" <| fun (NonEmptyArray (tail: obj array)) ->
        throwsT<ArgumentNullException> (fun () -> NonEmptyEnumerable(null, tail) |> ignore) "throws an ArgumentNullException"
               
      testProperty "with only the second argument null" <| fun (NonNull head) ->
        throwsT<ArgumentNullException> (fun () -> NonEmptyEnumerable(head, null) |> ignore) "throws an ArgumentNullException"
          
      testCase "Singleton with null" <| fun _ ->
        throwsT<ArgumentNullException> (fun () -> NonEmptyEnumerable.Singleton null |> ignore) "throws an ArgumentNullException"

      testProperty "Singleton" <| fun (NonNull head) ->
        let singleton = NonEmptyEnumerable.Singleton head
            
        Expect.equal (singleton.Head ()) head "the head is equal to the head used to create it"
        Expect.isEmpty (singleton.Tail ()) "the tail is empty on a singleton"

      testProperty "FromEnumerable not empty" <| fun (NonEmptyArray (arr: obj NonNull array)) ->
        let xs = NonEmptyEnumerable.FromEnumerable arr

        Expect.equal (xs.Head ()) (Array.head arr) "the heads are equal"
        Expect.sequenceEqual (xs.Tail ()) (Array.tail arr) "the tails are equal"

      testCase "FromEnumerable with null" <| fun _ -> 
        throwsT<ArgumentException> (fun () -> NonEmptyEnumerable.FromEnumerable null |> ignore) "throws an ArgumentException"

      testCase "FromEnumerable with an empty enumerable" <| fun _ -> 
        throwsT<ArgumentException> (fun () -> NonEmptyEnumerable.FromEnumerable (Enumerable.Empty<obj> ()) |> ignore) "throws an ArgumentException"  
    ]

    testList "core functionality" [
        testProperty "Head" <| fun (NonEmptyArray (arr: obj NonNull array)) ->
          let xs = NonEmptyEnumerable.FromEnumerable arr
          Expect.equal (xs.Head ()) (Array.head arr) "the heads are equal"

        testProperty "Tail" <| fun (NonEmptyArray (arr: obj NonNull array)) ->
          let xs = NonEmptyEnumerable.FromEnumerable arr
          Expect.sequenceEqual (xs.Tail ()) (Array.tail arr) "the tails are equal"

        testProperty "Init" <| fun (NonEmptyArray (arr: obj NonNull array)) -> 
          let xs = NonEmptyEnumerable.FromEnumerable arr
          Expect.sequenceEqual (xs.Init ()) (arr.SkipLast 1) "inits are equal"
            
        testProperty "Select" <| fun (NonEmptyArray (enumerable: int array)) ->
          let xs = NonEmptyEnumerable.FromEnumerable enumerable
          let add1 = (+) 1

          let mappedArray = Array.map add1 enumerable
          let mappedNonEmptyArray = xs.Select add1

          Expect.sequenceEqual mappedNonEmptyArray mappedArray "the mapped enumerables are the same"

        testProperty "SelectMany" <| fun (NonEmptyArray (enumerable: PositiveInt array)) ->
          let justInts = Array.map (function PositiveInt i -> i) enumerable
          let xs = NonEmptyEnumerable.FromEnumerable justInts

          let collectedArray = justInts |> Array.collect (fun n -> [|0..n|])
          let collectedNonEmptyList = xs.SelectMany (fun n -> [0..n] |> NonEmptyEnumerable.FromEnumerable)

          Expect.sequenceEqual collectedNonEmptyList collectedArray "the collected enumerables are the same"

        testProperty "Concat" <| fun (NonEmptyArray (arr: obj NonNull array)) ->
          let xs = NonEmptyEnumerable.FromEnumerable arr
          let appended = Array.append arr arr
          let nonEmptyConcated = xs.Concat xs

          Expect.sequenceEqual nonEmptyConcated appended "the concated enumerables are equal"

        testProperty "Cons" <| fun ((head: obj NonNull), NonEmptyArray (enumerable: obj NonNull array)) ->
          let xs = NonEmptyEnumerable.FromEnumerable enumerable
          let cons = xs.Cons head
          let expected = Array.append [| head |] enumerable

          Expect.sequenceEqual cons expected "the cons enumerables are equal"

        testProperty "Reverse" <| fun (NonEmptyArray (arr: obj NonNull array)) ->
          let xs = NonEmptyEnumerable.FromEnumerable arr
          let reversed = Array.rev arr
          let expected = xs.Reverse ()

          Expect.sequenceEqual reversed expected "the reversed enumerables are equal"

        testProperty "SortBy" <| fun (NonEmptyArray (arr: int array)) ->
          let xs = NonEmptyEnumerable.FromEnumerable arr
          let sorted = Array.sort arr
          let expected = xs.SortBy (fun i -> i)

          Expect.sequenceEqual sorted expected "the sorted enumerables are equal"

        testProperty "SortByDecending" <| fun (NonEmptyArray (arr: int array)) ->
          let xs = NonEmptyEnumerable.FromEnumerable arr
          let sorted = Array.sortDescending arr
          let expected = xs.SortByDescending (fun i -> i)

          Expect.sequenceEqual sorted expected "the sorted enumerables are equal"

        testList "Partition" [
          testProperty "Partition all true" <| fun (NonEmptyArray (arr: obj NonNull array)) ->
            let enumerable = NonEmptyEnumerable.FromEnumerable arr
            let struct (whenTrue, whenFalse) = enumerable.Partition (fun _ -> true)

            Expect.isEmpty whenFalse "the whenFalse is empty"
            Expect.sequenceEqual whenTrue enumerable "the whenTrue and original enumerable are the same"

          testProperty "Partition all false" <| fun (NonEmptyArray (arr: obj NonNull array)) ->
            let enumerable = NonEmptyEnumerable.FromEnumerable arr
            let struct (whenTrue, whenFalse) = enumerable.Partition (fun _ -> false)

            Expect.isEmpty whenTrue "the whenTrue is empty"
            Expect.sequenceEqual whenFalse enumerable "the whenFalse and original enumerable are the same"

          testCase "Partition" <| fun _ ->
            let enumerable = NonEmptyEnumerable.FromEnumerable [0..10]
            let struct (lessThan5, fiveAndGreater) = enumerable.Partition (fun i -> i < 5)
            
            Expect.sequenceEqual lessThan5 [0..4] "lessThan contains values that satisfy the predicate"
            Expect.sequenceEqual fiveAndGreater [5..10] "more than contains values that do not satisfy the predicate"
        ]

        testProperty "Intersperse" <| fun ((NonNull (v: obj)), NonEmptyArray (enumerable: obj NonNull array)) ->
          let objs = fromNonNullToObjs enumerable
          let nee = NonEmptyEnumerable.FromEnumerable objs
          let interspersed = nee.Intersperse v
          let expected = Array.collect (fun s -> [|v; s|]) objs

          Expect.sequenceEqual interspersed expected "the interspersed enumerables are equal"

        testProperty "Scan" <| fun (NonEmptyArray (arr: int array)) ->
          let nee = NonEmptyEnumerable.FromEnumerable arr
          let scaned = nee.Scan(0, fun a b -> a + b)
          let expected = Array.scan (+) 0 arr

          Expect.sequenceEqual scaned expected "the scanned enumerables are equal"
    ]

    testList "Equality tests" [
      testProperty "equality" <| fun (NonEmptyArray (enumerable: obj NonNull array)) ->
        let first = nonEmptyFromNonNullObjs enumerable
        let second = nonEmptyFromNonNullObjs enumerable

        Expect.isTrue (first = second) "two NonEmptyEnumerbales are equal with the same values"
        Expect.isTrue (first.Equals second) "two NonEmptyEnumerbales are equal with the same values"
        Expect.isFalse (first <> second) "two NonEmptyEnumerbales are not, not equal with the same values"
        Expect.equal first second "two NonEmptyEnumerbales are equal with the same values"

      testProperty "inequality" <| fun _ ->
        gen {
            let genNonEmptyArray = Arb.generate<NonEmptyArray<obj NonNull>>
            let! (NonEmptyArray enumerable1) = genNonEmptyArray
            let! (NonEmptyArray enumerable2) =
                genNonEmptyArray |> Gen.filter (fun (NonEmptyArray nonEmpty) -> nonEmpty <> enumerable1)

            let first = nonEmptyFromNonNullObjs enumerable1
            let second = nonEmptyFromNonNullObjs enumerable2

            Expect.isTrue (first <> second) "two NonEmptyEnumerbales are not equal with different values"
            Expect.isFalse (first.Equals second) "two NonEmptyEnumerbales are not equal with different values"
            Expect.isFalse (first = second) "two NonEmptyEnumerbales are not equal with different values"
            Expect.notEqual first second "two NonEmptyEnumerbales are not equal with different values"
        }

      testProperty "equality with null" <| fun (NonEmptyArray (enumerable: obj NonNull array)) ->
        let xs = nonEmptyFromNonNullObjs enumerable
        Expect.isFalse (xs.Equals null) "a NonEmptyEnumerable never equals null"
        Expect.notEqual xs null "a NonEmptyEnumerable never equals null"
    ]
  ]
