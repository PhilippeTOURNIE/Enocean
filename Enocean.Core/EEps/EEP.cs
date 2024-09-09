using Enocean.Core.Constants;
using Microsoft.Extensions.Logging;
using System.Collections;
using System.Globalization;
using System.Xml.Serialization;

namespace Enocean.Core.EEps
{
    public partial class EEP
    {
        static Dictionary<RORG, Dictionary<int, Dictionary<int, Profile>>> telegrams = new Dictionary<RORG, Dictionary<int, Dictionary<int, Profile>>>();

        bool init_ok = false;
        ILogger? logger;
        Telegrams? xml_telegrams;

        public static Dictionary<RORG, Dictionary<int, Dictionary<int, Profile>>> Telegrams => telegrams;
        public bool InitOk => init_ok;
        public EEP(ILogger? logger = null)
        {
            this.logger = logger;

            if (telegrams.Any())
            {
                init_ok = true;
                return;
            }

            try
            {
                // Create an instance of the XmlSerializer.
                XmlSerializer serializer = new XmlSerializer(typeof(Telegrams));

                // Declare an object variable of the type to be deserialized.
                using (Stream reader = new FileStream("EEP.xml", FileMode.Open))
                {
                    // Call the Deserialize method to restore the object's state.
                    xml_telegrams = (Telegrams?)serializer.Deserialize(reader);
                }

                // fill
                if (xml_telegrams != null)
                {
                    telegrams = xml_telegrams.Telegram
                     .ToDictionary(
                         telegram => (RORG)Utils.FromHexStringToInt(telegram.Rorg!),
                         telegram => telegram.Profiles
                             .ToDictionary(
                                 function => Utils.FromHexStringToInt(function.Func!),
                                 function => function.Profile
                                     .ToDictionary(
                                         type => Utils.FromHexStringToInt(type.Type!),
                                         type => type
                                     )
                             )
                     );

                    init_ok = true;
                }
            }
            catch (Exception e)
            {
                logger?.LogError($"Parsed Epp.xml Error {e.Message}");

            }
        }

        /// <summary>
        /// Find profile and data description, matching RORG, FUNC and TYPE
        /// </summary>
        public List<Data>? FindProfil(BitArray bitarray, RORG eep_rorg, int rorg_func, int rorg_type, int? direction = null, int? command = null)
        {
            if (!init_ok)
                //self.logger?.warn('EEP.xml not loaded!')
                return null;

            if (!init_ok)
            {
                //this.logger?.Warn("EEP.xml not loaded!");
                return null;
            }

            if (!telegrams.ContainsKey(eep_rorg))
            {
                //this.logger?.Warn($"Cannot find rorg {eepRorg:X} in EEP!");
                return null;
            }

            if (!telegrams[eep_rorg].ContainsKey(rorg_func))
            {
                //this.logger?.Warn($"Cannot find rorg {eep_rorg:X} func {rorg_func:X} in EEP!");
                return null;
            }

            if (!telegrams[eep_rorg][rorg_func].ContainsKey(rorg_type))
            {
                //this.logger?.Warn($"Cannot find rorg {eepRorg:X} func {rorgFunc:X} type {rorgType:X} in EEP!");
                return null;
            }

            var profile = telegrams[eep_rorg][rorg_func][rorg_type];

            //  multiple commands can be defined, with the command id always in same location (per RORG-FUNC-TYPE).
            //  If commands are not set in EEP, or command is None,
            //  get the first data as a "best guess".
            if (command != null)
            {
                var eepCommand = profile.Command;
                if (profile.Command == null)
                {
                    return profile.Data;
                }

                return profile.Data?.Where(e => e.Command == command.ToString()).ToList();
            }

            // extract data description
            // the direction tag is optional
            if (direction == null)
            {
                return profile.Data;
            }

            return profile.Data.Where(e => e.Direction == direction.ToString()).ToList();
        }

