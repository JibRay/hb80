﻿using System;
//using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.IO.Ports;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media;
//using System.Windows.Input;
using System.Timers;

namespace hb80
{
    public enum FrequencyChange
    {
        Idle,
        Increase,
        Decrease,
        IncreaseFast,
        DecreaseFast
    }

    public class LocalOscillator
    {
        private static SerialPort _port = new SerialPort("COM1", 19200);
        private bool _connected = false;
        private static double _frequency = 0.0;
        private static int _wordsPerMinute = 20;
        private static TextBox? _frequencyTextBox, _statusTextBox, _wordsPerMinuteTextBox;
        private static FrequencyChange _frequencyChangeMode = FrequencyChange.Idle;
        private System.Windows.Threading.DispatcherTimer _frequencyChangeTimer, _frequencyUpdateTimer;
        public bool Connected => _connected;

        public TextBox? StatusTextBox
        {
            get => _statusTextBox;
            set => _statusTextBox = value;
        }
        public FrequencyChange FrequencyChangeMode
        {
            get => _frequencyChangeMode;
            set
            {
                _frequencyChangeMode = value;
                if (_frequencyChangeMode == FrequencyChange.Idle)
                {
                    // If idle stop the frequency step timer.
                    _frequencyChangeTimer.Stop();
                }
                else
                {
                    // Step the frequency value up or down one step, pause
                    // then start the timer.
                    stepFrequency();
                    Thread.Sleep(200);
                    _frequencyChangeTimer.Start();
                }
            }
        }

        ~LocalOscillator()
        {
            _port.Dispose();
            _frequencyChangeTimer.Stop();
            _frequencyUpdateTimer.Stop();
        }

        // Initialize this LocalOscillator object. Must be called before using
        // this object.
        public bool initialize()
        {
            if (findPortConnection())
            {
                StatusTextBox.Text = "Local oscillator found at " + _port.PortName;
            }
            else
            {
                StatusTextBox.Text = "Unable to find local oscillator connection";
                return false;
            }

            _frequencyChangeTimer = new System.Windows.Threading.DispatcherTimer();
            _frequencyChangeTimer.Tick += new EventHandler(onChangeTimerTick);
            _frequencyChangeTimer.Interval = new TimeSpan(0, 0, 0, 0, 100); // 100 milliseconds.

            _frequencyUpdateTimer = new System.Windows.Threading.DispatcherTimer();
            _frequencyUpdateTimer.Tick += new EventHandler(onUpdateTimerTick);
            _frequencyUpdateTimer.Interval = new TimeSpan(0, 0, 0, 1); // 1 second.
            _frequencyUpdateTimer.Start();

            return true;
        }

        // Step the frequency value up or down depending on the value of
        // _frequencyChangeMode.
        private static void stepFrequency()
        {
            switch (_frequencyChangeMode)
            {
                case FrequencyChange.Idle:
                    break;
                case FrequencyChange.Increase:
                    increaseFrequency(0.001);
                    break;
                case FrequencyChange.Decrease:
                    decreaseFrequency(0.001);
                    break;
                case FrequencyChange.IncreaseFast:
                    increaseFrequency(0.005);
                    break;
                case FrequencyChange.DecreaseFast:
                    decreaseFrequency(0.005);
                    break;
            }

        }

        // This timer steps the frequency value up or down on each tick.
        private static void onChangeTimerTick(object sender, EventArgs e)
        {
            stepFrequency();
        }

        private static void onUpdateTimerTick(object sender, EventArgs e)
        {
            var text = string.Format("f{0,0:F6}\n", _frequency);
            var length = text.Length;
            byte[] send = Encoding.ASCII.GetBytes(text);
            if (_port.IsOpen)
                _port.Write(send, 0, length);

            text = string.Format("w{0}\n", _wordsPerMinute);
            length = text.Length;
            send = Encoding.ASCII.GetBytes(text);
            if (_port.IsOpen)
                _port.Write(send, 0, length);
        }

        // Scan the serial ports for the local oscillator connection. If
        // successful set _port.PortName and return true. Return false
        // otherwise.
        public bool findPortConnection ()
        {
            bool status = false;
            byte[] line = new byte[1000];
            int index = 0;
            _connected = false;

            // Scan the first 20 ports for the local oscillator connection.
            for (int n = 1; n < 21; n++)
            {
                _port.DtrEnable = true;
                _port.PortName = "COM" + n.ToString();
                try
                {
                    _port.Open();

                    Thread.Sleep(2000);

                    // Prompt the device to send its ID.
                    byte[] send = Encoding.ASCII.GetBytes("@");
                    _port.Write(send, 0, 1);

                    Thread.Sleep(100);

                    while (_port.BytesToRead > 0)
                        line[index++] = (byte)_port.ReadByte();
                    string response = System.Text.Encoding.Default.GetString(line);
                    if (response.Contains("Local Oscillator"))
                    {
                        send = Encoding.ASCII.GetBytes("\n");
                        _port.Write(send, 0, 1);
                        status = true;
                        _connected = true;
                        break;
                    }
                }
                catch
                {
                    status = false;
                }
            }
            if (!status)
                _port.Close();
            return status;
        }

        // Process a frequency text change event from the Textbox.
        public void frequencyChanged(object sender, TextChangedEventArgs args)
        {
             _frequencyTextBox = sender as TextBox;
            var valueText = _frequencyTextBox.Text;
            if (valueText != null)
            {
                try
                {
                    _frequency = Convert.ToDouble(valueText);
                    if (_frequency >= 3.5 && _frequency <= 4.0)
                        _frequencyTextBox.Background = Brushes.LightGray;
                    else
                        _frequencyTextBox.Background = Brushes.LightPink;
                }
                catch (Exception ex)
                {
                    _frequencyTextBox.Background = Brushes.LightPink;
                }
            }
        }

        public void wordsPerMinuteChanged(object sender, TextChangedEventArgs args)
        {
            _wordsPerMinuteTextBox = sender as TextBox;
            var valueText = _wordsPerMinuteTextBox.Text;
            if (valueText != null)
            {
                try
                {
                    _wordsPerMinute = Convert.ToInt32(valueText);
                    if (_wordsPerMinute >= 1 && _wordsPerMinute <= 40)
                        _wordsPerMinuteTextBox.Background = Brushes.LightGray;
                    else
                        _wordsPerMinuteTextBox.Background= Brushes.LightPink;
                }
                catch (Exception ex)
                {
                    _wordsPerMinuteTextBox.Background = Brushes.LightPink;
                }
            }
        }

        // Increase the frequency by 'increment'. Check for upper bound.
        private static void increaseFrequency(double increment)
        {
            if (_frequency < 4.0)
            {
                _frequency += increment;
                _frequency = _frequency > 4.0 ? 4.0 : _frequency;
                _frequencyTextBox.Text = string.Format("{0,-7:F6}", _frequency);
            }
        }

        // Decrease the frequency by 'increment'. Check for lower bound.
        private static void decreaseFrequency(double increment)
        {
            if (_frequency > 3.5)
            {
                _frequency -= increment;
                _frequency = _frequency < 3.5 ? 3.5 : _frequency;
                _frequencyTextBox.Text = string.Format("{0,-7:F6}", _frequency);
            }
        }
    }
}
