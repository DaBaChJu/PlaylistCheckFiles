using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Plcheck
{
	/// <summary>
	/// Logique d'interaction pour Window1.xaml
	/// </summary>
	public partial class Window1 : Window
	{

		const string default_Title = "Playlist Check";

		HashSet<string> paths = new HashSet<string>();
		HashSet<string> files_in_playlist = new HashSet<string>();
		HashSet<string> files_in_folders = new HashSet<string>();
		HashSet<string> extensions = new HashSet<string>();
		string playlist_folder = null;

		public Window1()
		{
			InitializeComponent();
			if (App.command_line_args != null && App.command_line_args[0].EndsWith(".m3u"))
			//	new System.Threading.Thread(Load_Playlist).Start(App.command_line_args[0]);
				Load_Playlist(App.command_line_args[0]);
		}

		private void Window_DragOver(object sender, DragEventArgs e)
		{
			if(e.Data.GetDataPresent(DataFormats.FileDrop))
				e.Effects = DragDropEffects.Copy;
			e.Handled = true;
		}

		private void Window_Drop(object sender, DragEventArgs e)
		{

			e.Handled = true;

			string[] files = e.Data.GetData(DataFormats.FileDrop) as string[];

			if (files != null && files.Length == 1 && files[0].EndsWith(".m3u"))
				Load_Playlist(files[0]);

		}

		private void MenuItem_Click(object sender, RoutedEventArgs e)
		{
			Clipboard.SetText( (((sender as MenuItem).Parent as ContextMenu).PlacementTarget as ListBoxItem).Content.ToString() );
		}

		public void Load_Playlist(object data)
		{
			Load_Playlist(data as string);
		}

		public void Load_Playlist(string playlist_file)
		{

			progress.Value = 1;

			StreamReader reader;
			string line;
			IEnumerable<string> intersection;
			IEnumerable<string> union;
			IEnumerable<string> files_in_different_folder;

			liste.Items.Clear();
			paths.Clear();
			files_in_playlist.Clear();
			files_in_folders.Clear();
			extensions.Clear();

			playlist_folder = Path.GetDirectoryName(playlist_file);

			reader = new StreamReader(playlist_file,Encoding.Default);
			try
			{

				while ((line = reader.ReadLine()) != null)
				{

					if (Path.IsPathRooted(line))
					{
						files_in_playlist.Add(line);
						paths.Add(Path.GetDirectoryName(line));
					}
					else
					{
						files_in_playlist.Add(Path.Combine(playlist_folder, line));
						paths.Add(Path.Combine(playlist_folder, Path.GetDirectoryName(line)));
					}

					extensions.Add(Path.GetExtension(line));

				}

			}
			catch (Exception exc)
			{
				MessageBox.Show("Exception while reading playlist:" + Environment.NewLine + exc);
			}
			finally
			{
				reader.Close();
			}

			progress.Value = 33;

			foreach (string path in paths)
			{
				if(Directory.Exists(path))
					foreach (string file in Directory.GetFiles(path))
					{
						if (extensions.Contains(Path.GetExtension(file)))
							files_in_folders.Add(file);
					}
			}

			progress.Value = 50;

			intersection = files_in_folders.Intersect(files_in_playlist);
			union = files_in_folders.Union(files_in_playlist);

			files_in_different_folder = union.GroupBy((string s) => { return Path.GetFileNameWithoutExtension(s); }).Where(g => g.Count() > 1).Select(x => x.Key);

			progress.Value = 75;

			foreach (string file in union)
			{

				ListBoxItem item = new ListBoxItem();

				item.ToolTip = file;
				item.Content = file;

				if (intersection.Contains(file))
				{
					item.Background = Brushes.LightGreen;
					item.ToolTip += " OK";
				}
				else
				if (files_in_different_folder.Contains(Path.GetFileNameWithoutExtension(file)))
				{

#warning untested here

					item.Background = Brushes.Blue;

					if (files_in_playlist.Contains(file))
						item.ToolTip = files_in_folders.Where(path => path.Contains(Path.GetFileNameWithoutExtension(file))).First() + " File in different folder";
					else
						item.ToolTip = files_in_playlist.Where(path => path.Contains(Path.GetFileNameWithoutExtension(file))).First() + " File in different folder";
					//item.ToolTip += " File in different folder";

				}
				else
				if (files_in_playlist.Contains(file))
				{
					item.Background = Brushes.Black;
					item.Foreground = Brushes.White;
					item.ToolTip += " File in playlist only";
				}
				else
				if(!File.Exists(file))
				{
					item.Background = Brushes.Red;
					item.ToolTip += " File not on disk";
				}
				else
				{
					item.Background = Brushes.Black;
					item.Foreground = Brushes.White;
					item.ToolTip += " File not in playlist";
				}

				item.ContextMenu = this.Resources["ListItemContextMenu"] as ContextMenu;

				liste.Items.Add(item);

			}

			progress.Value = 100;

			this.Activate();

			if (intersection.Count() == union.Count())
			{
				if (this.Title == default_Title)
					this.Title = "Playlist Check (All green)";
			}
			else
			if (this.Title != default_Title)
				this.Title = default_Title;

		}

	}
}
