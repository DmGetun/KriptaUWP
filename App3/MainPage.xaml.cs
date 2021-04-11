using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using UWP.Алгоритмы;
using UWP.Помошники;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x419

namespace App3
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        List<Algorithm> listAlgorithm;
        Dictionary<string, Algorithm> dictAlgorithm;
        string cypher = string.Empty;
        string plain = string.Empty;
        Algorithm algoritm;
        Config config;
        string text = string.Empty;
        Brush defaultColorButton;
        public MainPage()
        {
            this.InitializeComponent();
            this.Measure(new Windows.Foundation.Size(double.PositiveInfinity, double.PositiveInfinity));
            this.Arrange(new Rect(0, 0, this.DesiredSize.Width, this.DesiredSize.Height));
            listAlgorithm = new List<Algorithm>();
            dictAlgorithm = new Dictionary<string, Algorithm>();
            CreateAlgorithms();
            this.SizeChanged += MainPage_SizeChanged;
            var color = new Button().Background;
            defaultColorButton = color;
            algoritm = listAlgorithm[1];
            config = new Config();
            OriginalText.TextChanged += OriginalText_TextChanged;
            cypherTextArea.TextChanged += CypherTextArea_TextChanged;
            GenerateKey.Click += GenerateKey_Click;
            KeyArea.TextChanged += KeyArea_TextChanged;
        }

        private void KeyArea_TextChanged(object sender, TextChangedEventArgs e)
        {
            OriginalText_TextChanged(null, null);
        }

        private void GenerateKey_Click(object sender, RoutedEventArgs e)
        {
            string key = algoritm.GenerateKey();
            if (key == null) return;
            KeyArea.Text = key;
            OriginalText_TextChanged(null, null);
        }

        private void CypherTextArea_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                config.Key = KeyArea.Text;
                cypher = cypherTextArea.Text;
                if (string.IsNullOrEmpty(cypher)) return;
                plain = algoritm.Decrypt(cypher, config);
                if (algoritm.IsReplaceText)
                    plain = ReplaceTextAfterDecrypt(plain);
                plainTextArea.Text = plain;
                string keyView = algoritm.KeyView();
                ErrorArea.Text = keyView;
            }
            catch (Error error)
            {
                ErrorArea.Text = error.Message;
            }
        }

        private void OriginalText_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                Reset();

                text = OriginalText.Text;

                if (algoritm.IsReplaceText)
                    text = ReplaceTextBeforeEncrypt(text);

                if (string.IsNullOrEmpty(text)) return;

                if (algoritm.IsReplaceText)
                    text = text.ToUpper();

                config.Key = KeyArea.Text;
                ViewText.Text = text;
                cypher = algoritm.Encrypt(text, config);
                cypherTextArea.Text = cypher;
            }
            catch (Error error)
            {
                ErrorArea.Text = error.Message;
            }
        }

        private string ReplaceTextBeforeEncrypt(string text)
        {
            text = text.Replace(" ", "ПРБЛ").Replace(",", "ЗПТ").Replace(".", "ТЧК");
            if (algoritm.Name == "Шифр Плейфера")
            {
                text = text.Replace('Ъ', 'Ь').Replace('Й', 'И').Replace('Ё', 'Е');
                text = text.Replace('ъ', 'ь').Replace('й', 'и').Replace('ё', 'е');
            }
            StringBuilder str = new StringBuilder();
            foreach (char s in text)
            {
                if ((s >= 'А' && s <= 'Я') || (s >= 'а' && s <= 'я'))
                    str.Append(s);
            }

            return str.ToString();
        }

        private string ReplaceTextAfterDecrypt(string text)
        {
            return text.Replace("ПРБЛ", " ").Replace("ЗПТ", ",").Replace("ТЧК", ".");
        }

        private void MainPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            int i = 0;
            foreach (object obj in ButtonGrid.Children)
            {
                Button button = obj as Button;
                if (button == null) continue;
                button.Width = ButtonGrid.ActualWidth - ContentArea.ActualWidth;
                button.Height = ButtonGrid.ActualHeight / listAlgorithm.Count;
                button.Margin = new Thickness(0, button.Height * i++, 0, 0);
            }
        }

        private void PushAlgorithms()
        {
            listAlgorithm.Add(new Atbash()); // Шифр АТБАШ
            listAlgorithm.Add(new Cezar()); // Шифр Цезаря
            listAlgorithm.Add(new Polibii()); // шифр Квадрат полибия
            listAlgorithm.Add(new Tritemy()); // Шифр Тритемия
            listAlgorithm.Add(new Belazo()); // Шифр Белазо
            listAlgorithm.Add(new Visener()); // Шифр Виженера
            listAlgorithm.Add(new Matrics()); // Матричный шифр
            listAlgorithm.Add(new Plaifer()); // Шифр Плейфера
            listAlgorithm.Add(new VertSwapa()); // Шифр вертикальной перестановки
            listAlgorithm.Add(new Bloknot()); // Шифр Блокнот Шеннона
            listAlgorithm.Add(new Magma()); // Шифр Магма
            listAlgorithm.Add(new Kardano()); // Шифр Кардано
            listAlgorithm.Add(new GammaGost28147()); // Гост 28147-89
            listAlgorithm.Add(new A51()); // A5/1 шифр
            listAlgorithm.Add(new A52()); // A5/2 шифр
            listAlgorithm.Add(new Kuznec()); // Кузнечик
            listAlgorithm.Add(new RSA()); // RSA шифр
            listAlgorithm.Add(new Elgamal()); // Elgamal шифр
            listAlgorithm.Add(new AES()); // Elgamal шифр
            listAlgorithm.Add(new ECC()); // ECC шифр
            listAlgorithm.Add(new RSACP()); // ЦП RSA
            listAlgorithm.Add(new ElgamalCP()); // ЦП Elgamal
            listAlgorithm.Add(new GOST28147()); // ГОСТ 28147-89
            listAlgorithm.Add(new GOST341094()); // ГОСТ Р 34.10-94
            listAlgorithm.Add(new GOST34102012()); // ГОСТ Р 34.10-94
            listAlgorithm.Add(new DiffieHellman()); // Обмен ключами

            foreach (Algorithm alg in listAlgorithm)
            {
                string name = alg.Name;
                dictAlgorithm.Add(name, alg);
            }
        }

        private void CreateAlgorithms()
        {
            PushAlgorithms();

            for (int i = 0; i < listAlgorithm.Count; i++)
            {
                Algorithm algoritm = listAlgorithm[i];
                Button button = new Button();
                button.Name = algoritm.Name;
                button.Content = algoritm.Name;
                button.VerticalAlignment = VerticalAlignment.Top;
                button.Width = ButtonGrid.ActualWidth - ContentArea.ActualWidth;
                button.Height = ButtonGrid.ActualHeight / listAlgorithm.Count;
                button.Margin = new Thickness(0, button.Height * i, 0, 0);
                button.Click += Button_Click;
                ButtonGrid.Children.Add(button);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button == null) return;

            string name = button.Name;
            SetAlgorithm(name);
            KeyArea.Text = algoritm.DefaultKey;
            if (algoritm.DefaultKey == Algorithm.NonKey)
                KeyArea.IsReadOnly = true;
            else
                KeyArea.IsReadOnly = false;
        }

        private void SetAlgorithm(string name)
        {
            algoritm = dictAlgorithm[name];
            Reset();
        }

        private void Reset()
        {
            ErrorArea.Text = string.Empty;
            ViewText.Text = string.Empty;
            KeyArea.IsReadOnly = false;
            foreach (object obj in ButtonGrid.Children)
            {
                Button button = obj as Button;
                if (button == null) continue;
                if (algoritm.Name.Equals(button.Name))
                {
                    //button.Background = new SolidColorBrush(Color.FromArgb(255, 250, 218, 221));
                }
                else
                {

                }
            }
        }
    }
}
