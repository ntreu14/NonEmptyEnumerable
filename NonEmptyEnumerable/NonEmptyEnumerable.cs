using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NonEmptyEnumerable
{
  public class NonEmptyEnumerable<T> : IReadOnlyCollection<T>, IEnumerable<T> where T : notnull
  {
    private readonly T _head;
    private readonly IEnumerable<T> _tail;

    public NonEmptyEnumerable(T head, IEnumerable<T> tail)
    {
      _head = head ?? throw new ArgumentNullException(nameof(head));
      _tail = tail ?? throw new ArgumentNullException(nameof(tail));
    }
    
    public static NonEmptyEnumerable<T> Singleton(T head) => new NonEmptyEnumerable<T>(head, Enumerable.Empty<T>());

    public static NonEmptyEnumerable<T> FromEnumerable(IEnumerable<T> enumerable)
    {
      if (enumerable == null || !enumerable.Any())
      {
        throw new ArgumentException("Cannot create a NonEmptyEnumerable from null or empty", nameof(enumerable));
      }

      var head = enumerable.First();
      var tail = enumerable.Skip(1);

      return new NonEmptyEnumerable<T>(head, tail);
    }

    public T Head() => _head;

    public IEnumerable<T> Tail() => _tail;

    public NonEmptyEnumerable<TResult> Select<TResult>(Func<T, TResult> f) where TResult : notnull =>
      new NonEmptyEnumerable<TResult>(f(_head), _tail.Select(f));

    public NonEmptyEnumerable<TResult> SelectMany<TResult>(Func<T, NonEmptyEnumerable<TResult>> f) where TResult : notnull
    {
      var headResult = f(_head);
      var newHead = headResult.Head();
      var firstTail = headResult.Tail();
      var secondTail = _tail.SelectMany(f);

      return new NonEmptyEnumerable<TResult>(newHead, firstTail.Concat(secondTail));
    }

    public NonEmptyEnumerable<T> Concat(NonEmptyEnumerable<T> enumerable) =>
      new NonEmptyEnumerable<T>(_head, _tail.Concat(enumerable));

    public NonEmptyEnumerable<T> Cons(T newHead) =>
      new NonEmptyEnumerable<T>(newHead, AsEnumerable());

    public NonEmptyEnumerable<T> Reverse()
    {
      var reversed = AsEnumerable().Reverse();
      var head = reversed.First();
      var tail = reversed.Skip(1);

      return new NonEmptyEnumerable<T>(head, tail);
    }

    public NonEmptyEnumerable<T> SortBy<TKey>(Func<T, TKey> keySelector) => 
      FromEnumerable(AsEnumerable().OrderBy(keySelector));

    public NonEmptyEnumerable<T> SortByDescending<TKey>(Func<T, TKey> keySelector) =>
      FromEnumerable(AsEnumerable().OrderByDescending(keySelector));

    public (IEnumerable<T> whenTrue, IEnumerable<T> whenFalse) Partition(Func<T, bool> predicate)
    {
      var lookup = AsEnumerable().ToLookup(predicate);
      return (lookup[true], lookup[false]);
    }

    public NonEmptyEnumerable<T> Intersperse(T value) =>
      FromEnumerable(AsEnumerable().SelectMany(x => new [] { value, x }));

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

    public int Count => _tail.Count() + 1;

    protected bool Equals(NonEmptyEnumerable<T> other) =>
      EqualityComparer<T>.Default.Equals(_head, other._head) && Equals(_tail, other._tail);

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
