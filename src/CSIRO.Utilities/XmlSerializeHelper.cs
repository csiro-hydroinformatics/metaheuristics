using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace CSIRO.Utilities
{
    /// <summary>
    /// XML serialization/deserialization helper - NOTE: a partial copy of TIME\TIME.Tools\Persistence\InputOutputHelper.cs to limit TIME dependencies
    /// </summary>
    public static class XmlSerializeHelper
    {
        /// <summary>
        /// Serialize an object or object hierarchy to a file.
        /// </summary>
        /// <typeparam name="T">The type of the object to serialize</typeparam>
        /// <param name="obj">Object instance to serialize</param>
        /// <param name="extraTypes">All potential concrete class types that may be encountered in the object hierarchy. Required  to specify concrete class that are referenced as parent classes by the serialised object</param>
        /// <param name="fullPath">Full file name</param>
        public static void SerializeAsXML<T>(T obj, Type[] extraTypes, string fullPath)
        {
            var ser = createSerializer<T>(extraTypes);
            using (StreamWriter sw = new StreamWriter(fullPath))
            {
                ser.Serialize(sw, obj);
            }
        }

        /// <summary>
        /// Serialize an object or object hierarchy to a newly created memory stream. Do dispose of the stream properly (e.g. using 'using')
        /// </summary>
        /// <typeparam name="T">The type of the object to serialize</typeparam>
        /// <param name="obj">Object instance to serialize</param>
        /// <param name="extraTypes">All potential concrete class types that may be encountered in the object hierarchy. Required  to specify concrete class that are referenced as parent classes by the serialised object</param>
        public static MemoryStream SerializeAsXML<T>(T obj, Type[] extraTypes)
        {
            MemoryStream stream = new MemoryStream();
            var ser = createSerializer<T>(extraTypes);
            using (StreamWriter sw = new StreamWriter(stream))
            {
                ser.Serialize(sw, obj);
            }
            return stream;
        }


        /// <summary>
        /// Serialize an object or object hierarchy to an XML file.
        /// </summary>
        /// <typeparam name="T">The type of the object to serialize</typeparam>
        /// <param name="obj">Object instance to serialize</param>
        /// <param name="fullPath">Full file name</param>
        public static void SerializeAsXML<T>(T obj, string fullPath)
        {
            SerializeAsXML(obj, null, fullPath);
        }

        /// <summary>
        /// Deserialize an object or object hierarchy from an XML file.
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize</typeparam>
        /// <param name="extraTypes">All potential concrete class types that may be encountered in the object hierarchy. Required  to specify concrete class that are referenced as parent classes by the serialised object</param>
        /// <param name="fullPath">Full file name</param>
        /// <returns>The deserialized object(s)</returns>
        public static T DeserializeFromXML<T>(string fullPath, Type[] extraTypes)
        {
            if (!File.Exists(fullPath))
                throw new ArgumentException("File does not exist: " + fullPath);
            T result = default(T);
            using (StreamReader sr = new StreamReader(fullPath))
            {
                result = DeserializeFromXML<T>(sr, extraTypes);
            }
            return result;
        }

        /// <summary>
        /// Deserialize an object or object hierarchy from a stream.
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize</typeparam>
        /// <param name="extraTypes">All potential concrete class types that may be encountered in the object hierarchy. Required  to specify concrete class that are referenced as parent classes by the serialised object</param>
        /// <returns>The deserialized object(s)</returns>
        /// <param name="sr">The stream containing the information to deserialize</param>
        /// <returns>The deserialized object(s)</returns>
        public static T DeserializeFromXML<T>(StreamReader sr, Type[] extraTypes)
        {
            T result = default(T);
            var ser = createSerializer<T>(extraTypes);
            using (StreamReader sr2 = new StreamReader(sr.BaseStream))
            {
                result = (T) ser.Deserialize(sr2);
            }
            return result;
        }

        /// <summary>
        /// Deserialize an object or object hierarchy from an XML file.
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize</typeparam>
        /// <param name="fullPath">Full file name</param>
        /// <returns>The deserialized object(s)</returns>
        public static T DeserializeFromXML<T>(string fullPath)
        {
            return DeserializeFromXML<T>(fullPath, null);
        }

        /// <summary>
        /// Deserialize an object or object hierarchy from a stream.
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize</typeparam>
        /// <param name="stream">The stream containing the information to deserialize</param>
        /// <param name="extraTypes">All potential concrete class types that may be encountered in the object hierarchy. Required  to specify concrete class that are referenced as parent classes by the serialised object</param>
        /// <returns>The deserialized object(s)</returns>
        public static T DeserializeFromXML<T>(Stream stream, Type[] extraTypes)
        {
            T result = default(T);
            using (StreamReader sr = new StreamReader(stream))
            {
                result = DeserializeFromXML<T>(sr, extraTypes);
            }
            return result;
        }

        /// <summary>
        /// Test support - Do a round trip, serializing to then deserializing from a transient memory stream
        /// </summary>
        public static T SerializeDeserialize<T>(T obj, Type[] extraTypes)
        {
            T result = default(T);
            var ser = createSerializer<T>(extraTypes);
            using (MemoryStream ms = new MemoryStream())
            {
                using (StreamWriter sw = new StreamWriter(ms))
                {
                    ser.Serialize(sw, obj);
                    sw.Flush();
                    ms.Seek(0, SeekOrigin.Begin);
                    using (StreamReader sr = new StreamReader(ms))
                    {
                        result = (T) ser.Deserialize(sr);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Deserialize an object or object hierarchy from its string XML representation.
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize</typeparam>
        /// <param name="stream">The stream containing the information to deserialize</param>
        /// <param name="extraTypes">Defaults to null (internally, Type.EmptyTypes). Optionally, all potential concrete class types that may be encountered in the object hierarchy. Required  to specify concrete class that are referenced as parent classes by the serialised object</param>
        /// <returns>The deserialized object(s)</returns>
        public static T DeserializeFromXMLContent<T>(string xmlStream, Type[] extraTypes = null)
        {
            T result = default(T);
            if (extraTypes == null) extraTypes = Type.EmptyTypes;
            using (var ms = new MemoryStream())
            {
                using (StreamWriter sw = new StreamWriter(ms))
                {
                    sw.Write(xmlStream);
                    sw.Flush();
                    ms.Seek(0, SeekOrigin.Begin);
                    using (StreamReader sr = new StreamReader(ms))
                    {
                        result = DeserializeFromXML<T>(sr, extraTypes);
                    }
                }
            }
            return result;
        }

        private static XmlSerializer createSerializer<T>(Type[] extraTypes)
        {
            return new XmlSerializer(typeof(T), extraTypes);
        }

    }

}
