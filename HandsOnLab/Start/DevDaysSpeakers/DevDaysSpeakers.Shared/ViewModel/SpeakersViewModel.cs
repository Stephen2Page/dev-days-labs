using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using DevDaysSpeakers.Model;
using DevDaysSpeakers.Services;
using System.Net.Http;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace DevDaysSpeakers.ViewModel
{

	public class SpeakersViewModel : INotifyPropertyChanged
	{
		public ObservableCollection<Speaker> Speakers { get; set; }

		public Command GetSpeakersCommand { get; set; }

		public SpeakersViewModel()
		{
			Speakers = new ObservableCollection<Speaker>();
			GetSpeakersCommand = new Command(
				async () => await GetSpeakers(),
				() => !IsBusy);
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private bool busy;

		public bool IsBusy
		{
			get { return busy; }
			set
			{
				busy = value;
				OnPropertyChanged();
				//Update the can execute
				GetSpeakersCommand.ChangeCanExecute();
			}
		}

		private void OnPropertyChanged([CallerMemberName] string name = null) =>
	PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

private async Task GetSpeakers()
		{
			if (IsBusy)
				return;

			Exception error = null;
			try
			{
				IsBusy = true;

				//using Azure to get data
				var service = DependencyService.Get<AzureService>();
				var items = await service.GetSpeakers();

				Speakers.Clear();
				foreach (var item in items)
					Speakers.Add(item);
				
				//Using HTTP to get json data
				/*using (var client = new HttpClient())
				{
					//grab json from server
					var json = await client.GetStringAsync("http://demo4404797.mockable.io/speakers");

					//Deserialize json
					var items = JsonConvert.DeserializeObject<List<Speaker>>(json);

					//Load speakers into list
					Speakers.Clear();
					foreach (var item in items)
						Speakers.Add(item);
				}*/
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Error: " + ex);
				error = ex;
			}
			finally
			{
				IsBusy = false;
			}

			//If anything goes wrong the catch will save the exception and AFTER the finally block we can pop up an alert:

			if (error != null)
				await Application.Current.MainPage.DisplayAlert("Error!", error.Message, "OK");
		}

		public async Task UpdateSpeaker(Speaker speaker)
		{
			var service = DependencyService.Get<AzureService>();
			service.UpdateSpeaker(speaker);
			await GetSpeakers();
		}
	}

}
