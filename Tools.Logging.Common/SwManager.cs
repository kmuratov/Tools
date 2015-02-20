using System;

namespace Tools.Logging.Common
{
    /// <summary>
    /// Обработчик результата для формирования строки для записи в лог
    /// </summary>
    /// <typeparam name="TResult">Тип результата</typeparam>
    /// <param name="result">Результат</param>
    /// <returns>Строка для записи в лог</returns>
    public delegate string StopwatchResultHandler<in TResult>(TResult result);

    public static class SwManager
    {
        public static ISwContext<TResult> Start<TResult>(string name, StopwatchResultHandler<TResult> resultHandler = null, Action<string> writeToLog = null)
        {
            return new SwContext<TResult>(name, resultHandler, writeToLog);
        }

        public static IDisposable Start(string name, Action<string> writeToLog = null)
        {
            return new SwContext<object>(name, null, writeToLog);
        }
    }
}
