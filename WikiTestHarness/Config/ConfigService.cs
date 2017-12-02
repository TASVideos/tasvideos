using System;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace WikiTestHarness
{
	public static class ConfigService
	{
		private static readonly JsonSerializer Serializer;

		static ConfigService()
		{
			Serializer = new JsonSerializer
			{
				MissingMemberHandling = MissingMemberHandling.Ignore,
				TypeNameHandling = TypeNameHandling.Auto,
				ConstructorHandling = ConstructorHandling.Default,
			};
		}

		public static T Load<T>(string filepath) where T : new()
		{
			T config = default(T);

			try
			{
				var file = new FileInfo(filepath);
				if (file.Exists)
				{
					using (var reader = file.OpenText())
					{
						var r = new JsonTextReader(reader);
						config = (T)Serializer.Deserialize(r, typeof(T));
					}
				}
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException("Config Error", ex);
			}

			if (config == null)
			{
				return new T();
			}

			return config;
		}

		public static void Save(string filepath, object config)
		{
			var file = new FileInfo(filepath);
			try
			{
				using (var writer = file.CreateText())
				{
					var w = new JsonTextWriter(writer) { Formatting = Formatting.Indented };
					Serializer.Serialize(w, config);
				}
			}
			catch
			{
				/* Eat it */
			}
		}
	}
}
