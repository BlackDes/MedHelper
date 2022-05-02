using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedHelper
{
	//public static class Globals
	//{
	//	public static bool autoStartCheck;
	//}

	class HelpClass
	{
		public void NumberHourMinuterHelp(System.Windows.Controls.TextBox tbx, int numberOne, int numberTwo, int numberCalculate)
		{
			if (false == int.TryParse(tbx.Text, out var numberTest))
			{
				tbx.Text = "0";
			}
			else
			{
				int numberHelp = Convert.ToInt32(tbx.Text);
				if (numberHelp == numberOne)
				{
					numberHelp = numberTwo;
					tbx.Text = numberHelp.ToString();
				}
				else if ((numberHelp > numberTwo && numberCalculate == -1) || (numberHelp < numberOne && numberCalculate == -1))
				{
					numberHelp = numberOne;
					tbx.Text = numberHelp.ToString();
				}
				else if ((numberHelp < numberTwo && numberCalculate == 1) || (numberHelp > numberOne && numberCalculate == 1))
				{
					numberHelp = numberOne;
					tbx.Text = numberHelp.ToString();
				}
				else
				{
					numberHelp += numberCalculate;
					tbx.Text = numberHelp.ToString();
				}
			}
		}

		//public void HelpVoid4StartButton(System.Windows.Controls.Button btn, string textMode, bool checkHelp, int AutoStart)
		//{
		//	btn.Content = $"Автоматически запускать вместе с Windows | {textMode} Включено Отключено";
		//	Globals.autoStartCheck = checkHelp;

		//	string sql = $@"
		//					update Settings 
		//					set [AutoStart] = {AutoStart}
		//					where [SettingID] = 1";

		//	conDB.SqliteModification(sql);
		//}
	}
}
