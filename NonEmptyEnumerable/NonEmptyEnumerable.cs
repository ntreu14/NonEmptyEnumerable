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

    public static NonEmptyEnumerable<T> Singleton(T head) =>
      new NonEmptyEnumerable<T>(head, Enumerable.Empty<T>());

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
      var a = headResult.Head();
      var firstTail = headResult.Tail();

      var secondTail = _tail.SelectMany(t => f(t).ToList());

      return new NonEmptyEnumerable<TResult>(a, firstTail.Concat(secondTail));
    }
    
    public IEnumerator<T> GetEnumerator() =>
      new [] { _head }.Concat(_tail).GetEnumerator();

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
        return (EqualityComparer<T>.Default.GetHashCode(_head) * 397) ^ (_tail != null ? _tail.GetHashCode() : 0);
      }
    }
  }
}
