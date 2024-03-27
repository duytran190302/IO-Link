using MQTTnet.Client;
using MQTTnet;
using Timer = System.Timers.Timer;
using System.Timers;
using Microsoft.Extensions.Options;

namespace IO_Link
{
    public class ManagedMqtt2
    {
		public IP_Model Options { get; set; }
		public bool IsConnected => _mqttClient2 is not null && _mqttClient2.IsConnected;

       // public event Func<MqttApplicationMessageReceivedEventArgs, Task>? ApplicationMessageReceived;
        public event Func<MqttClientDisconnectedEventArgs, Task>? Disconnected;
        private readonly Timer _reconnectTimer;

        private IMqttClient? _mqttClient2;
        public ManagedMqtt2(IOptions<IP_Model> options)
        {
			Options = options.Value;
			_reconnectTimer = new Timer(5000);
            _reconnectTimer.Elapsed += OnReconnectTimerElapsed;
        }


        private async void OnReconnectTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            await ConnectAsync(Options.IP2); 
		}

        public async Task ConnectAsync(string IP)
        {
            _reconnectTimer.Enabled = false;
            if (_mqttClient2 is not null)
            {
                await _mqttClient2.DisconnectAsync();
                _mqttClient2.Dispose();
            }

            _mqttClient2 = new MqttFactory().CreateMqttClient();
            var mqttClientOptions = new MqttClientOptionsBuilder()
                .WithTcpServer(IP, 1883)
                .WithTimeout(TimeSpan.FromSeconds(5))
                .WithClientId(Guid.NewGuid().ToString())
                .WithKeepAlivePeriod(TimeSpan.FromSeconds(1));

            //_mqttClient2.ApplicationMessageReceivedAsync += ApplicationMessageReceived;
            _mqttClient2.DisconnectedAsync += OnDisconnected;



            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var result = await _mqttClient2.ConnectAsync(mqttClientOptions.Build(), timeout.Token);

            if (result.ResultCode != MqttClientConnectResultCode.Success)
            {
                _reconnectTimer.Enabled = true;
            }
            else
            {
                await _mqttClient2.SubscribeAsync("VTSauto/AR_project/IOT_pub/IO");



            }
        }
        private async Task OnDisconnected(MqttClientDisconnectedEventArgs eventArgs)
        {
            await ConnectAsync(Options.IP2);
        }

        public async Task DisconnectedAsync()
        {
            await _mqttClient2.DisconnectAsync();
        }   

        public async Task Subscribe(string topic)
        {
            if (_mqttClient2 is null)
            {
                throw new InvalidOperationException("MQTT Client is not connected.");
            }

            var topicFilter = new MqttTopicFilterBuilder()
                .WithTopic(topic)
                .Build();

            var subscribeOptions = new MqttClientSubscribeOptionsBuilder()
                .WithTopicFilter(topicFilter)
                .Build();

            var result = await _mqttClient2.SubscribeAsync(subscribeOptions);

            foreach (var subscription in result.Items)
            {
                if (subscription.ResultCode != MqttClientSubscribeResultCode.GrantedQoS0 &&
                    subscription.ResultCode != MqttClientSubscribeResultCode.GrantedQoS1 &&
                    subscription.ResultCode != MqttClientSubscribeResultCode.GrantedQoS2)
                {
                    Console.WriteLine($"MQTT Client 2 Subscription {subscription.TopicFilter.Topic} Failed: {subscription.ResultCode}");
                }
            }
        }

        public async Task Publish(string topic, string payload, bool retainFlag)
        {
            if (_mqttClient2 is null)
            {
                throw new InvalidOperationException("MQTT Client 2 is not connected.");
            }

            var applicationMessageBuilder = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithRetainFlag(retainFlag)
                .WithPayload(payload);

            var applicationMessage = applicationMessageBuilder.Build();

            var result = await _mqttClient2.PublishAsync(applicationMessage);

            if (result.ReasonCode != MqttClientPublishReasonCode.Success)
            {
                Console.WriteLine($"MQTT Client 2 Publish {applicationMessage.Topic} Failed: {result.ReasonCode}");
            }
        }
    }
}
