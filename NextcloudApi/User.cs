using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NextcloudApi {

	public class UserInfo : ApiEntryBase {
		public string userid;
		public string password;
		public string displayName;
		public string email;
		public string [] groups;
		public string [] subadmin;
		public string quota;
		public string language;
	}
	public class Quota : ApiEntryBase {
		public long free;
		public long used;
		public long total;
		public double relative;
		public long quota;
	}
	public class User : ApiEntryBase {
		public bool enabled;
		public string storageLocation;
		public string id;
		public DateTime lastLogin;
		public string backend;
		public string[] groups;
		public string[] subadmin;
		public Quota quota;
		public string email;
		public string displayname;
		public string phone;
		public string address;
		public string website;
		public string twitter;
		public string language;
		public string locale;
		public JObject backendCapabilities;

		static public async Task<ApiList<string>> List(Api api, ListRequest request = null) {
			return await api.GetListAsync<string>("ocs/v1.php/cloud/users", "ocs.data.users", request);
		}

		static public async Task<User> Get(Api api, string userid = null) {
			if (string.IsNullOrEmpty(userid))
				userid = api.Settings.User;
			if (string.IsNullOrEmpty(userid))
				userid = api.Settings.Username;
			OcsEntry entry = await api.GetAsync<OcsEntry>(Api.Combine("ocs/v1.php/cloud/users", userid));
			return entry.ocs.data.ConvertToObject<User>();
		}

		static public async Task Create(Api api, UserInfo info) {
			await api.PostAsync("ocs/v1.php/cloud/users", null, info);
		}

		public async Task Update(Api api, string password = null) {
			await api.PutAsync(Api.Combine("ocs/v1.php/cloud/users", id), null, new {
				email,
				quota.quota,
				displayname,
				phone,
				address,
				website,
				twitter,
				password
			});
		}

		static public async Task Disable(Api api, string userid) {
			await api.PutAsync(Api.Combine("ocs/v1.php/cloud/users", userid, "disable"));
		}

		static public async Task Enable(Api api, string userid) {
			await api.PutAsync(Api.Combine("ocs/v1.php/cloud/users", userid, "enable"));
		}

		static public async Task Delete(Api api, string userid) {
			await api.DeleteAsync(Api.Combine("ocs/v1.php/cloud/users", userid));
		}

		static public async Task<ApiList<string>> GetGroups(Api api, string userid, ListRequest request = null) {
			return await api.GetListAsync<string>(Api.Combine("ocs/v1.php/cloud/users", userid, "groups"), "ocs.data.groups", request);
		}

		static public async Task AddToGroup(Api api, string userid, string groupid) {
			await api.PostAsync(Api.Combine("ocs/v1.php/cloud/users", userid, "groups"), null, new {
				groupid
			});
		}

		static public async Task RemoveFromGroup(Api api, string userid, string groupid) {
			await api.DeleteAsync(Api.Combine("ocs/v1.php/cloud/users", userid, "groups"), new {
				groupid
			});
		}

		static public async Task PromoteToSubadminOfGroup(Api api, string userid, string groupid) {
			await api.PostAsync(Api.Combine("ocs/v1.php/cloud/users", userid, "subadmins"), null, new {
				groupid
			});
		}

		static public async Task DemoteFromSubadminOfGroup(Api api, string userid, string groupid) {
			await api.DeleteAsync(Api.Combine("ocs/v1.php/cloud/users", userid, "subadmins"), new {
				groupid
			});
		}

		static public async Task<ApiList<string>> GetSubadminGroups(Api api, string userid, ListRequest request = null) {
			return await api.GetListAsync<string>(Api.Combine("ocs/v1.php/cloud/users", userid, "subadmins"), "ocs.data", request);
		}

		static public async Task ResendWelcomeEmail(Api api, string userid) {
			await api.PostAsync(Api.Combine("ocs/v1.php/cloud/users", userid, "welcome"));
		}

	}
}
