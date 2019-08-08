using System.Threading.Tasks;

namespace NextcloudApi {
	public class Group {
		static public async Task<ApiList<string>> List(Api api, ListRequest request = null) {
			return await api.GetListAsync<string>("/ocs/v1.php/cloud/groups", "ocs.data.groups", request);
		}

		static public async Task Create(Api api, string groupid) {
			await api.PostAsync("ocs/v1.php/cloud/groups", null, new { groupid });
		}

		static public async Task<ApiList<string>> GetMembers(Api api, string groupid, ListRequest request = null) {
			return await api.GetListAsync<string>(Api.Combine("/ocs/v1.php/cloud/groups", groupid), "ocs.data.users", request);
		}

		static public async Task<ApiList<string>> GetSubadmins(Api api, string groupid, ListRequest request = null) {
			return await api.GetListAsync<string>(Api.Combine("/ocs/v1.php/cloud/groups", groupid, "subadmins"), "ocs.data", request);
		}

		static public async Task Delete(Api api, string groupid) {
			await api.DeleteAsync(Api.Combine("ocs/v1.php/cloud/groups", groupid));
		}

	}
}
