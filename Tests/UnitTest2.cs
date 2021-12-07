using Microsoft.VisualStudio.TestTools.UnitTesting;
using NextcloudApi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests {
	[TestClass]
	public class ShareTests : TestBase
	{
		[TestMethod]
		public void GetShares()
		{
			ShowList(Share.List(Api));
		}
		[TestMethod]
		public void GetShare()
		{
			RunTest(Share.Get(Api, Settings.TestShareID));
		}
		[TestMethod]
		public void CreateAndDelete()
		{
            string id = RunTest(Share.Create(Api, new ShareCreateInfo
            {
				path = "Documents/test.png",
				shareType = 3
            }));

			RunTest(Share.Delete(Api, id));
		}

		[TestMethod]
		public void Update()
		{
			var share = RunTest(Share.Get(Api, "33"));

			share.permissions = 23;
			share.password = "";
			share.hide_download = false;
			share.expiration = "2022-01-31";

			RunTest(share.Update(Api));

			var after = RunTest(Share.Get(Api, "33"));
		}

	}
}
