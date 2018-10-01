using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Windows.Input;
using Helpers;
using Terminal.Models;
using Terminal.Service;

namespace Terminal.ViewModels
{
    public class MainVM : BaseViewModel
    {
        IFileWorker fileWorker = new FileWorker();

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

        SerialParameters _Parameters;
        /// <summary>Параметры подключения к порту</summary>
        public SerialParameters Parameters
        {
            get { return _Parameters; }
            set { SetProperty(ref _Parameters, value); }
        }

        public Serial COM_Port { get; set; } = new Serial();

        string _Data;
        /// <summary></summary>
        public string Data
        {
            get { return _Data; }
            set { SetProperty(ref _Data, value); }
        }

        /// <summary>История отправленных сообщений</summary>
        List<string> TxStack = new List<string>();
        int _txStackCounter = -1;

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
            Parameters = fileWorker.LoadSettings() ?? SerialParameters.Default;

            COM_Port.ConnectionChanged += status =>
            {
                IsConnected = status;
                cmdsRaiseCanExecuteChanged();
            };

            COM_Port.DataReceived += data =>
            {
                Data += data;
            };

            cmdConnect = new RelayCommand(() =>
            {
                if (!COM_Port.IsConnected)
                {
                    COM_Port.Connect(Parameters, error => Data += error);
                    fileWorker.SaveSettings(Parameters, error => Data += error); //Подключение
                }
                else
                    COM_Port.Disonnect(error => Data += error);//Отключение
            });

            //Отправка из текста из TextBox
            cmdWriteText = new RelayCommand(() =>
            {
                Write();
            }, () => COM_Port.IsConnected);

            //Очистка входящего окна
            cmdZeroing = new RelayCommand(() =>
            {
                Data = string.Empty;
                for (int i = 0; i < COM_Port.RxData.Length; i++)
                {
                    COM_Port.RxData[i] = 0;
                }
            });

            cmdKeyDown = new Command<object>((a) =>
            {
                var key = ((KeyEventArgs)a).Key;
                if (!COM_Port.IsConnected) return;
                switch (key)
                {
                    case Key.Enter:
                        Write();
                        break;
                    case Key.Up:
                    case Key.Down:
                        if (key == Key.Up)
                            _txStackCounter++;
                        if (key == Key.Down)
                            _txStackCounter--;
                        if (_txStackCounter >= TxStack.Count)
                            _txStackCounter = TxStack.Count - 1;
                        if (_txStackCounter <= -1)
                        {
                            _txStackCounter = -1;
                            COM_Port.TxData = string.Empty;
                        }
                        else
                        if (TxStack.Count > 0)
                            COM_Port.TxData = TxStack[TxStack.Count - _txStackCounter - 1];

                        break;
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
            var tx = string.Empty;
            tx = string.Compare(COM_Port.TxData, "$$$") != 0 ? $"{COM_Port.TxData}\r" : $"{COM_Port.TxData}";
            COM_Port.Write(tx);
            Data += tx;

            TxStack.Add(COM_Port.TxData);
            COM_Port.TxData = string.Empty;
            _txStackCounter = -1;
        }
    }
}
