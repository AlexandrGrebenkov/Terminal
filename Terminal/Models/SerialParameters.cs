﻿using System.IO.Ports;
using Helpers;

namespace Terminal.Models
{
    public class SerialParameters : BaseDataObject
    {
        string portName;
        /// <summary>Имя порта</summary>
        public string PortName
        {
            get { return portName; }
            set { SetProperty(ref portName, value); }
        }

        int baudRate;
        /// <summary>Скорость</summary>
        public int BaudRate
        {
            get { return baudRate; }
            set { SetProperty(ref baudRate, value); }
        }

        int dataBits;
        /// <summary>Количество бит</summary>
        public int DataBits
        {
            get { return dataBits; }
            set { SetProperty(ref dataBits, value); }
        }

        Parity parity;
        /// <summary>Бит чётности</summary>
        public Parity Parity
        {
            get { return parity; }
            set { SetProperty(ref parity, value); }
        }

        StopBits stopBits;
        /// <summary>Стоп-бит</summary>
        public StopBits StopBits
        {
            get { return stopBits; }
            set { SetProperty(ref stopBits, value); }
        }

        Handshake handshake;
        /// <summary>Подтверждение</summary>
        public Handshake Handshake
        {
            get { return handshake; }
            set { SetProperty(ref handshake, value); }
        }

        public static SerialParameters Default => new SerialParameters
        {
            PortName = "COM1",
            baudRate = 9600,
            dataBits = 8,
            parity = Parity.None,
            stopBits = StopBits.One,
            handshake = Handshake.None
        };        
    }
}
