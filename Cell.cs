using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

namespace LifeGame
{
    public struct CellLocation
    {
        public uint I;
        public uint J;
        public uint L;
        public CellLocation(uint l,uint i, uint j)
        {
            L = l;
            I = i;
            J = j;
        }

        public uint Quadrant
        {
            get { return J/I; }
        }
        public uint Position
        {
            get { return J % I; }
        }
        public uint PreviousJ
        {
            get { return J == 0 ? I * 6 - 1 : J-1; }
        }

        public uint NextJ
        {
            get { return J == I * 6 - 1 ? 0 : J+1; }
        }

        public Cell Cell
        {
            get { return Cell.CellMap[L,I][J]; }
        }

        public Cell NextCell
        {
            get { return Cell.CellMap[L, I][NextJ]; }
        }

        public Cell PreviousCell
        {
            get { return Cell.CellMap[L, I][PreviousJ]; }
        }

        public Cell UpCell
        {
            get { return Cell.CellMap[L<Cell.LayerCount -1?L+1:0, I][J]; }
        }

        public Cell DownCell
        {
            get { return Cell.CellMap[L>0?L-1:Cell.LayerCount-1, I][J]; }
        }
    }

    public class Rule
    {
        public bool Value { get; set; }
        public int Index { get; set; }
    }
    public class Cell
    {
        public const int Size = 1;
        public const int LayerCount = 3;
        public const int SizeCount = 100;
        public static Cell[,][] CellMap = new Cell[LayerCount,SizeCount][];
        public CellLocation Location;
        public readonly SolidColorBrush LifeBrush ;
        public static readonly SolidColorBrush DeadBrush = new SolidColorBrush(Colors.Black);
        //public static readonly bool?[] Rules = new bool?[64];
        public Cell[] AroundCells;
        public static List<int> SurvivalsRule = new List<int>();
        public static List<int> BirthsRule = new List<int>();
        public static int Generations { get; set; }
        public static int BackBufferStride;
        public int NextState;
        public Point Position;
        static Cell()
        {
           // Rules[0x00] = false;

            var survivalsRule = new int[0];//  { 0, 1, 2, 3, 4, 5, 6 };
            var birthsRule = new int[] {1, 2 };
            for (var i = 1; i < 64; i++)
            {
                var count = -1;
                var n = i;
                do count++; while ((n &= (n - 1))!=0);
                if (birthsRule.Contains(count))
                {
                    BirthsRule.Add(i);
                }
                if (survivalsRule.Contains(count))
                {
                    SurvivalsRule.Add(i);
                }
                //Rules[i] = rules[count];
                //Rules[i] = rand.Next(6) == 0;
            }
        }

        public static void ApplyRules(IEnumerable<int> survivalsRule, IEnumerable<int> birthsRule)
        {
            BirthsRule = new List<int>();
            SurvivalsRule = new List<int>();
            if (birthsRule.Contains(0))
            {
                BirthsRule.Add(0);
            }
            if (survivalsRule.Contains(0))
            {
                SurvivalsRule.Add(0);
            }
            for (var i = 1; i < 256; i++)
            {
                var count = 0;
                var n = i;
                do count++; while ((n &= (n - 1)) != 0);
                //Debug.WriteLine(Convert.ToString(i,2)+":"+count);
                if (birthsRule.Contains(count))
                {
                    BirthsRule.Add(i);
                }
                if (survivalsRule.Contains(count))
                {
                    SurvivalsRule.Add(i);
                }
            }
        }
        public int State;

        public void ChangeAlive()
        {
            State = NextState;
            //Debug.WriteLine(Location.I+","+Location.J+":"+Alive);
        }

        public Cell()
        {
            
        }
        public Cell(uint l ,uint x,uint y)
        {
            Location = new CellLocation(l,x,y);
            //Shape.Width = Size;
            //Shape.Height = Size;

            //for (var i = 0; i < 6; i++)
            //{
            //    var 弧度 = Math.PI * i / 3 + Math.PI / 6;
            //    Shape.Points.Add(new Point(Size * Math.Cos(弧度), Size * Math.Sin(弧度)));
            //}
            LifeBrush = new SolidColorBrush(Color.FromRgb((byte)(x * 15), (byte)(x * 15), (byte)(x * 15)));
            //Shape.Fill = new SolidColorBrush(Color.FromRgb((byte) (x*40),(byte) (y*40),(byte) (x*40)));
            //Shape.Fill = DeadBrush;
            Position = Location.I != 0 ? ToXY() : new Point(Size * SizeCount, Size * SizeCount);
            //Shape.SetValue(Canvas.LeftProperty,shapePosition.X);
            //Shape.SetValue(Canvas.TopProperty, shapePosition.Y);
            //Children.Add(new TextBlock(new Run( x + "," + y )) { Foreground = new SolidColorBrush(Colors.Gray), RenderTransform = new TranslateTransform(-Size+10, -Size+10) });
           // Shape.MouseDown += Cell_MouseDown;

        }

