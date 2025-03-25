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

                    string applicationClientId = "";
                    string applicationKey = url;
                    string adxInstanceURL = "";
                    string adxDatabaseName = "";
                    string tenantId = "";

                    // acquire OAuth2 token via AAD REST endpoint
                    webClient.DefaultRequestHeaders.Add("Accept", "application/json");
                    string content = $"grant_type=client_credentials&resource={adxInstanceURL}&client_id={applicationClientId}&client_secret={applicationKey}";
                    HttpResponseMessage responseMessage = webClient.SendAsync(new HttpRequestMessage(HttpMethod.Post, "https://login.microsoftonline.com/" + tenantId + "/oauth2/token")
                    {
                        Content = new StringContent(content, Encoding.UTF8, "application/x-www-form-urlencoded")
                    }).GetAwaiter().GetResult();
                    string restResponse = responseMessage.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                    // call ADX REST endpoint with query
                    string query = "opcua_metadata_lkv"
                       + "| where Name contains 'assembly'"
                        + "| where Name contains 'seattle'"
                        + "| join kind = inner(opcua_telemetry"
                        + "| where Name contains 'Energy'"
                        + "| where Timestamp > now() - 5m and Timestamp < now()"
                        + ") on DataSetWriterID"
                        + "| sort by Timestamp desc"
                        + "| project Timestamp, abs(todouble(Value))";

                    webClient.DefaultRequestHeaders.Remove("Accept");
                    webClient.DefaultRequestHeaders.Add("Authorization", "bearer " + JObject.Parse(restResponse)["access_token"]?.ToString());
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
                            double value = double.Parse(row[1].ToString());
                            if (value < 1000.0f)
                            {
                                continue;
                            }

                            // entry 1 is the value
                            OnTelemetryMessage?.Invoke(new TelemetryMessage
                            {
                                Ambient = 12.0,
                                TurbineID = "T" + i.ToString(),
                                Power = value,
                                Rotor = value * 0.1f,
                                WindSpeed = value * 0.002f
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