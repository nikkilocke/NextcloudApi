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
			RunTest(Share.Get(Api, "53"));
		}
		[TestMethod]
		public void UpdateShare()
		{
			var share = RunTest(Share.Get(Api, "53"));
			RunTest(share.Update(Api, share));
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
		public void GetAndUpdate()
		{
            var shareID = "54";
            var share = RunTest(Share.Get(Api, shareID));

            share.permissions = 17;
            share.password = "";
            share.hide_download = false;
            share.expiration = "2026-01-31";

            RunTest(share.Update(Api, share));

			var after = RunTest(Share.Get(Api, shareID));

		}

	}
}
