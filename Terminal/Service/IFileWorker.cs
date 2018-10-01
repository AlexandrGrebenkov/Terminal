using System;
using Terminal.Models;

namespace Terminal.Service
{
    /// <summary>Работа с файловой системой</summary>
    public interface IFileWorker
    {
        /// <summary>Сохранение параметров порта</summary>
        /// <param name="parameters">Параметры порта</param>
        /// <param name="errorHandler">Обработчик ошибок</param>
        void SaveSettings(SerialParameters parameters, Action<string> errorHandler = null);

        /// <summary>Загрузка параметров порта</summary>
        /// <param name="errorHandler">Обработчик ошибок</param>
        /// <returns>Параметры порта - при успешном чтении, null - если не удалось прочитать</returns>
        SerialParameters LoadSettings(Action<string> errorHandler = null);
    }
}
