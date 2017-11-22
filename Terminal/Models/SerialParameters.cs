using Helpers;
using System.IO.Ports;

namespace BLE_SpeedTest.Models
{
    public class SerialParameters : BaseDataObject
    {
        string portName;
        public string PortName
        {
            get { return portName; }
            set { SetProperty(ref portName, value); }
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

        Parity parity;
        public Parity Parity
        {
            get { return parity; }
            set { SetProperty(ref parity, value); }
        }

        StopBits stopBits;
        public StopBits StopBits
        {
            get { return stopBits; }
            set { SetProperty(ref stopBits, value); }
        }

        Handshake handshake;
        public Handshake Handshake
        {
            get { return handshake; }
            set { SetProperty(ref handshake, value); }
        }
    }
}
