using dotnetJs;
using System.Collections;
using System.Collections.Generic;

namespace System
{
    internal class YieldToIterator<T> : IEnumerable<T>
    {
        [ObjectLiteral]
        internal class IteratorResult
        {
            [Name("value")]
            public T? Value { get; set; }
            [Name("done")]
            public bool Done { get; set; }
        }

        internal interface IGenerator
        {
            [Name("next")]
            IteratorResult Next();
        }

        Func<IGenerator> _getGenerator;
        internal YieldToIterator(Func<IGenerator> getGenerator)
        {
            _getGenerator = getGenerator;
        }
        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(_getGenerator());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        class Enumerator : IEnumerator<T>
        {
            IGenerator _generator;
            T? _current;
            public Enumerator(IGenerator generator)
            {
                _generator = generator;
            }

            public T Current => _current!;
            object IEnumerator.Current => _current!;

            public void Dispose()
            {
            }

            bool alreadyDone;
            public bool MoveNext()
            {
                if (alreadyDone)
                    return false;
                var nxt = _generator.Next();
                alreadyDone = nxt.Done;
                _current = nxt.Value;
                return true;
            }

            public void Reset()
            {
            }
        }
    }
}