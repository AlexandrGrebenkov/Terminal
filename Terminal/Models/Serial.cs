using System;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Xml.Serialization;
using Helpers;

namespace Terminal.Models
{
    public class Serial : BaseDataObject
    {
        /// <summary>COM-Порт</summary>
        public SerialPort Port = new SerialPort();

        public bool IsConnected => Port.IsOpen;

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
        public void Connect(SerialParameters parameters, Action<string> errorHandler = null)
        {
            // Настраиваем порт
            Port = new SerialPort
            {
                PortName = parameters.PortName,
                BaudRate = parameters.BaudRate,
                Parity = parameters.Parity,
                DataBits = parameters.DataBits,
                StopBits = parameters.StopBits,
                Handshake = parameters.Handshake
            };
            // Открываем порт
            try { Port.Open(); }
            catch (Exception ex)
            {
                errorHandler?.Invoke($"Ошибка открытия порта: {ex.Message}");
                ConnectionChanged?.Invoke(false);
                return;
            }
            ConnectionChanged?.Invoke(true);

            // Настраиваем приём данных
            IAsyncResult recv_result;
            Encoding DataEncoder = Encoding.GetEncoding("ASCII");   // Windows-1251
            kickoffRead = (() => recv_result = Port.BaseStream.BeginRead(RxData, 0, RxData.Length, delegate (IAsyncResult ar)
            {
                try
                {
                    int count = Port.BaseStream.EndRead(ar);
                    DataReceived?.Invoke(DataEncoder.GetString(RxData, 0, count));
                }
                catch (Exception exception)
                {
                    errorHandler?.Invoke($"------Rx Exception:------\r\nTime: {DateTime.Now} \r\n{exception.Message}");
                }
                kickoffRead?.Invoke();
            }, null));
            kickoffRead?.Invoke();
        }

        public void Write(string data, Action<string> errorHandler = null)
        {
            try
            {
                Port.Write(data);
            }
            catch (Exception ex)
            {
                errorHandler?.Invoke($"Ошибка отправки данных: {ex.Message}");
            }
        }

        /// <summary>Отключение (Закрытие порта)</summary>
        public void Disonnect(Action<string> errorHandler = null)
        {
            try
            {
                //Port.DiscardOutBuffer();
                //Port.BaseStream.EndRead(recv_result);
                kickoffRead = null;
                Port.Close();
                ConnectionChanged?.Invoke(false);
            }
            catch (Exception ex)
            {
                errorHandler?.Invoke($"Ошибка закрытия порта: {ex.Message}");
            }
        }

        /// <summary>Событие изменения состояния подключения порта</summary>
        public event Action<bool> ConnectionChanged;

        /// <summary>Событие приёма новых данных</summary>
        public event Action<string> DataReceived;
    }
}
