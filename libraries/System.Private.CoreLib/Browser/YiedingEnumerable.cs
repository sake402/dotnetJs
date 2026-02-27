//using System.Collections;
//using System.Collections.Generic;

//namespace System
//{
//    public delegate void YieldResult<T>(T Value, Yielder<T> _continue);
//    public delegate void YieldBreak();
//    public delegate void Yielder<T>(YieldResult<T> _yield, YieldBreak _break);
//    public class YiedingEnumerable<T> : IEnumerable<T>
//    {
//        Yielder<T> _yielder;
//        public YiedingEnumerable(Yielder<T> yielder)
//        {
//            _yielder = yielder;
//        }
//        public IEnumerator<T> GetEnumerator()
//        {
//            return new YiedingEnumerator(_yielder);
//        }

//        IEnumerator IEnumerable.GetEnumerator()
//        {
//            return GetEnumerator();
//        }

//        class YiedingEnumerator : IEnumerator<T>
//        {
//            Yielder<T> _yielder;
//            T? _current;
//            public YiedingEnumerator(Yielder<T> yielder)
//            {
//                _yielder = yielder;
//            }

//            public T Current => _current!;
//            object IEnumerator.Current => _current!;

//            public void Dispose()
//            {
//            }

//            bool broken = false;
//            Yielder<T>? _yieldContinue;
//            public bool MoveNext()
//            {
//                bool nextResult = false;
//                (_yieldContinue ?? _yielder)((result, _continue) =>
//                {
//                    _current = result;
//                    nextResult = true;
//                    _yieldContinue = _continue;
//                }, () =>
//                {
//                    broken = true;
//                    nextResult = false;
//                    _yieldContinue = null;
//                });
//                return nextResult;
//            }

//            public void Reset()
//            {
//            }
//        }
//    }
//}