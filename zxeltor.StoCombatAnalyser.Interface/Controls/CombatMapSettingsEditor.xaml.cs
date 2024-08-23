using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using zxeltor.StoCombatAnalyzer.Interface.Model.CombatMap;

namespace zxeltor.StoCombatAnalyzer.Interface.Controls
{
    /// <summary>
    /// Interaction logic for CombatMapSettingsEditor.xaml
    /// </summary>
    public partial class CombatMapSettingsEditor : Window
    {
        public CombatMapSettingsEditor()
        {
            InitializeComponent();
        }

        public bool? ShowDialog(CombatMap combatMap)
        {
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            this.DataContext = combatMap;
            return base.ShowDialog();
        }

        private void UiButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
