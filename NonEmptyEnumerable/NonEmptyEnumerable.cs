using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NonEmptyEnumerable
{
  /// <summary>
  /// An enumerable collection that cannot be empty.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class NonEmptyEnumerable<T> : IReadOnlyCollection<T>, IEquatable<NonEmptyEnumerable<T>> 
    where T : notnull
  {
    private readonly T _head;
    private readonly IEnumerable<T> _tail;

    /// <summary>
    /// Initialize a new instance of the <see cref="NonEmptyEnumerable{T}" /> class.
    /// </summary>
    /// <param name="head"></param>
    /// <param name="tail"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public NonEmptyEnumerable(T head, IEnumerable<T> tail)
    {
      _head = head ?? throw new ArgumentNullException(nameof(head));
      _tail = tail ?? throw new ArgumentNullException(nameof(tail));
    }

    /// <summary>
    /// Create an instance of <see cref="NonEmptyEnumerable{T}" /> with a single element.
    /// </summary>
    /// <param name="head"></param>
    public static NonEmptyEnumerable<T> Singleton(T head) => 
      new(head, Enumerable.Empty<T>());

    /// <summary>
    /// Create an instance of <see cref="NonEmptyEnumerable{T}" /> from an <see cref="IEnumerable{T}"/>.
    /// </summary>
    /// <param name="enumerable"></param>
    /// <exception cref="ArgumentException">
    ///   Occurs when <paramref name="enumerable" /> is null or empty.
    /// </exception>
    public static NonEmptyEnumerable<T> FromEnumerable(IEnumerable<T> enumerable)
    {
      if (enumerable is null || !enumerable.Any())
      {
        throw new ArgumentException("Cannot create a NonEmptyEnumerable from null or empty", nameof(enumerable));
      }

      var head = enumerable.First();
      var tail = enumerable.Skip(1);

      return new NonEmptyEnumerable<T>(head, tail);
    }

    /// <summary>
    /// Get the first element in the enumerable.
    /// </summary>
    public T Head() => _head;

    /// <summary>
    /// Get all elements after the first element in the enumerable.
    /// </summary>
    public IEnumerable<T> Tail() => _tail;

    /// <summary>
    /// Projects each element of a sequence into a new form. 
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="selector"></param>
    /// <returns>
    ///   A <see cref="NonEmptyEnumerable{T}"/> whose elements are the result of invoking the transform function on each element.
    /// </returns>
    public NonEmptyEnumerable<TResult> Select<TResult>(Func<T, TResult> selector) where TResult : notnull =>
      new(selector(_head), _tail.Select(selector));

    /// <summary>
    /// Projects each element of a sequence to a <see cref="NonEmptyEnumerable{TResult}"/> and flattens the resulting sequences into one sequence.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="selector"></param>
    /// <returns>
    ///    A <see cref="NonEmptyEnumerable{T}"/> whose elements are the result of invoking the one-to-many transform function on each element of the input sequence.
    /// </returns>
    public NonEmptyEnumerable<TResult> SelectMany<TResult>(Func<T, NonEmptyEnumerable<TResult>> selector) 
      where TResult : notnull
    {
      var headResult = selector(_head);
      var newHead = headResult.Head();
      var firstTail = headResult.Tail();
      var secondTail = _tail.SelectMany(selector);

      return new NonEmptyEnumerable<TResult>(newHead, firstTail.Concat(secondTail));
    }

    /// <summary>
    /// Concatenates two <see cref="NonEmptyEnumerable{T}"/>.
    /// </summary>
    /// <param name="enumerable"></param>
    /// <returns>
    ///   A <see cref="NonEmptyEnumerable{T}"/> that contains the concatenated elements of two sequences.
    /// </returns>
    public NonEmptyEnumerable<T> Concat(NonEmptyEnumerable<T> enumerable) => 
      new(_head, _tail.Concat(enumerable));

    /// <summary>
    /// Adds a value as the new head of a <see cref="NonEmptyEnumerable{T}"/>.
    /// </summary>
    /// <param name="newHead"></param>
    /// <returns>
    ///   a <see cref="NonEmptyEnumerable{T}"/> with the new head.
    /// </returns>
    public NonEmptyEnumerable<T> Cons(T newHead) => 
      new(newHead, AsEnumerable());

    /// <summary>
    /// Inverts the order of the elements in the <see cref="NonEmptyEnumerable{T}"/>.
    /// </summary>
    /// <returns>
    ///   A <see cref="NonEmptyEnumerable{T}"/> whose elements are correspond to those of the original sequence in reverse order.
    /// </returns>
    public NonEmptyEnumerable<T> Reverse() =>
      FromEnumerable(AsEnumerable().Reverse());

    /// <summary>
    /// Sorts the elements of a <see cref="NonEmptyEnumerable{T}"/> in ascending order according to a key.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="keySelector"></param>
    /// <returns>
    ///   A <see cref="NonEmptyEnumerable{T}"/> whose elements are sorted according to a key.
    /// </returns>
    public NonEmptyEnumerable<T> SortBy<TKey>(Func<T, TKey> keySelector) => 
      FromEnumerable(AsEnumerable().OrderBy(keySelector));

    /// <summary>
    /// Sorts the elements of a <see cref="NonEmptyEnumerable{T}"/> in descending order according to a key.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="keySelector"></param>
    /// <returns>
    ///   A <see cref="NonEmptyEnumerable{T}"/> whose elements are sorted in ascending order according to a key.
    /// </returns>
    public NonEmptyEnumerable<T> SortByDescending<TKey>(Func<T, TKey> keySelector) =>
      FromEnumerable(AsEnumerable().OrderByDescending(keySelector));

    /// <summary>
    /// Splits the elements in a <see cref="NonEmptyEnumerable{T}"/> into two <see cref="IEnumerable{T}"/> according to a predicate function.
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns>
    ///   A <see cref="Tuple"/> with the first <see cref="IEnumerable{T}"/> been elements that return <c>true</c> from the given predicate and the second
    ///   being elements that return <c>false</c> from the given predicate.
    /// </returns>
    public (IEnumerable<T> whenTrue, IEnumerable<T> whenFalse) Partition(Func<T, bool> predicate)
    {
      var lookup = AsEnumerable().ToLookup(predicate);
      return (lookup[true], lookup[false]);
    }

    /// <summary>
    /// Alternates elements of the list with copies of the provided <paramref name="value"/>.
    /// </summary>
    /// <param name="value"></param>
    public NonEmptyEnumerable<T> Intersperse(T value) =>
      SelectMany(x => FromEnumerable(new[] { value, x }));

    /// <summary>
    /// Similar to Aggregate, but returns intermediary and final results of applying the given function. 
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    /// <param name="state"></param>
    /// <param name="folder"></param>
    /// <returns>
    ///   A <see cref="NonEmptyEnumerable{T}"/> with all the intermediary and final results.
    /// </returns>
    public NonEmptyEnumerable<TState> Scan<TState>(TState state, Func<TState, T, TState> folder) where TState : notnull
    {
      var currentState = state;
      var accumulatingStates = new List<TState> { state };

      foreach (var element in AsEnumerable())
      {
        var nextState = folder(currentState, element);
        accumulatingStates.Add(nextState);
        currentState = nextState;
      }

      return new NonEmptyEnumerable<TState>(accumulatingStates.First(), accumulatingStates.Skip(1));
    }

    public IEnumerator<T> GetEnumerator() => AsEnumerable().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Return the current number of elements in the <see cref="NonEmptyEnumerable{T}"/>.
    /// </summary>
    public int Count => _tail.Count() + 1;

    public static bool operator ==(NonEmptyEnumerable<T> first, NonEmptyEnumerable<T> second) =>
      Equals(first, second);

    public static bool operator !=(NonEmptyEnumerable<T> first, NonEmptyEnumerable<T> second) =>
      !Equals(first, second);

    public static bool Equals(NonEmptyEnumerable<T> first, NonEmptyEnumerable<T> second) =>
      first.Equals(second);

    public bool Equals(NonEmptyEnumerable<T>? other) =>
      other switch
      {
        null => false,
        _ => 
          EqualityComparer<T>.Default.Equals(_head, other._head)
             && _tail.SequenceEqual(other._tail)
      };
      
    public override bool Equals(object? obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != this.GetType()) return false;
      return Equals((NonEmptyEnumerable<T>)obj);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        return (EqualityComparer<T>.Default.GetHashCode(_head) * 397) ^ _tail.GetHashCode();
      }
    }

    private IEnumerable<T> AsEnumerable() => new[] { _head }.Concat(_tail);
  }
}
