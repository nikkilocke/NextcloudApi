using Microsoft.VisualStudio.TestTools.UnitTesting;
using NextcloudApi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests {
	public class Settings : NextcloudApi.Settings {
		public bool LoginTests = false;
		public bool ModifyTests = true;
		public bool DestructiveTests = false;
		public string TestUser;
		public string TestGroup;
		public int TestGroupFolderID = 1;
		public override List<string> Validate() {
			List<string> errors = base.Validate();
			return errors;
		}
	}

	public class TestBase {
		static Settings _settings;
		static Api _api;

		public static Api Api {
			get {
				if (_api == null) {
					_api = new Api(Settings);
				}
				return _api;
			}
		}

		public static Settings Settings {
			get {
				if (_settings == null) {
					string dataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NextcloudApi");
					Directory.CreateDirectory(dataPath);
					string filename = Path.Combine(dataPath, "TestSettings.json");
					_settings = new Settings();
					_settings.Load(filename);
					List<string> errors = _settings.Validate();
					if (errors.Count > 0)
						throw new ApplicationException(string.Join("\r\n", errors));
				}
				return _settings;
			}
		}


		public static T RunTest<T>(Task<T> task) {
			T t = task.Result;
			Console.WriteLine(t);
			return t;
		}

		public static void RunTest(Task task) {
			task.Wait();
		}

		public static void ShowList<T>(Task<ApiList<T>> task) {
			ApiList<T> result = RunTest(task);
			foreach (T o in result.All(Api))
				Console.WriteLine(o);
		}

		public static void ShowList<T>(Task<List<T>> task) {
			List<T> result = RunTest(task);
			foreach (T o in result)
				Console.WriteLine(o);
		}

		public static void ShowList<T>(Task<PlainList<T>> task) {
			PlainList<T> result = RunTest(task);
			foreach (T o in result.List)
				Console.WriteLine(o);
		}

		const string alphabet = "ybndrfg8ejkmcpqxot1uwisza345h769";

		public string UniqueId() {
			Guid guid = Guid.NewGuid();
			byte[] bytes = guid.ToByteArray();
			StringBuilder output = new StringBuilder();
			for (int bitIndex = 0; bitIndex < bytes.Length * 8; bitIndex += 5) {
				int dualbyte = bytes[bitIndex / 8] << 8;
				if (bitIndex / 8 + 1 < bytes.Length)
					dualbyte |= bytes[bitIndex / 8 + 1];
				dualbyte = 0x1f & (dualbyte >> (16 - bitIndex % 8 - 5));
				output.Append(alphabet[dualbyte]);
			}
			return output.ToString().Substring(0, 26);
		}


	}
	[TestClass]
	public class UnitTest1 : TestBase {
		[TestMethod]
		public void Login() {
			if (Settings.LoginTests)
				RunTest(Api.LoginAsync());
		}
	}
	[TestClass]
	public class UserTests : TestBase {
		[TestMethod]
		public void GetUser() {
			RunTest(User.Get(Api));
		}
		[TestMethod]
		public void GetUsers() {
			ShowList(User.List(Api));
		}
		[TestMethod]
		public void Create100Users() {
			if (!Settings.DestructiveTests)
				return;
			UserInfo u = new UserInfo() {
				password = "n1234321",
				groups = new string[] { "everyone" }
			};
			for(int i = 0; i < 100; i++) {
				u.userid = "user" + i;
				u.displayName = "User " + i;
				u.email = "user" + i + "@ubuntu.local";
				RunTest(User.Create(Api, u));
			}
		}
		[TestMethod]
		public void ListGroups() {
			ShowList(User.GetGroups(Api, Api.Settings.Username));			
		}
		[TestMethod]
		public void ListSubadminGroups() {
			ShowList(User.GetSubadminGroups(Api, Api.Settings.Username));
		}
	}
	[TestClass]
	public class GroupTests : TestBase {
		[TestMethod]
		public void GetGroups() {
			ShowList(Group.List(Api));
		}
		[TestMethod]
		public void GetMembers() {
			ShowList(Group.GetMembers(Api, Settings.TestGroup));
		}
	}
	[TestClass]
	/// <summary>
	/// Must have Group Folders app enabled and set Settings:TestGroupFolderID
	/// </summary>
	public class GroupFolderTests : TestBase {
		[TestMethod]
		public void GetGroupFolders() {
			ShowList(GroupFolder.List(Api));
		}
		[TestMethod]
		public void GetGroupFolder() {
			RunTest(GroupFolder.Get(Api, Settings.TestGroupFolderID));
		}
		[TestMethod]
		public void CreateAndDelete() {
			int id = RunTest(GroupFolder.Create(Api, "test"));
			if(!RunTest(Group.List(Api, new ListRequest() { search = "test" })).List.Any(n => n == "test")) { 
				RunTest(Group.Create(Api, "test"));
			}
			GroupFolder g = RunTest(GroupFolder.Get(Api, id));
			RunTest(g.AddGroup(Api, "test"));
			RunTest(g.SetPermissions(Api, "test", GroupFolder.Permissions.All));
			RunTest(g.SetQuota(Api, 1000000000));
			RunTest(GroupFolder.Get(Api, g.id));
			RunTest(g.SetQuota(Api, -3));
			RunTest(GroupFolder.Get(Api, g.id));
			RunTest(GroupFolder.Delete(Api, g.id));
		}
	}
	[TestClass]
	public class FolderTests : TestBase {
		[TestMethod]
		public void List() {
			ShowList(CloudFolder.List(Api, Settings.Username));
		}
		[TestMethod]
		public void ListAll() {
			ShowList(CloudFolder.List(Api, Settings.Username + "/Documents", CloudInfo.Properties.All));
		}
		[TestMethod]
		public void Favorites() {
			string docs = Settings.Username + "/Documents";
			RunTest(CloudFolder.SetFavorite(Api, docs, true));
			ShowList(CloudFolder.GetFavorites(Api, Settings.Username, CloudInfo.Properties.All));
			RunTest(CloudFolder.SetFavorite(Api, docs, true));
			ShowList(CloudFolder.GetFavorites(Api, Settings.Username));
		}
		[TestMethod]
		public void CreateFolder() {
			RunTest(CloudFolder.Create(Api, Settings.Username + "/Documents/test"));
		}
		[TestMethod]
		public void DeleteFolder() {
			RunTest(CloudFolder.Delete(Api, Settings.Username + "/Documents/test"));
		}
		[TestMethod]
		public void ListComments() {
			
			CloudInfo props = RunTest(CloudInfo.GetProperties(Api, Settings.Username + "/Documents", CloudInfo.Properties.FileId));
			ShowList(Comment.List(Api, props.FileId));
		}
	}
}
