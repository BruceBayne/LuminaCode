using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using AiReview.Core.LLM.Review;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace AiReview.Core.Database;

public static class SolutionDatabase
{
	private class WritablePropertiesContractResolver(IEnumerable<string> propertiesToIgnore)
		: DefaultContractResolver
	{
		private readonly HashSet<string> propertiesToIgnore = [.. propertiesToIgnore];


		protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
		{
			// Get the properties using reflection
			var properties = base.CreateProperties(type, memberSerialization);

			// Filter the properties to include only those with setters
			return properties.Where(p => p.Writable).ToList();
		}

		protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
		{
			var property = base.CreateProperty(member, memberSerialization);

			if (propertiesToIgnore.Contains(property.PropertyName))
			{
				property.ShouldSerialize = _ => false;
			}

			return property;
		}
	}

	public static readonly JsonSerializerSettings SerializerSettings = new()
	{
		Formatting = Formatting.Indented,
		DefaultValueHandling = DefaultValueHandling.Ignore,
		NullValueHandling = NullValueHandling.Ignore,
		ContractResolver = new WritablePropertiesContractResolver([
			nameof(CodeReviewSummary.RawRequest), nameof(CodeReviewSummary.RawResponse)
		])
	};

	public static ReviewDatabase CreateOrLoadDatabase(string dbPath)
	{
		if (File.Exists(dbPath))
		{
			var content = File.ReadAllText(dbPath);

			if (!string.IsNullOrEmpty(content) || !string.IsNullOrWhiteSpace(content) || content.Length > 5)
			{
				return JsonConvert.DeserializeObject<ReviewDatabase>(content, SerializerSettings) ?? CreateNew(dbPath);
			}
		}

		return CreateNew(dbPath);
	}

	private static ReviewDatabase CreateNew(string dbPath)
	{
		var database = new ReviewDatabase() { };
		File.WriteAllText(dbPath, JsonConvert.SerializeObject(database, SerializerSettings));
		return database;
	}
}