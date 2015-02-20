using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace Tools.Logging.Common
{
    internal static class TimeLog<TKey>
    {
        /// <summary>
        /// Константа длина отступа в логе при форматировании
        /// </summary>
        private const int Shift = 2;

        /// <summary>
        /// Обертка над таймером
        /// </summary>
        private class WrStopWatch : Stopwatch
        {
            /// <summary>
            /// Идентификатор таймера
            /// </summary>
            public int Index;

            /// <summary>
            /// Уровень таймера для форматирования результата
            /// </summary>
            public int Level;

            /// <summary>
            /// Имя таймера для лога
            /// </summary>
            public string Name;
        }

        /// <summary>
        /// Словарь таймеров
        /// </summary>
        private static readonly ThreadLocal<IDictionary<TKey, WrStopWatch>> Sw = new ThreadLocal<IDictionary<TKey, WrStopWatch>>();

        /// <summary>
        /// Текущий индекс
        /// </summary>
        private static readonly ThreadLocal<int> CurrentIndex = new ThreadLocal<int>();

        /// <summary>
        /// Текущий уровень
        /// </summary>
        private static readonly ThreadLocal<int> CurrentLevel = new ThreadLocal<int>();

        /// <summary>
        /// Метод запускает таймер
        /// </summary>
        /// <param name="key">Идентификатор таймера</param>
        /// <param name="name">Имя метода</param>
        public static void Start(TKey key, string name)
        {
            if (!Sw.IsValueCreated)
                Sw.Value = new Dictionary<TKey, WrStopWatch>();
                
            var sw = Sw.Value;
            WrStopWatch val;
            if (!sw.TryGetValue(key, out val))
            {
                val = new WrStopWatch();
                sw.Add(key, val);
            }

            val.Index = CurrentIndex.Value++;
            val.Level = CurrentLevel.Value++;
            val.Name = name;

            val.Start();
        }

        /// <summary>
        /// Метод останавливает таймер и делает запись в собственный лог
        /// </summary>
        /// <param name="key">Идентификатор таймера</param>
        public static void Stop(TKey key)
        {
            CurrentLevel.Value--;

            Sw.Value[key].Stop();
        }

        /// <summary>
        /// Метод добавляет запись в лог
        /// </summary>
        /// <param name="key">Идентификатор таймера</param>
        /// <param name="message">Сообщение</param>
        public static void Add(TKey key, string message)
        {
            Start(key, message);
            Stop(key);
        }

        /// <summary>
        /// Метод выполняющий сброс в лог
        /// </summary>
        /// <param name="writeToLog">метод для записи в лог</param>
        public static void Log(Action<string> writeToLog)
        {
            var sb = new StringBuilder();

            sb.AppendLine(Environment.NewLine);
            foreach (var sw in Sw.Value.Values.OrderBy(x => x.Index))
                sb.AppendLine(String.Format("{0}: {1}", sw.Elapsed, sw.Name.PadLeft(sw.Name.Length + sw.Level * Shift, ' ')));

            writeToLog(sb.ToString());
            Sw.Value.Clear();
        }
    }
}