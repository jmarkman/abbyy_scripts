using System;
using System.Collections.Generic;
using System.Net;
using System.Xml;
using System.IO;
/*
strings: 
    Address - the address that was read by ABBYY
    requestUri - the Google API url that will utilize Google's geocoding tool
    formattedAddress - after acquiring the address from Google, it will be in XML format and will need to be extracted
    street - the variable that will store the extracted street (number and name)
    city - the variable to hold the extracted city
    state - the variable to hold the extracted state
    zip - the variable to hold the extracted zip code
    geoCheck - the variable to hold the GeoCode Response status; should come back as "Ok" most of the time barring Google being FUBAR
    errorMsg - if something goes wrong on google's end, the user needs to see an error message of what exactly happened
    abbyyZIP - for the 823, the address exists on one line. We need someplace to store the zip code so google can have something accurate to work with

lists:
    longName - the list of all pieces of information contained between the XML brackets <long_name>. The elements are the parts of the address.
    type - the list of all pieces of informaton contained between the XML brackets <type>. The elements are the type of part of the address.
*/
string address, requestUri, geoCheck, bldgNum, street1, street2, city, county, state, zip, abbyyZIP, errorMsg;

List<string> longName = new List<string>();
List<string> type = new List<string>();

if (Context.Field("zip").Text == "")
{
    address = Context.Text; // Get the address from the field
}
else
{
    address = Context.Text + " " + Context.Field("zip").Text; // Results are better with a zip code and the address field is just the street
}
requestUri = "https://maps.googleapis.com/maps/api/geocode/xml?sensor=false&address=" + address; // Build the API url

