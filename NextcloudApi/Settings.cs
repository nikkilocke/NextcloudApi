using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace NextcloudApi {
	public interface ISettings {
		/// <summary>
		/// The Uri of the server (e.g. https://localhost:8065/)
		/// </summary>
		Uri ServerUri { get; }

		/// <summary>
		/// Application Name is required by the api
		/// </summary>
		string ApplicationName { get; }

		/// <summary>
		/// Redirect uri for authorisation. Usually "http://localhost:port/". 
		/// BaseCampApi will listen on the port to pick up the redirect during the authorisation process.
		/// </summary>
		Uri RedirectUri { get; }

		/// <summary>
		/// Page to redirect the user to after the Oauth process has logged them in.
		/// Leave empty to return PageToSendAfterLogin.
		/// </summary>
		string RedirectAfterLogin { get; }

		/// <summary>
		/// Page to send to the user to after the Oauth process has logged them in.
		/// Leave empty to show a page that closes itself.
		/// </summary>
		string PageToSendAfterLogin { get; }

		/// <summary>
		/// The length of time in seconds it takes for a login/password session to expire
		/// </summary>
		int LoginExpiryTime { get; }

		/// <summary>
		/// As allocated when you registered your application
		/// </summary>
		string ClientId { get; }

		/// <summary>
		/// As allocated when you registered your application
		/// </summary>
		string ClientSecret { get; }

		/// <summary>
		/// Authorisation returns this token, which is then used to access the api without having to login
		/// every time.
		/// </summary>
		string AccessToken { get; set; }

		/// <summary>
		/// Authorisation returns this token, which is then used to refresh the access token without having to login
		/// every time.
		/// </summary>
		string RefreshToken { get; set; }

		/// <summary>
		/// When the AccessToken expires
		/// </summary>
		DateTime TokenExpires { get; set; }

		/// <summary>
		/// The currently logged in user id (returned in token)
		/// </summary>
		string User { get; set; }
		string Username { get; set; }
		string Password { get; set; }

		/// <summary>
		/// If the access token is due to expire before this time elapses, refresh it
		/// </summary>
		TimeSpan RefreshTokenIfDueToExpireBefore { get; }

		/// <summary>
		/// Set to greater than zero to log all requests going to Basecamp. 
		/// Larger numbers give more verbose logging.
		/// </summary>
		int LogRequest { get; }

		/// <summary>
		/// Set greater than zero to log all replies coming from Basecamp. 
		/// Larger numbers give more verbose logging.
		/// </summary>
		int LogResult { get; }

		/// <summary>
		/// After BaseCampApi has update tokens, save the infomation.
		/// </summary>
		void Save();

	}

	public class Settings : ISettings {
		/// <summary>
		/// The Uri of the server (e.g. https://localhost:8065/)
		/// </summary>
		public Uri ServerUri { get; set; }
		/// <summary>
		/// Application Name is required by the api
		/// </summary>
		public string ApplicationName { get; set; }
		/// <summary>
		/// Redirect uri for authorisation. Usually "http://localhost:port/". 
		/// BaseCampApi will listen on the port to pick up the redirect during the authorisation process.
		/// </summary>
		public Uri RedirectUri { get; set; }
		/// <summary>
		/// Page to redirect the user to after the Oauth process has logged them in.
		/// Leave empty to return PageToSendAfterLogin.
		/// </summary>
		public string RedirectAfterLogin { get; set; }
		/// <summary>
		/// Page to send to the user to after the Oauth process has logged them in.
		/// Leave empty to show a page that closes itself.
		/// </summary>
		public string PageToSendAfterLogin { get; set; }
		/// <summary>
		/// The length of time in seconds it takes for a login/password session to expire
		/// </summary>
		public int LoginExpiryTime { get; set; } = 30 * 60 * 60;	// default is 30 days
		/// <summary>
		/// As allocated when you registered your application
		/// </summary>
		public string ClientId { get; set; }
		/// <summary>
		/// As allocated when you registered your application
		/// </summary>
		public string ClientSecret { get; set; }
		/// <summary>
		/// Authorisation returns this token, which is then used to access the api without having to login
		/// every time.
		/// </summary>
		public string AccessToken { get; set; }
		/// <summary>
		/// Login with username and password returns this token, which is then used to access the api
		/// every time.
		/// </summary>
		public string CsrfToken { get; set; }
		/// <summary>
		/// Authorisation returns this token, which is then used to refresh the access token without having to login
		/// every time.
		/// </summary>
		public string RefreshToken { get; set; }
		/// <summary>
		/// When the AccessToken expires
		/// </summary>
		public DateTime TokenExpires { get; set; }
		/// <summary>
		/// The currently logged in user id (returned in token)
		/// </summary>
		public string User { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
		/// <summary>
		/// If the access token is due to expire before this time elapses, refresh it
		/// </summary>
		public TimeSpan RefreshTokenIfDueToExpireBefore { get; set; } = new TimeSpan(1, 0, 0, 0);
		/// <summary>
		/// Set to greater than zero to log all requests going to Basecamp. 
		/// Larger numbers give more verbose logging.
		/// </summary>
		public int LogRequest { get; set; }

		/// <summary>
		/// Set greater than zero to log all replies coming from Basecamp. 
		/// Larger numbers give more verbose logging.
		/// </summary>
		public int LogResult { get; set; }

		[JsonIgnore]
		public string Filename;

		/// <summary>
		/// Load a Settings object from LocalApplicationData/BaseCampApi/Settings.json
		/// </summary>
		public static Settings Load() {
			string dataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NextcloudApi");
			Directory.CreateDirectory(dataPath);
			string filename = Path.Combine(dataPath, "Settings.json");
			Settings settings = new Settings();
			settings.Load(filename);
			return settings;
		}

		/// <summary>
		/// Load a Settings object from the supplied json file
		/// </summary>
		public virtual void Load(string filename) {
			if (File.Exists(filename))
				using (StreamReader s = new StreamReader(filename))
					JsonConvert.PopulateObject(s.ReadToEnd(), this);
			this.Filename = filename;
		}

		/// <summary>
		/// Save updated settings back where they came from
		/// </summary>
		public virtual void Save() {
			Directory.CreateDirectory(Path.GetDirectoryName(Filename));
			using (StreamWriter w = new StreamWriter(Filename))
				w.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented));
		}

		/// <summary>
		/// Check the Settings for missing data.
		/// If you derive from this class you can override this method to add additional checks.
		/// </summary>
		/// <returns>List of error strings - empty if no missing data</returns>
		public virtual List<string> Validate() {
			List<string> errors = new List<string>();
			if (ServerUri == null) {
				errors.Add("ServerUri missing");
			}
			if (string.IsNullOrEmpty(ApplicationName)) {
				errors.Add("ApplicationName missing");
			}
			return errors;
		}
	}
}
