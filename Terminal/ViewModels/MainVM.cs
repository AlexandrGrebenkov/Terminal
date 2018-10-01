﻿using System;
using System.Collections.Generic;
using System.Windows.Input;
using Helpers;
using Terminal.Models;
using Terminal.Service;

namespace Terminal.ViewModels
{
    public class MainVM : BaseViewModel
    {
        IFileWorker fileWorker = new FileWorker();
        ISerial serial = new Serial();

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

        string _Data;
        /// <summary></summary>
        public string Data
        {
            get { return _Data; }
            set { SetProperty(ref _Data, value); }
        }

        string _TxData;
        /// <summary></summary>
        public string TxData
        {
            get { return _TxData; }
            set { SetProperty(ref _TxData, value); }
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
            PortNames = serial.PortNames; //Получаем список доступных портов
            SelectedIndex = PortNames.Length - 1; //Выбираем последний из них

            //Устанавливаем стартовые значения параметров порта
            Parameters = fileWorker.LoadSettings() ?? SerialParameters.Default;

            serial.ConnectionChanged += status =>
            {
                IsConnected = status;
                cmdsRaiseCanExecuteChanged();
            };

            serial.DataReceived += data =>
            {
                Data += data;
            };

            cmdConnect = new RelayCommand(() =>
            {
                if (!serial.IsConnected)
                {
                    serial.Connect(Parameters, error => Data += error);
                    fileWorker.SaveSettings(Parameters, error => Data += error); //Подключение
                }
                else
                    serial.Disonnect(error => Data += error);//Отключение
            });

            //Отправка из текста из TextBox
            cmdWriteText = new RelayCommand(() =>
            {
                Write();
            }, () => serial.IsConnected);

            //Очистка входящего окна
            cmdZeroing = new RelayCommand(() =>
            {
                Data = string.Empty;
                serial.ClearRx();
            });

            cmdKeyDown = new Command<object>((a) =>
            {
                var key = ((KeyEventArgs)a).Key;
                if (!serial.IsConnected) return;
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
                            TxData = string.Empty;
                        }
                        else
                        if (TxStack.Count > 0)
                            TxData = TxStack[TxStack.Count - _txStackCounter - 1];

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
            tx = string.Compare(TxData, "$$$") != 0 ? $"{TxData}\r" : $"{TxData}";
            serial.Write(tx);
            Data += tx;

            TxStack.Add(TxData);
            TxData = string.Empty;
            _txStackCounter = -1;
        }
    }
}
