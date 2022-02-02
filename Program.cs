using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using Newtonsoft.Json;
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
        private static Rootobject obj;

        InfluxDBClient client;

        static void Main(string[] args)
        {
            // You can generate an API token from the "API Tokens Tab" in the UI


            using
            var client = InfluxDBClientFactory.Create("https://us-east-1-1.aws.cloud2.influxdata.com", token);

            try
            {
                /*
                MqttClient mqttClient = new MqttClient("9ad5964d48f54c5d90d18fbec9bb78d8.s2.eu.hivemq.cloud",
                                MqttSettings.MQTT_BROKER_DEFAULT_SSL_PORT, true, MqttSslProtocols.TLSv1_2, null, null);
                //.ProtocolVersion = MqttProtocolVersion.Version_3_1;*/
                MqttClient mqttClient = new MqttClient("127.0.0.1");
                Console.WriteLine("Conectando se ao Broker em 127.0.0.1:8086...");
                mqttClient.MqttMsgPublishReceived += MqttClient_MqttMsgPublishReceived;
                mqttClient.Subscribe(new string[] { "devices/sensors/pressure" }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE });
                mqttClient.Connect(Guid.NewGuid().ToString(), "MyDevice", "MyDevice123");
                Console.WriteLine("Conexão ao Broker em 127.0.0.1:8086 bem sucedida! Inscrito no tópico devices/sensors/pressure");


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
                      .Measurement("pressure")
                      .Tag("host", "Instrumento 01")
                      .Field(string.Format("Pressão ({0})", obj.Unit), measurement)
                      .Timestamp(DateTime.UtcNow, WritePrecision.Ns);

                    using (var writeApi = client.GetWriteApi())
                    {
                        writeApi.WritePoint(bucket, org, point);
                        Console.WriteLine(string.Format("Enviando dado ao InfluxDB Cloud. Tag = Instrumento 01, Field Pressão = {0}, Unidade = {1}", obj.Telemetry, obj.Unit));
                    }
                }

            }

        }

        private static void MqttClient_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            string message = Encoding.UTF8.GetString(e.Message);
            obj = JsonConvert.DeserializeObject<Rootobject>(message);
            Console.WriteLine(string.Format("Mensagem recebida do tópico devices/sensors/pressure:\nTelemetria: {0} {1} \n", obj.Telemetry, obj.Unit));
            measurement = obj.Telemetry;
            sendMessage = true;
        }


        public class Rootobject
        {
            public float Telemetry { get; set; }
            public string Unit { get; set; }
            public string QoS { get; set; }
            public DateTime sent { get; set; }
        }

    }
}