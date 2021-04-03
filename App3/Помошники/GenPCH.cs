using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UWP.Помошники
{
    class GenPCH
    {
        private int _period;
        private int t = 3;
        private int a = 557;
        private int c = 7229;
        private int m = 33;
        public GenPCH(string text,int a,int c,int t)
        {
            _period = text.Length;
            this.a = a;
            this.c = c;
            this.t = t;
        }

        public int[] Generate()
        {
            int[] numbers = new int[_period];
            if (_period == 0) throw new Error();
            numbers[0] = (a * t + c) % m;
            for(int i = 1; i < _period; i++)
            {
                numbers[i] = (a * numbers[i - 1] + c) % m;
            }
            return numbers;
        }
    }
}
