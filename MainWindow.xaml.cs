using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace hb80
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        LocalOscillator _localOscillator = new LocalOscillator();
        public MainWindow()
        {
            InitializeComponent();
            _localOscillator.StatusTextBox = StatusText;
            bool result = _localOscillator.initialize();
        }

        // Event handlers
        private void frequencyChanged(object sender, TextChangedEventArgs args)
        {
            _localOscillator.frequencyChanged(sender, args);
        }

        private void frequencyDownStart(object sender, MouseButtonEventArgs args)
        {
            _localOscillator.FrequencyChangeMode = FrequencyChange.Decrease;
        }

        private void frequencyDownEnd(object sender, MouseButtonEventArgs args)
        {
            _localOscillator.FrequencyChangeMode = FrequencyChange.Idle;
        }

        private void frequencyDownFastStart(object sender, MouseButtonEventArgs args)
        {
            _localOscillator.FrequencyChangeMode = FrequencyChange.DecreaseFast;
        }

        private void frequencyDownFastEnd(object sender, MouseButtonEventArgs args)
        {
            _localOscillator.FrequencyChangeMode = FrequencyChange.Idle;
        }

        private void frequencyUpFastStart(object sender, MouseButtonEventArgs args)
        {
            _localOscillator.FrequencyChangeMode = FrequencyChange.IncreaseFast;
        }

        private void frequencyUpFastEnd(object sender, MouseButtonEventArgs args)
        {
            _localOscillator.FrequencyChangeMode = FrequencyChange.Idle;
        }

        private void frequencyUpStart(object sender, MouseButtonEventArgs args)
        {
            _localOscillator.FrequencyChangeMode = FrequencyChange.Increase;
        }

        private void frequencyUpEnd(object sender, MouseButtonEventArgs args)
        {
            _localOscillator.FrequencyChangeMode = FrequencyChange.Idle;
        }
    }
}
