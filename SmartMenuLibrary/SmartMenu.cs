using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SmartMenuLibrary
{
	public class SmartMenu
	{
		private string currentLanguage;
		private Dictionary<char, string> languages = new Dictionary<char, string>();
		private Dictionary<string, string> menuForLanguages = new Dictionary<string, string>();
		private Dictionary<string, Dictionary<char, string>> menuActionForLanguages = new Dictionary<string, Dictionary<char, string>>();
		public IBinding Binding;

		public void LoadMenu(string path, IBinding Binding)
		{
			this.Binding = Binding;
			string menuSpec = new StreamReader(new FileStream($"../../{path}", FileMode.Open)).ReadToEnd();
			LoadLanguages(menuSpec);
		}

		/// <summary>
		/// Rasponsible for identefying languages and loading them
		/// </summary>
		/// <param name="menuSpec"></param>
		private void LoadLanguages(string menuSpec)
		{
			List<int> indexOfLanguages = new List<int>();
			string[] menuSpecLines = menuSpec.Split('\n');

			//Finds the line numbers with language information
			for (int i = 0; i < menuSpecLines.Length; i++)
			{
				if (menuSpecLines[i].Contains('§'))
				{
					indexOfLanguages.Add(i);
					languages.Add(indexOfLanguages.Count.ToString()[0], menuSpecLines[i].Split('§')[0]);
				}
			}

			//Loads each language
			for (int i = 0; i < indexOfLanguages.Count; i++)
			{
				string languageMenuSpec = "";
				for (int j = indexOfLanguages[i] + 1; !indexOfLanguages.Contains(j) && j < menuSpecLines.Length; j++)
				{
					languageMenuSpec += $"{menuSpecLines[j]}\n";
				}

				LoadLanguage(languages[(i + 1).ToString()[0]], languageMenuSpec);
			}
		}

		/// <summary>
		/// Resposible for loading one language
		/// </summary>
		/// <param name="language"></param>
		/// <param name="languageMenuSpec"></param>
		private void LoadLanguage(string language, string languageMenuSpec)
		{
			LoadMenuForLanguage(language, languageMenuSpec);
			LoadMenuActionsForLanguage(language, languageMenuSpec);
		}

		/// <summary>
		/// Responsible for loading menu tekst for one language
		/// </summary>
		/// <param name="language"></param>
		/// <param name="menuSpec"></param>
		private void LoadMenuForLanguage(string language, string menuSpec)
		{
			string menuForLanguage = "";
			string[] menuSpecLines = menuSpec.Split('\n');

			//Finds all the information that needs to be shown to the user
			for (int i = 0; i < menuSpecLines.Length; i++)
			{
				menuForLanguage += $"{menuSpecLines[i].Split(';')[0]}\n";
			}
			menuForLanguage = menuForLanguage.Trim();

			menuForLanguages.Add(language, menuForLanguage);
		}

		/// <summary>
		/// Resposible for loading the actions for one language
		/// </summary>
		/// <param name="language"></param>
		/// <param name="menuSpec"></param>
		private void LoadMenuActionsForLanguage(string language, string menuSpec)
		{
			Dictionary<char, string> menuActionForLanguage = new Dictionary<char, string>();
			string[] menuSpecLines = menuSpec.Split('\n');
			menuActionForLanguage.Add('0', "exit");

			//Finds all the lines that have a menu point on it
			for (int i = 0; i < menuSpecLines.Length; i++)
			{
				if (menuSpecLines[i].Contains(';'))
				{
					//Finds the key needed to activate a menu point and the menuID that corresponds to it
					menuActionForLanguage.Add(menuSpecLines[i].Split(' ')[1].ToLower()[0], menuSpecLines[i].Split(';')[1].Trim());
				}
			}

			menuActionForLanguages.Add(language, menuActionForLanguage);
		}

		public void Activate()
		{
			Console.CursorVisible = false;
			string menuID = "";

			while (menuID != "exit")
			{
				if (!languages.ContainsValue(currentLanguage))
				{
					DisplayLanguageMenu();
					currentLanguage = GetValueInDictionaryFromUser(languages);
				}
				else
				{
					Display(menuForLanguages[currentLanguage]);
					if ((menuID = GetValueInDictionaryFromUser(menuActionForLanguages[currentLanguage])) != null)
					{
						Binding.Call(menuID);
					}
				}
			}
		}

		/// <summary>
		/// Responsible for displaying the language menu
		/// </summary>
		private void DisplayLanguageMenu()
		{
			string menu = "";
			foreach (KeyValuePair<char, string> item in languages)
			{
				menu += ($"{item.Key}:{item.Value}\n");
			}
			Display(menu.Trim());
		}

		/// <summary>
		/// Get a possible action in a dictionary from the user
		/// </summary>
		/// <param name="dictionary"></param>
		/// <returns></returns>
		private string GetValueInDictionaryFromUser(Dictionary<char, string> dictionary)
		{
			string value;
			char userChoice = Console.ReadKey().KeyChar;
			//If the useres choice is not in the ditionary return null
			if (!dictionary.TryGetValue(userChoice, out value))
			{
				return null;
			}

			return value;
		}

		/// <summary>
		/// Resposible for displaying a string to the console
		/// </summary>
		/// <param name="str"></param>
		private void Display(string str)
		{
			Console.Clear();
			string[] lines = str.Split('\n');
			for (int i = 0; i < lines.Length; i++)
			{
				Console.WriteLine($"{ RepeatChar(' ', 50 - lines[i].Length / 2) }{lines[i]}");
			}
		}

		private string RepeatChar(char ch, int i)
		{
			return new string(ch, i);
		}
	}
}
