using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace NextcloudApi
{
    public class OcsShare
    {
        public Meta meta;
        public List<JObject> data;
    }

    public class OcsShareEntry : ApiEntry
    {
        public OcsShare ocs;
    }
}
