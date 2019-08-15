using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NextcloudApi {

	public class Mention : ApiEntryBase {
		public string mentionType;
		public string mentionId;
		public string mentionDisplayName;
	}

	public class CommentList : ApiList<Comment> {
		public XDocument PostParams() {
			XDocument postParams = new XDocument();
			XElement root = new XElement("{http://owncloud.org/ns}filter-comments");
			postParams.Add(root);
			root.Add(new XElement("{http://owncloud.org/ns}limit", Request.limit));
			root.Add(new XElement("{http://owncloud.org/ns}offset", Request.offset));
			return postParams;
		}
		public override async Task<ApiList<Comment>> Read(Api api) {
			XElement data = await api.SendMessageAsyncAndGetXmlResponse(Api.REPORT, MetaData.Uri, PostParams());
			return Convert(data);
		}
		public CommentList Convert(XElement data) {
			CommentList result = new CommentList() {
				MetaData = MetaData,
				Request = Request,
				Path = Path,
				List = new List<Comment>()
			};
			foreach(XElement e in data.Elements()) {
				JObject props = new JObject();
				Api.FillJObject(props, data);
				result.List.Add(props.ConvertToObject<Comment>());
			}
			return result;
		}
	}
	public class Comment : ApiEntryBase {
		public string id;
		public string parentId;
		public string topmostParentId;
		public int? childrenCount;
		public string verb;
		public string actorType;
		public string actorId;
		public DateTime? creationDateDime;
		public DateTime? latestChildDateTime;
		public string objectType;
		public string objectId;
		public bool? isUnread;
		public string message;
		public string actorDisplayName;
		public Mention[] mentions;

		public static async Task Create(Api api, string fileId, string message) {
			string result = await api.SendMessageAsyncAndGetStringResponse(HttpMethod.Post, Api.Combine("remote.php/dav/comments/files", fileId), 
				new {
					actorType = "users",
					message,
					objectType = "files",
					verb = "comment"
				});
		}

		public static async Task<ApiList<Comment>> List(Api api, string fileId, ListRequest request = null) {
			CommentList r = new CommentList() {
				Request = request ?? new ListRequest(),
				MetaData = new MetaData() {
					Uri = Api.Combine("remote.php/dav/comments/files", fileId)
				}
			};
			return await r.Read(api);
		}

	}
}
