using System.Collections.Generic;
using Unity.Mathematics;

namespace GeoVision.Data
{
    /// <summary>
    /// Extracts lon/lat rings from a GeoJSON Polygon or MultiPolygon without a full JSON library.
    /// Unity JsonUtility cannot parse nested coordinate arrays.
    /// </summary>
    public static class GeoJsonRingParser
    {
        public static List<List<double2>> ExtractRings(string json)
        {
            List<List<double2>> rings = new List<List<double2>>();
            if (string.IsNullOrEmpty(json))
            {
                return rings;
            }

            int start = json.IndexOf("\"coordinates\"");
            if (start < 0)
            {
                return rings;
            }

            int i = json.IndexOf('[', start);
            if (i < 0)
            {
                return rings;
            }

            ParseArray(json, ref i, 0, rings);
            return rings;
        }

        private static void ParseArray(string json, ref int i, int depth, List<List<double2>> rings)
        {
            i++;
            List<double2> currentRing = null;
            SkipSpace(json, ref i);

            while (i < json.Length)
            {
                SkipSpace(json, ref i);
                char c = json[i];
                if (c == ']')
                {
                    i++;
                    if (currentRing != null && currentRing.Count >= 2)
                    {
                        rings.Add(currentRing);
                    }

                    return;
                }

                if (c == ',')
                {
                    i++;
                    continue;
                }

                if (c == '[')
                {
                    int look = i + 1;
                    SkipSpace(json, ref look);
                    if (look < json.Length && (json[look] == '-' || char.IsDigit(json[look])))
                    {
                        double lon = ReadNumber(json, ref look);
                        SkipSpace(json, ref look);
                        if (look < json.Length && json[look] == ',')
                        {
                            look++;
                            double lat = ReadNumber(json, ref look);
                            SkipSpace(json, ref look);
                            while (look < json.Length && json[look] != ']')
                            {
                                look++;
                            }

                            if (look < json.Length && json[look] == ']')
                            {
                                look++;
                            }

                            if (currentRing == null)
                            {
                                currentRing = new List<double2>();
                            }

                            currentRing.Add(new double2(lon, lat));
                            i = look;
                            continue;
                        }
                    }

                    ParseArray(json, ref i, depth + 1, rings);
                    continue;
                }

                i++;
            }
        }

        private static double ReadNumber(string json, ref int i)
        {
            SkipSpace(json, ref i);
            int start = i;
            if (i < json.Length && (json[i] == '-' || json[i] == '+'))
            {
                i++;
            }

            while (i < json.Length && (char.IsDigit(json[i]) || json[i] == '.' || json[i] == 'e' || json[i] == 'E' || json[i] == '+' || json[i] == '-'))
            {
                if ((json[i] == '+' || json[i] == '-') && i > start && json[i - 1] != 'e' && json[i - 1] != 'E')
                {
                    break;
                }

                i++;
            }

            double value;
            double.TryParse(
                json.Substring(start, i - start),
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture,
                out value);
            return value;
        }

        private static void SkipSpace(string json, ref int i)
        {
            while (i < json.Length && char.IsWhiteSpace(json[i]))
            {
                i++;
            }
        }
    }
}
