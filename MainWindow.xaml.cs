using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace LifeGame
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private Timer _timer = new Timer(1000);
        private int _count;
        private WriteableBitmap _wb;
        readonly byte[] whiteOrblack = { 0x00, 0xFF };
        private RenderWindow _renderWindow ;
        private int CurrentLayer;
        //private byte[] Rules = Enumerable.Repeat((byte)0, 64).ToArray();
        //private ObservableCollection<byte> ItemSource;
        public MainWindow()
        {
            InitializeComponent();
            _timer.Elapsed += _timer_Elapsed;
            Cell.CreateCells();
            LayerSelector.Maximum = Cell.LayerCount - 1;
            var cellSize = Cell.Size * 2 * Cell.SizeCount;
            _wb = new WriteableBitmap(cellSize, cellSize, 72, 72, PixelFormats.Gray8,BitmapPalettes.Gray256);
            MainImage.Source = _wb;
            Cell.BackBufferStride = _wb.BackBufferStride;
            //var bytes = new byte[] { 0xFF,0x00};
            
            //for (var i = 0; i < Cell.Rules.Length; i++)
            //{
            //    Rules[i] = (byte) ((i << 1) | (Cell.Rules[i] == true ? 1 : 0));
            //    Rules[i] = (byte) ((Rules[i] << 1) | (Cell.Rules[i] == null ? 0 : 1));
            //}
            //ItemSource = new ObservableCollection<byte>(Rules); 
            //RuleItems.ItemsSource = ItemSource;
            SurvivalsRule.ItemsSource = new ObservableCollection<Rule>(Enumerable.Range(0,9).Select(x=>new Rule{Index = x}));
            BirthsRule.ItemsSource = new ObservableCollection<Rule>(Enumerable.Range(0, 9).Select(x => new Rule { Index = x }));
        }

        void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            long bufferPoint = 0;
            Dispatcher.Invoke(new Action(() =>
            {
                _wb.Lock();
                bufferPoint = (long)_wb.BackBuffer;
            }));
            try
            {
                var needChange = new List<Cell>();
                for (uint l = 0; l < Cell.LayerCount; l++)
                {
                    if (l == CurrentLayer)
                    {
                        if (Cell.CellMap[l, 0][0].ChangeLife(bufferPoint)) needChange.Add(Cell.CellMap[l, 0][0]);
                    }
                    else Cell.CellMap[l, 0][0].SetNextState();
                    for (uint i = 1; i < Cell.SizeCount; i++)
                        for (uint j = 0; j < 6*i; j++)
                            if (l == CurrentLayer)
                            {
                                if (Cell.CellMap[l, i][j].ChangeLife(bufferPoint)) needChange.Add(Cell.CellMap[l, i][j]);
                            }
                            else Cell.CellMap[l, i][j].SetNextState();
                }
                Parallel.ForEach(needChange, x => x.ChangeAlive());
                //Dispatcher.Invoke(new Action(() =>
                //{
                //    foreach (var cell in needChange)
                //    {
                //        cell.Alive = !cell.Alive;

                //        _wb.WritePixels(new Int32Rect(
                //            (int) (cell.Position.X),
                //            (int) (cell.Position.Y),
                //            1,
                //            1), whiteOrblack, 1, cell.Alive ? 1 : 0);
                //    }
                //}));
            }
            catch (Exception ex)
            {
                Debug.Write(ex);
            }
            
            Dispatcher.Invoke(new Action(() =>
            {
                _wb.AddDirtyRect(new Int32Rect(0,0,_wb.PixelWidth,_wb.PixelHeight));
                _wb.Unlock();
            }));
          // _wb.Unlock();
         
        }

        private void StartRender(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            _timer.Start();
            var rand = new Random();
            //_renderWindow = new RenderWindow {MainImage = {Source = _wb}};
            for (uint l = 0; l < Cell.LayerCount; l++)
            {
                Cell.CellMap[l, 0][0].State = 1;
                for (uint i = 1; i < Cell.SizeCount; i++)
                    for (uint j = 0; j < 6 * i; j++)
                        Cell.CellMap[l, i][j].State = rand.Next(2);
            }
            //_wb.Lock();
            //foreach (var cell in Cell.CellMap.SelectMany(cellFloor => cellFloor).Take(100))
            //{

                //cell.State = rand.Next(0, 2);
                //_wb.WritePixels(new Int32Rect(
                //       (int)(cell.Position.X),
                //       (int)(cell.Position.Y),
                //       1,
                //       1), whiteOrblack, 1, cell.Alive ? 1 : 0);
            //}
            //_wb.Unlock();
            //for (int i = 0; i < 64; i++)
            //{
            //    if ((ItemSource[i] & 1) == 0) continue;
            //    var j = ItemSource[i] >> 1;
            //    Cell.Rules[i] = (j & 1) == 1;
            //}
            Cell.ApplyRules((SurvivalsRule.ItemsSource as ObservableCollection<Rule>).Where(x => x.Value).Select(x => x.Index), (BirthsRule.ItemsSource as ObservableCollection<Rule>).Where(x => x.Value).Select(x => x.Index));
            //_renderWindow.ShowDialog();
            
        }

        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            Application.Current.Shutdown(0);
        }

        private void SaveRule(object sender, RoutedEventArgs e)
        {
            var savefileDialog = new SaveFileDialog();
            if (savefileDialog.ShowDialog(this) == true)
            {
                //File.WriteAllBytes(savefileDialog.FileName, ItemSource.ToArray());
            }
        }

        private void ReadRule(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog(this) == true)
            {
                //ItemSource.Clear();
                //RuleItems.ItemsSource = ItemSource = new ObservableCollection<byte>(Rules = File.ReadAllBytes(openFileDialog.FileName));
            }
        }

        private void LayerSelector_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            CurrentLayer = (int) e.NewValue;
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
           _timer.Stop();
            if (e.NewValue != 0)
            {
                _timer.Interval = 1000 / e.NewValue;
                _timer.Start();
            }
            
        }
    }
}
