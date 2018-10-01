using System;
using System.IO;
using System.Xml.Serialization;
using Terminal.Models;

namespace Terminal.Service
{
    class FileWorker : IFileWorker
    {
        public SerialParameters LoadSettings(Action<string> errorHandler = null)
        {
            SerialParameters parameters = null;
            try
            {
                using (var fs = new FileStream("Parameters.xml", FileMode.Open, FileAccess.Read))
                using (var reader = new StreamReader(fs))
                {
                    var xs = new XmlSerializer(typeof(SerialParameters));
                    parameters = (SerialParameters)xs.Deserialize(reader);
                }
            }
            catch (Exception ex)
            {
                errorHandler?.Invoke($"Ошибка загрузки параметров: {ex.Message}");
            }
            return parameters;
        }

        public void SaveSettings(SerialParameters parameters, Action<string> errorHandler = null)
        {
            try
            {
                using (var fs = new FileStream("Parameters.xml", FileMode.Create, FileAccess.Write))
                using (var writer = new StreamWriter(fs))
                {
                    var xs = new XmlSerializer(typeof(SerialParameters));
                    xs.Serialize(writer, parameters);
                }
            }
            catch (Exception ex)
            {
                errorHandler?.Invoke($"Ошибка сохранения параметров: {ex.Message}");
            }
        }
    }
}
