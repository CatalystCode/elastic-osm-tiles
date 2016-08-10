
namespace Microsoft.PCT.Serialization
{
    using Newtonsoft.Json;
    using System.IO;
    using System.Text;

    /// <summary>
    /// JSON serialization helpers.
    /// </summary>
    public static class JsonHelpers
    {
        private static readonly JsonSerializer s_serializer = JsonSerializer.Create();

        /// <summary>
        /// Deserialize a value from a JSON string.
        /// </summary>
        /// <typeparam name="T">Expected type to deserialize into.</typeparam>
        /// <param name="value">The JSON string.</param>
        /// <returns>The deserialized value.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "The writer will not dispose the stream.")]
        public static T Deserialize<T>(string value)
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = new StreamWriter(stream, Encoding.UTF8, 1024, true))
                {
                    writer.Write(value);
                }

                stream.Position = 0;
                return Deserialize<T>(stream);
            }
        }

        /// <summary>
        /// Deserialize a value from a JSON stream.
        /// </summary>
        /// <typeparam name="T">Expected type to deserialize into.</typeparam>
        /// <param name="stream">The JSON stream.</param>
        /// <returns>The deserialized value.</returns>
        public static T Deserialize<T>(Stream stream)
        {
            var reader = new StreamReader(stream, Encoding.UTF8, true, 1024, true);

            try
            {
                using (var jsonReader = new JsonTextReader(reader))
                {
                    reader = null;
                    return s_serializer.Deserialize<T>(jsonReader);
                }
            }
            finally
            {
                if (reader != null)
                {
                    reader.Dispose();
                }
            }
        }

        /// <summary>
        /// Serialize a value into a stream as JSON.
        /// </summary>
        /// <typeparam name="T">Type of value to serialize.</typeparam>
        /// <param name="stream">The stream to serialize into.</param>
        /// <param name="value">The value to serialize.</param>
        public static void Serialize<T>(Stream stream, T value)
        {
            var writer = new StreamWriter(stream, Encoding.UTF8, 1024, true);

            try
            {
                using (var jsonWriter = new JsonTextWriter(writer))
                {
                    writer = null;
                    s_serializer.Serialize(jsonWriter, value, typeof(T));
                }
            }
            finally
            {
                if (writer != null)
                {
                    writer.Dispose();
                }
            }
        }

        /// <summary>
        /// Converts an object to a byte array
        /// </summary>
        /// <typeparam name="T">The object type</typeparam>
        /// <param name="obj">The object to convert into a byte array</param>
        /// <returns>byte array</returns>
        public static byte[] SerializeBytes<T>(T obj)
        {
            using (var ms = new MemoryStream())
            {
                JsonHelpers.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Converts a object to a json string
        /// </summary>
        /// <typeparam name="T">The object type</typeparam>
        /// <param name="value">The source object value</param>
        /// <returns>the JSON string</returns>
        public static string SerializeObjectToJson<T>(T value)
        {
            return JsonConvert.SerializeObject(value);
        }
    }
}
