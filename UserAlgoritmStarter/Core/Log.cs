using System;
using System.Collections.Generic;

namespace TogiSoft.IO
{
    /// <summary>
    /// Логирование
    /// </summary>
    public static class Log
    {

        /// <summary>
        /// Логгер NLOG
        /// </summary>
        private static LogWrapper logger = new LogWrapper();

        /// <summary>
        /// Папка с логами
        /// </summary>
        public static string Folder
        {
            get
            {
                return logger.LogFolder;
            }
        }

        /// <summary>
        /// Включить выключить подробное ведение логов
        /// </summary>
        public static bool DebugLogsEnabled { get; set; }

        /// <summary>
        /// Вывод сообщения об ошибке с информацией об исключении
        /// </summary>
        /// <param name="message"> Текстовое сообщение </param>
        /// <param name="ex"> Ошибка </param>
        public static void Error(string message, Exception ex = null)
        {
            logger.Error(message, ex);
        }

        /// <summary>
        /// Вывод информационного сообщения
        /// </summary>
        /// <param name="message"> Текстовое сообщение </param>
        /// <param name="sendToControls"> Флаг визуального отображения сообщения для пользователя в листе сообщений </param>
        public static void Info(string message)
        {
            logger.Info(message);
        }

        /// <summary>
        /// Вывод предупреждения
        /// </summary>
        /// <param name="message"> Текстовое сообщение </param>
        /// <param name="sendToControls"> Флаг визуального отображения сообщения для пользователя в листе сообщений </param>
        public static void Warning(string message)
        {
            logger.Warn(message);
        }
    }
}