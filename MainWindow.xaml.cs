using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Globalization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Configuration;
using Newtonsoft.Json;

namespace WPF_Azure_CosmosDB
{
    public class UserData
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "partitionKey")]
        public string PartitionKey { get; set; } = "Data";
        private string? textValue;
        private int? intValue;
        private double? doubleValue;
        private Boolean? boolValue;
        private DateTime? dateValue;
        private int VersionValue;
        public int Version
        {
            get { return VersionValue; }
            set { VersionValue = value; OnPropertyChanged("VersionValue"); }
        }
        public string? TextValue
        {
            get { return textValue; }
            set { textValue = value; OnPropertyChanged("TextValue"); }
        }
        public int? IntValue
        {
            get { return intValue; }
            set { intValue = value; OnPropertyChanged("IntValue"); }
        }
        public double? DoubleValue
        {
            get { return doubleValue; }
            set { doubleValue = value; OnPropertyChanged("DoubleValue"); }
        }
        public Boolean? BoolValue
        {
            get { return boolValue; }
            set { boolValue = value; OnPropertyChanged("BoolValue"); }
        }
        public DateTime? DateValue
        {
            get { return dateValue; }
            set { dateValue = value; OnPropertyChanged("DateValue"); }
        }        
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {        
        readonly bool is_initialize = true;
        bool is_filter = false;
        public string DataGrig_Id;        

        public MainWindow()
        {
            InitializeComponent();

            value2.IsEnabled = false;
            is_initialize = false;

            SelectDBAndUpdateDatagrid();
        }

        // Формирование запроса
        private string CreateQueryText()
        {            
            string sqlQueryText = "";
            if (is_filter == false)
            {
                sqlQueryText = "select * from c where c.partitionKey = '" + new UserData().PartitionKey + "'";
            }
            else
            {
                String m_value1 = value1.Text.ToString();
                String m_value2 = value2.Text.ToString();

                if (value_type.Text == "id")
                {
                    sqlQueryText = "select * from c where c.partitionKey = '" + new UserData().PartitionKey + "' and c.id = '" + m_value1 + "'";
                }
                else if (value_type.Text == "text")
                {
                    sqlQueryText = "select * from c where c.partitionKey = '" + new UserData().PartitionKey + "' and c.TextValue like '%" + m_value1 + "%'";
                }
                else if (value_type.Text == "int")
                {
                    _ = int.TryParse(m_value1, out int m_value1_int);
                    _ = int.TryParse(m_value2, out int m_value2_int);
                    sqlQueryText = "select * from c where c.partitionKey = '" + new UserData().PartitionKey + "' and c.IntValue >= " + m_value1.Replace(",",".") + " and c.IntValue <=" + m_value2.Replace(",", ".");
                }
                else if (value_type.Text == "double")
                {
                    _ = double.TryParse(m_value1, out double m_value1_dbl);
                    _ = double.TryParse(m_value2, out double m_value2_dbl);
                    sqlQueryText = "select * from c where c.partitionKey = '" + new UserData().PartitionKey + "' and c.DoubleValue >= " + m_value1.Replace(",", ".") + " and c.DoubleValue <=" + m_value2.Replace(",", ".");
                }
                else if (value_type.Text == "bool")
                {
                    _ = Boolean.TryParse(m_value1, out Boolean m_value1_bool);
                    sqlQueryText = "select * from c where c.partitionKey = '" + new UserData().PartitionKey + "' and c.BoolValue = " + m_value1;
                }
                else if (value_type.Text == "date")
                {
                    _ = DateTime.TryParseExact(m_value1, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime m_value1_dat);
                    _ = DateTime.TryParseExact(m_value2, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime m_value2_dat);
                    m_value2_dat = m_value2_dat.AddDays(1);
                    sqlQueryText = "select * from c where c.partitionKey = '" + new UserData().PartitionKey + "' and c.DateValue >= '" + m_value1_dat.ToString("yyyy-MM-dd") + "' and c.DateValue <= '" + m_value2_dat.ToString("yyyy-MM-dd") + "'";
                }
            }
            return sqlQueryText;
        }

        // Обновить грид
        private void UpdateDatagrid(List<UserData> UserDataList)
        {
            if (is_initialize == true) return;
                                   
            DataGrid1.ItemsSource = UserDataList;
            this.DataContext = DataGrid1.ItemsSource;            

            // Выделить сроку с курсором
            Boolean m_is_focus = false;
            if (DataGrid1.Items.Count > 0)
            {
                foreach (UserData drv in DataGrid1.ItemsSource)
                {
                    if (drv.Id == DataGrig_Id)
                    {
                        DataGrid1.SelectedItem = drv;
                        DataGrid1.ScrollIntoView(drv);
                        DataGrid1.Focus();
                        m_is_focus = true;
                        break;
                    }
                }
                
                if (!m_is_focus) 
                {
                    DataGrid1.SelectedItem = 1;
                    DataGrid1.ScrollIntoView(1);
                    DataGrid1.Focus();
                }
            }
        }

        private async void SelectDBAndUpdateDatagrid()
        {
            var cur = Mouse.OverrideCursor;
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;

            Azure_CosmosDB p = new();
            await p.SelectItemsAsync(CreateQueryText());
            UpdateDatagrid(p.UserDataList);

            Mouse.OverrideCursor = cur;
        }

        private async void InsertDBAndUpdateDatagrid(UserData ud)
        {
            var cur = Mouse.OverrideCursor;
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            
            Azure_CosmosDB p = new();
            await p.InsertItemsAsync(CreateQueryText(), ud);
            UpdateDatagrid(p.UserDataList);

            Mouse.OverrideCursor = cur;
        }

        private async void ReplaceDBAndUpdateDatagrid(UserData ud)
        {
            var cur = Mouse.OverrideCursor;
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;

            Azure_CosmosDB p = new();
            await p.ReplaceItemsAsync(CreateQueryText(), ud);

            // защита от неконтролируемого обновления
            if (p.IsControl)
            {
                MessageBox("Данные в базе данных изменились обновите данные в гриде", System.Windows.MessageBoxImage.Warning);
            }

            UpdateDatagrid(p.UserDataList);

            Mouse.OverrideCursor = cur;            
        }

        private async void DeleteDBAndUpdateDatagrid(UserData ud)
        {
            var cur = Mouse.OverrideCursor;
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;

            Azure_CosmosDB p = new();
            await p.DeleteItemsAsync(CreateQueryText(), ud);
            UpdateDatagrid(p.UserDataList);

            Mouse.OverrideCursor = cur;
        }

        // добавить запись
        private void Button_insertClick(object sender, RoutedEventArgs e)
        {
            AddWindow addWin = new(new UserData());
            if (addWin.ShowDialog() == true)
            {                
                UserData ud = addWin.UserDataAdd;                
                ud.Id = Guid.NewGuid().ToString();
                ud.Version = 0;
                InsertDBAndUpdateDatagrid(ud);
            }
        }

        // изменить запись
        private void Button_updateClick(object sender, RoutedEventArgs e)
        {
            // если ни одного объекта не выделено, выходим
            if (DataGrid1.SelectedItem == null) return;
            // получаем выделенный объект
            UserData? ud = DataGrid1.SelectedItem as UserData;

            AddWindow addWin = new(new UserData());
            if (ud != null) 
            { 
                addWin = new(new UserData
                {
                    Id = ud.Id,
                    TextValue = ud.TextValue,
                    IntValue = ud.IntValue,
                    DoubleValue = ud.DoubleValue,
                    BoolValue = ud.BoolValue,
                    DateValue = ud.DateValue,
                    Version = ud.Version
                });
            }

            if (addWin.ShowDialog() == true)
            {
                // получаем измененный объект                                
                if (ud != null)
                {
                    ud.TextValue = addWin.UserDataAdd.TextValue;
                    ud.IntValue = addWin.UserDataAdd.IntValue;
                    ud.DoubleValue = addWin.UserDataAdd.DoubleValue;
                    ud.BoolValue = addWin.UserDataAdd.BoolValue;
                    ud.DateValue = addWin.UserDataAdd.DateValue;
                    ud.Version = addWin.UserDataAdd.Version;

                    try
                    {
                      ReplaceDBAndUpdateDatagrid(ud);                    
                      MessageBox("Запись обновлена");
                    }
                    catch (Exception ex)
                    {
                        MessageBox(ex.Message, System.Windows.MessageBoxImage.Warning);
                        SelectDBAndUpdateDatagrid();
                    }
                }
            }
        }

        // удалить запись
        private void Button_deleteClick(object sender, RoutedEventArgs e)
        {
            // если ни одного объекта не выделено, выходим
            if (DataGrid1.SelectedItem == null) return;

            MessageBoxResult result = System.Windows.MessageBox.Show("Удалить запись ???", "Сообщение", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    // получаем выделенный объект
                    if (DataGrid1.SelectedItem is UserData ud)
                    {                        
                        DeleteDBAndUpdateDatagrid(ud);
                    }

                    MessageBox("Запись удалена");
                    break;
                case MessageBoxResult.No:
                    break;
            }
        }

        // обновить запись
        private void Button_selectClick(object sender, RoutedEventArgs e)
        {
            SelectDBAndUpdateDatagrid();
        }

        private readonly SolidColorBrush hb = new(Colors.MistyRose);
        private readonly SolidColorBrush nb = new(Colors.AliceBlue);
        private void DataGrid1_LoadingRow(object sender, DataGridRowEventArgs e)
        {            
            if ((e.Row.GetIndex() + 1) % 2 == 0)
                e.Row.Background = hb;
            else
                e.Row.Background = nb;

            // А можно в WPF установить - RowBackground - для нечетных строк и AlternatingRowBackground
        }

        // вывод диалогового окна
        public static void MessageBox(String infoMessage, MessageBoxImage mImage = System.Windows.MessageBoxImage.Information)
        {
            System.Windows.MessageBox.Show(infoMessage, "Сообщение", System.Windows.MessageBoxButton.OK, mImage);
        }
        
        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var row_list = (UserData)DataGrid1.SelectedItem;
                if (row_list != null)
                    DataGrig_Id = row_list.Id;
            }
            catch
            {
                DataGrig_Id = "";
            }
        }

        private void DataGrid_MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            Button_updateClick(sender, e);
        }

        // применить фильтр
        private void Button_findClick(object sender, RoutedEventArgs e)
        {
            is_filter = true;
            SelectDBAndUpdateDatagrid();
        }

        // отменить фильтр
        private void Button_find_cancelClick(object sender, RoutedEventArgs e)
        {
            is_filter = false;
            value1.Text = "";
            value2.Text = "";
            SelectDBAndUpdateDatagrid();
        }

        // изменение типа данных
        private void Value_type_SelectedIndexChanged(object sender, SelectionChangedEventArgs e)
        {
            if (is_initialize == true) return;

            ComboBox comboBox = (ComboBox)sender;
            ComboBoxItem selectedItem = (ComboBoxItem)comboBox.SelectedItem;
            String? value_type = selectedItem.Content.ToString();

            if (value_type == "id") { value2.IsEnabled = false; value2.Text = ""; }
            else if (value_type == "text") { value2.IsEnabled = false; value2.Text = ""; }
            else if (value_type == "int") value2.IsEnabled = true;
            else if (value_type == "double") value2.IsEnabled = true;
            else if (value_type == "bool") { value2.IsEnabled = false; value2.Text = ""; }
            else if (value_type == "date") value2.IsEnabled = true;
        }

        // изменение фокуса на value2
        private void Value2_GotKeyboardFocus(object sender, EventArgs e)
        {
            if (value1.Text != "") value2.Text = value1.Text;
        }
    }
}

