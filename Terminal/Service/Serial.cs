using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using Helpers;
using Terminal.Models;

namespace Terminal.Service
{
    public class Serial : BaseDataObject, ISerial
    {
        /// <summary>COM-Порт</summary>
        SerialPort port;

        public bool IsConnected => port?.IsOpen ?? false;

        readonly byte[] rxData = new byte[2000];

        Action kickoffRead = null;
        public void Connect(SerialParameters parameters, Action<string> errorHandler = null)
        {
            // Настраиваем порт
            port = new SerialPort
            {
                PortName = parameters.PortName,
                BaudRate = parameters.BaudRate,
                Parity = parameters.Parity,
                DataBits = parameters.DataBits,
                StopBits = parameters.StopBits,
                Handshake = parameters.Handshake
            };
            // Открываем порт
            try { port.Open(); }
            catch (Exception ex)
            {
                errorHandler?.Invoke($"Ошибка открытия порта: {ex.Message}");
                ConnectionChanged?.Invoke(false);
                return;
            }
            ConnectionChanged?.Invoke(true);

            // Настраиваем приём данных
            var dataEncoder = Encoding.GetEncoding("ASCII");   // Windows-1251
            kickoffRead = (() => port.BaseStream.BeginRead(rxData, 0, rxData.Length, delegate (IAsyncResult ar)
            {
                try
                {
                    var count = port.BaseStream.EndRead(ar);
                    DataReceived?.Invoke(dataEncoder.GetString(rxData, 0, count));
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
                port.Write(data);
            }
            catch (Exception ex)
            {
                errorHandler?.Invoke($"Ошибка отправки данных: {ex.Message}");
            }
        }

        public void ClearRx()
        {
            for (int i = 0; i < rxData.Length; i++)
                rxData[i] = 0;
        }

        /// <summary>Отключение (Закрытие порта)</summary>
        public void Disonnect(Action<string> errorHandler = null)
        {
            try
            {
                //Port.DiscardOutBuffer();
                //Port.BaseStream.EndRead(recv_result);
                kickoffRead = null;
                port.Close();
                ConnectionChanged?.Invoke(false);
            }
            catch (Exception ex)
            {
                errorHandler?.Invoke($"Ошибка закрытия порта: {ex.Message}");
            }
        }

        public string[] PortNames => SerialPort.GetPortNames();

        /// <summary>Событие изменения состояния подключения порта</summary>
        public event Action<bool> ConnectionChanged;

        /// <summary>Событие приёма новых данных</summary>
        public event Action<string> DataReceived;
    }
}
