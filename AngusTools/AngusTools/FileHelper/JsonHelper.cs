using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AngusTools.FileHelper
{
    /// <summary>
    /// JSON 文件操作辅助类（线程安全、支持泛型、自动处理路径和序列化配置）
    /// </summary>
    public static class JsonHelper
    {
        /// <summary>
        /// 全局 JSON 序列化配置（可根据需求调整）
        /// </summary>
        private static readonly JsonSerializerOptions DefaultSerializerOptions = new()
        {
            // 支持驼峰命名（如：UserName -> userName）
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            // 忽略循环引用
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            // 允许注释和尾随逗号（提高兼容性）
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
            // 格式化输出（便于阅读）
            WriteIndented = true
        };

        /// <summary>
        /// 将对象序列化为 JSON 并保存到文件
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="path">文件完整路径（如：C:\data\config.json）</param>
        /// <param name="obj">要序列化的对象</param>
        /// <param name="options">自定义序列化配置（可选）</param>
        /// <exception cref="ArgumentNullException">路径或对象为空</exception>
        /// <exception cref="IOException">文件写入失败</exception>
        public static void SaveToJson<T>(string path, T obj, JsonSerializerOptions options = null)
        {
            // 参数校验
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException(nameof(path), "JSON 文件路径不能为空");
            if (obj == null)
                throw new ArgumentNullException(nameof(obj), "要序列化的对象不能为空");

            // 使用自定义配置或默认配置
            var serializeOptions = options ?? DefaultSerializerOptions;

            try
            {
                // 确保目录存在（如果不存在则创建）
                string directory = System.IO.Path.GetDirectoryName(path);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                // 序列化并写入文件（使用 using 确保资源释放）
                string json = JsonSerializer.Serialize(obj, serializeOptions);
                File.WriteAllText(path, json, Encoding.UTF8);
            }
            catch (Exception ex) when (ex is not ArgumentNullException)
            {
                throw new IOException($"保存 JSON 文件失败：{path}", ex);
            }
        }

        /// <summary>
        /// 从 JSON 文件反序列化为指定类型对象
        /// </summary>
        /// <typeparam name="T">目标对象类型</typeparam>
        /// <param name="path">文件完整路径</param>
        /// <param name="options">自定义反序列化配置（可选）</param>
        /// <returns>反序列化后的对象</returns>
        /// <exception cref="ArgumentNullException">路径为空</exception>
        /// <exception cref="FileNotFoundException">文件不存在</exception>
        /// <exception cref="JsonException">JSON 格式错误或反序列化失败</exception>
        /// <exception cref="IOException">文件读取失败</exception>
        public static T GetJson<T>(string path, JsonSerializerOptions options = null)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException(nameof(path), "JSON 文件路径不能为空");

            if (!File.Exists(path))
                throw new FileNotFoundException("JSON 文件不存在", path);

            try
            {
                // 读取文件内容（UTF-8 编码）
                string json = File.ReadAllText(path, Encoding.UTF8);
                var deserializeOptions = options ?? DefaultSerializerOptions;

                // 反序列化（支持 nullable 类型）
                return JsonSerializer.Deserialize<T>(json, deserializeOptions)
                    ?? throw new JsonException($"JSON 反序列化为 {typeof(T).Name} 失败：返回 null");
            }
            catch (FileNotFoundException)
            {
                throw; // 直接抛出文件不存在异常
            }
            catch (JsonException)
            {
                throw; // 直接抛出 JSON 格式错误异常
            }
            catch (Exception ex) when (ex is not ArgumentNullException)
            {
                throw new IOException($"读取 JSON 文件失败：{path}", ex);
            }
        }

        /// <summary>
        /// 重载：从 JSON 文件反序列化为 dynamic 类型
        /// </summary>
        /// <param name="path">文件完整路径</param>
        /// <param name="options">自定义反序列化配置（可选）</param>
        /// <returns>dynamic 类型的 JSON 数据</returns>
        public static dynamic GetJson(string path, JsonSerializerOptions options = null)
        {
            // 复用泛型方法，指定返回类型为 JsonElement（System.Text.Json 的 dynamic 实现）
            return GetJson<JsonElement>(path, options);
        }

        /// <summary>
        /// 尝试从 JSON 文件反序列化（不抛出异常）
        /// </summary>
        /// <typeparam name="T">目标对象类型</typeparam>
        /// <param name="path">文件完整路径</param>
        /// <param name="result">反序列化后的对象（失败时为默认值）</param>
        /// <param name="options">自定义反序列化配置（可选）</param>
        /// <returns>是否成功</returns>
        public static bool TryGetJson<T>(string path, out T result, JsonSerializerOptions options = null)
        {
            result = default;
            try
            {
                result = GetJson<T>(path, options);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}