using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

namespace NextcloudApi {
	public class UnixMsecDateTimeConverter : Newtonsoft.Json.JsonConverter {
		static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		public override bool CanConvert(Type objectType) {
			return objectType == typeof(DateTime) || objectType == typeof(DateTime?);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
			var t = (long?)reader.Value;
			return t == null ? (DateTime?)null : t == 0 ? DateTime.MinValue : epoch.AddMilliseconds((long)t);
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
			long? msec = value == null ? (long?)null : (long)((DateTime)value - epoch).TotalMilliseconds;
			writer.WriteValue(msec);
		}
	}

	public static class Extensions {
		static Extensions() {
			_humanSettings = new JsonSerializerSettings() {
				NullValueHandling = NullValueHandling.Ignore
			};
			// Force Enums to be converted as strings
			_humanSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
			_apiSettings = new JsonSerializerSettings() {
				NullValueHandling = NullValueHandling.Ignore
			};
			// Force Enums to be converted as strings
			_apiSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
			// Convert dates to Unix msec timestamps
			_apiSettings.Converters.Add(new UnixMsecDateTimeConverter());
			_serializer = JsonSerializer.Create(_apiSettings);
		}

		static readonly JsonSerializerSettings _humanSettings;
		static readonly JsonSerializerSettings _apiSettings;
		static readonly JsonSerializer _serializer;

		/// <summary>
		/// Convert object to Json string. 
		/// Note Enums are converted as strings, and Dates as Unix msec timestamps.
		/// </summary>
		public static string ToJson(this object o) {
			return Newtonsoft.Json.JsonConvert.SerializeObject(o, Newtonsoft.Json.Formatting.Indented, _apiSettings);
		}

		public static string ToJsonString(this JToken token) {
			switch (token.Type) {
				case JTokenType.Boolean:
					return token.ToString().ToLower();
				default:
					return token.ToString();
			}
		}

		/// <summary>
		/// Convert object to collection of KeyValuePairs, for posting as form data.
		/// If one of the elements of o is an object, each member of the object is included separately
		/// in the form "objectname[elementname]" elementvalue
		/// </summary>
		public static IEnumerable<KeyValuePair<string, string>> ToCollection(this object o) {
			JObject j = o.ToJObject();
			foreach (JProperty v in j.Properties()) {
				switch (v.Value.Type) {
					case JTokenType.Object:
						foreach (JProperty s in ((JObject)v.Value).Properties()) {
							if (s.Value.Type != JTokenType.Null)
								yield return new KeyValuePair<string, string>($"{v.Name}[{s.Name}]", s.Value.ToJsonString());
						}
						break;
					case JTokenType.Array:
						foreach (JToken a in ((JArray)v.Value))
							yield return new KeyValuePair<string, string>(v.Name + "[]", a.ToJsonString());
						break;
					case JTokenType.Null:
						break;
					default:
						yield return new KeyValuePair<string, string>(v.Name, v.Value.ToJsonString());
						break;
				}
			}
		}

		/// <summary>
		/// Convert object to Json string. 
		/// Note Enums are converted as strings.
		/// </summary>
		public static string ToHumanReadableJson(this object o) {
			return Newtonsoft.Json.JsonConvert.SerializeObject(o, Newtonsoft.Json.Formatting.Indented, _humanSettings);
		}

		/// <summary>
		/// Convert Object to JObject.
		/// Note Enums are converted as strings, and Dates as Unix msec timestamps.
		/// </summary>
		public static JObject ToJObject(this object o) {
			return o is JObject ? o as JObject : JObject.FromObject(o, _serializer);
		}

		/// <summary>
		/// Convert JToken to Object.
		/// Note Enums are converted as strings, and Dates as Unix msec timestamps.
		/// </summary>
		public static T ConvertToObject<T>(this JToken self) {
			return self.ToObject<T>(_serializer);
		}

		/// <summary>
		/// Is a JToken null or empty
		/// </summary>
		public static bool IsNullOrEmpty(this JToken token) {
			return (token == null) ||
				   (token.Type == JTokenType.Array && !token.HasValues) ||
				   (token.Type == JTokenType.Object && !token.HasValues) ||
				   (token.Type == JTokenType.String && token.ToString() == String.Empty) ||
				   (token.Type == JTokenType.Null);
		}

#if false
		public static void Get<T>(this XElement element, string path, out T value) {
			XElement r = element.XPathSelectElements(path).FirstOrDefault();
			value = r == null ? default(T) : (T)Convert.ChangeType(r.Value, typeof(T));
		}
#endif

	}

	/// <summary>
	/// Just an object whose ToString shows the whole object as Json, for debugging.
	/// </summary>
	public class ApiEntryBase {
		override public string ToString() {
			return this.ToHumanReadableJson();
		}
	}

	/// <summary>
	/// Information sent to and returned from an Api call
	/// </summary>
	public class MetaData {
		/// <summary>
		/// The Uri called
		/// </summary>
		public string Uri;
		/// <summary>
		/// Last modified date for caching.
		/// </summary>
		public DateTime Modified;
	}

	public class Meta {
		public string status;
		public int statuscode;
		public string message;
		public int totalitems;
		public int itemsperpage;
	}

