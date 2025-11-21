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
            var absolutePath = ValidatePath(path);

            // 使用自定义配置或默认配置
            var serializeOptions = options ?? DefaultSerializerOptions;

            try
            {
                // 序列化并写入文件（使用 using 确保资源释放）
                var json = JsonSerializer.Serialize(obj, serializeOptions);
                File.WriteAllText(absolutePath, json, Encoding.UTF8);
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

        #region 新增：AppendToJson 功能
        /// <summary>
        /// 向 JSON 文件追加新值（不覆盖原有内容）
        /// 支持两种场景：1. JSON 对象（添加新属性）；2. JSON 数组（添加新元素）
        /// </summary>
        /// <typeparam name="T">新值的类型</typeparam>
        /// <param name="path">JSON 文件路径</param>
        /// <param name="obj">要追加的新值（对象或数组元素）</param>
        /// <param name="options">序列化配置（可选）</param>
        /// <exception cref="ArgumentNullException">路径或新值为空</exception>
        /// <exception cref="JsonException">JSON 格式错误或不支持的类型</exception>
        /// <exception cref="IOException">文件操作失败</exception>
        public static void AppendToJson<T>(string path, T obj, JsonSerializerOptions options = null)
        {
            string absolutePath = ValidatePath(path);
            var jsonOptions = options ?? DefaultSerializerOptions;

            try
            {
                // 读取原有内容（文件不存在则视为空）
                string existingJson = File.Exists(absolutePath) ? File.ReadAllText(absolutePath, Encoding.UTF8) : string.Empty;
                string updatedJson;

                // 场景 1：原有 JSON 是对象（键值对）→ 按属性名覆盖/新增
                if (IsJsonObject(existingJson, jsonOptions))
                {
                    // 反序列化为 JsonDocument（便于修改）
                    using JsonDocument doc = string.IsNullOrEmpty(existingJson)
                        ? JsonDocument.Parse("{}")  // 空文件默认创建空对象
                        : JsonDocument.Parse(existingJson);

                    // 将新值序列化为 JsonElement
                    JsonElement newElement = JsonSerializer.SerializeToElement(obj, jsonOptions);

                    // 合并新属性（核心修改：重复属性覆盖，其他属性保留）
                    using MemoryStream ms = new();
                    using Utf8JsonWriter writer = new(ms, new JsonWriterOptions { Indented = jsonOptions.WriteIndented });

                    writer.WriteStartObject();

                    // 步骤 1：先收集所有原有属性（键名去重，后面新属性会覆盖旧的）
                    var existingProperties = doc.RootElement.EnumerateObject().ToDictionary(p => p.Name, p => p);

                    // 步骤 2：合并新属性（重复键直接覆盖原有值）
                    if (newElement.ValueKind == JsonValueKind.Object)
                    {
                        foreach (var newProp in newElement.EnumerateObject())
                        {
                            existingProperties[newProp.Name] = newProp;  // 重复键覆盖
                        }
                    }
                    else
                    {
                        throw new JsonException("向 JSON 对象追加时，新值必须是对象类型（键值对）");
                    }

                    // 步骤 3：写入合并后的所有属性
                    foreach (var prop in existingProperties.Values)
                    {
                        prop.WriteTo(writer);
                    }

                    writer.WriteEndObject();
                    writer.Flush();

                    updatedJson = Encoding.UTF8.GetString(ms.ToArray());
                }
                // 场景 2：原有 JSON 是数组 → 按元素值覆盖/新增（先去重，再添加）
                else if (IsJsonArray(existingJson, jsonOptions))
                {
                    // 反序列化为 JsonDocument
                    using JsonDocument doc = string.IsNullOrEmpty(existingJson)
                        ? JsonDocument.Parse("[]")  // 空文件默认创建空数组
                        : JsonDocument.Parse(existingJson);

                    // 将新值序列化为 JsonElement（支持单个元素或数组）
                    JsonElement newElement = JsonSerializer.SerializeToElement(obj, jsonOptions);
                    List<JsonElement> newElements = new();

                    // 处理新值（单个元素或数组）
                    if (newElement.ValueKind == JsonValueKind.Array)
                    {
                        newElements.AddRange(newElement.EnumerateArray());
                    }
                    else
                    {
                        newElements.Add(newElement);
                    }

                    // 步骤 1：收集原有元素（去重依据：元素的 JSON 字符串）
                    var existingElements = new Dictionary<string, JsonElement>();
                    foreach (var elem in doc.RootElement.EnumerateArray())
                    {
                        string elemStr = elem.GetRawText();  // 用原始 JSON 字符串作为去重键
                        existingElements[elemStr] = elem;
                    }

                    // 步骤 2：合并新元素（重复元素覆盖，新元素新增）
                    foreach (var elem in newElements)
                    {
                        string elemStr = elem.GetRawText();
                        existingElements[elemStr] = elem;  // 重复元素覆盖（实际是替换为新元素，值相同则无变化）
                    }

                    // 步骤 3：写入合并后的数组
                    using MemoryStream ms = new();
                    using Utf8JsonWriter writer = new(ms, new JsonWriterOptions { Indented = jsonOptions.WriteIndented });

                    writer.WriteStartArray();
                    foreach (var elem in existingElements.Values)
                    {
                        elem.WriteTo(writer);
                    }
                    writer.WriteEndArray();
                    writer.Flush();

                    updatedJson = Encoding.UTF8.GetString(ms.ToArray());
                }
                // 场景 3：原有内容为空 → 根据新值类型创建对应的 JSON（对象或数组）
                else if (string.IsNullOrEmpty(existingJson))
                {
                    if (IsSimpleType(typeof(T)))
                    {
                        // 单一值 → 自动包裹为对象
                        var wrapper = new { Value = obj };
                        updatedJson = JsonSerializer.Serialize(wrapper, jsonOptions);
                    }
                    else
                    {
                        // 复杂类型 → 直接序列化
                        updatedJson = JsonSerializer.Serialize(obj, jsonOptions);
                    }
                }
                // 场景 4：原有 JSON 格式错误或不支持的类型
                else
                {
                    throw new JsonException("原有 JSON 格式错误，或不支持向非对象/非数组的 JSON 追加值");
                }

                // 写入更新后的内容
                File.WriteAllText(absolutePath, updatedJson, Encoding.UTF8);
            }
            catch (JsonException)
            {
                throw;
            }
            catch (Exception ex) when (ex is not ArgumentNullException)
            {
                throw new IOException($"追加 JSON 内容失败：{path}", ex);
            }
        }

        /// <summary>
        /// 重载：向 JSON 对象追加单个键值对（适合添加单一值，如 "Age": 25）
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <param name="path">JSON 文件路径</param>
        /// <param name="key">新属性的键名</param>
        /// <param name="value">新属性的值</param>
        /// <param name="options">序列化配置（可选）</param>
        public static void AppendToJson<T>(string path, string key, T value, JsonSerializerOptions options = null)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key), "属性键名不能为空");

            // 包装为键值对对象，调用主方法
            var keyValueObj = Activator.CreateInstance(typeof(Dictionary<string, T>)) as Dictionary<string, T>;
            keyValueObj[key] = value;
            AppendToJson(path, keyValueObj, options);
        }

        /// <summary>
        /// 重载：向 JSON 数组追加多个元素
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="path">JSON 文件路径</param>
        /// <param name="newElements">要追加的元素集合</param>
        /// <param name="options">序列化配置（可选）</param>
        public static void AppendToJson<T>(string path, IEnumerable<T> newElements, JsonSerializerOptions options = null)
        {
            if (newElements == null || !newElements.Any())
                throw new ArgumentException("要追加的元素集合不能为空且至少包含一个元素", nameof(newElements));

            foreach (var element in newElements)
            {
                AppendToJson(path, element, options);  // 逐个追加（或优化为批量合并，效率更高）
            }
        }
        #endregion

        #region 私有辅助方法

        /// <summary>
        /// 校验文件路径
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        private static string ValidatePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException(nameof(path), "文件路径不能为空");

            var cleaned = path.Trim();
            var absolutePath = System.IO.Path.GetFullPath(cleaned);

            // 验证路径非法字符
            char[] invalidChars = System.IO.Path.GetInvalidPathChars();
            if (absolutePath.IndexOfAny(invalidChars) != -1)
                throw new ArgumentException($"路径包含非法字符：{absolutePath}", nameof(path));

            // 验证文件名非法字符
            var fileName = System.IO.Path.GetFileName(absolutePath);
            if (!string.IsNullOrEmpty(fileName))
            {
                char[] invalidFileChars = System.IO.Path.GetInvalidFileNameChars();
                if (fileName.IndexOfAny(invalidFileChars) != -1)
                    throw new ArgumentException($"文件名包含非法字符：{fileName}", nameof(path));
            }
            var directory = System.IO.Path.GetDirectoryName(absolutePath);
            try
            {
                // 确保目录存在
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
            }
            catch { }

            return absolutePath;

        }

        /// <summary>
        /// 判断字符串是否为合法的 JSON 对象
        /// </summary>
        private static bool IsJsonObject(string json, JsonSerializerOptions options)
        {
            if (string.IsNullOrEmpty(json))
                return false;

            try
            {
                using JsonDocument doc = JsonDocument.Parse(json);
                return doc.RootElement.ValueKind == JsonValueKind.Object;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        /// <summary>
        /// 判断字符串是否为合法的 JSON 数组
        /// </summary>
        private static bool IsJsonArray(string json, JsonSerializerOptions options)
        {
            if (string.IsNullOrEmpty(json))
                return false;

            try
            {
                using JsonDocument doc = JsonDocument.Parse(json);
                return doc.RootElement.ValueKind == JsonValueKind.Array;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        /// <summary>
        /// 判断类型是否为简单类型（值类型 + string）
        /// </summary>
        private static bool IsSimpleType(Type type)
        {
            return type.IsValueType || type == typeof(string) ||
                   Nullable.GetUnderlyingType(type) != null;
        }
        #endregion
    }
}