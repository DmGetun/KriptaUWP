using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UWP.Помошники
{
    class Error : Exception
    {
        public static string MissingKey { get; } = "Ошибка: Отсутствует ключ";
        public static string StepNumber { get; } = "Ошибка: Шаг должен быть числом";
        public static string InvalidKey { get; } = "Ошибка: Недопустимый символ ключа";
        public static string KeySingleCharacter { get; } = "Ошибка: Ключ должен быть односимвольным";
        public static string MatrixSize { get; } = "Ошибка: Неверная размерность матрицы";
        public static string MatrixValue { get; internal set; } = "Ошибка: Матрица не является вырожденной";
        public static string WrongSymbol { get; } = "Ошибка: Недопустимый символ";
        public static string LimitKeyCezar { get; internal set; } = "Ошибка: Значение ключа должно быть положительным и быть меньше 32.";
        public static string KeyLength64bit { get; internal set; } = "Ошибка: Длина ключа должна быть равна 64 битам";
        public static string KeyLength32Byte { get; internal set; } = "Ошибка: Длина ключа должна быть равна 32 байтам (16 символов).";
        public static string AesLengthKey { get; internal set; } = "Ошибка: Длина ключа должна быть равна 16,24 или 32 байта";
        public static string InvalidValueKey { get; internal set; } = "Ошибка: Неверный формат ключа";

        public Error(string message) : base(message)
        {

        }

        public Error() : base()
        {

        }
    }
}
