using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Xml;

namespace FormziApi.Services
{
    //Added by jay mistry 14-7-2016
    public static class ReverseGeoLoc
    {
        #region Methods

        //Refer below link
        //http://stackoverflow.com/questions/3151450/google-geolocation-api-use-longitude-and-latitude-to-get-address

        // http://code.google.com/apis/maps/documentation/geocoding/#ReverseGeocoding
        public static string GetGeoLoction(string latitude, string longitude,
            out string route,
            out string shortName,
            out string country,
            out string administrative_area_level_1,
            out string administrative_area_level_2,
            out string administrative_area_level_3,
            out string colloquial_area,
            out string locality,
            out string sublocality,
            out string neighborhood,
            out string postal_code)
        {

            route = "";
            shortName = "";
            country = "";
            administrative_area_level_1 = "";
            administrative_area_level_2 = "";
            administrative_area_level_3 = "";
            colloquial_area = "";
            locality = "";
            sublocality = "";
            neighborhood = "";
            postal_code = "";

            XmlDocument doc = new XmlDocument();

            try
            {
                doc.Load("https://maps.googleapis.com/maps/api/geocode/xml?latlng=" + latitude + "," + longitude + "&key=" + Helper.Constants.GOOGLE_MAP_REVERSE_GEO_CODE_KEY);
                XmlNode element = doc.SelectSingleNode("//GeocodeResponse/status");
                if (element.InnerText == "ZERO_RESULTS")
                {
                    return ("No data available for the specified location");
                }
                else if (element.InnerText == "REQUEST_DENIED")
                {
                    return ("There is some error");
                }
                else if (element.InnerText == "OVER_QUERY_LIMIT")
                {
                    return ("Google map geocode limit over");
                }
                else
                {

                    element = doc.SelectSingleNode("//GeocodeResponse/result/formatted_address");

                    string longname = "";
                    string shortname = "";
                    string typename = "";
                    bool fHit = false;


                    XmlNodeList xnList = doc.SelectNodes("//GeocodeResponse/result/address_component");
                    foreach (XmlNode xn in xnList)
                    {
                        try
                        {
                            longname = xn["long_name"].InnerText;
                            shortname = xn["short_name"].InnerText;
                            typename = xn["type"].InnerText;


                            fHit = true;
                            switch (typename)
                            {
                                //Add whatever you are looking for below
                                case "country":
                                    {
                                        country = longname;
                                        shortName = shortname;
                                        break;
                                    }

                                case "locality":
                                    {
                                        locality = longname;
                                        //Address_locality = shortname; //Om Longname visar sig innehålla konstigheter kan man använda shortname istället
                                        break;
                                    }

                                case "sublocality":
                                    {
                                        sublocality = longname;
                                        break;
                                    }

                                case "neighborhood":
                                    {
                                        neighborhood = longname;
                                        break;
                                    }

                                case "colloquial_area":
                                    {
                                        colloquial_area = longname;
                                        break;
                                    }

                                case "administrative_area_level_1":
                                    {
                                        administrative_area_level_1 = longname;
                                        break;
                                    }

                                case "administrative_area_level_2":
                                    {
                                        administrative_area_level_2 = longname;
                                        break;
                                    }

                                case "administrative_area_level_3":
                                    {
                                        administrative_area_level_3 = longname;
                                        break;
                                    }
                                case "postal_code":
                                    {
                                        postal_code = longname;
                                        break;
                                    }
                                case "route":
                                    {
                                        route = longname;
                                        break;
                                    }
                                default:
                                    fHit = false;
                                    break;
                            }
                            if (fHit)
                            {
                                Console.Write(typename);
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.Write("\tL: " + longname + "\tS:" + shortname + "\r\n");
                                Console.ForegroundColor = ConsoleColor.Gray;
                            }
                        }
                        catch (Exception e)
                        {
                            //Node missing either, longname, shortname or typename
                            fHit = false;
                            Console.Write(" Invalid data: ");
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Write("\tX: " + xn.InnerXml + "\r\n");
                            Console.ForegroundColor = ConsoleColor.Gray;
                            return ("(Address lookup failed: ) " + e.Message);
                        }
                    }
                    //Console.ReadKey();
                    return (element.InnerText);
                }
            }
            catch (Exception ex)
            {
                return ("(Address lookup failed: ) " + ex.Message);
            }
        } 

        #endregion
    }
}