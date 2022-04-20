using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media;

namespace hb80
{
    public class LocalOscillator
    {
        private static SerialPort _port = new SerialPort("COM1", 230400);
        private bool _connected = false;
        private double _frequency = 0.0;

        // Scan the serial ports for the local oscillator connection. If
        // successful set _port.PortName and return true. Return false
        // otherwise.
        public bool initialize ()
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
                finally
                {
                    _port.Close();
                }
            }
            return status;
        }

        public void frequencyChanged(object sender, TextChangedEventArgs args)
        {
            double frequency = 0;

            TextBox? textBox = sender as TextBox;
            var valueText = textBox.Text;
            if (valueText != null)
            {
                try
                {
                    frequency = Convert.ToDouble(valueText);
                    if (frequency >= 3.5 && frequency <= 4.0)
                    {
                        textBox.Background = Brushes.Snow;
                    }
                    else
                    {
                        textBox.Background = Brushes.Orange;
                    }
                }
                catch (Exception ex)
                {
                    textBox.Background = Brushes.Orange;
                }
            }
        }
    }
}
