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

namespace App3
{
    public sealed partial class MainPage : Page
    {
        List<Algorithm> listAlgorithm;
        Dictionary<string, Algorithm> dictAlgorithm;
        string cypher = string.Empty;
        string plain = string.Empty;
        Algorithm algoritm;
        Config config;
        string text = string.Empty;

        public List<Button> AllButtons { get; private set; }

        public MainPage()
        {
            this.InitializeComponent();
            this.Measure(new Windows.Foundation.Size(double.PositiveInfinity, double.PositiveInfinity));
            this.Arrange(new Rect(0, 0, this.DesiredSize.Width, this.DesiredSize.Height));
            listAlgorithm = new List<Algorithm>();
            dictAlgorithm = new Dictionary<string, Algorithm>();
            CreateAlgorithms();
            this.SizeChanged += MainPage_SizeChanged;
            config = new Config();
            OriginalText.TextChanged += OriginalText_TextChanged;
            cypherTextArea.TextChanged += CypherTextArea_TextChanged;
            KeyArea.TextChanged += KeyArea_TextChanged;
            AlgName.Text = "Текущий алгоритм шифрования: Шифр Цезаря";
            ModName.Visibility = Visibility.Collapsed;
            ModText.Visibility = Visibility.Collapsed;
            AddChangeMod();
        }

        /*
            Добавить режимы работы Магмы в поле выбора режима 
        */
        private void AddChangeMod()
        {
            ModName.SelectionChanged += ModName_SelectionChanged;
            ModName.Items.Add("Режим простой замены");
            ModName.Items.Add("Режим гаммирования");
            ModName.Items.Add("Режим гаммирования по выходу");
            ModName.Items.Add("Режим гаммирования по шифртексту");
            ModName.SelectedItem = ModName.Items[0];
            ModName.SelectedValue = "Режим простой замены";
        }
        /*
            Изменить текущий режим работы Магмы 
        */
        private void ModName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox item = sender as ComboBox;
            if (item == null) return;
            if (algoritm.Name != new Magma().Name) return;

