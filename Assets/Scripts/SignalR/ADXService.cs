// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ADXService
{
    public event Action<TelemetryMessage> OnTelemetryMessage;

    public class Column
    {
        public string ColumnName { get; set; }

        public string ColumnType { get; set; }
    }

    public class ADXResponse
    {
        public string FrameType { get; set; }

        public bool IsProgressive { get; set; }

        public string Version { get; set; }

        public int? TableId { get; set; }

        public string TableKind { get; set; }

        public string TableName { get; set; }

        public List<Column> Columns { get; set; }

        public List<List<object>> Rows { get; set; }

        public bool? HasErrors { get; set; }

        public bool? Cancelled { get; set; }
    }

    public Task StartAsync(string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            return Task.CompletedTask;
        }

        return Task.Factory.StartNew(() =>
        {
            while (true)
            {
                try
                {
                    HttpClient webClient = new HttpClient();

                    string applicationClientId = "da43d633-f342-4607-afe8-c02ebec17835";
                    string applicationKey = url;
                    string adxInstanceURL = "https://opcmetaverseadx.westeurope.kusto.windows.net";
                    string adxDatabaseName = "opcmetaverse-DB";
                    string tenantId = "6e660ce4-d51a-4585-80c6-58035e212354";

                    // acquire OAuth2 token via AAD REST endpoint
                    webClient.DefaultRequestHeaders.Add("Accept", "application/json");
                    string content = $"grant_type=client_credentials&resource={adxInstanceURL}&client_id={applicationClientId}&client_secret={applicationKey}";
                    HttpResponseMessage responseMessage = webClient.SendAsync(new HttpRequestMessage(HttpMethod.Post, "https://login.microsoftonline.com/" + tenantId + "/oauth2/token")
                    {
                        Content = new StringContent(content, Encoding.UTF8, "application/x-www-form-urlencoded")
                    }).GetAwaiter().GetResult();
                    string restResponse = responseMessage.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                    // call ADX REST endpoint with query
                    string query = "let shiftEndTime = now();"
                    + "let shiftStartTime = datetime_add('hour', -1, shiftEndTime);"
                    + "let dataHistoryTable = AdtPropertyEvents"
                    + "| where TimeStamp between(shiftStartTime..shiftEndTime)"
                    + "| where Id == toscalar(GetDigitalTwinIdForUANode('assembly', 'munich', 'EnergyConsumption'));"
                    + "dataHistoryTable"
                    + "| where isnotnull(SourceTimeStamp)"
                    + "| extend energy = todouble(Value)"
                    + "| take 10";

                    webClient.DefaultRequestHeaders.Remove("Accept");
                    webClient.DefaultRequestHeaders.Add("Authorization", "bearer " + JObject.Parse(restResponse)["access_token"].ToString());
                    responseMessage = webClient.SendAsync(new HttpRequestMessage(HttpMethod.Post, adxInstanceURL + "/v2/rest/query")
                    {
                        Content = new StringContent("{ \"db\":\"" + adxDatabaseName + "\", \"csl\":\"" + query + "\" }", Encoding.UTF8, "application/json")
                    }).GetAwaiter().GetResult();

                    ADXResponse[] response = JsonConvert.DeserializeObject<ADXResponse[]>(responseMessage.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                    // make sure we got at least 3 entries
                    if (response.Length > 2)
                    {
                        // our turbine IDs start at 98
                        int i = 98;

                        // the 3rd entry is the data table
                        foreach (List<object> row in response[2].Rows)
                        {
                            // entry 6 is the value
                            OnTelemetryMessage?.Invoke(new TelemetryMessage
                            {
                                Ambient = 12.0,
                                TurbineID = "T" + i.ToString(),
                                Power = double.Parse(row[6].ToString()) * 1000,
                                Rotor = double.Parse(row[6].ToString()) * 100,
                                WindSpeed = double.Parse(row[6].ToString()) * 20
                            });

                            i++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.Log(ex.Message);
                }
            }
        });
    }
}