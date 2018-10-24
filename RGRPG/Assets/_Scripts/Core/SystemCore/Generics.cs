using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RGRPG.Core.Generics
{

    public class Pair<TFirst, TSecond>
    {
        public TFirst first;
        public TSecond second;

        public Pair()
        {
            this.first = default(TFirst);
            this.second = default(TSecond);
        }

        public Pair(TFirst first, TSecond second)
        {
            this.first = first;
            this.second = second;
        }

        public Pair(Pair<TFirst, TSecond> other)
        {
            this.first = other.first;
            this.second = other.second;
        }

    }

    public struct PairStruct<TFirst, TSecond>
    {
        public TFirst first;
        public TSecond second;

        public PairStruct(TFirst first, TSecond second)
        {
            this.first = first;
            this.second = second;
        }

        public PairStruct(Pair<TFirst, TSecond> other)
        {
            this.first = other.first;
            this.second = other.second;
        }

    }




}