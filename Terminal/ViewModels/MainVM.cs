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
        public Serial BLE_Serial { get; set; } = new Serial();

        List<string> TxStack = new List<string>();
        int TxStackCounter = -1;

        public MainVM()
        {
            BLE_Serial.GetPortNames();
            //Подключение
            cmdConnect = new RelayCommand(() =>
            {
                BLE_Serial.Connect();
                cmdsRaiseCanExecuteChanged();
            }, () => !BLE_Serial.Port.IsOpen);
            //Отключение
            cmdDisconnect = new RelayCommand(() =>
            {
                BLE_Serial.Disonnect();
                cmdsRaiseCanExecuteChanged();
            }, () => BLE_Serial.Port.IsOpen);
            //Отправка из текста из TextBox
            cmdWriteText = new RelayCommand(() =>
            {
                Write();
            }, () => BLE_Serial.Port.IsOpen);
            //Макрос №1
            cmdWriteMacro1 = new RelayCommand(() =>
            {
                byte[] buf = new byte[BLE_Serial.packSize];
                for (int i = 0; i < buf.Length; i++)
                {
                    buf[i] = (byte)i;
                }
                BLE_Serial.start = DateTime.Now;
                BLE_Serial.Port.Write(buf, 0, buf.Length);
            }, () => BLE_Serial.Port.IsOpen);
            cmdZeroing = new RelayCommand(() =>
                {
                    BLE_Serial.Data = String.Empty;
                    for (int i = 0; i < BLE_Serial.RxData.Length; i++)
                    {
                        BLE_Serial.RxData[i] = 0;
                    }
                });
            cmdKeyDown = new Command<object>((a) =>
            {
                if (BLE_Serial.Port.IsOpen)
                {
                    if (((KeyEventArgs)a).Key == Key.Enter)
                    {
                        Write();
                    }
                    if ((((KeyEventArgs)a).Key == Key.Up) |
                        (((KeyEventArgs)a).Key == Key.Down))
                    {
                        if (((KeyEventArgs)a).Key == Key.Up)
                            TxStackCounter++;
                        if (((KeyEventArgs)a).Key == Key.Down)
                            TxStackCounter--;
                        if (TxStackCounter >= TxStack.Count)
                            TxStackCounter = TxStack.Count - 1;
                        if (TxStackCounter <= -1)
                        {
                            TxStackCounter = -1;
                            BLE_Serial.TxData = String.Empty;
                        }
                        else
                        if (TxStack.Count > 0)
                            BLE_Serial.TxData = TxStack[TxStack.Count - TxStackCounter - 1];
                    }

                }
            });
        }

        public RelayCommand cmdConnect { get; set; }
        public RelayCommand cmdDisconnect { get; set; }
        public RelayCommand cmdWriteText { get; set; }
        public RelayCommand cmdWriteMacro1 { get; set; }
        public RelayCommand cmdZeroing { get; set; }
        public Command cmdKeyDown { get; set; }

        void cmdsRaiseCanExecuteChanged()
        {
            cmdConnect.RaiseCanExecuteChanged();
            cmdDisconnect.RaiseCanExecuteChanged();
            cmdWriteText.RaiseCanExecuteChanged();
            cmdWriteMacro1.RaiseCanExecuteChanged();
        }

        void Write()
        {
            if (String.Compare(BLE_Serial.TxData, "$$$") != 0)
                BLE_Serial.Port.Write(BLE_Serial.TxData + "\n");
            else
                BLE_Serial.Port.Write(BLE_Serial.TxData);
            TxStack.Add(BLE_Serial.TxData);
            BLE_Serial.TxData = String.Empty;
            TxStackCounter = -1;
        }
    }
}
