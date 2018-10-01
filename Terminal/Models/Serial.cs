using System;
using System.Text;
using Helpers;
using System.IO.Ports;
using System.IO;
using System.Xml.Serialization;

namespace BLE_SpeedTest.Models
{
    public class Serial : BaseDataObject
    {
        SerialParameters parameters;
        /// <summary>Параметры COM-порта</summary>
        public SerialParameters Parameters
        {
            get { return parameters; }
            set { SetProperty(ref parameters, value); }
        }

        /// <summary>COM-Порт</summary>
        public SerialPort Port = new SerialPort();
        IAsyncResult recv_result;


        string data;
        /// <summary>Полученные данные (Отображаемая на UI строка)</summary>
        public string Data
        {
            get { return data; }
            set { SetProperty(ref data, value); }
        }

        string txData;
        /// <summary>Отправленные данные</summary>
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

        Action kickoffRead = null;
        public void Connect()
        {
            // Настраиваем порт
            Port.PortName = Parameters.PortName;
            Port.BaudRate = Parameters.BaudRate;
            Port.Parity = Parameters.Parity;
            Port.DataBits = Parameters.DataBits;
            Port.StopBits = Parameters.StopBits;
            Port.Handshake = Parameters.Handshake;
            // Открываем порт
            Port.Open();

            // Настраиваем приём данных
            Encoding DataEncoder = Encoding.GetEncoding("ASCII");   // Windows-1251
            kickoffRead = (() => recv_result = Port.BaseStream.BeginRead(RxData, 0, RxData.Length, delegate (IAsyncResult ar)
            {
                try
                {
                    int count = Port.BaseStream.EndRead(ar);
                    Data += DataEncoder.GetString(RxData, 0, count);
                }
                catch (Exception exception)
                {
                    Data += String.Format("------Rx Exception:------\r\nTime: {0} \r\n{1}", DateTime.Now, exception.Message);
                }
                kickoffRead?.Invoke();
            }, null));
            kickoffRead?.Invoke();
        }

        /// <summary>Отключение (Закрытие порта)</summary>
        public void Disonnect()
        {
            //Port.DiscardOutBuffer();
            //Port.BaseStream.EndRead(recv_result);
            kickoffRead = null;
            Port.Close();
        }

        public void SaveParameters(SerialParameters param)
        {
            using (var writer = new StreamWriter("Parameters.xml"))
            {
                var xs = new XmlSerializer(typeof(SerialParameters));
                xs.Serialize(writer, param);
            }
        }

        public SerialParameters LoadParameters()
        {
            var param = new SerialParameters();
            try
            {
                using (var reader = new StreamReader("Parameters.xml"))
                {
                    var xs = new XmlSerializer(typeof(SerialParameters));
                    param = (SerialParameters)xs.Deserialize(reader);
                }
            }
            catch (Exception ex)
            {
                param = new SerialParameters()
                {
                    BaudRate = 115200,
                    DataBits = 8,
                    Parity = Parity.None,
                    Handshake = Handshake.None,
                    StopBits = StopBits.One
                };
            }
            return param;
        }
    }
}
