using System;
using System.Threading;

namespace Tools.Logging.Common
{
    /// <summary>
    /// Интерфейс описывает контекст обработчика результата для формирования строки для записи в лог
    /// </summary>
    /// <typeparam name="TResult">Тип результата</typeparam>
    public interface ISwContext<TResult> : IDisposable
    {
        /// <summary>
        /// Результат, может использоваться для логирования
        /// </summary>
        TResult Result { get; set; }

        /// <summary>
        /// Метод для добавления сообщения в лог
        /// </summary>
        /// <param name="msg">Сообщение</param>
        void Append(string msg);
    }

    /// <summary>
    /// Базовый контекст
    /// </summary>
    public abstract class BaseSwContext
    {
        /// <summary>
        /// Счетчик созданных контекстов, в базовом классе, чтобы был один на все дженерики
        /// </summary>
        protected static readonly ThreadLocal<int> Counter = new ThreadLocal<int>();
    }

    /// <summary>
    /// Контекст
    /// </summary>
    /// <typeparam name="TResult">Тип результата</typeparam>
    public class SwContext<TResult> : BaseSwContext, ISwContext<TResult>
    {
        /// <summary>
        /// Результат, может использоваться для логирования
        /// </summary>
        public TResult Result { get; set; }

        /// <summary>
        /// Идентификатор
        /// </summary>
        private readonly Guid _key;

        /// <summary>
        /// Обработчик результата для формирования строки для записи в лог
        /// </summary>
        private readonly StopwatchResultHandler<TResult> _resultHandler;

        /// <summary>
        /// Метод для сброса в лог
        /// </summary>
        private readonly Action<string> _writeToLog;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="message">Сообщение</param>
        /// <param name="resultHandler">Обработчик результата</param>
        /// <param name="writeToLog">Метод для сброса в лог</param>
        public SwContext(string message, StopwatchResultHandler<TResult> resultHandler = null, Action<string> writeToLog = null)
        {
            if (Counter.Value == 0 && writeToLog == null)
                throw new ArgumentNullException("writeToLog");

            _resultHandler = resultHandler;
            _writeToLog = writeToLog;
            _key = Guid.NewGuid();

            Counter.Value++;

            TimeLog<Guid>.Start(_key, message);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            TimeLog<Guid>.Stop(_key);

            Counter.Value--;

            //Если есть обработчик результата, выполняем
            if (_resultHandler != null)
                TimeLog<Guid>.Add(_key, _resultHandler(Result));

            //если указан способ записи в лог,
            //если последний контекст в потоке,
            //то выполняем
            if (_writeToLog != null && Counter.Value == 0)
                TimeLog<Guid>.Log(_writeToLog);
        }

        /// <summary>
        /// Метод для добавления сообщения в лог
        /// </summary>
        /// <param name="message">Сообщение</param>
        public void Append(string message)
        {
            TimeLog<Guid>.Add(_key, message);
        }
    }
}