            string name = item.SelectedValue.ToString();
            config.Mode = name;
            OriginalText_TextChanged(null,null);
        }
        /*
            Событие при изменении содержимого поля ключа.
            Вызывает событие смены содержимого поля открытого текста
            или поля зашифрованного текста
        */
        private void KeyArea_TextChanged(object sender, TextChangedEventArgs e)
        {
            text = OriginalText.Text;

            if (!string.IsNullOrEmpty(text))
                OriginalText_TextChanged(null, null);
            else
                CypherTextArea_TextChanged(null, null);
        }
        /*
            Событие нажатия на кнопку генерации ключа.
            Вызывает функцию генерации ключа текущего алгоритма 
            и устанавливает полученный ключ в поле ключа.
        */
        private void GenerateKey_Click(object sender, RoutedEventArgs e)
        {
            string key = algoritm.GenerateKey();
            if (key == null) return;
            KeyArea.Text = key;
        }
        /*
            Событие при изменении содержимого поля шифрованного текста.
            Получает значения ключа и зашифрованного текста,
            передает это в функцию дешифрования алгоритма,
            при необходимости производит обработку полученного текста,
            устанавливает расшифрованный текст в поле расшифрованного текста.
            Выводит внутренний вид ключа в поле Вид ключа.
        */
        private void CypherTextArea_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                config.Key = KeyArea.Text;
                cypher = cypherTextArea.Text;
                if (string.IsNullOrEmpty(cypher)) return;
                plain = algoritm.Decrypt(cypher, config);
                if (algoritm.IsReplaceText)
                    plain = Alphabet.ReplaceTextAfterDecrypt(plain);
                plainTextArea.Text = plain;
                string keyView = algoritm.KeyView();
                ErrorArea.Text = keyView;
            }
            catch (Error error)
            {
                ErrorArea.Text = error.Message;
            }
        }
        /*
            Событие при изменении поля открытого текста.         
        */
        private void OriginalText_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                Reset();

                text = OriginalText.Text;
                if (string.IsNullOrEmpty(text)) return;
                if (algoritm.IsReplaceText)
                {
                    text = Alphabet.ReplaceTextBeforeEncrypt(text, algoritm);
                    text = text.ToUpper();
                }

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
        /*
            Событие изменения размера окна программы.
            Меняет размеры элементов.
        */
        private void MainPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            int i = 0;
            foreach (object obj in ButtonGrid.Children)
            {
                Button button = obj as Button;
                if (button == null) continue;
                button.Width = ButtonGrid.ActualWidth - ContentArea.ActualWidth;
                button.Height = (ButtonGrid.ActualHeight / AllButtons.Count) - 10;
                button.Margin = new Thickness(0, button.Height * i++, 0, 0);
            }
        }
        /*
            Функция добавления всех алгоритмов в общий список.
            Добавляем алгоритмы в общий список,
            создаем словарь сопоставления имени и класса
            для дальнейшего переключения между алгоритмами.
        */
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
            foreach(var key in dictAlgorithm.Keys)
            {
                Dictionary<string, Algorithm> t = new Dictionary<string, Algorithm>();
                t.Add(key, dictAlgorithm[key]);
            }
            algoritm = dictAlgorithm["Шифр Цезаря"];
            KeyArea.Text = algoritm.DefaultKey;
            CreateButtons();
        }
        /*
            Функция создания кнопок групп алгоритмов.
            Для каждой группы создаем свою кнопку на панели выбора алгоритмов.
        */
        private void CreateButtons()
        {
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
            diffhell.Click += ButtonAlgoritmName_Click;
            diffhell.Text = "Диффи-Хеллман";
            MenuFlyout odnZamena = new MenuFlyout();
            odnZamena.Placement = FlyoutPlacementMode.Bottom;
            odnZamena.ShowMode = FlyoutShowMode.TransientWithDismissOnPointerMoveAway;
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
            gost94.Click += ButtonAlgoritmName_Click;
            gost94.Text = "ГОСТ Р 34.10-94";
            MenuFlyoutItem gost12 = new MenuFlyoutItem();
            gost12.Click += ButtonAlgoritmName_Click;
            gost12.Text = "ГОСТ Р 34.10-2012";
            MenuFlyout odnZamena = new MenuFlyout();
            odnZamena.Placement = FlyoutPlacementMode.Bottom;
            odnZamena.ShowMode = FlyoutShowMode.TransientWithDismissOnPointerMoveAway;
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
            rsa.Click += ButtonAlgoritmName_Click;
            rsa.Text = "ЦП RSA";
            MenuFlyoutItem elgamal = new MenuFlyoutItem();
            elgamal.Click += ButtonAlgoritmName_Click;
            elgamal.Text = "ЦП Elgamal";
            MenuFlyout odnZamena = new MenuFlyout();
            odnZamena.Placement = FlyoutPlacementMode.Bottom;
            odnZamena.ShowMode = FlyoutShowMode.TransientWithDismissOnPointerMoveAway;
            odnZamena.Items.Add(rsa);
            odnZamena.Items.Add(elgamal);
            Button buttonZamen = new Button();
            buttonZamen.Name = "Цифровые подписи";
            buttonZamen.Content = "Цифровые подписи";
            buttonZamen.Flyout = odnZamena;
            AllButtons.Add(buttonZamen);
        }

        private void AsymmetricCipher()
        {
            MenuFlyoutItem rsa = new MenuFlyoutItem();
            rsa.Click += ButtonAlgoritmName_Click;
            rsa.Text = "Шифр RSA";
            MenuFlyoutItem elgamal = new MenuFlyoutItem();
            elgamal.Click += ButtonAlgoritmName_Click;
            elgamal.Text = "Шифр Elgamal";
            MenuFlyoutItem ecc = new MenuFlyoutItem();
            ecc.Click += ButtonAlgoritmName_Click;
            ecc.Text = "Шифр ЕСС";
            MenuFlyout odnZamena = new MenuFlyout();
            odnZamena.Placement = FlyoutPlacementMode.Bottom;
            odnZamena.ShowMode = FlyoutShowMode.TransientWithDismissOnPointerMoveAway;
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
            magma.Click += ButtonAlgoritmName_Click;
            magma.Text = "Шифр Магма";
            MenuFlyoutItem aes = new MenuFlyoutItem();
            aes.Click += ButtonAlgoritmName_Click;
            aes.Text = "Шифр AES";
            MenuFlyoutItem kuznechik = new MenuFlyoutItem();
            kuznechik.Click += ButtonAlgoritmName_Click;
            kuznechik.Text = "Шифр Кузнечик";
            MenuFlyout odnZamena = new MenuFlyout();
            odnZamena.Placement = FlyoutPlacementMode.Bottom;
            odnZamena.ShowMode = FlyoutShowMode.TransientWithDismissOnPointerMoveAway;
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
            a51.Click += ButtonAlgoritmName_Click;
            a51.Text = "Шифр A5/1";
            MenuFlyoutItem a52 = new MenuFlyoutItem();
            a52.Click += ButtonAlgoritmName_Click;
            a52.Text = "Шифр A5/2";
            MenuFlyout odnZamena = new MenuFlyout();
            odnZamena.Placement = FlyoutPlacementMode.Bottom;
            odnZamena.ShowMode = FlyoutShowMode.TransientWithDismissOnPointerMoveAway;
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
            bloknot.Click += ButtonAlgoritmName_Click;
            bloknot.Text = "Шифр Блокнот Шеннона";
            MenuFlyout odnZamena = new MenuFlyout();
            odnZamena.Placement = FlyoutPlacementMode.Bottom;
            odnZamena.ShowMode = FlyoutShowMode.TransientWithDismissOnPointerMoveAway;
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
            vert.Click += ButtonAlgoritmName_Click;
            vert.Text = "Шифр вертикальной перестановки";
            MenuFlyoutItem kardano = new MenuFlyoutItem();
            kardano.Click += ButtonAlgoritmName_Click;
            kardano.Text = "Шифр Решетка Кардано";
            MenuFlyout odnZamena = new MenuFlyout();
            odnZamena.Placement = FlyoutPlacementMode.Bottom;
            odnZamena.ShowMode = FlyoutShowMode.TransientWithDismissOnPointerMoveAway;
            odnZamena.Items.Add(vert);
            odnZamena.Items.Add(kardano);
            Button buttonZamen = new Button();
            buttonZamen.Name = "Шифры перестановки";
            buttonZamen.Content = "Шифры перестановки";
            buttonZamen.Flyout = odnZamena;
            AllButtons.Add(buttonZamen);
        }

        private void BlockCipher()
        {
            MenuFlyoutItem matric = new MenuFlyoutItem();
            matric.Text = "Матричный шифр";
            matric.Click += ButtonAlgoritmName_Click;
            MenuFlyoutItem plaifer = new MenuFlyoutItem();
            plaifer.Text = "Шифр Плэйфера";
            plaifer.Click += ButtonAlgoritmName_Click;
            MenuFlyout odnZamena = new MenuFlyout();
            odnZamena.Placement = FlyoutPlacementMode.Bottom;
            odnZamena.ShowMode = FlyoutShowMode.TransientWithDismissOnPointerMoveAway;
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
            tritemi.Click += ButtonAlgoritmName_Click;
            tritemi.Text = "Шифр Тритемия";
            MenuFlyoutItem belazo = new MenuFlyoutItem();
            belazo.Click += ButtonAlgoritmName_Click;
            belazo.Text = "Шифр Белазо";
            MenuFlyoutItem visener = new MenuFlyoutItem();
            visener.Click += ButtonAlgoritmName_Click;
            visener.Text = "Шифр Виженера";
            MenuFlyout odnZamena = new MenuFlyout();
            odnZamena.Placement = FlyoutPlacementMode.Bottom;
            odnZamena.ShowMode = FlyoutShowMode.TransientWithDismissOnPointerMoveAway;
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
            atbash.Click += ButtonAlgoritmName_Click;
            atbash.Text = "Шифр Атбаш";
            MenuFlyoutItem cezar = new MenuFlyoutItem();
            cezar.Text = "Шифр Цезаря";
            cezar.Click += ButtonAlgoritmName_Click;
            MenuFlyoutItem polibii = new MenuFlyoutItem();
            polibii.Text = "Шифр Полибия";
            polibii.Click += ButtonAlgoritmName_Click;
            MenuFlyout odnZamena = new MenuFlyout();
            odnZamena.Placement = FlyoutPlacementMode.Bottom;
            odnZamena.ShowMode = FlyoutShowMode.TransientWithDismissOnPointerMoveAway;
            odnZamena.Items.Add(atbash);
            odnZamena.Items.Add(cezar);
            odnZamena.Items.Add(polibii);
            Button buttonZamen = new Button();
            buttonZamen.Name = "Шифры однозначной замены";
            buttonZamen.Content = "Шифры однозначной замены";
            buttonZamen.Flyout = odnZamena;
            AllButtons.Add(buttonZamen);
        }
        /*
            Событие нажатия на всплывающее меню с алгоритмом шифрования
            Получаем имя выбранного алгоритма,
            меняем текущий алгоритм на выбранный.
        */
        private void ButtonAlgoritmName_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem item = sender as MenuFlyoutItem;
            if (item == null) return;

            string name = item.Text;
            algoritm = dictAlgorithm[name];
            AlgName.Text = $"Текущий алгоритм шифрования: {name}";
            KeyArea.Text = algoritm.DefaultKey;
            Reset();
        }
        /*
            Функция добавления кнопок с алгоритмами на панель.
            Добавляем кнопки на панель, окрашиваем.
            
        */
        private void CreateAlgorithms()
        {
            PushAlgorithms();
            foreach(var item in AllButtons)
            {
                item.VerticalAlignment = VerticalAlignment.Top;
                item.Background = new SolidColorBrush(Color.FromArgb(255, 86, 11, 115));
                ButtonGrid.Children.Add(item);
            }
            Button but = new Button();
            but.Content = "Генерация ключа";
            but.VerticalAlignment = VerticalAlignment.Bottom;
            but.Background = new SolidColorBrush(Color.FromArgb(80, 242, 29, 166));
            but.Click += GenerateKey_Click;
            ButtonGrid.Children.Add(but);
        }
        /*
            Функция сброса полей.
            Сбрасывает поля Вид ключа и Вид внутри программы,
            Показывает или скрывает режимы работы магмы,
            окрашивает кнопку с группой текущего алгоритма шифрования.
        */
        private void Reset()
        {
            ErrorArea.Text = string.Empty;
            ViewText.Text = string.Empty;
            KeyArea.IsReadOnly = false;
            if(algoritm.Name.Equals("Шифр Магма"))
            {
                ModName.Visibility = Visibility.Visible;
                ModText.Visibility = Visibility.Visible;
            }
            else
            {
                ModName.Visibility = Visibility.Collapsed;
                ModText.Visibility = Visibility.Collapsed;
            }

            foreach (object obj in AllButtons)
            {
                Button button = obj as Button;
                if (button == null) continue;

                if (algoritm.Group.Equals(button.Name))
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
