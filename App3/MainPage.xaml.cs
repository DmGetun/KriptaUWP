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
using Windows.UI;
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
        Dictionary<string, Dictionary<string, Algorithm>> dictItems;

        public List<Button> AllButtons { get; private set; }

        public MainPage()
        {
            this.InitializeComponent();
            this.Measure(new Windows.Foundation.Size(double.PositiveInfinity, double.PositiveInfinity));
            this.Arrange(new Rect(0, 0, this.DesiredSize.Width, this.DesiredSize.Height));
            listAlgorithm = new List<Algorithm>();
            dictAlgorithm = new Dictionary<string, Algorithm>();
            dictItems = new Dictionary<string, Dictionary<string, Algorithm>>();
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
            text = text.Replace(" ", "ПРБЛ").Replace(",", "ЗПТ").Replace(".", "ТЧК").Replace("-","ТИРЕ");
            text = text.Replace("\r\r","ДАБЛНОВСТР").Replace("\r", "НОВСТР");
            if (algoritm.Name == "Шифр Плейфера")
            {
                text = text.Replace('Ъ', 'Ь').Replace('Й', 'И').Replace('Ё', 'Е');
                text = text.Replace('ъ', 'ь').Replace('й', 'и').Replace('ё', 'е');
            }
            StringBuilder str = new StringBuilder();
            foreach (char s in text)
            {
                if ((s >= 'А' && s <= 'Я') || (s >= 'а' && s <= 'я') || (s >= '0' && s <='9'))
                    str.Append(s);
            }

            return str.ToString();
        }

        private string ReplaceTextAfterDecrypt(string text)
        {
            return text.Replace("ПРБЛ", " ").Replace("ЗПТ", ",").Replace("ТЧК", ".").Replace("ТИРЕ","-").Replace("ДАБЛНОВСТР","\r\r").Replace("НОВСТР","\r");
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
            listAlgorithm.Add(new Atbash()); // Шифр АТБАШ 0
            listAlgorithm.Add(new Cezar()); // Шифр Цезаря 1
            listAlgorithm.Add(new Polibii()); // шифр Квадрат полибия 2
            listAlgorithm.Add(new Tritemy()); // Шифр Тритемия 3
            listAlgorithm.Add(new Belazo()); // Шифр Белазо 4
            listAlgorithm.Add(new Visener()); // Шифр Виженера 5
            listAlgorithm.Add(new Matrics()); // Матричный шифр 6
            listAlgorithm.Add(new Plaifer()); // Шифр Плейфера 7
            listAlgorithm.Add(new VertSwapa()); // Шифр вертикальной перестановки 8
            listAlgorithm.Add(new Kardano()); // Шифр Кардано 9
            listAlgorithm.Add(new Bloknot()); // Шифр Блокнот Шеннона 10
            listAlgorithm.Add(new A51()); // A5/1 шифр 11
            listAlgorithm.Add(new A52()); // A5/2 шифр 12
            listAlgorithm.Add(new Magma()); // Магма 13
            listAlgorithm.Add(new Kuznec()); // Кузнечик 14
            listAlgorithm.Add(new AES()); // Elgamal шифр 15
            listAlgorithm.Add(new RSA()); // RSA шифр 16
            listAlgorithm.Add(new Elgamal()); // Elgamal шифр 17
            listAlgorithm.Add(new ECC()); // ECC шифр 18
            listAlgorithm.Add(new RSACP()); // ЦП RSA 19
            listAlgorithm.Add(new ElgamalCP()); // ЦП Elgamal 20
            listAlgorithm.Add(new GOST341094()); // ГОСТ Р 34.10-94 21
            listAlgorithm.Add(new GOST34102012()); // ГОСТ Р 34.10-94 22
            listAlgorithm.Add(new DiffieHellman()); // Обмен ключами 23

            foreach (Algorithm alg in listAlgorithm)
            {
                string name = alg.Name;
                dictAlgorithm.Add(name, alg);
            }
            List<Dictionary<string, Algorithm>> a = new List<Dictionary<string, Algorithm>>();
            foreach(var key in dictAlgorithm.Keys)
            {
                Dictionary<string, Algorithm> t = new Dictionary<string, Algorithm>();
                t.Add(key, dictAlgorithm[key]);
            }
/*            dictItems.Add("Шифры Многозначной замены", a[3]);
            dictItems.Add("Шифры Многозначной замены", a[4]);
            dictItems.Add("Шифры Многозначной замены", a[5]);
            dictItems.Add("Шифры Блочной замены", a[6]);
            dictItems.Add("Шифры Блочной замены", a[7]);
            dictItems.Add("Шифры перестановки", a[8]);
            dictItems.Add("Шифры перестановки", a[9]);
            dictItems.Add("Шифры гаммирования", a[10]);
            dictItems.Add("Поточные шифры", a[11]); // A5/1
            dictItems.Add("Поточные шифры", a[12]);
            dictItems.Add("Комбинационные шифры", a[13]);
            dictItems.Add("Комбинационные шифры", a[14]);
            dictItems.Add("Комбинационные шифры", a[15]);
            dictItems.Add("Асимметричные шифры", a[16]);
            dictItems.Add("Асимметричные шифры", a[17]);
            dictItems.Add("Асимметричные шифры", a[18]);
            dictItems.Add("Цифровые подписи", a[19]);
            dictItems.Add("Цифровые подписи", a[20]);
            dictItems.Add("ГОСТ ЦП", a[21]);
            dictItems.Add("ГОСТ ЦП", a[22]);
            dictItems.Add("Обмен ключами", a[23]);*/

            CreateButtons();
        }

        private void CreateButtons()
        {
            StackPanel panel = new StackPanel();
            AllButtons = new List<Button>();
            OdnZamena();
            MnogoZamena();
            BlockCipher();
            TransCipher();
            GammaCipher();
            StreamCipher();
            CombCipher();
            AsymmetricCipher();
            CPCipher();
            GostCipher();
            SwapCipher();
        }

        private void SwapCipher()
        {
            MenuFlyoutItem diffhell = new MenuFlyoutItem();
            diffhell.Text = "Диффи-Хеллман";
            MenuFlyout odnZamena = new MenuFlyout();
            odnZamena.Items.Add(diffhell);
            Button buttonZamen = new Button();
            buttonZamen.Name = "Обмен ключами";
            buttonZamen.Content = "Обмен ключами";
            buttonZamen.Flyout = odnZamena;
            AllButtons.Add(buttonZamen);
        }

        private void GostCipher()
        {
            MenuFlyoutItem gost94 = new MenuFlyoutItem();
            gost94.Text = "ГОСТ Р 34.10-94";
            MenuFlyoutItem gost12 = new MenuFlyoutItem();
            gost12.Text = "ГОСТ Р 34.10-2012";
            MenuFlyout odnZamena = new MenuFlyout();
            odnZamena.Items.Add(gost94);
            odnZamena.Items.Add(gost12);
            Button buttonZamen = new Button();
            buttonZamen.Name = "ЦП ГОСТ Р";
            buttonZamen.Content = "ЦП ГОСТ Р";
            buttonZamen.Flyout = odnZamena;
            AllButtons.Add(buttonZamen);
        }

        private void CPCipher()
        {
            MenuFlyoutItem rsa = new MenuFlyoutItem();
            rsa.Text = "ЦП RSA";
            MenuFlyoutItem elgamal = new MenuFlyoutItem();
            elgamal.Text = "ЦП Elgamal";
            MenuFlyout odnZamena = new MenuFlyout();
            odnZamena.Items.Add(rsa);
            odnZamena.Items.Add(elgamal);
            Button buttonZamen = new Button();
            buttonZamen.Name = "Поточные шифры";
            buttonZamen.Content = "Поточные шифры";
            buttonZamen.Flyout = odnZamena;
            AllButtons.Add(buttonZamen);
        }

        private void AsymmetricCipher()
        {
            MenuFlyoutItem rsa = new MenuFlyoutItem();
            rsa.Text = "Шифр RSA";
            MenuFlyoutItem elgamal = new MenuFlyoutItem();
            elgamal.Text = "Шифр Elgamal";
            MenuFlyoutItem ecc = new MenuFlyoutItem();
            ecc.Text = "Шифр ЕСС";
            MenuFlyout odnZamena = new MenuFlyout();
            odnZamena.Items.Add(rsa);
            odnZamena.Items.Add(elgamal);
            odnZamena.Items.Add(ecc);
            Button buttonZamen = new Button();
            buttonZamen.Name = "Асимметричные шифры";
            buttonZamen.Content = "Асимметричные шифры";
            buttonZamen.Flyout = odnZamena;
            AllButtons.Add(buttonZamen);
        }

        private void CombCipher()
        {
            MenuFlyoutItem magma = new MenuFlyoutItem();
            magma.Text = "Шифр Магма";
            MenuFlyoutItem aes = new MenuFlyoutItem();
            aes.Text = "Шифр AES";
            MenuFlyoutItem kuznechik = new MenuFlyoutItem();
            kuznechik.Text = "Шифр Кузнечик";
            MenuFlyout odnZamena = new MenuFlyout();
            odnZamena.Items.Add(magma);
            odnZamena.Items.Add(aes);
            odnZamena.Items.Add(kuznechik);
            Button buttonZamen = new Button();
            buttonZamen.Name = "Комбинационные шифры";
            buttonZamen.Content = "Комбинационные шифры";
            buttonZamen.Flyout = odnZamena;
            AllButtons.Add(buttonZamen);
        }

        private void StreamCipher()
        {
            MenuFlyoutItem a51 = new MenuFlyoutItem();
            a51.Text = "Шифр A5/1";
            MenuFlyoutItem a52 = new MenuFlyoutItem();
            a52.Text = "Шифр A5/2";
            MenuFlyout odnZamena = new MenuFlyout();
            odnZamena.Items.Add(a51);
            odnZamena.Items.Add(a52);
            Button buttonZamen = new Button();
            buttonZamen.Name = "Поточные шифры";
            buttonZamen.Content = "Поточные шифры";
            buttonZamen.Flyout = odnZamena;
            AllButtons.Add(buttonZamen);
        }

        private void GammaCipher()
        {
            MenuFlyoutItem bloknot = new MenuFlyoutItem();
            bloknot.Text = "Шифр вертикальной перестановки";
            MenuFlyout odnZamena = new MenuFlyout();
            odnZamena.Items.Add(bloknot);
            Button buttonZamen = new Button();
            buttonZamen.Name = "Шифры гаммирования";
            buttonZamen.Content = "Шифры гаммирования";
            buttonZamen.Flyout = odnZamena;
            AllButtons.Add(buttonZamen);
        }

        private void TransCipher()
        {
            MenuFlyoutItem vert = new MenuFlyoutItem();
            vert.Text = "Шифр вертикальной перестановки";
            MenuFlyoutItem kardano = new MenuFlyoutItem();
            kardano.Text = "Шифр Решетка Кардано";
            MenuFlyout odnZamena = new MenuFlyout();
            odnZamena.Items.Add(vert);
            odnZamena.Items.Add(kardano);
            Button buttonZamen = new Button();
            buttonZamen.Name = "Шифры блочной замены";
            buttonZamen.Content = "Шифры блочной замены";
            buttonZamen.Flyout = odnZamena;
            AllButtons.Add(buttonZamen);
        }

        private void BlockCipher()
        {
            MenuFlyoutItem matric = new MenuFlyoutItem();
            matric.Text = "Матричный шифр";
            MenuFlyoutItem plaifer = new MenuFlyoutItem();
            plaifer.Text = "Шифр Плэйфера";
            MenuFlyout odnZamena = new MenuFlyout();
            odnZamena.Items.Add(matric);
            odnZamena.Items.Add(plaifer);
            Button buttonZamen = new Button();
            buttonZamen.Name = "Шифры блочной замены";
            buttonZamen.Content = "Шифры блочной замены";
            buttonZamen.Flyout = odnZamena;
            AllButtons.Add(buttonZamen);
        }

        private void MnogoZamena()
        {
            MenuFlyoutItem tritemi = new MenuFlyoutItem();
            tritemi.Text = "Шифр Тритемия";
            MenuFlyoutItem belazo = new MenuFlyoutItem();
            belazo.Text = "Шифр Белазо";
            MenuFlyoutItem visener = new MenuFlyoutItem();
            visener.Text = "Шифр Виженера";
            MenuFlyout odnZamena = new MenuFlyout();
            odnZamena.Items.Add(tritemi);
            odnZamena.Items.Add(belazo);
            odnZamena.Items.Add(visener);
            Button buttonZamen = new Button();
            buttonZamen.Name = "Шифры многозначной замены";
            buttonZamen.Content = "Шифры многозначной замены";
            buttonZamen.Flyout = odnZamena;
            AllButtons.Add(buttonZamen);
        }

        private void OdnZamena()
        {
            MenuFlyoutItem atbash = new MenuFlyoutItem();
            atbash.Text = "Шифр Атбаш";
            MenuFlyoutItem cezar = new MenuFlyoutItem();
            cezar.Text = "Шифр Цезаря";
            MenuFlyoutItem polibii = new MenuFlyoutItem();
            polibii.Text = "Шифр Полибия";
            MenuFlyout odnZamena = new MenuFlyout();
            odnZamena.Items.Add(atbash);
            odnZamena.Items.Add(cezar);
            odnZamena.Items.Add(polibii);
            Button buttonZamen = new Button();
            buttonZamen.Name = "Шифры однозначной замены";
            buttonZamen.Content = "Шифр однозначной замены";
            buttonZamen.Flyout = odnZamena;
            AllButtons.Add(buttonZamen);
        }

        private void CreateAlgorithms()
        {
            PushAlgorithms();
            foreach(var item in AllButtons)
            {
                item.VerticalAlignment = VerticalAlignment.Top;
                ButtonGrid.Children.Add(item);
            }
        }

        private void SetAlgorithm(string name)
        {
            algoritm = dictAlgorithm[name];
            AlgName.Text = $"Текущий алгоритм шифрования: {name}";
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
                    button.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 0));
                }
                else
                {
                    button.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                }
            }
        }
    }
}
