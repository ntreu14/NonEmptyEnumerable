namespace NonEmptyEnumerable.Specs

module Specs =
  open System
  open System.Linq
  open System.Collections.Generic
  open Expecto
  open Expect
  open FsCheck
  open NonEmptyEnumerable

  [<Tests>]
  let specs: Test =
    testList "NonEmptyEnumerable specs" 
      [
        testList "Constructing an NonEmptyEnumerable" [
          
          testCase "with both arguments as null" <| fun _ ->
            throwsT<ArgumentNullException> (fun () -> NonEmptyEnumerable(null, null) |> ignore) "throws an exception"

          testProperty "with only the first argument null" <| fun (enumerable: obj list) ->
            let tail = enumerable :> IEnumerable<obj>
            throwsT<ArgumentNullException> (fun () -> NonEmptyEnumerable(null, tail) |> ignore) "throws an ArgumentNullException"
               
          testProperty "with only the second argument null" <| fun head ->
            throwsT<ArgumentNullException> (fun () -> NonEmptyEnumerable(head, null) |> ignore) "throws an ArgumentNullException"
          
          testProperty "Singleton" <| fun (NonNull head) ->
            let singleton = NonEmptyEnumerable.Singleton head
            
            Expect.equal (singleton.Head ()) head "the head is equal to the head used to create it"
            Expect.isEmpty (singleton.Tail ()) "the tail is empty on a singleton"

          testProperty "FromEnumerable not empty" <| fun (NonEmptyArray (arr : obj NonNull [])) ->
            let xs = NonEmptyEnumerable.FromEnumerable arr

            Expect.equal (Array.head arr) (xs.Head ()) "the heads are equal"
            Expect.sequenceEqual (Array.tail arr) (xs.Tail ()) "the tails are equal"

          testCase "FromEnumerable with null" <| fun _ -> 
             throwsT<ArgumentException> (fun () -> NonEmptyEnumerable.FromEnumerable null |> ignore) "throws an ArgumentException"

          testCase "FromEnumerable with Empty" <| fun _ -> 
            throwsT<ArgumentException> (fun () -> NonEmptyEnumerable.FromEnumerable (Enumerable.Empty<obj> ()) |> ignore) "throws an ArgumentException"  
        ]

        testProperty "Head" <| fun (NonEmptyArray (arr : obj NonNull [])) ->
          let xs = NonEmptyEnumerable.FromEnumerable arr
          Expect.equal (xs.Head ()) (Array.head arr) "the heads are equal"

        testProperty "Tail" <| fun (NonEmptyArray (arr : obj NonNull [])) ->
          let xs = NonEmptyEnumerable.FromEnumerable arr
          Expect.equal (Array.ofSeq <| xs.Tail ()) (Array.tail arr) "the tails are equal"
        
        testProperty "Select" <| fun (NonEmptyArray (enumerable : int [])) ->
          let xs = NonEmptyEnumerable.FromEnumerable enumerable
          let add1 = (+) 1

          let mappedArray = Array.map add1 enumerable
          let mappedNonEmptyList = xs.Select add1

          Expect.equal (Array.ofSeq <| mappedNonEmptyList.ToList ()) mappedArray "the mapped lists are the same"

        testProperty "SelectMany" <| fun (NonEmptyArray (enumerable : PositiveInt [])) ->
          let justInts = Array.map (function PositiveInt i -> i) enumerable
          
          let xs = NonEmptyEnumerable.FromEnumerable justInts
          let toManyInts n = [|0..n|] |> NonEmptyEnumerable.FromEnumerable

          let mappedArray = justInts |> Array.collect (Array.ofSeq << toManyInts)
          let mappedNonEmptyList = xs.SelectMany toManyInts

          Expect.equal (Array.ofSeq <| mappedNonEmptyList.ToList ()) mappedArray "the collected lists are the same"
      
        testProperty "Count" <| fun (NonEmptyArray (arr : obj NonNull [])) ->
          let xs = NonEmptyEnumerable.FromEnumerable arr

          Expect.equal (xs.Count) (Array.length arr) "the counts are equal"
      ]