	/// <summary>
	/// Standard Api call return value.
	/// </summary>
	public class ApiEntry : ApiEntryBase {
		/// <summary>
		/// MetaData about the call and return values
		/// </summary>
		public MetaData MetaData;
		/// <summary>
		/// Any unexpected json items returned will be in here
		/// </summary>
		[JsonExtensionData]
		public IDictionary<string, JToken> AdditionalData;
#if DEBUG
		/// <summary>
		/// For debugging, to flag up when there is AdditionalData
		/// </summary>
		/// <returns></returns>
		public override string ToString() {
			if (AdditionalData != null && AdditionalData.Count > 0) {
				System.Diagnostics.Debug.WriteLine("***ADDITIONALDATA***");
				System.Diagnostics.Debug.WriteLine(AdditionalData.ToHumanReadableJson());
			}
			return base.ToString();
		}
#endif
	}
	public class Ocs {
		public Meta meta;
		public JObject data;
	}

	public class OcsEntry : ApiEntry {
		public Ocs ocs;
	}

	/// <summary>
	/// Standard Api call single page List return
	/// </summary>
	/// <typeparam name="T">The type of item in the List</typeparam>
	public class PlainList<T> : ApiEntry {
		public static PlainList<T> EmptyList(string uri) {
			PlainList<T> list = new PlainList<T> {
				MetaData = new MetaData() { Uri = uri }
			};
			return list;
		}

		public string Path;

		/// <summary>
		/// List of items
		/// </summary>
		public List<T> List = new List<T>();

		/// <summary>
		/// Number of items retrieved in this chunk.
		/// </summary>
		public int Count {
			get { return List.Count; }
		}

		/// <summary>
		/// Convert a JObject into the current type.
		/// </summary>
		public virtual PlainList<T> Convert(JObject j) {
			return new PlainList<T>() {
				MetaData = j["MetaData"].ConvertToObject<MetaData>(),
				Path = Path,
				List = j.SelectToken(Path).ConvertToObject<List<T>>()
			};
		}
	}


	/// <summary>
	/// Requests to return lists which support paging
	/// </summary>
	public class ListRequest : ApiEntryBase {
		public string search;
		public int limit = 50;
		public int offset;
		public JObject PostParameters;
	}

	public class ApiList : ApiEntry {
		public ListRequest Request;

		public int RequestedCount {
			get { return Request == null ? 0 : Request.offset + Request.limit; }
		}

	}
	/// <summary>
	/// Standard Api call List return
	/// </summary>
	/// <typeparam name="T">The type of item in the List</typeparam>
	public class ApiList<T> : ApiList {
		public static ApiList<T> EmptyList(string uri) {
			ApiList<T> list = new ApiList<T>() {
				MetaData = new MetaData() { Uri = uri }
			};
			list.Request = new ListRequest();
			return list;
		}

		public string Path;

		/// <summary>
		/// List of items returned in this chunk.
		/// </summary>
		public List<T> List = new List<T>();

		/// <summary>
		/// Number of items retrieved in this chunk.
		/// </summary>
		public int Count {
			get { return List.Count; }
		}

		/// <summary>
		/// There is data on the server we haven't fetched yet
		/// </summary>
		public bool HasMoreData {
			get { return List.Count == Request.limit; }
		}

		/// <summary>
		/// Get the next chunk of data from the server
		/// </summary>
		public async Task<ApiList<T>> GetNext(Api api) {
			if (!HasMoreData)
				return null;
			Request.offset += Request.limit;
			return await Read(api);
		}

		public virtual async Task<ApiList<T>> Read(Api api) {
			JObject j = await api.SendMessageAsync(Request.PostParameters == null ? HttpMethod.Get : HttpMethod.Post,
							Api.AddGetParams(MetaData.Uri, new {
								Request.search,
								Request.offset,
								Request.limit
							}), Request.PostParameters);
			return Convert(j);
		}

		/// <summary>
		/// Return an Enumerable of all the items in the list, getting more from the server when required
		/// </summary>
		/// <param name="api"></param>
		/// <returns></returns>
		public IEnumerable<T> All(Api api) {
			ApiList<T> chunk = this;
			while (chunk != null && chunk.Count > 0) {
				foreach (T t in chunk.List)
					yield return t;
				chunk = chunk.GetNext(api).Result;
			}
		}

		/// <summary>
		/// Convert a JObject into the current type.
		/// </summary>
		public virtual ApiList<T> Convert(JObject j) {
			return new ApiList<T>() {
				MetaData = j["MetaData"].ConvertToObject<MetaData>(),
				Request = Request,
				Path = Path,
				List = j.SelectToken(Path).ConvertToObject<List<T>>()
			};
		}

	}

	public class ApiCollection<T> : ApiList<T> {
		public override ApiList<T> Convert(JObject j) {
			JToken col = j.SelectToken(Path);
			return new ApiCollection<T>() {
				MetaData = j["MetaData"].ConvertToObject<MetaData>(),
				Request = Request,
				Path = Path,
				List = new List<T>(col.Values().Select(v => v.ConvertToObject<T>()))
			};
		}
	}

	public class PlainCollection<T> : PlainList<T> {
		public override PlainList<T> Convert(JObject j) {
			JToken col = j.SelectToken(Path);
			return new PlainCollection<T>() {
				MetaData = j["MetaData"].ConvertToObject<MetaData>(),
				Path = Path,
				List = new List<T>(col.Values().Select(v => v.ConvertToObject<T>()))
			};
		}
	}

}