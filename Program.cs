using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using System;
using System.Text;
using System.Threading;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace influxapp
{
    class Program
    {
        const string token = "PKvopDZJuDTIHESA6dMUxJwZ6xksrgZSP_3nD_LFzmIQvGSp9gck1IEMai8mQEIOaMJckloK7x6iz0R0XJVi9w==";
        const string bucket = "Kadin_'s Bucket";
        const string org = "Kadin_@live.com";
        private static bool sendMessage = false;
        private static float measurement;

        InfluxDBClient client;

        static void Main(string[] args)
        {
            // You can generate an API token from the "API Tokens Tab" in the UI


            using
            var client = InfluxDBClientFactory.Create("https://us-east-1-1.aws.cloud2.influxdata.com", token);

            try
            {
                MqttClient mqttClient = new MqttClient("9ad5964d48f54c5d90d18fbec9bb78d8.s2.eu.hivemq.cloud",
                                MqttSettings.MQTT_BROKER_DEFAULT_SSL_PORT, true, MqttSslProtocols.TLSv1_2, null, null);
                //.ProtocolVersion = MqttProtocolVersion.Version_3_1;
                mqttClient.MqttMsgPublishReceived += MqttClient_MqttMsgPublishReceived;
                mqttClient.Subscribe(new string[] { "devices/sensors/temperature" }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE });
                mqttClient.Connect(Guid.NewGuid().ToString(), "MyDevice", "MyDevice123");

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }


            /*
            const string data = "mem,host=host1 used_percent=23.43234543";
            using (var writeApi = client.GetWriteApi())
            {
                writeApi.WriteRecord(bucket, org, WritePrecision.Ns, data);
            }

            */
            while (true)
            {
                if (sendMessage)
                {
                    sendMessage = false;
                    var point = PointData
                      .Measurement("mem")
                      .Tag("host", "host1")
                      .Field("used_percent", measurement)
                      .Timestamp(DateTime.UtcNow, WritePrecision.Ns);

                    using (var writeApi = client.GetWriteApi())
                    {
                        writeApi.WritePoint(bucket, org, point);
                    }
                }

            }

        }

        private static void MqttClient_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            string message = Encoding.UTF8.GetString(e.Message).Replace(".", ",");
            Console.WriteLine("Mensagem recebida: " + message);
            measurement = float.Parse(message);
            sendMessage = true;


        }
    }
}