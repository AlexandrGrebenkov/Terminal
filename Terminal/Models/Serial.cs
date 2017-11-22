using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Helpers;
using System.IO.Ports;

namespace BLE_SpeedTest.Models
{
    public class Serial : BaseDataObject
    {
        public DateTime start = new DateTime();
        public DateTime stop = new DateTime();

        double speed;
        public double Speed
        {
            get { return speed; }
            set { SetProperty(ref speed, value); }
        }

        public int packSize = 1024 * 5;

        public SerialPort Port = new SerialPort();
        string name;
        public string Name
        {
            get { return name; }
            set { SetProperty(ref name, value); }
        }

        int selectedIndex;
        public int SelectedIndex
        {
            get { return selectedIndex; }
            set { SetProperty(ref selectedIndex, value); }
        }

        int baudRate;
        public int BaudRate
        {
            get { return baudRate; }
            set { SetProperty(ref baudRate, value); }
        }

        int dataBits;
        public int DataBits
        {
            get { return dataBits; }
            set { SetProperty(ref dataBits, value); }
        }


        string data;
        public string Data
        {
            get { return data; }
            set { SetProperty(ref data, value); }
        }

        string txData;
        public string TxData
        {
            get { return txData; }
            set { SetProperty(ref txData, value); }
        }

        byte[] rxData = new byte[2000];
        public byte[] RxData
        {
            get { return rxData; }
            set { SetProperty(ref rxData, value); }
        }

        string[] portNames;
        public string[] PortNames
        {
            get { return portNames; }
            set { SetProperty(ref portNames, value); }
        }

        public void GetPortNames()
        {
            PortNames = SerialPort.GetPortNames();
        }
        Action kickoffRead = null;
        public void Connect()
        {
            Encoding windows1251 = Encoding.GetEncoding("ASCII");//Windows-1251
            Port.PortName = Name;
            Port.BaudRate = BaudRate;
            Port.Parity = Parity.None;
            Port.DataBits = DataBits;
            Port.StopBits = StopBits.One;
            Port.Handshake = Handshake.None;
            Port.Open();

            kickoffRead = (Action)(() => Port.BaseStream.BeginRead(RxData, 0, RxData.Length, delegate (IAsyncResult ar)
            {
                try
                {
                    int count = Port.BaseStream.EndRead(ar);

                    Data += windows1251.GetString(RxData, 0, count);
                }
                catch (Exception exception)
                {
                    Data += String.Format("------Rx Exception:------ Time: {0} \r\n{1}", DateTime.Now, exception.Message);
                }
                kickoffRead?.Invoke();
            }, null)); kickoffRead?.Invoke();

        }

        public void Disonnect()
        {
            kickoffRead = null;
            Port.Close();
        }
    }
}
