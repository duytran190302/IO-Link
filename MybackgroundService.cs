using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using MQTTnet.Client;
using Newtonsoft.Json;
using System.Text;

namespace IO_Link
{
    public class MybackgroundService : BackgroundService
    {
		private readonly IOptions<IP_Model> _mqttOptions;
		private readonly ManagedMqtt _managedMqtt;
        private readonly ManagedMqtt2 _managedMqtt2;
        public MybackgroundService(ManagedMqtt managedMqtt, ManagedMqtt2 managedMqtt2, IOptions<IP_Model> mqttOptions)
        {
            _managedMqtt = managedMqtt;
            _managedMqtt2 = managedMqtt2;
            _mqttOptions = mqttOptions;
            _managedMqtt.ApplicationMessageReceived += OnMqttClientMessageReceivedAsync;

        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            ConnectAsync(_mqttOptions.Value.IP1, "VTSauto/AR_project/IOT_pub/IO").Wait();
            ConnectAsync2(_mqttOptions.Value.IP2, "VTSauto/AR_project/IOT_pub/IO").Wait();
        }
        // thuc hien tai day
        private async Task OnMqttClientMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
        {
            //string topic = e.ApplicationMessage.Topic;
            var jsonString = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
            jsonString = jsonString.Replace("\r", "");
            jsonString = jsonString.Replace("\n", "");
            jsonString = jsonString.Replace(" ", "");
            Message message = JsonConvert.DeserializeObject<Message>(jsonString);
            string? KI6000 = message.Data.Payload["/iolinkmaster/port[3]/iolinkdevice/pdin"].Data;
            string? O5D150 = message.Data.Payload["/iolinkmaster/port[4]/iolinkdevice/pdin"].Data;
            string? UGT524 = message.Data.Payload["/iolinkmaster/port[5]/iolinkdevice/pdin"].Data;
            string? RVP510 = message.Data.Payload["/iolinkmaster/port[8]/iolinkdevice/pdin"].Data;
            //{"name":"tempTW2000","value":31.0,"timestamp":"2023-07-13T18:47:28"}


            int decimalValue1 = int.Parse(KI6000, System.Globalization.NumberStyles.HexNumber);
            if (decimalValue1 % 2 == 0)
            {KI6000= "FALSE"; }
            else
            {KI6000 = "TRUE";}
            
            O5D150 = O5D150.Substring(0, 3);
            int decimalValue2 = int.Parse(O5D150, System.Globalization.NumberStyles.HexNumber);

            UGT524 = UGT524.Substring(0, 4);
			//int decimalValue3 = int.Parse(UGT524, System.Globalization.NumberStyles.HexNumber);
			int decimalValue3 = Convert.ToInt32(UGT524, 16);


			RVP510 = RVP510.Substring(4, 4);
            // Chuyển đổi từ hex sang số nguyên 16-bit
            int intValue = Convert.ToInt32(RVP510, 16);
            // Chuyển số nguyên thành chuỗi nhị phân 16-bit
            string binaryValue = Convert.ToString(intValue, 2).PadLeft(16, '0');
            binaryValue = binaryValue.Substring(0, 14);
            int decimalValue4 = Convert.ToInt32(binaryValue, 2);

            string? payload_KI6000 = $"{{\"name\":\"KI6000\",\"value\":\"{KI6000}\"}}";
            string? payload_O5D150 = $"{{\"name\":\"O5D150\",\"value\":\"{decimalValue2}\"}}";
            string? payload_UGT524 = $"{{\"name\":\"UGT524\",\"value\":\"{decimalValue3}\"}}";
            string? payload_RVP510 = $"{{\"name\":\"RVP510\",\"value\":\"{decimalValue4}\"}}";






            //ConnectAsync2("20.214.136.1", "VTSauto/AR_project/IOT_pub/IO").Wait();
            await _managedMqtt2.Publish("VTSauto/AR_project/IOT_pub/IO", payload_KI6000,true);
            await _managedMqtt2.Publish("VTSauto/AR_project/IOT_pub/IO", payload_O5D150, true);
            await _managedMqtt2.Publish("VTSauto/AR_project/IOT_pub/IO", payload_UGT524, true);
            await _managedMqtt2.Publish("VTSauto/AR_project/IOT_pub/IO", payload_RVP510, true);


            /*
             {
  "code": "event",
  "cid": 1,
  "adr": "/qaz",
  "data": {
    "eventno": "846",
    "srcurl": "00-02-01-71-9A-FC/timer[1]/counter/datachanged",
    "payload": {
      "/timer[1]/counter": {
        "code": 200,
        "data": 463
      },
      "/iolinkmaster/port[1]/iolinkdevice/pdin": {
        "code": 200,
        "data": "0000000000000000"
      },
      "/iolinkmaster/port[3]/iolinkdevice/pdin": {
        "code": 200,
        "data": "02A2"
      },
      "/iolinkmaster/port[4]/iolinkdevice/pdin": {
        "code": 200,
        "data": "00F0"
      },
      "/iolinkmaster/port[5]/iolinkdevice/pdin": {
        "code": 200,
        "data": "7FF8FD00"
      },
      "/iolinkmaster/port[8]/iolinkdevice/pdin": {
        "code": 200,
        "data": "000009A0"
      }
    }
  }
}
 


             */


        }

        private async Task ConnectAsync(string IP1, string TOPIC1)
        {
            
            try
            {
                await _managedMqtt.ConnectAsync(IP1);
                await _managedMqtt.Subscribe(TOPIC1);


            }
            catch (Exception ex)
            {
                Console.WriteLine($"MQTT connection 1 failed: {ex.Message}");

            }
        }
        private async Task ConnectAsync2(string IP2, string TOPIC2)
        {

            try
            {
                await _managedMqtt2.ConnectAsync(IP2);
                await _managedMqtt2.Subscribe(TOPIC2);


            }
            catch (Exception ex)
            {
                Console.WriteLine($"MQTT connection 2 failed: {ex.Message}");

            }
        }

    }
}
