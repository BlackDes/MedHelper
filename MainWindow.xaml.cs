using System;
using System.Collections.Generic;
using System.Data;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.Win32;

namespace MedHelper
{
	public partial class MainWindow : Window
	{
		#region Переменные
		//Timer
		DispatcherTimer timer = new DispatcherTimer();

		//Work with time
		int minuteNow, hourNow;
		int minuteOnApp, hourOnApp;

		//Work with SQLite
		ConnectionDB conDB = new ConnectionDB();
		string sql;
		DataTable dt = new DataTable();
		DataTable dtCheck = new DataTable();
		DataTable dtAlarm = new DataTable();
		DataTable dtSettings = new DataTable();

		//Taskbar
		WindowState prevState;

		//Settings

		bool autoStartCheck;
		int modeColor, modeDistraction;
		bool modeDistractionCheck;
		TimeSpan tsDistraction;

		//Distraction alarm
		TimeSpan tsResult, tsNowTime, tsBreakTime, tsOnApp;

		//Help class
		RndText rt = new RndText();
		HelpClass hc = new HelpClass();

		
		#endregion

		public MainWindow()
		{
			InitializeComponent();
			Settings();

			SettingsForAddAlarmTab();
			SettingsForListAlarmsTab();
			SettingsForAboutProgrammTab();
			SettingsForSettingsTab();

			timer.Tick += MainTimer;
			timer.Interval = TimeSpan.FromMilliseconds(100);
			timer.Start();
		}

		#region Таймер / Вывод уведомлений / Трей / Интернет соединение
		private void MainTimer(object sender, EventArgs e)
		{
			SettingsForSystemTimeTab();
			InformationAboutAlarm();
			DistractionAlarm();
			NetInfo();
		}

		private void InformationAboutAlarm()
		{
			hourNow = Convert.ToInt32(DateTime.Now.ToString("HH"));
			minuteNow = Convert.ToInt32(DateTime.Now.ToString("mm"));

			tsNowTime = new TimeSpan(hourNow, minuteNow, 0);

			sql = $@"
					select A.AlarmId, A.Name, A.Hour, A.Minute, A.IsActive
					from Alarms A
					where A.Hour = {hourNow} and A.Minute = {minuteNow}";

			conDB.SqliteReader(sql, dtAlarm);

			if (dtAlarm.Rows.Count == 1 && Convert.ToInt32(dtAlarm.Rows[0][4]) == 1)
			{
				new ToastContentBuilder()
				.AddText($"{dtAlarm.Rows[0][1]}")
				.AddText($"Время: {dtAlarm.Rows[0][2]}:{dtAlarm.Rows[0][3]}")
				//.AddText($"Если вы хотите ещё раз использовать этот будильник, то перейдите во вкладку «‎Будильники» и активируйте его!»‎")
				.Show();

				sql = $@"
							update Alarms
							set IsActive = 0
							where AlarmID = {dtAlarm.Rows[0][0]}";

				conDB.SqliteModification(sql);
			}
		}

		private void Window_StateChanged(object sender, EventArgs e)
		{
			if (prevState == WindowState.Minimized)
			{
				Hide();
			}
			else
			{
				prevState = WindowState;
			}
		}

		private void TaskbarIcon_TrayLeftMouseDown(object sender, RoutedEventArgs e)
		{
			Show();
			WindowState = WindowState.Normal;
		}

		private async Task NetInfo()
		{
			while (true)
			{
				IPStatus status = IPStatus.Unknown;
				try
				{
					status = new Ping().Send("ya.ru").Status;
				}
				catch { }

				if (status == IPStatus.Success)
				{
					lblNetInfo.Content = "подключен";
					lblNetInfo.Foreground = Brushes.Green;
				}
				else
				{
					lblNetInfo.Content = "нет подключения";
					lblNetInfo.Foreground = Brushes.Red;
				}
				await Task.Delay(5000);
			}
		}
		#endregion

		#region Настройки
		private void Settings()
		{
			Height += 15;
			Width += 15;
		}

		private void SettingsForAddAlarmTab()
		{
			//класс создан в будущего развития
		}

