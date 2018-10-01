using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Windows.Input;
using Helpers;
using Terminal.Models;

namespace Terminal.ViewModels
{
    public class MainVM : BaseViewModel
    {
        string[] portNames;
        /// <summary>Список доступных портов в системе</summary>
        public string[] PortNames
        {
            get { return portNames; }
            set { SetProperty(ref portNames, value); }
        }

        int selectedIndex;
        /// <summary>Выбранный порт</summary>
        public int SelectedIndex
        {
            get { return selectedIndex; }
            set { SetProperty(ref selectedIndex, value); }
        }

        public Serial COM_Port { get; set; } = new Serial();

        /// <summary>История отправленных сообщений</summary>
        List<string> TxStack = new List<string>();
        int TxStackCounter = -1;

        bool _IsConnected;
        /// <summary>Статус подключения порта</summary>
        public bool IsConnected
        {
            get { return _IsConnected; }
            set { SetProperty(ref _IsConnected, value); }
        }

        public MainVM()
        {
            PortNames = SerialPort.GetPortNames(); //Получаем список доступных портов
            SelectedIndex = PortNames.Length - 1; //Выбираем последний из них

            //Устанавливаем стартовые значения параметров порта
            COM_Port.Parameters = COM_Port.LoadParameters();

            cmdConnect = new RelayCommand(() =>
            {
                if (!COM_Port.Port.IsOpen)
                {
                    try { COM_Port.Connect(); IsConnected = true; COM_Port.SaveParameters(COM_Port.Parameters); }//Подключение
                    catch (Exception ex) { }
                }
                else
                {
                    COM_Port.Disonnect();//Отключение
                    IsConnected = false;
                }
                cmdsRaiseCanExecuteChanged();
            });

            //Отправка из текста из TextBox
            cmdWriteText = new RelayCommand(() =>
            {
                Write();
            }, () => COM_Port.Port.IsOpen);

            //Очистка входящего окна
            cmdZeroing = new RelayCommand(() =>
            {
                COM_Port.Data = String.Empty;
                for (int i = 0; i < COM_Port.RxData.Length; i++)
                {
                    COM_Port.RxData[i] = 0;
                }
            });

            cmdKeyDown = new Command<object>((a) =>
            {
                var key = ((KeyEventArgs)a).Key;
                if (COM_Port.Port.IsOpen)
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
                            COM_Port.TxData = String.Empty;
                        }
                        else
                        if (TxStack.Count > 0)
                            COM_Port.TxData = TxStack[TxStack.Count - TxStackCounter - 1];
                    }
                }
            });
        }

        public RelayCommand cmdConnect { get; set; }
        public RelayCommand cmdWriteText { get; set; }
        public RelayCommand cmdZeroing { get; set; }
        public Command cmdKeyDown { get; set; }

        void cmdsRaiseCanExecuteChanged()
        {
            cmdConnect?.RaiseCanExecuteChanged();
            cmdWriteText?.RaiseCanExecuteChanged();
        }

        void Write()
        {
            string Tx = String.Empty;
            if (String.Compare(COM_Port.TxData, "$$$") != 0)
                Tx = $"{COM_Port.TxData}\r";
            else
                Tx = $"{COM_Port.TxData}";
            COM_Port.Port.Write(Tx);
            COM_Port.Data += Tx;

            TxStack.Add(COM_Port.TxData);
            COM_Port.TxData = String.Empty;
            TxStackCounter = -1;
        }
    }
}
