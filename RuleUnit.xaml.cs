using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LifeGame
{
    /// <summary>
    /// RuleUnit.xaml 的交互逻辑
    /// </summary>
    public partial class RuleUnit : UserControl
    {
        public static readonly DependencyProperty RuleProperty = DependencyProperty.Register(
            "Rule", typeof (byte), typeof (RuleUnit), new PropertyMetadata(
                (d, e) =>
                {
                    var i = (byte)e.NewValue;
                    var instance = (RuleUnit) d;
                    if ((i & 1) == 1)
                    {
                        i = (byte) (i >> 1);
                        instance.CheckBoxCenter.IsChecked = (i & 1) == 1;
                    }
                    else
                    {
                        i = (byte) (i >> 1);
                    }
                    for (var j = 5; j >=0; j--)
                    {
                       i= (byte) (i >> 1);
                        instance.AllCheckBoxs[j].IsChecked = (i & 1) == 1;
                    }
                }));

        public byte Rule
        {
            get { return (byte)GetValue(RuleProperty); }
            set { SetValue(RuleProperty, value); }
        }

        public CheckBox[] AllCheckBoxs;
        public RuleUnit()
        {
            InitializeComponent();
            AllCheckBoxs = new[] {CheckBox0, CheckBox1, CheckBox2, CheckBox3, CheckBox4, CheckBox5};
            foreach (var allCheckBox in AllCheckBoxs)
            {
                allCheckBox.IsEnabled = false;
            }
        }

        private void OnChange(object sender, RoutedEventArgs e)
        {
            var i = (byte) (Rule & 0xFC);
            Rule =  (byte) (i | (CheckBoxCenter.IsChecked == true ? 3 : 1));
        }
    }
}
