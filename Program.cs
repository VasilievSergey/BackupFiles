using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace BackupFiles
{
    class Program
    {
        public class Settings
        {
            public required List<string> SourcePaths { get; set; }
            public required string DestinationPath { get; set; }
            public required string LogLevel { get; set; }
        }

        public enum LogLevel
        {
            Error,
            Info,
            Debug
        }

        private static LogLevel currentLogLevel;

        static void Main(string[] args)
        {
            string settingsFilePath = "settings.json";
            Settings settings;

            // Загрузка настроек из файла
            try
            {
                string json = File.ReadAllText(settingsFilePath);
                settings = JsonConvert.DeserializeObject<Settings>(json);
                currentLogLevel = Enum.Parse<LogLevel>(settings.LogLevel);
                Log("Запуск приложения.", LogLevel.Info);
            }
            catch (Exception ex)
            {
                Log("Ошибка при загрузке настроек: " + ex.Message, LogLevel.Error);
                return;
            }

            // Проверка существования целевой папки
            if (!Directory.Exists(settings.DestinationPath))
            {
                Log("Целевая папка не найдена: " + settings.DestinationPath, LogLevel.Error);
                return;
            }

            // Создание файла журнала
            string logFilePath = Path.Combine(settings.DestinationPath, $"backup_log_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
            using (StreamWriter logWriter = new StreamWriter(logFilePath))
            {
                foreach (var sourcePath in settings.SourcePaths)
                {
                    if (!Directory.Exists(sourcePath))
                    {
                        Log($"Исходная папка не найдена: {sourcePath}", LogLevel.Error, logWriter);
                        continue;
                    }

                    string backupFolder = Path.Combine(settings.DestinationPath, $"Backup_{Path.GetFileName(sourcePath)}_{DateTime.Now:yyyyMMdd_HHmmss}");
                    Directory.CreateDirectory(backupFolder);
                    Log($"Создана папка резервного копирования: {backupFolder}", LogLevel.Info, logWriter);

                    // Копирование файлов
                    foreach (var file in Directory.GetFiles(sourcePath))
                    {
                        try
                        {
                            string fileName = Path.GetFileName(file);
                            string destFile = Path.Combine(backupFolder, fileName);
                            File.Copy(file, destFile);
                            Log($"Скопирован файл: {fileName}", LogLevel.Debug, logWriter);
                        }
                        catch (Exception ex)
                        {
                            Log($"Ошибка при копировании файла {file}: {ex.Message}", LogLevel.Error, logWriter);
                        }
                    }
                }
                Log("Резервное копирование завершено.", LogLevel.Info, logWriter);
            }
        }

        private static void Log(string message, LogLevel level, StreamWriter writer = null)
        {
            if (writer == null)
            {
                writer = new StreamWriter("log.txt", true);
            }

            if (level <= currentLogLevel)
            {
                string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}";
                Console.WriteLine(logMessage);
                writer.WriteLine(logMessage);
                writer.Flush();
            }

            
        }
    }
}
