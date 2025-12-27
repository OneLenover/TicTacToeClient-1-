using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace TicTacToeClient_1_
{
    public class AppConfig
    {
        public GrpcConfig gRPC { get; set; } = null!;

        public static AppConfig Load(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Файл конфигурации {path} не найден.");

            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<AppConfig>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            })!;
        }

    }

    public class GrpcConfig
    {
        public string Host { get; set; } = null!;
        public int Port { get; set; }
    }
}
