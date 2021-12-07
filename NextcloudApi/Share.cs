using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System;

namespace NextcloudApi
{
    public class ShareResult
    {
        public List<Share> Shares;
    }

    public class ShareCreateInfo : ApiEntryBase
    {
        // Required
        // path to the file/folder which should be shared
        public string path { get; set; }

        // Required
        // 0 = user; 1 = group; 3 = public link; 4 = email; 6 = federated cloud share; 7 = circle; 10 = Talk conversation
        public int shareType { get; set; }

        // Required only if ShareType = 0 (User) or 1 (Group)
        // user / group id / email address / circleID / conversation name with which the file should be shared
        public string shareWith { get; set; }

        // allow public upload to a public shared folder (true/false)
        public string publicUpload { get; set; }

        // password to protect public link Share with
        public string password { get; set; }

        // This argument expects a well formatted date string, e.g. ‘YYYY-MM-DD’
        public string expireDate { get; set; }

        // 1 = read; 2 = update; 4 = create; 8 = delete; 16 = share; 31 = all
        // default: public shares = 1, all others = 31
        public int permissions { get; set; }
    }

    public class ShareUpdateInfo : ApiEntryBase
    {
        public int permissions { get; set; }
        public string password { get; set; }
        public string publicUpload { get; set; }

        // This argument expects a well formatted date string, e.g. ‘YYYY-MM-DD’
        public string expireDate { get; set; }
        public string note { get; set; }
    }

    public class Share : ApiEntryBase
    {
        public string id { get; set; }
        public int share_type { get; set; }
        public string uid_owner { get; set; }
        public string displayname_owner { get; set; }
        public int permissions { get; set; }
        public bool can_edit { get; set; }
        public bool can_delete { get; set; }
        public long stime { get; set; }
        public string expiration { get; set; }
        public string uid_file_owner { get; set; }
        public string note { get; set; }
        public string label { get; set; }
        public string displayname_file_owner { get; set; }
        public string path { get; set; }
        public string item_type { get; set; }
        public string mimetype { get; set; }
        public bool has_preview { get; set; }
        public string storage_id { get; set; }
        public long storage { get; set; }
        public long item_source { get; set; }
        public long file_source { get; set; }
        public long file_parent { get; set; }
        public string file_target { get; set; }
        public string share_with { get; set; }
        public string share_with_displayname { get; set; }
        public string password { get; set; }
        public bool send_password_by_talk { get; set; }
        public string url { get; set; }
        public bool mail_send { get; set; }
        public bool hide_download { get; set; }


        static public async Task<Share> Get(Api api, string shareid)
		{
			OcsShareEntry entry = await api.GetAsync<OcsShareEntry>(Api.Combine("ocs/v2.php/apps/files_sharing/api/v1/shares", shareid));
            return entry.ocs.data.FirstOrDefault().ConvertToObject<Share>();
		}

        static public async Task<PlainList<OcsShareEntry>> List(Api api)
        {
            return await api.GetPlainListAsync<OcsShareEntry>("ocs/v2.php/apps/files_sharing/api/v1/shares", "ocs.data");
        }

        static public async Task<string> Create(Api api, ShareCreateInfo info)
        {
            if (info.permissions.Equals(0))
            {
                if (info.shareType.Equals(3))
                {
                    info.permissions = 1;
                }
                else
                {
                    info.permissions = 31;
                }
            }

            OcsEntry r = await api.PostAsync<OcsEntry>("ocs/v2.php/apps/files_sharing/api/v1/shares", null, info);
            return r.ocs.data["id"].Value<string>();
        }

        public async Task Update(Api api)
        {
            var info = new
            {
                permissions = permissions,
                password = password,
                expireDate = expiration,
                note = note,
                hideDownload = hide_download.ToString().ToLower()
            };

            await api.PutAsync(Api.Combine("ocs/v2.php/apps/files_sharing/api/v1/shares", id), null, info);
        }




        static public async Task Delete(Api api, string shareid)
        {
            await api.DeleteAsync(Api.Combine("ocs/v2.php/apps/files_sharing/api/v1/shares", shareid));
        }


    }
}
