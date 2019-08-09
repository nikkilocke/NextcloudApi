using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NextcloudApi {
	public class CloudInfo : ApiEntryBase {
		protected static readonly HttpMethod PROPFIND = new HttpMethod("PROPFIND");
		protected static readonly HttpMethod MKCOL = new HttpMethod("MKCOL");
		protected static readonly HttpMethod MOVE = new HttpMethod("MOVE");
		protected static readonly HttpMethod COPY = new HttpMethod("COPY");
		protected static readonly HttpMethod PROPPATCH = new HttpMethod("PROPPATCH");
		protected static readonly HttpMethod REPORT = new HttpMethod("REPORT");
		protected static readonly HttpMethod SEARCH = new HttpMethod("SEARCH");
		[Flags]
		public enum Properties {
			LastModified = 1,
			Tag = 2,
			Type = 4,
			Length = 8,
			Id = 16,
			FileId = 32,
			Favorite = 64,
			CommentsHref = 128,
			CommentsCount = 256,
			CommentsUnread = 512,
			OwnerId = 1024,
			OwnerDisplayName = 2048,
			ShareTypes = 4096,
			Checksums = 8192,
			HasPreview = 16384,
			Size = 32768,
			QuotaUsed = 65536,
			QuotaAvailable = 131072,
			Basic = LastModified|Tag|Type|Length|QuotaUsed|QuotaAvailable,
			All = 262143
		}

		[JsonProperty("href")]
		public string Path;
		[JsonProperty("getlastmodified")]
		public DateTime LastModified;
		[JsonProperty("getetag")]
		public string Tag;
		[JsonProperty("id")]
		public string Id;
		[JsonProperty("fileid")]
		public string FileId;
		[JsonProperty("favorite")]
		public int Favorite;
		[JsonProperty("comments-href")]
		public string CommentsHref;
		[JsonProperty("comments-count")]
		public int CommentsCount;
		[JsonProperty("comments-unread")]
		public int CommentsUnread;
		[JsonProperty("owner-id")]
		public string OwnerId;
		[JsonProperty("owner-display-name")]
		public string OwnerDisplayName;
		[JsonProperty("share-types")]
		public string ShareTypes;
		[JsonProperty("checksums")]
		public string Checksums;
		[JsonProperty("size")]
		public long Size;

		static public void FillJObject(JObject j, XElement x) {
			foreach (XElement e in x.Elements()) {
				if (e.HasElements)
					FillJObject(j, e);
				else
					j[e.Name.LocalName] = e.Value;

			}
		}
		static public CloudInfo Parse(XElement data) {
			JObject props = new JObject();
			FillJObject(props, data);
			System.Diagnostics.Debug.WriteLine(props.ToHumanReadableJson());
			if (props["collection"] == null)
				return props.ToObject<CloudFile>();
			return props.ToObject<CloudFolder>();
		}

	}
	public class CloudFile : CloudInfo {
		[JsonProperty("has-preview")]
		public string HasPreview;
		[JsonProperty("getcontentlength")]
		public long Length;
		[JsonProperty("getcontenttype")]
		public string Type;
	}
	public class CloudFolder : CloudInfo {
		[JsonProperty("quota-used")]
		public string QuotaUsed;
		[JsonProperty("quota-available")]
		public string QuotaAvailable;

		static public async Task<List<CloudInfo>> List(Api api, string path, Properties properties = Properties.Basic) {
			XDocument postParams = EncodeProperties(properties);
			string data = await GetResponse(api, PROPFIND, path, postParams);
			XElement result = XElement.Parse(data);
			return new List<CloudInfo>(result.Elements().Select(t => CloudInfo.Parse(t)));
		}

		static public async Task Create(Api api, string path) {
			await GetResponse(api, MKCOL, path);
		}

		static public async Task Delete(Api api, string path) {
			await GetResponse(api, HttpMethod.Delete, path);
		}

		static public async Task Move(Api api, string source, string dest) {
			await GetResponse(api, MOVE, source, null, new { Destination = api.MakeUri(Api.Combine("remote.php/dav/files", dest)) });
		}

		static public async Task SetFavorite(Api api, string path, bool favorite) {
			XDocument postParams = new XDocument();
			XElement p = new XElement("{DAV:}propertyupdate");
			postParams.Add(p);
			XElement set = new XElement("{DAV:}set");
			p.Add(set);
			XElement prop = new XElement("{DAV:}prop");
			set.Add(prop);
			prop.Add(new XElement("{http://owncloud.org/ns}favorite", favorite ? "1" : "0"));
			await GetResponse(api, PROPPATCH, path, postParams);
		}

		static public async Task<List<CloudInfo>> GetFavorites(Api api, string path, Properties properties = Properties.Basic) {
			XDocument postParams = new XDocument();
			XElement p = new XElement("{http://owncloud.org/ns}filter-files");
			postParams.Add(p);
			XElement rules = new XElement("{http://owncloud.org/ns}filter-rules");
			p.Add(rules);
			rules.Add(new XElement("{http://owncloud.org/ns}favorite", "1"));
			XDocument propParams = EncodeProperties(properties);
			if(propParams != null)
				p.Add(propParams.Element("{DAV:}propfind").Element("{DAV:}prop"));
			string data = await GetResponse(api, REPORT, path, postParams);
			XElement result = XElement.Parse(data);
			return new List<CloudInfo>(result.Elements().Select(t => CloudInfo.Parse(t)));
		}

		static public async Task<List<CloudInfo>> Search(Api api, XDocument query) {
			string data = await GetResponse(api, REPORT, null, query);
			XElement result = XElement.Parse(data);
			return new List<CloudInfo>(result.Elements().Select(t => CloudInfo.Parse(t)));
		}

		static async Task<string> GetResponse(Api api, HttpMethod method, string path = null, XDocument postParams = null, object headers = null) {
			if (path == null) {
				path = "remote.php/dav";
			} else {
				path = path.Replace("\\", "/");
				while (path.StartsWith("/"))
					path = path.Substring(1);
				if (Regex.IsMatch(path, @"/\.*/"))
					throw new ApplicationException("Invalid path:" + path);
				path = Api.Combine("remote.php/dav/files", path);
			}
			string uri = api.MakeUri(path);
			using (HttpResponseMessage response = await api.SendMessageAsyncAndGetResponse(method, uri, postParams, headers)) {
				string data = await response.Content.ReadAsStringAsync();
				if (api.Settings.LogResult > 0 || !response.IsSuccessStatusCode)
					api.Log("Received Data -> " + data);
				if (!response.IsSuccessStatusCode)
					throw new ApiException(response.ReasonPhrase, data);
				return data;
			}
		}

		static XDocument EncodeProperties(Properties properties) {
			XDocument postParams = null;
			if (properties != Properties.Basic) {
				postParams = new XDocument();
				XElement p = new XElement("{DAV:}propfind");
				postParams.Add(p);
				XElement prop = new XElement("{DAV:}prop");
				p.Add(prop);
				prop.Add(new XElement("{DAV:}resourcetype"));   // Always ask for this, it's how we tell folder from file
				if (properties.HasFlag(Properties.LastModified))
					prop.Add(new XElement("{DAV:}getlastmodified"));
				if (properties.HasFlag(Properties.Tag))
					prop.Add(new XElement("{DAV:}getetag"));
				if (properties.HasFlag(Properties.Type))
					prop.Add(new XElement("{DAV:}getcontenttype"));
				if (properties.HasFlag(Properties.Length))
					prop.Add(new XElement("{DAV:}getcontentlength"));
				if (properties.HasFlag(Properties.Id))
					prop.Add(new XElement("{http://owncloud.org/ns}id"));
				if (properties.HasFlag(Properties.FileId))
					prop.Add(new XElement("{http://owncloud.org/ns}fileid"));
				if (properties.HasFlag(Properties.Favorite))
					prop.Add(new XElement("{http://owncloud.org/ns}favorite"));
				if (properties.HasFlag(Properties.CommentsHref))
					prop.Add(new XElement("{http://owncloud.org/ns}comments-href"));
				if (properties.HasFlag(Properties.CommentsCount))
					prop.Add(new XElement("{http://owncloud.org/ns}comments-count"));
				if (properties.HasFlag(Properties.CommentsUnread))
					prop.Add(new XElement("{http://owncloud.org/ns}comments-unread"));
				if (properties.HasFlag(Properties.OwnerId))
					prop.Add(new XElement("{http://owncloud.org/ns}owner-id"));
				if (properties.HasFlag(Properties.OwnerDisplayName))
					prop.Add(new XElement("{http://owncloud.org/ns}owner-display-name"));
				if (properties.HasFlag(Properties.ShareTypes))
					prop.Add(new XElement("{http://owncloud.org/ns}share-types"));
				if (properties.HasFlag(Properties.Checksums))
					prop.Add(new XElement("{http://owncloud.org/ns}checksums"));
				if (properties.HasFlag(Properties.HasPreview))
					prop.Add(new XElement("{http://owncloud.org/ns}has-preview"));
				if (properties.HasFlag(Properties.Size))
					prop.Add(new XElement("{http://owncloud.org/ns}size"));
				if (properties.HasFlag(Properties.QuotaUsed))
					prop.Add(new XElement("{DAV:}quota-used-bytes"));
				if (properties.HasFlag(Properties.QuotaAvailable))
					prop.Add(new XElement("{DAV:}quota-available-bytes"));
			}
			return postParams;
		}

	}
}
