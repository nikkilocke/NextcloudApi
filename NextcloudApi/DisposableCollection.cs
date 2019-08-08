using System;
using System.Collections.Generic;
using System.Text;

namespace NextcloudApi {

	/// <summary>
	/// Class to collect a bunch of IDisposables, and Dispose of them all when it is disposed
	/// </summary>
	public class DisposableCollection : List<IDisposable>, IDisposable {

		/// <summary>
		/// Add an IDisposable to the collection
		/// </summary>
		/// <typeparam name="T">Any type that implements IDisposable</typeparam>
		/// <param name="item">The object to add</param>
		/// <returns>The added object, so you can use it in line like FileStream f = collection.Add(new FilStream(...))</returns>
		public T Add<T>(T item) where T : IDisposable {
			base.Add(item);
			return item;
		}

		/// <summary>
		/// Disposes of all the objects in the collection
		/// </summary>
		public void Dispose() {
			foreach(var v in this) {
				try {
					v.Dispose();
				} catch {
				}
			}
		}
	}
}