		private void SettingsForListAlarmsTab()
		{
			sql = $@"
					select AlarmID, Name, Hour, Minute, Hour || ':' || Minute as Time, IsActive 
					from Alarms";

			conDB.SqliteReader(sql, dt);

			dgTest.ItemsSource = dt.AsDataView();
		}

		private void SettingsForSystemTimeTab()
		{
			lblSistemTime.Content = DateTime.Now.ToString("HH:mm:ss");
			lblSistemDate.Content = DateTime.Now.ToString("dd MMMM yyyy");
		}

		private void SettingsForAboutProgrammTab()
		{
			rt.AdviсeText(lblAbout);
		}

		private void SettingsForSettingsTab()
		{
			minuteOnApp = Convert.ToInt32(DateTime.Now.ToString("mm"));
			hourOnApp = Convert.ToInt32(DateTime.Now.ToString("HH"));

			tsOnApp = new TimeSpan(hourOnApp, minuteOnApp, 0);
			tsBreakTime = new TimeSpan(0, 20, 0);

			sql = $@"
					select SettingID, AutoStart, ModeColor, ModeDistraction
					from Settings";

			conDB.SqliteReader(sql, dtSettings);

			autoStartCheck = Convert.ToBoolean(dtSettings.Rows[0][1]);
			modeColor = Convert.ToInt32(dtSettings.Rows[0][2]);
			modeDistraction = Convert.ToInt32(dtSettings.Rows[0][3]);

			if (autoStartCheck == true)
			{
				btnStartWithWindows.Content = "Автоматически запускать вместе с Windows | Включено";
				autoStartCheck = false;
			}
			else
			{
				btnStartWithWindows.Content = "Автоматически запускать вместе с Windows | Отключено";
				autoStartCheck = true;
			}

			switch (modeColor)
			{
				case 0:
					rbnLight.IsChecked = true;
					break;
				case 1:
					rbnNight.IsChecked = true;
					break;
				default:
					sql = $@"
					update Settings 
					set [ModeDistraction] = 0
					where [SettingID] = 1";

					conDB.SqliteModification(sql);

					MessageBox.Show("Пожалуйста, не изменяйте ничего в файле SQLite сами :D", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);

					rbnLight.IsChecked = true;
					break;
			}

			switch (modeDistraction)
			{
				case 0:
					rbnOff.IsChecked = true;
					modeDistractionCheck = false;
					break;
				case 1:
					tsDistraction = new TimeSpan(1, 0, 0);
					rbnHour.IsChecked = true;
					modeDistractionCheck = true;
					tsResult = tsOnApp + tsDistraction;
					break;
				case 2:
					tsDistraction = new TimeSpan(1, 45, 0);
					rbnHour45Minute.IsChecked = true;
					modeDistractionCheck = true;
					tsResult = tsOnApp + tsDistraction;
					break;
				case 3:
					tsDistraction = new TimeSpan(2, 0, 0);
					rbn2Hours.IsChecked = true;
					modeDistractionCheck = true;
					tsResult = tsOnApp + tsDistraction;
					break;
				default:
					sql = $@"
					update Settings 
					set [ModeDistraction] = 0
					where [SettingID] = 1";

					conDB.SqliteModification(sql);

					MessageBox.Show("Пожалуйста, не изменяйте ничего в файле SQLite сами :D", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);

					rbnOff.IsChecked = true;
					modeDistractionCheck = false;
					break;
			}
		}
		#endregion

		#region Кнопки
		#region Кнопки для добавления будильника
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

