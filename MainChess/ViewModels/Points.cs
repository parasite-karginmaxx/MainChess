using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainChess.ViewModels
{
    internal class Points : ICloneable
    {
        private double x;
        public double X { get => x; set => x = value; }

        private double y;
        public double Y { get => y; set => y = value; }


        public Points(double x, double y)
        {
            X = x;
            Y = y;
        }

        public static Points operator /(Points a, double b)
        {
            return new Points(a.X / b, a.Y / b);
        }

        public static Points operator -(Points a, Points b)
        {
            return new Points(a.X - b.X, a.Y - b.Y);
        }

        public static Points operator *(Points a, double b)
        {
            return new Points(a.X * b, a.Y * b);
        }


        public static Points operator *(Points a, Points b)
        {
            return new Points(a.X * b.X, a.Y * b.Y);
        }

        public static Points operator +(Points a, Points b)
        {
            return new Points(a.X + b.X, a.Y + b.Y);
        }

        public static bool operator ==(Points a, Points b)
        {
            if (a is null)
                return b is null;
            else if (b is null)
                return a is null;


            return a.X == b.X && a.Y == b.Y;

        }
        public static bool operator !=(Points a, Points b)
        {

            return !(a == b);

        }

        public static bool CheckBound(Points boundCheck, int border)
        {
            return boundCheck.X != 0 && boundCheck.Y != 0 && boundCheck.X != border && boundCheck.Y != border ? true : false;

        }

        public static double GetDistence(Points a, Points b)
        {

            Points res = b - a;

            res.X *= res.X;
            res.Y *= res.Y;
            //size of one unit 1 on 1 so we convert the result to match our unit
            return Math.Sqrt(res.X + res.Y) / 1.4;


        }

        public override bool Equals(object obj)
        {


            return this == obj as Points;

        }

        public override int GetHashCode()
        {
            int x_Hash = (int)X + 100;
            int y_Hash = (int)Y + 100;

            int res = ((x_Hash + y_Hash) * (x_Hash + y_Hash + 1)) / 2;
            return y_Hash + res;
        }

        public object Clone()
        {
            return new Points(this.X, this.Y);
        }
    }
}