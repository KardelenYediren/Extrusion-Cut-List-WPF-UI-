using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ExtrusionCutList
{
    // DPI ayarları için Decorator sınıfı
    public class DpiDecorator : Decorator
    {
        public DpiDecorator()
        {
            this.Loaded += (s, e) =>
            {
                // Mevcut ekranın DPI ayarlarını almak için kullanılan kod
                System.Windows.Media.Matrix m = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice;
                ScaleTransform dpiTransform = new ScaleTransform(1 / m.M11, 1 / m.M22);
                if (dpiTransform.CanFreeze)
                    dpiTransform.Freeze();
                this.LayoutTransform = dpiTransform;
            };
        }
    }

    public partial class MainWindow : Window
    {
        // Hücre font boyutunu ayarlamak için DependencyProperty
        public static readonly DependencyProperty CellFontSizeProperty =
            DependencyProperty.Register("CellFontSize", typeof(double), typeof(MainWindow), new PropertyMetadata(12.0));

        public double CellFontSize
        {
            get { return (double)GetValue(CellFontSizeProperty); }
            set { SetValue(CellFontSizeProperty, value); }
        }

        // Genel font boyutunu ayarlamak için DependencyProperty
        public double FontSizeValue
        {
            get { return (double)GetValue(FontSizeValueProperty); }
            set { SetValue(FontSizeValueProperty, value); }
        }

        public static readonly DependencyProperty FontSizeValueProperty =
            DependencyProperty.Register("FontSizeValue", typeof(double), typeof(MainWindow), new PropertyMetadata(12.0));

        // Veri kaynağı için ObservableCollection
        public ObservableCollection<ExtrusionItem> Items { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            // Örnek veri ile ObservableCollection'ı dolduruyoruz
            Items = new ObservableCollection<ExtrusionItem>
            {
                new ExtrusionItem { Thumbnail = "pack://application:,,,/Images/image1.png", ExtrusionType = "EG-05", Description = "Groove Eeeeeeeeeeee", ItemQty = 1, ItemLength = 2721.9, Angle1 = 0, CutDetail1 = "", CutDetail2 = "", Angle2 = 0, NotchPoint = "855.722, 1360.972, ..." },
                new ExtrusionItem { Thumbnail = "pack://application:,,,/Images/image2.png", ExtrusionType = "EG-05", Description = "Groove Eeeeeeeeeeee", ItemQty = 2, ItemLength = 699.13, Angle1 = -45, CutDetail1 = "", CutDetail2 = "", Angle2 = -45, NotchPoint = "-" },
                new ExtrusionItem { Thumbnail = "pack://application:,,,/Images/image3.png", ExtrusionType = "EG-05", Description = "Groove E...", ItemQty = 1, ItemLength = 539.81, Angle1 = -45, CutDetail1 = "", CutDetail2 = "", Angle2 = 0, NotchPoint = "477.1911" },
                new ExtrusionItem { Thumbnail = "pack://application:,,,/Images/image4.png", ExtrusionType = "EG-05", Description = "Groove E...", ItemQty = 2, ItemLength = 222, Angle1 = 0, CutDetail1 = "", CutDetail2 = "", Angle2 = 0, NotchPoint = "-" }
            };

            // DataGrid'in veri kaynağını ayarlama
            ExtrusionDataGrid.ItemsSource = Items;

            // Pencere boyutu değiştiğinde olay işleyici ekleme
            this.SizeChanged += MainWindow_SizeChanged;
        }

        // Extrusion öğesini temsil eden sınıf
        public class ExtrusionItem
        {
            public string Thumbnail { get; set; }
            public string ExtrusionType { get; set; }
            public string Description { get; set; }
            public int ItemQty { get; set; }
            public double ItemLength { get; set; }
            public int Angle1 { get; set; }
            public string CutDetail1 { get; set; }
            public string CutDetail2 { get; set; }
            public int Angle2 { get; set; }
            public string NotchPoint { get; set; }
        }

        // Pencere boyutu değiştiğinde çağrılan metod
        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double windowWidth = e.NewSize.Width;
            double windowHeight = e.NewSize.Height;

            // Pencere genişliğine bağlı olarak font boyutunu ayarlama
            CellFontSize = Math.Max(26, windowWidth / 50);
            FontSizeValue = Math.Max(16, windowWidth / 170);
            titleLabel.FontSize = Math.Max(16, windowWidth / 170);

            // Pencere yüksekliğine bağlı olarak headerRow'un yüksekliğini ayarlama
            headerRow.Height = new GridLength(Math.Max(35, windowHeight / 25)); // Minimum yüksekliği 35 birim olarak sabitledik

            // DataGrid sütun genişliklerini orantılı olarak ayarlama
            foreach (var column in ExtrusionDataGrid.Columns)
            {
                if (column is DataGridTextColumn textColumn)
                {
                    textColumn.Width = new DataGridLength(1, DataGridLengthUnitType.Star);

                    // TextBlock stilini ayarlama
                    textColumn.ElementStyle = new Style(typeof(TextBlock))
                    {
                        Setters = { new Setter(TextBlock.FontSizeProperty, FontSizeValue) }
                    };
                }
            }

            // Başlık font boyutunu dinamik olarak ayarlama
            Style headerStyle = new Style(typeof(DataGridColumnHeader))
            {
                Setters = { new Setter(DataGridColumnHeader.FontSizeProperty, FontSizeValue) }
            };
            ExtrusionDataGrid.ColumnHeaderStyle = headerStyle;

            // BUtonların genişlik ve yüksekliklerini ayarlama
            ExportCSVButton.Width = Math.Max(100, windowWidth / 10);
            ExportCSVButton.Height = Math.Max(30, windowHeight / 20);
            ExportPDFButton.Width = Math.Max(100, windowWidth / 10);
            ExportPDFButton.Height = Math.Max(30, windowHeight / 20);
        }

        // Pencereyi simge durumuna küçültmek için olay işleyici
        private void ButtonMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        // Pencere durumunu değiştirmek için olay işleyici
        private void WindowStateButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
        }

        // Pencereyi kapatmak için olay işleyici
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // Pencereyi sürüklemek için olay işleyici
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        // CSV'ye veri dışa aktarma işlevi
        private void ExportCSV_Click(object sender, RoutedEventArgs e)
        {
            // CSV dışa aktarma işlevini burada uygulayın
            MessageBox.Show("Export CSV button clicked");
        }

        // PDF'ye veri dışa aktarma işlevi
        private void ExportPDF_Click(object sender, RoutedEventArgs e)
        {
            // PDF dışa aktarma işlevini burada uygulayın
            MessageBox.Show("Export PDF button clicked");
        }

        // DataGrid seçim değişikliği işleyicisi
        private void ExtrusionDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Seçim değişikliklerini burada ele alın
        }

        // İkinci DataGrid seçim değişikliği işleyicisi
        private void ExtrusionDataGrid_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            // Seçim değişikliklerini burada ele alın
        }
    }
}