		private void BtnAddAlarm_Click(object sender, RoutedEventArgs e)
		{
			if (tbxAlarmsName.SelectionLength > 30)
			{
				MessageBox.Show("Колличество символов в названии превышает допустимый лимит в 30 символов!", "Предупреждение!", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			if (dt.Rows.Count > 10)
			{
				MessageBox.Show("Колличество будильников превышает лимит!", "Предупреждение!", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			if (tbxHour.Text == string.Empty || tbxMinute.Text == string.Empty || tbxAlarmsName.Text == string.Empty)
			{
				MessageBox.Show("Нельзя оставлять поля пустыми!", "Предупреждение!", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			bool anothersymbolminute = int.TryParse(tbxMinute.Text, out var minute);
			bool anothersymbolhour = int.TryParse(tbxHour.Text, out var hour);
			if ((anothersymbolminute && anothersymbolhour) == false)
			{
				MessageBox.Show("Символы, кроме чисел, нельзя вписывать в время!", "Предупреждение!", MessageBoxButton.OK, MessageBoxImage.Error);
				tbxHour.Text = "0";
				tbxMinute.Text = "0";
				return;
			}

			bool checkmin = minute < -1 || minute > 60;
			bool checkhour = hour < -1 || hour > 24;
			if (checkmin || checkhour)
			{
				MessageBox.Show($"Неверно введенно время!", "Предупреждение!", MessageBoxButton.OK, MessageBoxImage.Error);
				tbxHour.Text = "0";
				tbxMinute.Text = "0";
				return;
			}

			sql = $@"
						select A.Hour, A.Minute 
						from Alarms A 
						where A.Hour = {hour} and A.Minute = {minute}";
			conDB.SqliteReader(sql, dtCheck);

			if (dtCheck.Rows.Count != 0)
			{
				MessageBox.Show($"Будильник с таким временем уже существует!", "Предупреждение!", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}
			else
			{
				sql = $@"
							insert into Alarms (Name, Hour, Minute, IsActive)
							values ('{tbxAlarmsName.Text}', {Convert.ToInt32(tbxHour.Text)}, {Convert.ToInt32(tbxMinute.Text)}, 1)";

				conDB.SqliteModification(sql);
				dgTest.ItemsSource = dt.DefaultView;

				MessageBox.Show($"Добавлен будильник!\nНаименование: {tbxAlarmsName.Text}\nВремя: {tbxHour.Text}:{tbxMinute.Text}\nКолличестов будильников: {dt.Rows.Count + 1}", "Предупреждение!", MessageBoxButton.OK, MessageBoxImage.Information);

				tbxAlarmsName.Clear();
				tbxHour.Text = "0";
				tbxMinute.Text = "0";

				SettingsForListAlarmsTab();
			}
		}

		#endregion
		#region Кнопка для информации о будильнике
		private void BtnInfo_Click(object sender, RoutedEventArgs e)
		{
			int selectedindex = Convert.ToInt32(dgTest.SelectedIndex.ToString());
			if (selectedindex == -1)
			{
				MessageBox.Show($"Для отображения информации выберите заполненную строку!", "Предупреждение!", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}
			else
			{
				try
				{
					int id = Convert.ToInt32(dt.Rows[Convert.ToInt32(dgTest.SelectedIndex)][0]);
					string nameAlarmDt = dt.Rows[Convert.ToInt32(dgTest.SelectedIndex)][1].ToString();
					int hourAlarmDt = Convert.ToInt32(dt.Rows[Convert.ToInt32(dgTest.SelectedIndex)][2]);
					int minuteAlarmDt = Convert.ToInt32(dt.Rows[Convert.ToInt32(dgTest.SelectedIndex)][3]);
					bool isActiveAlarmDt = Convert.ToBoolean(dt.Rows[Convert.ToInt32(dgTest.SelectedIndex)][5]);

					AlarmInfoWindow aiw = new AlarmInfoWindow(id, nameAlarmDt, hourAlarmDt, minuteAlarmDt, isActiveAlarmDt);
					aiw.Closed += (sender2, e2) =>
					{
						SettingsForListAlarmsTab();
					};

					aiw.ShowDialog();
				}
				catch
				{
					MessageBox.Show($"Для отображения информации выберите заполненную строку!", "Предупреждение!", MessageBoxButton.OK, MessageBoxImage.Error);
					return;
				}
			}
		}
		#endregion
		#region Кнопка в настройках
		private void BtnStartWithWindows_Click(object sender, RoutedEventArgs e)
		{
			RegistryKey regKey = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run\\");

			try
			{
				if (autoStartCheck == false)
				{
					regKey.DeleteValue("MedHelperAlarm");

					HelpVoid4StartButton(btnStartWithWindows, "Отключено", true, 0);
				}
				else
				{
					regKey.SetValue("MedHelperAlarm", Assembly.GetExecutingAssembly().Location);

					HelpVoid4StartButton(btnStartWithWindows, "Включено", false, 1);
				}
			}
			catch
			{
				MessageBox.Show("Возможно программа не стоит на автозапуске.\nПовторите попытку.", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
				regKey.SetValue("MedHelperAlarm", Assembly.GetExecutingAssembly().Location);

				HelpVoid4StartButton(btnStartWithWindows, "Включено", false, 1);
			}
		}

		public void HelpVoid4StartButton(System.Windows.Controls.Button btn, string textMode, bool checkHelp, int AutoStart)
		{
			btn.Content = $"Автоматически запускать вместе с Windows | {textMode} Включено Отключено";
			autoStartCheck = checkHelp;

			string sql = $@"
							update Settings 
							set [AutoStart] = {AutoStart}
							where [SettingID] = 1";

			conDB.SqliteModification(sql);
		}
		#endregion
		#region Кнопка для советов и анекдотов
		private void BtnNextAdvice_Click(object sender, RoutedEventArgs e)
		{
			rt.AdviсeText(lblAbout);
		}

		private void BtnNextJoke_Click(object sender, RoutedEventArgs e)
		{
			rt.JokeText(lblAbout);
		}
		#endregion
		#endregion


		#region Изменение цвета приложения
		private void RbnLight_Checked(object sender, RoutedEventArgs e)
		{
			ChangeColor("light", 0);
		}

		private void RbnNight_Checked(object sender, RoutedEventArgs e)
		{
			ChangeColor("dark", 1);
		}

		private void ChangeColor(string mode, int idmod)
		{
			sql = $@"
					update Settings 
					set [ModeColor] = {idmod}
					where [SettingID] = 1";

			conDB.SqliteModification(sql);

			modeColor = idmod;

			var uri = new Uri($"{mode}.xaml", UriKind.Relative);
			ResourceDictionary resourceDict = Application.LoadComponent(uri) as ResourceDictionary;
			Application.Current.Resources.Clear();
			Application.Current.Resources.MergedDictionaries.Add(resourceDict);
		}


		#endregion

		#region Отвлекающий будильник / Изменение отвлекающего будильника
		private void DistractionAlarm()
		{
			if (modeDistractionCheck == false)
			{
				return;
			}

			if (tsResult == tsNowTime)
			{
				new ToastContentBuilder()
				.AddText($"Время отвлечься от компьютера.\nОтдохните от компьтера 20 минут.")
				.AddText($"Время: {tsResult:hh':'mm}")
				.Show();

				tsResult += tsBreakTime + tsDistraction;
			}
		}

        private void RbnOff_Checked(object sender, RoutedEventArgs e)
		{
			DistractionVoidHelp(new TimeSpan(0, 0, 0), false, 0);
		}

		private void RbnHour_Checked(object sender, RoutedEventArgs e)
		{
			DistractionVoidHelp(new TimeSpan(1, 0, 0), true, 1);
		}

		private void RbnHour30Minute_Checked(object sender, RoutedEventArgs e)
		{
			DistractionVoidHelp(new TimeSpan(1, 30, 0), true, 2);
		}

		private void Rbn2Hours_Checked(object sender, RoutedEventArgs e)
		{
			DistractionVoidHelp(new TimeSpan(2, 0, 0), true, 3);
		}

		private void DistractionVoidHelp(TimeSpan ts, bool checkHelp, int modeDistraction)
        {
			tsDistraction = ts;

			modeDistractionCheck = checkHelp;

			sql = $@"
					update Settings 
					set [ModeDistraction] = {modeDistraction}
					where [SettingID] = 1";

			conDB.SqliteModification(sql);
		}
		#endregion
		


		private void TestVoid()
		{
			MessageBox.Show("ВСЁ РАБОТАЕТ!", "УСПЕХ!", MessageBoxButton.OK, MessageBoxImage.Warning);

			//TimeSpan t = new TimeSpan(20, 35, 0);
			//TimeSpan i = new TimeSpan(1, 45, 0);

			//MessageBox.Show($"{t + i}");

			TimeSpan t1 = new TimeSpan(hourOnApp, minuteOnApp, 0);
			TimeSpan i1 = new TimeSpan(0, 20, 0);

			TimeSpan result = t1 + i1;

			MessageBox.Show($"{result:hh':'mm}");
		}
	}
}