        public void SetNextState()
        {
            var i = AroundCells.Aggregate(0, (current, cell) => (current << 1) | (cell.State == 1 ? 1 : 0));
            switch (State)
            {
                case 1:
                    NextState = SurvivalsRule.Contains(i) ? 1 : -Generations;
                    break;
                case 0:
                    NextState = BirthsRule.Contains(i) ? 1 : 0;
                    break;
                default:
                    NextState = State + 1;
                    break;
            }
        }
        public bool ChangeLife(long point)
        {
            SetNextState();
            point += (int)Position.X * BackBufferStride + (int)Position.Y;
            unsafe
            {
                *((byte*)point) = (byte)(NextState == 1? 0xFF :NextState==0?0x00: -NextState*256/(Generations+2));
            }
            // _nextColor = Color.FromRgb((byte)(i * 3), (byte)(i * 2 + Location.I * 5), (byte)(i * (Location.I / 4)));
            return NextState != State;
        }
      
        public static void CreateCells()
        {
            for (uint l = 0; l < LayerCount; l++)
            {
                CellMap[l,0] = new Cell[1];
                CellMap[l,0][0] = new Cell(l,0, 0) { State = 1 };
                for (uint i = 1; i < SizeCount; i++)
                {
                    CellMap[l,i] = new Cell[6 * i];
                    for (uint j = 0; j < 6 * i; j++)
                    {
                        CellMap[l,i][j] = new Cell(l,i, j);
                    }
                }
            }
            for (uint l = 0; l < LayerCount; l++)
            {
                CellMap[l,0][0].InitAroundCells(l);
                for (uint i = 1; i < SizeCount; i++)
                    for (uint j = 0; j < 6*i; j++)
                        CellMap[l, i][j].InitAroundCells(l);
            }
        }

        public Point ToXY()
        {
            var 起点坐标弧度 = Math.PI * Location.Quadrant / 3;
            var 起点坐标 = new Point(Size * Location.I * Math.Cos(起点坐标弧度), Size * Location.I * Math.Sin(起点坐标弧度));
            var 位置坐标弧度 = 起点坐标弧度 + Math.PI * 2 / 3;
            var 位置坐标相对起点坐标 = new Point(Size * Location.Position * Math.Cos(位置坐标弧度), Size * Location.Position * Math.Sin(位置坐标弧度));
            return new Point(位置坐标相对起点坐标.X + 起点坐标.X + Size * SizeCount, 位置坐标相对起点坐标.Y + 起点坐标.Y + Size * SizeCount);
        }

        class Sorter1 : IComparer<Cell>
        {
            public int Compare(Cell x, Cell y)
            {
                if ((int)x.Position.Y == (int)y.Position.Y)
                {
                    return x.Position.X < y.Position.X ? -1 : 1;
                }
                return x.Position.Y < y.Position.Y ? -1 : 1;
            }
        }

        private static Sorter1 _sorter1 = new Sorter1();

        public void InitAroundCells(uint l)
        {
            AroundCells = GetAroundCells(l);
        }
        private Cell[] GetAroundCells(uint l)
        {
            var result = new List<Cell>();
            if (LayerCount == 2)
            {
                result.Add(Location.UpCell);
            }
            else if (LayerCount > 2)
            {
                result.Add(Location.UpCell);
                result.Add(Location.DownCell);
            }
            if (Location.I + Location.J == 0)
            {
                result.AddRange(CellMap[l, 1]);
                return result.ToArray();
            }
            var 象限 = Location.Quadrant;
            var outSide = new CellLocation(l,Location.I + 1, Location.J + 象限);
            result.Add(Location.PreviousCell);
            result.Add(Location.NextCell);
            if (Location.Position == 0)
            {
                result.Add(CellMap[l,Location.I - 1][Location.J - 象限]);

                if (Location.I + 1 < SizeCount)
                {
                    result.Add(outSide.Cell);
                    result.Add(outSide.PreviousCell);
                    result.Add(outSide.NextCell);
                }
            }
            else
            {
                var inSide = new CellLocation(l,Location.I - 1, Location.J - 象限 - 1);
                result.Add(inSide.NextCell);
                result.Add(inSide.Cell);
                if (Location.I + 1 < SizeCount)
                {
                    result.Add(outSide.Cell);
                    result.Add(outSide.NextCell);
                }
            }
           //result.Sort(_sorter1);
            return result.ToArray();
        }
    }
}