        public Dictionary<string, StateItem> GetValues(List<Data> profile, BitArray bitarray, BitArray status)
        {
            // Get keys and values from bitarray
            if (!init_ok || profile == null)
            {
                return new();
            }

            var output = new Dictionary<string, StateItem>();
            foreach (var source in profile)
            {
                foreach (var value in source.Value)
                {
                    foreach (var item in GetValue(value, bitarray))
                    {
                        output[item.Key] = item.Value;
                    }
                }

                foreach (var en in source.Enum)
                {
                    foreach (var item in GetEnum(en, bitarray))
                    {
                        output[item.Key] = item.Value;
                    }
                }

                foreach (var st in source.Status)
                {
                    foreach (var item in GetBoolean(st, status))
                    {
                        output[item.Key] = item.Value;
                    }
                }
            }

            return output;
        }

        /// <summary>
        /// Get boolean value, based on the data in XML
        /// </summary>
        /// <param name="status"></param>
        /// <param name="bitarray"></param>
        /// <returns></returns>
        public Dictionary<string, StateItem> GetBoolean(Status status, BitArray bitarray)
        {
            int rawValue = GetRaw(status, bitarray);

            var dico = new Dictionary<string, StateItem>();
            dico.Add(
                status.Shortcut!,
                new StateItem()
                {
                    description = status.Description,
                    unit = "",
                    value = rawValue.ToString(),
                    raw_value = rawValue.ToString()
                }
            );

            return dico;
        }


        public Dictionary<string, StateItem> GetEnum(Enum en, BitArray bitarray)
        {
            int rawValue = GetRaw(en, bitarray);

            string? description;
            var item = en.Item.FirstOrDefault(i => i.Value == rawValue);
            if (item == null)
            {
                description = GetRangeItem(en, rawValue)?.Description;
            }
            else
            {
                description = item.Description;
            }

            var dico = new Dictionary<string, StateItem>();
            dico.Add(
                en.Shortcut!,
                new StateItem()
                {
                    description = en.Description,
                    unit = en.Unit ?? "",
                    value = string.Format((description ?? "").Replace("{value}", "{0}"), rawValue),
                    raw_value = rawValue.ToString()
                }
            );

            return dico;
        }

        /// <summary>
        /// Get value, based on the data in XML
        /// </summary>
        /// <param name="value"></param>
        /// <param name="bitarray"></param>
        /// <returns></returns>
        public Dictionary<string, StateItem> GetValue(Value value, BitArray bitarray)
        {
            int rawValue = GetRaw(value, bitarray);

            var rng = value.Range!;
            var rng_min = float.Parse(rng.Min!);
            var rng_max = float.Parse(rng.Max!);

            var scl = value.Scale!;
            var scl_min = float.Parse(scl.Min!, CultureInfo.InvariantCulture);
            var scl_max = float.Parse(scl.Max!, CultureInfo.InvariantCulture);

            var dico = new Dictionary<string, StateItem>();
            dico.Add(
                value.Shortcut!,
                new StateItem()
                {
                    description = value?.Description,
                    unit = value?.Unit ?? "",
                    value = ((scl_max - scl_min) / (rng_max - rng_min) * (rawValue - rng_min) + scl_min).ToString(),
                    raw_value = rawValue.ToString()
                }
            );

            return dico;
        }

        public static Rangeitem? GetRangeItem(Enum en, int rawValue)
        {
            foreach (var rangeitem in en.Rangeitem)
            {
                int start = int.Parse(rangeitem.Start!);
                int end = int.Parse(rangeitem.End!);
                if (rawValue >= start && rawValue <= end)
                {
                    return rangeitem;
                }
            }

            return null;
        }

        /// <summary>
        /// Get raw data as integer, based on offset and size
        /// </summary>
        /// <param name="raw"></param>
        /// <param name="bitArray"></param>
        private int GetRaw(IRaw raw, BitArray bitArray)
        {
            string binaryString = "";
            var b = bitArray.Skip(raw.Offset).Take(raw.Size);
            for (int i = 0; i < b.Length; i++)
            {
                binaryString += b[i] ? '1' : '0';
            }

            return Convert.ToInt32(binaryString, 2);
        }


