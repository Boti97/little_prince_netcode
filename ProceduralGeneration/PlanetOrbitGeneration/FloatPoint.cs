using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts
{
    public class FloatPoint
    {
        private float x;
        private float y;

        public FloatPoint(float x, float y)
        {
            X = x;
            Y = y;
        }

        public float X { get => x; set => x = value; }
        public float Y { get => y; set => y = value; }
    }
}