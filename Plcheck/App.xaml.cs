using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace Plcheck
{
	/// <summary>
	/// Logique d'interaction pour App.xaml
	/// </summary>
	public partial class App : Application
	{

		public static string[] command_line_args = null;

		private void Application_Startup(object sender, StartupEventArgs e)
		{
			if (e.Args.Length == 1)
				command_line_args = e.Args;
		}


	}
}
