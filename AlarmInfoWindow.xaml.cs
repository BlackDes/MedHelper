using System;
using System.Windows;

namespace MedHelper
{
    /// <summary>
    /// Логика взаимодействия для AlarmInfoWindow.xaml
    /// </summary>
    public partial class AlarmInfoWindow : Window
    {
		ConnectionDB conDB = new ConnectionDB();
		HelpClass hc = new HelpClass();

		public bool isChanged = false;

		public int p_id;
		public string p_name;
		public int p_hour, p_minute;
		public bool p_isActive;

        public AlarmInfoWindow(int mw_id, string mw_name, int mw_hour, int mw_minute, bool mw_isActive)
        {
            InitializeComponent();

			p_id = mw_id;
            p_name = mw_name;
			p_hour = mw_hour;
			p_minute = mw_minute;
			p_isActive = mw_isActive;

            tbxAlarmsName.Text = p_name;
            tbxHour.Text = p_hour.ToString();
            tbxMinute.Text = p_minute.ToString();

            if (p_isActive == true)
            {
                btnOnOff.Content = "Деактивировать";
				p_isActive = false;
			}
            else
            {
				btnOnOff.Content = "Активировать";
				p_isActive = true;
			}
        }

		private void BtnPlusHour_Click(object sender, RoutedEventArgs e)
		{
			hc.NumberHourMinuterHelp(tbxHour, 23, 0, 1);
		}

		private void BtnMinusHour_Click(object sender, RoutedEventArgs e)
		{
			hc.NumberHourMinuterHelp(tbxHour, 0, 23, -1);
		}

		private void BtnPlusMinute_Click(object sender, RoutedEventArgs e)
		{
			hc.NumberHourMinuterHelp(tbxMinute, 59, 0, 1);
		}

		private void BtnMinusMinute_Click(object sender, RoutedEventArgs e)
		{
			hc.NumberHourMinuterHelp(tbxMinute, 0, 59, -1);
		}

		private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
			MessageBoxResult result = MessageBox.Show("Вы действительно хотите удалить будильник?", "Предупреждение!", MessageBoxButton.YesNo, MessageBoxImage.Information);
			if (result == MessageBoxResult.Yes)
            {
				string sql = $@"
								delete from Alarms
								where AlarmID = {p_id}";

				conDB.SqliteModification(sql);
				MessageBox.Show($"Удалён будильник!\nНазвание: {p_name}\n" +
								$"Время: {p_hour}:{p_minute}", "Предупреждение!", MessageBoxButton.OK, MessageBoxImage.Information);
				this.Close();
			}
		}

        private void BtnOnOff_Click(object sender, RoutedEventArgs e)
        {
            if (p_isActive == true)
            {
                btnOnOff.Content = "Деактивировать";
                p_isActive = false;
                SwitchOnOff(1);
				return;
			}

            if (p_isActive == false)
			{
				btnOnOff.Content = "Активировать";
				p_isActive = true;
                SwitchOnOff(0);
				return;
			}
        }

		private void SwitchOnOff(int onoff)
        {
			string sql = $@"
							update Alarms 
							set [IsActive] = {onoff}
							where [AlarmID] = {p_id}";

			conDB.SqliteModification(sql);
		}

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
			this.Close();
        }

        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
			if (isChanged != true)
            {
				UpdateSwith("Сохранить", true, false, Visibility.Hidden, Visibility.Visible);
			}
            else
            {
				UpdateSwith("Изменить", false, true, Visibility.Visible, Visibility.Hidden);

                if (tbxAlarmsName.SelectionLength > 30)
                {
                    MessageBox.Show("Колличество символов в названии превышает допустимый лимит в 30 символов!", "Предупреждение!", MessageBoxButton.OK, MessageBoxImage.Error);
					tbxAlarmsName.Text = p_name;
					tbxHour.Text = p_hour.ToString();
					tbxMinute.Text = p_minute.ToString();
					return;
                }

                if (tbxAlarmsName.Text == string.Empty || tbxHour.Text == string.Empty || tbxMinute.Text == string.Empty)
                {
                    MessageBox.Show("Нельзя оставлять поля пустыми!", "Предупреждение!", MessageBoxButton.OK, MessageBoxImage.Error);
					tbxAlarmsName.Text = p_name;
					tbxHour.Text = p_hour.ToString();
					tbxMinute.Text = p_minute.ToString();
					return;
                }

				bool anothersymbolminute = int.TryParse(tbxMinute.Text, out var minute);
				bool anothersymbolhour = int.TryParse(tbxHour.Text, out var hour);
                if ((anothersymbolminute && anothersymbolhour) == false)
                {
                    MessageBox.Show("Символы, кроме чисел, нельзя вписывать в время!", "Предупреждение!", MessageBoxButton.OK, MessageBoxImage.Error);
					tbxAlarmsName.Text = p_name;
					tbxHour.Text = p_hour.ToString();
					tbxMinute.Text = p_minute.ToString();
					return;
                }

				bool checkmin = minute < -1 || minute > 60;
				bool checkhour = hour < -1 || hour > 24;
                if (checkmin || checkhour)
                {
                    MessageBox.Show($"Неверно введенно время!", "Предупреждение!", MessageBoxButton.OK, MessageBoxImage.Error);
                    tbxAlarmsName.Text = p_name;
                    tbxHour.Text = p_hour.ToString();
                    tbxMinute.Text = p_minute.ToString();
                    return;
                }
                else
                {
                    string sql = $@"
								update Alarms 
								set [Name] = '{tbxAlarmsName.Text}', [Hour] = {Convert.ToInt32(tbxHour.Text)}, [Minute] = {Convert.ToInt32(tbxMinute.Text)}, [IsActive] = 1 
								where [AlarmID] = {p_id}";

                    conDB.SqliteModification(sql);

					MessageBox.Show($"Будильник успешно изменён\nНазвание: {tbxAlarmsName.Text}\nВремя: {tbxHour.Text}:{tbxMinute.Text}", "Успех!", MessageBoxButton.OK, MessageBoxImage.Information);

					this.Close();
				}
            }
        }

		private void UpdateSwith(string text, bool activeT, bool activeF, Visibility visT, Visibility visF)
        {
			btnUpdate.Content = text;

			tbxAlarmsName.IsEnabled = activeT;
			tbxHour.IsEnabled = activeT;
			tbxMinute.IsEnabled = activeT;

			btnBack.IsEnabled = activeF;
			btnDelete.IsEnabled = activeF;
			btnOnOff.IsEnabled = activeF;

			btnPlusHour.IsEnabled = activeT;
			btnPlusMinute.IsEnabled = activeT;
			btnMinusHour.IsEnabled = activeT;
			btnMinusMinute.IsEnabled = activeT;

			btnDelete.Visibility = visT;
			btnCancel.Visibility = visF;

			isChanged = activeT;
		}

		private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
			UpdateSwith("Изменить", false, true, Visibility.Visible, Visibility.Hidden);
		}
	}
}
