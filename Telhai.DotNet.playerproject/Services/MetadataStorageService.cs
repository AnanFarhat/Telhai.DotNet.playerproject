using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Telhai.DotNet.PlayerProject.Models;

namespace Telhai.DotNet.PlayerProject.Services
{
    internal class MetadataStorageService
    {
        private readonly string _filePath;

        public MetadataStorageService()
        {
            _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "songs_metadata.json");
        }

        public Dictionary<string, SongMetadata> LoadAll()
        {
            if (!File.Exists(_filePath))
                return new Dictionary<string, SongMetadata>();

            var json = File.ReadAllText(_filePath);

            if (string.IsNullOrWhiteSpace(json))
                return new Dictionary<string, SongMetadata>();

            return JsonSerializer.Deserialize<Dictionary<string, SongMetadata>>(json)
                   ?? new Dictionary<string, SongMetadata>();
        }

        public void SaveAll(Dictionary<string, SongMetadata> data)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            var json = JsonSerializer.Serialize(data, options);
            File.WriteAllText(_filePath, json);
        }

        public SongMetadata? Get(string filePath)
        {
            var all = LoadAll();
            return all.ContainsKey(filePath) ? all[filePath] : null;
        }

        public void Save(string filePath, SongMetadata metadata)
        {
            var all = LoadAll();
            all[filePath] = metadata;
            SaveAll(all);
        }
        public SongMetadata? TryGet(string filePath)
        {
            var all = LoadAll();
            return all.TryGetValue(filePath, out var meta) ? meta : null;
        }

        public void Upsert(string filePath, SongMetadata meta)
        {
            var all = LoadAll();
            all[filePath] = meta;
            SaveAll(all);
        }

    }
}