/*
    If the field is blank, display a warning. Simplistic since so long as there's a best guess and a zip, Google will try to handle it.
*/
if (Context.Text == "" || Context.Text == null)
{
    FCTools.ShowMessage("The address is invalid! It is either blank, incomplete, or was not read properly by ABBYY.");
}
else
{
    // Create a new web request to the Google API
    WebRequest googReq = WebRequest.Create(requestUri);
    
    // Using the response from that request...
    using (WebResponse googResp = googReq.GetResponse())
    {    
        // Get the information from it using a response stream
        using (Stream xmlStream = googResp.GetResponseStream())
        {
            // Instantiate a new XmlDocument and load the data from the data stream using StreamReader
            XmlDocument googXMLDoc = new XmlDocument();
            googXMLDoc.Load(new StreamReader(xmlStream));
            
            // There's no way to select a single XML node based on its tag name, so grab everything that makes up the address
            // The contents of all tag groups that start with <address_component>
            XmlNodeList formattedXML = googXMLDoc.GetElementsByTagName("address_component"); 
            // The status tag under <GeocodeResponse>
            XmlNodeList geoRespXML = googXMLDoc.GetElementsByTagName("GeocodeResponse");
            // Access the singular node <status>
            XmlNode resp = geoRespXML[0];
            geoCheck = resp["status"].InnerXml.ToString();
            
            // Status check for google. "Ok" means everything is fine and everything will be returned, otherwise not.
            if (geoCheck.ToLower() == "ok")
            {
                // For each XML node in the XML Node Array "formattedXML"...
                foreach (XmlNode addressNode in formattedXML)
                {
                    // Add everything associated with the tag "long"
                    longName.Add(addressNode["long_name"].InnerXml.ToString());
                    type.Add(addressNode["type"].InnerXml.ToString());
                }
                // try this pretty big switch statement
                try 
                {
                    int count = type.Count;
                    switch (count)
                    {
                        case 9: // If the type list is 9 items long, this means that the relevant results to us will look like:
                            if (type[8].Contains("postal_code_suffix")) // So far, this case has shown up in two different forms
                            {
                                street2 = "";
                                bldgNum = longName[0];
                                street1 = longName[1];
                                city = longName[3];
                                county = longName[4];
                                state = longName[5];
                                zip = longName[7];

                                // Fill in the data validation text boxes in ABBYY based on their aliases
                                Context.Field("singleBldg").Text = bldgNum;
                                Context.Field("st1").Text = street1;
                                Context.Field("st2").Text = street2;
                                Context.Field("city").Text = city;
                                Context.Field("county").Text = county;
                                Context.Field("state").Text = state;
                                Context.Field("zip").Text = zip;
                            }
                            else
                            {
                                street2 = longName[0];
                                bldgNum = longName[1];
                                street1 = longName[2];
                                city = longName[3];
                                county = longName[5];
                                state = longName[6];
                                zip = longName[8];

                                // Fill in the data validation text boxes in ABBYY based on their aliases
                                Context.Field("singleBldg").Text = bldgNum;
                                Context.Field("st1").Text = street1;
                                Context.Field("st2").Text = street2;
                                Context.Field("city").Text = city;
                                Context.Field("county").Text = county;
                                Context.Field("state").Text = state;
                                Context.Field("zip").Text = zip;
                            }
                            break;
                        case 8: // Similar to the previous case: the type list is 8 items long, so the relevant results are going to look like:
                            if (type[7].Contains("postal_code_suffix")) // So far, this case has shown up in two different forms
                            {
                                street2 = "";
                                bldgNum = longName[0];
                                street1 = longName[1];
                                city = longName[2];
                                county = longName[3];
                                state = longName[4];
                                zip = longName[6];

                                Context.Field("singleBldg").Text = bldgNum;
                                Context.Field("st1").Text = street1;
                                Context.Field("st2").Text = street2;
                                Context.Field("city").Text = city;
                                Context.Field("county").Text = county;
                                Context.Field("state").Text = state;
                                Context.Field("zip").Text = zip;
                            }
                            else
                            {
                                street2 = "";
                                bldgNum = longName[0];
                                street1 = longName[1];
                                city = longName[2];
                                county = longName[4];
                                state = longName[5];
                                zip = longName[7];

                                Context.Field("singleBldg").Text = bldgNum;
                                Context.Field("st1").Text = street1;
                                Context.Field("st2").Text = street2;
                                Context.Field("city").Text = city;
                                Context.Field("county").Text = county;
                                Context.Field("state").Text = state;
                                Context.Field("zip").Text = zip;
                            }
                            break;
                        case 7: // And so on, for the whole way down
                            street2 = "";
                            bldgNum = longName[0];
                            street1 = longName[1];
                            city = longName[2];
                            county = longName[3];
                            state = longName[4];
                            zip = longName[6];

                            Context.Field("singleBldg").Text = bldgNum;
                            Context.Field("st1").Text = street1;
                            Context.Field("st2").Text = street2;
                            Context.Field("city").Text = city;
                            Context.Field("county").Text = county;
                            Context.Field("state").Text = state;
                            Context.Field("zip").Text = zip;
                            break;
                        case 6:
                            street1 = longName[0];
                            city = longName[1];
                            county = longName[2];
                            state = longName[3];
                            zip = longName[5];
                            street2 = "";
                            bldgNum = "";

                            Context.Field("singleBldg").Text = bldgNum;
                            Context.Field("st1").Text = street1;
                            Context.Field("st2").Text = street2;
                            Context.Field("city").Text = city;
                            Context.Field("county").Text = county;
                            Context.Field("state").Text = state;
                            Context.Field("zip").Text = zip;
                            break;
                        case 5:
                            street1 = longName[0];
                            city = longName[1];
                            state = longName[2];
                            zip = longName[4];
                            street2 = "";
                            county = "";
                            bldgNum = "";
                            
                            Context.Field("singleBldg").Text = bldgNum;
                            Context.Field("st1").Text = street1;
                            Context.Field("st2").Text = street2;
                            Context.Field("city").Text = city;
                            Context.Field("county").Text = county;
                            Context.Field("state").Text = state;
                            Context.Field("zip").Text = zip;
                            break;
                        default:
                            street2 = "";
                            bldgNum = longName[0];
                            street1 = longName[1];
                            city = longName[2];
                            county = longName[3];
                            state = longName[4];
                            zip = longName[6];

                            Context.Field("singleBldg").Text = bldgNum;
                            Context.Field("st1").Text = street1;
                            Context.Field("st2").Text = street2;
                            Context.Field("city").Text = city;
                            Context.Field("county").Text = county;
                            Context.Field("state").Text = state;
                            Context.Field("zip").Text = zip;
                            break;
                    } 
                }   
                catch (ArgumentOutOfRangeException)
                {
                    FCTools.ShowMessage("Location not geocoded!");
                }          
            }
            else
            {
                // Error message box that will be shown to the user if something goes wrong with the google geocoding
                // Contains statuses straight from Google's documentation (with some edits for relevance)
                errorMsg = "Error! Google geocode response status: " + geoCheck + 
                "\n\n 'ZERO_RESULTS' indicates that the parse was successful but returned no results.\n" + 
                "This may occur if Google's geocoder was passed a non-existent address.\n\n" +
                "'OVER_QUERY_LIMIT' indicates that the script is over its verification quota.\n\n" +
                "'REQUEST_DENIED' indicates that your address parse request was denied.\n" +
                "Please report this as soon as possible\n\n" +
                "'INVALID_REQUEST' generally indicates that critical components of the address are missing.\n\n" +
                "'UNKNOWN_ERROR' indicates that the request could not be processed due to a server error.\n" +
                "The request may succeed if you try again.";
                errorMsg = errorMsg.Replace("\n", System.Environment.NewLine);
                FCTools.ShowMessage(errorMsg);
            }
        }
    }
}