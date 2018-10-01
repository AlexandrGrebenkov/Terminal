using System;
using System.Collections.Generic;
using Terminal.Models;

namespace Terminal.Service
{
    public interface ISerial
    {
        bool IsConnected { get; }

        event Action<bool> ConnectionChanged;
        event Action<string> DataReceived;

        void ClearRx();
        void Connect(SerialParameters parameters, Action<string> errorHandler = null);
        void Disonnect(Action<string> errorHandler = null);
        void Write(string data, Action<string> errorHandler = null);

        string[] PortNames { get; }
    }
}