        /// <summary>
        /// put value into bit array
        /// </summary>
        private BitArray SetRaw(IRaw raw, int rawValue, BitArray bitArray)
        {
            for (int digit = 0; digit < raw.Size; digit++)
            {
                bitArray[raw.Offset + digit] = (rawValue >> raw.Size - digit - 1 & 0x01) != 0;
            }

            return bitArray;
        }


        /// <summary>
        /// Update data based on data contained in properties 
        /// </summary>
        /// <param name="profile"></param>
        /// <param name=""></param>
        /// <param name="status"></param>
        /// <param name=""></param>
        public (BitArray, BitArray) SetValues(List<Data> profile, BitArray data, BitArray status, Dictionary<string, object>? properties)
        {
            status.Reverse();
            // Update data based on data contained in properties
            if (init_ok && profile is not null && properties is not null)
            {
                // Update data based on data contained in properties
                foreach (var item in properties)
                {
                    // find the given property from EEP
                    var value = profile.First().Value.FirstOrDefault(v => ((IRaw)v).Shortcut == item.Key);
                    if (value != null)
                    {
                        data = SetValue(value, Convert.ToSingle(item.Value), data);
                    }
                    var en = profile.First().Enum.FirstOrDefault(e => ((IRaw)e).Shortcut == item.Key);
                    if (en != null)
                    {
                        data = SetEnum(en, item.Value, data);
                    }
                    var st = profile.First().Status.FirstOrDefault(e => ((IRaw)e).Shortcut == item.Key);
                    if (st != null)
                    {
                        status = SetBoolean(st, item.Value, status);
                    }
                }

                return (data, status);
            }
            else
            {
                return (data, status);
            }
        }

        /// <summary>
        /// set given numeric value to target field in bitarray 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="value"></param>
        /// <param name="bitArray"></param>
        private BitArray SetValue(Value target, float value, BitArray bitarray)
        {
            var rng = target.Range!;
            var rng_min = float.Parse(rng.Min!);
            var rng_max = float.Parse(rng.Max!);

            var scl = target.Scale!;
            var scl_min = float.Parse(scl.Min!, CultureInfo.InvariantCulture);
            var scl_max = float.Parse(scl.Max!, CultureInfo.InvariantCulture);
            int raw_value = Convert.ToInt32((value - scl_min) * (rng_max - rng_min) / (scl_max - scl_min) + rng_min);

            // store value in bitfield
            return SetRaw(target, raw_value, bitarray);
        }

        /// <summary>
        /// set given enum value (by string or integer value) to target field in bitarray
        /// </summary>
        /// <param name="target"></param>
        /// <param name="value"></param>
        /// <param name="bitarray"></param>
        private BitArray SetEnum(Enum target, object value, BitArray bitarray)
        {
            // derive raw value
            int rawValue;
            if (value is int | value is byte)
            {
                // check whether this value exists
                var item = target.Item.FirstOrDefault(e => e.Value == Convert.ToInt32(value));
                if (item is not null)
                {

                    rawValue = Convert.ToInt32(item.Value);
                }
                else
                {
                    var rangeItem = GetRangeItem(target, Convert.ToInt32(value));
                    if (rangeItem != null)
                    {
                        rawValue = Convert.ToInt32(value);
                    }
                    else
                    {
                        logger?.LogError($"Enum value {value} not found in EEP.");
                        throw new Exception($"Enum value {value} not found in EEP.");
                    }
                }
            }
            else
            {
                var item = target.Item.FirstOrDefault(e => e.Description?.ToLower() == value.ToString().ToLower());
                if (item is not null)
                {
                    rawValue = item.Value;
                }
                else
                {
                    logger?.LogError($"Enum description for value '{value}' not found in EEP.");
                    throw new Exception($"Enum description for value '{value}' not found in EEP.");
                }
            }

            return SetRaw(target, rawValue, bitarray);
        }

        /// <summary>
        /// set given value to target bit in bitarray 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="value"></param>
        /// <param name="status"></param>
        private BitArray SetBoolean(Status target, object value, BitArray bitarray)
        {
            bitarray[target.Offset] = (bool)value;
            return bitarray;
        }
    }
}
