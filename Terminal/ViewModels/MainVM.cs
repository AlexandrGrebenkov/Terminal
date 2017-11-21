using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Helpers;
using BLE_SpeedTest.Models;
using System.IO.Ports;
using System.Windows.Input;

namespace BLE_SpeedTest.ViewModels
{
    public class MainVM : BaseViewModel
    {
        public Serial SerialPort { get; set; } = new Serial();

        //История отправленных сообщений
        List<string> TxStack = new List<string>();
        int TxStackCounter = -1;

        string connectButtonText;
        public string ConnectButtonText
        {
            get { return connectButtonText; }
            set { SetProperty(ref connectButtonText, value); }
        }

        public MainVM()
        {
            SerialPort.GetPortNames();
            SerialPort.SelectedIndex = SerialPort.PortNames.Length - 1;
            SerialPort.BaudRate = 115200;
            //SerialPort.DataBits = 8;

            ConnectButtonText = "Подключиться";
            cmdConnect = new RelayCommand(() =>
            {
                if (!SerialPort.Port.IsOpen)
                {
                    try { SerialPort.Connect(); ConnectButtonText = "Отключиться"; }//Подключение
                    catch(Exception ex) {  }
                }
                else
                {
                    SerialPort.Disonnect();//Отключение
                    ConnectButtonText = "Подключиться";
                }
                cmdsRaiseCanExecuteChanged();
            });

            //Отправка из текста из TextBox
            cmdWriteText = new RelayCommand(() =>
            {
                Write();
            }, () => SerialPort.Port.IsOpen);
            //Макрос №1
            cmdWriteMacro1 = new RelayCommand(() =>
            {
                byte[] buf = new byte[SerialPort.packSize];
                for (int i = 0; i < buf.Length; i++)
                {
                    buf[i] = (byte)i;
                }
                SerialPort.start = DateTime.Now;
                SerialPort.Port.Write(buf, 0, buf.Length);
            }, () => SerialPort.Port.IsOpen);
            //Очистка входящего окна
            cmdZeroing = new RelayCommand(() =>
            {
                SerialPort.Data = String.Empty;
                for (int i = 0; i < SerialPort.RxData.Length; i++)
                {
                    SerialPort.RxData[i] = 0;
                }
            });

            cmdKeyDown = new Command<object>((a) =>
            {
                var key = ((KeyEventArgs)a).Key;
                if (SerialPort.Port.IsOpen)
                {
                    if (key == Key.Enter)
                    {
                        Write();
                    }
                    if ((key == Key.Up) |
                        (key == Key.Down))
                    {
                        if (key == Key.Up)
                            TxStackCounter++;
                        if (key == Key.Down)
                            TxStackCounter--;
                        if (TxStackCounter >= TxStack.Count)
                            TxStackCounter = TxStack.Count - 1;
                        if (TxStackCounter <= -1)
                        {
                            TxStackCounter = -1;
                            SerialPort.TxData = String.Empty;
                        }
                        else
                        if (TxStack.Count > 0)
                            SerialPort.TxData = TxStack[TxStack.Count - TxStackCounter - 1];
                    }

                }
            });
        }

        public RelayCommand cmdConnect { get; set; }
        public RelayCommand cmdWriteText { get; set; }
        public RelayCommand cmdWriteMacro1 { get; set; }
        public RelayCommand cmdZeroing { get; set; }
        public Command cmdKeyDown { get; set; }

        void cmdsRaiseCanExecuteChanged()
        {
            cmdConnect.RaiseCanExecuteChanged();
            cmdWriteText.RaiseCanExecuteChanged();
            cmdWriteMacro1.RaiseCanExecuteChanged();
        }

        void Write()
        {
            if (String.Compare(SerialPort.TxData, "$$$") != 0)
                SerialPort.Port.Write(SerialPort.TxData + "\n");
            else
                SerialPort.Port.Write(SerialPort.TxData);
            TxStack.Add(SerialPort.TxData);
            SerialPort.TxData = String.Empty;
            TxStackCounter = -1;
        }
    }
}
